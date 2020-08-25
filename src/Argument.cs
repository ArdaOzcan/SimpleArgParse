using System;

using System.Collections.Generic;
using System.Globalization;

namespace ArdaOzcan.SimpleArgParse
{
    public class Argument
    {
        public virtual string DisplayName
        {
            get
            {   
                if(LongName != null)
                        return string.Format("{0}, {1}", Name, LongName);
                    else
                        return Name;
            }
        }

        public string Name { get; }

        public bool IsPositional { get; }

        public bool IsOptional { get; }

        public string LongName { get; }

        public ArgumentAction Action { get; }

        public object DefaultValue { get; }

        public Type Type { get; }

        public List<string> Choices { get; }

        public bool Required { get; }

        public string Help { get; }

        public string Usage { get; set; }

        public object Constant { get; }

        public string KeyName { get; }

        public Argument(string name,
                        string longName = null,
                        ArgumentAction action = ArgumentAction.Store,
                        object defaultValue = null,
                        Type type = null,
                        List<string> choices = null,
                        bool required = false,
                        string help = "",
                        object constant = null)
        {
            Name = name;
            IsOptional = Name.IsOptionalArgument();
            IsPositional = !IsOptional;
            LongName = longName;
            Action = action;
            DefaultValue = defaultValue;
            Type = type;
            Choices = choices;
            Required = required;
            Help = help;
            Constant = constant;
            Usage = Name;

            if(LongName == null)
                KeyName = Name.TrimStart(StringUtils.DefaultPrefix);
            else
                KeyName = LongName.TrimStart(StringUtils.DefaultPrefix);
        }

        public override string ToString() => Name;

    }
}