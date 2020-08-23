namespace ArdaOzcan.SimpleArgParse
{
    public static class StringUtils
    {
        public const char DefaultPrefix = '-';

        public const int HelpStringOffset = 24;

        public static bool IsOptionalArgument(this string str)
        {
            if(str.Length > 0)
                return str[0] == DefaultPrefix;

            return false;
        }
    }
}