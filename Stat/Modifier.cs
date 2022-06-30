using System.Text;
using System.ComponentModel;

namespace SeawispHunter.Game.Stat;

public interface IModifier<T> : INotifyPropertyChanged {
  string name { get; }
  T Modify(T given);
  event PropertyChangedEventHandler PropertyChanged;
}

public class FuncModifier<T> : IModifier<T>, IDisposable where T : struct {
  public readonly Func<T, T> func;
  public string name { get; init; }
  private Action onDispose = null;

  public FuncModifier(Func<T, T> func, out Action funcChanged) {
    this.func = func;
    funcChanged = () => OnChange(nameof(func));
  }

  public FuncModifier(Func<T, T> func) {
    this.func = func;
  }

  public T Modify(T given) => func(given);

  public event PropertyChangedEventHandler PropertyChanged;
  // public override string ToString() {
  //   var builder = new StringBuilder();
  //   builder.Append('"');
  //   builder.Append(name);
  //   builder.Append('"');
  //   builder.Append(' ');
  //   builder.Append('?');
  //   if (substitute.HasValue) {
  //     builder.Append('=');
  //     builder.Append(substitute.Value);
  //     builder.Append(' ');
  //   }
  //   if (add.HasValue){
  //     builder.Append('+');
  //     builder.Append(add.Value);
  //     builder.Append(' ');
  //   }
  //   if (multiply.HasValue){
  //     builder.Append('*');
  //     builder.Append(multiply.Value);
  //     builder.Append(' ');
  //   }
  //   builder.Length--;
  //   return builder.ToString();
  // }

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }

  public void Dispose() {
    onDispose?.Invoke();
  }

  protected void FuncChanged(object sender, PropertyChangedEventArgs e) {
    OnChange(nameof(func));
  }

  // public static FuncModifier<T> Add<T>(IStat<T> stat)  where T : struct{
  //   var modifier = new FuncModifier<T>(given => given + stat.value);

  //   stat.PropertyChanged -= modifier.FuncChanged;
  //   stat.PropertyChanged += modifier.FuncChanged;
  //   modifier.onDispose = () => stat.PropertyChanged -= modifier.FuncChanged;
  //   return modifier;
  // }
}

public abstract class Modifier<T> : IModifier<T> where T : struct {
  public string name { get; init; }
  // public string description { get; init; }
  public virtual T? substitute { get; init; }
  public virtual T? add { get; init; }
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
    if (add.HasValue){
      builder.Append('+');
      builder.Append(add.Value);
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

public abstract class DerivedModifier<T> : IModifier<T> where T : struct {
  public string name { get; init; }
  // public string description { get; init; }
  public virtual Func<T>? substitute { get; init; }
  public virtual Func<T>? add { get; init; }
  public virtual Func<T>? multiply { get; init; }
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
      builder.Append(substitute());
      builder.Append(' ');
    }
    if (add != null){
      builder.Append('+');
      builder.Append(add());
      builder.Append(' ');
    }
    if (multiply != null){
      builder.Append('*');
      builder.Append(multiply());
      builder.Append(' ');
    }
    builder.Length--;
    return builder.ToString();
  }

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}

public class DerivedModifierFloat : DerivedModifier<float> {
  public override float Modify(float given) {
    if (substitute != null)
      given = substitute();
    if (add != null)
      given += add();
    if (multiply != null)
      given *= multiply();
    return given;
  }

}

public class ModifierFloat : Modifier<float> {
  public override float Modify(float given) {
    if (substitute.HasValue)
      given = substitute.Value;
    if (add.HasValue)
      given += add.Value;
    if (multiply.HasValue)
      given *= multiply.Value;
    return given;
  }
}

public class MutableModifierFloat : ModifierFloat {
  private float? _add;
  public override float? add {
    get => _add ?? base.add;
    init => _add = value;
  }
  public void SetAdd(float value) {
    if (_add.HasValue && _add.Value == value)
      return;
    _add = value;
    OnChange(nameof(add));
  }
}

public class ModifierInt : Modifier<int> {
  public override int Modify(int given) {
    if (substitute.HasValue)
      given = substitute.Value;
    if (add.HasValue)
      given += add.Value;
    if (multiply.HasValue)
      given *= multiply.Value;
    return given;
  }
}

