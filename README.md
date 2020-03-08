# PL-Compiler
Simple programming language's compiler which uses Lex, Yacc and Roslyn

## Why are a few projects in this solution?
The main project is `MyCompiler`, it is a project of my language's compiler. Also in the solution there are some parts of `Roslyn` which were modified.

## How to build the `MyCompiler` project?
1. If you need, change file `config\lex.lex` (it is lexer's file) or `config\yacc.y` (it is Yacc's file with language's grammar)
2. Run `config\generate.bat` for generating code of lexer and parser
3. It will generate files `Lexer.cs` and `Parser.cs` in folder `GeneratedParser\`. These files have to be recompiled after any change of files `lex.lex` or `yacc.y`
4. Generated files has already been added to project `MyCompiler`. The project is ready for launch! 
