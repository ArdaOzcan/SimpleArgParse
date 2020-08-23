
using System.Collections.Generic;


namespace ArdaOzcan.SimpleArgParse
{
    public class ArgumentValueList : List<object>
    {
        public override string ToString() => "[" + string.Join(", ", this) + "]";
    }
}