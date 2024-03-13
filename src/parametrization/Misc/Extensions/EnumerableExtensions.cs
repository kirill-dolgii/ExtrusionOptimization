using System;
using System.Collections.Generic;
using System.Linq;

namespace Parametrization.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<Tuple<T, T>> Pairs<T>(this IEnumerable<T> source, bool closed = false)
    {
        if (source == null) throw new ArgumentNullException();
        var secondSource = source.Skip(1);
        if (closed) secondSource = secondSource.Append(source.First());
        var pairs = source.Zip(secondSource, (arg1, arg2) => new Tuple<T, T>(arg1, arg2));
        return pairs;
    }
}