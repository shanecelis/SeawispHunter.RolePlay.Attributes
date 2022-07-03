using System.Text;
using System.ComponentModel;

namespace SeawispHunter.Game.Stat;

/** A IModifier<T> modifies an IStat<T>'s value. */
public interface IModifier<T> : INotifyPropertyChanged {
  string name { get; }
  // S context { get; set; }
  T Modify(T given);
  // event PropertyChangedEventHandler PropertyChanged;
}

public interface IModifierValue<S,T> : IModifier<T>, IMutableValue<S> {
  /* We want this to be settable. */
  // new S value { get; set; }
}

/** Most modifiers will be of the same type as the stat they're modifying, so
    let's make that easier to express. */
public interface IModifierValue<T> : IModifierValue<T,T> { }

public static class Modifier {
  public static IModifierValue<float> Plus(float v, string name = null) => new ModifierValue<float> { value = v, op = (given, v) => given + v, name = name, symbol = '+' };
  public static IModifierValue<float> Multiply(float v, string name = null) => new ModifierValue<float> { value = v, op = (given, v) => given * v, name = name, symbol = '*' };
  public static IModifierValue<float> Substitute(float v, string name = null) => new ModifierValue<float> { value = v, op = (given, v) => v, name = name, symbol = '=' };

  public static IModifierValue<float> Plus(IValue<float> v, string name = null) => new ModifierReference<float>(v) { op = (given, v) => given + v, name = name, symbol = '+' };
  public static IModifierValue<float> Multiply(IValue<float> v, string name = null) => new ModifierReference<float>(v) { op = (given, v) => given * v, name = name, symbol = '*' };
  public static IModifierValue<float> Substitute(IValue<float> v, string name = null) => new ModifierReference<float>(v) { op = (given, v) => v, name = name, symbol = '=' };

  public static IModifierValue<int> Plus(int v, string name = null) => new ModifierValue<int> { value = v, op = (given, v) => given + v, name = name, symbol = '+' };
  public static IModifierValue<int> Multiply(int v, string name = null) => new ModifierValue<int> { value = v, op = (given, v) => given * v, name = name, symbol = '*' };
  public static IModifierValue<int> Substitute(int v, string name = null) => new ModifierValue<int> { value = v, op = (given, v) => v, name = name, symbol = '=' };

  public static IModifierValue<int> Plus(IValue<int> v, string name = null) => new ModifierReference<int>(v) { op = (given, v) => given + v, name = name, symbol = '+' };
  public static IModifierValue<int> Multiply(IValue<int> v, string name = null) => new ModifierReference<int>(v) { op = (given, v) => given * v, name = name, symbol = '*' };
  public static IModifierValue<int> Substitute(IValue<int> v, string name = null) => new ModifierReference<int>(v) { op = (given, v) => v, name = name, symbol = '=' };

  // This looks way more complicated, does weird runtime stuff. Prefer above.
  public static IModifierValue<S, T> Plus<S,T>(S v, string name = null) {
    switch (Type.GetTypeCode(typeof(S))) {
      case TypeCode.Single:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IModifierValue<S,T>) new ModifierValue<float, int> { value = (float) (object) v, op = (given, v) => (int) (given + v), name = name, symbol = '+' };

          case TypeCode.Single:
            return (IModifierValue<S,T>) new ModifierValue<float, float> { value = (float) (object) v, op = (given, v) => given + v, name = name, symbol = '+' };
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      case TypeCode.Int32:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IModifierValue<S,T>) new ModifierValue<int, int> { value = (int) (object) v, op = (given, v) => given + v, name = name, symbol = '+' };

