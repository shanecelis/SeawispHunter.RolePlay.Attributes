using System.Text;
using System.ComponentModel;

namespace SeawispHunter.RolePlay.Attributes;

/* IValue<T> notifies listeners when changed. That's it. */
public interface IValue<T> : INotifyPropertyChanged {
  /** NOTE: This might seem weird to not have a setter since it will notify you
      when it changes. However, if you consider a value that is not provided by
      a field but by some other thing like a `Func<T>` then it makes more sense.
      However, the implementation often will have a setter like `Value<T>`
      does. */
  T value { get; }
  // event PropertyChangedEventHandler PropertyChanged;
}

/* IMutableValue<T> is an IValue<T> that can be directly changed. */
public interface IMutableValue<T> : IValue<T> {
  /* We want this to be settable. */
  new T value { get; set; }
}

/** This IModifiableValue<T> class is meant to capture values in games like health,
    strength, etc. that can be modified by various, some distal, effects.

    ## Acknowledgments

    This class was informed by the following sources:

    - http://howtomakeanrpg.com/a/how-to-make-an-rpg-stats.html
    - https://jkpenner.wordpress.com/2015/06/09/rpgsystems-stat-system-02-modifiers/
    - https://gamedevelopment.tutsplus.com/tutorials/using-the-composite-design-pattern-for-an-rpg-attributes-system--gamedev-243

    XXX: Consider renaming to IDependantValue? IModifiableValue?
 */
public interface IModifiableValue<T> : IValue<T>, INotifyPropertyChanged {
  T baseValue { get; set; }
  // T value { get; set; }
  IEnumerable<IModifier<T>> modifiers { get; }
  void Add(IModifier<T> modifier);
  void Remove(IModifier<T> modifier);
  /** Remove all modifiers. */
  void Clear();
  // event PropertyChangedEventHandler PropertyChanged;
}

public static class Value {
  public static IValue<T> FromFunc<T>(Func<T> f, out Action callOnChange) => new DerivedValue<T>(f, out callOnChange);
  public static IValue<T> FromFunc<T>(Func<T> f) => new DerivedValue<T>(f, out var callOnChange);

  internal class DerivedValue<T> : IValue<T> {
    private Func<T> func;
    public DerivedValue(Func<T> func, out Action callOnChange) {
      this.func = func;
      callOnChange = OnChange;
    }
    public T value => func();
    public event PropertyChangedEventHandler PropertyChanged;
    private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(value));
    protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
  }
}

public static class ModifiableValue {
  public static IModifiableValue<T> FromFunc<T>(Func<T> f, out Action callOnChange) => new DerivedModifiableValue<T>(f, out callOnChange);
  public static IModifiableValue<T> FromFunc<T>(Func<T> f) => new DerivedModifiableValue<T>(f, out var callOnChange);

  public static IModifiableValue<T> FromValue<T>(IValue<T> v) => new DerivedModifiableValue<T>(v);
  public static IModifiableValue<T> FromValue<T>(IValue<T> v, string name) => new DerivedModifiableValue<T>(v) { };

  /* This stat's base value is given by a Func<T> or another stat's value. */
  internal class DerivedModifiableValue<T> : ModifiableValue<T>, IDisposable {
    public readonly Func<T> func;
    public override T baseValue => func();
    private Action onDispose = null;

    public DerivedModifiableValue(Func<T> func, out Action callOnChange) {
      this.func = func;
      callOnChange = () => OnChange(nameof(baseValue));
    }

    public DerivedModifiableValue(Func<T> func) {
      this.func = func;
    }

    public DerivedModifiableValue(IValue<T> value) : this(() => value.value) {
      value.PropertyChanged -= BaseChanged;
      value.PropertyChanged += BaseChanged;
      onDispose = () => value.PropertyChanged -= BaseChanged;
    }

    protected void BaseChanged(object sender, PropertyChangedEventArgs e) {
      OnChange(nameof(baseValue));
    }

    public void Dispose() {
      onDispose?.Invoke();
    }
  }
}


public static class ValueExtensions {
  /** Give ourselves a little projection. */
  public static IValue<T> Select<S,T>(this IValue<S> v, Func<S,T> func) {
    /** HACK: This has problems. This "derived" value is still settable. It gets
        updated whenever v is set, which seems a little "magic-y" and is unlike
        the ModifiableValue which notifies but doesn't update any state.
      */
    // var w = new Value<T> { value = func(v.value) };
    // v.PropertyChanged += (_, _) => w.value = func(v.value);
    // return w;
    var w = Value.FromFunc(() => func(v.value), out var callOnChange);
    v.PropertyChanged += (_, _) => callOnChange();
    return w;
  }

}

public class Value<T> : IMutableValue<T> {

  private T _value;
  public virtual T value {
    get => _value;
    set {
      // Not valid for structs generally.
      if (_value != null && _value.Equals(value))
        return;
      _value = value;
      OnChange(nameof(value));
    }
  }
  // Looks like this isn't technically needed. Good.
  // T IValue<T>.value => _value;
  public event PropertyChangedEventHandler PropertyChanged;
  private static PropertyChangedEventArgs valueEventArgs = new PropertyChangedEventArgs(nameof(value));

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, name == nameof(value)
                            ? valueEventArgs
                            : new PropertyChangedEventArgs(name));
  }
}

public class ModifiableValue<T> : IModifiableValue<T> {
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
  }
  public event PropertyChangedEventHandler PropertyChanged;
  private static PropertyChangedEventArgs modifiersEventArgs
    = new PropertyChangedEventArgs(nameof(modifiers));

  protected void Chain(object sender, PropertyChangedEventArgs args) => OnChange(nameof(value));

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
    if (_modifiers != null) {
      _modifiers.Clear();
      OnChange(nameof(modifiers));
    }
  }

  public override string ToString() => value.ToString();
  public string ToString(bool showModifiers) {
    if (! showModifiers)
      return ToString();
    var builder = new StringBuilder();
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

