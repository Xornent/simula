%{
#define _CRT_NONSTDC_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS

#include <string.h>
#include <stdio.h>
extern int line;
#include "syntax.tab.h"
#include "resources.h"
%}

%x comment
%x multiline

%%

module  return token_module;
use  return token_use;
if  return token_if;
eif  return token_eif;
while  return token_while;
iter  return token_iter;
end  return token_end;
else  return token_else;
def  return token_def;
class  return token_class;
func  return token_func;
prop  return token_prop;
option  return token_option;
assert  return token_assert;
conditional  return token_conditional;
expose  return token_expose;
hidden  return token_hidden;
readonly  return token_readonly;
continue  return token_continue;
pass  return token_pass;
return  return token_return;
break  return token_break;
at  return token_at;
in  return token_in;
block  return token_block;

[a-zA-Z_][a-zA-Z_0-9]* yylval.string = strcat_c("", yytext); return token_word;

[0-9]+ yylval.string = strcat_c("", yytext); return token_integer;
[0-9]+(\.[0-9]+) yylval.string = strcat_c("", yytext); return token_digits;

\,  return token_comma;
\.  return token_dot;
\:  return token_colon;
[\*\/\+\-\$\%\^\&\:\?\>\<\~\=]+  yylval.string = strcat_c(yytext, ""); return token_ops_1;

\"[^\"]*\"  yylval.string = strcat_c("", yytext); return token_quote;
\{  return token_obrace;
\}  return token_ebrace;
\[  return token_obracket;
\]  return token_ebracket;
\(  return token_oparen;
\)  return token_eparen;
;  return token_newline;
[ \t\s\S]+ {};
[\n\r]  line++; return token_newline;

"'" BEGIN comment;
<comment>[\r\n] BEGIN INITIAL;
<comment>. {}

"`:" BEGIN multiline;
<multiline>":`" BEGIN INITIAL;
<multiline>. {}

%%