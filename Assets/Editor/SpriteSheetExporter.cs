using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SpriteSheetExporter : EditorWindow
{
    [MenuItem("Tools/Sprite Sheet -> Export Sprites as PNGs")]
    public static void ExportSprites()
    {
        Object selected = Selection.activeObject;
        if (selected == null || !(selected is Texture2D))
        {
            Debug.LogError("Please select a sliced sprite sheet (Texture2D) in the Project window.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(selected);
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path)
                                        .OfType<Sprite>()
                                        .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError("No sprites found in the selected asset. Make sure it's sliced.");
            return;
        }

        string outputFolder = Application.dataPath + "/Exported";
        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        foreach (var sprite in sprites)
        {
            Texture2D tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.rect.x,
                (int)sprite.rect.y,
                (int)sprite.rect.width,
                (int)sprite.rect.height
            );
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            string fileName = sprite.name.Replace("/", "_"); // prevent folder creation in file name
            File.WriteAllBytes($"{outputFolder}/{fileName}.png", png);
        }

        AssetDatabase.Refresh();
        Debug.Log($"Exported {sprites.Length} sprites to {outputFolder}");
    }
}
