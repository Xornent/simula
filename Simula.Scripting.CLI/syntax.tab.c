
/* A Bison parser, made by GNU Bison 2.4.1.  */

/* Skeleton implementation for Bison's Yacc-like parsers in C
   
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

/* C LALR(1) parser skeleton written by Richard Stallman, by
   simplifying the original so-called "semantic" parser.  */

/* All symbols defined below should begin with yy or YY, to avoid
   infringing on user name space.  This should be done even for local
   variables, as they might otherwise be expanded by user macros.
   There are some unavoidable exceptions within include files to
   define necessary library symbols; they are noted "INFRINGES ON
   USER NAME SPACE" below.  */

/* Identify Bison output.  */
#define YYBISON 1

/* Bison version.  */
#define YYBISON_VERSION "2.4.1"

/* Skeleton name.  */
#define YYSKELETON_NAME "yacc.c"

/* Pure parsers.  */
#define YYPURE 0

/* Push parsers.  */
#define YYPUSH 0

/* Pull parsers.  */
#define YYPULL 1

/* Using locations.  */
#define YYLSP_NEEDED 0



/* Copy the first part of user declarations.  */

/* Line 189 of yacc.c  */
#line 1 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"

#define _CRT_NONSTDC_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <string.h>
#include <errno.h>

#include "syntaxtree.h"
#include "syntax.tab.h"

#include "resources.h"

void parseMessage(char* token);


/* Line 189 of yacc.c  */
#line 89 "syntax.tab.c"

/* Enabling traces.  */
#ifndef YYDEBUG
# define YYDEBUG 0
#endif

/* Enabling verbose error messages.  */
#ifdef YYERROR_VERBOSE
# undef YYERROR_VERBOSE
# define YYERROR_VERBOSE 1
#else
# define YYERROR_VERBOSE 0
#endif

/* Enabling the token table.  */
#ifndef YYTOKEN_TABLE
# define YYTOKEN_TABLE 0
#endif


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
     token_return = 280,
     token_at = 281,
     token_in = 282,
     token_word = 283,
     token_quote = 284,
     token_obrace = 285,
     token_integer = 286,
     token_digits = 287,
     token_ebrace = 288,
     token_obracket = 289,
     token_ebracket = 290,
     token_oparen = 291,
     token_eparen = 292,
     token_newline = 293,
     token_break = 294,
     token_pass = 295,
     token_continue = 296,
     token_ops_1 = 297
   };
#endif



#if ! defined YYSTYPE && ! defined YYSTYPE_IS_DECLARED
typedef union YYSTYPE
{

/* Line 214 of yacc.c  */
#line 16 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"

    const char* string;
    syntaxTree* tree;



/* Line 214 of yacc.c  */
#line 174 "syntax.tab.c"
} YYSTYPE;
# define YYSTYPE_IS_TRIVIAL 1
# define yystype YYSTYPE /* obsolescent; will be withdrawn */
# define YYSTYPE_IS_DECLARED 1
#endif


/* Copy the second part of user declarations.  */


/* Line 264 of yacc.c  */
#line 186 "syntax.tab.c"

#ifdef short
# undef short
#endif

#ifdef YYTYPE_UINT8
typedef YYTYPE_UINT8 yytype_uint8;
#else
typedef unsigned char yytype_uint8;
#endif

#ifdef YYTYPE_INT8
typedef YYTYPE_INT8 yytype_int8;
#elif (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
typedef signed char yytype_int8;
#else
typedef short int yytype_int8;
#endif

#ifdef YYTYPE_UINT16
typedef YYTYPE_UINT16 yytype_uint16;
#else
typedef unsigned short int yytype_uint16;
#endif

#ifdef YYTYPE_INT16
typedef YYTYPE_INT16 yytype_int16;
#else
typedef short int yytype_int16;
#endif

#ifndef YYSIZE_T
# ifdef __SIZE_TYPE__
#  define YYSIZE_T __SIZE_TYPE__
# elif defined size_t
#  define YYSIZE_T size_t
# elif ! defined YYSIZE_T && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
#  include <stddef.h> /* INFRINGES ON USER NAME SPACE */
#  define YYSIZE_T size_t
# else
#  define YYSIZE_T unsigned int
# endif
#endif

#define YYSIZE_MAXIMUM ((YYSIZE_T) -1)

#ifndef YY_
# if YYENABLE_NLS
#  if ENABLE_NLS
#   include <libintl.h> /* INFRINGES ON USER NAME SPACE */
#   define YY_(msgid) dgettext ("bison-runtime", msgid)
#  endif
# endif
# ifndef YY_
#  define YY_(msgid) msgid
# endif
#endif

/* Suppress unused-variable warnings by "using" E.  */
#if ! defined lint || defined __GNUC__
# define YYUSE(e) ((void) (e))
#else
# define YYUSE(e) /* empty */
#endif

/* Identity function, used to suppress warnings about constant conditions.  */
#ifndef lint
# define YYID(n) (n)
#else
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static int
YYID (int yyi)
#else
static int
YYID (yyi)
    int yyi;
#endif
{
  return yyi;
}
#endif

#if ! defined yyoverflow || YYERROR_VERBOSE

/* The parser invokes alloca or malloc; define the necessary symbols.  */

# ifdef YYSTACK_USE_ALLOCA
#  if YYSTACK_USE_ALLOCA
#   ifdef __GNUC__
#    define YYSTACK_ALLOC __builtin_alloca
#   elif defined __BUILTIN_VA_ARG_INCR
#    include <alloca.h> /* INFRINGES ON USER NAME SPACE */
#   elif defined _AIX
#    define YYSTACK_ALLOC __alloca
#   elif defined _MSC_VER
#    include <malloc.h> /* INFRINGES ON USER NAME SPACE */
#    define alloca _alloca
#   else
#    define YYSTACK_ALLOC alloca
#    if ! defined _ALLOCA_H && ! defined _STDLIB_H && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
#     include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
#     ifndef _STDLIB_H
#      define _STDLIB_H 1
#     endif
#    endif
#   endif
#  endif
# endif

# ifdef YYSTACK_ALLOC
   /* Pacify GCC's `empty if-body' warning.  */
#  define YYSTACK_FREE(Ptr) do { /* empty */; } while (YYID (0))
#  ifndef YYSTACK_ALLOC_MAXIMUM
    /* The OS might guarantee only one guard page at the bottom of the stack,
       and a page size can be as small as 4096 bytes.  So we cannot safely
       invoke alloca (N) if N exceeds 4096.  Use a slightly smaller number
       to allow for a few compiler-allocated temporary stack slots.  */
#   define YYSTACK_ALLOC_MAXIMUM 4032 /* reasonable circa 2006 */
#  endif
# else
#  define YYSTACK_ALLOC YYMALLOC
#  define YYSTACK_FREE YYFREE
#  ifndef YYSTACK_ALLOC_MAXIMUM
#   define YYSTACK_ALLOC_MAXIMUM YYSIZE_MAXIMUM
#  endif
#  if (defined __cplusplus && ! defined _STDLIB_H \
       && ! ((defined YYMALLOC || defined malloc) \
	     && (defined YYFREE || defined free)))
#   include <stdlib.h> /* INFRINGES ON USER NAME SPACE */
#   ifndef _STDLIB_H
#    define _STDLIB_H 1
#   endif
#  endif
#  ifndef YYMALLOC
#   define YYMALLOC malloc
#   if ! defined malloc && ! defined _STDLIB_H && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
void *malloc (YYSIZE_T); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
#  ifndef YYFREE
#   define YYFREE free
#   if ! defined free && ! defined _STDLIB_H && (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
void free (void *); /* INFRINGES ON USER NAME SPACE */
#   endif
#  endif
# endif
#endif /* ! defined yyoverflow || YYERROR_VERBOSE */


#if (! defined yyoverflow \
     && (! defined __cplusplus \
	 || (defined YYSTYPE_IS_TRIVIAL && YYSTYPE_IS_TRIVIAL)))

