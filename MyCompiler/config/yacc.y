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
    public TypeNode typeValue;

    public List<AssignVarNode> defineVarsListValue;
}

%using System.IO;
%using MyCompiler.SyntaxTree;

%namespace MyCompiler

%token LRBRACKET RRBRACKET COMMA SEMICOLON PRINT LFBRACKET RFBRACKET
%token ASSIGNEQ

%token <iValue> INTNUM
%token <dValue> REALNUM
%token <sValue> ID

%type <exprValue> expression
%type <statValue> statement

%type <blockValue> statementsList block
%type <printValue> printStmt
%type <defineVarValue> defineVarsStmt
%type <assignVarValue> assignVarStmt defineVarsItem

%type <typeValue> type
%type <idValue> ident

%type <defineVarsListValue> defineVarList

%%

program         : statementsList { Root = $1; Root.Location = @$; (Root as BlockNode).IsMainBlock = true; } ;

type            : ID { $$ = new TypeNode($1, @$); } ;

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

statement       : printStmt SEMICOLON { $$ = $1; }
                | defineVarsStmt SEMICOLON { $$ = $1; }
                | assignVarStmt SEMICOLON { $$ = $1; }
                | block { $$ = $1; }
                ;

block           : LFBRACKET RFBRACKET { $$ = new BlockNode(@$); }
                | LFBRACKET statementsList RFBRACKET { $$ = $2; }
                ;

statementsList  : statement { $$ = new BlockNode($1, @$); }
                | statementsList statement { $1.AddStatement($2); $$ = $1; }
                ;

expression      : INTNUM { $$ = new IntNumNode($1, @$); }
                | REALNUM { $$ = new RealNumNode($1, @$); }
                | ident { $$ = $1; }
                ;

%%