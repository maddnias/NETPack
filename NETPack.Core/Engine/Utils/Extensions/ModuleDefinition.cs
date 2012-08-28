using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace NETPack.Core.Engine.Utils.Extensions
{
    static class Functional
    {

        public static System.Func<A, R> Y<A, R>(System.Func<System.Func<A, R>, System.Func<A, R>> f)
        {
            System.Func<A, R> g = null;
            g = f(a => g(a));
            return g;
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return PrependIterator(source, element);
        }

        static IEnumerable<TSource> PrependIterator<TSource>(IEnumerable<TSource> source, TSource element)
        {
            yield return element;

            foreach (var item in source)
                yield return item;
        }
    }

    public static class ModuleDefinitionExt
    {
        public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            // it was fun to write, but we need a somewhat less convoluted implementation
            return self.Types.SelectMany(
                Functional.Y<TypeDefinition, IEnumerable<TypeDefinition>>(f => type => type.NestedTypes.SelectMany(f).Prepend(type)));
        }
    }
}
