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
          .LerpOnChange(this, Mathf.Lerp, 1f, 0.1f, InterruptMode.Serial) // Lerp for 1 second on 1/10 of second intervals.
          .Select(x => x.ToString("0.#"));

        // Let's add some text change juice.
        Vector4 originalColor = attributeValues[j].color;
        IValue<Vector4> textColor = new Value<Vector4>(originalColor);
        // var lerpyTextColor = textColor.LerpOnChange(this, LerpBack<Vector4>(Vector4.Lerp), 0.5f, 0.01f, InterruptMode.Serial);
        var lerpyTextColor = textColor.LerpOnChange(this, Vector4.Lerp, 0.5f, 0.01f, InterruptMode.Serial);
        // textColor.OnChange(attr => attributeValues[j].color = (Color) attr.value);
        lerpyTextColor.OnChange(attr => attributeValues[j].color = (Color) attr.value);

        lerpy.OnChange(attr => {
          attributeValues[j].text = attr.value;
          // textColor.value = Color.red;
          // StartCoroutine(LerpToAndBack<Vector4>(textColor, Vector4.Lerp, Color.blue, 1f, 0.1f));
        });
        var lastValue = attributes[j].value;
        attributes[j].OnChange(attr => {
          lerpyTextColor.Cancel(); // Cancel any current transition.
          textColor.value = attr.value > lastValue ? Color.green : Color.red;
          textColor.value = originalColor;
          lastValue = attr.value;
          // StartCoroutine(LerpToAndBack<Vector4>(textColor, Vector4.Lerp, Color.red, 1f, 0.1f));
        });
      } else {
        attributes[j].OnChange(attr => attributeValues[j].text = attr.value.ToString());
      }
      
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

  // public static IEnumerator LerpToAndBack<T>(IValue<T> v,
  //                                            Func<T,T,float,T> lerp,
  //                                            T targetValue,
  //                                            float duration,
  //                                            float? period = null) {
  //   T startValue = v.value;
  //   yield return v.LerpTo(lerp, targetValue, duration / 2f, period);
  //   yield return v.LerpTo(lerp, startValue, duration / 2f, period);

  // }

  // IEnumerator LerpToAndBack<T>(IValue<T> v, T b, float duration, float? interval) {

  /** Lerp from a to b and back again. */
  Func<T,T,float,T> LerpBack<T>(Func<T,T,float,T> lerp) {
    return (T a, T b, float t) => {
      if (t < 0.5f)
        return lerp(a, b, t * 2f);
      else
        return lerp(a, b, (1f - t) * 2f);
    };
  }

  void HandleToggle(Toggle toggle) {
    var item = toggle.GetComponent<Item>();
    if (item == null)
      return;
    if (toggle.isOn) {
      // Can use the attributes array.
      item.AddModifiers(attributes);
      // Or you can use this class directly.
      // item.AddModifiers(this);
    } else {
      // Can use the attributes array.
      item.RemoveModifiers(attributes);
      // Or you can use this class directly.
      // item.RemoveModifiers(this);
    }
  }

  void HandleButton(Button button) {
    var item = button.GetComponent<Item>();
    if (item == null)
      return;
    // Button hit.
    item.AddModifiers(this);
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