/* A type that is properly aligned for any stack member.  */
union yyalloc
{
  yytype_int16 yyss_alloc;
  YYSTYPE yyvs_alloc;
};

/* The size of the maximum gap between one aligned stack and the next.  */
# define YYSTACK_GAP_MAXIMUM (sizeof (union yyalloc) - 1)

/* The size of an array large to enough to hold all stacks, each with
   N elements.  */
# define YYSTACK_BYTES(N) \
     ((N) * (sizeof (yytype_int16) + sizeof (YYSTYPE)) \
      + YYSTACK_GAP_MAXIMUM)

/* Copy COUNT objects from FROM to TO.  The source and destination do
   not overlap.  */
# ifndef YYCOPY
#  if defined __GNUC__ && 1 < __GNUC__
#   define YYCOPY(To, From, Count) \
      __builtin_memcpy (To, From, (Count) * sizeof (*(From)))
#  else
#   define YYCOPY(To, From, Count)		\
      do					\
	{					\
	  YYSIZE_T yyi;				\
	  for (yyi = 0; yyi < (Count); yyi++)	\
	    (To)[yyi] = (From)[yyi];		\
	}					\
      while (YYID (0))
#  endif
# endif

/* Relocate STACK from its old location to the new one.  The
   local variables YYSIZE and YYSTACKSIZE give the old and new number of
   elements in the stack, and YYPTR gives the new location of the
   stack.  Advance YYPTR to a properly aligned location for the next
   stack.  */
# define YYSTACK_RELOCATE(Stack_alloc, Stack)				\
    do									\
      {									\
	YYSIZE_T yynewbytes;						\
	YYCOPY (&yyptr->Stack_alloc, Stack, yysize);			\
	Stack = &yyptr->Stack_alloc;					\
	yynewbytes = yystacksize * sizeof (*Stack) + YYSTACK_GAP_MAXIMUM; \
	yyptr += yynewbytes / sizeof (*yyptr);				\
      }									\
    while (YYID (0))

#endif

/* YYFINAL -- State number of the termination state.  */
#define YYFINAL  56
/* YYLAST -- Last index in YYTABLE.  */
#define YYLAST   365

/* YYNTOKENS -- Number of terminals.  */
#define YYNTOKENS  43
/* YYNNTS -- Number of nonterminals.  */
#define YYNNTS  27
/* YYNRULES -- Number of rules.  */
#define YYNRULES  70
/* YYNRULES -- Number of states.  */
#define YYNSTATES  139

/* YYTRANSLATE(YYLEX) -- Bison symbol number corresponding to YYLEX.  */
#define YYUNDEFTOK  2
#define YYMAXUTOK   297

#define YYTRANSLATE(YYX)						\
  ((unsigned int) (YYX) <= YYMAXUTOK ? yytranslate[YYX] : YYUNDEFTOK)

/* YYTRANSLATE[YYLEX] -- Bison symbol number corresponding to YYLEX.  */
static const yytype_uint8 yytranslate[] =
{
       0,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     2,     2,     2,     2,
       2,     2,     2,     2,     2,     2,     1,     2,     3,     4,
       5,     6,     7,     8,     9,    10,    11,    12,    13,    14,
      15,    16,    17,    18,    19,    20,    21,    22,    23,    24,
      25,    26,    27,    28,    29,    30,    31,    32,    33,    34,
      35,    36,    37,    38,    39,    40,    41,    42
};

#if YYDEBUG
/* YYPRHS[YYN] -- Index of the first RHS symbol of rule number YYN in
   YYRHS.  */
static const yytype_uint8 yyprhs[] =
{
       0,     0,     3,     5,     9,    10,    12,    14,    16,    18,
      20,    22,    24,    26,    28,    30,    32,    34,    36,    38,
      40,    42,    44,    46,    48,    50,    52,    54,    56,    59,
      62,    65,    68,    71,    74,    77,    80,    84,    88,    92,
      97,   102,   107,   112,   117,   122,   124,   126,   128,   130,
     134,   136,   140,   142,   146,   149,   150,   152,   156,   166,
     174,   182,   191,   199,   204,   207,   211,   215,   225,   233,
     239
};

/* YYRHS -- A `-1'-separated list of the rules' RHS.  */
static const yytype_int8 yyrhs[] =
{
      44,     0,    -1,    45,    -1,    44,    38,    45,    -1,    -1,
      49,    -1,    47,    -1,    28,    -1,    29,    -1,    53,    -1,
      56,    -1,    66,    -1,    67,    -1,    68,    -1,    32,    -1,
      31,    -1,    48,    -1,    50,    -1,    51,    -1,    52,    -1,
      46,    -1,    42,    -1,    64,    -1,    69,    -1,    41,    -1,
      25,    -1,    40,    -1,    39,    -1,    49,    48,    -1,    49,
      50,    -1,    49,    51,    -1,    49,    46,    -1,    49,    52,
      -1,    49,    42,    -1,    49,    64,    -1,    49,    69,    -1,
      30,    59,    33,    -1,    34,    59,    35,    -1,    36,    59,
      37,    -1,     9,    44,    38,    55,    -1,     9,    44,    38,
      54,    -1,     9,    44,    38,    13,    -1,    14,    44,    38,
      13,    -1,     3,    44,    38,    54,    -1,     3,    44,    38,
      13,    -1,    62,    -1,    63,    -1,    65,    -1,    46,    -1,
      57,     5,    46,    -1,    49,    -1,    58,     5,    49,    -1,
      58,    -1,    59,    38,    58,    -1,    49,    46,    -1,    -1,
      60,    -1,    61,     5,    60,    -1,    16,    46,    12,    57,
      20,    57,    38,    44,    13,    -1,    16,    46,    12,    57,
      38,    44,    13,    -1,    16,    46,    20,    57,    38,    44,
      13,    -1,    17,    46,    36,    61,    37,    38,    44,    13,
      -1,    17,    36,    61,    37,    38,    44,    13,    -1,    18,
      46,    42,    49,    -1,    18,    46,    -1,    21,    44,    13,
      -1,    10,    44,    13,    -1,    11,    46,    27,    49,    26,
      46,    38,    44,    13,    -1,    11,    46,    27,    49,    38,
      44,    13,    -1,    11,    49,    38,    44,    13,    -1,     4,
      44,    13,    -1
};

/* YYRLINE[YYN] -- source line where rule number YYN was defined.  */
static const yytype_uint8 yyrline[] =
{
       0,    35,    35,    42,    50,    51,    52,    55,    56,    59,
      60,    61,    62,    63,    66,    66,    68,    75,    76,    77,
      78,    85,    92,    93,    94,    95,    96,    97,    98,   104,
     105,   106,   112,   113,   119,   120,   125,   127,   129,   131,
     132,   133,   136,   138,   139,   142,   143,   144,   147,   148,
     151,   152,   154,   155,   157,   159,   160,   161,   164,   165,
     166,   169,   171,   173,   174,   177,   179,   181,   182,   183,
     186
};
#endif

#if YYDEBUG || YYERROR_VERBOSE || YYTOKEN_TABLE
/* YYTNAME[SYMBOL-NUM] -- String name of the symbol SYMBOL-NUM.
   First, the terminals, then, starting at YYNTOKENS, nonterminals.  */
static const char *const yytname[] =
{
  "$end", "error", "$undefined", "token_eif", "token_block",
  "token_comma", "token_dot", "token_module", "token_use", "token_if",
  "token_while", "token_iter", "token_colon", "token_end", "token_else",
  "token_def", "token_class", "token_func", "token_prop", "token_option",
  "token_assert", "token_conditional", "token_expose", "token_hidden",
  "token_readonly", "token_return", "token_at", "token_in", "token_word",
  "token_quote", "token_obrace", "token_integer", "token_digits",
  "token_ebrace", "token_obracket", "token_ebracket", "token_oparen",
  "token_eparen", "token_newline", "token_break", "token_pass",
  "token_continue", "token_ops_1", "$accept", "commands", "command",
  "word", "block", "numeral", "expression", "brace", "bracket", "paren",
  "ifblock", "elseblock", "eifblock", "defblock", "commasepnames",
  "commasepexprs", "commasepgroup", "funcpair", "commaseppairs",
  "classdef", "funcdef", "anonyfuncdef", "propdef", "conditionalblock",
  "whileblock", "iterblock", "contentblock", 0
};
#endif

