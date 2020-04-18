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
    public IDNode idValue;
    public TypeNode typeValue;
}

%using System.IO;
%using MyCompiler.SyntaxTree;

%namespace MyCompiler

%token LRBRACKET RRBRACKET SEMICOLON PRINT
%token ASSIGNEQ

%token <iValue> INTNUM
%token <dValue> REALNUM
%token <sValue> ID

%type <exprValue> expression
%type <statValue> statement

%type <blockValue> statementsList
%type <printValue> printStmt
%type <defineVarValue> defineVarStmt

%type <typeValue> type
%type <idValue> ident

%%

program     : statementsList { Root = $1; Root.Location = @$; } ;

type    : ID { $$ = new TypeNode($1, @$); } ;

ident   : ID { $$ = new IDNode($1, @$); } ;

printStmt   : PRINT expression { $$ = new PrintNode($2, @$); }
            ;

defineVarStmt   : type ident { $$ = new DefineVarNode($1, $2, @$); } ;

statement   : printStmt SEMICOLON { $$ = $1; }
            | defineVarStmt SEMICOLON { $$ = $1; }
            ;

statementsList  : statement { $$ = new BlockNode($1, @$); }
                | statementsList statement { $1.AddStatement($2); $$ = $1; }
                ;

expression  : INTNUM { $$ = new IntNumNode($1, @$); }
            | REALNUM { $$ = new RealNumNode($1, @$); }
            | ident { $$ = $1; }
            ;

%%