# Calcex

Calcex is a simple mathematical expression parser and evaluator for .NET.
This repository contains _calcex.core_, the actual .NET parser library,
and the _calcex_ command-line app.

## Overview

_calcex.core_ is a basic parser and evaluator for mathematical expressions built on .NET Core.
Some functionalities are:

- support for many common arithmetic and bitwise operators (*, ^, %, &, ...)

- support for many mathematical functions (sin, ln, log, min, ...)

- user-defined variables and functions

- evaluate to double, decimal, boolean and postfix expressions

## How To

### Simple example

```csharp
Parser parser = new Parser();
// Set a custom variable
parser.SetVariable("x", -12);
// Parse
var tree = parser.Parse("2*pi+5-x");
// Evaluate to double
double doubleResult = tree.Evaluate();
// Evaluate to decimal
decimal decimalResult = tree.EvaluateDecimal(); 
```

## License

Calcex is published under [BSD-3-Clause license](LICENSE.txt) by Bluegrams.