# ifdef YYPRINT
/* YYTOKNUM[YYLEX-NUM] -- Internal token number corresponding to
   token YYLEX-NUM.  */
static const yytype_uint16 yytoknum[] =
{
       0,   256,   257,   258,   259,   260,   261,   262,   263,   264,
     265,   266,   267,   268,   269,   270,   271,   272,   273,   274,
     275,   276,   277,   278,   279,   280,   281,   282,   283,   284,
     285,   286,   287,   288,   289,   290,   291,   292,   293,   294,
     295,   296,   297
};
# endif

/* YYR1[YYN] -- Symbol number of symbol that rule YYN derives.  */
static const yytype_uint8 yyr1[] =
{
       0,    43,    44,    44,    45,    45,    45,    46,    46,    47,
      47,    47,    47,    47,    48,    48,    49,    49,    49,    49,
      49,    49,    49,    49,    49,    49,    49,    49,    49,    49,
      49,    49,    49,    49,    49,    49,    50,    51,    52,    53,
      53,    53,    54,    55,    55,    56,    56,    56,    57,    57,
      58,    58,    59,    59,    60,    61,    61,    61,    62,    62,
      62,    63,    64,    65,    65,    66,    67,    68,    68,    68,
      69
};

/* YYR2[YYN] -- Number of symbols composing right hand side of rule YYN.  */
static const yytype_uint8 yyr2[] =
{
       0,     2,     1,     3,     0,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     1,     1,
       1,     1,     1,     1,     1,     1,     1,     1,     2,     2,
       2,     2,     2,     2,     2,     2,     3,     3,     3,     4,
       4,     4,     4,     4,     4,     1,     1,     1,     1,     3,
       1,     3,     1,     3,     2,     0,     1,     3,     9,     7,
       7,     8,     7,     4,     2,     3,     3,     9,     7,     5,
       3
};

/* YYDEFACT[STATE-NAME] -- Default rule to reduce with in state
   STATE-NUM when YYTABLE doesn't specify something else to do.  Zero
   means the default is an error.  */
static const yytype_uint8 yydefact[] =
{
       4,     4,     4,     4,     0,     0,     0,     0,     4,    25,
       7,     8,     0,    15,    14,     0,     0,    27,    26,    24,
      21,     0,     2,    20,     6,    16,     5,    17,    18,    19,
       9,    10,    45,    46,    22,    47,    11,    12,    13,    23,
       0,     0,     0,     0,    20,     0,     0,    55,     0,    64,
       0,    50,    52,     0,     0,     0,     1,     4,    33,    31,
      28,    29,    30,    32,    34,    35,    70,     4,    66,     0,
       4,     0,     0,     0,    56,     0,    55,     0,    65,     0,
      36,     0,    37,    38,     3,     4,    41,     4,    40,    39,
       0,     0,    48,     0,     0,    31,     0,     0,     0,    63,
      51,    53,     0,     0,     0,     4,    69,     0,     0,     4,
       4,    57,     4,     0,     4,     4,     0,     0,    49,     0,
       0,     0,     0,     4,    44,    43,    42,     4,    68,     4,
      59,    60,    62,     0,     0,     0,    61,    67,    58
};

/* YYDEFGOTO[NTERM-NUM].  */
static const yytype_int8 yydefgoto[] =
{
      -1,    21,    22,    23,    24,    25,    26,    27,    28,    29,
      30,    88,    89,    31,    93,    52,    53,    74,    75,    32,
      33,    34,    35,    36,    37,    38,    39
};

/* YYPACT[STATE-NUM] -- Index in YYTABLE of the portion describing
   STATE-NUM.  */
#define YYPACT_NINF -70
static const yytype_int16 yypact[] =
{
     289,   289,   289,   289,   307,     8,   146,     8,   289,   -70,
     -70,   -70,   307,   -70,   -70,   307,   307,   -70,   -70,   -70,
     -70,    22,   -70,   -70,   -70,   -70,    13,   -70,   -70,   -70,
     -70,   -70,   -70,   -70,   -70,   -70,   -70,   -70,   -70,   -70,
      12,   -32,    62,   -18,    -3,   323,    -4,   307,    17,    21,
     103,    13,    54,   100,    36,    83,   -70,   289,   -70,   -70,
     -70,   -70,   -70,   -70,   -70,   -70,   -70,   187,   -70,   307,
     289,     8,     8,    13,   -70,    28,   307,   307,   -70,   307,
     -70,   307,   -70,   -70,   -70,   289,   -70,   289,   -70,   -70,
     135,   104,   -70,     0,    14,    30,   307,    32,    61,    13,
      13,    54,    43,    51,     8,   289,   -70,     8,     8,   289,
     289,   -70,   289,    63,   221,   255,    97,   105,   -70,    26,
     110,   111,   112,   289,   -70,   -70,   -70,   289,   -70,   289,
     -70,   -70,   -70,   116,   117,   119,   -70,   -70,   -70
};

/* YYPGOTO[NTERM-NUM].  */
static const yytype_int16 yypgoto[] =
{
     -70,    -1,   -53,     6,   -70,   -17,    11,    -5,     3,    46,
     -70,   -15,   -70,   -70,   -69,    66,   165,    31,    68,   -70,
     -70,    86,   -70,   -70,   -70,   -70,    89
};

/* YYTABLE[YYPACT[STATE-NUM]].  What to do in state STATE-NUM.  If
   positive, shift that token.  If negative, reduce the rule which
   number is the opposite.  If zero, do what YYDEFACT says.
   If YYTABLE_NINF, syntax error.  */
#define YYTABLE_NINF -55
static const yytype_int16 yytable[] =
{
      40,    41,    42,    94,    84,   107,    67,    50,    71,    60,
      44,    46,    48,    49,    84,    45,    72,     1,    47,   107,
     108,    61,    56,    51,    69,    66,    51,    51,    60,    62,
      43,   107,    59,    96,    60,   -54,    10,    11,   109,   119,
      61,    10,    11,    12,    13,    14,    61,    15,    62,    16,
      57,    59,   110,    76,    62,    58,    60,    59,    73,    79,
      57,    84,    84,    77,   129,    97,    96,   -54,    61,    91,
     112,    82,    63,    60,    81,    68,    62,    92,    92,    95,
      90,   114,    60,    60,   102,    61,   103,    73,    99,   115,
     100,    63,    51,    62,    61,    61,    59,    63,   113,   125,
      57,   123,    62,    62,   117,    59,    59,    73,   120,   121,
     116,   122,    64,   118,    92,    65,    78,   106,   128,    63,
      83,    81,   133,   130,   131,   132,   134,   111,   135,   136,
     137,    64,   138,    80,    65,   127,    63,    64,    81,     1,
      65,    57,    57,    57,    98,    63,    63,   101,    57,    57,
      57,     0,    43,     0,    57,    57,     0,    57,     0,    64,
       0,   104,    65,    10,    11,    12,    13,    14,     0,    15,
       0,    16,     0,   105,    10,    11,    64,    58,     0,    65,
      54,    55,    47,     0,     0,    64,    64,     0,    65,    65,
      85,     1,     0,     0,     0,     0,     2,     3,     4,     0,
      86,    87,     0,     5,     6,     7,     0,     0,     8,     0,
       0,     0,     9,     0,     0,    10,    11,    12,    13,    14,
       0,    15,     0,    16,     0,     1,    17,    18,    19,    20,
       2,     3,     4,     0,   124,    87,     0,     5,     6,     7,
       0,     0,     8,     0,     0,     0,     9,     0,     0,    10,
      11,    12,    13,    14,     0,    15,     0,    16,     0,     1,
      17,    18,    19,    20,     2,     3,     4,     0,   126,     0,
       0,     5,     6,     7,     0,     0,     8,     0,     0,     0,
       9,     0,     0,    10,    11,    12,    13,    14,     0,    15,
       0,    16,     0,     1,    17,    18,    19,    20,     2,     3,
       4,     0,     0,     0,     0,     5,     6,     7,     0,     0,
       8,     1,     0,     0,     9,     0,     0,    10,    11,    12,
      13,    14,     0,    15,    43,    16,     0,     1,    17,    18,
      19,    20,     9,     0,     0,    10,    11,    12,    13,    14,
      43,    15,     0,    16,     0,     0,    17,    18,    19,    20,
       0,    10,    11,    12,    13,    14,     0,    15,     0,    16,
       0,    70,     0,     0,     0,    58
};

