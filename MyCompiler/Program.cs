using MyCompiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MyCompiler.Visitors;
using System.IO;
using QUT.Gppg;
using Microsoft.CodeAnalysis.Text;
using MyCompiler.Errors;
using System.Reflection;

namespace MyCompiler
{
    using LocationMap = Dictionary<int, Tuple<int,int>>;

    class Program
    {
        static ErrorFormatter errorFormatter = new ErrorFormatter();

        static Tuple<int, int> ParseAnnotationPart(string a) 
        {
            var parts = a.Split(',').Select(s => int.Parse(s)).ToList();
            return Tuple.Create(parts[0], parts[1]);
        }

        /// <summary>
        /// Build location map
        /// </summary>
        /// <param name="root">Root of Roslyn tree</param>
        /// <param name="annotations">List of location annotations</param>
        /// <returns>Matching of Roslyn's positions and locations in source code</returns>
        static LocationMap GetLocationMap(SyntaxNode root, List<SyntaxAnnotation> annotations)
        {
            LocationMap result = new LocationMap();

            foreach (var annotation in annotations)
            {
                var node = root.GetAnnotatedNodes(annotation).FirstOrDefault();
                if (node == null) continue;

                var nodeSpan = node.FullSpan;
                var nodeAnnotationParts = annotation.Data.Split(';');

                if (!result.ContainsKey(nodeSpan.Start))
                    result.Add(nodeSpan.Start, ParseAnnotationPart(nodeAnnotationParts[0]));
                if (!result.ContainsKey(nodeSpan.End))
                    result.Add(nodeSpan.End, ParseAnnotationPart(nodeAnnotationParts[1]));
            }

            return result;
        }

        static void GenerateRuntimeConfig(string configPath)
        {
            string netCoreVersion = "3.1.3";
            using StreamWriter sw = new StreamWriter(configPath);
            sw.WriteLine(
@"{
    ""runtimeOptions"": {
        ""tfm"": ""netcoreapp3.0"",
        ""framework"": {
            ""name"": ""Microsoft.NETCore.App"",
            ""version"": """ + netCoreVersion + @"""
        }
    }
}");
        }

        static Microsoft.CodeAnalysis.SyntaxTree GetLibraryModuleSyntaxTree(string filename)
        {
            if (Path.GetExtension(filename) == ".mylang")
            {
                if (MakeSyntaxAnalysis(filename, out Node syntaxTree))
                {
                    //initialize visitor
                    RoslynTreeBuilderVisitor visitor = new RoslynTreeBuilderVisitor(Path.GetFileNameWithoutExtension(filename));

                    //run visitor
                    syntaxTree.Visit(visitor);

                    return visitor.UnitNode.SyntaxTree;
                }
                else throw new Exception($"Error in standard library: {Path.GetFileNameWithoutExtension(filename)}");
            }
            else if (Path.GetExtension(filename) == ".cs")
            {
                try
                {
                    return CSharpSyntaxTree.ParseText(File.ReadAllText(filename));
                }
                catch
                {
                    throw new Exception($"Error in standard library: {Path.GetFileNameWithoutExtension(filename)}");
                }
            }
            else throw new Exception($"Unknow item in standard library: {Path.GetFileNameWithoutExtension(filename)}");
        }

        /// <summary>
        /// Compile the program which is setted by syntax tree
        /// </summary>
        /// <param name="syntaxTree">Syntax tree</param>
        static void Compile(Node syntaxTree, string outputFileName)
        {
            //initialize visitor
            RoslynTreeBuilderVisitor visitor = new RoslynTreeBuilderVisitor();

            //run visitor
            syntaxTree.Visit(visitor);

            //get unit which prepared for compilation
            var programUnit = visitor.UnitNode;

            //get all location annotations and build location map
            var locationAnnotations = visitor.LocationAnnotations;
            var locationMap = GetLocationMap(programUnit, locationAnnotations);

            //get syntax trees of standard library
            var arraySyntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText("../../../MyCompilerLibrary/Array.cs"));

            var syntaxTrees = new List<Microsoft.CodeAnalysis.SyntaxTree> { programUnit.SyntaxTree };
            foreach (var file in Directory.GetFiles("../../../MyCompilerLibrary/"))
                syntaxTrees.Add(GetLibraryModuleSyntaxTree(file));

            CSharpCompilation compilation = CSharpCompilation.Create(
                "assemblyName",
                syntaxTrees,
                new[] {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Console").Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
                    MetadataReference.CreateFromFile(typeof(System.Diagnostics.Stopwatch).Assembly.Location),
                },
                new CSharpCompilationOptions(
                    outputKind: OutputKind.ConsoleApplication, //set application type as console app
                    optimizationLevel: OptimizationLevel.Release //set optimization level for Release mode
                )
            );

            string configPath = Path.GetDirectoryName(outputFileName) + "/" + Path.GetFileNameWithoutExtension(outputFileName) + ".runtimeconfig.json";
            GenerateRuntimeConfig(configPath.TrimStart('/')); //generate runtime config

            //generate program to file
            using var exeStream = new FileStream(outputFileName, FileMode.Create);
            var emitResult = compilation.Emit(exeStream); //compile

            //if we have warnings, print them
            foreach (var error in emitResult.Diagnostics.Where(diagnostic => diagnostic.WarningLevel > 0))
                Console.WriteLine(errorFormatter.GetErrorString(error, locationMap));

            if (!emitResult.Success)
            {
                //if we have errors, print them
                foreach (var error in emitResult.Diagnostics.Where(diagnostic => diagnostic.WarningLevel == 0))
                    Console.WriteLine(errorFormatter.GetErrorString(error, locationMap));
            }

            //print C# source code which matches to builded Roslyn's syntax tree
            //Console.WriteLine("========== SOURCE CODE ==========");
            //Console.WriteLine(programUnit.NormalizeWhitespace().ToFullString());
        }

        static bool MakeSyntaxAnalysis(string sourceFileName, out Node root)
        {
            root = null;

            string sourceCode = File.ReadAllText(sourceFileName);

            //lexical analysis
            Scanner scanner = new Scanner();
            scanner.SetSource(sourceCode, 0);

            //syntax analysis
            Parser parser = new Parser(scanner);
            bool isCorrect = parser.Parse();
            if (isCorrect) root = parser.Root;
            return isCorrect;
        }

        static void Main(string[] args)
        {
            //set culture for correct double values parsing
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            //get parameters of command line
            string sourceFileName = args.Length > 0 ? args[0] : "../../../examples/performance.mylang";
            string outputFileName = args.Length > 1 ? args[1] : "../../../out/program.exe";

            if (MakeSyntaxAnalysis(sourceFileName, out Node syntaxTree)) //if program was parsed successfully
            {
                Compile(syntaxTree, outputFileName); //compile the program
            }
        }
    }
}
