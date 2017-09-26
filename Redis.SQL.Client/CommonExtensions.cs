using System;
using System.Collections.Generic;
using System.Linq;

namespace Redis.SQL.Client
{
    internal static class CommonExtensions
    {
        internal static IEnumerable<string> FilterStringIgnoreCase(this IEnumerable<string> source, string filter)
        {
            return source.Where(x => !string.Equals(x, filter, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