static const yytype_int16 yycheck[] =
{
       1,     2,     3,    72,    57,     5,    38,     8,    12,    26,
       4,     5,     6,     7,    67,     4,    20,     4,    36,     5,
      20,    26,     0,    12,    27,    13,    15,    16,    45,    26,
      17,     5,    26,     5,    51,     5,    28,    29,    38,   108,
      45,    28,    29,    30,    31,    32,    51,    34,    45,    36,
      38,    45,    38,    36,    51,    42,    73,    51,    47,     5,
      38,   114,   115,    42,    38,    37,     5,    37,    73,    70,
      38,    35,    26,    90,    38,    13,    73,    71,    72,    73,
      69,    38,    99,   100,    85,    90,    87,    76,    77,    38,
      79,    45,    81,    90,    99,   100,    90,    51,    37,   114,
      38,    38,    99,   100,   105,    99,   100,    96,   109,   110,
     104,   112,    26,   107,   108,    26,    13,    13,    13,    73,
      37,    38,   123,    13,    13,    13,   127,    96,   129,    13,
      13,    45,    13,    33,    45,    38,    90,    51,    38,     4,
      51,    38,    38,    38,    76,    99,   100,    81,    38,    38,
      38,    -1,    17,    -1,    38,    38,    -1,    38,    -1,    73,
      -1,    26,    73,    28,    29,    30,    31,    32,    -1,    34,
      -1,    36,    -1,    38,    28,    29,    90,    42,    -1,    90,
      15,    16,    36,    -1,    -1,    99,   100,    -1,    99,   100,
       3,     4,    -1,    -1,    -1,    -1,     9,    10,    11,    -1,
      13,    14,    -1,    16,    17,    18,    -1,    -1,    21,    -1,
      -1,    -1,    25,    -1,    -1,    28,    29,    30,    31,    32,
      -1,    34,    -1,    36,    -1,     4,    39,    40,    41,    42,
       9,    10,    11,    -1,    13,    14,    -1,    16,    17,    18,
      -1,    -1,    21,    -1,    -1,    -1,    25,    -1,    -1,    28,
      29,    30,    31,    32,    -1,    34,    -1,    36,    -1,     4,
      39,    40,    41,    42,     9,    10,    11,    -1,    13,    -1,
      -1,    16,    17,    18,    -1,    -1,    21,    -1,    -1,    -1,
      25,    -1,    -1,    28,    29,    30,    31,    32,    -1,    34,
      -1,    36,    -1,     4,    39,    40,    41,    42,     9,    10,
      11,    -1,    -1,    -1,    -1,    16,    17,    18,    -1,    -1,
      21,     4,    -1,    -1,    25,    -1,    -1,    28,    29,    30,
      31,    32,    -1,    34,    17,    36,    -1,     4,    39,    40,
      41,    42,    25,    -1,    -1,    28,    29,    30,    31,    32,
      17,    34,    -1,    36,    -1,    -1,    39,    40,    41,    42,
      -1,    28,    29,    30,    31,    32,    -1,    34,    -1,    36,
      -1,    38,    -1,    -1,    -1,    42
};

/* YYSTOS[STATE-NUM] -- The (internal number of the) accessing
   symbol of state STATE-NUM.  */
static const yytype_uint8 yystos[] =
{
       0,     4,     9,    10,    11,    16,    17,    18,    21,    25,
      28,    29,    30,    31,    32,    34,    36,    39,    40,    41,
      42,    44,    45,    46,    47,    48,    49,    50,    51,    52,
      53,    56,    62,    63,    64,    65,    66,    67,    68,    69,
      44,    44,    44,    17,    46,    49,    46,    36,    46,    46,
      44,    49,    58,    59,    59,    59,     0,    38,    42,    46,
      48,    50,    51,    52,    64,    69,    13,    38,    13,    27,
      38,    12,    20,    49,    60,    61,    36,    42,    13,     5,
      33,    38,    35,    37,    45,     3,    13,    14,    54,    55,
      49,    44,    46,    57,    57,    46,     5,    37,    61,    49,
      49,    58,    44,    44,    26,    38,    13,     5,    20,    38,
      38,    60,    38,    37,    38,    38,    46,    44,    46,    57,
      44,    44,    44,    38,    13,    54,    13,    38,    13,    38,
      13,    13,    13,    44,    44,    44,    13,    13,    13
};

#define yyerrok		(yyerrstatus = 0)
#define yyclearin	(yychar = YYEMPTY)
#define YYEMPTY		(-2)
#define YYEOF		0

#define YYACCEPT	goto yyacceptlab
#define YYABORT		goto yyabortlab
#define YYERROR		goto yyerrorlab


/* Like YYERROR except do call yyerror.  This remains here temporarily
   to ease the transition to the new meaning of YYERROR, for GCC.
   Once GCC version 2 has supplanted version 1, this can go.  */

#define YYFAIL		goto yyerrlab

#define YYRECOVERING()  (!!yyerrstatus)

#define YYBACKUP(Token, Value)					\
do								\
  if (yychar == YYEMPTY && yylen == 1)				\
    {								\
      yychar = (Token);						\
      yylval = (Value);						\
      yytoken = YYTRANSLATE (yychar);				\
      YYPOPSTACK (1);						\
      goto yybackup;						\
    }								\
  else								\
    {								\
      yyerror (YY_("syntax error: cannot back up")); \
      YYERROR;							\
    }								\
while (YYID (0))


#define YYTERROR	1
#define YYERRCODE	256


/* YYLLOC_DEFAULT -- Set CURRENT to span from RHS[1] to RHS[N].
   If N is 0, then set CURRENT to the empty location which ends
   the previous symbol: RHS[0] (always defined).  */

#define YYRHSLOC(Rhs, K) ((Rhs)[K])
#ifndef YYLLOC_DEFAULT
# define YYLLOC_DEFAULT(Current, Rhs, N)				\
    do									\
      if (YYID (N))                                                    \
	{								\
	  (Current).first_line   = YYRHSLOC (Rhs, 1).first_line;	\
	  (Current).first_column = YYRHSLOC (Rhs, 1).first_column;	\
	  (Current).last_line    = YYRHSLOC (Rhs, N).last_line;		\
	  (Current).last_column  = YYRHSLOC (Rhs, N).last_column;	\
	}								\
      else								\
	{								\
	  (Current).first_line   = (Current).last_line   =		\
	    YYRHSLOC (Rhs, 0).last_line;				\
	  (Current).first_column = (Current).last_column =		\
	    YYRHSLOC (Rhs, 0).last_column;				\
	}								\
    while (YYID (0))
#endif


/* YY_LOCATION_PRINT -- Print the location on the stream.
   This macro was not mandated originally: define only if we know
   we won't break user code: when these are the locations we know.  */

#ifndef YY_LOCATION_PRINT
# if YYLTYPE_IS_TRIVIAL
#  define YY_LOCATION_PRINT(File, Loc)			\
     fprintf (File, "%d.%d-%d.%d",			\
	      (Loc).first_line, (Loc).first_column,	\
	      (Loc).last_line,  (Loc).last_column)
# else
#  define YY_LOCATION_PRINT(File, Loc) ((void) 0)
# endif
#endif


/* YYLEX -- calling `yylex' with the right arguments.  */

