using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CozyWorld.Editor
{
    /// <summary>
    /// Applies initial import settings only to this art pack. Never changes scenes,
    /// game scripts, packages, or textures outside Assets/CozyWorld/Art/Sprites/.
    /// Individual manual texture changes survive subsequent ordinary imports.
    /// </summary>
    public sealed class CozyWorldArtImporter : AssetPostprocessor
    {
        internal const string SpriteRoot = "Assets/CozyWorld/Art/Sprites";
        private const string ImportStamp = "CozyWorld.Sprite.Import.v1";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(SpriteRoot + "/", StringComparison.Ordinal) ||
                !assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (TextureImporter)assetImporter;
            if (string.Equals(importer.userData, ImportStamp, StringComparison.Ordinal))
                return;

            ApplySettings(importer);
        }

        internal static void ApplySettings(TextureImporter importer)
        {
            if (importer == null) throw new ArgumentNullException(nameof(importer));
            if (!importer.assetPath.StartsWith(SpriteRoot + "/", StringComparison.Ordinal))
                throw new ArgumentException("Texture is outside the Cozy World pack.", nameof(importer));

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 1;
            importer.maxTextureSize = 2048;
            // Small reference-derived sprites: preserve edge detail for art review.
            // Shipping platform compression and atlases should be profiled separately.
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            settings.spriteExtrude = 1;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);

            string name = Path.GetFileNameWithoutExtension(importer.assetPath);
            if (name == "ui_button_blue" || name == "ui_button_orange")
                importer.spriteBorder = new Vector4(37, 22, 37, 22);
            else if (name == "ui_panel_cream")
                importer.spriteBorder = new Vector4(36, 32, 36, 32);
            else if (name == "ui_wood_banner")
                importer.spriteBorder = new Vector4(38, 26, 42, 28);
            else
                importer.spriteBorder = Vector4.zero;

            importer.userData = ImportStamp;
        }

        [MenuItem("Tools/Cozy World/Reapply Pack Import Settings")]
        private static void ReapplyAll()
        {
            if (!AssetDatabase.IsValidFolder(SpriteRoot))
            {
                Debug.LogWarning("Cozy World sprite folder was not found.");
                return;
            }
            if (!EditorUtility.DisplayDialog("Cozy World", 
                "Reset import settings for this pack? Custom settings on these textures will be replaced. " +
                "Other assets will not be changed.", "Apply", "Cancel"))
                return;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpriteRoot });
            int applied = 0;
            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                        continue;
                    EditorUtility.DisplayProgressBar("Cozy World", path,
                        guids.Length == 0 ? 1f : (float)i / guids.Length);
                    ApplySettings(importer);
                    importer.SaveAndReimport();
                    applied++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            Debug.Log("Cozy World: import settings applied to " + applied + " sprites.");
        }

        [MenuItem("Tools/Cozy World/Validate Imported Sprites")]
        private static void ValidateAll()
        {
            if (!AssetDatabase.IsValidFolder(SpriteRoot))
            {
                Debug.LogError("Cozy World sprite folder was not found.");
                return;
            }
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpriteRoot });
            int errors = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (importer == null || importer.textureType != TextureImporterType.Sprite ||
                    importer.spriteImportMode != SpriteImportMode.Single || sprite == null)
                {
                    errors++;
                    Debug.LogError("Invalid Cozy World sprite import: " + path);
                }
            }
            if (guids.Length != 44)
            {
                errors++;
                Debug.LogWarning("Cozy World v0.1 expects 44 textures; found " + guids.Length + ".");
            }
            Debug.Log("Cozy World validation: " + guids.Length + " textures, " + errors + " issues. " +
                "This checks imports, not gameplay, mobile rendering, or final art quality.");
        }
    }
}
