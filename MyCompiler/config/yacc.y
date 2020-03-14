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

%token <iValue> INTNUM
%token <dValue> REALNUM
%token <sValue> ID

%%

program     : ID
            ;

%%