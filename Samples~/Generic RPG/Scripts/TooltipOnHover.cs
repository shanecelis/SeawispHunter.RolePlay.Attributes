using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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
