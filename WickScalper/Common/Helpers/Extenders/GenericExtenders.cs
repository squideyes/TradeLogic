using System;

namespace WickScalper.Common
{
    public static class GenericExtenders
    {
        public static R Convert<T, R>(
            this T value, Func<T, R> convert) => convert(value);
    }
}
