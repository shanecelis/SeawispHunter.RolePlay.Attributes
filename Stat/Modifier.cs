using System.Text;
using System.ComponentModel;

namespace SeawispHunter.Game.Stat;

/** A IModifier<T> modifies an IStat<T>'s value. */
public interface IModifier<T> : INotifyPropertyChanged {
  string name { get; }
  T Modify(T given);
  // event PropertyChangedEventHandler PropertyChanged;
}

public interface IModifierValue<S,T> : IModifier<T>, IValue<S> {
  /* We want this to be settable. */
  new S value { get; set; }
}

/** Most modifiers will be of the same type as the stat they're modifying, so
    let's make that easier to express. */
public interface IModifierValue<T> : IModifierValue<T,T> { }

public static class Modifier {
  // public static IModifier<float> Plus(float f) => new Modifier<float> { plus = f };
  // public static IModifier<float> Plus(float f) => new ModifierFloat { plus = f };
  // public static IModifier<float> Plus(float f) => new ModifierFloat { plus = f };
  public static IModifierValue<float> Plus(float v, string name = null) => new ModifierValue<float> { value = v, op = (given, v) => given + v, name = name };
  public static IModifierValue<float> Multiply(float v, string name = null) => new ModifierValue<float> { value = v, op = (given, v) => given * v, name = name };
  public static IModifierValue<float> Substitute(float v, string name = null) => new ModifierValue<float> { value = v, op = (given, v) => v, name = name };

  public static IModifierValue<int> Plus(int v, string name = null) => new ModifierValue<int> { value = v, op = (given, v) => given + v, name = name };
  public static IModifierValue<int> Multiply(int v, string name = null) => new ModifierValue<int> { value = v, op = (given, v) => given * v, name = name };
  public static IModifierValue<int> Substitute(int v, string name = null) => new ModifierValue<int> { value = v, op = (given, v) => v, name = name };

  // public static IModifierValue<float> Plus(float v) => new ModifierValue<float, float> { value = v, op = (given, v) => given + v };
  // public static IModifierValue<float> Multiply(float v) => new ModifierValue<float, float> { value = v, op = (given, v) => given * v };
  // public static IModifierValue<float> Substitute(float v) => new ModifierValue<float, float> { value = v, op = (given, v) => v };
  // public static IModifier<float> Plus(IValue<float> v) => new ModifierValue<float, float> { value = v, op => (given, v) => given + v };


  internal class ModifierValue<S,T> : IModifierValue<S,T> {
    public string name { get; init; }
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
      this.PropertyChanged += (_, _) => m.value = func(this.value);
      return m;
    }
  }

  /** Most modifiers will be of the same type as the stat they're modifying, so
      let's make that easier to express. */
  internal class ModifierValue<T> : ModifierValue<T,T>, IModifierValue<T> { }
}


public abstract class Modifier<T> : IModifier<T> where T : struct, IEquatable<T> {
  public string name { get; init; }

  private T? _substitute;
  public virtual T? substitute {
    get => _substitute;
    set {
      if (_substitute != null && _substitute.Equals(value))
        return;
      _substitute = value;
      OnChange(nameof(substitute));
    }
  }
  private T? _plus;
  public virtual T? plus {
    get => _plus;
    set {
      if (_plus != null && _plus.Equals(value))
        return;
      _plus = value;
      OnChange(nameof(plus));
    }
  }
  private T? _multiply;
  public virtual T? multiply {
    get => _multiply;
    set {
      if (_multiply != null && _multiply.Equals(value))
        return;
      _multiply = value;
      OnChange(nameof(multiply));
    }
  }
  public abstract T Modify(T given);

  public event PropertyChangedEventHandler PropertyChanged;
  public override string ToString() {
    var builder = new StringBuilder();
    builder.Append('"');
    builder.Append(name);
    builder.Append('"');
    builder.Append(' ');
    if (substitute.HasValue) {
      builder.Append('=');
      builder.Append(substitute.Value);
      builder.Append(' ');
    }
    if (plus.HasValue){
      builder.Append('+');
      builder.Append(plus.Value);
      builder.Append(' ');
    }
    if (multiply.HasValue){
      builder.Append('*');
      builder.Append(multiply.Value);
      builder.Append(' ');
    }
    builder.Length--;
    return builder.ToString();
  }

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}

