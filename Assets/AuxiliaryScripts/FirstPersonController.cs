using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    //private float minX = -60.0f;
    //private float maxX = 60.0f;
    public Animator animator; 
    public float sensitivityX = 3.0f;
    public float sensitivityFwd = 0.05f;
    private float sensitivityY = 3.0f;

    public GameObject cam; 

    private float rotationY = 0.0f; 
    private float rotationX = 0.0f;
    private float posFwd = 0.0f;
    private float posSideways = 0.0f;

    private Vector3 rotVector = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 posVector = new Vector3(0.0f, 0.0f, 0.0f);

    // Start is called before the first frame update
    void Start()
    {
        animator = cam.GetComponent<Animator>();
        Debug.Log("First-Person Controller started");
    }

    // Update is called once per frame
    void Update()
    {
        rotationY += Input.GetAxis("Horizontal") * sensitivityY;
        rotationX += Input.GetAxis("Vertical") * sensitivityX;

       
        if (Input.GetAxis("MoveForward") < 0)
        {
            posFwd += Input.GetAxis("MoveForward") * sensitivityFwd;
            animator.Play("Base_Layer.WalkFwd");
        }
        else if (Input.GetAxis("MoveForward") > 0)
        {
            posFwd += Input.GetAxis("MoveForward") * sensitivityFwd;
            animator.Play("Base_Layer.WalkBwd");
        }
        else
        {
            posFwd = 0.0f;
            animator.Play("Base_Layer.Idle");
        }    


        rotVector.x = -rotationX;
        rotVector.y = rotationY;
        posVector.z = -posFwd; 

        cam.transform.localEulerAngles = rotVector;
        cam.transform.Translate(posVector * Time.deltaTime);
    }
}

