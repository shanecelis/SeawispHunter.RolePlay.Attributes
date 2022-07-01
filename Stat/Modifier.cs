using System.Text;
using System.ComponentModel;

namespace SeawispHunter.Game.Stat;

public interface IModifier<T> : INotifyPropertyChanged {
  string name { get; }
  T Modify(T given);
  // event PropertyChangedEventHandler PropertyChanged;
}

// public abstract class Modifier2<T> : IModifier<T>, IValue<T> where T : struct {
//   public string name { get; init; }
// }



// public static class ModifierExtensions {
//   public static IModifier<Y> Select<X,Y>(IModifier<X> modifier) {
//   }
// }

public abstract class Modifier<T> : IModifier<T> where T : struct {
  public string name { get; init; }
  public virtual T? substitute { get; init; }
  public virtual T? plus { get; init; }
  public virtual T? multiply { get; init; }
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

public abstract class DerivedModifier<S,T> : IModifier<T>, IDisposable where T : struct {
  public string name { get; init; }

  private IStat<S>? _substitute;
  public virtual IStat<S>? substitute {
    get => _substitute;
    init => _substitute = Listen(value);
  }
  private IStat<S>? _add;
  public virtual IStat<S>? plus {
    get => _add;
    init => _add = Listen(value);
  }

  private IStat<S>? _multiply;
  public virtual IStat<S>? multiply {
    get => _multiply;
    init => _multiply = Listen(value);
  }
  protected IStat<S> Listen(IStat<S> stat) {
    stat.PropertyChanged -= StatChanged;
    stat.PropertyChanged += StatChanged;
    return stat;
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

  protected void StatChanged(object sender, PropertyChangedEventArgs e) {
    OnChange("stat");
  }
  public void Dispose() {
    if (substitute != null)
      substitute.PropertyChanged -= StatChanged;
    if (plus != null)
      plus.PropertyChanged -= StatChanged;
    if (multiply != null)
      multiply.PropertyChanged -= StatChanged;
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

public class MutableModifierFloat : ModifierFloat {
  private float? _add;
  public override float? plus {
    get => _add ?? base.plus;
    init => _add = value;
  }
  public void SetAdd(float value) {
    if (_add.HasValue && _add.Value == value)
      return;
    _add = value;
    OnChange(nameof(plus));
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
