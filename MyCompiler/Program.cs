using MyCompiler.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            //set culture for correct double values parsing
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            string sourceCode = 
@"
print 1.5;
print 100;
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
            }
        }
    }
}
