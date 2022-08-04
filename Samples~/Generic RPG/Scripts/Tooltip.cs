/* Original code[1] Copyright (c) 2022 Seawisp Hunter, LLC[2]
   Licensed under the Unity Asset Store End User License[3]

   Author: Shane Celis[4]

   [1]: https://github.com/shanecelis/SeawispHunter.RolePlay.Attributes
   [2]: https://seawisphunter.com
   [3]: https://unity3d.com/legal/as_terms
   [4]: https://twitter.com/shanecelis
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SeawispHunter.RolePlay.Attributes.Samples {

public class Tooltip : MonoBehaviour {
  [SerializeField] Text textField;
  [SerializeField] bool autoHide = true;
  void Awake() {
    if (autoHide)
      gameObject.SetActive(false);
  }

  public bool isVisible => gameObject.activeSelf;

  public void Show() {
    gameObject.SetActive(true);
  }

  public void Show(string message) {
    if (textField != null)
      textField.text = message;
    else
      Debug.LogWarning("Trying to set message on tooltip without textField.", this.gameObject);

    gameObject.SetActive(true);
  }

  public void Hide() {
    gameObject.SetActive(false);
  }
}

}
