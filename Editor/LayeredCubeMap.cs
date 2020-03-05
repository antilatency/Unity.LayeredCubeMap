using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;

namespace Antilatency {
    public class LayeredCubeMap : ScriptableWizard {

        public InspectorButton SelectDirectory_;
        public void SelectDirectory() {
            string directoryCandidate = EditorUtility.OpenFolderPanel("Select folder for new scene.", Application.dataPath, "");
            if (directoryCandidate.StartsWith(Application.dataPath)) {
                directory = directoryCandidate.Substring(Application.dataPath.Length).TrimStart(new char[] { '/', '\\' });
                var sceneNameCandidate = Path.GetFileName(directory);
                if (!string.IsNullOrEmpty(sceneNameCandidate)) {
                    sceneName = sceneNameCandidate;
                }
            }
            OnWizardUpdate();
        }

        public string directory = "";
        public string sceneName = "Scene";
        public Cubemap cubemap;


        public string scenePath { 
            get{
                return Path.Combine(Application.dataPath, directory, sceneName + ".unity");
            }
        }

        public void Begin() {

        }


        [MenuItem("Antilatency/LayeredCubeMap/New")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<LayeredCubeMap>("New LayeredCubeMap scene", "Create");
            //If you don't want to use the secondary button simply leave it out:
            //ScriptableWizard.DisplayWizard<WizardCreateLight>("Create Light", "Create");
        }

        

        void OnWizardCreate() {
            var shader = Shader.Find("Antilatency/Skybox/CubemapProjection");
            var material = new Material(shader);
            material.mainTexture = cubemap;
            AssetDatabase.CreateAsset(material, Path.Combine("Assets", directory, "Sky.mat"));

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = sceneName;
            RenderSettings.skybox = material;

            


            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.Refresh(); 
            

        }

        bool checkIfValid() {
            if (string.IsNullOrEmpty(sceneName)) {
                helpString = " set Scene Name";
                return false;
            }


            if (File.Exists(scenePath)) {
                helpString = scenePath + " already exists";
                return false;
            }
            /*if (!cubemap) {
                helpString = "select cubemap";
                return false;
            }*/


            helpString = "";
            return true;
        }

        void OnWizardUpdate() {
            

            isValid = checkIfValid();
        }





        // When the user presses the "Apply" button OnWizardOtherButton is called.
        /*void OnWizardOtherButton() {
            if (Selection.activeTransform != null) {
                Light lt = Selection.activeTransform.GetComponent<Light>();

                if (lt != null) {
                    lt.color = Color.red;
                }
            }
        }*/
    }
}