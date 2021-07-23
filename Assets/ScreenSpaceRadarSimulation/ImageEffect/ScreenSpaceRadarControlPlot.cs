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
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RosSharp.RosBridgeClient
{
    public class ScreenSpaceRadarControlPlot : MonoBehaviour
    {
        /* Properties for UI -- mainly contains radar sensor configuration parameters */
        #region Properties

        Material mat;
        Material copydepthmat;

        // Option Selection
        public enum Option {Unity_and_TCP, Unity_to_ROS }; 
        [Tooltip("Select type of simulation")]
        public Option optionselection;

        // Field of View of the radar sensor
        [Tooltip("Field of view")]
        [Range(0.0f, 179.0f)]
        public float fov = 100.0f;

        // Number of Chirps
        [Tooltip("Number of chirps")]
        [Range(1, 32)]
        public int chirps = 16;
        int savechirps; // lock chirps at play mode (prevent array length problems)

        // Number of samples per chirp
        [Tooltip("Number of samples")]
        [Range(1, 1024)]
        public int samples = 256;
        int savesamples; // lock samples at play mode (prevent array length problems)

        // Configuration of receiver antennas (azimuth or elevation)
        [Range(1, 2)]
        [Tooltip("Receiver Configuration")]
        public int recv_config = 1;

        // Number of receiveing antenna elements
        [Range(1, 4)]
        [Tooltip("Number of Receiver Antennas")]
        public int antennas = 1;

        // radar sampling frequency
        [Range(10000.0f, 10000000.0f)]
        [Tooltip("Sampling Frequency of the radar chip")]
        public float samplingFrequency = 2000000.0f;

        // lower chirp frequency (start frequency)
        [Tooltip("Lower chirp frequency")]
        public float lowerFrequency;

        // chirp bandwidth
        [Tooltip("Bandwidth of the chirp")]
        public float bandwidth;

        // Radiation Pattern Mask of radar sensor
        [Tooltip("Radiation Pattern Weighting Texture(No texture means no pattern)")]
        public Texture RadiationPatternMask;

        // Radiation Pattern Mask of radar sensor
        [Tooltip("Radiation Pattern Weighting Texture(No texture means no pattern)")]
        public Texture RoughnessMap;

        // ROS connector for ROS communication via rosbridge
        private StreamWriter writer;
        [Tooltip("ROS connector (required Unity_to_ROS)")]
        public RosConnector rosConnector;

        private ComputeBuffer gpuBuffer1; // buffer 1 on GPU memory
        private ComputeBuffer gpuBuffer2; // buffer 2 on GPU memory
        private Camera cam; // radar sensor object
        private float Ts;                       // sampling rate
        private float lambda;                   // wavelength
        private float centerFrequency;          // chirp center frequency
        private float K;                        // chirp rate
        private float maxRange;                 // max. sensing range
        private float maxVelocity;              // max. velocity
        private float c0 = 299792458.0f;        // speed of light
        private RenderTexture pastFrame;        // render texture for last frame depth
        private RenderTexture holdcurrentFrame; // render texture for hold actual depth 
        private bool help;                      // compute buffer read lock variable
        private Thread thread;                  // thread for data sum 
        private UnityEngine.Vector2[] vec1;      // data vector array (after GPU read)
        private UnityEngine.Vector2[] vec2;      // data vector array (after GPU read)
        private UnityEngine.Vector2[] data1;     // data vector array (after data sum thread)
        private UnityEngine.Vector2[] data2;     // data vector array (after data sum thread)
        private bool helpplot;                  // update method lock variable
        private int width;                      // image width (in pixels)
        private int height;                     // image height (in pixels)
        private float[] t;                      // time vector
   
        /* TCP/ROS related variables */
        internal Boolean socketReady = false;
        private TcpClient mySocket;             // socket for TCP connection
        private NetworkStream theStream;
        private StreamWriter theWriter;         // write stream
        private StreamReader theReader;         // read stream
        private String Host = "localhost";      // 
        private Int32 Port = 55000;             // TCP port number
        private float[] senddataarray;          // data array for transmission
        private string distancestring;          // distance string for OnlyUnity simulation
        private string velocitystring;          // velocity string for OnlyUnity simulation
        private FloatArrayPublisher floatpup;   // ROS publisher

        GameObject GlasRenderer;                // needed for rendering transparent objects
        #endregion

        #region Called when script instance is being loaded 
        void Awake() // run before all other functions
        {
            if (optionselection == Option.Unity_to_ROS)
            {
                rosConnector.gameObject.SetActive(true); // activate ROS connector
            }
            else
            {
                rosConnector.gameObject.SetActive(false); // deactivate ROS connector
            }

            // assign shader to use
            mat = new Material(Shader.Find("Hidden/ScreenSpaceRadarShader"));
            copydepthmat = new Material(Shader.Find("Hidden/CopyDepthShader"));

            savechirps = chirps;
            savesamples = samples;

            // GlasRenderer init -- needed for transparent objects
            UnityEngine.Vector3[] vertices = {
                new UnityEngine.Vector3 (-0.5f, -0.5f, -0.5f),
                new UnityEngine.Vector3 (0.5f, -0.5f, -0.5f),
                new UnityEngine.Vector3 (0.5f, 0.5f, -0.5f),
                new UnityEngine.Vector3 (-0.5f, 0.5f, -0.5f),
                new UnityEngine.Vector3 (-0.5f, 0.5f, 0.5f),
                new UnityEngine.Vector3 (0.5f, 0.5f, 0.5f),
                new UnityEngine.Vector3 (0.5f, -0.5f, 0.5f),
                new UnityEngine.Vector3 (-0.5f, -0.5f, 0.5f),
            };

            int[] triangles = {
                    0, 2, 1, //face front
                    0, 3, 2,
                    2, 3, 4, //face top
                    2, 4, 5,
                    1, 2, 5, //face right
                    1, 5, 6,
                    0, 7, 4, //face left
                    0, 4, 3,
                    5, 4, 7, //face back
                    5, 7, 6,
                    0, 6, 7, //face bottom
                    0, 1, 6
             };

            GlasRenderer = new GameObject("GlasRenderer");
            GlasRenderer.AddComponent<MeshFilter>();
            Mesh mesh = GlasRenderer.GetComponent<MeshFilter>().mesh;
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.Optimize();
            mesh.RecalculateNormals();
            GlasRenderer.GetComponent<MeshFilter>().mesh = mesh;
            GlasRenderer.AddComponent<MeshRenderer>();
            GlasRenderer.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            GlasRenderer.transform.localScale = new UnityEngine.Vector3(1000.0f, 1000.0f, 1000.0f);
            GlasRenderer.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            GlasRenderer.AddComponent(typeof(CustomTransparentRenderer));
            
        }
        #endregion

        /* Sensor Simulation Initialization */
        #region Initialization   
        void Start()
        {
            cam = Camera.main;                                      // fetch camera object
            cam.renderingPath = RenderingPath.DeferredShading;      // use deferred rendering path
            cam.depthTextureMode = DepthTextureMode.Depth;          // activate depthmode (needed for depth texture)

            pastFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);          // create render texture for past depth frame
            holdcurrentFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);   // create render texture for current depth frame
            helpplot = false; // Hilfsvariable

            Ts = samples / samplingFrequency;
            K = bandwidth / Ts;
            centerFrequency = lowerFrequency + (bandwidth * 05f);
            lambda = c0 / centerFrequency;
            maxRange = c0 * samples / (4.0f * bandwidth);
            maxVelocity = lambda / (4.0f * Ts);

            // create TCP client if TCP is set active, otherwise set up ROS publisher
            if (optionselection == Option.Unity_and_TCP)
            {
                mat.SetInt("_Enum", 1);
                mySocket = new TcpClient(Host, Port); // create TCP client object
                theStream = mySocket.GetStream();
                socketReady = true;
            }
            else if (optionselection == Option.Unity_to_ROS)
            {
                mat.SetInt("_Enum", 2); 
                floatpup = rosConnector.GetComponent<FloatArrayPublisher>(); 
            }
            help = true; // init
        }
        #endregion

        /* Configuration of the render pipeline setting up all stuff needed for GPU */
        #region Radar render pipeline configuration and initialization
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (antennas <= 2)
            {
                if (gpuBuffer1 == null)
                {
                    int number;
                    number = cam.pixelWidth * cam.pixelHeight * samples * chirps;                     // init number of pixel
                    Graphics.ClearRandomWriteTargets();                                                 // This function clears any "random write" targets that were previously set with SetRandomWriteTarget.
                    gpuBuffer1 = new ComputeBuffer(number, 2 * sizeof(float), ComputeBufferType.Default); // ComputeBufferType.Default
                    Graphics.SetRandomWriteTarget(1, gpuBuffer1);
                }
            }
            else
            {
                if (gpuBuffer1 == null && gpuBuffer2 == null)
                {
                    int number;
                    number = cam.pixelWidth * cam.pixelHeight * samples * chirps;                         // init number of pixel
                    Graphics.ClearRandomWriteTargets();                                                     // This function clears any "random write" targets that were previously set with SetRandomWriteTarget.
                    gpuBuffer1 = new ComputeBuffer(number, 2 * sizeof(float), ComputeBufferType.Default);   // ComputeBufferType.Default
                    gpuBuffer2 = new ComputeBuffer(number, 2 * sizeof(float), ComputeBufferType.Default);   // ComputeBufferType.Default
                    Graphics.SetRandomWriteTarget(1, gpuBuffer1);
                    Graphics.SetRandomWriteTarget(2, gpuBuffer2);
                }
            }

            if (Screen.width != pastFrame.width || Screen.height != pastFrame.height)
            {
                pastFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);          // create new render texture
                holdcurrentFrame = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBFloat);   // create render texture for current depth frame
            }

            if (antennas <= 2)
            {
                Graphics.SetRandomWriteTarget(1, gpuBuffer1);
                mat.SetBuffer("_gpuBuffer1", gpuBuffer1);
            }
            else
            {
                Graphics.SetRandomWriteTarget(1, gpuBuffer1);
                Graphics.SetRandomWriteTarget(2, gpuBuffer2);
                mat.SetBuffer("_gpuBuffer1", gpuBuffer1);
                mat.SetBuffer("_gpuBuffer2", gpuBuffer2);
            }

            // set shader input values
            mat.SetInt("_width", cam.pixelWidth);
            mat.SetInt("_height", cam.pixelHeight);
            mat.SetInt("_chirpsNumber", chirps);
            mat.SetInt("_samplesNumber", samples);
            mat.SetFloat("_fov", fov);
            mat.SetInt("_NrAntennas", antennas);
            mat.SetInt("_ReceiverConfig", recv_config);
            mat.SetFloat("_ChirpRate", K);
            mat.SetFloat("_LowerChirpFrequency", lowerFrequency);
            mat.SetFloat("_BandwidthOfTheChirp", bandwidth);
            mat.SetTexture("_BTex", pastFrame);
            mat.SetFloat("_MaxDistance", maxRange);
            mat.SetFloat("_MaxVelocity", maxVelocity);
    
            Graphics.Blit(source, holdcurrentFrame, copydepthmat);              // source to destination render texture
            Shader.SetGlobalTexture("_HoldDepthTexture", holdcurrentFrame);     // set global texture for command buffer
            Graphics.Blit(source, destination, mat);                            // copies source texture to destination texture 
            Graphics.Blit(source, pastFrame, copydepthmat);                     // source to destination render texture  

            Graphics.ClearRandomWriteTargets();

            if (help == true)
            {
                help = false;
                vec1 = new UnityEngine.Vector2[cam.pixelWidth * cam.pixelHeight * samples * chirps];
                vec2 = new UnityEngine.Vector2[cam.pixelWidth * cam.pixelHeight * samples * chirps];

                if (antennas <= 2)
                {
                    gpuBuffer1.GetData(vec1);              // read data from GPU memory
                }
                else
                {
                    gpuBuffer1.GetData(vec1);              // read data from GPU memory
                    gpuBuffer2.GetData(vec2);              // read data from GPU memory
                }

                width = Screen.width;
                height = Screen.height;

                thread = new Thread(Calc);          // create thread for sum calculation
                thread.Start();                     // start thread for sum calculation
            }
        }
        #endregion

        /* Fetching GPU data and unwrap it in an array data format for further processing */
        #region Calculate the sum of radar simulation data 
        void Calc()
        {
            var stopwatch = Stopwatch.StartNew();                    // stopwatch for time debugging
            data1 = new UnityEngine.Vector2[samples * chirps];     // Final vector with data
            data2 = new UnityEngine.Vector2[samples * chirps];

            // start timing
            //stopwatch.Start();

            // fetch data from GPU 
            Parallel.For(0, chirps, c =>
            {
                for (int a = 0; a < samples; a++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        for (int h = 0; h < height; h++)
                        {
                            if (antennas == 1)
                            {
                                data1[a + samples * c].x += vec1[w + width * h + width * height * a + width * height * samples * c].x; // sum of the image data element and 4d to 2d (antenna 1)
                                data1[a + samples * c].y += 0.0f; //  vec1[w + width * h + width * height * a + width * height * samples * c].y;
                                data2[a + samples * c].x += 0.0f;
                                data2[a + samples * c].y += 0.0f;
                            }
                            else if (antennas == 2)
                            {
                                data1[a + samples * c].x += vec1[w + width * h + width * height * a + width * height * samples * c].x; // sum of the image data element and 4d to 2d (antenna 1)
                                data1[a + samples * c].y += vec1[w + width * h + width * height * a + width * height * samples * c].y; // sum of the image data element and 4d to 2d (antenna 2)    
                                data2[a + samples * c].x += 0.0f;
                                data2[a + samples * c].y += 0.0f;
                            }
                            else if (antennas == 3)
                            {
                                data1[a + samples * c].x += vec1[w + width * h + width * height * a + width * height * samples * c].x; // sum of the image data element and 4d to 2d (antenna 1)
                                data1[a + samples * c].y += vec1[w + width * h + width * height * a + width * height * samples * c].y; // sum of the image data element and 4d to 2d (antenna 2) 
                                data2[a + samples * c].x += vec2[w + width * h + width * height * a + width * height * samples * c].x; // sum of the image data element and 4d to 2d (antenna 3)
                                data2[a + samples * c].y = 0.0f;
                            }
                            else
                            {
                                data1[a + samples * c].x += vec1[w + width * h + width * height * a + width * height * samples * c].x; // sum of the image data element and 4d to 2d (antenna 1)
                                data1[a + samples * c].y += vec1[w + width * h + width * height * a + width * height * samples * c].y; // sum of the image data element and 4d to 2d (antenna 2) 
                                data2[a + samples * c].x += vec2[w + width * h + width * height * a + width * height * samples * c].x; // sum of the image data element and 4d to 2d (antenna 3)
                                data2[a + samples * c].y += vec2[w + width * h + width * height * a + width * height * samples * c].y; // sum of the image data element and 4d to 2d (antenna 4)
                            }
                        }
                    }
                }
            });

            // stop timing and output elapsed time
            //stopwatch.Stop();
            //UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds.ToString("F4"));
           //UnityEngine.Debug.Log(vec1[1].ToString("F4"));
           //UnityEngine.Debug.Log(vec2[1].ToString("F4"));
           //stopwatch.Reset();

            // unwrap raw data in a 1D vector containing both receiver signals sendarray = [Rx1, Rx2]

            senddataarray = new float[data1.Length * antennas];

            if (antennas == 1)
            {
                for (int i = 0; i < senddataarray.Length; i++)
                {
                    senddataarray[i] = data1[i].x;
                }
            }
            else if (antennas == 2)
            {
                for (int i = 0; i < senddataarray.Length; i++)
                {
                    if (i < senddataarray.Length / 2)
                    {
                        senddataarray[i] = data1[i].x;
                    }
                    else
                    {
                        senddataarray[i] = data1[i - senddataarray.Length / 2].y;
                    }
                }
            }
            else if (antennas == 3)
            {
                for (int i = 0; i < senddataarray.Length; i++)
                {
                    if (i < senddataarray.Length / 3)
                    {
                        senddataarray[i] = data1[i].x;
                    }
                    else if ((i >= senddataarray.Length / 3) && (i < (2 * senddataarray.Length / 3)))
                    {
                        senddataarray[i] = data1[i - senddataarray.Length / 3].y;
                    }
                    else
                    {
                        senddataarray[i] = data2[i - 2 * senddataarray.Length / 3].x;
                    }
                }
            }
            else
            {
                for (int i = 0; i < senddataarray.Length; i++)
                {
                    if (i < senddataarray.Length / 4)
                    {
                        senddataarray[i] = data1[i].x;
                    }
                    else if ((i >= senddataarray.Length / 4) && (i < senddataarray.Length / 2))
                    {
                        senddataarray[i] = data1[i - senddataarray.Length / 4].y;
                    }
                    else if ((i >= senddataarray.Length / 2) && (i < (3 * senddataarray.Length / 4)))
                    {
                        senddataarray[i] = data2[i - senddataarray.Length / 2].x;
                    }
                    else
                    {
                        senddataarray[i] = data2[i - 3 * senddataarray.Length / 4].y;
                    }
                }
            }
            helpplot = true;
        }
        #endregion

        /* TCP Connection setup and data transfer */
        #region TCP connection and data transfer    
        public void setupSocket()
        {
            try
            {
                theWriter = new StreamWriter(theStream);
                var byteArray = new byte[senddataarray.Length * 4];                     // byte array for data transfer
                Buffer.BlockCopy(senddataarray, 0, byteArray, 0, byteArray.Length);     // float array to byte array
                mySocket.GetStream().Write(byteArray, 0, byteArray.Length);
                help = true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Socket error: " + e);
            }
        }
        #endregion

        /* Update function - gets called at every simulation step */
        #region Method is called once per frame
        void Update()
        {
            chirps = savechirps;
            samples = savesamples;

            if (helpplot == true)
            {
                helpplot = false;
              
                if (optionselection == Option.Unity_and_TCP)
                {
                    setupSocket(); // TCP connection ad data transfer
                }
                else if (optionselection == Option.Unity_to_ROS)
                {
                    floatpup.messageData = new float[data1.Length * antennas]; // 2D-Vector to 1D float array (needed for tcp data transfer)
                    for (int i = 0; i < senddataarray.Length; i++)
                    {
                        floatpup.messageData[i] = senddataarray[i];
                    }
                    help = true;
                }
                else
                {
                    help = true;
                }

            }

            cam.farClipPlane = maxRange; // CameraFarPlane

            mat.SetTexture("_RoughnessMap", RoughnessMap);

            if (RadiationPatternMask != null)
            {
                mat.SetTexture("_RadiationPatternMask", RadiationPatternMask);
            }
            else
            {
                mat.SetTexture("_RadiationPatternMask", Texture2D.whiteTexture);
            }

            mat.SetVector("_ViewDir", new UnityEngine.Vector4(cam.transform.forward.x, cam.transform.forward.y, cam.transform.forward.z, 0)); // ViewDirection of sensor to shader                                                                                                                                     
            cam.fieldOfView = fov;
            GlasRenderer.transform.position = Camera.main.transform.position;
        }
        #endregion

        /* GPU memory clean to be ready for next step */
        #region Clean gpu memory on disable component
        void OnDisable()
        {
            if (gpuBuffer1 != null)
            {
                gpuBuffer1.Dispose();
            }

            if (gpuBuffer2 != null)
            {
                gpuBuffer2.Dispose();
            }

            gpuBuffer1 = null;
            gpuBuffer2 = null;
        }
        #endregion
    }
}