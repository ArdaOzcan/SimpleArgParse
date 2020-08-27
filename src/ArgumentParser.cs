using System;

using System.Collections.Generic;
using System.Linq;

namespace ArdaOzcan.SimpleArgParse
{
    public class ArgumentParser
    {
        public string Prog { get; set; }

        public string Usage { get; set; }

        public string Description { get; }

        public string Epilog { get; }

        const string positionalArgsTitle = "Positional arguments";

        const string optionalArgsTitle = "Optional arguments";

        List<Argument> PositionalArguments { get; set; }

        List<Argument> OptionalArguments { get; set; }

        Dictionary<string, List<Argument>> Categories { get; set; }

        private List<Argument> arguments;

        /// <summary>
        /// Argument parser for the command line arguments.
        /// </summary>
        /// <param name="prog">Program name mentioned in the usage string. It is the executable name by default.</param>
        /// <param name="usage">Similar to `prog`, you can override `usage` with a custom string. It is `usage: {prog} {optionalArgs} {positionalArgs}` by default.</param>
        /// <param name="description">`description` is a string printed just after the usage in a help message. It is `null` by default.</param>
        /// <param name="epilog">String that is printed after the whole help message. It is `null` by default.</param>
        public ArgumentParser(string prog = null,
                              string usage = null,
                              string description = null,
                              string epilog = null)
        {
            Prog = prog;

            if (Prog == null)
                Prog = System.AppDomain.CurrentDomain.FriendlyName;

            Usage = usage;
            Description = description;
            Epilog = epilog;
            Argument helpArg = new Argument("-h",
                                            longName: "--help",
                                            action: ArgumentAction.Help,
                                            help: "Show this message.");

            arguments = new List<Argument> { helpArg };

            OptionalArguments = new List<Argument> { helpArg };
            PositionalArguments = new List<Argument>();

            Categories = new Dictionary<string, List<Argument>>();

            Categories[optionalArgsTitle] = new List<Argument>() { helpArg };
            Categories[positionalArgsTitle] = new List<Argument>();
        }

        /// <summary>
        /// Add an argument with the specified properties to the argument parser.
        /// </summary>
        /// <param name="name">Name of the argument. If this name starts with the "-" prefix then it will be an optional argument, else it will be a positional argument.</param>
        /// <param name="longName">Longer (not necessarily) name of an optional argument, an alias. It can be used for positional arguments, it would only change the key value in the namespace.</param>
        /// <param name="action">Action of an optional argument.</param>
        /// <param name="defaultValue">Default value of an optional argument if it is not supplied. Default value is `null` by default.</param>
        /// <param name="choices">A list of strings indicating the only selectable options for an argument. An error will be thrown if the provided values is not in the list of choices.</param>
        /// <param name="required">Determines is an optional argument is required to be supplied. Can be used for positional arguments too but it won't effect anything.</param>
        /// <param name="help">Help string to be printed in the help message for the argument. It is empty by default.</param>
        /// <param name="constant">Constant value for the argument. This parameter only works for arguments with `StoreConst` and `AppendConst` actions.</param>
        public void AddArgument(string name,
                                string longName = null,
                                ArgumentAction action = ArgumentAction.Store,
                                object defaultValue = null,
                                List<string> choices = null,
                                bool required = false,
                                string help = "",
                                object constant = null)
        {
            if (name.Contains(" "))
                throw new InvalidArgumentNameException("An argument name can't contain spaces.");

            Argument arg = new Argument(name,
                                        longName: longName,
                                        action: action,
                                        defaultValue: defaultValue,
                                        choices: choices,
                                        required: required,
                                        help: help,
                                        constant: constant);
            arguments.Add(arg);

            if (arg.Name.IsOptionalArgument())
            {
                Categories[optionalArgsTitle].Add(arg);
                OptionalArguments.Add(arg);
            }
            else
            {
                Categories[positionalArgsTitle].Add(arg);
                PositionalArguments.Add(arg);
            }
        }

        /// <summary>
        /// Add subparsers for the parent parser. Subparsers is an 
        /// object that resembles a branch split in a tree. That means you can 
        /// branch into different parsers from a parent parser. This can be used 
        /// for a tool that does more than one job and requires keyword commands 
        /// to do them. .NET CLI can be an example since it contains a lot of 
        /// different commands (subparsers) for different actions.
        /// </summary>
        /// <param name="help">Help string to be printed in the help message for the subparsers object. It is empty by default.</param>
        /// <param name="title">Shows the subparsers help message in a different section with the given name.</param>
        /// <param name="dest">Adds a key to the namespace for the parser with the supplied string as value.</param>
        /// <returns>The added subparser object.</returns>
        public Subparsers AddSubparsers(string help = "", string title = "", string dest = "")
        {
            var subparsers = new Subparsers(Prog, help, title, dest);
            arguments.Add(subparsers);

            if (!string.IsNullOrEmpty(subparsers.Title))
            {
                if (Categories.ContainsKey(subparsers.Title))
                    Categories[subparsers.Title].Add(subparsers);
                else
                    Categories[subparsers.Title] = new List<Argument>() { subparsers };
            }
            else
            {
                Categories[positionalArgsTitle].Add(subparsers);
            }

            PositionalArguments.Add(subparsers);

            return subparsers;
        }

