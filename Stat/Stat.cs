using System.Text;
using System.ComponentModel;

namespace SeawispHunter.Game.Stat;

/** This IStat<T> class is meant to capture the many stats in games like health,
    strength, etc. that can be modified by many different effects.

    ## Acknowledgments

    This class was informed by the following sources:

    - http://howtomakeanrpg.com/a/how-to-make-an-rpg-stats.html
    - https://jkpenner.wordpress.com/2015/06/09/rpgsystems-stat-system-02-modifiers/
 */
public interface IStat<T> : INotifyPropertyChanged {
  string name { get; }
  T baseValue { get; }
  T value { get; }
  IList<IModifier<T>> modifiers { get; }
  void AddModifier(IModifier<T> modifier, bool notifyOnModifierChange);
  void RemoveModifier(IModifier<T> modifier);
  event PropertyChangedEventHandler PropertyChanged;
}

public interface IModifier<T> : INotifyPropertyChanged {
  string name { get; }
  T Modify(T given);
  event PropertyChangedEventHandler PropertyChanged;
}


public abstract class Modifier<T> : IModifier<T> where T : struct {
  public string name { get; init; }
  public string description { get; init; }
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
  private float? _plus;
  public override float? plus {
    get => _plus ?? base.plus;
    init => _plus = value;
  }
  public void SetPlus(float value) {
    if (_plus.HasValue && _plus.Value == value)
      return;
    _plus = value;
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

public class Stat<T> : IStat<T> {
  public string name { get; init; }
  public string description { get; init; }
  public IList<IModifier<T>> modifiers { get; } = new List<IModifier<T>>();
  public virtual T baseValue { get; init; }
  public T value {
    get {
      T v = baseValue;
      foreach (var modifier in modifiers)
        v = modifier.Modify(v);
      return v;
    }
  }
  public event PropertyChangedEventHandler PropertyChanged;
  protected static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(modifiers));
  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }

  public void ModifierChanged(object sender, PropertyChangedEventArgs e) {
    PropertyChanged?.Invoke(this, eventArgs);
  }

  public void AddModifier(IModifier<T> modifier, bool notifyOnModifierChange) {
    if (notifyOnModifierChange) {
      modifier.PropertyChanged -= ModifierChanged;
      modifier.PropertyChanged += ModifierChanged;
    }
    modifiers.Add(modifier);
    OnChange(nameof(modifiers));
  }

  public void RemoveModifier(IModifier<T> modifier) {
    modifier.PropertyChanged -= ModifierChanged;
    modifiers.Remove(modifier);
    OnChange(nameof(modifiers));
  }

  public override string ToString() => $"{name} {value}";
  public string ToString(bool showModifiers) {
    if (! showModifiers)
      return ToString();
    var builder = new StringBuilder();
    builder.Append(name);
    builder.Append(" \"base\" ");
    builder.Append(baseValue);
    builder.Append(' ');
    foreach (var modifier in modifiers) {
      builder.Append(modifier);
      builder.Append(' ');
    }
    builder.Append("-> ");
    builder.Append(value);
    return builder.ToString();
  }
}

public class DerivedStat<T> : Stat<T>, IDisposable {
  public readonly Func<T> func;
  public override T baseValue => func();
  private Action onDispose = null;

  public DerivedStat(Func<T> func, out Action funcChanged) {
    this.func = func;
    funcChanged = () => OnChange(nameof(baseValue));
  }

  public DerivedStat(Func<T> func) {
    this.func = func;
  }

  public DerivedStat(IStat<T> stat) : this(() => stat.value) {
    stat.PropertyChanged -= BaseChanged;
    stat.PropertyChanged += BaseChanged;
    onDispose = () => stat.PropertyChanged -= BaseChanged;
  }

  protected void BaseChanged(object sender, PropertyChangedEventArgs e) {
    OnChange(nameof(baseValue));
  }

  public void Dispose() {
    onDispose?.Invoke();
  }
}
