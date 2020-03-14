%{
    public Parser(AbstractScanner<ValueType, LexLocation> scanner) : base(scanner) { }
%}

%output = ../GeneratedParser/Parser.cs

%union {
    public double dValue;
    public int iValue;
    public string sValue;
}

%using System.IO;

%namespace MyCompiler

%token LRBRACKET RRBRACKET SEMICOLON PRINT

%token <iValue> INTNUM
%token <dValue> REALNUM
%token <sValue> ID

%%

program     : statementsList ;

printStmt   : PRINT expression
            ;

statement   : printStmt SEMICOLON
            ;

statementsList  : statement
                | statementsList statement
                ;

expression  : INTNUM
            | REALNUM
            | ID
            ;

%%