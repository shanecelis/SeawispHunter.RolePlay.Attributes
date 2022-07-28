using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour {
  [SerializeField] Text textField;
  void Awake() {
    gameObject.SetActive(false);
  }

  public void Show(string message) {
    textField.text = message;
    gameObject.SetActive(true);
  }

  public void Hide() {
    gameObject.SetActive(false);
  }
}
