using System;
using System.Collections.Generic;
using System.ComponentModel;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SeawispHunter.RolePlay.Attributes;

namespace SeawispHunter.RolePlay.Attributes.Benchmarks {
  /**
|                       Method |       Mean |     Error |    StdDev |
|----------------------------- |-----------:|----------:|----------:|
|                 NewEventArgs |  5.3300 ns | 0.0589 ns | 0.0551 ns |
|        StaticCachedEventArgs |  0.3040 ns | 0.0104 ns | 0.0097 ns |
|         DictCachedEventArgsA | 16.5737 ns | 0.0813 ns | 0.0721 ns |
|         DictCachedEventArgsB | 16.5667 ns | 0.0745 ns | 0.0697 ns |
| ReadOnlyDictCachedEventArgsA | 12.8319 ns | 0.0709 ns | 0.0664 ns |
| ReadOnlyDictCachedEventArgsB | 15.6396 ns | 0.1075 ns | 0.0953 ns |
|  ConditionalStaticEventArgsA |  2.0519 ns | 0.0248 ns | 0.0232 ns |
|  ConditionalStaticEventArgsB | 10.7172 ns | 0.1023 ns | 0.0957 ns |
   */
  public class EventBenchmarks {
    IValue<int> a = new Value<int>(0);
    IReadOnlyValue<int> b;
    IReadOnlyValue<int> c;
    static PropertyChangedEventArgs args = new PropertyChangedEventArgs(nameof(a));
    static PropertyChangedEventArgs argsB = new PropertyChangedEventArgs(nameof(b));
    Dictionary<string, PropertyChangedEventArgs> cache = new Dictionary<string, PropertyChangedEventArgs>();
    Dictionary<string, PropertyChangedEventArgs> readOnlyCache = new Dictionary<string, PropertyChangedEventArgs>(10);
    public EventBenchmarks() {
      b = a.Select(x => x + 1);
      readOnlyCache[nameof(a)] = new PropertyChangedEventArgs(nameof(a));
    }

    PropertyChangedEventArgs GetReadOnlyDictCached(string name) {
      if (readOnlyCache.TryGetValue(name, out var args))
        return args;
      else
        return new PropertyChangedEventArgs(name);
    }

    PropertyChangedEventArgs GetDictCached(string name) {
      PropertyChangedEventArgs args;
      if (! cache.TryGetValue(name, out args))
        cache[name] = args = new PropertyChangedEventArgs(name);
      return args;
    }

    PropertyChangedEventArgs GetOneCached(string name) {
      return name == nameof(a)
        ? args
        : new PropertyChangedEventArgs(name);
    }

    PropertyChangedEventArgs GetTwoCached(string name) {
      return name == nameof(a)
        ? args
        : name == nameof(b)
            ? argsB
            : new PropertyChangedEventArgs(name);
    }

    [Benchmark]
    public PropertyChangedEventArgs NewEventArgs() => new PropertyChangedEventArgs(nameof(a));

    [Benchmark]
    public PropertyChangedEventArgs StaticCachedEventArgs() => args;

    [Benchmark]
    public PropertyChangedEventArgs DictCachedEventArgsA() => GetDictCached(nameof(a));
    [Benchmark]
    public PropertyChangedEventArgs DictCachedEventArgsB() => GetDictCached(nameof(b));

    [Benchmark]
    public PropertyChangedEventArgs ReadOnlyDictCachedEventArgsA() => GetReadOnlyDictCached(nameof(a));
    [Benchmark]
    public PropertyChangedEventArgs ReadOnlyDictCachedEventArgsB() => GetReadOnlyDictCached(nameof(b));

    [Benchmark]
    public PropertyChangedEventArgs ConditionalStaticEventArgsA() => GetOneCached(nameof(a));
    [Benchmark]
    public PropertyChangedEventArgs ConditionalStaticEventArgsB() => GetOneCached(nameof(b));

    [Benchmark]
    public PropertyChangedEventArgs TwoConditionalStaticEventArgsA() => GetTwoCached(nameof(a));
    [Benchmark]
    public PropertyChangedEventArgs TwoConditionalStaticEventArgsB() => GetTwoCached(nameof(b));
    [Benchmark]
    public PropertyChangedEventArgs TwoConditionalStaticEventArgsC() => GetTwoCached(nameof(c));
  }

  public class Program
  {
    public static void Main(string[] args)
    {
      var summary = BenchmarkRunner.Run<EventBenchmarks>();
    }
  }
}
