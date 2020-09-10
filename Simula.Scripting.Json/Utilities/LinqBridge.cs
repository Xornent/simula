﻿
#if !HAVE_LINQ

#region License, Terms and Author(s)
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Simula.Scripting.Json.Serialization;

#nullable disable

namespace Simula.Scripting.Json.Utilities.LinqBridge
{
  internal static partial class Enumerable
  {

    public static IEnumerable<TSource> AsEnumerable<TSource>(IEnumerable<TSource> source)
    {
      return source;
    }

    public static IEnumerable<TResult> Empty<TResult>()
    {
      return Sequence<TResult>.Empty;
    }

    public static IEnumerable<TResult> Cast<TResult>(
      this IEnumerable source)
    {
      CheckNotNull(source, "source");

        var servesItself = source as IEnumerable<TResult>;
        if (servesItself != null
            && (!(servesItself is TResult[]) || servesItself.GetType().GetElementType() == typeof(TResult)))
        {
            return servesItself;
        }

        return CastYield<TResult>(source);
    }

    private static IEnumerable<TResult> CastYield<TResult>(
      IEnumerable source)
    {
      foreach (var item in source)
        yield return (TResult) item;
    }

    public static IEnumerable<TResult> OfType<TResult>(
      this IEnumerable source)
    {
      CheckNotNull(source, "source");

      return OfTypeYield<TResult>(source);
    }

    private static IEnumerable<TResult> OfTypeYield<TResult>(
      IEnumerable source)
    {
      foreach (var item in source)
        if (item is TResult)
          yield return (TResult) item;
    }

    public static IEnumerable<int> Range(int start, int count)
    {
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", count, null);

      var end = (long) start + count;
      if (end - 1 >= int.MaxValue)
        throw new ArgumentOutOfRangeException("count", count, null);

      return RangeYield(start, end);
    }

    private static IEnumerable<int> RangeYield(int start, long end)
    {
      for (var i = start; i < end; i++)
        yield return i;
    }

    public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
    {
      if (count < 0) throw new ArgumentOutOfRangeException("count", count, null);

      return RepeatYield(element, count);
    }

    private static IEnumerable<TResult> RepeatYield<TResult>(TResult element, int count)
    {
      for (var i = 0; i < count; i++)
        yield return element;
    }

    public static IEnumerable<TSource> Where<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      CheckNotNull(source, "source");
      CheckNotNull(predicate, "predicate");

      return WhereYield(source, predicate);
    }

    private static IEnumerable<TSource> WhereYield<TSource>(
      IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      foreach (var item in source)
        if (predicate(item))
          yield return item;
    }

