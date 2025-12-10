using UnityEngine;

public class StarAnimationManager : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] GameObject objectToRotate = null;
    [SerializeField] private float rotationSpeed = -20.0f;

    [Header("Levitation")]
    [SerializeField] GameObject objectToLevitate = null;
    [SerializeField] private float maxFloatingHeight = 2.0f;
    [SerializeField] private float sinePeriod = 3.0f;

    private Vector3 initialPosition;
    private Vector3 currentPosition;

    private void Start()
    {
        if (objectToLevitate == null) Debug.LogWarning("Star Graphics was not assigned !");
        initialPosition = objectToLevitate.gameObject.transform.position;
    }

    void Update()
    {
        RotateTransform(Vector3.up, rotationSpeed);
        Levitate();
    }

    private void Levitate()
    {
        currentPosition = objectToLevitate.transform.position;
        currentPosition.y = SineWave.SineWaveEffect(sinePeriod, initialPosition.y, maxFloatingHeight);
        objectToLevitate.transform.position = currentPosition;
    }

    private void RotateTransform(Vector3 rotationAxis, float speed)
    {
        objectToRotate.transform.Rotate(rotationAxis, speed * Time.deltaTime);
    }
}
