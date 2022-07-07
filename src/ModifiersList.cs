using System.Collections;

namespace SeawispHunter.RolePlay.Attributes; 

public class ModifiersList<T> : IList<IModifier<T>> {
  private readonly ModifiableValue<T> parent;
  private readonly IList<IModifier<T>> modifiers = new List<IModifier<T>>();

  public ModifiersList(ModifiableValue<T> parent) => this.parent = parent;

  public IEnumerator<IModifier<T>> GetEnumerator() {
    return modifiers.GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return ((IEnumerable)modifiers).GetEnumerator();
  }

  public void Add(IModifier<T> modifier) {

    modifier.PropertyChanged -= parent.ModifiersChanged;
    modifier.PropertyChanged += parent.ModifiersChanged;
    modifiers.Add(modifier);
    // _modifiers.Add(modifier);
    parent.OnChange(nameof(modifiers));
  }

  public void Clear() {
    foreach (var modifier in modifiers)
      modifier.PropertyChanged -= parent.ModifiersChanged;
    modifiers.Clear();
    parent.OnChange(nameof(modifiers));
  }

  public bool Contains(IModifier<T> modifier) {
    return modifiers.Contains(modifier);
  }

  public void CopyTo(IModifier<T>[] array, int arrayIndex) {
    modifiers.CopyTo(array, arrayIndex);
  }

  public bool Remove(IModifier<T> modifier) {
    modifier.PropertyChanged -= parent.ModifiersChanged;
    var result = modifiers.Remove(modifier);
    parent.OnChange(nameof(modifiers));
    return result;
  }

  public int Count => modifiers.Count;

  public bool IsReadOnly => modifiers.IsReadOnly;

  public int IndexOf(IModifier<T> modifier) {
    return modifiers.IndexOf(modifier);
  }

  public void Insert(int index, IModifier<T> modifier) {
    modifier.PropertyChanged -= parent.ModifiersChanged;
    modifier.PropertyChanged += parent.ModifiersChanged;
    modifiers.Insert(index, modifier);
    parent.OnChange(nameof(modifiers));
  }

  public void RemoveAt(int index) {
    var modifier = modifiers[index];
    modifier.PropertyChanged -= parent.ModifiersChanged;
    modifiers.RemoveAt(index);
    parent.OnChange(nameof(modifiers));
  }

  public IModifier<T> this[int index] {
    get => modifiers[index];
    set => modifiers[index] = value;
  }
}