    public static IEnumerable<TSource> Where<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int, bool> predicate)
    {
      CheckNotNull(source, "source");
      CheckNotNull(predicate, "predicate");

      return WhereYield(source, predicate);
    }

    private static IEnumerable<TSource> WhereYield<TSource>(
      IEnumerable<TSource> source,
      Func<TSource, int, bool> predicate)
    {
      var i = 0;
      foreach (var item in source)
        if (predicate(item, i++))
          yield return item;
    }

    public static IEnumerable<TResult> Select<TSource, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, TResult> selector)
    {
      CheckNotNull(source, "source");
      CheckNotNull(selector, "selector");

      return SelectYield(source, selector);
    }

    private static IEnumerable<TResult> SelectYield<TSource, TResult>(
      IEnumerable<TSource> source,
      Func<TSource, TResult> selector)
    {
      foreach (var item in source)
        yield return selector(item);
    }

    public static IEnumerable<TResult> Select<TSource, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, int, TResult> selector)
    {
      CheckNotNull(source, "source");
      CheckNotNull(selector, "selector");

      return SelectYield(source, selector);
    }

    private static IEnumerable<TResult> SelectYield<TSource, TResult>(
      IEnumerable<TSource> source,
      Func<TSource, int, TResult> selector)
    {
      var i = 0;
      foreach (var item in source)
        yield return selector(item, i++);
    }

    public static IEnumerable<TResult> SelectMany<TSource, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, IEnumerable<TResult>> selector)
    {
      CheckNotNull(selector, "selector");

      return source.SelectMany((item, i) => selector(item));
    }

    public static IEnumerable<TResult> SelectMany<TSource, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, int, IEnumerable<TResult>> selector)
    {
      CheckNotNull(selector, "selector");

      return source.SelectMany(selector, (item, subitem) => subitem);
    }

    public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, IEnumerable<TCollection>> collectionSelector,
      Func<TSource, TCollection, TResult> resultSelector)
    {
      CheckNotNull(collectionSelector, "collectionSelector");

      return source.SelectMany((item, i) => collectionSelector(item), resultSelector);
    }

    public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
      Func<TSource, TCollection, TResult> resultSelector)
    {
      CheckNotNull(source, "source");
      CheckNotNull(collectionSelector, "collectionSelector");
      CheckNotNull(resultSelector, "resultSelector");

      return SelectManyYield(source, collectionSelector, resultSelector);
    }

    private static IEnumerable<TResult> SelectManyYield<TSource, TCollection, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, int, IEnumerable<TCollection>> collectionSelector,
      Func<TSource, TCollection, TResult> resultSelector)
    {
      var i = 0;
      foreach (var item in source)
        foreach (var subitem in collectionSelector(item, i++))
          yield return resultSelector(item, subitem);
    }

    public static IEnumerable<TSource> TakeWhile<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      CheckNotNull(predicate, "predicate");

      return source.TakeWhile((item, i) => predicate(item));
    }

    public static IEnumerable<TSource> TakeWhile<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int, bool> predicate)
    {
      CheckNotNull(source, "source");
      CheckNotNull(predicate, "predicate");

      return TakeWhileYield(source, predicate);
    }

    private static IEnumerable<TSource> TakeWhileYield<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int, bool> predicate)
    {
      var i = 0;
      foreach (var item in source)
        if (predicate(item, i++))
          yield return item;
        else
          break;
    }

    private static class Futures<T>
    {
      public static readonly Func<T> Default = () => default(T);
      public static readonly Func<T> Undefined = () => { throw new InvalidOperationException(); };
    }

    private static TSource FirstImpl<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource> empty)
    {
      CheckNotNull(source, "source");
      MiscellaneousUtils.Assert(empty != null);

      var list = source as IList<TSource>; // optimized case for lists
      if (list != null)
        return list.Count > 0 ? list[0] : empty();

      using (var e = source.GetEnumerator()) // fallback for enumeration
        return e.MoveNext() ? e.Current : empty();
    }

    public static TSource First<TSource>(
      this IEnumerable<TSource> source)
    {
      return source.FirstImpl(Futures<TSource>.Undefined);
    }

    public static TSource First<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      return First(source.Where(predicate));
    }

    public static TSource FirstOrDefault<TSource>(
      this IEnumerable<TSource> source)
    {
      return source.FirstImpl(Futures<TSource>.Default);
    }

    public static TSource FirstOrDefault<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      return FirstOrDefault(source.Where(predicate));
    }

    private static TSource LastImpl<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource> empty)
    {
      CheckNotNull(source, "source");

      var list = source as IList<TSource>; // optimized case for lists
      if (list != null)
        return list.Count > 0 ? list[list.Count - 1] : empty();

      using (var e = source.GetEnumerator())
      {
        if (!e.MoveNext())
          return empty();

        var last = e.Current;
        while (e.MoveNext())
          last = e.Current;

        return last;
      }
    }
    public static TSource Last<TSource>(
      this IEnumerable<TSource> source)
    {
      return source.LastImpl(Futures<TSource>.Undefined);
    }

    public static TSource Last<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      return Last(source.Where(predicate));
    }

    public static TSource LastOrDefault<TSource>(
      this IEnumerable<TSource> source)
    {
      return source.LastImpl(Futures<TSource>.Default);
    }

    public static TSource LastOrDefault<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      return LastOrDefault(source.Where(predicate));
    }

    private static TSource SingleImpl<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource> empty)
    {
      CheckNotNull(source, "source");

      using (var e = source.GetEnumerator())
      {
        if (e.MoveNext())
        {
          var single = e.Current;
          if (!e.MoveNext())
            return single;

          throw new InvalidOperationException();
        }

        return empty();
      }
    }

    public static TSource Single<TSource>(
      this IEnumerable<TSource> source)
    {
      return source.SingleImpl(Futures<TSource>.Undefined);
    }

    public static TSource Single<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      return Single(source.Where(predicate));
    }

    public static TSource SingleOrDefault<TSource>(
      this IEnumerable<TSource> source)
    {
      return source.SingleImpl(Futures<TSource>.Default);
    }

    public static TSource SingleOrDefault<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      return SingleOrDefault(source.Where(predicate));
    }

    public static TSource ElementAt<TSource>(
      this IEnumerable<TSource> source,
      int index)
    {
      CheckNotNull(source, "source");

      if (index < 0)
        throw new ArgumentOutOfRangeException("index", index, null);

      var list = source as IList<TSource>;
      if (list != null)
        return list[index];

      try
      {
        return source.SkipWhile((item, i) => i < index).First();
      }
      catch (InvalidOperationException) // if thrown by First
      {
        throw new ArgumentOutOfRangeException("index", index, null);
      }
    }

    public static TSource ElementAtOrDefault<TSource>(
      this IEnumerable<TSource> source,
      int index)
    {
      CheckNotNull(source, "source");

      if (index < 0)
        return default(TSource);

      var list = source as IList<TSource>;
      if (list != null)
        return index < list.Count ? list[index] : default(TSource);

      return source.SkipWhile((item, i) => i < index).FirstOrDefault();
    }

    public static IEnumerable<TSource> Reverse<TSource>(
      this IEnumerable<TSource> source)
    {
      CheckNotNull(source, "source");

      return ReverseYield(source);
    }

    private static IEnumerable<TSource> ReverseYield<TSource>(IEnumerable<TSource> source)
    {
      var stack = new Stack<TSource>(source);

      foreach (var item in stack)
        yield return item;
    }

    public static IEnumerable<TSource> Take<TSource>(
      this IEnumerable<TSource> source,
      int count)
    {
      return source.Where((item, i) => i < count);
    }

    public static IEnumerable<TSource> Skip<TSource>(
      this IEnumerable<TSource> source,
      int count)
    {
      return source.Where((item, i) => i >= count);
    }

    public static IEnumerable<TSource> SkipWhile<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      CheckNotNull(predicate, "predicate");

      return source.SkipWhile((item, i) => predicate(item));
    }

    public static IEnumerable<TSource> SkipWhile<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int, bool> predicate)
    {
      CheckNotNull(source, "source");
      CheckNotNull(predicate, "predicate");

      return SkipWhileYield(source, predicate);
    }

    private static IEnumerable<TSource> SkipWhileYield<TSource>(
      IEnumerable<TSource> source,
      Func<TSource, int, bool> predicate)
    {
      using (var e = source.GetEnumerator())
      {
        for (var i = 0;; i++)
        {
          if (!e.MoveNext())
            yield break;

          if (!predicate(e.Current, i))
            break;
        }

        do
        {
          yield return e.Current;
        } while (e.MoveNext());
      }
    }

    public static int Count<TSource>(
      this IEnumerable<TSource> source)
    {
      CheckNotNull(source, "source");

      var collection = source as ICollection;
      if (collection != null)
      {
        return collection.Count;
      }

      using (var en = source.GetEnumerator())
      {
        int count = 0;
        while (en.MoveNext())
        {
          ++count;
        }

        return count;
      }
    }

    public static int Count<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      return Count(source.Where(predicate));
    }

    public static long LongCount<TSource>(
      this IEnumerable<TSource> source)
    {
      CheckNotNull(source, "source");

      var array = source as Array;
      return array != null
               ? array.LongLength
               : source.Aggregate(0L, (count, item) => count + 1);
    }

    public static long LongCount<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      return LongCount(source.Where(predicate));
    }

    public static IEnumerable<TSource> Concat<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second)
    {
      CheckNotNull(first, "first");
      CheckNotNull(second, "second");

      return ConcatYield(first, second);
    }

    private static IEnumerable<TSource> ConcatYield<TSource>(
      IEnumerable<TSource> first,
      IEnumerable<TSource> second)
    {
      foreach (var item in first)
        yield return item;

      foreach (var item in second)
        yield return item;
    }

    public static List<TSource> ToList<TSource>(
      this IEnumerable<TSource> source)
    {
      CheckNotNull(source, "source");

      return new List<TSource>(source);
    }

      public static TSource[] ToArray<TSource>(
        this IEnumerable<TSource> source)
      {
          IList<TSource> ilist = source as IList<TSource>;
          if (ilist != null)
          {
              TSource[] array = new TSource[ilist.Count];
              ilist.CopyTo(array, 0);
              return array;
          }

          return source.ToList().ToArray();
      }

    public static IEnumerable<TSource> Distinct<TSource>(
      this IEnumerable<TSource> source)
    {
      return Distinct(source, /* comparer */ null);
    }

    public static IEnumerable<TSource> Distinct<TSource>(
      this IEnumerable<TSource> source,
      IEqualityComparer<TSource> comparer)
    {
      CheckNotNull(source, "source");

      return DistinctYield(source, comparer);
    }

    private static IEnumerable<TSource> DistinctYield<TSource>(
      IEnumerable<TSource> source,
      IEqualityComparer<TSource> comparer)
    {
      var set = new Dictionary<TSource, object>(comparer);
      var gotNull = false;

      foreach (var item in source)
      {
        if (item == null)
        {
          if (gotNull)
            continue;
          gotNull = true;
        }
        else
        {
          if (set.ContainsKey(item))
            continue;
          set.Add(item, null);
        }

        yield return item;
      }
    }

    public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector)
    {
      return ToLookup(source, keySelector, e => e, /* comparer */ null);
    }

    public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      IEqualityComparer<TKey> comparer)
    {
      return ToLookup(source, keySelector, e => e, comparer);
    }

    public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TElement> elementSelector)
    {
      return ToLookup(source, keySelector, elementSelector, /* comparer */ null);
    }

    public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TElement> elementSelector,
      IEqualityComparer<TKey> comparer)
    {
      CheckNotNull(source, "source");
      CheckNotNull(keySelector, "keySelector");
      CheckNotNull(elementSelector, "elementSelector");

      var lookup = new Lookup<TKey, TElement>(comparer);

      foreach (var item in source)
      {
        var key = keySelector(item);

        var grouping = (Grouping<TKey, TElement>) lookup.Find(key);
        if (grouping == null)
        {
          grouping = new Grouping<TKey, TElement>(key);
          lookup.Add(grouping);
        }

        grouping.Add(elementSelector(item));
      }

      return lookup;
    }

    public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector)
    {
      return GroupBy(source, keySelector, /* comparer */ null);
    }

    public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      IEqualityComparer<TKey> comparer)
    {
      return GroupBy(source, keySelector, e => e, comparer);
    }

    public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TElement> elementSelector)
    {
      return GroupBy(source, keySelector, elementSelector, /* comparer */ null);
    }

    public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TElement> elementSelector,
      IEqualityComparer<TKey> comparer)
    {
      CheckNotNull(source, "source");
      CheckNotNull(keySelector, "keySelector");
      CheckNotNull(elementSelector, "elementSelector");

      return ToLookup(source, keySelector, elementSelector, comparer);
    }

    public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TKey, IEnumerable<TSource>, TResult> resultSelector)
    {
      return GroupBy(source, keySelector, resultSelector, /* comparer */ null);
    }

    public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TKey, IEnumerable<TSource>, TResult> resultSelector,
      IEqualityComparer<TKey> comparer)
    {
      CheckNotNull(source, "source");
      CheckNotNull(keySelector, "keySelector");
      CheckNotNull(resultSelector, "resultSelector");

      return ToLookup(source, keySelector, comparer).Select(g => resultSelector(g.Key, g));
    }

    public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TElement> elementSelector,
      Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
    {
      return GroupBy(source, keySelector, elementSelector, resultSelector, /* comparer */ null);
    }

    public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TElement> elementSelector,
      Func<TKey, IEnumerable<TElement>, TResult> resultSelector,
      IEqualityComparer<TKey> comparer)
    {
      CheckNotNull(source, "source");
      CheckNotNull(keySelector, "keySelector");
      CheckNotNull(elementSelector, "elementSelector");
      CheckNotNull(resultSelector, "resultSelector");

      return ToLookup(source, keySelector, elementSelector, comparer)
        .Select(g => resultSelector(g.Key, g));
    }

    public static TSource Aggregate<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, TSource, TSource> func)
    {
      CheckNotNull(source, "source");
      CheckNotNull(func, "func");

      using (var e = source.GetEnumerator())
      {
        if (!e.MoveNext())
          throw new InvalidOperationException();

        return e.Renumerable().Skip(1).Aggregate(e.Current, func);
      }
    }

    public static TAccumulate Aggregate<TSource, TAccumulate>(
      this IEnumerable<TSource> source,
      TAccumulate seed,
      Func<TAccumulate, TSource, TAccumulate> func)
    {
      return Aggregate(source, seed, func, r => r);
    }

    public static TResult Aggregate<TSource, TAccumulate, TResult>(
      this IEnumerable<TSource> source,
      TAccumulate seed,
      Func<TAccumulate, TSource, TAccumulate> func,
      Func<TAccumulate, TResult> resultSelector)
    {
      CheckNotNull(source, "source");
      CheckNotNull(func, "func");
      CheckNotNull(resultSelector, "resultSelector");

      var result = seed;

      foreach (var item in source)
        result = func(result, item);

      return resultSelector(result);
    }

    public static IEnumerable<TSource> Union<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second)
    {
      return Union(first, second, /* comparer */ null);
    }

    public static IEnumerable<TSource> Union<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second,
      IEqualityComparer<TSource> comparer)
    {
      return first.Concat(second).Distinct(comparer);
    }

    public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
      this IEnumerable<TSource> source)
    {
      return source.DefaultIfEmpty(default(TSource));
    }

    public static IEnumerable<TSource> DefaultIfEmpty<TSource>(
      this IEnumerable<TSource> source,
      TSource defaultValue)
    {
      CheckNotNull(source, "source");

      return DefaultIfEmptyYield(source, defaultValue);
    }

    private static IEnumerable<TSource> DefaultIfEmptyYield<TSource>(
      IEnumerable<TSource> source,
      TSource defaultValue)
    {
      using (var e = source.GetEnumerator())
      {
        if (!e.MoveNext())
          yield return defaultValue;
        else
          do
          {
            yield return e.Current;
          } while (e.MoveNext());
      }
    }

    public static bool All<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      CheckNotNull(source, "source");
      CheckNotNull(predicate, "predicate");

      foreach (var item in source)
        if (!predicate(item))
          return false;

      return true;
    }

    public static bool Any<TSource>(
      this IEnumerable<TSource> source)
    {
      CheckNotNull(source, "source");

      using (var e = source.GetEnumerator())
        return e.MoveNext();
    }

    public static bool Any<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, bool> predicate)
    {
      foreach (TSource item in source)
      {
        if (predicate(item))
        {
          return true;
        }
      }

      return false;
    }

    public static bool Contains<TSource>(
      this IEnumerable<TSource> source,
      TSource value)
    {
      return source.Contains(value, /* comparer */ null);
    }

    public static bool Contains<TSource>(
      this IEnumerable<TSource> source,
      TSource value,
      IEqualityComparer<TSource> comparer)
    {
      CheckNotNull(source, "source");

      if (comparer == null)
      {
        var collection = source as ICollection<TSource>;
        if (collection != null)
          return collection.Contains(value);
      }

      comparer = comparer ?? EqualityComparer<TSource>.Default;
      return source.Any(item => comparer.Equals(item, value));
    }

    public static bool SequenceEqual<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second)
    {
      return first.SequenceEqual(second, /* comparer */ null);
    }

    public static bool SequenceEqual<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second,
      IEqualityComparer<TSource> comparer)
    {
      CheckNotNull(first, "first");
      CheckNotNull(second, "second");

      comparer = comparer ?? EqualityComparer<TSource>.Default;

      using (IEnumerator<TSource> lhs = first.GetEnumerator(),
                                  rhs = second.GetEnumerator())
      {
        do
        {
          if (!lhs.MoveNext())
            return !rhs.MoveNext();

          if (!rhs.MoveNext())
            return false;
        } while (comparer.Equals(lhs.Current, rhs.Current));
      }

      return false;
    }

    private static TSource MinMaxImpl<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, TSource, bool> lesser)
    {
      CheckNotNull(source, "source");
      MiscellaneousUtils.Assert(lesser != null);

      return source.Aggregate((a, item) => lesser(a, item) ? a : item);
    }

    private static TSource? MinMaxImpl<TSource>(
      this IEnumerable<TSource?> source,
      TSource? seed, Func<TSource?, TSource?, bool> lesser) where TSource : struct
    {
      CheckNotNull(source, "source");
      MiscellaneousUtils.Assert(lesser != null);

      return source.Aggregate(seed, (a, item) => lesser(a, item) ? a : item);
    }

    public static TSource Min<TSource>(
      this IEnumerable<TSource> source)
    {
      var comparer = Comparer<TSource>.Default;
      return source.MinMaxImpl((x, y) => comparer.Compare(x, y) < 0);
    }

    public static TResult Min<TSource, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, TResult> selector)
    {
      return source.Select(selector).Min();
    }

    public static TSource Max<TSource>(
      this IEnumerable<TSource> source)
    {
      var comparer = Comparer<TSource>.Default;
      return source.MinMaxImpl((x, y) => comparer.Compare(x, y) > 0);
    }

    public static TResult Max<TSource, TResult>(
      this IEnumerable<TSource> source,
      Func<TSource, TResult> selector)
    {
      return source.Select(selector).Max();
    }

    private static IEnumerable<T> Renumerable<T>(this IEnumerator<T> e)
    {
      MiscellaneousUtils.Assert(e != null);

      do
      {
        yield return e.Current;
      } while (e.MoveNext());
    }

    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector)
    {
      return source.OrderBy(keySelector, /* comparer */ null);
    }

    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      IComparer<TKey> comparer)
    {
      CheckNotNull(source, "source");
      CheckNotNull(keySelector, "keySelector");

      return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, /* descending */ false);
    }

    public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector)
    {
      return source.OrderByDescending(keySelector, /* comparer */ null);
    }

    public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      IComparer<TKey> comparer)
    {
      CheckNotNull(source, "source");
      CheckNotNull(source, "keySelector");

      return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, /* descending */ true);
    }

    public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
      this IOrderedEnumerable<TSource> source,
      Func<TSource, TKey> keySelector)
    {
      return source.ThenBy(keySelector, /* comparer */ null);
    }

    public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
      this IOrderedEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      IComparer<TKey> comparer)
    {
      CheckNotNull(source, "source");

      return source.CreateOrderedEnumerable(keySelector, comparer, /* descending */ false);
    }

    public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
      this IOrderedEnumerable<TSource> source,
      Func<TSource, TKey> keySelector)
    {
      return source.ThenByDescending(keySelector, /* comparer */ null);
    }

    public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
      this IOrderedEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      IComparer<TKey> comparer)
    {
      CheckNotNull(source, "source");

      return source.CreateOrderedEnumerable(keySelector, comparer, /* descending */ true);
    }

    private static IEnumerable<TSource> IntersectExceptImpl<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second,
      IEqualityComparer<TSource> comparer,
      bool flag)
    {
      CheckNotNull(first, "first");
      CheckNotNull(second, "second");

      var keys = new List<TSource>();
      var flags = new Dictionary<TSource, bool>(comparer);

      foreach (var item in first.Where(k => !flags.ContainsKey(k)))
      {
        flags.Add(item, !flag);
        keys.Add(item);
      }

      foreach (var item in second.Where(flags.ContainsKey))
        flags[item] = flag;

      return keys.Where(item => flags[item]);
    }

    public static IEnumerable<TSource> Intersect<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second)
    {
      return first.Intersect(second, /* comparer */ null);
    }

    public static IEnumerable<TSource> Intersect<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second,
      IEqualityComparer<TSource> comparer)
    {
      return IntersectExceptImpl(first, second, comparer, /* flag */ true);
    }

    public static IEnumerable<TSource> Except<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second)
    {
      return first.Except(second, /* comparer */ null);
    }

    public static IEnumerable<TSource> Except<TSource>(
      this IEnumerable<TSource> first,
      IEnumerable<TSource> second,
      IEqualityComparer<TSource> comparer)
    {
      return IntersectExceptImpl(first, second, comparer, /* flag */ false);
    }

    public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector)
    {
      return source.ToDictionary(keySelector, /* comparer */ null);
    }

    public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      IEqualityComparer<TKey> comparer)
    {
      return source.ToDictionary(keySelector, e => e);
    }

    public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TElement> elementSelector)
    {
      return source.ToDictionary(keySelector, elementSelector, /* comparer */ null);
    }

    public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TElement> elementSelector,
      IEqualityComparer<TKey> comparer)
    {
      CheckNotNull(source, "source");
      CheckNotNull(keySelector, "keySelector");
      CheckNotNull(elementSelector, "elementSelector");

      var dict = new Dictionary<TKey, TElement>(comparer);

      foreach (var item in source)
      {

        dict.Add(keySelector(item), elementSelector(item));
      }

      return dict;
    }

    public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
      this IEnumerable<TOuter> outer,
      IEnumerable<TInner> inner,
      Func<TOuter, TKey> outerKeySelector,
      Func<TInner, TKey> innerKeySelector,
      Func<TOuter, TInner, TResult> resultSelector)
    {
      return outer.Join(inner, outerKeySelector, innerKeySelector, resultSelector, /* comparer */ null);
    }

    public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
      this IEnumerable<TOuter> outer,
      IEnumerable<TInner> inner,
      Func<TOuter, TKey> outerKeySelector,
      Func<TInner, TKey> innerKeySelector,
      Func<TOuter, TInner, TResult> resultSelector,
      IEqualityComparer<TKey> comparer)
    {
      CheckNotNull(outer, "outer");
      CheckNotNull(inner, "inner");
      CheckNotNull(outerKeySelector, "outerKeySelector");
      CheckNotNull(innerKeySelector, "innerKeySelector");
      CheckNotNull(resultSelector, "resultSelector");

      var lookup = inner.ToLookup(innerKeySelector, comparer);

      return
        from o in outer
        from i in lookup[outerKeySelector(o)]
        select resultSelector(o, i);
    }

    public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
      this IEnumerable<TOuter> outer,
      IEnumerable<TInner> inner,
      Func<TOuter, TKey> outerKeySelector,
      Func<TInner, TKey> innerKeySelector,
      Func<TOuter, IEnumerable<TInner>, TResult> resultSelector)
    {
      return outer.GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, /* comparer */ null);
    }

    public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(
      this IEnumerable<TOuter> outer,
      IEnumerable<TInner> inner,
      Func<TOuter, TKey> outerKeySelector,
      Func<TInner, TKey> innerKeySelector,
      Func<TOuter, IEnumerable<TInner>, TResult> resultSelector,
      IEqualityComparer<TKey> comparer)
    {
      CheckNotNull(outer, "outer");
      CheckNotNull(inner, "inner");
      CheckNotNull(outerKeySelector, "outerKeySelector");
      CheckNotNull(innerKeySelector, "innerKeySelector");
      CheckNotNull(resultSelector, "resultSelector");

      var lookup = inner.ToLookup(innerKeySelector, comparer);
      return outer.Select(o => resultSelector(o, lookup[outerKeySelector(o)]));
    }

    [DebuggerStepThrough]
    private static void CheckNotNull<T>(T value, string name) where T : class
    {
      if (value == null)
        throw new ArgumentNullException(name);
    }

    private static class Sequence<T>
    {
      public static readonly IEnumerable<T> Empty = new T[0];
    }

    private sealed class Grouping<K, V> : List<V>, IGrouping<K, V>
    {
      internal Grouping(K key)
      {
        Key = key;
      }

      public K Key { get; private set; }
    }
  }

  internal partial class Enumerable
  {

    public static int Sum(
      this IEnumerable<int> source)
    {
      CheckNotNull(source, "source");

      int sum = 0;
      foreach (var num in source)
        sum = checked(sum + num);

      return sum;
    }

    public static int Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int> selector)
    {
      return source.Select(selector).Sum();
    }

    public static double Average(
      this IEnumerable<int> source)
    {
      CheckNotNull(source, "source");

      long sum = 0;
      long count = 0;

      foreach (var num in source)
        checked
        {
          sum += (int) num;
          count++;
        }

      if (count == 0)
        throw new InvalidOperationException();

      return (double) sum/count;
    }

    public static double Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int> selector)
    {
      return source.Select(selector).Average();
    }

    public static int? Sum(
      this IEnumerable<int?> source)
    {
      CheckNotNull(source, "source");

      int sum = 0;
      foreach (var num in source)
        sum = checked(sum + (num ?? 0));

      return sum;
    }

    public static int? Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int?> selector)
    {
      return source.Select(selector).Sum();
    }

    public static double? Average(
      this IEnumerable<int?> source)
    {
      CheckNotNull(source, "source");

      long sum = 0;
      long count = 0;

      foreach (var num in source.Where(n => n != null))
        checked
        {
          sum += (int) num;
          count++;
        }

      if (count == 0)
        return null;

      return (double?) sum/count;
    }

    public static double? Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int?> selector)
    {
      return source.Select(selector).Average();
    }

    public static int? Min(
      this IEnumerable<int?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null), null, (min, x) => min < x);
    }

    public static int? Min<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int?> selector)
    {
      return source.Select(selector).Min();
    }

    public static int? Max(
      this IEnumerable<int?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null),
                        null, (max, x) => x == null || (max != null && x.Value < max.Value));
    }

    public static int? Max<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, int?> selector)
    {
      return source.Select(selector).Max();
    }

    public static long Sum(
      this IEnumerable<long> source)
    {
      CheckNotNull(source, "source");

      long sum = 0;
      foreach (var num in source)
        sum = checked(sum + num);

      return sum;
    }

    public static long Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, long> selector)
    {
      return source.Select(selector).Sum();
    }

    public static double Average(
      this IEnumerable<long> source)
    {
      CheckNotNull(source, "source");

      long sum = 0;
      long count = 0;

      foreach (var num in source)
        checked
        {
          sum += (long) num;
          count++;
        }

      if (count == 0)
        throw new InvalidOperationException();

      return (double) sum/count;
    }

    public static double Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, long> selector)
    {
      return source.Select(selector).Average();
    }

    public static long? Sum(
      this IEnumerable<long?> source)
    {
      CheckNotNull(source, "source");

      long sum = 0;
      foreach (var num in source)
        sum = checked(sum + (num ?? 0));

      return sum;
    }

    public static long? Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, long?> selector)
    {
      return source.Select(selector).Sum();
    }

    public static double? Average(
      this IEnumerable<long?> source)
    {
      CheckNotNull(source, "source");

      long sum = 0;
      long count = 0;

      foreach (var num in source.Where(n => n != null))
        checked
        {
          sum += (long) num;
          count++;
        }

      if (count == 0)
        return null;

      return (double?) sum/count;
    }

    public static double? Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, long?> selector)
    {
      return source.Select(selector).Average();
    }

    public static long? Min(
      this IEnumerable<long?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null), null, (min, x) => min < x);
    }

    public static long? Min<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, long?> selector)
    {
      return source.Select(selector).Min();
    }

    public static long? Max(
      this IEnumerable<long?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null),
                        null, (max, x) => x == null || (max != null && x.Value < max.Value));
    }

    public static long? Max<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, long?> selector)
    {
      return source.Select(selector).Max();
    }

    public static float Sum(
      this IEnumerable<float> source)
    {
      CheckNotNull(source, "source");

      float sum = 0;
      foreach (var num in source)
        sum = checked(sum + num);

      return sum;
    }

    public static float Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, float> selector)
    {
      return source.Select(selector).Sum();
    }

    public static float Average(
      this IEnumerable<float> source)
    {
      CheckNotNull(source, "source");

      float sum = 0;
      long count = 0;

      foreach (var num in source)
        checked
        {
          sum += (float) num;
          count++;
        }

      if (count == 0)
        throw new InvalidOperationException();

      return (float) sum/count;
    }

    public static float Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, float> selector)
    {
      return source.Select(selector).Average();
    }

    public static float? Sum(
      this IEnumerable<float?> source)
    {
      CheckNotNull(source, "source");

      float sum = 0;
      foreach (var num in source)
        sum = checked(sum + (num ?? 0));

      return sum;
    }

    public static float? Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, float?> selector)
    {
      return source.Select(selector).Sum();
    }

    public static float? Average(
      this IEnumerable<float?> source)
    {
      CheckNotNull(source, "source");

      float sum = 0;
      long count = 0;

      foreach (var num in source.Where(n => n != null))
        checked
        {
          sum += (float) num;
          count++;
        }

      if (count == 0)
        return null;

      return (float?) sum/count;
    }

    public static float? Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, float?> selector)
    {
      return source.Select(selector).Average();
    }

    public static float? Min(
      this IEnumerable<float?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null), null, (min, x) => min < x);
    }

    public static float? Min<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, float?> selector)
    {
      return source.Select(selector).Min();
    }

    public static float? Max(
      this IEnumerable<float?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null),
                        null, (max, x) => x == null || (max != null && x.Value < max.Value));
    }

    public static float? Max<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, float?> selector)
    {
      return source.Select(selector).Max();
    }

    public static double Sum(
      this IEnumerable<double> source)
    {
      CheckNotNull(source, "source");

      double sum = 0;
      foreach (var num in source)
        sum = checked(sum + num);

      return sum;
    }

    public static double Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, double> selector)
    {
      return source.Select(selector).Sum();
    }

    public static double Average(
      this IEnumerable<double> source)
    {
      CheckNotNull(source, "source");

      double sum = 0;
      long count = 0;

      foreach (var num in source)
        checked
        {
          sum += (double) num;
          count++;
        }

      if (count == 0)
        throw new InvalidOperationException();

      return (double) sum/count;
    }

    public static double Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, double> selector)
    {
      return source.Select(selector).Average();
    }

    public static double? Sum(
      this IEnumerable<double?> source)
    {
      CheckNotNull(source, "source");

      double sum = 0;
      foreach (var num in source)
        sum = checked(sum + (num ?? 0));

      return sum;
    }

    public static double? Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, double?> selector)
    {
      return source.Select(selector).Sum();
    }

    public static double? Average(
      this IEnumerable<double?> source)
    {
      CheckNotNull(source, "source");

      double sum = 0;
      long count = 0;

      foreach (var num in source.Where(n => n != null))
        checked
        {
          sum += (double) num;
          count++;
        }

      if (count == 0)
        return null;

      return (double?) sum/count;
    }

    public static double? Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, double?> selector)
    {
      return source.Select(selector).Average();
    }

    public static double? Min(
      this IEnumerable<double?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null), null, (min, x) => min < x);
    }

    public static double? Min<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, double?> selector)
    {
      return source.Select(selector).Min();
    }

    public static double? Max(
      this IEnumerable<double?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null),
                        null, (max, x) => x == null || (max != null && x.Value < max.Value));
    }

    public static double? Max<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, double?> selector)
    {
      return source.Select(selector).Max();
    }

    public static decimal Sum(
      this IEnumerable<decimal> source)
    {
      CheckNotNull(source, "source");

      decimal sum = 0;
      foreach (var num in source)
        sum = checked(sum + num);

      return sum;
    }

    public static decimal Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, decimal> selector)
    {
      CheckNotNull(source, "source");
      CheckNotNull(selector, "selector");

      decimal sum = 0;
      foreach (TSource item in source)
      {
        sum += selector(item);
      }

        return sum;
    }

    public static decimal Average(
      this IEnumerable<decimal> source)
    {
      CheckNotNull(source, "source");

      decimal sum = 0;
      long count = 0;

      foreach (var num in source)
        checked
        {
          sum += (decimal) num;
          count++;
        }

      if (count == 0)
        throw new InvalidOperationException();

      return (decimal) sum/count;
    }

    public static decimal Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, decimal> selector)
    {
      return source.Select(selector).Average();
    }

    public static decimal? Sum(
      this IEnumerable<decimal?> source)
    {
      CheckNotNull(source, "source");

      decimal sum = 0;
      foreach (var num in source)
        sum = checked(sum + (num ?? 0));

      return sum;
    }

    public static decimal? Sum<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, decimal?> selector)
    {
      return source.Select(selector).Sum();
    }

    public static decimal? Average(
      this IEnumerable<decimal?> source)
    {
      CheckNotNull(source, "source");

      decimal sum = 0;
      long count = 0;

      foreach (var num in source.Where(n => n != null))
        checked
        {
          sum += (decimal) num;
          count++;
        }

      if (count == 0)
        return null;

      return (decimal?) sum/count;
    }

    public static decimal? Average<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, decimal?> selector)
    {
      return source.Select(selector).Average();
    }

    public static decimal? Min(
      this IEnumerable<decimal?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null), null, (min, x) => min < x);
    }

    public static decimal? Min<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, decimal?> selector)
    {
      return source.Select(selector).Min();
    }

    public static decimal? Max(
      this IEnumerable<decimal?> source)
    {
      CheckNotNull(source, "source");

      return MinMaxImpl(source.Where(x => x != null),
                        null, (max, x) => x == null || (max != null && x.Value < max.Value));
    }

    public static decimal? Max<TSource>(
      this IEnumerable<TSource> source,
      Func<TSource, decimal?> selector)
    {
      return source.Select(selector).Max();
    }
  }
  internal partial interface IGrouping<TKey, TElement> : IEnumerable<TElement>
  {

    TKey Key { get; }
  }
  internal partial interface ILookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>
  {
    bool Contains(TKey key);
    int Count { get; }
    IEnumerable<TElement> this[TKey key] { get; }
  }
  internal partial interface IOrderedEnumerable<TElement> : IEnumerable<TElement>
  {

    IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
      Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
  }
  internal sealed class Lookup<TKey, TElement> : ILookup<TKey, TElement>
  {
    private readonly Dictionary<TKey, IGrouping<TKey, TElement>> _map;

    internal Lookup(IEqualityComparer<TKey> comparer)
    {
      _map = new Dictionary<TKey, IGrouping<TKey, TElement>>(comparer);
    }

    internal void Add(IGrouping<TKey, TElement> item)
    {
      _map.Add(item.Key, item);
    }

    internal IEnumerable<TElement> Find(TKey key)
    {
      IGrouping<TKey, TElement> grouping;
      return _map.TryGetValue(key, out grouping) ? grouping : null;
    }

    public int Count => _map.Count;

    public IEnumerable<TElement> this[TKey key]
    {
      get
      {
        IGrouping<TKey, TElement> result;
        return _map.TryGetValue(key, out result) ? result : Enumerable.Empty<TElement>();
      }
    }

    public bool Contains(TKey key)
    {
      return _map.ContainsKey(key);
    }

    public IEnumerable<TResult> ApplyResultSelector<TResult>(
      Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
    {
      if (resultSelector == null)
        throw new ArgumentNullException("resultSelector");

      foreach (var pair in _map)
        yield return resultSelector(pair.Key, pair.Value);
    }

    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
    {
      return _map.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  internal sealed class OrderedEnumerable<T, K> : IOrderedEnumerable<T>
  {
    private readonly IEnumerable<T> _source;
    private readonly List<Comparison<T>> _comparisons;

    public OrderedEnumerable(IEnumerable<T> source,
                             Func<T, K> keySelector, IComparer<K> comparer, bool descending) :
                               this(source, null, keySelector, comparer, descending)
    {
    }

    private OrderedEnumerable(IEnumerable<T> source, List<Comparison<T>> comparisons,
                              Func<T, K> keySelector, IComparer<K> comparer, bool descending)
    {
      if (source == null) throw new ArgumentNullException("source");
      if (keySelector == null) throw new ArgumentNullException("keySelector");

      _source = source;

      comparer = comparer ?? Comparer<K>.Default;

      if (comparisons == null)
        comparisons = new List<Comparison<T>>( /* capacity */ 4);

      comparisons.Add((x, y)
                      => (descending ? -1 : 1)*comparer.Compare(keySelector(x), keySelector(y)));

      _comparisons = comparisons;
    }

    public IOrderedEnumerable<T> CreateOrderedEnumerable<KK>(
      Func<T, KK> keySelector, IComparer<KK> comparer, bool descending)
    {
      return new OrderedEnumerable<T, KK>(_source, _comparisons, keySelector, comparer, descending);
    }

    public IEnumerator<T> GetEnumerator()
    {

      var list = _source.Select(new Func<T, int, Tuple<T, int>>(TagPosition)).ToList();

      list.Sort((x, y) =>
        {

          var comparisons = _comparisons;
          for (var i = 0; i < comparisons.Count; i++)
          {
            var result = comparisons[i](x.First, y.First);
            if (result != 0)
              return result;
          }

          return x.Second.CompareTo(y.Second);
        });

      return list.Select(new Func<Tuple<T, int>, T>(GetFirst)).GetEnumerator();

    }

    private static Tuple<T, int> TagPosition(T e, int i)
    {
      return new Tuple<T, int>(e, i);
    }

    private static T GetFirst(Tuple<T, int> pv)
    {
      return pv.First;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  [Serializable]
  internal readonly struct Tuple<TFirst, TSecond> : IEquatable<Tuple<TFirst, TSecond>>
  {
    public TFirst First { get; }
    public TSecond Second { get; }

    public Tuple(TFirst first, TSecond second)
      : this()
    {
      First = first;
      Second = second;
    }

    public override bool Equals(object obj)
    {
      return obj != null
             && obj is Tuple<TFirst, TSecond>
             && base.Equals((Tuple<TFirst, TSecond>) obj);
    }

    public bool Equals(Tuple<TFirst, TSecond> other)
    {
      return EqualityComparer<TFirst>.Default.Equals(other.First, First)
             && EqualityComparer<TSecond>.Default.Equals(other.Second, Second);
    }

    public override int GetHashCode()
    {
      var num = 0x7a2f0b42;
      num = (-1521134295*num) + EqualityComparer<TFirst>.Default.GetHashCode(First);
      return (-1521134295*num) + EqualityComparer<TSecond>.Default.GetHashCode(Second);
    }

    public override string ToString()
    {
      return string.Format(CultureInfo.InvariantCulture, @"{{ First = {0}, Second = {1} }}", First, Second);
    }
  }
}

namespace Simula.Scripting.Json.Serialization
{
#pragma warning disable 1591
  public delegate TResult Func<TResult>();

  public delegate TResult Func<T, TResult>(T a);

  public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);

  public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);

  public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

  public delegate void Action();

  public delegate void Action<T1, T2>(T1 arg1, T2 arg2);

  public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);

  public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
#pragma warning restore 1591
}

namespace System.Runtime.CompilerServices
{

  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
  internal sealed class ExtensionAttribute : Attribute { }
}

#endif