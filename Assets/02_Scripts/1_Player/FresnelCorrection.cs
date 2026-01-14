using UnityEngine;

public class FresnelCorrection : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponentInChildren<MeshRenderer>().material.SetFloat("_FesnelPower", 0.85f);
    }
}