          case TypeCode.Single:
            return (IModifierValue<S,T>) new ModifierValue<int, float> { value = (int) (object) v, op = (given, v) => given + v, name = name, symbol = '+' };
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      default:
        throw new NotImplementedException($"No handler for first type {typeof(S)}.");
    }
  }

  public static IModifierValue<S, T> Plus<S,T>(IValue<S> v, string name = null) {
    switch (Type.GetTypeCode(typeof(S))) {
      case TypeCode.Single:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IModifierValue<S,T>) new ModifierReference<float, int>((IValue<float>) v) { op = (given, v) => (int) (given + v), name = name };

          case TypeCode.Single:
            return (IModifierValue<S,T>) new ModifierReference<float, float>((IValue<float>) v) { op = (given, v) => given + v, name = name };
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      case TypeCode.Int32:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IModifierValue<S,T>) new ModifierReference<int, int>((IValue<int>) v) { op = (given, v) => given + v, name = name };

          case TypeCode.Single:
            return (IModifierValue<S,T>) new ModifierReference<int, float>((IValue<int>) v) { op = (given, v) => given + v, name = name };
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      default:
        throw new NotImplementedException($"No handler for first type {typeof(S)}.");
    }
  }

  public static IModifierValue<S, T> Multiply<S,T>(S v, string name = null) {
    switch (Type.GetTypeCode(typeof(S))) {
      case TypeCode.Single:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IModifierValue<S,T>) new ModifierValue<float, int> { value = (float) (object) v, op = (given, v) => (int) (given * v), name = name, symbol = '*' };

          case TypeCode.Single:
            return (IModifierValue<S,T>) new ModifierValue<float, float> { value = (float) (object) v, op = (given, v) => given * v, name = name, symbol = '*' };
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      case TypeCode.Int32:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IModifierValue<S,T>) new ModifierValue<int, int> { value = (int) (object) v, op = (given, v) => given * v, name = name, symbol = '*' };

          case TypeCode.Single:
            return (IModifierValue<S,T>) new ModifierValue<int, float> { value = (int) (object) v, op = (given, v) => given * v, name = name, symbol = '*' };
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      default:
        throw new NotImplementedException($"No handler for first type {typeof(S)}.");
    }
  }

  public static IModifierValue<S, T> Multiply<S,T>(IValue<S> v, string name = null) {
    switch (Type.GetTypeCode(typeof(S))) {
      case TypeCode.Single:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IModifierValue<S,T>) new ModifierReference<float, int>((IValue<float>) v) { op = (given, v) => (int) (given * v), name = name };

          case TypeCode.Single:
            return (IModifierValue<S,T>) new ModifierReference<float, float>((IValue<float>) v) { op = (given, v) => given * v, name = name };
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      case TypeCode.Int32:
        switch (Type.GetTypeCode(typeof(T))) {
          case TypeCode.Int32:
            return (IModifierValue<S,T>) new ModifierReference<int, int>((IValue<int>) v) { op = (given, v) => given * v, name = name };

          case TypeCode.Single:
            return (IModifierValue<S,T>) new ModifierReference<int, float>((IValue<int>) v) { op = (given, v) => given * v, name = name };
          default:
            throw new NotImplementedException($"No handler for second type {typeof(T)}.");
        }
      default:
        throw new NotImplementedException($"No handler for first type {typeof(S)}.");
    }
  }

  public static IModifierValue<float, int> Multiply<T>(float v, string name = null) {
    switch (Type.GetTypeCode(typeof(T))) {
        case TypeCode.Int32:
          return new ModifierValue<float, int> { value = v, op = (given, v) => (int) (given * v), name = name };
      default:
        throw new NotImplementedException($"No handler for type {typeof(T)}.");
    }
  }

  public static IModifierValue<float, int> Multiply<T>(IValue<float> v, string name = null) {
    switch (Type.GetTypeCode(typeof(T))) {
        case TypeCode.Int32:
          return new ModifierReference<float, int>(v) { op = (given, v) => (int) (given * v), name = name };
      default:
        throw new NotImplementedException($"No handler for type {typeof(T)}.");
    }
  }

  public static IModifierValue<float, int> Substitute<T>(float v, string name = null) {
    switch (Type.GetTypeCode(typeof(T))) {
        case TypeCode.Int32:
          return new ModifierValue<float, int> { value = v, op = (given, v) => (int) given, name = name };
      default:
        throw new NotImplementedException($"No handler for type {typeof(T)}.");
    }
  }

  // public static IModifierValue<float> Plus(float v) => new ModifierValue<float, float> { value = v, op = (given, v) => given + v };
  // public static IModifierValue<float> Multiply(float v) => new ModifierValue<float, float> { value = v, op = (given, v) => given * v };
  // public static IModifierValue<float> Substitute(float v) => new ModifierValue<float, float> { value = v, op = (given, v) => v };
  // public static IModifier<float> Plus(IValue<float> v) => new ModifierValue<float, float> { value = v, op => (given, v) => given + v };


  internal class ModifierValue<S,T> : IModifierValue<S,T> {
    public string name { get; init; }
    public char symbol { get; init; } = '?';

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

    public ModifierValue<S,T> Select(Func<S,S> func) {
      var m = new ModifierValue<S,T> { name = this.name,
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
      if (symbol != null)
        builder.Append(symbol);

      builder.Append(_value);
      return builder.ToString();
    }
  }

  /** Most modifiers will be of the same type as the stat they're modifying, so
      let's make that easier to express. */
  internal class ModifierValue<T> : ModifierValue<T,T>, IModifierValue<T> { }

  internal class ModifierReference<T> : ModifierReference<T,T>, IModifierValue<T> where T : IEquatable<T> {
    public ModifierReference(IValue<T> value) : base(value) { }
  }
  internal class ModifierReference<S,T> : IModifierValue<S,T>, IDisposable where S : IEquatable<S> {
    public string name { get; init; }
    public char symbol { get; init; } = '?';
    private readonly IValue<S> reference;
    public S value {
      get => reference.value;
      set {
        if (reference.value.Equals(value))
          return;
        if (reference is IMutableValue<S> mutable)
          mutable.value = value;
        else
          throw new InvalidOperationException("Cannot mutate an IValue<S>. Consider providing an IMutableValue<S> instead.");
        // OnChange(nameof(value));
      }
    }

    S IValue<S>.value => reference.value;

    public Func<T,S,T> op { get; init; }

    public event PropertyChangedEventHandler PropertyChanged;

    public ModifierReference(IValue<S> value) {
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
      if (symbol != null)
        builder.Append(symbol);

      builder.Append(value);
      return builder.ToString();
    }

  }
}
