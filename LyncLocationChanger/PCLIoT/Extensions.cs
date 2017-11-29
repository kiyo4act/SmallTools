using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLIoT
{
    public static class Extensions
    {

        public static TSource MinBy<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector
        )
        {
            var value = source.Min(selector);
            return source.First(c => selector(c).Equals(value));
        }

    }
}
