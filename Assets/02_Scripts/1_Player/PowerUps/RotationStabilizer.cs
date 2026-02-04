using UnityEngine;

public class RotationStabilizer : MonoBehaviour
{
    [SerializeField] private Quaternion rotation;
    void Start()
    {
        rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.rotation != rotation)
        {
            transform.rotation = rotation;
        }
    }
}
