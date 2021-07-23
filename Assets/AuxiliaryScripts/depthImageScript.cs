using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode]
public class depthImageScript : MonoBehaviour
{
    [Range(0f, 3f)]
    public float depthLevel = 0.5f;

    private Shader _shader;
    private Shader shader
    {
        get 
        { 
            return _shader != null ? _shader : (_shader = Shader.Find("Hidden/depthImageShader")); 
        }
    }

    private Material _material;
    private Material material  
    {
        get
        {
            if (_material == null)
            {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave; 
            }
            return _material;
        }
    } 

    // Start is called before the first frame update
    void Start()
    {
        if (!SystemInfo.supportsImageEffects)
        {
            print("System doesn't support image effects");
            enabled = false;
            return;
        }

        if (shader == null || !shader.isSupported)
        {
            enabled = false;
            print("Shader" + shader.name + "is not supported");
            return;
        }

        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth; 
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (shader != null)
        {
            Debug.Log("Executing...");
            material.SetFloat("_DepthLevel", depthLevel);
            Graphics.Blit(src, dest, material);
        }
        else
        {
            Debug.Log("Executing...");
            Graphics.Blit(src, dest);
        }
    }
}
