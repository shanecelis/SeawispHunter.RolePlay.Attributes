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

  public void AddModifier(IModifier<T> modifier, bool notifyOnModifierChange) {
    if (notifyOnModifierChange) {
      modifier.PropertyChanged -= ModifiersChanged;
      modifier.PropertyChanged += ModifiersChanged;
    }
    modifiers.Add(modifier);
    OnChange(nameof(modifiers));
  }

  public void RemoveModifier(IModifier<T> modifier) {
    modifier.PropertyChanged -= ModifiersChanged;
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

/* This stat's base value is given by a Func<T> or another stat's value. */
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
