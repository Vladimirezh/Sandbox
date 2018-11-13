using System;

namespace Sandbox.Common
{
    public static class Guard
    {
        public static string NotNullOrEmpty(string val, string paramName)
        {
            if (val == null)
                throw new ArgumentNullException(paramName);
            if (val == string.Empty)
                throw new ArgumentException(paramName);
            return val;
        }

        public static T NotNull<T>(T val)
        {
            if (val == null)
                throw new ArgumentNullException();
            return val;
        }
    }
}