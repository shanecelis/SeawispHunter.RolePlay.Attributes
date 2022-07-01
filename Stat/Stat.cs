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
public interface IStat<T> : IValue<T>, INotifyPropertyChanged {
  string name { get; }
  T baseValue { get; set; }
  T value { get; set; }
  // T modifiedvalue { get; }
  IEnumerable<IModifier<T>> modifiers { get; }
  void Add(IModifier<T> modifier);
  void Remove(IModifier<T> modifier);
  void Clear();
  // event PropertyChangedEventHandler PropertyChanged;
}

/* IValue<T> is notifies listeners when changed. */
public interface IValue<T> : INotifyPropertyChanged {
  T value { get; set; }
  // event PropertyChangedEventHandler PropertyChanged;
}

public class Value<T> : IValue<T> where T : IEquatable<T> {

  private T _value;
  public T value {
    get => _value;
    set {
      // Not valid for structs generally.
      if (_value != null && _value.Equals(value))
        return;
      _value = value;
      PropertyChanged?.Invoke(this, valueEventArgs);
    }
  }
  public event PropertyChangedEventHandler PropertyChanged;
  private static PropertyChangedEventArgs valueEventArgs = new PropertyChangedEventArgs(nameof(value));
}

public class Stat<T> : IStat<T> where T : IEquatable<T> {
  public string name { get; init; }
  public string description { get; init; }
  protected IList<IModifier<T>> _modifiers;
  public IEnumerable<IModifier<T>> modifiers => _modifiers == null ? Enumerable.Empty<IModifier<T>>() : _modifiers;

  protected T _baseValue;
  public virtual T baseValue {
    get => _baseValue;
    set {
      if (_baseValue != null && _baseValue.Equals(value))
        return;
      _baseValue = value;
      OnChange(nameof(baseValue));
    }
  }
  public T value {
    get {
      T v = baseValue;
      foreach (var modifier in modifiers)
        v = modifier.Modify(v);
      return v;
    }
    set => throw new InvalidOperationException("Cannot set `value` in Stat<T> class. Consider setting `baseValue instead.");
  }
  public event PropertyChangedEventHandler PropertyChanged;
  protected static PropertyChangedEventArgs modifiersEventArgs
    = new PropertyChangedEventArgs(nameof(modifiers));

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, name == nameof(modifiers)
                            ? modifiersEventArgs
                            : new PropertyChangedEventArgs(name));
  }

  protected void ModifiersChanged(object sender, PropertyChangedEventArgs e) {
    PropertyChanged?.Invoke(this, modifiersEventArgs);
  }

  public void Add(IModifier<T> modifier) {
    if (_modifiers == null)
      _modifiers = new List<IModifier<T>>();
    modifier.PropertyChanged -= ModifiersChanged;
    modifier.PropertyChanged += ModifiersChanged;
    _modifiers.Add(modifier);
    OnChange(nameof(modifiers));
  }

  public void Remove(IModifier<T> modifier) {
    if (_modifiers == null)
      return;
    modifier.PropertyChanged -= ModifiersChanged;
    _modifiers.Remove(modifier);
    OnChange(nameof(modifiers));
  }

  public void Clear() {
    _modifiers?.Clear();
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

/* This stat's base value is given by a Func<T> or another stat's value. */
public class DerivedStat<T> : Stat<T>, IDisposable where T : IEquatable<T> {
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
