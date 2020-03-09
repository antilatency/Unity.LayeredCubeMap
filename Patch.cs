using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Experimental.Rendering;
using UnityEditor.Rendering;
using System.Linq;
using System.Collections.Generic;
using PhotoshopFile;


namespace Antilatency.LayeredCubeMap {
    [ExecuteAlways]
    //[RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(RectTransform))]
    public class Patch : MonoBehaviour {
        

        static int cubemapSize {
            get {
                return (RenderSettings.skybox.mainTexture as Cubemap).height;
            }
        }

        public InspectorButton Test_;
        public void Test() {
            
            Debug.Log(resourceDirectory);
        }

        
        public Utils.UnityPath directory => new Utils.UnityPath(Path.Combine(Utils.patchesDirectory.ToString(), name));
        
        
        public Utils.UnityPath designFilesDirectory => new Utils.UnityPath(Path.Combine(Utils.designFilesDirectory.ToString(), name));
        public Utils.UnityPath exrFilePath => new Utils.UnityPath(Path.Combine(designFilesDirectory.ToString(), $"{name}.exr"));
        public Utils.UnityPath psdFilePath => new Utils.UnityPath(Path.Combine(designFilesDirectory.ToString(), $"{name}.psd"));


        public static string GetUniqueName() {
            var directories = Directory.GetDirectories(Utils.patchesDirectory.ToAbsolute()).Select(x => Path.GetFileName(x)).ToList();
            int index = 0;
            while (directories.Contains(Utils.patchName + index.ToString("D4"))) {
                index++;
            }
            return Utils.patchName + index.ToString("D4");
        }

        public Utils.UnityPath resourceDirectory => new Utils.UnityPath(Path.Combine(Utils.currentSceneDirectory.ToString(), Utils.patchesDirectoryName, name));

        [MenuItem("Antilatency/LayeredCubeMap/Create patch &p")]
        public static void Create() {
            var name = GetUniqueName();
            //Directory.CreateDirectory()


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
            if (!Directory.Exists(Utils.designFilesDirectory.ToAbsolute())){
                Directory.CreateDirectory(Utils.designFilesDirectory.ToAbsolute());
            }
            if (!Directory.Exists(designFilesDirectory.ToAbsolute())) {
                Directory.CreateDirectory(designFilesDirectory.ToAbsolute());
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

            /*PhotoshopFile.PsdFile output = new PhotoshopFile.PsdFile(PhotoshopFile.PsdFileVersion.Psd);
            output.RowCount = renderTexture.height;
            output.ColumnCount = renderTexture.width;
            output.ChannelCount = 4;
            output.ColorMode = PhotoshopFile.PsdColorMode.RGB;
            output.BitDepth = 32;
            output.ImageCompression = PhotoshopFile.ImageCompression.Raw;


            output.Resolution = new PhotoshopFile.ResolutionInfo();
            var layer = new PhotoshopFile.Layer(output);
            layer.CreateMissingChannels();
            output.Layers.Add(layer);

            output.PrepareSave();
            output.Save(Path.Combine(resourceDirectory.ToAbsolute(), $"{name}.psd"),System.Text.Encoding.UTF8);*/

            var bytes = texture.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);
            var outputPath = exrFilePath.ToAbsolute();
            File.WriteAllBytes(outputPath, bytes);

            AssetDatabase.Refresh();
            //Debug.Log(rectTransform.sizeDelta * cubemapSize);
        }

        public InspectorButton Edit_;
        public void Edit() {
            if (File.Exists(psdFilePath.ToAbsolute())) {
                System.Diagnostics.Process.Start(psdFilePath.ToAbsolute());
            } else {
                if (File.Exists(exrFilePath.ToAbsolute())) {
                    System.Diagnostics.Process.Start(exrFilePath.ToAbsolute());
                } else {
                    if (EditorUtility.DisplayDialog("File not found", $"{exrFilePath} does not exist. Click Render first.", "Render", "Cancel")) {
                        Render();
                        Edit();
                    }                    
                }
            }            
        }

