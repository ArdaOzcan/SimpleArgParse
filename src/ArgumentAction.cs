namespace ArdaOzcan.SimpleArgParse
{
    public enum ArgumentAction
    {
        /// <summary>Store the value right after the argument as a string.</summary>
        Store,
        /// <summary>Store the constant passed in to the argument as a value when the argument is supplied.</summary>
        StoreConst,
        /// <summary>Store a true boolean as a value when the argument is supplied. Default value of the argument is false.</summary>
        StoreTrue,
        /// <summary>Store a false boolean as a value when the argument is supplied. Default value of the argument is true.</summary>
        StoreFalse,
        /// <summary>Append the value right after the argument to the same list whenever the argument is supplied.</summary>
        Append,
        /// <summary>Append the constant passed in to the argument to the same list whenever the argument is supplied.</summary>
        AppendConst,
        /// <summary>Print the help message. This action belongs to the "-h" argument by default.</summary>
        Help
    }
}