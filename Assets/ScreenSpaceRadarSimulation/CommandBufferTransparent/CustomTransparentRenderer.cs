using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;


#region Transparent objects to transparent system
public class CustomTransparentSystem
{
    static CustomTransparentSystem m_Instance; // singleton
    static public CustomTransparentSystem instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = new CustomTransparentSystem();
            return m_Instance;
        }
    }

    internal HashSet<CustomTransparentObj> m_GlasObjs = new HashSet<CustomTransparentObj>();

    public void Add(CustomTransparentObj o)//add transparent object to renderer
    {
        Remove(o);
        m_GlasObjs.Add(o);
        
    }

    public void Remove(CustomTransparentObj o)//remove transparent object to renderer
    {
        m_GlasObjs.Remove(o);
        
    }
}
#endregion

//The object with this script must have a visible mesh renderer component inside the scene because of the OnWillRenderObject methode.
//In best case create a big cube and add this script to it. Your full scene should inside this cube (volume).
[RequireComponent(typeof(MeshRenderer))]
public class CustomTransparentRenderer : MonoBehaviour
{

    private CommandBuffer m_GlasDepthNormalBuffer;
  
    private Dictionary<Camera, CommandBuffer> m_Cameras = new Dictionary<Camera, CommandBuffer>();
   
    private void Cleanup()
    {
        foreach (var cam in m_Cameras)
        {
            if (cam.Key)
            {
                
                cam.Key.RemoveCommandBuffer(CameraEvent.AfterGBuffer, m_GlasDepthNormalBuffer);// rremove command buffer from radar sensor
            
            }              
        }
        m_Cameras.Clear();
    }


    public void OnDisable()//run this mehode if object get disabled
    {
        Cleanup();
    }

    public void OnEnable()//run this mehode if object get enabled
    {
        Cleanup();
    }

    //

   
    void Update()//run this mehode every frame
    {
        Cleanup();

       
    }
    //

