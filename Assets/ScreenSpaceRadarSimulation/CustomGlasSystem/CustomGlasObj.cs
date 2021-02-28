
using UnityEngine;

//[ExecuteInEditMode]
public class CustomGlasObj : MonoBehaviour
{
    public Material GlasMaterial;
    [Range(0.0f, 1.0f)]
    public float absorption=0.0f;
    public void OnEnable()
    {
        CustomGlasSystem.instance.Add(this);
    }

    public void Start()
    {
        GlasMaterial = new Material(Shader.Find("Custom/Glas"));
        CustomGlasSystem.instance.Add(this);
    }

    public void OnDisable()
    {
        CustomGlasSystem.instance.Remove(this);
    }

    private void Update()
    {
        GlasMaterial.SetFloat("_Absorption", absorption);
    }
}
