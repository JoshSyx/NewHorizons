using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.IO;

namespace Tools
{
    public class Texture2DArrayCreator : OdinEditorWindow
    {
        [VerticalGroup("Textures")]
        [LabelText("Albedo"), PreviewField(32)]
        public Texture2D albedo;
        
        [VerticalGroup("Textures")]
        [LabelText("Normal"), PreviewField(32)]
        public Texture2D normal;
        
        [VerticalGroup("Textures")]
        [LabelText("Roughness"), PreviewField(32)]
        public Texture2D roughness;
        
        [VerticalGroup("Textures1")]
        [LabelText("AO"), PreviewField(32)]
        public Texture2D ao;
        
        [Space]
        [FolderPath(RequireExistingPath = true)]
        [LabelText("Save Folder")]
        public string saveFolder = "Assets/";
        
        [LabelText("Asset Name")]
        public string assetName = "Texture2DArray.asset";
        
        private Texture2D GetDefaultForChannel(string channel, int width, int height, TextureFormat format, bool mipmap)
        {
            Color color = Color.white;
            switch (channel)
            {
                case "Albedo": color = Color.white; break;
                case "Normal": color = new Color(0.5f, 0.5f, 1.0f, 1.0f); break; // flat normal
                case "Roughness": color = Color.gray; break; // typical roughness default
                case "AO": color = Color.white; break;
            }
            Texture2D tex = new Texture2D(width, height, format, mipmap);
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = color;
            tex.SetPixels(colors);
            tex.Apply();
            tex.name = $"Default_{channel}";
            return tex;
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0.2f, 1, 0.2f)]
        public void SaveArray()
        {
            Texture2D[] textures = { albedo, normal, roughness, ao };
            string[] channels = { "Albedo", "Normal", "Roughness", "AO" };

            // Find first non-null texture for reference settings
            Texture2D reference = System.Array.Find(textures, t => t != null);
            if (reference == null)
            {
                EditorUtility.DisplayDialog("Error", "At least one texture must be assigned.", "OK");
                return;
            }

            int width = reference.width;
            int height = reference.height;
            TextureFormat format = reference.format;
            bool mipmap = reference.mipmapCount > 1;

            // Replace null textures with defaults
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] == null)
                {
                    textures[i] = GetDefaultForChannel(channels[i], width, height, format, mipmap);
                }
            }

            foreach (var tex in textures)
            {
                if (tex.width != width || tex.height != height)
                {
                    EditorUtility.DisplayDialog("Error", "All textures must have the same size.", "OK");
                    return;
                }
                if (tex.format != format)
                {
                    EditorUtility.DisplayDialog("Error", "All textures must have the same format.", "OK");
                    return;
                }
                if ((tex.mipmapCount > 1) != mipmap)
                {
                    EditorUtility.DisplayDialog("Error", "All textures must have the same mipmap setting.", "OK");
                    return;
                }
                if (!tex.isReadable)
                {
                    EditorUtility.DisplayDialog("Error", $"Texture {tex.name} is not readable.", "OK");
                    return;
                }
            }

            // Create Texture2DArray
            var array = new Texture2DArray(width, height, textures.Length, format, mipmap, false);
            for (var i = 0; i < textures.Length; i++)
            {
                array.SetPixels(textures[i].GetPixels(), i);
            }
            array.Apply();

            var path = Path.Combine(saveFolder, assetName);
            AssetDatabase.CreateAsset(array, path);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Success", $"Texture2DArray saved at {path}", "OK");
        }

        [MenuItem("Tools/Texture2DArray Creator")]
        private static void OpenWindow()
        {
            GetWindow<Texture2DArrayCreator>("Texture2DArray Creator").Show();
        }
    }

}