/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   This comment generated by code-cite[3].

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
   [4]: https://github.com/shanecelis/code-cite
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Runtime.CompilerServices;
#if NET6_0_OR_GREATER
using System.Numerics;
#endif
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif


namespace SeawispHunter.RolePlay.Attributes {

public static class Modifier {
  public static IModifier<T> FromFunc<T>(Func<T,T> func,
                                         out Action callOnChange,
                                         [CallerArgumentExpression("func")]
                                         string funcExpression = null)
    => new FuncModifier<T>(func, out callOnChange) { name = funcExpression };

  /** Create a modifier from the given function.

      var m = Modifier.FromFunc((int x) => x + 1);
      Console.WriteLine($"m = {m}"); // Prints: m = (int x) => x + 1
    */
  public static IModifier<T> FromFunc<T>(Func<T,T> func,
                                         [CallerArgumentExpression("func")]
                                         string funcExpression = null)
    => new FuncModifier<T>(func) { name = funcExpression };

  internal class FuncModifier<T> : ContextModifier<Func<T,T>, T> {
    public FuncModifier(Func<T,T> func, out Action callOnChange) : this(func) {
      callOnChange = () => OnChange(nameof(context));
    }
    public FuncModifier(Func<T,T> func) : base(func) {}

    public override T Modify(T given) => context(given);
    public override string ToString() => name ?? "?f()";
  }

  public static void EnableAfter<T>(this IModifier<T> modifier, TimeSpan timeSpan) {
    var timer = new Timer(Enable, modifier, timeSpan, Timeout.InfiniteTimeSpan);
    void Enable(object modifier) => ((IModifier<T>) modifier).enabled = true;
  }
  
  public static void DisableAfter<T>(this IModifier<T> modifier, TimeSpan timeSpan) {
    var timer = new Timer(Disable, modifier, timeSpan, Timeout.InfiniteTimeSpan);
    void Disable(object modifier) => ((IModifier<T>) modifier).enabled = false;
  }

  public static IModifier<S,T> WithContext<S,T>(this IModifier<T> modifier, S context)
    => new WrappedModifier<S,T>(context, modifier);

  internal class WrappedModifier<S,T> : ContextModifier<S,T>, IDecorator<IModifier<T>> {
    protected IModifier<T> inner;
    public IModifier<T> decorated => inner;
    public override bool enabled {
      get => inner.enabled;
      set => inner.enabled = value;
    }
    public WrappedModifier(S context, IModifier<T> inner) : base(context) {
      this.inner = inner;
      this.inner.PropertyChanged += Chain;
    }
    public override T Modify(T given) => inner.Modify(given);
    public override string ToString() => inner.ToString();
  }

#if UNITY_5_3_OR_NEWER
  public static IEnumerator EnableAfterCoroutine<T>(this IModifier<T> modifier, float seconds) {
    yield return new WaitForSeconds(seconds);
    modifier.enabled = true;
  }

  public static IEnumerator DisableAfterCoroutine<T>(this IModifier<T> modifier, float seconds) {
    yield return new WaitForSeconds(seconds);
    modifier.enabled = false;
  }
#endif

  public static ITarget<IList<IModifiable<T>>,T> TargetList<T>(this IModifier<T> modifier,
                                                                              int index,
                                                                              [CallerArgumentExpression("index")]
                                                                              string name = null) {
    return new ListTarget<T> { modifier = modifier, context = index, name = name };
  }

  public static ITarget<IDictionary<K,IModifiable<T>>,T> TargetDictionary<K,T>(this IModifier<T> modifier,
                                                                                              K key,
                                                                                              [CallerArgumentExpression("key")]
                                                                                              string name = null)
    => new DictionaryTarget<K,T> { modifier = modifier, context = key, name = name };

  public static ITarget<S,T> Target<S,T>(this IModifier<T> modifier,
                                                    Func<S,IModifiable<T>> target,
                                                   string name = null)
    => new FuncTarget<S,T> { modifier = modifier, context = target, name = name };

