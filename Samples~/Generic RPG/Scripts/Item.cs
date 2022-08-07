/* Original code[1] Copyright (c) 2022 Seawisp Hunter, LLC[2]
   Licensed under the Unity Asset Store End User License[3]

   Author: Shane Celis[4]

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://seawisphunter.com
   [3]: https://unity3d.com/legal/as_terms
   [4]: https://twitter.com/shanecelis
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SeawispHunter.RolePlay.Attributes;
using System.Text;

namespace SeawispHunter.RolePlay.Attributes.Samples {

/** Some problems with this class.

    - We don't know how many attributes there are, so no exhaustive operations
      like Clear() are possible.

    - Conceivably, we could implement ICollection<ITargetedModifier>; however,
      then we'd be constraining all the modifiers to be of one type only.

    XXX ModifierValueDict
    XXX ModifierValuesBag
  */
public class TargetedModifiersCollection<S> {
  private readonly S bag;
  public TargetedModifiersCollection(S bag) => this.bag = bag;

  public void Add<T>(ITargetedModifier<S,T> targeted) => Add(0, targeted);
  public void Add<T>(int priority, ITargetedModifier<S,T> targeted)
    => targeted.AppliesTo(bag).modifiers.Add(priority, targeted.modifier);

  public bool Contains<T>(ITargetedModifier<S,T> targeted)
    => targeted.AppliesTo(bag).modifiers.Contains(targeted.modifier);

  public bool Remove<T>(ITargetedModifier<S,T> targeted)
    => targeted.AppliesTo(bag).modifiers.Remove(targeted.modifier);

}

// public static class TargetedModifier {

//   public static void Add<S,T>(this ITargetedModifier<S,T> targeted, S bag)
//     => targeted.AppliesTo(bag).modifiers.Add(targeted.modifier);
//   public static bool Remove<S,T>(this ITargetedModifier<S,T> targeted, S bag)
//     => targeted.AppliesTo(bag).modifiers.Remove(targeted.modifier);
//   public static bool Contains<S,T>(this ITargetedModifier<S,T> targeted, S bag)
//     => targeted.AppliesTo(bag).modifiers.Remove(targeted.modifier);
//   // public void Remove(S bag) => target(bag).modifiers.Remove(this);
// }

// public class ApplicableModifier<S, T> : IModifier<T> {
//   Func<S, IModifiableValue<T>> target;
//   IModifier<T> inner;

//   public bool enabled {
//     get => inner.enabled;
//     set => inner.enabled = value;
//   }
//   public T Modify(T given) => inner.Modify(given);

//   public void Add(S bag) => target(bag).modifiers.Add(this);
//   public void Remove(S bag) => target(bag).modifiers.Remove(this);
// }

public class Item : MonoBehaviour {
  public string _name;
  [System.Serializable]
  public class Entry {
    public AttributeKind attribute;
    public float bonus;
  }
  public Entry[] additiveBonuses;
  public Entry[] percentileBonuses;

  public class ItemModifier : NumericalModifier<IReadOnlyValue<float>, float> {
    public Item source;
    public int priority;
    public AttributeKind kind;
    public ItemModifier(IReadOnlyValue<float> context) : base(context) { }
    public static ItemModifier Plus(float value, Item source)
      => new ItemModifier(new ReadOnlyValue<float>(value)) {
      source = source,
      symbol = '+' };
    public static ItemModifier Percent(float value, Item source)
      => new ItemModifier(new ReadOnlyValue<float>(1f + value)) {
      source = source,
      symbol = '*' };
    public override string ToString() {
      if (symbol == '+')
        return $"+{Math.Round(context.value,4)}";
      else
        return $"+{Math.Round((context.value - 1f)*100, 4)}%";
    }
  }

  public IEnumerable<ItemModifier> modifiers {
    get {
      foreach (var entry in additiveBonuses)
        yield return new ItemModifier(new ReadOnlyValue<float>((float) entry.bonus)) {
          source = this,
            symbol = '+',
            kind = entry.attribute
            };

      foreach (var entry in percentileBonuses)
        yield return new ItemModifier(new ReadOnlyValue<float>(1f + entry.bonus)) {
          source = this,
            symbol = '*',
            kind = entry.attribute,
            priority = 100,
            };
    }
  }

  public IEnumerable<ITargetedModifier<Character, float>> targetedModifiers {
    get {
      foreach (var modifier in modifiers)
        yield return modifier.Target((Character c) => c.attributes[(int) modifier.kind]);
    }
  }


  public void AddModifiers(IModifiableValue<float>[] attributes) {
    foreach (var modifier in this.modifiers)
      attributes[(int) modifier.kind].modifiers.Add(modifier.priority, modifier);
  }

  public void AddModifiers(Character c) {
    foreach (var modifier in this.targetedModifiers)
      c.modifiers.Add<float>(modifier);
  }

  public void RemoveModifiers(IModifiableValue<float>[] attributes) {
    foreach (var attribute in attributes)
      RemoveAllModifiers(attribute);
  }

  public void RemoveModifiers(Character c) {
    foreach (var targeted in this.targetedModifiers)
      RemoveAllModifiers(targeted.AppliesTo(c));
  }

  private void RemoveAllModifiers(IModifiableValue<float> attr) {
    foreach (var modifier in attr.modifiers
             .OfType<ItemModifier>()
             .Where(mod => mod.source == this)
             .ToList())
      attr.modifiers.Remove(modifier);
  }

  void Awake() {
    var tooltip = GetComponent<TooltipOnHover>();
    if (! string.IsNullOrEmpty(_name) && tooltip != null)
      tooltip.messageDelegate = ToString;
  }

  public override string ToString() {
    var sb = new StringBuilder();
    sb.Append(_name);
    sb.Append(':');
    sb.Append(' ');
    // sb.AppendLine();
    foreach (var modifier in modifiers) {
      sb.Append(modifier.ToString());
      sb.Append(' ');
      sb.Append(modifier.kind.ToAbbreviatedString());
      sb.AppendLine();
    }
    return sb.ToString();
  }
}

}
