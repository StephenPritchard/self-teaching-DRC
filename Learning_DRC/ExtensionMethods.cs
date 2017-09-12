using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Learning_DRC
{
    internal static class ExtensionMethods
    {
        public static IEnumerable<IEnumerable<T>> Combinations<T>(
            this IEnumerable<T> source1, IEnumerable<T> source2)
        {
            return (from s1 in source1 from s2 in source2 select new[] { s1, s2 }).Cast<IEnumerable<T>>();
        }


        public static IEnumerable<IEnumerable<T>> Combinations<T>(
            this IEnumerable<IEnumerable<T>> source1, IEnumerable<T> source2)
        {
            foreach (IEnumerable<T> s1 in source1) yield return s1;
            foreach (T s2 in source2) yield return new[] { s2 };
            foreach (IEnumerable<T> s1 in source1)
                foreach (T s2 in source2)
                    yield return s1.Concat(new[] { s2 }).ToArray();
        }
    }
}
