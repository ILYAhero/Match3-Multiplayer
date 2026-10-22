using System;
using UnityEditor;
using UnityEngine;

namespace CozyWorld.Editor
{
    /// <summary>Read-only art browser. Does not create or modify scene objects.</summary>
    public sealed class CozyWorldGallery : EditorWindow
    {
        private const string ManifestPath = "Assets/CozyWorld/Data/asset_manifest.json";
        [Serializable] private sealed class Manifest { public Entry[] assets; }
        [Serializable] private sealed class Entry
        {
            public string id;
            public string label_ru;
            public string category;
            public string path;
        }

        private Manifest manifest;
        private Vector2 scroll;
        private string query = string.Empty;
        private string loadError;

        [MenuItem("Tools/Cozy World/Asset Gallery")]
        private static void Open()
        {
            GetWindow<CozyWorldGallery>("Cozy World");
        }

        private void OnEnable() { Reload(); }

        private void Reload()
        {
            manifest = null;
            loadError = null;
            var file = AssetDatabase.LoadAssetAtPath<TextAsset>(ManifestPath);
            if (file == null)
            {
                loadError = "Manifest not found at " + ManifestPath;
                return;
            }
            try
            {
                manifest = JsonUtility.FromJson<Manifest>(file.text);
                if (manifest == null || manifest.assets == null)
                    loadError = "Manifest contains no assets.";
            }
            catch (Exception exception)
            {
                loadError = "Cannot read manifest: " + exception.Message;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Cozy World v0.1: 44 concept-derived sprites. " +
                "Not a complete production UI. Inspect edges, native resolution and scale before shipping.",
                MessageType.Info);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            query = GUILayout.TextField(query, EditorStyles.toolbarTextField);
            if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60))) Reload();
            EditorGUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(loadError))
            {
                EditorGUILayout.HelpBox(loadError, MessageType.Warning);
                return;
            }
            if (manifest == null || manifest.assets == null) return;

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (Entry entry in manifest.assets)
            {
                if (entry == null || string.IsNullOrEmpty(entry.path)) continue;
                string searchable = (entry.id ?? "") + " " + (entry.label_ru ?? "") + " " + (entry.category ?? "");
                if (!string.IsNullOrEmpty(query) && searchable.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(entry.path);
                Rect preview = GUILayoutUtility.GetRect(80, 80, GUILayout.Width(80), GUILayout.Height(80));
                if (texture != null) GUI.DrawTexture(preview, texture, ScaleMode.ScaleToFit, true);
                EditorGUILayout.BeginVertical();
                GUILayout.Label(entry.label_ru ?? entry.id, EditorStyles.boldLabel);
                GUILayout.Label(entry.id ?? string.Empty, EditorStyles.miniLabel);
                GUILayout.Label(entry.category ?? string.Empty, EditorStyles.miniLabel);
                if (GUILayout.Button("Select asset", GUILayout.Width(110)))
                {
                    UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<Sprite>(entry.path);
                    if (asset == null) asset = texture;
                    Selection.activeObject = asset;
                    if (asset != null) EditorGUIUtility.PingObject(asset);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