        public InspectorButton Import_;
        public void Import() {
            PsdFile input = new PsdFile(psdFilePath.ToAbsolute(), new LoadContext());
            var layersForImport = input.Layers;


            if (!(layersForImport.Select(x => x.Name).Distinct().Count() == layersForImport.Count())) {
                if (EditorUtility.DisplayDialog("Duplicated layer name", $"Rename layers to be unique.", "Edit", "Cancel")) {
                    Edit();
                } else {
                    return;
                }
            };

            var existingFiles = Directory.EnumerateFiles(resourceDirectory.ToAbsolute(), "*.*", SearchOption.TopDirectoryOnly)
            .Where(s => s.EndsWith(".asset") || s.EndsWith(".mat"))
            .Select(x=>Path.GetFileName(x)).ToList();


            int imageW = input.ColumnCount;
            int imageH = input.RowCount;
            foreach (var l in layersForImport) {
                Color[] pixels = Enumerable.Repeat(new Color(0.5f, 0.5f, 0.5f, 0) , imageW* imageH).ToArray();

                foreach (var c in l.Channels) {
                    var w = c.Rect.Width;
                    var h = c.Rect.Height;
                    var x0 = c.Rect.X;
                    var y0 = c.Rect.Y;

                    
                    int rp = 0;
                    int channelID = c.ID;
                    switch (channelID) {
                        case -1: {
                                float[] values = new float[w * h];
                                System.Buffer.BlockCopy(c.ImageData, 0, values, 0, c.ImageData.Length);
                                for (int y = y0; y < y0 + h; y++) {
                                    for (int x = x0; x < x0 + w; x++) {
                                        pixels[(imageH - y - 1) * imageW + x].a = values[rp];
                                        rp++;
                                    }
                                }
                            }
                            break;
                        case 0:
                        case 1:
                        case 2: {
                                float[] values = new float[w * h];
                                System.Buffer.BlockCopy(c.ImageData, 0, values, 0, c.ImageData.Length);
                                for (int y = y0; y < y0 + h; y++) {
                                    for (int x = x0; x < x0 + w; x++) {
                                        pixels[(imageH - y - 1) * imageW + x][channelID] = values[rp];
                                        rp++;
                                    }
                                }
                            }
                            break;
                    }
                }
                

                if (l.Masks.LayerMask != null) {
                    var m = l.Masks.LayerMask;                   

                    float[] values = new float[m.Rect.Width * m.Rect.Height];
                    System.Buffer.BlockCopy(m.ImageData, 0, values, 0, m.ImageData.Length);
                    int rp = 0;
                    var w = m.Rect.Width;
                    var h = m.Rect.Height;
                    var x0 = m.Rect.X;
                    var y0 = m.Rect.Y;

                    float b = l.Masks.LayerMask.BackgroundColor / 255.0f;
                    var maskPixels = Enumerable.Repeat(b, imageW * imageH).ToArray();

                    for (int y = y0; y < y0 + h; y++) {
                        for (int x = x0; x < x0 + w; x++) {
                            maskPixels[(imageH - y - 1) * imageW + x] = values[rp];
                            rp++;
                        }
                    }
                    for (int i = 0; i < imageH * imageW; i++)
                        pixels[i].a *= maskPixels[i];
                }

                string textureName = $"{name}_{l.Name}.asset";
                string materialName = $"{name}_{l.Name}.mat";
                string texturePath = Path.Combine(resourceDirectory.ToString(), textureName);
                string materialPath = Path.Combine(resourceDirectory.ToString(), materialName);
                existingFiles.Remove(textureName);
                existingFiles.Remove(materialName);

                CreateTextureAndMaterial(pixels, imageW, imageH, texturePath, materialPath);

                

            }

            foreach (var f in existingFiles) {
                string path = Path.Combine(resourceDirectory.ToString(), f);
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.Refresh();
        }

        void CreateTextureAndMaterial(Color[] pixels, int w, int h, string texturePath, string materialPath) {
            Texture2D output = new Texture2D(w, h, GraphicsFormat.R32G32B32A32_SFloat, TextureCreationFlags.None);
            output.wrapMode = TextureWrapMode.Clamp;
            output.SetPixels(pixels);

            AssetDatabase.CreateAsset(output, texturePath);
            AssetDatabase.ImportAsset(texturePath);
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