#ifdef YYLEX_PARAM
# define YYLEX yylex (YYLEX_PARAM)
#else
# define YYLEX yylex ()
#endif

/* Enable debugging if requested.  */
#if YYDEBUG

# ifndef YYFPRINTF
#  include <stdio.h> /* INFRINGES ON USER NAME SPACE */
#  define YYFPRINTF fprintf
# endif

# define YYDPRINTF(Args)			\
do {						\
  if (yydebug)					\
    YYFPRINTF Args;				\
} while (YYID (0))

# define YY_SYMBOL_PRINT(Title, Type, Value, Location)			  \
do {									  \
  if (yydebug)								  \
    {									  \
      YYFPRINTF (stderr, "%s ", Title);					  \
      yy_symbol_print (stderr,						  \
		  Type, Value); \
      YYFPRINTF (stderr, "\n");						  \
    }									  \
} while (YYID (0))


/*--------------------------------.
| Print this symbol on YYOUTPUT.  |
`--------------------------------*/

/*ARGSUSED*/
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yy_symbol_value_print (FILE *yyoutput, int yytype, YYSTYPE const * const yyvaluep)
#else
static void
yy_symbol_value_print (yyoutput, yytype, yyvaluep)
    FILE *yyoutput;
    int yytype;
    YYSTYPE const * const yyvaluep;
#endif
{
  if (!yyvaluep)
    return;
# ifdef YYPRINT
  if (yytype < YYNTOKENS)
    YYPRINT (yyoutput, yytoknum[yytype], *yyvaluep);
# else
  YYUSE (yyoutput);
# endif
  switch (yytype)
    {
      default:
	break;
    }
}


/*--------------------------------.
| Print this symbol on YYOUTPUT.  |
`--------------------------------*/

#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yy_symbol_print (FILE *yyoutput, int yytype, YYSTYPE const * const yyvaluep)
#else
static void
yy_symbol_print (yyoutput, yytype, yyvaluep)
    FILE *yyoutput;
    int yytype;
    YYSTYPE const * const yyvaluep;
#endif
{
  if (yytype < YYNTOKENS)
    YYFPRINTF (yyoutput, "token %s (", yytname[yytype]);
  else
    YYFPRINTF (yyoutput, "nterm %s (", yytname[yytype]);

  yy_symbol_value_print (yyoutput, yytype, yyvaluep);
  YYFPRINTF (yyoutput, ")");
}

/*------------------------------------------------------------------.
| yy_stack_print -- Print the state stack from its BOTTOM up to its |
| TOP (included).                                                   |
`------------------------------------------------------------------*/

#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yy_stack_print (yytype_int16 *yybottom, yytype_int16 *yytop)
#else
static void
yy_stack_print (yybottom, yytop)
    yytype_int16 *yybottom;
    yytype_int16 *yytop;
#endif
{
  YYFPRINTF (stderr, "Stack now");
  for (; yybottom <= yytop; yybottom++)
    {
      int yybot = *yybottom;
      YYFPRINTF (stderr, " %d", yybot);
    }
  YYFPRINTF (stderr, "\n");
}

# define YY_STACK_PRINT(Bottom, Top)				\
do {								\
  if (yydebug)							\
    yy_stack_print ((Bottom), (Top));				\
} while (YYID (0))


/*------------------------------------------------.
| Report that the YYRULE is going to be reduced.  |
`------------------------------------------------*/

#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yy_reduce_print (YYSTYPE *yyvsp, int yyrule)
#else
static void
yy_reduce_print (yyvsp, yyrule)
    YYSTYPE *yyvsp;
    int yyrule;
#endif
{
  int yynrhs = yyr2[yyrule];
  int yyi;
  unsigned long int yylno = yyrline[yyrule];
  YYFPRINTF (stderr, "Reducing stack by rule %d (line %lu):\n",
	     yyrule - 1, yylno);
  /* The symbols being reduced.  */
  for (yyi = 0; yyi < yynrhs; yyi++)
    {
      YYFPRINTF (stderr, "   $%d = ", yyi + 1);
      yy_symbol_print (stderr, yyrhs[yyprhs[yyrule] + yyi],
		       &(yyvsp[(yyi + 1) - (yynrhs)])
		       		       );
      YYFPRINTF (stderr, "\n");
    }
}

# define YY_REDUCE_PRINT(Rule)		\
do {					\
  if (yydebug)				\
    yy_reduce_print (yyvsp, Rule); \
} while (YYID (0))

/* Nonzero means print parse trace.  It is left uninitialized so that
   multiple parsers can coexist.  */
int yydebug;
#else /* !YYDEBUG */
# define YYDPRINTF(Args)
# define YY_SYMBOL_PRINT(Title, Type, Value, Location)
# define YY_STACK_PRINT(Bottom, Top)
# define YY_REDUCE_PRINT(Rule)
#endif /* !YYDEBUG */


/* YYINITDEPTH -- initial size of the parser's stacks.  */
#ifndef	YYINITDEPTH
# define YYINITDEPTH 200
#endif

/* YYMAXDEPTH -- maximum size the stacks can grow to (effective only
   if the built-in stack extension method is used).

   Do not make this value too large; the results are undefined if
   YYSTACK_ALLOC_MAXIMUM < YYSTACK_BYTES (YYMAXDEPTH)
   evaluated with infinite-precision integer arithmetic.  */

#ifndef YYMAXDEPTH
# define YYMAXDEPTH 10000
#endif



#if YYERROR_VERBOSE

# ifndef yystrlen
#  if defined __GLIBC__ && defined _STRING_H
#   define yystrlen strlen
#  else
/* Return the length of YYSTR.  */
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static YYSIZE_T
yystrlen (const char *yystr)
#else
static YYSIZE_T
yystrlen (yystr)
    const char *yystr;
#endif
{
  YYSIZE_T yylen;
  for (yylen = 0; yystr[yylen]; yylen++)
    continue;
  return yylen;
}
#  endif
# endif

# ifndef yystpcpy
#  if defined __GLIBC__ && defined _STRING_H && defined _GNU_SOURCE
#   define yystpcpy stpcpy
#  else
/* Copy YYSRC to YYDEST, returning the address of the terminating '\0' in
   YYDEST.  */
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static char *
yystpcpy (char *yydest, const char *yysrc)
#else
static char *
yystpcpy (yydest, yysrc)
    char *yydest;
    const char *yysrc;
#endif
{
  char *yyd = yydest;
  const char *yys = yysrc;

  while ((*yyd++ = *yys++) != '\0')
    continue;

  return yyd - 1;
}
#  endif
# endif

# ifndef yytnamerr
/* Copy to YYRES the contents of YYSTR after stripping away unnecessary
   quotes and backslashes, so that it's suitable for yyerror.  The
   heuristic is that double-quoting is unnecessary unless the string
   contains an apostrophe, a comma, or backslash (other than
   backslash-backslash).  YYSTR is taken from yytname.  If YYRES is
   null, do not copy; instead, return the length of what the result
   would have been.  */
static YYSIZE_T
yytnamerr (char *yyres, const char *yystr)
{
  if (*yystr == '"')
    {
      YYSIZE_T yyn = 0;
      char const *yyp = yystr;

      for (;;)
	switch (*++yyp)
	  {
	  case '\'':
	  case ',':
	    goto do_not_strip_quotes;

	  case '\\':
	    if (*++yyp != '\\')
	      goto do_not_strip_quotes;
	    /* Fall through.  */
	  default:
	    if (yyres)
	      yyres[yyn] = *yyp;
	    yyn++;
	    break;

	  case '"':
	    if (yyres)
	      yyres[yyn] = '\0';
	    return yyn;
	  }
    do_not_strip_quotes: ;
    }

  if (! yyres)
    return yystrlen (yystr);

  return yystpcpy (yyres, yystr) - yyres;
}
# endif

/* Copy into YYRESULT an error message about the unexpected token
   YYCHAR while in state YYSTATE.  Return the number of bytes copied,
   including the terminating null byte.  If YYRESULT is null, do not
   copy anything; just return the number of bytes that would be
   copied.  As a special case, return 0 if an ordinary "syntax error"
   message will do.  Return YYSIZE_MAXIMUM if overflow occurs during
   size calculation.  */
