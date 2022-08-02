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

  public void AddModifiers(IModifiableValue<float>[] attributes) {
    foreach (var modifier in this.modifiers)
      attributes[(int) modifier.kind].modifiers.Add(modifier);
  }

  public void RemoveModifiers(IModifiableValue<float>[] attributes) {
    foreach (var attribute in attributes)
      RemoveAllModifiers(attribute);
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
