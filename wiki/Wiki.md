# SimpleArgParse Wiki

Welcome to SAP Wiki! Navigate through the sections to learn how to use SAP.

- [Quick Start](#Quick-Start)
    - [Positional Arguments](#Positional-Arguments)
    - [Optional Arguments](#Optional-Arguments)
- [ArgumentParser](#ArgumentParser)
    - [Constructor](#Constructor)
        - [prog](#prog)
        - [usage](#usage)
        - [description](#description)
        - [epilog](#epilog)
    - [AddArgument](#AddArgument)
        - [name](#name)
        - [longName](#longName)
        - [action](#action)
        - [defaultValue](#defaultValue)
        - [choices](#choices)
        - [required](#required)
        - [help](#help)
        - [constant](#constant)
    - [AddSubparsers](#AddSubparsers)
        - [help](#help)
        - [title](#title)
        - [dest](#dest)
    - [ParseArgs](#ParseArgs)
        - [args](#args)
- [ArgumentAction](#ArgumentAction)
    - [Store](#Store)
    - [StoreConst](#StoreConst)
    - [StoreTrue](#StoreTrue)
    - [StoreFalse](#StoreFalse)
    - [Append](#Append)
    - [AppendConst](#AppendConst)
    - [Help](#Help)


# Quick Start

## Positional Arguments
You can use positional arguments if an argument is required and has to be provided in a certain position.

If you pass in a name for the argument that does not start with the prefix "-", it will be parsed as a positional argument.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("name");
            parser.AddArgument("surname");
            var parsedArgs = parser.ParseArgs(args);
            Console.WriteLine(parsedArgs);
        }
    }
}
```
```
> wiki.exe -h
usage: wiki [-h] name surname

Optional arguments:
  -h, --help            Show this message.

Positional arguments:
  name
  surname
```
```
> wiki.exe arda özcan
Namespace([name, arda], [surname, özcan])
```
Positional arguments are mandatory.
```
> wiki.exe
usage: wiki [-h] name
wiki : error: the following arguments are required: name
```
You can access positional variables just like a dictionary since `Namespace` derives from `Dictionary<string, object>`.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("name");
            parser.AddArgument("surname");
            var parsedArgs = parser.ParseArgs(args);
            Console.WriteLine(parsedArgs["name"]);
        }
    }
}
```
```
> wiki.exe arda özcan
arda
```
## Optional Arguments
If you start an argument name with "-" prefix, it will be an optional argument.
Only `Store` action will be referenced here in the Quick Start section. Check the ArgumentAction section for more detailed information.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("-n", "--name");
            var parsedArgs = parser.ParseArgs(args);
            Console.WriteLine(parsedArgs);
        }
    }
}
```
```
> wiki.exe -n arda
Namespace([name, arda])
```
As the name implies these arguments are optional, so optional arguments' values are null by default.
```
> wiki.exe
Namespace([name, ])
```

# ArgumentParser
## Constructor
### prog
`prog` is the program name mentioned in the usage string. It is the executable name by default.

_**Example:**_
```C#
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser(prog: "progName");
            parser.ParseArgs(args);
        }
    }
}
```
```
> wiki -h
usage: progName [-h]

Optional arguments:
  -h, --help            Show this message.
```
### usage
Similar to `prog`, you can override `usage` with a custom string. It is `usage: {prog} {optionalArgs} {positionalArgs}` by default.

_**Example:**_
```C#
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser(usage: "This is my custom usage.");
            parser.ParseArgs(args);
        }
    }
}
```
```
> wiki -h
usage: This is my custom usage.

Optional arguments:
  -h, --help            Show this message.
```
### description
`description` is a string printed just after the usage in a help message. It is `null` by default.

_**Example:**_
```C#
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser(description: "This is a description of this non-existent tool.");
            parser.ParseArgs(args);
        }
    }
}
```
```
> wiki -h
usage: wiki [-h]

This is a description of this non-existent tool.

Optional arguments:
  -h, --help            Show this message.
```
### epilog
String that is printed after the whole help message. It is `null` by default.

_**Example:**_
```C#
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser(epilog: "Help message ends here btw.");
            parser.ParseArgs(args);
        }
    }
}
```
```
> wiki -h
usage: wiki [-h]

Optional arguments:
  -h, --help            Show this message.

Help message ends here btw.
```
## AddArgument
This method adds an argument with the specified properties to the argument parser.
### name
Name of the argument. If this name starts with the "-" prefix then it will be an optional argument, else it will be a positional argument.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("-n");
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki -n "arda özcan"
Namespace([n, arda özcan])
```
### longName
Longer (not necessarily) name of an optional argument, an alias. It can be used for positional arguments, it would only change the key value in the namespace.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("-n", longName: "--name");
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki --name "arda özcan"
Namespace([name, arda özcan])
```
### action
Action of an optional argument. Check the [ArgumentAction](#ArgumentAction) section for further information about actions and what they do.

This example show the usage of the `action` parameter in the AddArgument method.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("-v", action: ArgumentAction.StoreTrue);
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki -v
Namespace([v, True])
```
### defaultValue
Default value of an optional argument if it is not supplied. Default value is `null` by default.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--name", defaultValue: "admin");
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki
Namespace([name, admin])
```
### choices
A list of strings indicating the only selectable options for an argument. An error will be thrown if the provided values is not in the list of choices.

_**Example:**_
```C#
using System;
using System.Collections.Generic;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--level", choices: new List<string> {"low", "mid", "high"});
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki --level somethingElse
usage: wiki [-h] --level LEVEL
wiki: error: invalid choice: '--level' (choose from low, mid, high)
```
### required
Determines is an optional argument is required to be supplied. Can be used for positional arguments too but it won't effect anything.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--filename", required: true);
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki
usage: wiki [-h] --filename FILENAME
wiki: error: the following arguments are required: filename
```
### help
Help string to be printed in the help message for the argument. It is empty by default.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--filename", help: "The filename for the tool.");
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
usage: wiki [-h] --filename FILENAME

Optional arguments:
  -h, --help            Show this message.
  --filename FILENAME   The filename for the tool.
```
### constant
Constant value for the argument. This parameter only works for arguments with `StoreConst` and `AppendConst` actions.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--one", action: ArgumentAction.StoreConst, constant: 1);
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki --one
Namespace([one, 1])
```
## AddSubparsers
This method adds subparsers for the parent parser. Subparsers is an 
object that resembles a branch split in a tree. That means you can 
branch into different parsers from a parent parser. This can be used 
for a tool that does more than one job and requires keyword commands 
to do them. .NET CLI can be an example since it contains a lot of 
different commands (subparsers) for different actions.

A basic subparsers setup would be as follows.
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            // Subparsers object is cmd.
            var cmd = parser.AddSubparsers(help: "Command to be supplied.");

            // 'run' command for the cmd subparsers.
            var runParser = cmd.AddParser("run");
            // Add an argument to the 'run' parser just like a regular one.
            runParser.AddArgument("-fn", longName: "--filename", help: "Filename of the program.");

            // 'parser' command for the cmd subparsers.
            var parseParser = cmd.AddParser("parse");
            // Add an argument to the 'parse' parser.
            parseParser.AddArgument("-o", longName:"--output", help: "Output file of the parsed json.");


            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki -h
usage: wiki [-h] {run,parse} ...

Optional arguments:
  -h, --help            Show this message.

Positional arguments:
  {run,parse}           Command to be supplied.
```
```
> wiki run -h
usage: wiki run [-h] -fn FILENAME

Optional arguments:
  -h, --help            Show this message.
  -fn FILENAME, --filename FILENAME
                        Filename of the program.
```
```
> wiki run -fn "file.bin"
Namespace([filename, file.bin])
```

### help
Help string to be printed in the help message for the subparsers object. It is empty by default.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            var cmd = parser.AddSubparsers(help: "Command to be supplied.");

            var runParser = cmd.AddParser("run");
            runParser.AddArgument("-fn");

            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki -h
usage: wiki [-h] {run} ...

Optional arguments:
  -h, --help            Show this message.

Positional arguments:
  {run}                 Command to be supplied.
```
### title
Shows the subparsers help message in a different section with the given name.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            var cmd = parser.AddSubparsers(title: "cmd");

            var runParser = cmd.AddParser("run");
            runParser.AddArgument("-fn");

            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
usage: wiki [-h] {run} ...

Optional arguments:
  -h, --help            Show this message.

cmd:
  {run}
```
### dest
Adds a key to the namespace for the parser with the supplied string as value. 

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            var cmd = parser.AddSubparsers(dest: "cmd");

            var runParser = cmd.AddParser("run");
            runParser.AddArgument("-fn");

            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki run
Namespace([cmd, run], [fn, file.bin])
```
## ParseArgs
This method will parse the given arguments and return a Namespace object that will allow you to access all arguments.
### args
String array that has the provided arguments as elements. You can use the default `args` array from the Main method parameters or provide a custom array. 

**Note:** the first element shouldn't be the program name/path.

_**Example:**_
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--name");
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki --name arda
Namespace([name, arda])
```
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--name");
            var parsed = parser.ParseArgs(new string[] {"--name", "arda"});
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki
Namespace([name, arda])
```

## ArgumentAction
### Store
Store the value right after the argument as a string.
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--name", action: ArgumentAction.Store);
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki --name value
Namespace([name, value])
```
### StoreConst
Store the constant passed in to the argument as a value when the argument is supplied.
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--addConst", action: ArgumentAction.StoreConst, constant: "constant");
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki --addConst value
Namespace([addConst, constant])
```
### StoreTrue
Store a true boolean as a value when the argument is supplied. Default value of the argument is false.
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("-v", longName: "--verbose", action: ArgumentAction.StoreTrue);
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
Namespace([verbose, True])
```
### StoreFalse
Store a false boolean as a value when the argument is supplied. Default value of the argument is true.
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--doNotDoSomething", action: ArgumentAction.StoreFalse);
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki --doNotDoSomething
Namespace([doNotDoSomething, False])
```
### Append
Append the value right after the argument to the same list whenever the argument is supplied.
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--add", action: ArgumentAction.Append);
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki --add first --add second
Namespace([add, [first, second]])
```
### AppendConst
Append the constant passed in to the argument to the same list whenever the argument is supplied.
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("--one", action: ArgumentAction.AppendConst, constant: 1);
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki --one --one --one
Namespace([one, [1, 1, 1]])
```
### Help
Print the help message. This action belongs to the "-h" argument by default.
```C#
using System;
using ArdaOzcan.SimpleArgParse;

namespace Wiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser();
            parser.AddArgument("-sh", action: ArgumentAction.Help);
            var parsed = parser.ParseArgs(args);
            Console.WriteLine(parsed);
        }
    }
}
```
```
> wiki -sh
usage: wiki [-h] [-sh]

Optional arguments:
  -h, --help            Show this message.
  -sh
```