  internal abstract class BaseTarget<R,S,T> : ITarget<S, T> {
    public string name { get; init; }
    public R context { get; init; }
    // internal Func<S,IModifiable<T>> target { get; init; }
    public IModifier<T> modifier { get; init; }
    public abstract IModifiable<T> AppliesTo(S bag);
    public virtual string defaultName => context.ToString();
    public override string ToString() => name ?? defaultName;
  }

  /* The problem here is we don't know what this applies too. It's an opaque type. */
  internal class FuncTarget<S,T> : BaseTarget<Func<S,IModifiable<T>>,S, T> {
    public override IModifiable<T> AppliesTo(S bag) => context(bag);
  }

  internal class ListTarget<T> : BaseTarget<int, IList<IModifiable<T>>, T> {
    public override IModifiable<T> AppliesTo(IList<IModifiable<T>> bag) => bag[context];
  }

  internal class DictionaryTarget<K,T> : BaseTarget<K, IDictionary<K,IModifiable<T>>, T> {
    public override IModifiable<T> AppliesTo(IDictionary<K,IModifiable<T>> bag) => bag[context];
  }

#if NET6_0_OR_GREATER

  // Plus
  public static IModifier<IReadOnlyValue<S>,S> Plus<S>(S v, string name = null) where S : INumber<S>
    => Plus(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Plus<S>(IReadOnlyValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '+' };

  public static IModifier<IValue<S>,S> Plus<S>(IValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '+' };

  // Times
  public static IModifier<IReadOnlyValue<S>,S> Times<S>(S v, string name = null) where S : INumber<S>
    => Times(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Times<S>(IReadOnlyValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '*' };

  public static IModifier<IValue<S>,S> Times<S>(IValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '*' };

  // Minus
  public static IModifier<IReadOnlyValue<S>,S> Minus<S>(S v, string name = null) where S : INumber<S>
    => Minus(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Minus<S>(IReadOnlyValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '-' };

  public static IModifier<IValue<S>,S> Minus<S>(IValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '-' };


  // Divide
  public static IModifier<IReadOnlyValue<S>,S> Divide<S>(S v, string name = null) where S : INumber<S>
    => Divide(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Divide<S>(IReadOnlyValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '/' };

  public static IModifier<IValue<S>,S> Divide<S>(IValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '/' };

  // Substitute
  public static IModifier<IReadOnlyValue<S>,S> Substitute<S>(S v, string name = null) where S : INumber<S>
    => Substitute(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Substitute<S>(IReadOnlyValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '=' };

  public static IModifier<IValue<S>,S> Substitute<S>(IValue<S> v, string name = null) where S : INumber<S>
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '=' };
#else
  /* Here is the alternative to having a nice INumber<T> type like .NET7 will have. */
  public interface IOperator<X> {
    X Create<T>(T other);
    X Sum(X a, X b);
    X Times(X a, X b);
    X Divide(X a, X b);
    X Negate(X a);
    X Max(X a, X b);
    X Min(X a, X b);
    X zero { get; }
    X one { get; }
  }

  internal struct OpFloat : IOperator<float> {
    public float Create<T>(T other) => Convert.ToSingle(other);
    public float Sum(float a, float b) => a + b;
    public float Times(float a, float b) => a * b;
    public float Divide(float a, float b) => a / b;
    public float Negate(float a) => -a;
    public float Max(float a, float b) => Math.Max(a, b);
    public float Min(float a, float b) => Math.Min(a, b);
    public float zero => 0f;
    public float one => 1f;
  }

  internal struct OpDouble : IOperator<double> {
    public double Create<T>(T other) => Convert.ToDouble(other);
    public double Sum(double a, double b) => a + b;
    public double Times(double a, double b) => a * b;
    public double Divide(double a, double b) => a / b;
    public double Negate(double a) => -a;
    public double Max(double a, double b) => Math.Max(a, b);
    public double Min(double a, double b) => Math.Min(a, b);
    public double zero => 0.0;
    public double one => 1.0;
  }

  internal struct OpInt : IOperator<int> {
    public int Create<T>(T other) => Convert.ToInt32(other);
    public int Sum(int a, int b) => a + b;
    public int Times(int a, int b) => a * b;
    public int Divide(int a, int b) => a / b;
    public int Negate(int a) => -a;
    public int Max(int a, int b) => Math.Max(a, b);
    public int Min(int a, int b) => Math.Min(a, b);
    public int zero => 0;
    public int one => 1;
  }

#if UNITY_5_3_OR_NEWER
  internal struct OpVector3 : IOperator<Vector3> {
    public Vector3 Create<T>(T other) {
      var op = GetOp<float>();
      var o = op.Create(other);
      return new Vector3(o, o, o);
    }
    public Vector3 Sum(Vector3 a, Vector3 b) => a + b;
    public Vector3 Times(Vector3 a, Vector3 b) => Vector3.Scale(a, b);
    public Vector3 Divide(Vector3 a, Vector3 b)
      => Vector3.Scale(a, new Vector3(1f / b.x, 1f / b.y, 1f / b.z));
    public Vector3 Negate(Vector3 a) => -a;
    public Vector3 Max(Vector3 a, Vector3 b) => Vector3.Max(a, b);
    public Vector3 Min(Vector3 a, Vector3 b) => Vector3.Min(a, b);
    public Vector3 zero => Vector3.zero;
    public Vector3 one => Vector3.one;
  }
#endif

  /* Not quite zero cost since this boxes the struct. */
  public static IOperator<S> GetOp<S>() {
    switch (Type.GetTypeCode(typeof(S))) {
      case TypeCode.Double:
        return (IOperator<S>) (object) default(OpDouble);
      case TypeCode.Single:
        return (IOperator<S>) (object) default(OpFloat);
      case TypeCode.Int32:
        return (IOperator<S>) (object) default(OpInt);
      case TypeCode.Object:
#if UNITY_5_3_OR_NEWER
        if (typeof(S) == typeof(Vector3))
          return (IOperator<S>) (object) default(OpVector3);
        else
            goto default;
#endif
      default:
        throw new NotImplementedException($"No IOperator<T> implementation for type {typeof(S)}.");
    }
  }

  // Plus
  public static IModifier<IReadOnlyValue<S>,S> Plus<S>(S v, string name = null) where S : struct
    => Plus(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Plus<S>(IReadOnlyValue<S> v, string name = null)
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '+' };

  public static IModifier<IValue<S>,S> Plus<S>(IValue<S> v, string name = null)
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '+' };

  // Times
  public static IModifier<IReadOnlyValue<S>,S> Times<S>(S v, string name = null) where S : struct
    => Times(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Times<S>(IReadOnlyValue<S> v, string name = null)
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '*' };

  public static IModifier<IValue<S>,S> Times<S>(IValue<S> v, string name = null)
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '*' };

  // Minus
  public static IModifier<IReadOnlyValue<S>,S> Minus<S>(S v, string name = null) where S : struct
    => Minus(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Minus<S>(IReadOnlyValue<S> v, string name = null)
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '-' };

  public static IModifier<IValue<S>,S> Minus<S>(IValue<S> v, string name = null)
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '-' };


  // Divide
  public static IModifier<IReadOnlyValue<S>,S> Divide<S>(S v, string name = null) where S : struct
    => Divide(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Divide<S>(IReadOnlyValue<S> v, string name = null)
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '/' };

  public static IModifier<IValue<S>,S> Divide<S>(IValue<S> v, string name = null)
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '/' };

