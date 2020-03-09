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

        public static UnityPath GetCurrentScenePath() {
            return new UnityPath(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
        }

        public static UnityPath GetCurrentSceneDirectory() {
            return new UnityPath(Path.GetDirectoryName(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path));
        }
        
    }
}