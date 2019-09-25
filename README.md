# Calcex

[![Build status](https://ci.appveyor.com/api/projects/status/to343qmkhi2v5lae?svg=true)](https://ci.appveyor.com/project/alxnull/calcex)
[![](https://img.shields.io/appveyor/tests/alxnull/calcex.svg)](https://ci.appveyor.com/project/alxnull/calcex)
[![](https://img.shields.io/github/license/alxnull/calcex.svg)](https://github.com/alxnull/calcex/blob/master/LICENSE.txt)
[![](https://img.shields.io/nuget/v/calcex.core.svg)](https://www.nuget.org/packages/calcex.core/)

A simple mathematical expression parser and evaluator for .NET.
This repository contains _calcex.core_, the actual .NET parser library, the _calcex_ .NET Core CLI and a sample Windows GUI.

## Overview

**Calcex.Core** is a basic parser and evaluator library for mathematical expressions built on .NET Core.
Some functionalities are:

- support for many common arithmetic and bitwise operators (*, ^, %, &, ...)

- support for many mathematical functions (sin, ln, log, min, ...)

- user-defined variables and functions

- evaluate to double, decimal or boolean values

- compile expressions into callable delegates

- convert expressions to postfix notation and MathML strings

**Calcex.Console** contains a simple command-line interface that provides easy access to the mathematical evaluators.

**Calcex.Windows** contains _Calcex App_, a basic GUI calculator app for Windows built using WPF.

## How To (Calcex.Core)

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
// Compile to delegate
Func<double, double> func = tree.Compile("x");
```

Fore more, visit the [Calcex wiki](https://github.com/alxnull/calcex/wiki).

## License

Calcex is published under [BSD-3-Clause license](LICENSE.txt).
