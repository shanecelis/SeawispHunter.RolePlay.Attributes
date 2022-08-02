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
  [SerializeField] bool showBlankMessage = true;

  [Serializable]
  public class Criteria {
    public bool all;
    public KeyCode[] keys;

    public bool IsMet() { 
      if (all) {
        foreach (var key in keys)
          if (! Input.GetKey(key))
            return false;
        return true;
      } else {
        // any
        bool hasOne = false;
        foreach (var key in keys)
          hasOne |= Input.GetKey(key);
        return hasOne;
      }
    }
  }

  protected virtual bool showMessage
    => (showBlankMessage || ! string.IsNullOrEmpty(message))
       && AreValidKeys();

  [SerializeField] private Criteria[] mustHave;
  [SerializeField] private Criteria[] mustNotHave;

  public string message => messageDelegate == null ? _message : messageDelegate();

  public Func<string> messageDelegate;
  private bool entered = false;

  void Awake() {
    if (tooltip == null) {
      Debug.LogWarning("No tooltip selected; disabling.", this.gameObject);
      this.enabled = false;
    }
  }

  void Start() { }

  bool AreValidKeys(){
    bool valid = true;
    foreach (var entry in mustHave)
      valid &= entry.IsMet();
    foreach (var entry in mustNotHave)
      valid &= ! entry.IsMet();
    return valid;
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (showMessage) {
      tooltip.Show(message);
      entered = true;
    }
  }

  public void OnPointerExit(PointerEventData eventData) {
    // If we have a child object drawn on top of us, this method will be called
    // when the child is entered. But we only want to deal with someone exiting
    // us, so we use `eventData.fullyExited` to discriminate.
    if (! eventData.fullyExited)
      return;

    tooltip.Hide();
    entered = false;
  }

  void OnDisable() {
    if (entered)
      tooltip.Hide();
  }
}

}