static YYSIZE_T
yysyntax_error (char *yyresult, int yystate, int yychar)
{
  int yyn = yypact[yystate];

  if (! (YYPACT_NINF < yyn && yyn <= YYLAST))
    return 0;
  else
    {
      int yytype = YYTRANSLATE (yychar);
      YYSIZE_T yysize0 = yytnamerr (0, yytname[yytype]);
      YYSIZE_T yysize = yysize0;
      YYSIZE_T yysize1;
      int yysize_overflow = 0;
      enum { YYERROR_VERBOSE_ARGS_MAXIMUM = 5 };
      char const *yyarg[YYERROR_VERBOSE_ARGS_MAXIMUM];
      int yyx;

# if 0
      /* This is so xgettext sees the translatable formats that are
	 constructed on the fly.  */
      YY_("syntax error, unexpected %s");
      YY_("syntax error, unexpected %s, expecting %s");
      YY_("syntax error, unexpected %s, expecting %s or %s");
      YY_("syntax error, unexpected %s, expecting %s or %s or %s");
      YY_("syntax error, unexpected %s, expecting %s or %s or %s or %s");
# endif
      char *yyfmt;
      char const *yyf;
      static char const yyunexpected[] = "syntax error, unexpected %s";
      static char const yyexpecting[] = ", expecting %s";
      static char const yyor[] = " or %s";
      char yyformat[sizeof yyunexpected
		    + sizeof yyexpecting - 1
		    + ((YYERROR_VERBOSE_ARGS_MAXIMUM - 2)
		       * (sizeof yyor - 1))];
      char const *yyprefix = yyexpecting;

      /* Start YYX at -YYN if negative to avoid negative indexes in
	 YYCHECK.  */
      int yyxbegin = yyn < 0 ? -yyn : 0;

      /* Stay within bounds of both yycheck and yytname.  */
      int yychecklim = YYLAST - yyn + 1;
      int yyxend = yychecklim < YYNTOKENS ? yychecklim : YYNTOKENS;
      int yycount = 1;

      yyarg[0] = yytname[yytype];
      yyfmt = yystpcpy (yyformat, yyunexpected);

      for (yyx = yyxbegin; yyx < yyxend; ++yyx)
	if (yycheck[yyx + yyn] == yyx && yyx != YYTERROR)
	  {
	    if (yycount == YYERROR_VERBOSE_ARGS_MAXIMUM)
	      {
		yycount = 1;
		yysize = yysize0;
		yyformat[sizeof yyunexpected - 1] = '\0';
		break;
	      }
	    yyarg[yycount++] = yytname[yyx];
	    yysize1 = yysize + yytnamerr (0, yytname[yyx]);
	    yysize_overflow |= (yysize1 < yysize);
	    yysize = yysize1;
	    yyfmt = yystpcpy (yyfmt, yyprefix);
	    yyprefix = yyor;
	  }

      yyf = YY_(yyformat);
      yysize1 = yysize + yystrlen (yyf);
      yysize_overflow |= (yysize1 < yysize);
      yysize = yysize1;

      if (yysize_overflow)
	return YYSIZE_MAXIMUM;

      if (yyresult)
	{
	  /* Avoid sprintf, as that infringes on the user's name space.
	     Don't have undefined behavior even if the translation
	     produced a string with the wrong number of "%s"s.  */
	  char *yyp = yyresult;
	  int yyi = 0;
	  while ((*yyp = *yyf) != '\0')
	    {
	      if (*yyp == '%' && yyf[1] == 's' && yyi < yycount)
		{
		  yyp += yytnamerr (yyp, yyarg[yyi++]);
		  yyf += 2;
		}
	      else
		{
		  yyp++;
		  yyf++;
		}
	    }
	}
      return yysize;
    }
}
#endif /* YYERROR_VERBOSE */


/*-----------------------------------------------.
| Release the memory associated to this symbol.  |
`-----------------------------------------------*/

/*ARGSUSED*/
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
static void
yydestruct (const char *yymsg, int yytype, YYSTYPE *yyvaluep)
#else
static void
yydestruct (yymsg, yytype, yyvaluep)
    const char *yymsg;
    int yytype;
    YYSTYPE *yyvaluep;
#endif
{
  YYUSE (yyvaluep);

  if (!yymsg)
    yymsg = "Deleting";
  YY_SYMBOL_PRINT (yymsg, yytype, yyvaluep, yylocationp);

  switch (yytype)
    {

      default:
	break;
    }
}

/* Prevent warnings from -Wmissing-prototypes.  */
#ifdef YYPARSE_PARAM
#if defined __STDC__ || defined __cplusplus
int yyparse (void *YYPARSE_PARAM);
#else
int yyparse ();
#endif
#else /* ! YYPARSE_PARAM */
#if defined __STDC__ || defined __cplusplus
int yyparse (void);
#else
int yyparse ();
#endif
#endif /* ! YYPARSE_PARAM */


/* The lookahead symbol.  */
int yychar;

/* The semantic value of the lookahead symbol.  */
YYSTYPE yylval;

/* Number of syntax errors so far.  */
int yynerrs;



/*-------------------------.
| yyparse or yypush_parse.  |
`-------------------------*/

#ifdef YYPARSE_PARAM
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
int
yyparse (void *YYPARSE_PARAM)
#else
int
yyparse (YYPARSE_PARAM)
    void *YYPARSE_PARAM;
#endif
#else /* ! YYPARSE_PARAM */
#if (defined __STDC__ || defined __C99__FUNC__ \
     || defined __cplusplus || defined _MSC_VER)
int
yyparse (void)
#else
int
yyparse ()

