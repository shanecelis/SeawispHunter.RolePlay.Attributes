/* Original code[1] Copyright (c) 2022 Seawisp Hunter, LLC[2]
   Licensed under the Unity Asset Store End User License[3]

   Author: Shane Celis[4]

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://seawisphunter.com
   [3]: https://unity3d.com/legal/as_terms
   [4]: https://twitter.com/shanecelis
*/

using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SeawispHunter.RolePlay.Attributes;
using UnityEngine.UI;

namespace SeawispHunter.RolePlay.Attributes.Samples {
public enum AttributeKind {
  Attack,
  Defense,
  Agility,
  Magic
};

public static class AttributeKindExtensions {
  public static string ToAbbreviatedString(this AttributeKind attr) {
    switch (attr) {
      case AttributeKind.Attack:
        return "ATK";
      case AttributeKind.Defense:
        return "DEF";
      case AttributeKind.Agility:
        return "AGI";
      case AttributeKind.Magic:
        return "MAG";
      default:
        throw new System.InvalidOperationException();
    }
  }
}

public class Character : MonoBehaviour {
  public ModifiableValue<float> attack;
  public ModifiableValue<float> defense;
  public ModifiableValue<float> agility;
  public ModifiableValue<float> magic;

  [NonSerialized]
  public ModifiableValue<float>[] attributes;
  [Space]
  public Text[] attributeValues;
  public Toggle[] toggles;
  public Button[] buttons;

  public bool lerpChanges;

  void Awake() {
    // Cursor.visible = true;
    attributes = new [] { attack, defense, agility, magic };

    for (int i = 0; i < attributes.Length; i++) {
      int j = i; // We need this for the closure.
      attributeValues[j].text = attributes[j].value.ToString();
      if (lerpChanges) {
        IReadOnlyValue<string> lerpy = attributes[j]
          .LerpOnChange(this, 1f, 0.1f) // Lerp for 1 second on 1/10 of second intervals.
          .Select(x => x.ToString("0.#"));
        lerpy.OnChange(attr => attributeValues[j].text = attr.value.ToString());
      } else
        attributes[j].OnChange(attr => attributeValues[j].text = attr.value.ToString());
      
      // Set up the tooltips.
      var tooltip = attributeValues[j].transform.parent.GetComponent<TooltipOnHover>();
      tooltip.messageDelegate = () => AttributeTooltip(attributes[j]);
    }
    foreach (var toggle in toggles) {
      // Setup initially.
      HandleToggle(toggle);
      // Setup for the long haul.
      toggle.onValueChanged.AddListener(yes => HandleToggle(toggle));
    }

    foreach (var button in buttons)
      button.onClick.AddListener(() => HandleButton(button));
  }

  void HandleToggle(Toggle toggle) {
    var item = toggle.GetComponent<Item>();
    if (item == null)
      return;
    if (toggle.isOn)
      item.AddModifiers(attributes);
    else
      item.RemoveModifiers(attributes);
  }

  void HandleButton(Button button) {
    var item = button.GetComponent<Item>();
    if (item == null)
      return;
    // Button hit.
    item.AddModifiers(attributes);
  }

  string AttributeTooltip(ModifiableValue<float> attr) {
    var sb = new StringBuilder();
    int i = Array.IndexOf(attributes, attr);
    if (i < 0)
      return "Unknown";
    var kind = (AttributeKind) i;
    sb.Append(attr.value);
    sb.Append(' ');
    sb.Append(kind.ToAbbreviatedString());
    sb.AppendLine();
    sb.Append(attr.initial.value);
    sb.Append(' ');
    sb.Append("BASE");
    sb.AppendLine();
    foreach (var modifier in attr.modifiers.OfType<Item.ItemModifier>()) {
      sb.Append(modifier.ToString());
      sb.Append(' ');
      sb.Append(modifier.source._name);
      sb.AppendLine();
    }
    return sb.ToString();
  }

}
}
