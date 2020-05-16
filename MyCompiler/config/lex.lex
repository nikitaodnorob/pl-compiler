%using QUT.Gppg;
%using System.Linq;

%namespace MyCompiler

Alpha		[a-zA-Z_]
Digit		[0-9]
AlphaDigit	{Alpha}|{Digit}
INTNUM		{Digit}+
REALNUM		{INTNUM}\.{INTNUM}
ID			{Alpha}{AlphaDigit}*

%%

{INTNUM} { 
	yylval.iValue = int.Parse(yytext);
	return (int)Tokens.INTNUM;
}

{REALNUM} { 
	yylval.dValue = double.Parse(yytext);
	return (int)Tokens.REALNUM;
}

{ID} {
	int res = LexerHelper.GetIDToken(yytext);
	if (res == (int)Tokens.ID) yylval.sValue = yytext;
	return res;
}

";" { return (int)Tokens.SEMICOLON; }
"," { return (int)Tokens.COMMA; }
"(" { return (int)Tokens.LRBRACKET; }
")" { return (int)Tokens.RRBRACKET; }
"=" { return (int)Tokens.ASSIGNEQ; }
"{" { return (int)Tokens.LFBRACKET; }
"}" { return (int)Tokens.RFBRACKET; }
"+" { return (int)Tokens.PLUS; }
"-" { return (int)Tokens.MINUS; }
"*" { return (int)Tokens.MUL; }
"/" { return (int)Tokens.DIV; }
"%" { return (int)Tokens.MOD; }
"." { return (int)Tokens.DOT; }
"[" { return (int)Tokens.LSBRACKET; }
"]" { return (int)Tokens.RSBRACKET; }

[^ \r\n\t] {
	LexError();
}

%{
    yylloc = new LexLocation(tokLin, tokCol, tokELin, tokECol);
%}

%%

public void LexError() //processing of lexical errors
{
    throw new Exception(string.Format("Error: unexpected token {0} in ({1}:{2})", yytext, yyline, yycol + 1));
}

public override void yyerror(string format, params object[] args) //processing of grammar errors
{
    throw new Exception(string.Format("Error: unexpected symbol {0} in ({1}:{2})", args[0], yyline, yycol + 1));
}

static class LexerHelper
{
    private static Dictionary<string,int> keywords;

    static LexerHelper()
    {
        keywords = new Dictionary<string,int>();
        keywords.Add("print", (int)Tokens.PRINT);
        keywords.Add("return", (int)Tokens.RETURN);
        keywords.Add("loop", (int)Tokens.LOOP);
        keywords.Add("netusing", (int)Tokens.NETUSING);
    }

    public static int GetIDToken(string s) => keywords.ContainsKey(s) ? keywords[s] : (int)Tokens.ID;
}