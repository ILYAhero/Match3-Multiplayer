using System;
using MonoOrbit.Presentation;
using UnityEditor;
using UnityEngine;

namespace MonoOrbit.Presentation.Editor
{
    /// <summary>Explicit opt-in import step. Does not modify gameplay, scenes or existing assets.</summary>
    public static class CozyWorldThemeBuilder
    {
        private const string Sprites = "Assets/CozyWorld/Art/Sprites/";
        private const string ResourcesFolder = "Assets/MonoOrbit/Presentation/Resources";
        private const string AssetPath = ResourcesFolder + "/CozyWorldTheme.asset";

        private static Sprite FindSprite(string name)
        {
            foreach (string guid in AssetDatabase.FindAssets(name + " t:Sprite", new[] { "Assets/CozyWorld/Art/Sprites" }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(System.IO.Path.GetFileNameWithoutExtension(assetPath), name,
                    StringComparison.Ordinal))
                    return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
            return null;
        }

        [MenuItem("Tools/Mono Orbit/Cozy World/Build Theme from Imported Sprites")]
        public static void Build()
        {
            // Mapping is explicit: visual identities never change stable WeatherElement IDs.
            // Sun=star, Rain=drop, Snow=flower, Wind=leaf, Thunder=orange sphere, Cloud=heart.
            string[] paths =
            {
                "Tiles/tile_star.png", "Tiles/tile_drop.png", "Tiles/tile_flower.png",
                "Tiles/tile_leaf.png", "Alternates/tile_orange_orb.png", "Tiles/tile_heart.png"
            };
            Sprite[] sprites = new Sprite[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                sprites[i] = FindSprite(System.IO.Path.GetFileNameWithoutExtension(paths[i]));
                if (sprites[i] == null)
                {
                    Debug.LogError("Cozy World is not installed or not imported as Sprites: " +
                        Sprites + paths[i] + ". Import the supplied CozyWorld_Assets_v0.1.unitypackage first.");
                    return;
                }
            }

            Sprite cell = FindSprite("cell_base");
            Sprite panel = FindSprite("ui_panel_cream");
            Sprite banner = FindSprite("ui_wood_banner");
            if (cell == null || panel == null || banner == null)
            {
                Debug.LogError("Cozy World: expected board/UI sprites not found. Check the asset paths in this script.");
                return;
            }
            if (!AssetDatabase.IsValidFolder(ResourcesFolder))
                AssetDatabase.CreateFolder("Assets/MonoOrbit/Presentation", "Resources");

            CozyWorldTheme existing = AssetDatabase.LoadAssetAtPath<CozyWorldTheme>(AssetPath);
            if (existing != null && !EditorUtility.DisplayDialog("Cozy World",
                    "A theme already exists. Update sprite references and silhouettes? Existing scenes will not change.",
                    "Update theme", "Cancel"))
                return;

            var theme = existing != null ? existing : ScriptableObject.CreateInstance<CozyWorldTheme>();
            // Compensates the visible alpha bounds in the concept-derived PNGs.
            float[] scales = { 1.0f, 1.0f, 1.0f, 1.0f, 1.36f, 1.0f };
            theme.Assign(sprites, scales, cell, panel, banner);
            if (existing == null) AssetDatabase.CreateAsset(theme, AssetPath);
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            Debug.Log("Cozy World theme generated at " + AssetPath +
                ". Open the existing Mono Orbit Match scene and press Play to inspect.");
            Selection.activeObject = theme;
        }
    }
}
