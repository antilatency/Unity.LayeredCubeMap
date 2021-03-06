﻿using UnityEngine;
using UnityEditor;

namespace Antilatency {
    [ExecuteAlways]
    public class SkyControl : MonoBehaviour {


        public static SkyControl instance => FindObjectOfType<SkyControl>();
        public static Vector3 position => instance.transform.position;

        // Start is called before the first frame update
        void Start() {

        }

        private void OnEnable() {
            SceneView.duringSceneGui+= OnScene;
        }
        private void OnDisable() {
            SceneView.duringSceneGui -= OnScene;
        }

        void Update() {
            

            if (!RenderSettings.skybox) return;
            if (RenderSettings.skybox.HasProperty("_MatrixX")) {
                RenderSettings.skybox.SetVector("_MatrixX", transform.worldToLocalMatrix.GetRow(0));
                RenderSettings.skybox.SetVector("_MatrixY", transform.worldToLocalMatrix.GetRow(1));
                RenderSettings.skybox.SetVector("_MatrixZ", transform.worldToLocalMatrix.GetRow(2));
                RenderSettings.skybox.SetVector("_Origin", transform.position);
            }
        }

        void OnScene(SceneView scene) {
            Event e = Event.current;

            if (e.type == EventType.MouseDown) {
                if (e.button == 1) {
                    scene.pivot = scene.pivot - scene.camera.transform.position + transform.position;
                }
            }
            
        }
    }


}