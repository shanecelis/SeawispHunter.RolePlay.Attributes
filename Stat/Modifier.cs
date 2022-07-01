using System.Text;
using System.ComponentModel;

namespace SeawispHunter.Game.Stat;

public interface IModifier<T> : INotifyPropertyChanged {
  string name { get; }
  T Modify(T given);
  // event PropertyChangedEventHandler PropertyChanged;
}

// public enum Operation {
//   Add,
//   Times,
//   Substitute
// }

// internal abstract class Modifier2<T> : IModifier<T>, IValue<T> where T : struct {

//   public event PropertyChangedEventHandler PropertyChanged;
//   public string name { get; init; }
//   public T value { get; set; }
//   public abstract T Modify(T given);

//   protected void OnChange(string name) {
//     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
//   }
// }

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

