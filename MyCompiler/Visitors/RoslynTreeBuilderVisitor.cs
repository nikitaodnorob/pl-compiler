using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MyCompiler.SyntaxTree;
using QUT.Gppg;

namespace MyCompiler.Visitors
{
    using static SyntaxFactory;

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

        public List<SyntaxAnnotation> LocationAnnotations { get; private set; }

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
                var name = IdentifierName(identifier);

                if (qualifiedName != null) qualifiedName = QualifiedName(qualifiedName, name);
                else qualifiedName = name;
            }

            return UsingDirective(qualifiedName);
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
            else if (currentBlock is BlockSyntax)
            {
                blocks.Pop();
                blocks.Push((currentBlock as BlockSyntax).AddStatements(statement));
            }
            else if (currentBlock is MethodDeclarationSyntax)
            {
                blocks.Pop();
                blocks.Push((currentBlock as MethodDeclarationSyntax).AddBodyStatements(statement));
            }

            //if we forget some type of block, we will get an exception
            else throw new Exception($"current block was {currentBlock.GetType()}");
        }

        /// <summary>
        /// Auxiliary function for adding the variables into current block (it is in the top of blocks' stack)
        /// </summary>
        /// <param name="statement">Node of variable declaration</param>
        private void AddVariableToCurrentBlock(VariableDeclarationSyntax statement)
        {
            //look current block
            SyntaxNode currentBlock = blocks.Peek();

            //if statement isn't in any block, it's need to add the statement to Main method in C#
            if (currentBlock is MethodDeclarationSyntax && (currentBlock as MethodDeclarationSyntax).Identifier.Text == "Main")
            {
                var declaration = FieldDeclaration(statement);
                declaration = declaration.AddModifiers(Token(SyntaxKind.StaticKeyword));
                programClassNode = programClassNode.AddMembers(declaration);
            }
            else if (currentBlock is BlockSyntax)
            {
                blocks.Pop();
                var declaration = LocalDeclarationStatement(statement);
                blocks.Push((currentBlock as BlockSyntax).AddStatements(declaration));
            }
            else if (currentBlock is MethodDeclarationSyntax)
            {
                blocks.Pop();
                var declaration = LocalDeclarationStatement(statement);
                blocks.Push((currentBlock as MethodDeclarationSyntax).AddBodyStatements(declaration));
            }

            //if we forget some type of block, we will get an exception
            else throw new Exception($"current block was {currentBlock.GetType()}");
        }

        /// <summary>
        /// Index of service variable '#loop{id}' for loop cycle
        /// </summary>
        private int loopCountInd = 0;

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
                NamespaceDeclarationSyntax namespaceNode = 
                    NamespaceDeclaration(IdentifierName("RoslynApp")).AddMembers(programClassNode);

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
            unitNode = CompilationUnit().AddUsings(
                CreateUsingDirective("System")
            );

            //Create public class "Program"
            programClassNode = ClassDeclaration("Program")
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            //Create method void Main() 
            mainMethodNode = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), "Main");

            //Set Main method as static
            mainMethodNode = mainMethodNode.AddModifiers(Token(SyntaxKind.StaticKeyword));

            //Add to Main method empty body
            mainMethodNode = mainMethodNode.AddBodyStatements();

            //Set Main method as current block (push to stack)
            blocks.Push(mainMethodNode);

            //initialize dictionary of location annotations
            LocationAnnotations = new List<SyntaxAnnotation>();
        }

        private SyntaxNode GetNodeWithAnnotation(SyntaxNode node, LexLocation location)
        {
            var annotation = new SyntaxAnnotation(
                "LocationAnnotation",
                $"{location.StartLine},{location.StartColumn};{location.EndLine},{location.EndColumn}"
            );
            LocationAnnotations.Add(annotation);
            return node.WithAdditionalAnnotations(annotation);
        }

        private SyntaxToken GetNodeWithAnnotation(SyntaxToken node, LexLocation location)
        {
            var annotation = new SyntaxAnnotation(
                "LocationAnnotation",
                $"{location.StartLine},{location.StartColumn};{location.EndLine},{location.EndColumn}"
            );
            LocationAnnotations.Add(annotation);
            return node.WithAdditionalAnnotations(annotation);
        }

        public override void VisitIntNumNode(IntNumNode node)
        {
            var literal = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(node.Value));
            literal = GetNodeWithAnnotation(literal, node.Location) as LiteralExpressionSyntax;
            expressions.Push(literal);
        }

        public override void VisitRealNumNode(RealNumNode node)
        {
            var literal = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(node.Value));
            literal = GetNodeWithAnnotation(literal, node.Location) as LiteralExpressionSyntax;
            expressions.Push(literal);
        }

        public override void VisitIDNode(IDNode node)
        {
            var identifer = IdentifierName(node.Text);
            identifer = GetNodeWithAnnotation(identifer, node.Location) as IdentifierNameSyntax;
            expressions.Push(identifer);
        }

        public override void VisitTypeNode(TypeNode node)
        {
            var type = ParseTypeName(node.Name);
            type = GetNodeWithAnnotation(type, node.Location) as TypeSyntax;
            expressions.Push(type);
        }

        public override void VisitPrintNode(PrintNode node)
        {
            node.Expression.Visit(this);

            //"print" operation in my language is Console.WriteLine method call
            var printClassName = IdentifierName("Console");
            var printMethodName = IdentifierName("WriteLine");
            var printStatement = ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, printClassName, printMethodName)
                ).AddArgumentListArguments(Argument(expressions.Pop()))
            );
            AddStatementToCurrentBlock(printStatement);
        }

        public override void VisitDefineVarNode(DefineVarNode node)
        {
            node.Type.Visit(this);
            var type = expressions.Pop();

            var defineVar = VariableDeclaration(type as TypeSyntax);

            foreach (var variable in node.Variables)
            {
                variable.Expression?.Visit(this);

                var id = IdentifierName(variable.ID.Text);
                id = GetNodeWithAnnotation(id, variable.ID.Location) as IdentifierNameSyntax;

                var identifer = GetNodeWithAnnotation(id.Identifier, variable.ID.Location);
                var variableNode = VariableDeclarator(identifer);
                if (variable.Expression != null) variableNode = variableNode.WithInitializer(EqualsValueClause(expressions.Pop()));
                variableNode = GetNodeWithAnnotation(variableNode, variable.Location) as VariableDeclaratorSyntax;

                defineVar = defineVar.AddVariables(variableNode);
            }
            
            defineVar = GetNodeWithAnnotation(defineVar, node.Location) as VariableDeclarationSyntax;

            AddVariableToCurrentBlock(defineVar);
        }

        public override void VisitAssignVarNode(AssignVarNode node)
        {
            node.Expression.Visit(this);
            node.ID.Visit(this);

            var kindAssigment = SyntaxKind.SimpleAssignmentExpression;
            var assignVar = AssignmentExpression(kindAssigment, expressions.Pop(), expressions.Pop());
            assignVar = GetNodeWithAnnotation(assignVar, node.Location) as AssignmentExpressionSyntax;

            AddStatementToCurrentBlock(ExpressionStatement(assignVar));
        }

        public override void VisitBlockNode(BlockNode node)
        {
            if (!node.IsMainBlock) blocks.Push(Block());
            node.Statements.ForEach(statement => statement.Visit(this));
            if (!node.IsMainBlock) {
                var block = blocks.Pop();
                AddStatementToCurrentBlock(GetNodeWithAnnotation(block, node.Location) as BlockSyntax);
            }
        }

        public override void VisitDefineFunctionNode(DefineFunctionNode node)
        {
            TypeSyntax returnType = node.ReturnType.Name == "void"
                ? PredefinedType(Token(SyntaxKind.VoidKeyword))
                : ParseTypeName(node.ReturnType.Name);
            returnType = GetNodeWithAnnotation(returnType, node.ReturnType.Location) as TypeSyntax;

            var defineFunction = MethodDeclaration(returnType, node.ID.Text)
                .AddModifiers(Token(SyntaxKind.StaticKeyword));

            node.Arguments.ForEach(arg => defineFunction = defineFunction.AddParameterListParameters(
                Parameter(GetNodeWithAnnotation(Identifier(arg.Name.Text), arg.Name.Location))
                .WithType(GetNodeWithAnnotation(ParseTypeName(arg.Type.Name), arg.Type.Location) as TypeSyntax)
            ));

            if (node.Body.Statements.Count == 0) //if empty function
            {
                defineFunction = defineFunction.WithBody(Block());
            }
            else
            {
                blocks.Push(defineFunction);
                node.Body.Statements.ForEach(statement => statement.Visit(this));
                defineFunction = blocks.Pop() as MethodDeclarationSyntax;
            }

            defineFunction = GetNodeWithAnnotation(defineFunction, node.Location) as MethodDeclarationSyntax;
            programClassNode = programClassNode.AddMembers(defineFunction);
        }

        public override void VisitCallProcedureNode(CallProcedureNode node)
        {
            var procedureName = IdentifierName(node.Name.Text);
            procedureName = GetNodeWithAnnotation(procedureName, node.Name.Location) as IdentifierNameSyntax;

            var callProcedure = InvocationExpression(procedureName);

            foreach (var parameter in node.Arguments)
            {
                parameter.Expression.Visit(this);
                var arg = GetNodeWithAnnotation(Argument(expressions.Pop()), parameter.Location) as ArgumentSyntax;
                callProcedure = callProcedure.AddArgumentListArguments(arg);
            }

            callProcedure = GetNodeWithAnnotation(callProcedure, node.Location) as InvocationExpressionSyntax;
            AddStatementToCurrentBlock(ExpressionStatement(callProcedure));
        }

        public override void VisitCallFunctionNode(CallFunctionNode node)
        {
            var functionName = IdentifierName(node.Name.Text);
            functionName = GetNodeWithAnnotation(functionName, node.Name.Location) as IdentifierNameSyntax;

            var callFunction = InvocationExpression(functionName);

            foreach (var parameter in node.Arguments)
            {
                parameter.Expression.Visit(this);
                var arg = GetNodeWithAnnotation(Argument(expressions.Pop()), parameter.Location) as ArgumentSyntax;
                callFunction = callFunction.AddArgumentListArguments(arg);
            }

            callFunction = GetNodeWithAnnotation(callFunction, node.Location) as InvocationExpressionSyntax;
            expressions.Push(callFunction);
        }

        public override void VisitReturnNode(ReturnNode node)
        {
            node.Expression.Visit(this);

            var @return = ReturnStatement(expressions.Pop());
            @return = GetNodeWithAnnotation(@return, node.Location) as ReturnStatementSyntax;

            AddStatementToCurrentBlock(@return);
        }

        public override void VisitBinaryExpressionNode(BinaryExpressionNode node)
        {
            var operationKind = node.Operator switch
            {
                "+" => SyntaxKind.AddExpression,
                "-" => SyntaxKind.SubtractExpression,
                "*" => SyntaxKind.MultiplyExpression,
                "/" => SyntaxKind.DivideExpression,
                "%" => SyntaxKind.ModuloExpression,
                _ => throw new Exception($"forgot syntax kind for expression '{node.Operator}'"),
            };
            node.Right.Visit(this);
            node.Left.Visit(this);

            var expression = BinaryExpression(operationKind, expressions.Pop(), expressions.Pop());
            if (!node.IsInParens)
            {
                expression = GetNodeWithAnnotation(expression, node.Location) as BinaryExpressionSyntax;
                expressions.Push(expression);
            }
            else
            {
                var parenExpression = ParenthesizedExpression(expression);
                parenExpression = GetNodeWithAnnotation(parenExpression, node.Location) as ParenthesizedExpressionSyntax;
                expressions.Push(parenExpression);
            }
        }

        public override void VisitLoopNode(LoopNode node)
        {
            node.Count.Visit(this);
            var countExpr = expressions.Pop();
            countExpr = GetNodeWithAnnotation(countExpr, node.Count.Location) as ExpressionSyntax;

            string countVarName = "#loop" + ++loopCountInd;
            var count = VariableDeclaration(ParseTypeName("long"))
                .AddVariables(VariableDeclarator(countVarName)
                    .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))))
                );
            AddVariableToCurrentBlock(count);

            blocks.Push(Block());
            if (node.Statement is BlockNode) (node.Statement as BlockNode).Statements.ForEach(st => st.Visit(this));
            else node.Statement.Visit(this);

            var increment = ExpressionStatement(PrefixUnaryExpression(
                SyntaxKind.PreIncrementExpression,
                IdentifierName(countVarName)
            ));
            AddStatementToCurrentBlock(increment);

            var whileBlock = blocks.Pop() as BlockSyntax;
            var whileNode = WhileStatement(
                BinaryExpression(SyntaxKind.LessThanExpression, IdentifierName(countVarName), countExpr),
                whileBlock
            );
            AddStatementToCurrentBlock(whileNode);
        }
    }
}
