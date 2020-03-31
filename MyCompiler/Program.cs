using MyCompiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MyCompiler.Visitors;
using System.IO;

namespace MyCompiler
{
    class Program
    {
        /// <summary>
        /// Compile the program which is setted by syntax tree
        /// </summary>
        /// <param name="syntaxTree">Syntax tree</param>
        static void Compile(Node syntaxTree)
        {
            //initialize visitor
            RoslynTreeBuilderVisitor visitor = new RoslynTreeBuilderVisitor();

            //run visitor
            syntaxTree.Visit(visitor);

            //get unit which prepared for compilation
            var programUnit = visitor.UnitNode;

            CSharpCompilation compilation = CSharpCompilation.Create(
                "assemblyName",
                new[] { programUnit.SyntaxTree },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
                new CSharpCompilationOptions(
                    OutputKind.ConsoleApplication, //set application type as console app
                    true, //report about suppressed errors and warnings
                    null, //default module name
                    null, //set that any static Main method is may be entrypoint
                    null,
                    null, //set empty list of usings
                    OptimizationLevel.Release //set optimization level for Release mode
                )
            );

            //generate program to "program.exe" file
            using (var exeStream = new FileStream("../../program.exe", FileMode.Create))
            {
                var emitResult = compilation.Emit(exeStream); //compile

                //if we have warnings, print them
                foreach (var error in emitResult.Diagnostics.Where(diagnostic => diagnostic.WarningLevel > 0))
                    Console.WriteLine(error);

                if (!emitResult.Success)
                {
                    //if we have errors, print them
                    foreach (var error in emitResult.Diagnostics.Where(diagnostic => diagnostic.WarningLevel == 0))
                        Console.WriteLine(error);
                }
            }

            //print C# source code which matches to builded Roslyn's syntax tree
            Console.WriteLine("========== SOURCE CODE ==========");
            Console.WriteLine(programUnit.NormalizeWhitespace().ToFullString());
        }

        static void Main(string[] args)
        {
            //set culture for correct double values parsing
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            string sourceCode = 
@"
print 1.5;
print 100;
print a;
";
            //lexical analysis
            Scanner scanner = new Scanner();
            scanner.SetSource(sourceCode, 0);

            //syntax analysis
            Parser parser = new Parser(scanner);
            bool isCorrect = parser.Parse();

            if (isCorrect) //if program was parsed successfully
            {
                Node syntaxTree = parser.Root; //get program's syntax tree
                Compile(syntaxTree); //compile the program
            }
        }
    }
}
