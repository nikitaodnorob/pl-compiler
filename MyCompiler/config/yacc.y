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
    public ArrayNode arrayValue;
    public TupleNode tupleValue;
    public DefineTupleNode defineTupleValue;
    public TupleVarNode tupleVarValue;
    public AssignTupleNode assignTupleValue;
    public ForNode forValue;

    public List<AssignVarNode> defineVarsListValue;
    public List<TypeIDListElementNode> typeIdListElementsValue;
    public List<ExprListElementNode> exprListElementValue;
    public List<ArrayElement> arrayElementsValue;
    public List<ExprNode> exprsListValue;
    public List<IDNode> idsListValue;
}

%using System.IO;
%using MyCompiler.SyntaxTree;

%namespace MyCompiler

%token COMMA SEMICOLON PRINT RETURN LOOP DOT NETUSING FOR IN
%token LRBRACKET RRBRACKET LFBRACKET RFBRACKET LSBRACKET RSBRACKET
%token ASSIGNEQ
%token PLUS MINUS MUL DIV MOD RANGE

%token <iValue> INTNUM
%token <dValue> REALNUM
%token <sValue> ID STRING

%type <exprValue> expression expr2 expr3
%type <statValue> statement stmt stmtSemicolon

%type <blockValue> statementsList block
%type <printValue> printStmt
%type <defineVarValue> defineVarsStmt
%type <assignVarValue> assignVarStmt defineVarsItem

%type <typeValue> type simpleType
%type <idValue> ident
%type <complexIdValue> complexIdent complexIdent2

%type <defineVarsListValue> defineVarList
%type <typeIdListElementsValue> defFuncArgList
%type <exprListElementValue> callFuncArgList
%type <arrayElementsValue> arrayElemsList

%type <defineFuncValue> defineFuncStmt
%type <callProcValue> callFuncStmt
%type <callFuncValue> callFuncExpr
%type <returnValue> return

%type <loopValue> loop

%type <netUsingValue> netUsing

%type <arrayValue> array

%type <exprsListValue> tupleExprList
%type <tupleValue> tupleExpr
%type <defineTupleValue> defineTuple
%type <tupleVarValue> tupleVar
%type <typeIdListElementsValue> defTupleVarsList
%type <idsListValue> tupleVarList
%type <assignTupleValue> assignTuple

%type <forValue> for

%%

program         : statementsList { Root = $1; Root.Location = @$; (Root as BlockNode).IsMainBlock = true; } ;

complexIdent    : complexIdent DOT complexIdent2 { $$ = new ComplexIDNode($1, $3, @$); }
                | complexIdent2 { $$ = $1; }
                ;

complexIdent2   : ident { $$ = new ComplexIDNode($1, null, @$); } ;

type            : complexIdent { $$ = new TypeNode($1, @$); }
                | complexIdent LSBRACKET RSBRACKET { $$ = new TypeNode($1, @$); $$.SetArrayType(); } 
                ;

simpleType      : complexIdent { $$ = new TypeNode($1, @$); } ; /* only type, without [] */

ident           : ID { $$ = new IDNode($1, @$); } ;

printStmt       : PRINT expression { $$ = new PrintNode($2, @$); } ;

defineVarsStmt  : type defineVarList { $$ = new DefineVarNode($1, $2, @$); } ;

defineVarsItem  : ident { $$ = new AssignVarNode($1, null, @$); }
                | ident ASSIGNEQ expression { $$ = new AssignVarNode($1, $3, @$); }
                ;

defineVarList   : defineVarsItem { $$ = new List<AssignVarNode> { $1 }; }
                | defineVarList COMMA defineVarsItem { $1.Add($3); $$ = $1; }
                ;

assignVarStmt   : ident ASSIGNEQ expression { $$ = new AssignVarNode($1, $3, @$); } ;

defFuncArgList  : type ident { $$ = new List<TypeIDListElementNode> { new TypeIDListElementNode($1, $2, @$) }; }
                | defFuncArgList COMMA type ident { $1.Add(new TypeIDListElementNode($3, $4, @$)); $$ = $1; }
                | { $$ = new List<TypeIDListElementNode> { }; }
                ;

callFuncArgList : expression { $$ = new List<ExprListElementNode> { new ExprListElementNode($1, @$) }; }
                | callFuncArgList COMMA expression { $1.Add(new ExprListElementNode($3, @$)); $$ = $1; }
                | { $$ = new List<ExprListElementNode> { }; }
                ;

