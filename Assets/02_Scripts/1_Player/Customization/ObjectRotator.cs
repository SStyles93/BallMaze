using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField] float m_rotationSpeed = -10.0f;
    [SerializeField] Vector3 rotationAxis = Vector3.up;

    void Update()
    {
        transform.Rotate(rotationAxis, m_rotationSpeed * Time.deltaTime);
    }
}
