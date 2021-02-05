
/* A Bison parser, made by GNU Bison 2.4.1.  */

/* Skeleton interface for Bison's Yacc-like parsers in C
   
      Copyright (C) 1984, 1989, 1990, 2000, 2001, 2002, 2003, 2004, 2005, 2006
   Free Software Foundation, Inc.
   
   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.
   
   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.
   
   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.  */

/* As a special exception, you may create a larger work that contains
   part or all of the Bison parser skeleton and distribute that work
   under terms of your choice, so long as that work isn't itself a
   parser generator using the skeleton or a modified version thereof
   as a parser skeleton.  Alternatively, if you modify or redistribute
   the parser skeleton itself, you may (at your option) remove this
   special exception, which will cause the skeleton and the resulting
   Bison output files to be licensed under the GNU General Public
   License without this special exception.
   
   This special exception was added by the Free Software Foundation in
   version 2.2 of Bison.  */


/* Tokens.  */
#ifndef YYTOKENTYPE
# define YYTOKENTYPE
   /* Put the tokens into the symbol table, so that GDB and other debuggers
      know about them.  */
   enum yytokentype {
     token_eif = 258,
     token_block = 259,
     token_comma = 260,
     token_dot = 261,
     token_module = 262,
     token_use = 263,
     token_if = 264,
     token_while = 265,
     token_iter = 266,
     token_colon = 267,
     token_end = 268,
     token_else = 269,
     token_def = 270,
     token_class = 271,
     token_func = 272,
     token_prop = 273,
     token_option = 274,
     token_assert = 275,
     token_conditional = 276,
     token_expose = 277,
     token_hidden = 278,
     token_readonly = 279,
     token_continue = 280,
     token_pass = 281,
     token_return = 282,
     token_break = 283,
     token_at = 284,
     token_in = 285,
     token_word = 286,
     token_ops_1 = 287,
     token_ops_2 = 288,
     token_ops_3 = 289,
     token_quote = 290,
     token_obrace = 291,
     token_ebrace = 292,
     token_obracket = 293,
     token_ebracket = 294,
     token_oparen = 295,
     token_eparen = 296,
     token_newline = 297,
     token_integer = 298,
     token_digits = 299
   };
#endif



#if ! defined YYSTYPE && ! defined YYSTYPE_IS_DECLARED
typedef int YYSTYPE;
# define YYSTYPE_IS_TRIVIAL 1
# define yystype YYSTYPE /* obsolescent; will be withdrawn */
# define YYSTYPE_IS_DECLARED 1
#endif

extern YYSTYPE yylval;


