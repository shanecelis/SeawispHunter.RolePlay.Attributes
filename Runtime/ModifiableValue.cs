/* Original code[1] Copyright (c) 2022 Shane Celis[2]
   Licensed under the MIT License[3]

   This comment generated by code-cite[3].

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://twitter.com/shanecelis
   [3]: https://opensource.org/licenses/MIT
   [4]: https://github.com/shanecelis/code-cite
*/
using System;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace SeawispHunter.RolePlay.Attributes {

public static class ModifiableValue {

  public static IModifiableValue<T> FromValue<T>(T v) where T : struct => new ModifiableValue<T>(new Value<T>(v));
#if UNITY_5_3_OR_NEWER
  public static IModifiableValue<T> FromValue<T>(Value<T> v) => new ModifiableValue<T>(v);
#else
  public static IModifiableValue<T> FromValue<T>(IValue<T> v) => new ModifiableValue<T>(v);
#endif
  public static IModifiable<IReadOnlyValue<T>,T> FromValue<T>(IReadOnlyValue<T> v) => new Modifiable<IReadOnlyValue<T>, T>(v);
}
#if UNITY_5_3_OR_NEWER
/* In order to make Unity's serialization work properly we need to have a
   concrete rather than interface as its initial value. */
[Serializable]
public class ModifiableValue<T> : Modifiable<Value<T>, T>, IModifiableValue<T> {

  public ModifiableValue(Value<T> initial) : base(initial) { }
  public ModifiableValue(T initialValue) : base(new Value<T>(initialValue)) { }
  public ModifiableValue() : this(default(T)) { }

  IValue<T> IModifiable<IValue<T>,T>.initial => _initial;
}

[Serializable]
public class ModifiableReadOnlyValue<T> : Modifiable<ReadOnlyValue<T>, T>, IModifiableReadOnlyValue<T> {

  public ModifiableReadOnlyValue(ReadOnlyValue<T> initial) : base(initial) { }
  public ModifiableReadOnlyValue(T initialValue) : base(new ReadOnlyValue<T>(initialValue)) { }
  public ModifiableReadOnlyValue() : this(default(T)) { }

  IReadOnlyValue<T> IModifiable<IReadOnlyValue<T>,T>.initial => _initial;

}
#else
[Serializable]
public class ModifiableValue<T> : ModifiableValue<IValue<T>, T>, IModifiableValue<T> {

  public ModifiableValue(IValue<T> initial) : base(initial) { }
  public ModifiableValue(T initialValue) : base(new Value<T>(initialValue)) { }
  public ModifiableValue() : this(default(T)) { }
}

[Serializable]
public class ModifiableReadOnlyValue<T> : ModifiableValue<IReadOnlyValue<T>, T>, IModifiableReadOnlyValue<T> {

  public ModifiableValue(IValue<T> initial) : base(initial) { }
  public ModifiableValue(T initialValue) : base(new Value<T>(initialValue)) { }
  public ModifiableValue() : this(default(T)) { }
}
#endif

[Serializable]
public class Modifiable<S,T> : IModifiable<S,T> where S : IReadOnlyValue<T> {

  protected ModifiersSortedList _modifiers;
  public IPriorityCollection<IModifier<T>> modifiers => _modifiers;
#if UNITY_5_3_OR_NEWER
  [UnityEngine.SerializeField]
#endif
  protected S _initial;
  public virtual S initial => _initial;
  // XXX: Consider caching?
  public T value {
    get {
      T v = initial.value;
      foreach (var modifier in modifiers)
        if (modifier.enabled)
          v = modifier.Modify(v);
      return v;
    }
  }
  [field: NonSerialized]
  public event PropertyChangedEventHandler PropertyChanged;
  private static PropertyChangedEventArgs modifiersEventArgs
    = new PropertyChangedEventArgs(nameof(modifiers));
  public Modifiable(S initial) {
    _initial = initial;
    _initial.PropertyChanged += Chain;
    _modifiers = new ModifiersSortedList(this);
  }

  protected void Chain(object sender, PropertyChangedEventArgs args)
    => OnChange(nameof(initial));

  internal void OnChangeModifiers() => PropertyChanged?.Invoke(this, modifiersEventArgs);
  internal void OnChange(string name) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }

  internal void ModifiersChanged(object sender, PropertyChangedEventArgs e) {
    PropertyChanged?.Invoke(this, modifiersEventArgs);
  }

  public IEnumerable<(T before, T after)> ModifierAffects(IModifier<T> modifier) {
    T before = initial.value;
    foreach (var _modifier in modifiers) {
      T after = before;
      if (_modifier.enabled)
        after = _modifier.Modify(before);
      if (modifier == _modifier)
        yield return (before, after);
      before = after;
    }
  }

  public override string ToString() => value.ToString();
  public string ToString(bool showModifiers) {
    if (! showModifiers)
      return ToString();
    var builder = new StringBuilder();
    builder.Append(" \"base\" ");
    builder.Append(initial);
    builder.Append(' ');
    foreach (var modifier in modifiers) {
      builder.Append(modifier);
      builder.Append(' ');
    }
    builder.Append("-> ");
    builder.Append(value);
    return builder.ToString();
  }

  /** A sorted list for modifiers. It uses a tuple (int priority, int age)
      because SortedList<K,V> can only store one value per key. We may have many
      modifiers with the same priority (default priority is 0). So modifiers are
      ordered by priority first and age second. Each modifier will have a unique
      age ensuring that the keys will be unique.
   */
  protected class ModifiersSortedList : IPriorityCollection<IModifier<T>>, IComparer<(int priority, int age)> {
    private readonly Modifiable<S,T> parent;
    private readonly SortedList<(int, int), IModifier<T>> modifiers = new SortedList<(int, int), IModifier<T>>();
    private int addCount = 0;
    public ModifiersSortedList(Modifiable<S,T> parent) => this.parent = parent;

    public IEnumerator<IModifier<T>> GetEnumerator() => modifiers.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(IModifier<T> modifier) => Add(0, modifier);
    public void Add(int priority, IModifier<T> modifier) {
      modifier.PropertyChanged -= parent.ModifiersChanged;
      modifier.PropertyChanged += parent.ModifiersChanged;
      modifiers.Add((priority, ++addCount), modifier);
      parent.OnChangeModifiers();
    }

    public void Clear() {
      foreach (var modifier in modifiers.Values)
        modifier.PropertyChanged -= parent.ModifiersChanged;
      modifiers.Clear();
      parent.OnChangeModifiers();
    }

    public bool Contains(IModifier<T> modifier) => modifiers.ContainsValue(modifier);

    public void CopyTo(IModifier<T>[] array, int arrayIndex) => modifiers.Values.CopyTo(array, arrayIndex);

    public bool Remove(IModifier<T> modifier) {
      int i = modifiers.IndexOfValue(modifier);
      if (i < 0)
        return false;
      modifier.PropertyChanged -= parent.ModifiersChanged;
      modifiers.RemoveAt(i);
      parent.OnChangeModifiers();
      return true;
    }

    public int Count => modifiers.Count;
    public bool IsReadOnly => false;

    public int Compare((int priority, int age) x, (int priority, int age) y) {
      int result = x.priority.CompareTo(y.priority);
      if (result != 0)
        return result;
      return x.age.CompareTo(y.age);
    }
  }
}

}
