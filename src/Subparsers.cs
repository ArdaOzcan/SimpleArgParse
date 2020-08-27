
using System.Collections.Generic;


namespace ArdaOzcan.SimpleArgParse
{
    public class Subparsers : Argument
    {
        public override string DisplayName => string.Format("{{{0}}}", string.Join(",", parsers.Keys));

        public Dictionary<string, ArgumentParser> parsers;

        public string Prog { get; }

        public string Title { get; }

        internal Subparsers(string prog, string help = "", string title = "", string dest="") : base(dest, help: help)
        {
            parsers = new Dictionary<string, ArgumentParser>();
            Prog = prog;
            Title = title;
            Usage = DisplayName;
        }

        public ArgumentParser AddParser(string name)
        {   
            var parser = new ArgumentParser(Prog + " " + name);
            parsers[name] = parser;
            Usage = DisplayName;
            return parsers[name];
        }
    }
}