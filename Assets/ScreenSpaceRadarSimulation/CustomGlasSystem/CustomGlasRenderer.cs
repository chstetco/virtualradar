using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;




public class CustomGlasSystem
{
    static CustomGlasSystem m_Instance; // singleton
    static public CustomGlasSystem instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = new CustomGlasSystem();
            return m_Instance;
        }
    }

    internal HashSet<CustomGlasObj> m_GlasObjs = new HashSet<CustomGlasObj>();

    public void Add(CustomGlasObj o)
    {
        Remove(o);
        m_GlasObjs.Add(o);
        //Debug.Log("added effect " + o.gameObject.name);
    }

    public void Remove(CustomGlasObj o)
    {
        m_GlasObjs.Remove(o);
        //Debug.Log("removed effect " + o.gameObject.name);
    }
}

//[ExecuteInEditMode]
public class CustomGlasRenderer : MonoBehaviour
{

    private CommandBuffer m_GlasBuffer;
    private Dictionary<Camera, CommandBuffer> m_Cameras = new Dictionary<Camera, CommandBuffer>();

    private void Cleanup()
    {
        foreach (var cam in m_Cameras)
        {
            if (cam.Key)
                cam.Key.RemoveCommandBuffer(CameraEvent.BeforeLighting, cam.Value);
        }
        m_Cameras.Clear();
    }


    public void OnDisable()
    {
        Cleanup();
    }

    public void OnEnable()
    {
        Cleanup();
    }

    //
     void Update()
    {
        Cleanup();
    }
    //
    public void OnWillRenderObject()
    {
        var render = gameObject.activeInHierarchy && enabled;
        if (!render)
        {
            Cleanup();
            return;
        }
        
        var cam = Camera.current;
        if (!cam)
            return;

        if (m_Cameras.ContainsKey(cam))
            return;

        // create new command buffer
        m_GlasBuffer = new CommandBuffer();
        m_GlasBuffer.name = "Glas map buffer";
        m_Cameras[cam] = m_GlasBuffer;

        var glasSystem = CustomGlasSystem.instance;

        // create render texture for glas map
        int tempID = Shader.PropertyToID("_Temp1");
        m_GlasBuffer.GetTemporaryRT(tempID, -1, -1, 24, FilterMode.Bilinear);
        m_GlasBuffer.SetRenderTarget(tempID);
        m_GlasBuffer.ClearRenderTarget(true, true, Color.black); // clear before drawing to it each frame!!

        // draw all glas objects to it
        foreach (CustomGlasObj o in glasSystem.m_GlasObjs)
        {
            Renderer r = o.GetComponent<Renderer>();
            Material glasMat = o.GlasMaterial;
            if (r && glasMat)
                m_GlasBuffer.DrawRenderer(r, glasMat);
        }

        // set render texture as globally accessable 'glas map' texture
        m_GlasBuffer.SetGlobalTexture("_GlasMap", tempID);

        // add this command buffer to the pipeline
        cam.AddCommandBuffer(CameraEvent.BeforeLighting, m_GlasBuffer);
    }
}
