using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;

namespace Antilatency.LayeredCubeMap {
    public class WizardNew : ScriptableWizard {

        public InspectorButton SelectDirectory_;
        /*public void SelectDirectory() {
            string directoryCandidate = EditorUtility.OpenFolderPanel("Select folder for new scene.", Application.dataPath, "");
            if (directoryCandidate.StartsWith(Application.dataPath)) {
                directory = directoryCandidate.Substring(Application.dataPath.Length).TrimStart(new char[] { '/', '\\' });
                var sceneNameCandidate = Path.GetFileName(directory);
                if (!string.IsNullOrEmpty(sceneNameCandidate)) {
                    sceneName = sceneNameCandidate;
                }
            }
            OnWizardUpdate();
        }*/

        public string sceneName = "";
        public Cubemap cubemap;


        public Utils.UnityPath scenePath { 
            get{
                return new Utils.UnityPath(Path.Combine("Assets",sceneName, sceneName + ".unity"));
            }
        }
        public Utils.UnityPath sceneDirectory {
            get {
                return new Utils.UnityPath(Path.Combine("Assets", sceneName));
            }
        }

        public void Begin() {

        }


        [MenuItem("Antilatency/LayeredCubeMap/New")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<WizardNew>("New LayeredCubeMap scene", "Create");
            //If you don't want to use the secondary button simply leave it out:
            //ScriptableWizard.DisplayWizard<WizardCreateLight>("Create Light", "Create");
        }

        

        void OnWizardCreate() {

            PlayerSettings.colorSpace = ColorSpace.Linear;
            //var sceneDirectory = Utils.GetCurrentSceneDirectory().ToAbsolute();
            if (!Directory.Exists(sceneDirectory.ToAbsolute())) {
                Directory.CreateDirectory(sceneDirectory.ToAbsolute());
            }

            var shader = Shader.Find("Antilatency/Skybox/CubemapProjection");
            var material = new Material(shader);
            material.mainTexture = cubemap;
            AssetDatabase.CreateAsset(material, Path.Combine("Assets", sceneName, $"{sceneName}Sky.mat"));

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = sceneName;
            RenderSettings.skybox = material;

            
            Directory.CreateDirectory(Path.Combine(sceneDirectory.ToAbsolute(), Utils.patchesDirectoryName));

            GameObject layeredCubeMapControl = new GameObject("LayeredCubeMapControl");
            layeredCubeMapControl.AddComponent<SkyControl>();

            EditorSceneManager.SaveScene(newScene, scenePath.ToString());



            AssetDatabase.Refresh(); 
            

        }

        bool checkIfValid() {
            if (string.IsNullOrEmpty(sceneName)) {
                helpString = " set Name";
                return false;
            }

            if (File.Exists(scenePath.ToAbsolute())) {
                helpString = scenePath + " already exists";
                return false;
            }
            if (!cubemap) {
                helpString = "select cubemap";
                return false;
            }


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