  // Substitute
  public static IModifier<IReadOnlyValue<S>,S> Substitute<S>(S v, string name = null) where S : struct
    => Substitute(new ReadOnlyValue<S>(v), name);

  public static IModifier<IReadOnlyValue<S>,S> Substitute<S>(IReadOnlyValue<S> v, string name = null)
    => new NumericalModifier<IReadOnlyValue<S>,S>(v) { name = name, symbol = '=' };

  public static IModifier<IValue<S>,S> Substitute<S>(IValue<S> v, string name = null)
    => new NumericalModifier<IValue<S>,S>(v) { name = name, symbol = '=' };

#endif
/** Cast a numerical type into something else. */
  internal class CastingModifier<S,T> : ContextModifier<IModifier<S>,T>
#if NET6_0_OR_GREATER
    where S : INumber<S>
    where T : INumber<T>
#endif
  {
    public CastingModifier(IModifier<S> context) : base(context) { }

#if NET6_0_OR_GREATER
    public override T Modify(T given)
      => T.Create(context.Modify(S.Create(given)));
#else
    public override T Modify(T given) {
      var s = GetOp<S>();
      var t = GetOp<T>();
      return t.Create(context.Modify(s.Create(given)));
    }
#endif
  }
}

/** An abstract modifier that keeps a particular context about it.

    If that context implements `INotifyPropertyChanged`, its events will provoke
    this modifier's PropertyChanged events.
  */
public abstract class ContextModifier<S,T> : IModifier<S,T>, IDisposable {
  public string name { get; init; }
  private bool _enabled = true;
  public virtual bool enabled {
    get => _enabled;
    set {
      if (_enabled == value)
        return;
      _enabled = value;
      OnChange(nameof(enabled));
    }
  }
  public S context { get; }

