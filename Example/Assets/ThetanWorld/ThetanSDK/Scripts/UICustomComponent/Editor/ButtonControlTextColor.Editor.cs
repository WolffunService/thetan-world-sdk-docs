using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ThetanSDK.UI.CustomComponent.Editor
{
    [CustomEditor(typeof(ButtonControlTextColor))]
    public class ButtonControlTextColorEditor : UnityEditor.UI.ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            ButtonControlTextColor button = (ButtonControlTextColor)target;
            base.OnInspectorGUI();
        
            EditorGUILayout.Space();

            button.TxtButton =
                (TextMeshProUGUI)EditorGUILayout.ObjectField("Text Button", button.TxtButton, typeof(TextMeshProUGUI),
                    true);
            button.NormalColor = EditorGUILayout.ColorField("Normal Color", button.NormalColor);
            button.DisableColor = EditorGUILayout.ColorField("Disable Color", button.DisableColor);
        }
    }
}


