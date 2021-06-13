namespace Yates.Runtime.Helper
{
    public class IDHelper
    {
        private static long idCount;

        public static long Get()
        {
            return ++idCount;
        }
    }
}