     void OnWillRenderObject() // The function is called during the culling process just before rendering each culled object
    {
       
        var render = gameObject.activeInHierarchy && enabled; // is object with transparent renderer activated
        if (!render)
        {

            Cleanup();
            return;
        }

        var cam = Camera.main;
        if (!cam)
            return;

        if (m_Cameras.ContainsKey(cam))
            return;

        /// create new command buffer
        m_GlasDepthNormalBuffer = new CommandBuffer();
        m_GlasDepthNormalBuffer.name = "Glas map depth buffer";
        m_Cameras[cam] = m_GlasDepthNormalBuffer; // command buffer to camera
        ///
       

        var glasSystem = CustomTransparentSystem.instance;//transparent system





       
        #region Creat depth texture     
        int tempID = Shader.PropertyToID("_GlasDepthMap");//Unique integer for the name
        m_GlasDepthNormalBuffer.GetTemporaryRT(tempID, -1, -1, 24, FilterMode.Bilinear);//creates a temporary render texture
        m_GlasDepthNormalBuffer.SetRenderTarget(tempID);//Sets current render target
        m_GlasDepthNormalBuffer.ClearRenderTarget(true, true, Color.black); // clear before drawing to it each frame!!
        foreach (CustomTransparentObj o in glasSystem.m_GlasObjs)//search transparent object in transparent system
        {
            Renderer r = o.GetComponent<Renderer>();//get renderer component
            Material glasMatDepthNormal = o.CustomDepthNormalMaterial;//get material from renderer object 
    
            m_GlasDepthNormalBuffer.DrawRenderer(r, glasMatDepthNormal,0,0);//draw object with shader pass 0 from shader in material(glasMatDepthNormal)          
        }
        m_GlasDepthNormalBuffer.SetGlobalTexture("_GlasDepthMap", tempID);// set render texture as globally accessable _GlasDepthMap
        #endregion

        
        #region Creat normal texture
        int tempID2 = Shader.PropertyToID("_GlasNormalMap");
        m_GlasDepthNormalBuffer.GetTemporaryRT(tempID2, -1, -1, 24, FilterMode.Bilinear);
        m_GlasDepthNormalBuffer.SetRenderTarget(tempID2);
        m_GlasDepthNormalBuffer.ClearRenderTarget(true, true, Color.black); // clear before drawing to it each frame!!
        foreach (CustomTransparentObj o in glasSystem.m_GlasObjs)
        {
            Renderer r = o.GetComponent<Renderer>();
            Material glasMatDepthNormal = o.CustomDepthNormalMaterial;

            m_GlasDepthNormalBuffer.DrawRenderer(r, glasMatDepthNormal, 0, 1);//draw object with shader pass 1 from shader in material(glasMatDepthNormal)
        }
        m_GlasDepthNormalBuffer.SetGlobalTexture("_GlasNormalMap", tempID2);
        #endregion

        
        #region Creat metal texture front face
        int tempID3 = Shader.PropertyToID("_GlasMetalMapFront");
        m_GlasDepthNormalBuffer.GetTemporaryRT(tempID3, -1, -1, 24, FilterMode.Bilinear);
        m_GlasDepthNormalBuffer.SetRenderTarget(tempID3);
        m_GlasDepthNormalBuffer.ClearRenderTarget(true, true, Color.black); // clear before drawing to it each frame!!
        foreach (CustomTransparentObj o in glasSystem.m_GlasObjs)
        {
            Renderer r = o.GetComponent<Renderer>();
            Material glasMatDepthNormal = o.CustomDepthNormalMaterial;
            m_GlasDepthNormalBuffer.DrawRenderer(r, glasMatDepthNormal, 0, 2);//draw object with shader pass 2 from shader in material(glasMatDepthNormal)
        }
        m_GlasDepthNormalBuffer.SetGlobalTexture("_GlasMetalMapFront", tempID3);
        #endregion

        #region Creat metal texture back face
        int tempID5 = Shader.PropertyToID("_GlasMetalMapBack");
        m_GlasDepthNormalBuffer.GetTemporaryRT(tempID5, -1, -1, 24, FilterMode.Bilinear);
        m_GlasDepthNormalBuffer.SetRenderTarget(tempID5);
        m_GlasDepthNormalBuffer.ClearRenderTarget(true, true, Color.black); // clear before drawing to it each frame!!
        foreach (CustomTransparentObj o in glasSystem.m_GlasObjs)
        {
            Renderer r = o.GetComponent<Renderer>();
            Material glasMatDepthNormal = o.CustomDepthNormalMaterial;
            m_GlasDepthNormalBuffer.DrawRenderer(r, glasMatDepthNormal, 0, 4);//draw object with shader pass 2 from shader in material(glasMatDepthNormal)
        }
        m_GlasDepthNormalBuffer.SetGlobalTexture("_GlasMetalMapBack", tempID5);
        #endregion


        #region Creat velocity texture
        int tempID4 = Shader.PropertyToID("_VMaskMap");
        m_GlasDepthNormalBuffer.GetTemporaryRT(tempID4, -1, -1, 24, FilterMode.Bilinear);
        m_GlasDepthNormalBuffer.SetRenderTarget(tempID4);
        m_GlasDepthNormalBuffer.ClearRenderTarget(true, true, Color.black); // clear before drawing to it each frame!!
           foreach (CustomTransparentObj o in glasSystem.m_GlasObjs)
            {
                Renderer r = o.GetComponent<Renderer>();
                Material glasMatDepthNormal = o.CustomDepthNormalMaterial;
                m_GlasDepthNormalBuffer.DrawRenderer(r, glasMatDepthNormal, 0, 3);//draw object with shader pass 3 from shader in material(glasMatDepthNormal)
        }
        m_GlasDepthNormalBuffer.SetGlobalTexture("_VMaskMap", tempID4);
        #endregion

        ///Release render textures
        m_GlasDepthNormalBuffer.ReleaseTemporaryRT(tempID);
        m_GlasDepthNormalBuffer.ReleaseTemporaryRT(tempID2);
        m_GlasDepthNormalBuffer.ReleaseTemporaryRT(tempID3);
        m_GlasDepthNormalBuffer.ReleaseTemporaryRT(tempID4);
        m_GlasDepthNormalBuffer.ReleaseTemporaryRT(tempID5);
        ///


        cam.AddCommandBuffer(CameraEvent.AfterGBuffer, m_GlasDepthNormalBuffer);// add this command buffer to the pipeline


    }


}
