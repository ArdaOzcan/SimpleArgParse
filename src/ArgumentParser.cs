using System;

using System.Collections.Generic;


namespace ArdaOzcan.SimpleArgParse
{
    public class ArgumentParser
    {
        public string Prog { get; set; }

        public string Usage { get; set; }

        public string Description { get; }

        public string Epilog { get; }

        public List<Argument> PositionalArguments { get; set; }

        public List<Argument> OptionalArguments { get; set; }

        public Namespace ArgNamespace { get; }

        private List<Argument> arguments;

        private HashSet<string> optionalArgumentNames;

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
                                            alternativeName: "--help",
                                            action: ArgumentAction.Help,
                                            help: "Show this message.");

            arguments = new List<Argument> { helpArg };
            OptionalArguments = new List<Argument> { helpArg };
            PositionalArguments = new List<Argument>();
            optionalArgumentNames = new HashSet<string>();
            ArgNamespace= new Namespace();
        }

        public void AddArgument(string name,
                                string alternativeName = null,
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
                                        alternativeName: alternativeName,
                                        action: action,
                                        defaultValue: defaultValue,
                                        type: type,
                                        choices: choices,
                                        required: required,
                                        help: help,
                                        constant: constant);
            arguments.Add(arg);
            ArgNamespace.Add(arg.KeyName, null);
            if (arg.Name.IsOptionalArgument())
            {
                OptionalArguments.Add(arg);
                optionalArgumentNames.Add(arg.Name);
                if (arg.AlternativeName != null)
                    optionalArgumentNames.Add(arg.AlternativeName);
            }
            else
                PositionalArguments.Add(arg);
        }

        public void PrintHelp()
        {
            string[] positionalHelp = new string[PositionalArguments.Count];
            for (int i = 0; i < PositionalArguments.Count; i++)
            {
                Argument arg = PositionalArguments[i];
                positionalHelp[i] += "  " + arg.DisplayName;
            }

            string[] optionalHelp = new string[OptionalArguments.Count];
            for (int i1 = 0; i1 < OptionalArguments.Count; i1++)
            {
                Argument arg = OptionalArguments[i1];
                optionalHelp[i1] += "  " + arg.DisplayName;
            }

            PrintUsage();
            Console.WriteLine();
            if (positionalHelp.Length != 0)
            {
                Console.WriteLine("Positional arguments: ");
                for (int i2 = 0; i2 < positionalHelp.Length; i2++)
                {
                    Console.Write(positionalHelp[i2]);
                    if (positionalHelp[i2].Length >= StringUtils.HelpStringOffset - 1)
                    {
                        Console.WriteLine();
                        Console.Write(new string(' ', StringUtils.HelpStringOffset));
                    }
                    else
                    {
                        Console.Write(new string(' ', StringUtils.HelpStringOffset - positionalHelp[i2].Length));
                    }

                    Console.WriteLine(PositionalArguments[i2].Help);
                }
                
                Console.WriteLine();
            }

            if (optionalHelp.Length != 0)
            {
                Console.WriteLine("Optional arguments: ");
                for (int i3 = 0; i3 < optionalHelp.Length; i3++)
                {
                    Console.Write(optionalHelp[i3]);
                    if (optionalHelp[i3].Length >= StringUtils.HelpStringOffset - 1)
                    {
                        Console.WriteLine();
                        Console.Write(new string(' ', StringUtils.HelpStringOffset));
                    }
                    else
                    {
                        Console.Write(new string(' ', StringUtils.HelpStringOffset - optionalHelp[i3].Length));
                    }

                    Console.WriteLine(OptionalArguments[i3].Help);
                }

                Console.WriteLine();
            }

            Console.WriteLine(Epilog);
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
                s += p.Usage + " ";

            return s;
        }

        public bool OptionalArgumentExists(string arg, out Argument outArg)
        {
            foreach (var optArg in OptionalArguments)
            {
                if (optArg.Name == arg || optArg.AlternativeName == arg)
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
                    ArgNamespace[currentPositionalArg.Name] = arg;

                pos += 1;
                positionalArgPos += 1;
            }

            var notSuppliedPositionalArguments = new List<string>();
            foreach (var arg in PositionalArguments)
            {
                if (!ArgNamespace.ContainsKey(arg.Name))
                {
                    notSuppliedPositionalArguments.Add(arg.Name);
                }
            }

            if (notSuppliedPositionalArguments.Count > 0)
            {
                PrintUsage();
                PrintError($"the following arguments are required: {string.Join(", ", notSuppliedPositionalArguments)}");
            }

            return ArgNamespace;
        }

        private void PrintError(string msg)
        {
            Console.WriteLine($"{Prog} : error: {msg}");
            Environment.Exit(-1);
        }
    }
}