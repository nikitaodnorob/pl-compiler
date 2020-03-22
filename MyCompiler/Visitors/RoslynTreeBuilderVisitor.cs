using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MyCompiler.SyntaxTree;

namespace MyCompiler.Visitors
{
    class RoslynTreeBuilderVisitor : BaseVisitor
    {
        /// <summary>
        /// Stack of expressions for storing expressions' parts
        /// </summary>
        private Stack<ExpressionSyntax> expressions = new Stack<ExpressionSyntax>();

        /// <summary>
        /// Stack of program blocks for remembering what block is current
        /// </summary>
        private Stack<SyntaxNode> blocks = new Stack<SyntaxNode>();

        /// <summary>
        /// Auxiliary function for generating tree node of Using syntax
        /// </summary>
        /// <param name="usingName">Name of using</param>
        /// <returns>Using syntax node</returns>
        private UsingDirectiveSyntax CreateUsingDirective(string usingName)
        {
            NameSyntax qualifiedName = null;

            foreach (var identifier in usingName.Split('.'))
            {
                var name = SyntaxFactory.IdentifierName(identifier);

                if (qualifiedName != null) qualifiedName = SyntaxFactory.QualifiedName(qualifiedName, name);
                else qualifiedName = name;
            }

            return SyntaxFactory.UsingDirective(qualifiedName);
        }

        /// <summary>
        /// Auxiliary function for adding the statement into current block (it is in the top of blocks' stack)
        /// </summary>
        /// <param name="statement">Node of statement</param>
        private void AddStatementToCurrentBlock(StatementSyntax statement)
        {
            //look current block
            SyntaxNode currentBlock = blocks.Peek();

            //if statement isn't in any block, it's need to add the statement to Main method in C#
            if (currentBlock is MethodDeclarationSyntax && (currentBlock as MethodDeclarationSyntax).Identifier.Text == "Main")
                mainMethodNode = mainMethodNode.AddBodyStatements(statement);

            //if we forget some type of block, we will get an exception
            else throw new Exception($"current block was {currentBlock.GetType()}");
        }

        /// <summary>
        /// Node of C# unit which contains all classes
        /// </summary>
        private CompilationUnitSyntax unitNode;

        /// <summary>
        /// Node of C# class which contains Main method
        /// </summary>
        private ClassDeclarationSyntax programClassNode;

        /// <summary>
        /// Node of C# Main method
        /// </summary>
        private MethodDeclarationSyntax mainMethodNode;

        /// <summary>
        /// Ready C# unit node which prepared for compilation
        /// </summary>
        public CompilationUnitSyntax UnitNode
        {
            get
            {
                //add C# Main method into class
                programClassNode = programClassNode.AddMembers(mainMethodNode);

                //declare namespace and add the class into it 
                NamespaceDeclarationSyntax namespaceNode = SyntaxFactory
                    .NamespaceDeclaration(SyntaxFactory.IdentifierName("RoslynApp"))
                    .AddMembers(programClassNode);

                //add namespace into unit 
                unitNode = unitNode.AddMembers(namespaceNode);

                return unitNode;
            }
            private set { unitNode = value; }
        }

        /// <summary>
        /// Initializing of visitor
        /// </summary>
        public RoslynTreeBuilderVisitor()
        {
            //Create unit and add using "System"
            unitNode = SyntaxFactory.CompilationUnit().AddUsings(
                CreateUsingDirective("System")
            );

            //Create public class "Program"
            programClassNode = SyntaxFactory
                    .ClassDeclaration("Program")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            //Create method void Main() 
            mainMethodNode = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "Main"
            );

            //Set Main method as static
            mainMethodNode = mainMethodNode.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            //Add to Main method empty body
            mainMethodNode = mainMethodNode.AddBodyStatements();

            //Set Main method as current block (push to stack)
            blocks.Push(mainMethodNode);
        }

        public override void VisitIntNumNode(IntNumNode node)
        {
            var literal = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(node.Value));
            expressions.Push(literal);
        }

        public override void VisitRealNumNode(RealNumNode node)
        {
            var literal = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(node.Value));
            expressions.Push(literal);
        }

        public override void VisitIDNode(IDNode node)
        {
            var identifer = SyntaxFactory.IdentifierName(node.Text);
            expressions.Push(identifer);
        }

        public override void VisitPrintNode(PrintNode node)
        {
            node.Expression.Visit(this);

            //"print" operation in my language is Console.WriteLine method call
            var printClassName = SyntaxFactory.IdentifierName("Console");
            var printMethodName = SyntaxFactory.IdentifierName("WriteLine");
            var printStatement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, printClassName, printMethodName)
                ).AddArgumentListArguments(SyntaxFactory.Argument(expressions.Pop()))
            );
            AddStatementToCurrentBlock(printStatement);
        }
    }
}
