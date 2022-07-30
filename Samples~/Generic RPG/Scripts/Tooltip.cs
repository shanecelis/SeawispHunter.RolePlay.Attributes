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

}
