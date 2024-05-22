using UnityEngine.SceneManagement;

namespace TW.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using UnityEngine;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.UIElements;

    [InitializeOnLoad]
    public static class SceneToolbarUtilities
    {
        private static ScriptableObject _toolbar;
        private static string[] _scenePaths;
        private static string[] _sceneNames;

        static SceneToolbarUtilities()
        {
            EditorApplication.delayCall += () =>
            {
                EditorApplication.update -= Update;
                EditorApplication.update += Update;
            };
        }

        private static void Update()
        {
            if (_toolbar == null)
            {
                var editorAssembly = typeof(Editor).Assembly;
                var toolbars = Resources.FindObjectsOfTypeAll(editorAssembly.GetType("UnityEditor.Toolbar"));
                _toolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                if (_toolbar != null)
                {
#if UNITY_2021_1_OR_NEWER
                    var root = _toolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                    var rawRoot = root.GetValue(_toolbar);
                    var mRoot = rawRoot as VisualElement;
                    RegisterCallback("ToolbarZoneRightAlign", OnGUI);
                    void RegisterCallback(string root, Action cb)
                    {
                        var toolbarZone = mRoot.Q(root);
                        if (toolbarZone != null)
                        {
                            var parent = new VisualElement
                            {
                                style =
                                {
                                    flexGrow = 1,
                                    flexDirection = FlexDirection.Row,
                                }
                            };
                            var container = new IMGUIContainer();
                            container.onGUIHandler += () => { cb?.Invoke(); };
                            parent.Add(container);
                            toolbarZone.Add(parent);
                        }
                    }
#else
#endif
                }
            }

            if (_scenePaths == null || _scenePaths.Length != EditorBuildSettings.scenes.Length)
            {
                List<string> scenePaths = new();
                List<string> sceneNames = new();

                foreach (var scene in EditorBuildSettings.scenes)
                {
                    if (scene.path == null || scene.path.StartsWith("Assets") == false)
                        continue;
                    var scenePath = Application.dataPath + scene.path.Substring(6);
                    scenePaths.Add(scenePath);
                    sceneNames.Add(Path.GetFileNameWithoutExtension(scenePath));
                }

                _scenePaths = scenePaths.ToArray();
                _sceneNames = sceneNames.ToArray();
            }
        }

        private static void OnGUI()
        {
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                {
                    var sceneName = SceneManager.GetActiveScene().name;
                    var sceneIndex = -1;

                    for (var i = 0; i < _sceneNames.Length; ++i)
                    {
                        if (sceneName == _sceneNames[i])
                        {
                            sceneIndex = i;
                            break;
                        }
                    }

                    var newSceneIndex = EditorGUILayout.Popup(sceneIndex, _sceneNames, GUILayout.Width(200.0f));
                    if (newSceneIndex != sceneIndex)
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(_scenePaths[newSceneIndex], OpenSceneMode.Single);
                        }
                    }
                }
            }
        }
    }
}