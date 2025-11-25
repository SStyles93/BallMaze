using UnityEngine;

public class PlayerRotator : MonoBehaviour
{
    [SerializeField] float m_rotationSpeed = -10.0f;
    void Update()
    {
        transform.Rotate(Vector3.up, m_rotationSpeed * Time.deltaTime);
    }
}
