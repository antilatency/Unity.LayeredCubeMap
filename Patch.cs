using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Experimental.Rendering;
using UnityEditor.Rendering;
using System.Linq;

namespace Antilatency.LayeredCubeMap {
    [ExecuteAlways]
    //[RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(RectTransform))]
    public class Patch : MonoBehaviour {
        public static string PatchesDirectoryName => "Patches";
        public static string PatchName => "Patch";

        static int cubemapSize {
            get {
                return (RenderSettings.skybox.mainTexture as Cubemap).height;
            }
        }

        public InspectorButton Test_;
        public void Test() {
            
            Debug.Log(resourceDirectory);
        }


        public static string GetUniqueName() {
            var path = Path.Combine(Utils.GetCurrentSceneDirectory().ToAbsolute(), PatchesDirectoryName);
            var directories = Directory.GetDirectories(path).Select(x => Path.GetFileName(x)).ToList();
            int index = 0;
            while (directories.Contains(PatchName + index.ToString("D4"))) {
                index++;
            }
            return PatchName + index.ToString("D4");
        }

        public Utils.UnityPath resourceDirectory => new Utils.UnityPath(Path.Combine(Utils.GetCurrentSceneDirectory().ToString(), PatchesDirectoryName, name));

        [MenuItem("Antilatency/LayeredCubeMap/Create patch &p")]
        public static void Create() {
            var name = GetUniqueName();
            var patch = new GameObject(name, typeof(RectTransform), typeof(ShowRectTransform),typeof(Patch));
            var camera = new GameObject("Camera", typeof(Camera));
            camera.transform.parent = patch.transform;
            var viewRectTransform = patch.GetComponent<RectTransform>();
            viewRectTransform.sizeDelta = new Vector2(1, 1); // = new Rect(-0.5f, -0.5f, 1, 1);
            patch.GetComponent<Patch>().AlignToView();
        }


        
        public InspectorButton AlignToView_;
        public void AlignToView() {
            transform.rotation = SceneView.lastActiveSceneView.camera.transform.rotation;
            fixPosition();

        }

        public static int directionToSide(Vector3 direction) {
            var abs = new Vector3(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
            int maxAxisID = 0;
            for (int i = 1; i < 3; i++) {
                if (abs[i] > abs[maxAxisID]) {
                    maxAxisID = i;
                }
            }
            return 2 * maxAxisID + ((direction[maxAxisID] > 0) ? 0 : 1);
        }

        Quaternion sideRotation(int i) {

            float sq = 0.7071067811865475f;
            var c = new Quaternion[] {
                new Quaternion(0,sq,0,sq),
                new Quaternion(0,-sq,0,sq),
                new Quaternion(0.5f,0.5f,0.5f,-0.5f),
                new Quaternion(0.5f,0.5f,-0.5f,0.5f),
                new Quaternion(0,0,0,1),
                new Quaternion(0,1,0,0)
            };
            return c[i];
        }


        public InspectorButton AlignToNearestAxis_;
        public void AlignToNearestAxis() {
            var direction = transform.forward;
            int side = directionToSide(direction);
            transform.rotation = sideRotation(side);

            fixPosition();
            var p = transform.position;
            for (int i = 1; i < 3; i++) {
                p[(side + i) % 3] = Mathf.Round(p[(side + i) % 3] * cubemapSize) / cubemapSize;
            }
            transform.position = p;

            
        }

        void fixPosition() {
            var forward = transform.forward;
            transform.position = transform.position - forward * (Vector3.Dot(transform.position, forward)) + forward;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;

            int grid = cubemapSize / 4;
            var pixelSixe = rectTransform.sizeDelta * grid;
            rectTransform.sizeDelta = new Vector2(Mathf.Round(pixelSixe.x) / grid, Mathf.Round(pixelSixe.y) / grid);

        }

        

        public InspectorButton Render_;
        public void Render() {

            if (!Directory.Exists(resourceDirectory.ToAbsolute())) {
                Debug.LogError(resourceDirectory.ToString() + " Directory does not exist.");
                return;
            }

            var rectTransform = GetComponent<RectTransform>();
            Vector2 size = rectTransform.sizeDelta * cubemapSize;
            var renderTexture = new RenderTexture(
                Mathf.RoundToInt(size.x),
                Mathf.RoundToInt(size.y), 24, GraphicsFormat.R32G32B32A32_SFloat,0);
            var camera = GetComponentInChildren<Camera>();
            camera.targetTexture = renderTexture;
            camera.allowHDR = true;
            //TierSettings.hdr = true;
            camera.Render();
            camera.targetTexture = null;
            RenderTexture.active = renderTexture;
            Texture2D texture =  new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBAFloat, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            RenderTexture.active = null;

            PhotoshopFile.PsdFile output = new PhotoshopFile.PsdFile(PhotoshopFile.PsdFileVersion.Psd);
            output.RowCount = renderTexture.height;
            output.ColumnCount = renderTexture.width;
            output.BitDepth = 32;
            output.Layers.Add(new PhotoshopFile.Layer(output));
            output.PrepareSave();
            output.Save(Path.Combine(resourceDirectory.ToAbsolute(), $"{name}.psd"),System.Text.Encoding.UTF8);

            /*var bytes = texture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
            var outputPath = Path.Combine(Application.dataPath,"patch.exr");
            File.WriteAllBytes(outputPath, bytes);*/

            AssetDatabase.Refresh();
            //Debug.Log(rectTransform.sizeDelta * cubemapSize);
        }

        public InspectorButton Edit_;
        public void Edit() {
            
        }



        void Update() {
            fixPosition();

            var rectTransform = GetComponent<RectTransform>();

            var camera = GetComponentInChildren<Camera>();
            camera.transform.position = Vector3.zero;
            camera.transform.localRotation = Quaternion.identity;

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            var view = camera.transform.worldToLocalMatrix;            

            var min = view*corners[0];
            var max = view*corners[2];

            var c = transform.InverseTransformPoint(Vector3.zero);
            camera.projectionMatrix =
                PerspectiveOffCenter(
                min.x, max.x,
                min.y, max.y,
                camera.nearClipPlane, camera.farClipPlane);
            
        }


        Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far) {
            float x = 2.0F / (right - left);
            float y = 2.0F / (top - bottom);
            float a = (right + left) / (right - left);
            float b = (top + bottom) / (top - bottom);
            float c = -(far + near) / (far - near);
            float d = -(2.0F * far * near) / (far - near);
            float e = -1.0F ;
            Matrix4x4 m = Matrix4x4.zero;
            m[0, 0] = x;
            m[0, 2] = a;
            m[1, 1] = y;
            m[1, 2] = b;
            m[2, 2] = c;
            m[2, 3] = d;
            m[3, 2] = e;
            return m;
        }

        public static Camera selectedCamera;
        public void OnDrawGizmosSelected() {
            
            var newSelectedCamera = GetComponentInChildren<Camera>();
            if (selectedCamera != newSelectedCamera) {
                if (selectedCamera) selectedCamera.enabled = false;
                selectedCamera = newSelectedCamera;
                if (selectedCamera) selectedCamera.enabled = true;
            }

            
        }


    }
}
