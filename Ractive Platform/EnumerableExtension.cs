using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ReactivePlatform
{
    [DebuggerStepThrough]
    public static class EnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> enumerabe, Action<T> action)
        {
            foreach(var item in enumerabe)
            {
                action(item);
            }
        }
    }
}