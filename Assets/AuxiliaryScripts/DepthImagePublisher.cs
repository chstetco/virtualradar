using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
using System.Linq;
using System.Threading;
using UnityEditor;
using System.Numerics;
using System.Net;
using System.Net.Sockets;
using System;
using RosSharp.RosBridgeClient;
using System.Runtime.InteropServices;

//[ExecuteInEditMode]
namespace RosSharp.RosBridgeClient
{
    public class DepthImagePublisher : UnityPublisher<MessageTypes.Sensor.CompressedImage>
    {
       // [Range(0f, 3f)]

        public Camera ImageCamera;

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

        public int resolutionWidth = 128;
        public int resolutionHeight = 128;
        public int qualityLevel = 1;

        private MessageTypes.Sensor.CompressedImage message;
        private Texture2D texture2D;
        private Rect rect;

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

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

            InitializeGameObject();
            InitializeMessage();

            //ImageCamera.Render();
            //UpdateMessage();
            //Camera.onPostRender += UpdateImage;
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (shader != null)
            {
                //material.SetFloat("_DepthLevel", 1.0f);
                Graphics.Blit(src, dest, material);
                texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.ARGB32, false);
                rect = new Rect(0, 0, resolutionWidth, resolutionHeight);
                texture2D.ReadPixels(rect, 0, 0);
                texture2D.Apply();
            }
            else
            {
                Graphics.Blit(src, dest);
            }
        }

        void Update()
        {
            message.header.Update();
            //texture2D.ReadPixels(rect, 0, 0);
            //message.data = texture2D.EncodeToJPG(qualityLevel);
            // for (int i = 0; i < resolutionWidth*resolutionHeight; i++)
            //{
            //
            //}
            message.data = texture2D.GetRawTextureData();
            Publish(message);
        }

        private void InitializeGameObject()
        {
            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.ARGB32, false);
            rect = new Rect(0, 0, resolutionWidth, resolutionHeight);
            ImageCamera.targetTexture = new RenderTexture(resolutionWidth, resolutionHeight, 0);
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Sensor.CompressedImage();
            message.header.frame_id = "MainCam";
            message.format = "jpeg";
            message.data = new byte[resolutionWidth*resolutionHeight];
        }

       /* private void UpdateMessage()
        {
            message.header.Update();
            texture2D.ReadPixels(rect, 0, 0);
            message.data = texture2D.EncodeToJPG(qualityLevel);
            Publish(message);
        }

        private void UpdateImage(Camera _camera)
        {
            if (texture2D != null && _camera == this.ImageCamera)
                UpdateMessage();
        }
       */
    }
}
