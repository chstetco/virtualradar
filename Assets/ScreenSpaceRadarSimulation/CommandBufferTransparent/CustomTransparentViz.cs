using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CustomTransparentViz : MonoBehaviour
{
    [Tooltip("Visualization Material")]
    public Material material;
    private Camera cam;
    [Range(0.0f,1.0f)]
    [Tooltip("Blend normal map")]
    public float lerp;
    [Range(0.0f, 1.0f)]
    [Tooltip("Blend metal map")]
    public float lerp2;
    [Range(0.0f, 1.0f)]
    [Tooltip("Blend velocity map")]
    public float lerp3;
    
    void Start()
    {
        cam = Camera.main;
        cam.depthTextureMode = DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }

     void Update()
    {
        material.SetFloat("_Lerp", lerp);
        material.SetFloat("_Lerp2", lerp2);
        material.SetFloat("_Lerp3", lerp3);
    }
}

