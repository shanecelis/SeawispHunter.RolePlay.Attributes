/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   This comment generated by code-cite[3].

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
   [4]: https://github.com/shanecelis/code-cite
*/
using System;
using System.ComponentModel;

#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace SeawispHunter.RolePlay.Attributes {

[Serializable]
public class Value<T> : IValue<T> {

#if UNITY_5_3_OR_NEWER
  [UnityEngine.SerializeField]
#endif
  protected T _value;
  public virtual T value {
    get => _value;
    set {
      _value = value;
      // This is more costly.
      // OnChange(nameof(value))
      // So let's just do this.
      PropertyChanged?.Invoke(this, valueEventArgs);
    }
  }
  public Value() {}
  public Value(T value) => _value = value;

  public event PropertyChangedEventHandler PropertyChanged;

  private static PropertyChangedEventArgs valueEventArgs = new PropertyChangedEventArgs(nameof(value));

  protected void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}

public static class ReadOnlyValue {
  public static IReadOnlyValue<T> Create<T>(Func<T> f, out Action callOnChange)
    => new DerivedReadOnlyValue<T>(f, out callOnChange);

  public static IReadOnlyValue<T> Create<T>(Func<T> f)
    => new DerivedReadOnlyValue<T>(f, out var callOnChange);

  internal class DerivedReadOnlyValue<T> : IReadOnlyValue<T> {
    private Func<T> func;

    public DerivedReadOnlyValue(Func<T> func, out Action callOnChange) {
      this.func = func;
      callOnChange = OnChange;
    }

    public T value => func();

    public event PropertyChangedEventHandler PropertyChanged;
    private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(value));

    protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
  }

}

public static class Value {

  public static IValue<T> Create<T>(Func<T> @get, Action<T> @set, out Action callOnChange)
    => new DerivedValue<T>(@get, @set, out callOnChange);

  internal class DerivedValue<T> : IValue<T> {
    private readonly Func<T> @get;
    private readonly Action<T> @set;

    public DerivedValue(Func<T> @get, Action<T> @set, out Action callOnChange) {
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
}

/** Represents an IValue<T> that respects some bounds. Bounds may be dynamic.

    If bounds change impinge on this object's current value, that value will
    change.

    Why no BoundedReadOnlyValue<T>? It's not truly needed. With a read only
    value, one only needs to clamp it for instance, a simple projection will do:

    ```
    var boundedValue = readOnlyValue.Select(x => Math.Clamp(x, 0f, myMax.value));
    ```

    A special implementation is required for IValue<T> because it can be set,
    and although one can always clamp it's output as above, it won't function
    correctly. For instance, if you have `health` that is 100, you subtract 120.
    It will report 0. But when you add 10, it'll still report 0 because the
    underlying value would actually be at -10.
    */
[Serializable]
public class BoundedValue<T> : IValue<T>, IBounded<T>
#if NET7_0_OR_GREATER
  where T : INumber<T>
#endif
{
  public readonly IReadOnlyValue<T> lowerBound;
  public readonly IReadOnlyValue<T> upperBound;
  private T _value;

  public T minValue => lowerBound.value;
  public T maxValue => upperBound.value;

  public T value {
    get => _value;
    set {
      _value = Clamp(value, minValue, maxValue);
      OnChange();
    }
  }

  public static T Clamp(T value, T minValue, T maxValue) {
#if NET7_0_OR_GREATER
    if (value < minValue)
      value = minValue;
    if (value > maxValue)
      value = maxValue;
    return value;
#else
    var op = Modifier.GetOp<T>();
    return op.Max(minValue, op.Min(maxValue, value));
#endif
  }

  public BoundedValue(T value, IReadOnlyValue<T> lowerBound, IReadOnlyValue<T> upperBound) {
    _value = value;
    this.lowerBound = lowerBound;
    // this.lowerBound.PropertyChanged -= BoundChanged;
    this.lowerBound.PropertyChanged += BoundChanged;
    this.upperBound = upperBound;
    // this.upperBound.PropertyChanged -= BoundChanged;
    this.upperBound.PropertyChanged += BoundChanged;
  }
  public BoundedValue(T value, T lowerBound, IReadOnlyValue<T> upperBound)
    : this(value, new ReadOnlyValue<T>(lowerBound), upperBound) { }

  public BoundedValue(T value, IReadOnlyValue<T> lowerBound, T upperBound)
    : this(value, lowerBound, new ReadOnlyValue<T>(upperBound)) { }

  public BoundedValue(T value, T lowerBound, T upperBound)
    : this(value, new ReadOnlyValue<T>(lowerBound), new ReadOnlyValue<T>(upperBound)) { }

  protected void BoundChanged(object sender, PropertyChangedEventArgs e) {
    this.value = _value;
  }

  public event PropertyChangedEventHandler PropertyChanged;
  private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(value));

  protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
}

/** A simple read only value. */
[Serializable]
public class ReadOnlyValue<T> : IReadOnlyValue<T> {
#if UNITY_5_3_OR_NEWER
  [UnityEngine.SerializeField]
#endif
  private T _value;
  public T value => _value;
  public ReadOnlyValue(T value) => _value = value;
  public ReadOnlyValue(T value, Action callOnChange) : this(value) => callOnChange = OnChange;

  // HACK: Although this optimization is tempting, it
  // may not work for class values.

  // We don't ever call this because we don't change.
  // This isn't strictly true unless T is a struct.
  // public event PropertyChangedEventHandler PropertyChanged {
  //   add { }
  //   remove { }
  // }

  public event PropertyChangedEventHandler PropertyChanged;
  private static PropertyChangedEventArgs eventArgs = new PropertyChangedEventArgs(nameof(value));

  protected void OnChange() => PropertyChanged?.Invoke(this, eventArgs);
  public override string ToString() => value.ToString();

}
}
