%{
    public Parser(AbstractScanner<ValueType, LexLocation> scanner) : base(scanner) { }
    public Node Root { get; private set; } //a root of syntax tree
%}

%output = ../GeneratedParser/Parser.cs

%union {
    public double dValue;
    public int iValue;
    public string sValue;
    public ExprNode exprValue;
    public StatNode statValue;

    public BlockNode blockValue;
    public PrintNode printValue;
    public DefineVarNode defineVarValue;
    public AssignVarNode assignVarValue;
    public IDNode idValue;
    public ComplexIDNode complexIdValue;
    public TypeNode typeValue;
    public DefineFunctionNode defineFuncValue;
    public CallProcedureNode callProcValue;
    public CallFunctionNode callFuncValue;
    public ReturnNode returnValue;
    public LoopNode loopValue;
    public NetUsingNode netUsingValue;

    public List<AssignVarNode> defineVarsListValue;
    public List<DefineFunctionArgumentNode> defineFuncArgumentsValue;
    public List<CallFunctionArgumentNode> callFuncArgumentsValue;
}

%using System.IO;
%using MyCompiler.SyntaxTree;

%namespace MyCompiler

%token LRBRACKET RRBRACKET COMMA SEMICOLON PRINT LFBRACKET RFBRACKET RETURN LOOP DOT NETUSING
%token ASSIGNEQ
%token PLUS MINUS MUL DIV MOD

%token <iValue> INTNUM
%token <dValue> REALNUM
%token <sValue> ID

%type <exprValue> expression expr2 expr3
%type <statValue> statement

%type <blockValue> statementsList block
%type <printValue> printStmt
%type <defineVarValue> defineVarsStmt
%type <assignVarValue> assignVarStmt defineVarsItem

%type <typeValue> type
%type <idValue> ident
%type <complexIdValue> complexIdent complexIdent2

%type <defineVarsListValue> defineVarList
%type <defineFuncArgumentsValue> defFuncArgList
%type <callFuncArgumentsValue> callFuncArgList

%type <defineFuncValue> defineFuncStmt
%type <callProcValue> callFuncStmt
%type <callFuncValue> callFuncExpr
%type <returnValue> return
%type <loopValue> loop
%type <netUsingValue> netUsing

%%

program         : statementsList { Root = $1; Root.Location = @$; (Root as BlockNode).IsMainBlock = true; } ;

complexIdent    : complexIdent DOT complexIdent2 { $$ = new ComplexIDNode($1, $3, @$); }
                | complexIdent2 { $$ = $1; }
                ;

complexIdent2   : ident { $$ = new ComplexIDNode($1, null, @$); } ;

type            : complexIdent { $$ = new TypeNode($1, @$); } ;

ident           : ID { $$ = new IDNode($1, @$); } ;

printStmt       : PRINT expression { $$ = new PrintNode($2, @$); }
                ;

defineVarsStmt  : type defineVarList { $$ = new DefineVarNode($1, $2, @$); } ;

defineVarsItem  : ident { $$ = new AssignVarNode($1, null, @$); }
                | ident ASSIGNEQ expression { $$ = new AssignVarNode($1, $3, @$); }
                ;

defineVarList   : defineVarsItem { $$ = new List<AssignVarNode> { $1 }; }
                | defineVarList COMMA defineVarsItem { $1.Add($3); $$ = $1; }
                ;

assignVarStmt   : ident ASSIGNEQ expression { $$ = new AssignVarNode($1, $3, @$); } ;

defFuncArgList  : type ident { $$ = new List<DefineFunctionArgumentNode> { new DefineFunctionArgumentNode($1, $2, @$) }; }
                | defFuncArgList COMMA type ident { $1.Add(new DefineFunctionArgumentNode($3, $4, @$)); $$ = $1; }
                | { $$ = new List<DefineFunctionArgumentNode> { }; }
                ;

callFuncArgList : expression { $$ = new List<CallFunctionArgumentNode> { new CallFunctionArgumentNode($1, @$) }; }
                | callFuncArgList COMMA expression { $1.Add(new CallFunctionArgumentNode($3, @$)); $$ = $1; }
                | { $$ = new List<CallFunctionArgumentNode> { }; }
                ;

defineFuncStmt  : type ident LRBRACKET defFuncArgList RRBRACKET block
                  {
                    $$ = new DefineFunctionNode($1, $2, $4, $6, @$);
                  }
                ;

callFuncStmt    : complexIdent LRBRACKET callFuncArgList RRBRACKET { $$ = new CallProcedureNode($1, $3, @$); } ;

callFuncExpr    : complexIdent LRBRACKET callFuncArgList RRBRACKET { $$ = new CallFunctionNode($1, $3, @$); } ;

return          : RETURN expression { $$ = new ReturnNode($2, @$); } ;

loop            : LOOP expression statement { $$ = new LoopNode($2, $3, @$); } ;

netUsing        : NETUSING complexIdent { $$ = new NetUsingNode($2, @$); } ;

statement       : printStmt SEMICOLON { $$ = $1; }
                | defineVarsStmt SEMICOLON { $$ = $1; }
                | assignVarStmt SEMICOLON { $$ = $1; }
                | block { $$ = $1; }
                | defineFuncStmt { $$ = $1; }
                | callFuncStmt SEMICOLON { $$ = $1; }
                | return SEMICOLON { $$ = $1; }
                | loop { $$ = $1; }
                | netUsing SEMICOLON { $$ = $1; }
                ;

block           : LFBRACKET RFBRACKET { $$ = new BlockNode(@$); }
                | LFBRACKET statementsList RFBRACKET { $$ = $2; }
                ;

statementsList  : statement { $$ = new BlockNode($1, @$); }
                | statementsList statement { $1.AddStatement($2); $$ = $1; }
                ;

expression      : expression PLUS expr2 { $$ = new BinaryExpressionNode($1, $3, "+", @$); }
                | expression MINUS expr2 { $$ = new BinaryExpressionNode($1, $3, "-", @$); }
                | expr2 { $$ = $1; }
                ;

expr2           : expr2 MUL expr3 { $$ = new BinaryExpressionNode($1, $3, "*", @$); }
                | expr2 DIV expr3 { $$ = new BinaryExpressionNode($1, $3, "/", @$); }
                | expr2 MOD expr3 { $$ = new BinaryExpressionNode($1, $3, "%", @$); }
                | expr3 { $$ = $1; }
                ;

expr3           : INTNUM { $$ = new IntNumNode($1, @$); }
                | REALNUM { $$ = new RealNumNode($1, @$); }
                | complexIdent { $$ = $1; }
                | callFuncExpr { $$ = $1; }
                | LRBRACKET expression RRBRACKET { $2.IsInParens = true; $$ = $2; }
                ;

%%