/* Original code[1] Copyright (c) 2022 Seawisp Hunter, LLC[2]
   Licensed under the Unity Asset Store End User License[3]

   Author: Shane Celis[4]

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://seawisphunter.com
   [3]: https://unity3d.com/legal/as_terms
   [4]: https://twitter.com/shanecelis
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace SeawispHunter.RolePlay.Attributes.Samples {

public class TooltipOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

  [SerializeField] Tooltip tooltip;
  [FormerlySerializedAs("message")]
  [SerializeField] string _message;

  public string message => messageDelegate == null ? _message : messageDelegate();

  public Func<string> messageDelegate;
  private bool entered = false;

  void Start() { }

  public void OnPointerEnter(PointerEventData eventData) {
    if (! string.IsNullOrEmpty(message)) {
      tooltip.Show(message);
      entered = true;
    }
  }

  public void OnPointerExit(PointerEventData eventData) {
    tooltip.Hide();
    entered = false;
  }

  void OnDisable() {
    if (entered)
      tooltip.Hide();
  }
}

}