        private void PrintHelp()
        {
            PrintUsage();
            Console.WriteLine();
            if (!string.IsNullOrEmpty(Description))
            {
                Console.WriteLine(Description);
                Console.WriteLine();
            }

            foreach (var x in Categories)
                PrintHelpArray(x.Key, GetHelpList(x.Value), x.Value);


            if (!string.IsNullOrEmpty(Epilog))
                Console.WriteLine(Epilog);

            void PrintHelpArray(string title, List<string> helpList, List<Argument> args)
            {
                if (helpList.Count != 0)
                {
                    Console.WriteLine(title + ": ");
                    for (int i = 0; i < helpList.Count; i++)
                    {
                        Console.Write(helpList[i]);
                        if (helpList[i].Length >= StringUtils.HelpStringOffset - 1)
                        {
                            Console.WriteLine();
                            Console.Write(new string(' ', StringUtils.HelpStringOffset));
                        }
                        else
                        {
                            Console.Write(new string(' ', StringUtils.HelpStringOffset - helpList[i].Length));
                        }

                        if (!string.IsNullOrEmpty(args[i].Help))
                            Console.Write(args[i].Help);
                        Console.WriteLine();
                    }

                    Console.WriteLine();
                }
            }

            List<string> GetHelpList(List<Argument> args)
            {
                List<string> helpList = new List<string>();
                foreach (var arg in args)
                    helpList.Add("  " + arg.DisplayName);
                return helpList;
            }
        }

        private void PrintUsage()
        {
            Console.WriteLine($"usage: {GetUsage()}");
        }

        private string GetUsage()
        {
            if (Usage != null)
                return Usage;

            string s = $"{Prog} ";

            foreach (var o in OptionalArguments)
                s += o.Usage + " ";

            foreach (var p in PositionalArguments)
            {
                s += p.Usage + " ";
                if (p.GetType() == typeof(Subparsers))
                    s += "... ";
            }

            return s;
        }

        private bool OptionalArgumentExists(string arg, out Argument outArg)
        {
            foreach (var optArg in Categories[optionalArgsTitle])
            {
                if (optArg.Name == arg || optArg.LongName == arg)
                {
                    outArg = optArg;
                    return true;
                }
            }

            outArg = null;
            return false;
        }

        void CheckValueInChoices(Argument arg, object value)
        {
            if (arg.Choices != null && !arg.Choices.Contains(value))
                PrintError($"invalid choice: '{arg}' (choose from {string.Join(", ", arg.Choices)})");

        }

        /// <summary>
        /// This method will parse the given arguments and return a Namespace object that will allow you to access all arguments.
        /// </summary>
        /// <param name="args">String array that has the provided arguments as elements. You can use the default `args` array from the Main method parameters or provide a custom array. </param>
        /// <returns>Generated namespace.</returns>
        public Namespace ParseArgs(string[] args)
        {
            int notUsed;
            return ParseArgs(args, out notUsed);
        }

