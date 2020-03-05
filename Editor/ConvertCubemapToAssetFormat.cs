using UnityEditor;
using UnityEngine;
using System.IO;


public class ConvertCubemapToAssetFormat{
    [MenuItem("Assets/Antilatency/ConvertCubemapToAssetFormat")]
    private static void ConvertSelection() {
        foreach (var o in Selection.objects) {
            if (o is Cubemap) {
                ConvertCubemap(o as Cubemap);
            }
        }
    }

    private static void ConvertCubemap(Cubemap cubemap) {
        if (!ObjectIsValid(cubemap)) return;

        var path = AssetDatabase.GetAssetPath(cubemap);

        Debug.Log(path);
        var outputPath = Path.ChangeExtension(path, "asset");
        //Debug.Log(cubemap.isReadable);
        var copy = Object.Instantiate(cubemap);
        AssetDatabase.CreateAsset(copy, outputPath);
    }

    private static bool ObjectIsValid(Object o) {
        if (!(o is Cubemap)) return false;
        var extension = Path.GetExtension(AssetDatabase.GetAssetPath(o));
        if (extension == ".asset") return false;

        return true;
    }

    [MenuItem("Assets/Antilatency/ConvertCubemapToAssetFormat", true)]
    private static bool ConvertIsValid() {
        if (Selection.objects.Length == 0) return false;
        foreach (var o in Selection.objects) {
            if (ObjectIsValid(o)) {
                return true;
            }
        }
        return false;
    }

}
