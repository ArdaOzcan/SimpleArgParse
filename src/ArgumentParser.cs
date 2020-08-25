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

        public List<Argument> PositionalArguments { get; set; }

        public List<Argument> OptionalArguments { get; set; }

        public Dictionary<string, List<Argument>> Categories { get; set; }

        public Namespace ArgNamespace { get; }

        private List<Argument> arguments;

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

            ArgNamespace = new Namespace();
            Categories = new Dictionary<string, List<Argument>>();
            
            Categories[optionalArgsTitle] = new List<Argument>() { helpArg };
            Categories[positionalArgsTitle] = new List<Argument>();
        }

        public void AddArgument(string name,
                                string longName = null,
                                ArgumentAction action = ArgumentAction.Store,
                                object defaultValue = null,
                                Type type = null,
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
                                        type: type,
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

        public Subparsers AddSubparsers(string help = "", string title = "", string dest = "")
        {
            var subparsers = new Subparsers(Prog, help, title, dest);
            arguments.Add(subparsers);
            
            
            if(Categories.ContainsKey(subparsers.Title))
                Categories[subparsers.Title].Add(subparsers);
            else
                Categories[subparsers.Title] = new List<Argument>() {subparsers};
            
            PositionalArguments.Add(subparsers);
            
            return subparsers;
        }

        public void PrintHelp()
        {
            PrintUsage();
            Console.WriteLine();
            // PrintHelpArray("Positional arguments", GetHelpList(PositionalArguments), PositionalArguments);
            // PrintHelpArray("Optional arguments", GetHelpList(OptionalArguments), OptionalArguments);

            foreach(var x in Categories)
                PrintHelpArray(x.Key, GetHelpList(x.Value), x.Value);
            
            if(!string.IsNullOrEmpty(Epilog))
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

                        if(!string.IsNullOrEmpty(args[i].Help))
                            Console.WriteLine(args[i].Help);
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
            Console.WriteLine($"usage: {Prog} {GetUsage()}");
        }

        private string GetUsage()
        {
            if (Usage != null)
                return Usage;

            string s = "";

            foreach (var o in OptionalArguments)
                s += $"[{o.Usage}] ";

            foreach (var p in PositionalArguments)
            {
                s += p.Usage + " ";
                if (p.GetType() == typeof(Subparsers))
                    return s + "...";
            }

            return s;
        }

        public bool OptionalArgumentExists(string arg, out Argument outArg)
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

        public Namespace ParseArgs(string[] args)
        {
            int pos = 0;
            int positionalArgPos = 0;
            while (pos < args.Length)
            {
                string arg = args[pos];
                Argument currentPositionalArg = null;
                if (positionalArgPos < PositionalArguments.Count)
                    currentPositionalArg = PositionalArguments[positionalArgPos];

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

                                ArgNamespace[optArg.KeyName] = val;
                                break;

                            case ArgumentAction.StoreConst:
                                ArgNamespace[optArg.KeyName] = optArg.Constant;
                                break;

                            case ArgumentAction.StoreTrue:
                                ArgNamespace[optArg.KeyName] = true;
                                break;

                            case ArgumentAction.StoreFalse:
                                ArgNamespace[optArg.KeyName] = false;
                                break;

                            case ArgumentAction.Append:
                                pos += 1;

                                if (pos >= args.Length)
                                    PrintError($"argument {optArg.Name}: expected one argument");


                                string appendVal = args[pos];
                                if (appendVal.IsOptionalArgument())
                                    PrintError($"argument {optArg.Name}: expected one argument");


                                object list = null;
                                ArgNamespace.TryGetValue(optArg.KeyName, out list);
                                if (list == null)
                                    ArgNamespace[optArg.KeyName] = new ArgumentValueList();

                                ((ArgumentValueList)ArgNamespace[optArg.KeyName]).Add(appendVal);
                                break;

                            case ArgumentAction.AppendConst:
                                object constList = null;
                                ArgNamespace.TryGetValue(optArg.KeyName, out constList);

                                if (constList == null)
                                    ArgNamespace[optArg.KeyName] = new ArgumentValueList();

                                ((ArgumentValueList)ArgNamespace[optArg.KeyName]).Add(optArg.Constant);
                                break;

                            case ArgumentAction.Help:
                                PrintHelp();
                                Environment.Exit(0);
                                break;
                        }
                    }

                    pos += 1;
                    continue;
                }

                if (currentPositionalArg != null)
                {
                    if (!string.IsNullOrEmpty(currentPositionalArg.KeyName))
                        ArgNamespace[currentPositionalArg.KeyName] = arg;

                    if (currentPositionalArg.GetType() == typeof(Subparsers))
                    {
                        string[] newArr = new string[args.Length - pos - 1];
                        Array.Copy(args, pos + 1, newArr, 0, args.Length - pos - 1);

                        var currentSubparser = ((Subparsers)currentPositionalArg);
                        if (currentSubparser.parsers.ContainsKey(arg))
                        {
                            var ns = currentSubparser.parsers[arg].ParseArgs(newArr);
                            ArgNamespace.Join(ns);
                        }
                        else
                        {
                            PrintError($"invalid choice: '{arg}' (choose from {string.Join(", ", currentSubparser.parsers.Keys)})");
                        }

                        return ArgNamespace;
                    }
                }

                pos += 1;
                positionalArgPos += 1;
            }

            var notSuppliedPositionalArguments = new List<string>();
            foreach (var arg in Categories[positionalArgsTitle])
            {
                if (!ArgNamespace.ContainsKey(arg.Name))
                    notSuppliedPositionalArguments.Add(arg.Name);
            }

            if (notSuppliedPositionalArguments.Count > 0)
            {
                PrintError($"the following arguments are required: {string.Join(", ", notSuppliedPositionalArguments)}");
            }

            return ArgNamespace;
        }

        private void PrintError(string msg)
        {
            PrintUsage();
            Console.WriteLine($"{Prog} : error: {msg}");
            Environment.Exit(-1);
        }
    }
}