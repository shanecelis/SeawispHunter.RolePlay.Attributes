using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SeawispHunter.RolePlay.Attributes;

namespace SeawispHunter.RolePlay.Attributes.Samples {

public class Main : MonoBehaviour {
  [SerializeField] Tooltip helpTooltip;
  void Update() {
    if (Input.GetKeyDown(KeyCode.H)) {
      if (helpTooltip.isVisible)
        helpTooltip.Hide();
      else
        helpTooltip.Show();
    }

    if (Input.GetKeyDown(KeyCode.R)) {
      // Reset the scene.
      var scene = SceneManager.GetActiveScene();
      SceneManager.LoadScene(scene.name);
    }

  }

}
}
