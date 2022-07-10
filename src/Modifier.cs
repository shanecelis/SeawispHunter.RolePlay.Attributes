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
  // S context { get; set; }
  bool enabled { get; set; }
  T Modify(T given);
  // event PropertyChangedEventHandler PropertyChanged;
}

public interface IValuedModifier<S,T> : IModifier<T>, IMutableValue<S> {
  /* We want this to be settable. */
  // new S value { get; set; }
}

/** Most modifiers will be of the same type as the stat they're modifying, so
    let's make that easier to express. */
// public interface IValuedModifier<T> : IValuedModifier<T,T> { }

public static class ModifiableValueExtensions {

#if NET6_0_OR_GREATER
  public static IValuedModifier<S,T> Plus<S,T>(this IModifiableValue<T> mod, S v, string name = null)
    where T : INumber<T>
    where S : INumber<S> {
    var modifier = Modifier.Plus<S,T>(v, name);
    mod.modifiers.Add(modifier);
    return modifier;
  }
#endif

}

public static class Modifier {

#if NET6_0_OR_GREATER
  /* Four variations per operator because we want to cover T and IValue<T>, and
     we want to cover cases where the type T for the modifier and S for the
     value are distinct. */
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
  public static IValuedModifier<T,T> Plus<T>(IValue<T> v, string name = null) where T : INumber<T> => Plus<T,T>(v, name);

  public static IValuedModifier<S,T> Multiply<S,T>(S v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifier<S,T> { value = v, op = (given, v) => T.Create(S.Create(given) * v), name = name, symbol = '*' };

  public static IValuedModifier<S,T> Multiply<S,T>(IValue<S> v, string name = null)
    where T : INumber<T>
    where S : INumber<S>
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => T.Create(S.Create(given) * v), name = name, symbol = '*' };

  public static IValuedModifier<T,T> Multiply<T>(T v, string name = null) where T : INumber<T> => Multiply<T,T>(v, name);
  public static IValuedModifier<T,T> Multiply<T>(IValue<T> v, string name = null) where T : INumber<T> => Multiply<T,T>(v, name);

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
  internal interface IOperator<X,Y> {
    X Sum(X lhs, Y rhs);
    X Times(X lhs, Y rhs);
  }
  internal struct OpFloatFloat : IOperator<float,float> {
    public float Sum(float a, float b) => a + b;
    public float Times(float a, float b) => a * b;
  }
  internal struct OpIntInt : IOperator<int,int> {
    public int Sum(int a, int b) => a + b;
    public int Times(int a, int b) => a * b;
  }

  internal struct OpFloatInt : IOperator<float,int> {
    public float Sum(float a, int b) => a + b;
    public float Times(float a, int b) => a * b;
  }

  internal struct OpIntFloat : IOperator<int,float> {
    public int Sum(int a, float b) => (int) (a + b);
    public int Times(int a, float b) => (int) (a * b);
  }

  public static IValuedModifier<S,T> Plus<S,T>(S v, string name = null)
    => new ValuedModifier<S,T> { value = v, op = (given, v) => GetOperator<T,S>().Sum(given, v), name = name, symbol = '+' };
  public static IValuedModifier<S,T> Plus<S,T>(IValue<S> v, string name = null)
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => GetOperator<T,S>().Sum(given, v), name = name, symbol = '+' };
  public static IValuedModifier<T,T> Plus<T>(T v, string name = null) => Plus<T,T>(v, name);
  public static IValuedModifier<T,T> Plus<T>(IValue<T> v, string name = null) => Plus<T,T>(v, name);

  public static IValuedModifier<S,T> Multiply<S,T>(S v, string name = null)
    => new ValuedModifier<S,T> { value = v, op = (given, v) => GetOperator<T,S>().Times(given, v), name = name, symbol = '*' };
  public static IValuedModifier<S,T> Multiply<S,T>(IValue<S> v, string name = null)
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => GetOperator<T,S>().Times(given, v), name = name, symbol = '*' };
  public static IValuedModifier<T,T> Multiply<T>(T v, string name = null) => Multiply<T,T>(v, name);
  public static IValuedModifier<T,T> Multiply<T>(IValue<T> v, string name = null) => Multiply<T,T>(v, name);

  public static IValuedModifier<S,T> Substitute<S,T>(S v, string name = null)
    => new ValuedModifier<S,T> { value = v, op = (given, v) => (T) (object) v, name = name, symbol = '=' };
  public static IValuedModifier<S,T> Substitute<S,T>(IValue<S> v, string name = null)
    => new ValuedModifierReference<S,T>(v) { op = (given, v) => (T) (object) v, name = name, symbol = '=' };
  public static IValuedModifier<T,T> Substitute<T>(T v, string name = null) => Substitute<T,T>(v, name);
  public static IValuedModifier<T,T> Substitute<T>(IValue<T> v, string name = null) => Substitute<T,T>(v, name);

  private static IOperator<S,T> GetOperator<S,T>() {
    switch (Type.GetTypeCode(typeof(S))) {
      case TypeCode.Single:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IOperator<S,T>) (object) default(OpFloatInt);
          case TypeCode.Single:
            return (IOperator<S,T>) (object) default(OpFloatFloat);
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      case TypeCode.Int32:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IOperator<S,T>) (object) default(OpIntInt);
          case TypeCode.Single:
            return (IOperator<S,T>) (object) default(OpIntFloat);
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      default:
        throw new NotImplementedException($"No handler for first type {typeof(S)}.");
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
    public bool enabled { get; set; } = true;
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
        // OnChange(nameof(value));
      }
    }

    // S IValue<S>.value => reference.value;

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
    // protected void Chain(object sender, PropertyChangedEventArgs args) {
    //   PropertyChanged?.Invoke(this, args);
    // }

    public T Modify(T given) => op(given, value);

    // internal void Chain(object sender, PropertyChangedEventArgs args) => OnChange(nameof(value));
    public void Dispose() {
      reference.PropertyChanged -= Chain;
    }

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

  // internal class ValuedModifierReference<T> : ValuedModifierReference<T,T>, IValuedModifier<T> {
  //   public ValuedModifierReference(IValue<T> value) : base(value) { }
  // }
}

public class ValuedModifier<S,T> : IValuedModifier<S,T> {
  public string name { get; init; }
  public char symbol { get; init; } = '?';
  public bool enabled { get; set; } = true;

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

  S IValue<S>.value => _value;

  public Func<T,S,T> op { get; init; }

  public event PropertyChangedEventHandler PropertyChanged;
  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }

  public T Modify(T given) => op(given, value);

  // internal void Chain(object sender, PropertyChangedEventArgs args) => OnChange(nameof(value));

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

/** Most modifiers will be of the same type as the stat they're modifying, so
    let's make that easier to express. */
// internal class ValuedModifier<T> : ValuedModifier<T,T>, IValuedModifier<T> { }

}
