//
//  Copyright (c) 2014 Jan Lucansky. All rights reserved.
//
//  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTIES
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using WmBridge.Web.Model;

namespace WmBridge.Web
{
    public static class EnumerableExtensions
    {
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this IEnumerable<Dictionary<TKey, TValue>> source)
        {
            return source.SelectMany().ToDictionary(d => d.Key, d => d.Value);
        }
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> source1, params Dictionary<TKey, TValue>[] source2)
        {
            return source1.Concat(source2.SelectMany()).ToDictionary(d => d.Key, d => d.Value);
        }

        public static void AddArguments(this PowerShell powershell, params object[] arguments)
        {
            foreach (var arg in arguments)
                powershell.AddArgument(arg);
        }

        public static PSPropertySelector[] ForEach(this PSPropertySelector[] items, Action<PSPropertySelector> action)
        {
            foreach (var item in items)
                action(item);

            return items;
        }

        public static PSPropertySelector[] RemoveExpression(this PSPropertySelector[] items)
        {
            return items.ForEach(s => s.PSExpression = null);
        }

        public static PSDictionary ToDictionary<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, Func<TSource, object> elementSelector, Func<PSDictionary> factory = null)
        {
            PSDictionary result = factory == null ? new PSDictionary() : factory();
            foreach (var item in source)
                result.Add(keySelector(item), elementSelector(item));
            return result;
        }

        public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(_ => _);
        }

        public static IEnumerable<TSource> WhereNotNull<TSource>(this IEnumerable<TSource> source)
        {
            return source.Where(_ => _ != null);
        }

        public static IEnumerable<TSource> LessOrEmpty<TSource>(this IEnumerable<TSource> source, int maximum)
        {
            if (source.Count() <= maximum)
                return source;
            else
                return new TSource[0];
        }

    }
}