        internal Namespace ParseArgs(string[] args, out int lastPos, bool returnWhenDone=false)
        {
            var argNamespace = new Namespace();
            var unrecognizedArguments = new List<string>();

            foreach (var arg in OptionalArguments)
            {
                if (arg.Action != ArgumentAction.Help && !arg.Required)
                {
                    argNamespace[arg.KeyName] = arg.DefaultValue;
                    if (arg.Action == ArgumentAction.StoreTrue)
                        argNamespace[arg.KeyName] = false;
                    if (arg.Action == ArgumentAction.StoreFalse)
                        argNamespace[arg.KeyName] = true;
                }
            }


            int pos = 0;
            int positionalArgPos = 0;
            while (pos < args.Length)
            {
                string arg = args[pos];

                if (arg.IsOptionalArgument())
                {
                    Argument optArg;
                    if (OptionalArgumentExists(arg, out optArg))
                    {
                        switch (optArg.Action)
                        {
                            case ArgumentAction.Store:
                                pos += 1;

                                if (pos >= args.Length)
                                    PrintError($"argument {optArg.Name}: expected one argument");

                                string val = args[pos];
                                if (val.IsOptionalArgument())
                                    PrintError($"argument {optArg.Name}: expected one argument");

                                CheckValueInChoices(optArg, val);
                                argNamespace[optArg.KeyName] = val;
                                break;

                            case ArgumentAction.StoreConst:
                                argNamespace[optArg.KeyName] = optArg.Constant;
                                break;

                            case ArgumentAction.StoreTrue:
                                argNamespace[optArg.KeyName] = true;
                                break;

                            case ArgumentAction.StoreFalse:
                                argNamespace[optArg.KeyName] = false;
                                break;

                            case ArgumentAction.Append:
                                pos += 1;

                                if (pos >= args.Length)
                                    PrintError($"argument {optArg.Name}: expected one argument");


                                string appendVal = args[pos];
                                if (appendVal.IsOptionalArgument())
                                    PrintError($"argument {optArg.Name}: expected one argument");


                                object list = null;
                                argNamespace.TryGetValue(optArg.KeyName, out list);
                                if (list == null)
                                    argNamespace[optArg.KeyName] = new ArgumentValueList();

                                CheckValueInChoices(optArg, appendVal);
                                ((ArgumentValueList)argNamespace[optArg.KeyName]).Add(appendVal);
                                break;

                            case ArgumentAction.AppendConst:
                                object constList = null;
                                argNamespace.TryGetValue(optArg.KeyName, out constList);

                                if (constList == null)
                                    argNamespace[optArg.KeyName] = new ArgumentValueList();

                                ((ArgumentValueList)argNamespace[optArg.KeyName]).Add(optArg.Constant);
                                break;

                            case ArgumentAction.Help:
                                PrintHelp();
                                Environment.Exit(0);
                                break;
                        }
                    }
                    else
                        unrecognizedArguments.Add(arg);
                    

                    pos += 1;
                    continue;
                }

                Argument currentPositionalArg = null;
                if (positionalArgPos < PositionalArguments.Count)
                    currentPositionalArg = PositionalArguments[positionalArgPos];
                else if(returnWhenDone)
                {
                    lastPos = pos;
                    return argNamespace;
                }

                if (currentPositionalArg != null)
                {
                    if (!string.IsNullOrEmpty(currentPositionalArg.KeyName))
                    {
                        CheckValueInChoices(currentPositionalArg, arg);
                        argNamespace[currentPositionalArg.KeyName] = arg;
                    }

                    if (currentPositionalArg.GetType() == typeof(Subparsers))
                    {
                        string[] newArr = new string[args.Length - pos - 1];
                        Array.Copy(args, pos + 1, newArr, 0, args.Length - pos - 1);

                        var currentSubparser = ((Subparsers)currentPositionalArg);
                        if (currentSubparser.parsers.ContainsKey(arg))
                        {
                            int subLastPos;
                            var ns = currentSubparser.parsers[arg].ParseArgs(newArr, out subLastPos, returnWhenDone: true);
                            pos += subLastPos;
                            argNamespace.Join(ns);
                        }
                        else
                        {
                            PrintError($"invalid choice: '{arg}' (choose from {string.Join(", ", currentSubparser.parsers.Keys)})");
                        }
                    }
                }
                else
                    unrecognizedArguments.Add(arg);

                pos += 1;
                positionalArgPos += 1;
            }

            if(unrecognizedArguments.Count > 0)
                PrintError($"unrecognized arguments: {string.Join(", ", unrecognizedArguments)}");

            var notSuppliedPositionalArguments = new List<string>();
            foreach (var arg in PositionalArguments)
            {
                if (!argNamespace.ContainsKey(arg.KeyName) && arg.GetType() != typeof(Subparsers))
                    notSuppliedPositionalArguments.Add(arg.KeyName);
            }

            var notSuppliedRequiredOptionalArguments = new List<string>();
            foreach (var arg in OptionalArguments)
            {
                if (!argNamespace.ContainsKey(arg.KeyName) && arg.Required)
                    notSuppliedRequiredOptionalArguments.Add(arg.KeyName);
            }

            if (notSuppliedRequiredOptionalArguments.Count > 0)
                PrintError($"the following arguments are required: {string.Join(", ", notSuppliedRequiredOptionalArguments)}");

            if (notSuppliedPositionalArguments.Count > 0)
                PrintError($"the following arguments are required: {string.Join(", ", notSuppliedPositionalArguments)}");

            lastPos = pos;
            return argNamespace;
        }

        private void PrintError(string msg)
        {
            PrintUsage();
            Console.WriteLine($"{Prog}: error: {msg}");
            Environment.Exit(1);
        }
    }
}