using UnityEngine;
using UnityEditor;
using System.IO;

public class MetallicRoughnessImporter : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        if (!assetPath.Contains("metallicRoughness") ||
            assetPath.Contains("_Unity"))
            return;

        Color[] pixels = texture.GetPixels();

        Texture2D newTex =
            new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

        for (int i = 0; i < pixels.Length; i++)
        {
            Color p = pixels[i];

            float metallic = p.g;
            float smoothness = 1f - p.b;

            pixels[i] = new Color(
                metallic,
                metallic,
                metallic,
                smoothness
            );
        }

        newTex.SetPixels(pixels);
        newTex.Apply();

        byte[] png = newTex.EncodeToPNG();

        string newPath =
            Path.ChangeExtension(assetPath, null) + "_Unity.png";

        EditorApplication.delayCall += () =>
        {
            File.WriteAllBytes(newPath, png);
            AssetDatabase.ImportAsset(newPath);
        };
    }
}