%{
#define _CRT_NONSTDC_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <string.h>
#include <errno.h>

#define YYSTYPE char *
extern YYSTYPE yylval;
#include "syntax.tab.h"

void parseMessage(char* token);
%}

%token token_eif token_block token_comma token_dot token_module token_use token_if token_while token_iter
%token token_colon token_end token_else token_def token_class token_func token_prop token_option token_assert
%token token_conditional token_expose token_hidden token_readonly token_continue token_pass token_return 
%token token_break token_at token_in token_word token_ops_1 token_ops_2 token_ops_3 token_quote token_obrace
%token token_ebrace token_obracket token_ebracket token_oparen token_eparen token_newline token_integer token_digits

%%

commands: command
| commands token_newline command
;

command : { }
| expression{

}
| block {
	printf("[block]\n");
    return $1;
}
;

word: token_word
| token_quote
;

block: ifblock
| defblock
| conditionalblock
| whileblock
| iterblock
;

numeral : token_digits | token_integer

expression : numeral
| brace
| bracket
| paren
| word
| token_ops_1
| anonyfuncdef
| contentblock
| token_continue
| token_return
| token_pass
| token_break
| expression numeral
| expression brace
| expression bracket
| expression word
| expression paren
| expression token_ops_1
| expression anonyfuncdef
| expression contentblock
;

brace : token_obrace commasepgroup token_ebrace;

bracket : token_obracket commasepgroup token_ebracket;

paren : token_oparen commasepgroup token_eparen;

ifblock : token_if commands token_newline eifblock
| token_if commands token_newline elseblock
| token_if commands token_newline token_end
;

elseblock : token_else commands token_newline token_end;

eifblock : token_eif commands token_newline elseblock
| token_eif commands token_newline token_end
;

defblock : classdef
| funcdef
| propdef
;

commasepnames : word
| commasepnames token_comma word
;

commasepexprs : expression
| commasepexprs token_comma expression;

commasepgroup : commasepexprs
| commasepgroup token_newline commasepexprs;

funcpair: expression word{
    printf("[Expr %s] [Word %s]\n", $1, $2);
};

commaseppairs : 
| funcpair{
    printf("[ComSepPairs %s]\n", $1);
}
| commaseppairs token_comma funcpair{
    printf("[ComSepPairs %s, %s]\n", $1, $2);
}
;

classdef : token_class word token_colon commasepnames token_assert commasepnames token_newline commands token_end
| token_class word token_colon commasepnames token_newline commands token_end
| token_class word token_assert commasepnames token_newline commands token_end
;

funcdef: token_func word token_oparen commaseppairs token_eparen token_newline commands token_end{
    printf("[Function {%s : [ %s ]}\n", $2, $4);
};

anonyfuncdef : token_func token_oparen commaseppairs token_eparen token_newline commands token_end;

propdef : token_prop word token_ops_1 expression
| token_prop word
;

conditionalblock : token_conditional commands token_end;

whileblock : token_while commands token_end;

iterblock : token_iter word token_in expression token_at word token_newline commands token_end
| token_iter word token_in expression token_newline commands token_end
| token_iter expression token_newline commands token_end
;

contentblock : token_block commands token_end;

%%