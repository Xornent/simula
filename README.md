# simula
attempts to create an easy-to-read language in data processing and mathematic graphics.

## currently working on ...
* `improve the speed of scripting interpreter.`
* `implement a ss-to-csharp language translator, and using the generated c-sharp code to implement compiler service.`
* `build a c-sharp base calling alternative base librariy.`

## version history
### 0.7.14
1. `a backup for the c-sharp code translator and compiler service`
2. `test that the performance of loops and binary operations waste much in self-operation. however, true calculations take about 10% of total time use.`

### 0.7.13
1. `turn off the intellisense by default, and only display when user press ctrl+.`
2. `use statements.`
3. `matrices`
4. `iterator structure and iterate with integral constant.`

this version are fully compatible with 0.6 branches, emit branch will soom be merged.

### 0.7.12
1. `fix repetitive token when using module and use statements.`
2. `intellisense will not come out when typing a string's content.`
3. `a trial of address.`
4. `fix annonymous objects who don't have fullName collections. they are ignored in name-formatting and returns an annonymous set.`
5. `a trial in global localization resources.`

### 0.7.11
1. `remove built-in type 'selector' and 'array'`
2. `rewrite the matrix (esp. numerical matrix) implementation.`
3. `add a full set of integral value types, uint8(byte), uint16(char, ushort), uint32(uint), uint64(ulong), int8(sbyte), int16(short), int32(int), int64(long).`
4. `changes typename 'float' to 'double'`
5. `fix issue a literal can be named with digits that are not placed at the start.`
6. `fix issue a data member of a class can be called within it without adding the 'this.' prefix.`
7. `fix incorrect matrix behavior`

### 0.7.10
graphic user interface update
1. `the breacrumb bar navigator`
2. `side bar with xcode-like arrangements`
3. `cancelling top-level menus`
4. `non-model dialogs`

language runtime and features
1. `cancel integer type.`
2. `cancel the array type (but many of its applicance has not been replaced completely`
3. `intellisence now reads the comment directly above the declaration as its documentation.`
4. `line height edible in scales with the code editor`
5. `matrices. and object handles`
6. `indexing operation using brackets.`
7. `declaring 2-dimensional matrices using linebreaks in a closed pair of braces as well as semicolons.`
8. `vector assignment syntax.`

iteration and other matrix manipulations will be added in the following commits.

### 0.7.9
1. *`intellisense and type inference`*
2. `issue comments will trigger fatal exceptions`

### 0.7.8
1. `integrating mathnet`

### 0.7.7
feature update
1. `new load external types, functions and variables (defined in csharp code) to dynamic runtime`
2. `fix class initialization '_init' caller behavior`
3. `fix comments are now ignored rather than block execution.`
4. `new escape character issue found. ignore escape if strings are started with '$'`
5. `fix evaluation cache object type cast error`
6. `change default fullname into empty set (or else this may cause reference error). and fix assignment error if fullname is empty`
7. `change 'enum' keyword in control flow is changed to 'iter' for disambiguation`
8. `default variable type of digits is set to 'sys.float' (while you can use '000.' to manually set biginteger value type, error exists)`
9. `new tests of assembly loader, 'util' and 'math' packages.`
```
util.tex(string expr)
util.random(float min, float max)
util.randomSequence(float min, float max, float length)
util.randomVectorSequence(float min, float max, float length, float dimension)
and its floating-point versions.
class util.grid
```
10. `new math.net packages integrated`
11. `new unary operators, increment, decrement, negative,`
12. `new class inheritage and function overriding in correct arrangement.`

this version is rather close to the 0.6.4 update before engine rewrite. except for a fully-supported array operations, and iterate blocks.

### 0.7.6
function declared in global scope
1. `functions in global scope is registered as user-defined variables`
2. `solve naming problem of declaring variables in function scope. (and referencing to them)`
3. `note that exceptions exist when making a reference to an object that exist only in functions after the function ends. (dangling reference)`

### 0.7.5
1. `changes to the reference implementation.`
2. `add customize functions (bugs exist)`
3. `in later versions the reference implementation will change greatly to lazy-load pointers. this is an archieve.`

### 0.7.4
1. `pointer architecture to reference`

### 0.7.1 - 0.7.3
a major rewrite using dynamic language runtime due to low efficiency and speed of the last pointer-driven static language version [0.6.x].

1. `major engine rewrite.`
2. `improve speed of the language more than 1000 times (previous 11.900s per 1000 loops, now 8.900s per 100 000 loops)`
3. `expando objects and extensible language runtime.`
4. `code less and easier to read.`

this version is a incomplete version, it does not implement the scopes, variable references, and user class definitions prior than 0.6.0 version

### 0.5.x - 0.6.x
1. `refactor class inheritage and system architecture`
2. `separate reflection language runtime with built-in modules, using Member-ClrMember inheritage and separated implementation`
3. `pseudopointer and detailed memory management, by-reference value transmission, and real constants`
4. `extensibility support and less type casts, easier ways to initialize clr-based classes, functions and objects manually`
5. `improved reusability and less repetitions`
6. `program control structures and control token in oop mode`
7. `systematic module control system`
8. `transmeable scopes`

however, the 0.6.x architecture has been fully abandoned for the performance of the language.

### 0.4.x
1. `a basic trial implementation of scripting, without much systematic work`

from the version 0.4.0, the project is published on github.

### 0.3.x
1. `packaging and nuget package source support`

### 0.1.x - 0.2.x
1. `user interface design`