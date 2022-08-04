using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SeawispHunter.RolePlay.Attributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

  void Start() {
#if UNITY_EDITOR
    // This allows us to capture the cursor with Unity's recorder.
    Cursor.SetCursor(PlayerSettings.defaultCursor,
                     Vector2.zero, // hot spot of cursor
                     CursorMode.ForceSoftware);
#endif
  }

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
