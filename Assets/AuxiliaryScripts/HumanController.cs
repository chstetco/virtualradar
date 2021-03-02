using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : MonoBehaviour
{
    //private float minX = -60.0f;
    //private float maxX = 60.0f;
    public Animator animator;
    public float velocity = 0.05f;
    public float angVelocity = 3.0f;

    public GameObject cam;

    private float rotationY = 0.0f;
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
    void FixedUpdate()
    {
        rotationY += Input.GetAxis("Horizontal") * angVelocity;

        if (Input.GetKey("w"))
        {
            posFwd -= (velocity * Time.fixedDeltaTime);
            animator.Play("Base_Layer.WalkFwd");
            animator.speed = velocity;
        }
        else if (Input.GetKey("s"))
        {
            posFwd += (velocity * Time.fixedDeltaTime);
            animator.Play("Base_Layer.WalkBwd");
            animator.speed = velocity;
        }
        else
        {
            posFwd = 0.0f;
            animator.Play("Base_Layer.Idle");
        }
  

        rotVector.y = rotationY;
        posVector.z = -posFwd;

        cam.transform.localEulerAngles = rotVector;
        cam.transform.Translate(posVector * Time.fixedDeltaTime);
    }
}
