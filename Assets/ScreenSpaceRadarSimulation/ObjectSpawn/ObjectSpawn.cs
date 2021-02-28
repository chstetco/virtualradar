using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class ObjectSpawn : MonoBehaviour
{

    public GameObject[] prefabarray;
    public Transform[] spawnpositions;
    public Vector3 Randomoffset;
    public Vector3 Rotation;
    public Material[] matarray;
    GameObject[] gameobjarray;
   
    public bool randomrotation;
    // Start is called before the first frame update
    void Awake()
    {
        gameobjarray = new GameObject[spawnpositions.Length];
        for (int i=0;i<spawnpositions.Length;i++)
        {
            if (randomrotation==false)
            {
                gameobjarray[i] = Instantiate(prefabarray[Random.Range(0, prefabarray.Length)], spawnpositions[i].position + new Vector3(Random.Range(-Randomoffset.x, Randomoffset.x), Random.Range(-Randomoffset.y, Randomoffset.y), Random.Range(-Randomoffset.z, Randomoffset.z)), Quaternion.Euler(Rotation), transform);
            }
            else
            {
                gameobjarray[i] = Instantiate(prefabarray[Random.Range(0, prefabarray.Length)], spawnpositions[i].position + new Vector3(Random.Range(-Randomoffset.x, Randomoffset.x), Random.Range(-Randomoffset.y, Randomoffset.y), Random.Range(-Randomoffset.z, Randomoffset.z)), Quaternion.Euler(Rotation + new Vector3(0,Random.Range(0,360),0)), transform);
            }

           
            gameobjarray[i].GetComponent<Renderer>().material = matarray[Random.Range(0,matarray.Length)];
        }
    }

}