/** Until generic math is supported, we need this class. Support is coming in C# 7. */
public class ModifierFloat : Modifier<float> {
  public override float Modify(float given) {
    if (substitute.HasValue)
      given = substitute.Value;
    if (plus.HasValue)
      given += plus.Value;
    if (multiply.HasValue)
      given *= multiply.Value;
    return given;
  }
}

/** Until generic math is supported, we need this class. Support is coming in C# 7. */
public class ModifierInt : Modifier<int> {
  public override int Modify(int given) {
    if (substitute.HasValue)
      given = substitute.Value;
    if (plus.HasValue)
      given += plus.Value;
    if (multiply.HasValue)
      given *= multiply.Value;
    return given;
  }
}

/** A derived modifier allows for an IValue<S> or IStat<S> to modify some other stat. */
public abstract class DerivedModifier<S,T> : IModifier<T>, IDisposable where T : struct {
  public string name { get; init; }

  private IValue<S>? _substitute;
  public virtual IValue<S>? substitute {
    get => _substitute;
    init => _substitute = Listen(value);
  }
  private IValue<S>? _plus;
  public virtual IValue<S>? plus {
    get => _plus;
    init => _plus = Listen(value);
  }

  private IValue<S>? _multiply;
  public virtual IValue<S>? multiply {
    get => _multiply;
    init => _multiply = Listen(value);
  }
  protected IValue<S> Listen(IValue<S> value) {
    value.PropertyChanged -= ValueChanged;
    value.PropertyChanged += ValueChanged;
    return value;
  }

  public abstract T Modify(T given);

  public event PropertyChangedEventHandler PropertyChanged;
  public override string ToString() {
    var builder = new StringBuilder();
    builder.Append('"');
    builder.Append(name);
    builder.Append('"');
    builder.Append(' ');
    if (substitute != null) {
      builder.Append('=');
      builder.Append(substitute.value);
      builder.Append(' ');
    }
    if (plus != null){
      builder.Append('+');
      builder.Append(plus.value);
      builder.Append(' ');
    }
    if (multiply != null){
      builder.Append('*');
      builder.Append(multiply.value);
      builder.Append(' ');
    }
    builder.Length--;
    return builder.ToString();
  }

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }

  protected void ValueChanged(object sender, PropertyChangedEventArgs e) {
    OnChange("value");
  }
  public void Dispose() {
    if (substitute != null)
      substitute.PropertyChanged -= ValueChanged;
    if (plus != null)
      plus.PropertyChanged -= ValueChanged;
    if (multiply != null)
      multiply.PropertyChanged -= ValueChanged;
  }
}

public class DerivedModifierFloat : DerivedModifier<float, float> {
  public override float Modify(float given) {
    if (substitute != null)
      given = substitute.value;
    if (plus != null)
      given += plus.value;
    if (multiply != null)
      given *= multiply.value;
    return given;
  }
}

public class DerivedModifierFloatInt : DerivedModifier<float, int> {
  public override int Modify(int given) {
    float _given = (float) given;
    if (substitute != null)
      _given = substitute.value;
    if (plus != null)
      _given += plus.value;
    if (multiply != null)
      _given *= multiply.value;
    return (int) _given;
  }
}

public class DerivedModifierInt : DerivedModifier<int, int> {
  public override int Modify(int given) {
    if (substitute != null)
      given = substitute.value;
    if (plus != null)
      given += plus.value;
    if (multiply != null)
      given *= multiply.value;
    return given;
  }
}

public class DerivedModifierIntFloat : DerivedModifier<int, float> {
  public override float Modify(float given) {
    if (substitute != null)
      given = substitute.value;
    if (plus != null)
      given += plus.value;
    if (multiply != null)
      given *= multiply.value;
    return given;
  }
}

