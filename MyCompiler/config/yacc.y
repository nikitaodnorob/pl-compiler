
%{
    public Parser(AbstractScanner<int, LexLocation> scanner) : base(scanner) { }
%}

%output = ../GeneratedParser/Parser.cs

%using System.IO;

%namespace MyCompiler

%%

program     :
            ;

%%