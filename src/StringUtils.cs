namespace ArdaOzcan.SimpleArgParse
{
    internal static class StringUtils
    {
        public const char DefaultPrefix = '-';

        public const int HelpStringOffset = 24;

        public static bool IsOptionalArgument(this string str)
        {
            if(str == null)
                return false;
            
            if(str.Length > 0)
                return (str[0] == DefaultPrefix);

            return false;
        }
    }
}