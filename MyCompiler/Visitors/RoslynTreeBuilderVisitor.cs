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
        /// List of .NET usings
        /// </summary>
        public List<string> Usings { get; private set; } = new List<string>();

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

        private readonly string libraryModuleName;

        /// <summary>
        /// Ready C# unit node which prepared for compilation
        /// </summary>
        public CompilationUnitSyntax UnitNode
        {
            get
            {
                //add C# Main method into class, if the file is not part of library
                if (libraryModuleName == null)
                    programClassNode = programClassNode.AddMembers(mainMethodNode);

                //declare namespace and add the class into it 
                string namespaceName = libraryModuleName == null ? "RoslynApp" : "MyCompilerLibrary";
                NamespaceDeclarationSyntax namespaceNode = 
                    NamespaceDeclaration(IdentifierName(namespaceName)).AddMembers(programClassNode);

                //add namespace into unit 
                unitNode = unitNode.AddMembers(namespaceNode);

                //add usings
                if (libraryModuleName == null) Usings.Add("MyCompilerLibrary");
                Usings.ForEach(@using => unitNode = unitNode.AddUsings(CreateUsingDirective(@using)));

                return unitNode;
            }
            private set { unitNode = value; }
        }

        /// <summary>
        /// Initializing of visitor
        /// </summary>
        public RoslynTreeBuilderVisitor(string libraryModuleName = null)
        {
            //Create unit
            unitNode = CompilationUnit();

            //Create public class "Program" (or library unit's name)
            this.libraryModuleName = libraryModuleName;
            string className = libraryModuleName ?? "Program";
            programClassNode = ClassDeclaration(className).AddModifiers(Token(SyntaxKind.PublicKeyword));

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

        public override void VisitStringNode(StringNode node)
        {
            var literal = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(node.Text));
            literal = GetNodeWithAnnotation(literal, node.Location) as LiteralExpressionSyntax;
            expressions.Push(literal);
        }

        public override void VisitIDNode(IDNode node)
        {
            var identifer = IdentifierName(node.Text);
            identifer = GetNodeWithAnnotation(identifer, node.Location) as IdentifierNameSyntax;
            expressions.Push(identifer);
        }

        public override void VisitComplexIDNode(ComplexIDNode node)
        {
            if (node.Member == null) VisitIDNode(node);
            else
            {
                node.Member.Visit(this);
                node.SourceObject.Visit(this);
                var memberAccess = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression, 
                    expressions.Pop(), 
                    expressions.Pop() as IdentifierNameSyntax
                );
                memberAccess = GetNodeWithAnnotation(memberAccess, node.Location) as MemberAccessExpressionSyntax;
                expressions.Push(memberAccess);
            }
        }

        public override void VisitTypeNode(TypeNode node)
        {
            var type = ParseTypeName(node.ID.Text);
            if (node.IsArray) 
                type = QualifiedName(
                    IdentifierName("MyCompilerLibrary"), 
                    GenericName("Array").AddTypeArgumentListArguments(type)
                );
            type = GetNodeWithAnnotation(type, node.Location) as TypeSyntax;
            expressions.Push(type);
        }

        public override void VisitPrintNode(PrintNode node)
        {
            node.Expression.Visit(this);

            //"print" operation in my language is System.Console.WriteLine method call
            var systemName = IdentifierName("System");
            var consoleName = IdentifierName("Console");
            var writeLineName = IdentifierName("WriteLine");
            var printStatement = ExpressionStatement(
                InvocationExpression(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, systemName, consoleName),
                    writeLineName
                )).AddArgumentListArguments(Argument(expressions.Pop()))
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
            TypeSyntax returnType = node.ReturnType.ID.Text == "void"
                ? PredefinedType(Token(SyntaxKind.VoidKeyword))
                : ParseTypeName(node.ReturnType.ID.Text);
            returnType = GetNodeWithAnnotation(returnType, node.ReturnType.Location) as TypeSyntax;

            var defineFunction = MethodDeclaration(returnType, node.ID.Text)
                .AddModifiers(Token(SyntaxKind.StaticKeyword));

            node.Arguments.ForEach(arg => defineFunction = defineFunction.AddParameterListParameters(
                Parameter(GetNodeWithAnnotation(Identifier(arg.Name.Text), arg.Name.Location))
                .WithType(GetNodeWithAnnotation(ParseTypeName(arg.Type.ID.Text), arg.Type.Location) as TypeSyntax)
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
            defineFunction = defineFunction.AddModifiers(Token(SyntaxKind.PublicKeyword));

            defineFunction = GetNodeWithAnnotation(defineFunction, node.Location) as MethodDeclarationSyntax;
            programClassNode = programClassNode.AddMembers(defineFunction);
        }

        public override void VisitCallProcedureNode(CallProcedureNode node)
        {
            node.Name.Visit(this);
            var procedureName = expressions.Pop();

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
            node.Name.Visit(this);
            var functionName = expressions.Pop();

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

        public override void VisitIndexAccessExpressionNode(IndexAccessExpressionNode node)
        {
            node.Index.Visit(this);
            node.Expression.Visit(this);

            var expression = ElementAccessExpression(expressions.Pop())
                .AddArgumentListArguments(Argument(expressions.Pop()));
            expression = GetNodeWithAnnotation(expression, node.Location) as ElementAccessExpressionSyntax;
            expressions.Push(expression);
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

        public override void VisitNetUsingNode(NetUsingNode node) => Usings.Add(node.ID.Text);

        public override void VisitArrayNode(ArrayNode node)
        {
            node.Type.Visit(this);

            var initializer = InitializerExpression(SyntaxKind.ArrayInitializerExpression);
            foreach (var element in node.Elements)
            {
                element.Expression.Visit(this);
                initializer = initializer.AddExpressions(expressions.Pop());
            }

            var arrayType = ArrayType(expressions.Pop() as TypeSyntax);
            var rankSpecifier = ArrayRankSpecifier().AddSizes(OmittedArraySizeExpression());
            var array = ArrayCreationExpression(arrayType)
                .AddTypeRankSpecifiers(rankSpecifier)
                .WithInitializer(initializer);

            var libraryArray = ObjectCreationExpression(
                    GenericName("Array").AddTypeArgumentListArguments(arrayType)
                ).AddArgumentListArguments(Argument(array));


            libraryArray = GetNodeWithAnnotation(libraryArray, node.Location) as ObjectCreationExpressionSyntax;
            expressions.Push(libraryArray);
        }

        public override void VisitTupleNode(TupleNode node)
        {
            var tuple = TupleExpression();
            foreach (var expr in node.Expressions)
            {
                expr.Visit(this);
                tuple = tuple.AddArguments(Argument(expressions.Pop()));
            }
            tuple = GetNodeWithAnnotation(tuple, node.Location) as TupleExpressionSyntax;
            expressions.Push(tuple);
        }

        public override void VisitDefineTupleNode(DefineTupleNode node)
        {
            node.TupleValue.Visit(this);

            var tuple = TupleExpression();
            foreach (var item in node.Variables)
            {
                item.Type.Visit(this);

                var variableDesignation = SingleVariableDesignation(Identifier(item.Name.Text));
                variableDesignation = GetNodeWithAnnotation(variableDesignation, item.Name.Location) 
                    as SingleVariableDesignationSyntax;
                tuple = tuple.AddArguments(Argument(DeclarationExpression(
                    expressions.Pop() as TypeSyntax,
                    variableDesignation
                )));
            }

            var kindAssigment = SyntaxKind.SimpleAssignmentExpression;
            var assignTuple = AssignmentExpression(kindAssigment, tuple, expressions.Pop());
            assignTuple = GetNodeWithAnnotation(assignTuple, node.Location) as AssignmentExpressionSyntax;

            AddStatementToCurrentBlock(ExpressionStatement(assignTuple));
        }

        public override void VisitAssignTupleNode(AssignTupleNode node)
        {
            node.Value.Visit(this);

            var tuple = TupleExpression();
            foreach (var id in node.Tuple.Variables)
            {
                id.Visit(this);
                tuple = tuple.AddArguments(Argument(expressions.Pop()));
            }

            var kindAssigment = SyntaxKind.SimpleAssignmentExpression;
            var assignTuple = AssignmentExpression(kindAssigment, tuple, expressions.Pop());
            assignTuple = GetNodeWithAnnotation(assignTuple, node.Location) as AssignmentExpressionSyntax;

            AddStatementToCurrentBlock(ExpressionStatement(assignTuple));
        }

        public override void VisitForNode(ForNode node)
        {
            blocks.Push(Block());
            node.Statement.Visit(this);

            node.ID.Visit(this);
            var counterId = expressions.Pop() as IdentifierNameSyntax;

            var block = blocks.Pop() as BlockSyntax;
            
            var @for = ForStatement(block)
                .AddInitializers(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    counterId,
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(node.From))
                ))
                .WithCondition(BinaryExpression(
                    SyntaxKind.LessThanOrEqualExpression,
                    counterId,
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(node.To))
                ))
                .AddIncrementors(PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, counterId));

            node.Type?.Visit(this);
            if (node.Type == null) @for = @for.AddInitializers(AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                counterId,
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(node.From))
            ));
            else @for = @for.WithDeclaration(
                VariableDeclaration(expressions.Pop() as TypeSyntax)
                    .AddVariables(VariableDeclarator(counterId.Identifier).WithInitializer(EqualsValueClause(
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(node.From))
                    )))
            );

            @for = GetNodeWithAnnotation(@for, node.Location) as ForStatementSyntax;

            AddStatementToCurrentBlock(@for);
        }
    }
}
