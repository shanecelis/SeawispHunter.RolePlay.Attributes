/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   This comment generated by code-cite[3].

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
   [4]: https://github.com/shanecelis/code-cite
*/

using System;
using System.Text;
using System.ComponentModel;
#if NET6_0_OR_GREATER
using System.Numerics;
#endif

namespace SeawispHunter.RolePlay.Attributes {

/** A IModifier<T> modifies an IModifiableValue<T>'s value. */
public interface IModifier<T> : INotifyPropertyChanged {
  // string name { get; }
  bool enabled { get; set; }
  T Modify(T given);
  // event PropertyChangedEventHandler PropertyChanged;
}

public interface IValuedModifier<S,T> : IModifier<T>, IMutableValue<S> {
  /* We want this to be settable. */
  // S value { get; set; }
}

public static class Modifier {

#if NET6_0_OR_GREATER
  /* Four variations per operator because we want to cover T and IValue<T>, and
     we want to cover cases where the type T for the modifier and type S for the
     value are distinct. */

  // Plus
  public static IValuedModifier<S,T> Plus<S,T>(S v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifier<S,T> { value = v, op = (given, v) => T.Create(S.Create(given) + v), name = name, symbol = '+' };
  //                                                                    (T) ((S) given + v)
  //                                                                    ^ How to write typecasts in INumber.
  public static IValuedModifier<S,T> Plus<S,T>(IValue<S> v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => T.Create(S.Create(given) + v), name = name, symbol = '+' };
  public static IValuedModifier<T,T> Plus<T>(T v, string name = null) where T : INumber<T> => Plus<T,T>(v, name);
  public static IValuedModifier<T,T> Plus<T>(this IValue<T> v, string name = null) where T : INumber<T> => Plus<T,T>(v, name);

  // Subtract
  public static IValuedModifier<S,T> Subtract<S,T>(S v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifier<S,T> { value = v, op = (given, v) => T.Create(S.Create(given) - v), name = name, symbol = '-' };
  //                                                                    (T) ((S) given + v)
  //                                                                  ^ How to write typecasts in INumber.
  public static IValuedModifier<S,T> Subtract<S,T>(IValue<S> v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => T.Create(S.Create(given) - v), name = name, symbol = '-' };
  public static IValuedModifier<T,T> Subtract<T>(T v, string name = null) where T : INumber<T> => Subtract<T,T>(v, name);
  public static IValuedModifier<T,T> Subtract<T>(this IValue<T> v, string name = null) where T : INumber<T> => Subtract<T,T>(v, name);

  // Times
  public static IValuedModifier<S,T> Times<S,T>(S v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifier<S,T> { value = v, op = (given, v) => T.Create(S.Create(given) * v), name = name, symbol = '*' };
  public static IValuedModifier<S,T> Times<S,T>(IValue<S> v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => T.Create(S.Create(given) * v), name = name, symbol = '*' };
  public static IValuedModifier<T,T> Times<T>(T v, string name = null) where T : INumber<T> => Times<T,T>(v, name);
  public static IValuedModifier<T,T> Times<T>(IValue<T> v, string name = null) where T : INumber<T> => Times<T,T>(v, name);

  // Divide
  public static IValuedModifier<S,T> Divide<S,T>(S v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifier<S,T> { value = v, op = (given, v) => T.Create(S.Create(given) / v), name = name, symbol = '/' };
  public static IValuedModifier<S,T> Divide<S,T>(IValue<S> v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => T.Create(S.Create(given) / v), name = name, symbol = '/' };
  public static IValuedModifier<T,T> Divide<T>(T v, string name = null) where T : INumber<T> => Divide<T,T>(v, name);
  public static IValuedModifier<T,T> Divide<T>(IValue<T> v, string name = null) where T : INumber<T> => Divide<T,T>(v, name);

  // Substitute
  public static IValuedModifier<S,T> Substitute<S,T>(S v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifier<S,T> { value = v, op = (given, v) => T.Create(v), name = name, symbol = '=' };
  public static IValuedModifier<S,T> Substitute<S,T>(IValue<S> v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => T.Create(v), name = name, symbol = '=' };
  public static IValuedModifier<T,T> Substitute<T>(T v, string name = null) where T : INumber<T> => Substitute<T,T>(v, name);
  public static IValuedModifier<T,T> Substitute<T>(IValue<T> v, string name = null) where T : INumber<T> => Substitute<T,T>(v, name);

#else
  /* Here is the alternative to having a nice INumber<T> type like .NET7 will have. */
  internal interface IOperator<X> {
    X Create<T>(T other);
    X Sum(X a, X b);
    X Times(X a, X b);
    X Divide(X a, X b);
    X Negate(X a);
    X zero { get; }
    X one { get; }
  }

  internal struct OpFloat : IOperator<float> {
    public float Create<T>(T other) => Convert.ToSingle(other);
    public float Sum(float a, float b) => a + b;
    public float Times(float a, float b) => a * b;
    public float Divide(float a, float b) => a / b;
    public float Negate(float a) => -a;
    public float zero => 0f;
    public float one => 1f;
  }

  internal struct OpInt : IOperator<int> {
    public int Create<T>(T other) => Convert.ToInt32(other);
    public int Sum(int a, int b) => a + b;
    public int Times(int a, int b) => a * b;
    public int Divide(int a, int b) => a / b;
    public int Negate(int a) => -a;
    public int zero => 0;
    public int one => 1;
  }

  // Plus
  public static IValuedModifier<S,T> Plus<S,T>(S v, string name = null) {
    var s = GetOp<S>();
    var t = GetOp<T>();
    return new ValuedModifier<S,T> { value = v, op = (given, v) => t.Create(s.Sum(s.Create(given), v)), name = name, symbol = '+' };
  }
  public static IValuedModifier<S,T> Plus<S,T>(IValue<S> v, string name = null) {
    var s = GetOp<S>();
    var t = GetOp<T>();
    return new ValuedModifierReference<S,T>(v) { op = (given, v) => t.Create(s.Sum(s.Create(given), v)), name = name, symbol = '+' };
  }
  public static IValuedModifier<T,T> Plus<T>(T v, string name = null) => Plus<T,T>(v, name);
  public static IValuedModifier<T,T> Plus<T>(this IValue<T> v, string name = null) => Plus<T,T>(v, name);

  // Plus
  public static IValuedModifier<S,T> Subtract<S,T>(S v, string name = null) {
    var s = GetOp<S>();
    var t = GetOp<T>();
    return new ValuedModifier<S,T> { value = v, op = (given, v) => t.Create(s.Sum(s.Create(given), s.Negate(v))), name = name, symbol = '-' };
  }
  public static IValuedModifier<S,T> Subtract<S,T>(IValue<S> v, string name = null) {
    var s = GetOp<S>();
    var t = GetOp<T>();
    return new ValuedModifierReference<S,T>(v) { op = (given, v) => t.Create(s.Sum(s.Create(given), s.Negate(v))), name = name, symbol = '-' };
  }
  public static IValuedModifier<T,T> Subtract<T>(T v, string name = null) => Subtract<T,T>(v, name);
  public static IValuedModifier<T,T> Subtract<T>(this IValue<T> v, string name = null) => Subtract<T,T>(v, name);

  // Times
  public static IValuedModifier<S,T> Times<S,T>(S v, string name = null) {
    var s = GetOp<S>();
    var t = GetOp<T>();
    return new ValuedModifier<S,T> { value = v, op = (given, v) => t.Create(s.Times(s.Create(given), v)), name = name, symbol = '*' };
  }
  public static IValuedModifier<S,T> Times<S,T>(IValue<S> v, string name = null) {
    var s = GetOp<S>();
    var t = GetOp<T>();
    return new ValuedModifierReference<S,T>(v) { op = (given, v) => t.Create(s.Times(s.Create(given), v)), name = name, symbol = '*' };
  }
  public static IValuedModifier<T,T> Times<T>(T v, string name = null) => Times<T,T>(v, name);
  public static IValuedModifier<T,T> Times<T>(IValue<T> v, string name = null) => Times<T,T>(v, name);

  public static IValuedModifier<S,T> Divide<S,T>(S v, string name = null) {
    var s = GetOp<S>();
    var t = GetOp<T>();
    return new ValuedModifier<S,T> { value = v, op = (given, v) => t.Create(s.Divide(s.Create(given), v)), name = name, symbol = '/' };
  }
  public static IValuedModifier<S,T> Divide<S,T>(IValue<S> v, string name = null) {
    var s = GetOp<S>();
    var t = GetOp<T>();
    return new ValuedModifierReference<S,T>(v) { op = (given, v) => t.Create(s.Divide(s.Create(given), v)), name = name, symbol = '/' };
  }
  public static IValuedModifier<T,T> Divide<T>(T v, string name = null) => Divide<T,T>(v, name);
  public static IValuedModifier<T,T> Divide<T>(IValue<T> v, string name = null) => Divide<T,T>(v, name);

  // Substitute
  public static IValuedModifier<S,T> Substitute<S,T>(S v, string name = null)
    => new ValuedModifier<S,T> { value = v, op = (given, v) => (T) (object) v, name = name, symbol = '=' };
  public static IValuedModifier<S,T> Substitute<S,T>(IValue<S> v, string name = null)
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => (T) (object) v, name = name, symbol = '=' };
  public static IValuedModifier<T,T> Substitute<T>(T v, string name = null) => Substitute<T,T>(v, name);
  public static IValuedModifier<T,T> Substitute<T>(IValue<T> v, string name = null) => Substitute<T,T>(v, name);

  /* Not quite zero cost since this boxes the struct. */
  private static IOperator<S> GetOp<S>() {
    switch (Type.GetTypeCode(typeof(S))) {
      case TypeCode.Single:
        return (IOperator<S>) (object) default(OpFloat);
      case TypeCode.Int32:
        return (IOperator<S>) (object) default(OpInt);
      default:
            throw new NotImplementedException($"No handler for second type {typeof(S)}.");
    }
  }

// https://pvs-studio.com/en/blog/posts/csharp/0878/
// void SomeProcessing<T, TOperation>(...)
//     where TOperation : struct, IOperator<T>
// {
//     T var1 = ...;
//     T var2 = ...;
//     T sum = default(TOperation).Sum(var1, var2);  // This is zero cost!
// }

#endif

  internal class ValuedModifierReference<S,T> : IValuedModifier<S,T>, IDisposable {
    public string name { get; init; }
    public char symbol { get; init; } = '?';
    private bool _enabled = true;
    public bool enabled {
      get => _enabled;
      set {
        if (_enabled == value)
          return;
        _enabled = value;
        OnChange(nameof(enabled));
      }
    }
    private readonly IValue<S> reference;
    public S value {
      get => reference.value;
      set {
        if (reference.value != null && reference.value.Equals(value))
          return;
        if (reference is IMutableValue<S> mutable)
          mutable.value = value;
        else
          throw new InvalidOperationException("Cannot mutate an IValue<S>. Consider providing an IMutableValue<S> instead.");
        // Don't need to signify change because `reference` changes will already be propogated.
        // OnChange(nameof(value));
      }
    }
    
    /** The `op` or "operator" is what coalesces the value and attribute. */
    public Func<T,S,T> op { get; init; }

    public event PropertyChangedEventHandler PropertyChanged;

    public ValuedModifierReference(IValue<S> value) {
      reference = value;
      value.PropertyChanged += Chain;
    }
    protected void OnChange(string name) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    internal void Chain(object sender, PropertyChangedEventArgs args) => OnChange(nameof(value));

    public T Modify(T given) => op(given, value);

    public void Dispose() => reference.PropertyChanged -= Chain;

    public override string ToString() {
      var builder = new StringBuilder();
      builder.Append("ref ");
      if (name != null) {
        builder.Append('"');
        builder.Append(name);
        builder.Append('"');
        builder.Append(' ');
      }
      // if (symbol != null)
        builder.Append(symbol);

      builder.Append(value);
      return builder.ToString();
    }
  }
}

public class ValuedModifier<S,T> : IValuedModifier<S,T> {
  public string name { get; init; }
  public char symbol { get; init; } = '?';
  private bool _enabled = true;
  public bool enabled {
    get => _enabled;
    set {
      if (_enabled == value)
        return;
      _enabled = value;
      OnChange(nameof(enabled));
    }
  }

  private S _value;
  public S value {
    get => _value;
    set {
      if (_value != null && _value.Equals(value))
        return;
      _value = value;
      OnChange(nameof(value));
    }
  }
  public Func<T,S,T> op { get; init; }

  public event PropertyChangedEventHandler PropertyChanged;
  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }

  public T Modify(T given) => op(given, value);

  // Is this worth having?
  public ValuedModifier<S,T> Select(Func<S,S> func) {
    var m = new ValuedModifier<S,T> { name = this.name,
                                      op = this.op,
                                      value = func(this.value) };
    // HACK: Not sure I like this. Notify? yes. Mutate on notify? No.
    this.PropertyChanged += (_, _) => m.value = func(this.value);
    return m;
  }
  
  public override string ToString() {
    var builder = new StringBuilder();
    if (name != null) {
      builder.Append('"');
      builder.Append(name);
      builder.Append('"');
      builder.Append(' ');
    }
    // if (symbol != null)
      builder.Append(symbol);

    builder.Append(_value);
    return builder.ToString();
  }
}

}