defineFuncStmt  : type ident LRBRACKET defFuncArgList RRBRACKET block
                  {
                    $$ = new DefineFunctionNode($1, $2, $4, $6, @$);
                  }
                ;

callFuncStmt    : complexIdent LRBRACKET callFuncArgList RRBRACKET { $$ = new CallProcedureNode($1, $3, @$); } ;

callFuncExpr    : complexIdent LRBRACKET callFuncArgList RRBRACKET { $$ = new CallFunctionNode($1, $3, @$); } ;

return          : RETURN expression { $$ = new ReturnNode($2, @$); } ;

loop            : LOOP LRBRACKET expression RRBRACKET statement { $$ = new LoopNode($3, $5, @$); } ;

netUsing        : NETUSING complexIdent { $$ = new NetUsingNode($2, @$); } ;

arrayElemsList  : expression { $$ = new List<ArrayElement> { new ArrayElement($1, @$) }; }
                | arrayElemsList COMMA expression { $1.Add(new ArrayElement($3, @$)); $$ = $1; }
                | { $$ = new List<ArrayElement>{ }; }
                ;

array           : simpleType LFBRACKET arrayElemsList RFBRACKET { $$ = new ArrayNode($1, $3, @$); } ;

defTupleVarsList : type ident COMMA type ident
                    {
                        $$ = new List<TypeIDListElementNode> { new TypeIDListElementNode($1, $2), new TypeIDListElementNode($4, $5) };
                    }
                 | defTupleVarsList COMMA type ident { $1.Add(new TypeIDListElementNode($3, $4)); $$ = $1; }
                 ;

tupleExprList   : expression COMMA expression { $$ = new List<ExprNode> { $1, $3 }; }
                | tupleExprList COMMA expression { $1.Add($3); $$ = $1; }
                ;

tupleExpr       : LRBRACKET tupleExprList RRBRACKET { $$ = new TupleNode($2, @$); } ;

tupleVarList    : ident COMMA ident { $$ = new List<IDNode> { $1, $3 }; }
                | tupleVarList COMMA ident { $1.Add($3); $$ = $1; }
                ;

tupleVar        : LRBRACKET tupleVarList RRBRACKET { $$ = new TupleVarNode($2, @$); } ;

defineTuple     : LRBRACKET defTupleVarsList RRBRACKET ASSIGNEQ tupleExpr { $$ = new DefineTupleNode($2, $5, @$); }
                | LRBRACKET defTupleVarsList RRBRACKET ASSIGNEQ array { $$ = new DefineTupleNode($2, $5, @$); }
                | LRBRACKET defTupleVarsList RRBRACKET ASSIGNEQ ident { $$ = new DefineTupleNode($2, $5, @$); }
                ;

assignTuple     : tupleVar ASSIGNEQ tupleExpr { $$ = new AssignTupleNode($1, $3, @$); }
                | tupleVar ASSIGNEQ array { $$ = new AssignTupleNode($1, $3, @$); }
                | tupleVar ASSIGNEQ ident { $$ = new AssignTupleNode($1, $3, @$); }
                ;

for             : FOR ident IN INTNUM RANGE INTNUM statement { $$ = new ForNode($2, null, $4, $6, $7, @$); }
                | FOR type ident IN INTNUM RANGE INTNUM statement { $$ = new ForNode($3, $2, $5, $7, $8, @$); }
                ;

stmt            : block { $$ = $1; }
                | defineFuncStmt { $$ = $1; }
                | loop { $$ = $1; }
                | for { $$ = $1; }
                ;

stmtSemicolon   : printStmt { $$ = $1; }
                | defineVarsStmt { $$ = $1; }
                | assignVarStmt { $$ = $1; }
                | callFuncStmt { $$ = $1; }
                | return { $$ = $1; }
                | netUsing { $$ = $1; }
                | defineTuple { $$ = $1; }
                | assignTuple { $$ = $1; }
                ;

statement       : stmt { $$ = $1; }
                | stmtSemicolon SEMICOLON { $$ = $1; }
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
                | expr2 LSBRACKET expr3 RSBRACKET { $$ = new IndexAccessExpressionNode($1, $3, @$); }
                | expr3 { $$ = $1; }
                ;

expr3           : INTNUM { $$ = new IntNumNode($1, @$); }
                | REALNUM { $$ = new RealNumNode($1, @$); }
                | STRING { $$ = new StringNode($1, @$); }
                | complexIdent { $$ = $1; }
                | callFuncExpr { $$ = $1; }
                | array { $$ = $1; }
                | LRBRACKET expression RRBRACKET { $2.IsInParens = true; $$ = $2; }
                ;

%%