#endif
#endif
{


    int yystate;
    /* Number of tokens to shift before error messages enabled.  */
    int yyerrstatus;

    /* The stacks and their tools:
       `yyss': related to states.
       `yyvs': related to semantic values.

       Refer to the stacks thru separate pointers, to allow yyoverflow
       to reallocate them elsewhere.  */

    /* The state stack.  */
    yytype_int16 yyssa[YYINITDEPTH];
    yytype_int16 *yyss;
    yytype_int16 *yyssp;

    /* The semantic value stack.  */
    YYSTYPE yyvsa[YYINITDEPTH];
    YYSTYPE *yyvs;
    YYSTYPE *yyvsp;

    YYSIZE_T yystacksize;

  int yyn;
  int yyresult;
  /* Lookahead token as an internal (translated) token number.  */
  int yytoken;
  /* The variables used to return semantic value and location from the
     action routines.  */
  YYSTYPE yyval;

#if YYERROR_VERBOSE
  /* Buffer for error messages, and its allocated size.  */
  char yymsgbuf[128];
  char *yymsg = yymsgbuf;
  YYSIZE_T yymsg_alloc = sizeof yymsgbuf;
#endif

#define YYPOPSTACK(N)   (yyvsp -= (N), yyssp -= (N))

  /* The number of symbols on the RHS of the reduced rule.
     Keep to zero when no symbol should be popped.  */
  int yylen = 0;

  yytoken = 0;
  yyss = yyssa;
  yyvs = yyvsa;
  yystacksize = YYINITDEPTH;

  YYDPRINTF ((stderr, "Starting parse\n"));

  yystate = 0;
  yyerrstatus = 0;
  yynerrs = 0;
  yychar = YYEMPTY; /* Cause a token to be read.  */

  /* Initialize stack pointers.
     Waste one element of value and location stack
     so that they stay on the same level as the state stack.
     The wasted elements are never initialized.  */
  yyssp = yyss;
  yyvsp = yyvs;

  goto yysetstate;

/*------------------------------------------------------------.
| yynewstate -- Push a new state, which is found in yystate.  |
`------------------------------------------------------------*/
 yynewstate:
  /* In all cases, when you get here, the value and location stacks
     have just been pushed.  So pushing a state here evens the stacks.  */
  yyssp++;

 yysetstate:
  *yyssp = yystate;

  if (yyss + yystacksize - 1 <= yyssp)
    {
      /* Get the current used size of the three stacks, in elements.  */
      YYSIZE_T yysize = yyssp - yyss + 1;

#ifdef yyoverflow
      {
	/* Give user a chance to reallocate the stack.  Use copies of
	   these so that the &'s don't force the real ones into
	   memory.  */
	YYSTYPE *yyvs1 = yyvs;
	yytype_int16 *yyss1 = yyss;

	/* Each stack pointer address is followed by the size of the
	   data in use in that stack, in bytes.  This used to be a
	   conditional around just the two extra args, but that might
	   be undefined if yyoverflow is a macro.  */
	yyoverflow (YY_("memory exhausted"),
		    &yyss1, yysize * sizeof (*yyssp),
		    &yyvs1, yysize * sizeof (*yyvsp),
		    &yystacksize);

	yyss = yyss1;
	yyvs = yyvs1;
      }
#else /* no yyoverflow */
# ifndef YYSTACK_RELOCATE
      goto yyexhaustedlab;
# else
      /* Extend the stack our own way.  */
      if (YYMAXDEPTH <= yystacksize)
	goto yyexhaustedlab;
      yystacksize *= 2;
      if (YYMAXDEPTH < yystacksize)
	yystacksize = YYMAXDEPTH;

      {
	yytype_int16 *yyss1 = yyss;
	union yyalloc *yyptr =
	  (union yyalloc *) YYSTACK_ALLOC (YYSTACK_BYTES (yystacksize));
	if (! yyptr)
	  goto yyexhaustedlab;
	YYSTACK_RELOCATE (yyss_alloc, yyss);
	YYSTACK_RELOCATE (yyvs_alloc, yyvs);
#  undef YYSTACK_RELOCATE
	if (yyss1 != yyssa)
	  YYSTACK_FREE (yyss1);
      }
# endif
#endif /* no yyoverflow */

      yyssp = yyss + yysize - 1;
      yyvsp = yyvs + yysize - 1;

      YYDPRINTF ((stderr, "Stack size increased to %lu\n",
		  (unsigned long int) yystacksize));

      if (yyss + yystacksize - 1 <= yyssp)
	YYABORT;
    }

  YYDPRINTF ((stderr, "Entering state %d\n", yystate));

  if (yystate == YYFINAL)
    YYACCEPT;

  goto yybackup;

/*-----------.
| yybackup.  |
`-----------*/
yybackup:

  /* Do appropriate processing given the current state.  Read a
     lookahead token if we need one and don't already have one.  */

  /* First try to decide what to do without reference to lookahead token.  */
  yyn = yypact[yystate];
  if (yyn == YYPACT_NINF)
    goto yydefault;

  /* Not known => get a lookahead token if don't already have one.  */

  /* YYCHAR is either YYEMPTY or YYEOF or a valid lookahead symbol.  */
  if (yychar == YYEMPTY)
    {
      YYDPRINTF ((stderr, "Reading a token: "));
      yychar = YYLEX;
    }

  if (yychar <= YYEOF)
    {
      yychar = yytoken = YYEOF;
      YYDPRINTF ((stderr, "Now at end of input.\n"));
    }
  else
    {
      yytoken = YYTRANSLATE (yychar);
      YY_SYMBOL_PRINT ("Next token is", yytoken, &yylval, &yylloc);
    }

  /* If the proper action on seeing token YYTOKEN is to reduce or to
     detect an error, take that action.  */
  yyn += yytoken;
  if (yyn < 0 || YYLAST < yyn || yycheck[yyn] != yytoken)
    goto yydefault;
  yyn = yytable[yyn];
  if (yyn <= 0)
    {
      if (yyn == 0 || yyn == YYTABLE_NINF)
	goto yyerrlab;
      yyn = -yyn;
      goto yyreduce;
    }

  /* Count tokens shifted since error; after three, turn off error
     status.  */
  if (yyerrstatus)
    yyerrstatus--;

  /* Shift the lookahead token.  */
  YY_SYMBOL_PRINT ("Shifting", yytoken, &yylval, &yylloc);

  /* Discard the shifted token.  */
  yychar = YYEMPTY;

  yystate = yyn;
  *++yyvsp = yylval;

  goto yynewstate;


/*-----------------------------------------------------------.
| yydefault -- do the default action for the current state.  |
`-----------------------------------------------------------*/
yydefault:
  yyn = yydefact[yystate];
  if (yyn == 0)
    goto yyerrlab;
  goto yyreduce;


/*-----------------------------.
| yyreduce -- Do a reduction.  |
`-----------------------------*/
yyreduce:
  /* yyn is the number of a rule to reduce with.  */
  yylen = yyr2[yyn];

  /* If YYLEN is nonzero, implement the default value of the action:
     `$$ = $1'.

     Otherwise, the following line sets YYVAL to garbage.
     This behavior is undocumented and Bison
     users should not rely upon it.  Assigning to YYVAL
     unconditionally makes the parser a bit smaller, and it avoids a
     GCC warning that YYVAL may be used uninitialized.  */
  yyval = yyvsp[1-yylen];


  YY_REDUCE_PRINT (yyn);
  switch (yyn)
    {
        case 2:

/* Line 1455 of yacc.c  */
#line 35 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    {

syntaxTree* tree = createSyntaxTree(general);
appendChild((yyvsp[(1) - (1)].tree), tree);
(yyval.tree) = tree;

;}
    break;

  case 3:

/* Line 1455 of yacc.c  */
#line 42 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    {

appendChild((yyvsp[(3) - (3)].tree), (yyvsp[(1) - (3)].tree));
(yyval.tree) = (yyvsp[(1) - (3)].tree);

;}
    break;

  case 4:

/* Line 1455 of yacc.c  */
#line 50 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyval.tree) = NULL; ;}
    break;

  case 5:

/* Line 1455 of yacc.c  */
#line 51 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyval.tree) = (yyvsp[(1) - (1)].tree); ;}
    break;

  case 6:

/* Line 1455 of yacc.c  */
#line 52 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyval.tree) = (yyvsp[(1) - (1)].tree); ;}
    break;

  case 7:

/* Line 1455 of yacc.c  */
#line 55 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyval.tree) = createLiteral((yyvsp[(1) - (1)].string)); ;}
    break;

  case 8:

/* Line 1455 of yacc.c  */
#line 56 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyval.tree) = createLiteral((yyvsp[(1) - (1)].string)); ;}
    break;

  case 9:

/* Line 1455 of yacc.c  */
#line 59 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyvsp[(1) - (1)].tree)->type = ifblock; (yyval.tree) = (yyvsp[(1) - (1)].tree); ;}
    break;

  case 10:

/* Line 1455 of yacc.c  */
#line 60 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyval.tree) = (yyvsp[(1) - (1)].tree); ;}
    break;

  case 11:

/* Line 1455 of yacc.c  */
#line 61 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyvsp[(1) - (1)].tree)->type = conditionalblock; (yyval.tree) = (yyvsp[(1) - (1)].tree); ;}
    break;

  case 12:

/* Line 1455 of yacc.c  */
#line 62 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyvsp[(1) - (1)].tree)->type = whileblock; (yyval.tree) = (yyvsp[(1) - (1)].tree); ;}
    break;

  case 13:

/* Line 1455 of yacc.c  */
#line 63 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyvsp[(1) - (1)].tree)->type = iterateblock; (yyval.tree) = (yyvsp[(1) - (1)].tree); ;}
    break;

  case 14:

/* Line 1455 of yacc.c  */
#line 66 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyval.tree) = createLiteral((yyvsp[(1) - (1)].string)); ;}
    break;

  case 15:

/* Line 1455 of yacc.c  */
#line 66 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { (yyval.tree) = createLiteral((yyvsp[(1) - (1)].string)); ;}
    break;

  case 16:

/* Line 1455 of yacc.c  */
#line 68 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    { 

