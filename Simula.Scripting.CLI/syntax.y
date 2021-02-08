%{
#define _CRT_NONSTDC_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <string.h>
#include <errno.h>

#include "syntaxtree.h"
#include "syntax.tab.h"

#include "resources.h"

void parseMessage(char* token);
%}

%union {
    const char* string;
    syntaxTree* tree;
}

%token<string> token_eif token_block token_comma token_dot token_module token_use token_if token_while token_iter
%token<string> token_colon token_end token_else token_def token_class token_func token_prop token_option token_assert
%token<string> token_conditional token_expose token_hidden token_readonly token_return
%token<string> token_at token_in token_word token_quote token_obrace token_integer token_digits
%token<string> token_ebrace token_obracket token_ebracket token_oparen token_eparen token_newline
%token<tree> token_break token_pass token_continue token_ops_1

%type<tree> command commands returnstmt 
%type<tree> expression block ifblock defblock classdef funcdef propdef conditionalblock whileblock iterblock contentblock
%type<tree> brace bracket paren anonyfuncdef commasepnames commasepexprs commasepgroup funcpair commaseppairs
%type<tree> word numeral 

%%

commands: command{

syntaxTree* tree = createSyntaxTree(general);
appendChild($1, tree);
$$ = tree;

}
| commands token_newline command{

appendChild($3, $1);
$$ = $1;

}
;

command: { $$ = NULL; }
| expression{ $$ = $1; }
| block{ $$ = $1; }
;

word: token_word{ $$ = createLiteral($1); }
| token_quote{ $$ = createLiteral($1); }
;

block: ifblock{ $1->type = ifblock; $$ = $1; }
| defblock{ $$ = $1; }
| conditionalblock{ $1->type = conditionalblock; $$ = $1; }
| whileblock{ $1->type = whileblock; $$ = $1; }
| iterblock{ $1->type = iterateblock; $$ = $1; }
;

numeral: token_digits{ $$ = createLiteral($1); } | token_integer{ $$ = createLiteral($1); };

expression: numeral{ 

syntaxTree* tree = createSyntaxTree(evaluation);
appendChild($1, tree);
$$ = tree;

}
| brace
| bracket
| paren
| word{

syntaxTree * tree = createSyntaxTree(evaluation);
appendChild($1, tree);
$$ = tree;

}
| token_ops_1{

syntaxTree * tree = createSyntaxTree(evaluation);
appendChild($1, tree);
$$ = tree;

}
| anonyfuncdef
| contentblock
| token_continue
| token_return
| token_pass
| token_break
| expression numeral{

appendChild($2, $1);
$$ = $1;

}
| expression brace
| expression bracket
| expression word{

appendChild($2, $1);
$$ = $1;

}
| expression paren
| expression token_ops_1{

appendChild($2, $1);
$$ = $1;

}
| expression anonyfuncdef
| expression contentblock
;

returnstmt: token_return command;

brace: token_obrace commasepgroup token_ebrace;

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

defblock: classdef
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

funcpair: expression word;

commaseppairs : 
| funcpair
| commaseppairs token_comma funcpair
;

classdef : token_class word token_colon commasepnames token_assert commasepnames token_newline commands token_end
| token_class word token_colon commasepnames token_newline commands token_end
| token_class word token_assert commasepnames token_newline commands token_end
;

funcdef: token_func word token_oparen commaseppairs token_eparen token_newline commands token_end;

anonyfuncdef: token_func token_oparen commaseppairs token_eparen token_newline commands token_end;

propdef : token_prop word token_ops_1 expression
| token_prop word
;

conditionalblock : token_conditional commands token_end;

whileblock : token_while commands token_end;

iterblock : token_iter word token_in expression token_at word token_newline commands token_end
| token_iter word token_in expression token_newline commands token_end
| token_iter expression token_newline commands token_end
;

contentblock: token_block commands token_end{
    syntaxTree * tree = $2;
    tree->type = contentblock;
    $$ = tree;
};

%%