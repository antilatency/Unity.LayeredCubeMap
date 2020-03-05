using UnityEngine;

[ExecuteAlways]
public class SkyControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update(){
        if (!RenderSettings.skybox) return;
        //if (RenderSettings.skybox.HasProperty("SkyMatrix")) {
        RenderSettings.skybox.SetVector("_MatrixX", transform.worldToLocalMatrix.GetRow(0));
        RenderSettings.skybox.SetVector("_MatrixY", transform.worldToLocalMatrix.GetRow(1));
        RenderSettings.skybox.SetVector("_MatrixZ", transform.worldToLocalMatrix.GetRow(2));
        //}
    }
}
