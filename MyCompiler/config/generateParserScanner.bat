cls
gplex.exe /unicode /out:"../GeneratedParser/Lexer.cs" lex.lex
gppg.exe /no-lines /gplex yacc.y
