using System.IO;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace Antilatency.LayeredCubeMap {
    public class Utils {

        public struct UnityPath {
            public string value;
            public UnityPath(string unityPath) {
                value = unityPath;
            }
            public static string projectPath{
                get {
                    return Path.GetDirectoryName(Application.dataPath);
                }
            }

            public string ToAbsolute() {
                return Path.Combine(projectPath, value);
            }
            /*public string ToAsset() {
                return Path.Combine("Assets", value);
            }*/
            public override string ToString() {
                return value;
            }

        }

        public static string patchesDirectoryName => "Patches";
        public static string patchName => "Patch";
        public static string layeredCubeMapName => "LayeredCubeMap";

        public static Utils.UnityPath patchesDirectory => new Utils.UnityPath(Path.Combine(Utils.currentSceneDirectory.ToAbsolute(), patchesDirectoryName));
        
        public static Utils.UnityPath designFilesDirectory => new Utils.UnityPath(Path.Combine( "LayeredCubeMap", currentSceneName));

        public static string currentSceneName =>
            Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
        

        public static UnityPath currentScenePath =>
            new UnityPath(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
        

        public static UnityPath currentSceneDirectory =>
            new UnityPath(Path.GetDirectoryName(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path));
        
        
    }
}