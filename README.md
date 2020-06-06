# PL-Compiler
Simple programming language's compiler for .NET Core platform which uses Lex, Yacc and Roslyn

## Why are a few projects in this solution?
The main project is `MyCompiler`, it is a project of my language's compiler. `Roslyn` is the submodule which contains modified `Microsoft.CodeAnalysis.CSharp` project.

## How to clone the solution?
Long-time option (because of too many commits in submodule's history)
```
$ git clone https://github.com/nikitaodnorob/pl-compiler.git --recursive
```
Fast option (doesn't load all history)
```
$ git clone https://github.com/nikitaodnorob/pl-compiler.git --depth 1
$ cd pl-compiler
$ git submodule update --init --depth 1 Roslyn
```

## How to build the `MyCompiler` project?
1. Open solution `Roslyn/Compilers.sln` and build project `Microsoft.CodeAnalysis.CSharp`
2. If you need, change file `MyCompiler/config/lex.lex` (it is lexer's file) or `MyCompiler/config/yacc.y` (it is Yacc's file with language's grammar)
3. Run `MyCompiler/config/generate.bat` for generating code of lexer and parser
4. It will generate files `Lexer.cs` and `Parser.cs` in folder `MyCompiler/GeneratedParser/`. These files have to be recompiled after any change of files `lex.lex` or `yacc.y`
5. Generated files has already been added to project `MyCompiler`. The project is ready for build! 
6. Build project `MyCompiler`

## How to use the `MyCompiler` project
The `MyCompiler.exe` is located in `MyCompiler/bin/Debug/netcoreapp3.1`

Some language examples are located in `MyCompiler/examples`

```
$ cd MyCompiler/bin/Debug/netcoreapp3.1/
$ MyCompiler.exe ../../../examples/tuples.mylang tuples.exe

$ dotnet tuples.exe
1
2
3
4
Unpacking array to variables
50
100
150
200
```

## System requirements
* OS Windows (for `gppg` launching; later the compiled parser will be uploaded to repository)
* Installed .NET Core version 3.1 or above
