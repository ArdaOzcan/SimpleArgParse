
using System.Collections.Generic;


namespace ArdaOzcan.SimpleArgParse
{
    public class Namespace : Dictionary<string, object>
    {
        public override string ToString() => "Namespace(" + string.Join(", ", this) + ")";

        public void Join(Dictionary<string, object> other)
        {
            foreach(var kv in other)
                this[kv.Key] = kv.Value;
        }
    }
}