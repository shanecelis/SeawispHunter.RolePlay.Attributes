using UnityEditor;
using UnityEngine;
// https://docs.unity3d.com/ScriptReference/PropertyDrawer.html
namespace SeawispHunter.RolePlay.Attributes {

/** Only draw the `initial.value` element from the ModifiableValue. */
[CustomPropertyDrawer(typeof(ModifiableValue<>), true)]
public class ModifiableValueDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    // EditorGUI.BeginProperty(position, label, property);
    var initial = property.FindPropertyRelative("_initial");
    if (initial == null) {
      EditorGUI.HelpBox(position, $"Unable to find _initial in {label.text}.", MessageType.Warning);
      return;
    }
    var value = initial.FindPropertyRelative("_value");
    if (value == null) {
      EditorGUI.HelpBox(position, $"Unable to find _value in initial of {label.text}.", MessageType.Warning);
    }
    EditorGUI.PropertyField(position, value, label, true);
  }
}

/** Only draw the `value` element from Value and ReadOnlyValue. */
[CustomPropertyDrawer(typeof(Value<>), true)]
[CustomPropertyDrawer(typeof(ReadOnlyValue<>), true)]
public class ValueDrawer : PropertyDrawer {
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
    var value = property.FindPropertyRelative("_value");
    EditorGUI.PropertyField(position, value, label, true);
  }
}
}
