using System;
using System.ComponentModel;

namespace SeawispHunter.RolePlay.Attributes;

public class Value<T> : IMutableValue<T> {
  // HACK: This just seems like it's too much.
  /** When value is set, pass through `setter()` first. */
  public Func<T,T> setter;

  protected T _value;
  public virtual T value {
    get => _value;
    set {
      if (setter == null)
        _value = value;
      else
        _value = setter(value);
      OnChange(nameof(value));
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private static PropertyChangedEventArgs valueEventArgs = new PropertyChangedEventArgs(nameof(value));

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, name == nameof(value)
                                    ? valueEventArgs
                                    : new PropertyChangedEventArgs(name));
  }
}

public static class Value {
  public static IValue<T> FromFunc<T>(Func<T> f, out Action callOnChange) => new DerivedValue<T>(f, out callOnChange);

  public static IValue<T> FromFunc<T>(Func<T> f) => new DerivedValue<T>(f, out var callOnChange);

  public static IMutableValue<T> FromFunc<T>(Func<T> f, Action<T> @set, out Action callOnChange)
    => new DerivedMutableValue<T>(f, @set, out callOnChange);

  public static IMutableValue<T> WithBounds<T>(T value, T lowerBound, T upperBound)
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
    => new BoundedValue<T>(value,
                           new ReadOnlyValue<T> { value = lowerBound },
                           new ReadOnlyValue<T> { value = upperBound });

  public static IMutableValue<T> WithBounds<T>(T value, T lowerBound, IValue<T> upperBound)
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
    => new BoundedValue<T>(value,
                           new ReadOnlyValue<T> { value = lowerBound },
                           upperBound);

  public static IMutableValue<T> WithBounds<T>(T value, IValue<T> lowerBound, T upperBound)
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
    => new BoundedValue<T>(value,
                           lowerBound,
                           new ReadOnlyValue<T> { value = upperBound });

  public static IMutableValue<T> WithBounds<T>(T value, IValue<T> lowerBound, IValue<T> upperBound)
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
    => new BoundedValue<T>(value,
                           lowerBound,
                           upperBound);

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

  internal class DerivedMutableValue<T> : IMutableValue<T> {
    private readonly Func<T> @get;
    private readonly Action<T> @set;

    public DerivedMutableValue(Func<T> @get, Action<T> @set, out Action callOnChange) {
      this.@get = @get;
      this.@set = @set;
      callOnChange = OnChange;
    }

    public T value {
      get => @get();
      set => @set(value);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(value));

    protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
  }

  internal class BoundedValue<T> : IMutableValue<T>
#if NET6_0_OR_GREATER
    where T : INumber<T>
#else
    where T : IEquatable<T>
#endif
  {
    public readonly IValue<T> lowerBound;
    public readonly IValue<T> upperBound;
    private T _value;

    public T value {
      get => _value;
      set {
#if NET6_0_OR_GREATER
        _value = value;
        if (_value < lowerBound.value)
          _value = lowerBound.value;
        if (_value > upperBound.value)
          _value = upperBound.value;
#else
        var op = Modifier.GetOp<T>();
        _value = op.Max(lowerBound.value, op.Min(upperBound.value, value));
#endif
        OnChange();
      }
    }

    public BoundedValue(T value, IValue<T> lowerBound, IValue<T> upperBound) {
      _value = value;
      this.lowerBound = lowerBound;
      // this.lowerBound.PropertyChanged -= BoundChanged;
      this.lowerBound.PropertyChanged += BoundChanged;
      this.upperBound = upperBound;
      // this.upperBound.PropertyChanged -= BoundChanged;
      this.upperBound.PropertyChanged += BoundChanged;
    }

    protected void BoundChanged(object sender, PropertyChangedEventArgs e) {
      var oldValue = _value;
      var newValue = value;
      if (! oldValue.Equals(newValue))
        OnChange();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(value));

    protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
  }

  internal class ReadOnlyValue<T> : IValue<T> {
    public T value { get; init; }

    // We don't ever call this because we don't change.
    public event PropertyChangedEventHandler PropertyChanged {
      add { }
      remove { }
    }
  }

}