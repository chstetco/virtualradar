
using UnityEngine;

public class CustomTransparentObj : MonoBehaviour
{
    
    [HideInInspector]
    public Material CustomDepthNormalMaterial;//Pipeline injection material
    [Range(0.0f, 1.0f)]
    [Tooltip("Conductive Control(Metal)")]
    public float conductiveAdjustment = 0.01f;
    public Texture roughnessMap; 

    Vector3 oldcampos;//save previous camera(radar sensor) position

    float olddistanceCamAndObj;//distance between camera and this object
    float velocity;
    bool help;// help variable (lock variable)

    public void OnEnable()//run this mehode if object get enabled
    {
        CustomTransparentSystem.instance.Add(this);//add transparent object to transparent renderer
    }

    public void Start()//Initialization
    {
        velocity = 0.0f;
        oldcampos = new Vector3(0.0f, 0.0f, 0.0f);
        oldcampos = Camera.main.transform.position;//radar sensor position
        olddistanceCamAndObj = Vector3.Distance(Camera.main.transform.position, transform.position);//distance between camera and this object
        CustomDepthNormalMaterial = new Material(Shader.Find("Custom/CustomTransparentDepthNormal"));//creat new Material to get a new instance
        CustomTransparentSystem.instance.Add(this);//add transparent object to transparent renderer
        help = true;
    }

    public void OnDisable()//run this mehode if object get disabled
    {
        CustomTransparentSystem.instance.Remove(this);
    }

    private void LateUpdate()// run every frame
    {
          if (help==true)
          {
              help = false;
            
            float distanceCamAndObj = Vector3.Distance(Camera.main.transform.position, transform.position);
            if (olddistanceCamAndObj >= distanceCamAndObj)//get moving direction
              {
                
                  velocity = Vector3.Distance(Camera.main.transform.position - transform.position, oldcampos) / Time.deltaTime;
              }
              else
              {
                
                    velocity = -(Vector3.Distance(Camera.main.transform.position - transform.position, oldcampos) / Time.deltaTime);
              }


            olddistanceCamAndObj = distanceCamAndObj;
            oldcampos = Camera.main.transform.position - transform.position;
            CustomDepthNormalMaterial.SetFloat("_Velocity", velocity);//set velocity to transparent renderer material
            CustomDepthNormalMaterial.SetFloat("_ConductiveAdjustment", conductiveAdjustment);//set conductiveAdjustment to transparent renderer material


            help = true;
          }
        
       
       
       
    }
}