  public event PropertyChangedEventHandler PropertyChanged;

  public ContextModifier(S context) {
    if (context is INotifyPropertyChanged notify)
      notify.PropertyChanged += Chain;
    this.context = context;
  }

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }

  internal void Chain(object sender, PropertyChangedEventArgs args) => OnChange(nameof(context));

  public abstract T Modify(T given);

  public void Dispose() {
    if (context is INotifyPropertyChanged notify)
      notify.PropertyChanged -= Chain;
  }

  public override string ToString() {
    var builder = new StringBuilder();
    if (name != null) {
      builder.Append('"');
      builder.Append(name);
      builder.Append('"');
      builder.Append(' ');
    }

    builder.Append(context);
    return builder.ToString();
  }
}


  public class NumericalModifier<S,T> : ContextModifier<S,T>
    where S : IReadOnlyValue<T>
#if NET6_0_OR_GREATER
    where T : INumber<T>
#endif
  {
    public char symbol { get; init; } = '?';
    public NumericalModifier(S context) : base(context) { }

#if NET6_0_OR_GREATER
    public override T Modify(T given) {
      T v = context.value;
      switch (symbol) {
        case '+':
          return given + v;
        case '-':
          return given - v;
        case '*':
          return given * v;
        case '/':
          return given / v;
        case '=':
          return v;
        default:
          throw new NotImplementedException();
      }
    }
#else
    public override T Modify(T given) {
      var t = Modifier.GetOp<T>();
      T v = context.value;
      switch (symbol) {
        case '+':
          return t.Sum(given, v);
        case '-':
          return t.Sum(given, t.Negate(v));
        case '*':
          return t.Times(given, v);
        case '/':
          return t.Divide(given, v);
        case '=':
          return v;
        default:
          throw new NotImplementedException();
      }
    }
#endif

    public override string ToString() {
      var builder = new StringBuilder();
      // builder.Append("ref ");
      if (name != null) {
        builder.Append('"');
        builder.Append(name);
        builder.Append('"');
        builder.Append(' ');
      }
      builder.Append(symbol);

      builder.Append(context);
      return builder.ToString();
    }
  }

  public static class TargetedModifierExtensions {
    public static void AddTo<S,T>(this ITarget<S,T> applicator, S bag)
      => applicator.AppliesTo(bag).modifiers.Add(applicator.modifier);
    public static bool RemoveFrom<S,T>(this ITarget<S,T> applicator, S bag)
      => applicator.AppliesTo(bag).modifiers.Remove(applicator.modifier);
    public static bool ContainedIn<S,T>(this ITarget<S,T> applicator, S bag)
      => applicator.AppliesTo(bag).modifiers.Contains(applicator.modifier);
  }
}


