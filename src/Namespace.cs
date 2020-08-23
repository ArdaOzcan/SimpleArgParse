
using System.Collections.Generic;


namespace ArdaOzcan.SimpleArgParse
{
    public class Namespace : Dictionary<string, object>
    {
        public override string ToString() => "Namespace(" + string.Join(", ", this) + ")";
    }
}