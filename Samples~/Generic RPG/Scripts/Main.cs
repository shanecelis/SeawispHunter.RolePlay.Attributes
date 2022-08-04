using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SeawispHunter.RolePlay.Attributes;

namespace SeawispHunter.RolePlay.Attributes.Samples {

public class Main : MonoBehaviour {
  [SerializeField] Tooltip helpTooltip;

  [System.Serializable]
  public class Highlights {
    public KeyCode[] keys;
    public Tooltip[] highlights;

    public bool IsKeyDown() {
      foreach (var key in keys)
        if (Input.GetKeyDown(key))
          return true;
      return false;
    }

    public bool IsKeyUp() {
      foreach (var key in keys)
        if (Input.GetKeyUp(key))
          return true;
      return false;
    }

    public bool visible {
      set {
        foreach (var highlight in highlights)
          if (value)
            highlight.Show();
          else
            highlight.Hide();
      }
    }
  }
  [SerializeField] Highlights codeHighlights;
  [SerializeField] Highlights inspectorHighlights;

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

    UpdateHighlights(codeHighlights);
    UpdateHighlights(inspectorHighlights);
  }

  private void UpdateHighlights(Highlights highlights) {
    if (highlights.IsKeyDown())
      highlights.visible = true;
    if (highlights.IsKeyUp())
      highlights.visible = false;
  }

}
}