syntaxTree* tree = createSyntaxTree(evaluation);
appendChild((yyvsp[(1) - (1)].tree), tree);
(yyval.tree) = tree;

;}
    break;

  case 20:

/* Line 1455 of yacc.c  */
#line 78 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    {

syntaxTree * tree = createSyntaxTree(evaluation);
appendChild((yyvsp[(1) - (1)].tree), tree);
(yyval.tree) = tree;

;}
    break;

  case 21:

/* Line 1455 of yacc.c  */
#line 85 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    {

syntaxTree * tree = createSyntaxTree(evaluation);
appendChild((yyvsp[(1) - (1)].tree), tree);
(yyval.tree) = tree;

;}
    break;

  case 28:

/* Line 1455 of yacc.c  */
#line 98 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    {

appendChild((yyvsp[(2) - (2)].tree), (yyvsp[(1) - (2)].tree));
(yyval.tree) = (yyvsp[(1) - (2)].tree);

;}
    break;

  case 31:

/* Line 1455 of yacc.c  */
#line 106 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    {

appendChild((yyvsp[(2) - (2)].tree), (yyvsp[(1) - (2)].tree));
(yyval.tree) = (yyvsp[(1) - (2)].tree);

;}
    break;

  case 33:

/* Line 1455 of yacc.c  */
#line 113 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    {

appendChild((yyvsp[(2) - (2)].tree), (yyvsp[(1) - (2)].tree));
(yyval.tree) = (yyvsp[(1) - (2)].tree);

;}
    break;

  case 70:

/* Line 1455 of yacc.c  */
#line 186 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"
    {
    syntaxTree * tree = (yyvsp[(2) - (3)].tree);
    tree->type = contentblock;
    (yyval.tree) = tree;
;}
    break;



/* Line 1455 of yacc.c  */
#line 1746 "syntax.tab.c"
      default: break;
    }
  YY_SYMBOL_PRINT ("-> $$ =", yyr1[yyn], &yyval, &yyloc);

  YYPOPSTACK (yylen);
  yylen = 0;
  YY_STACK_PRINT (yyss, yyssp);

  *++yyvsp = yyval;

  /* Now `shift' the result of the reduction.  Determine what state
     that goes to, based on the state we popped back to and the rule
     number reduced by.  */

  yyn = yyr1[yyn];

  yystate = yypgoto[yyn - YYNTOKENS] + *yyssp;
  if (0 <= yystate && yystate <= YYLAST && yycheck[yystate] == *yyssp)
    yystate = yytable[yystate];
  else
    yystate = yydefgoto[yyn - YYNTOKENS];

  goto yynewstate;


/*------------------------------------.
| yyerrlab -- here on detecting error |
`------------------------------------*/
yyerrlab:
  /* If not already recovering from an error, report this error.  */
  if (!yyerrstatus)
    {
      ++yynerrs;
#if ! YYERROR_VERBOSE
      yyerror (YY_("syntax error"));
#else
      {
	YYSIZE_T yysize = yysyntax_error (0, yystate, yychar);
	if (yymsg_alloc < yysize && yymsg_alloc < YYSTACK_ALLOC_MAXIMUM)
	  {
	    YYSIZE_T yyalloc = 2 * yysize;
	    if (! (yysize <= yyalloc && yyalloc <= YYSTACK_ALLOC_MAXIMUM))
	      yyalloc = YYSTACK_ALLOC_MAXIMUM;
	    if (yymsg != yymsgbuf)
	      YYSTACK_FREE (yymsg);
	    yymsg = (char *) YYSTACK_ALLOC (yyalloc);
	    if (yymsg)
	      yymsg_alloc = yyalloc;
	    else
	      {
		yymsg = yymsgbuf;
		yymsg_alloc = sizeof yymsgbuf;
	      }
	  }

	if (0 < yysize && yysize <= yymsg_alloc)
	  {
	    (void) yysyntax_error (yymsg, yystate, yychar);
	    yyerror (yymsg);
	  }
	else
	  {
	    yyerror (YY_("syntax error"));
	    if (yysize != 0)
	      goto yyexhaustedlab;
	  }
      }
#endif
    }



  if (yyerrstatus == 3)
    {
      /* If just tried and failed to reuse lookahead token after an
	 error, discard it.  */

      if (yychar <= YYEOF)
	{
	  /* Return failure if at end of input.  */
	  if (yychar == YYEOF)
	    YYABORT;
	}
      else
	{
	  yydestruct ("Error: discarding",
		      yytoken, &yylval);
	  yychar = YYEMPTY;
	}
    }

  /* Else will try to reuse lookahead token after shifting the error
     token.  */
  goto yyerrlab1;


/*---------------------------------------------------.
| yyerrorlab -- error raised explicitly by YYERROR.  |
`---------------------------------------------------*/
yyerrorlab:

  /* Pacify compilers like GCC when the user code never invokes
     YYERROR and the label yyerrorlab therefore never appears in user
     code.  */
  if (/*CONSTCOND*/ 0)
     goto yyerrorlab;

  /* Do not reclaim the symbols of the rule which action triggered
     this YYERROR.  */
  YYPOPSTACK (yylen);
  yylen = 0;
  YY_STACK_PRINT (yyss, yyssp);
  yystate = *yyssp;
  goto yyerrlab1;


/*-------------------------------------------------------------.
| yyerrlab1 -- common code for both syntax error and YYERROR.  |
`-------------------------------------------------------------*/
yyerrlab1:
  yyerrstatus = 3;	/* Each real token shifted decrements this.  */

  for (;;)
    {
      yyn = yypact[yystate];
      if (yyn != YYPACT_NINF)
	{
	  yyn += YYTERROR;
	  if (0 <= yyn && yyn <= YYLAST && yycheck[yyn] == YYTERROR)
	    {
	      yyn = yytable[yyn];
	      if (0 < yyn)
		break;
	    }
	}

      /* Pop the current state because it cannot handle the error token.  */
      if (yyssp == yyss)
	YYABORT;


      yydestruct ("Error: popping",
		  yystos[yystate], yyvsp);
      YYPOPSTACK (1);
      yystate = *yyssp;
      YY_STACK_PRINT (yyss, yyssp);
    }

  *++yyvsp = yylval;


  /* Shift the error token.  */
  YY_SYMBOL_PRINT ("Shifting", yystos[yyn], yyvsp, yylsp);

  yystate = yyn;
  goto yynewstate;


/*-------------------------------------.
| yyacceptlab -- YYACCEPT comes here.  |
`-------------------------------------*/
yyacceptlab:
  yyresult = 0;
  goto yyreturn;

/*-----------------------------------.
| yyabortlab -- YYABORT comes here.  |
`-----------------------------------*/
yyabortlab:
  yyresult = 1;
  goto yyreturn;

#if !defined(yyoverflow) || YYERROR_VERBOSE
/*-------------------------------------------------.
| yyexhaustedlab -- memory exhaustion comes here.  |
`-------------------------------------------------*/
yyexhaustedlab:
  yyerror (YY_("memory exhausted"));
  yyresult = 2;
  /* Fall through.  */
#endif

yyreturn:
  if (yychar != YYEMPTY)
     yydestruct ("Cleanup: discarding lookahead",
		 yytoken, &yylval);
  /* Do not reclaim the symbols of the rule which action triggered
     this YYABORT or YYACCEPT.  */
  YYPOPSTACK (yylen);
  YY_STACK_PRINT (yyss, yyssp);
  while (yyssp != yyss)
    {
      yydestruct ("Cleanup: popping",
		  yystos[*yyssp], yyvsp);
      YYPOPSTACK (1);
    }
#ifndef yyoverflow
  if (yyss != yyssa)
    YYSTACK_FREE (yyss);
#endif
#if YYERROR_VERBOSE
  if (yymsg != yymsgbuf)
    YYSTACK_FREE (yymsg);
#endif
  /* Make sure YYID is used.  */
  return YYID (yyresult);
}



/* Line 1675 of yacc.c  */
#line 192 "D:\\projects\\xornent\\simula\\Simula.Scripting.CLI\\syntax.y"

