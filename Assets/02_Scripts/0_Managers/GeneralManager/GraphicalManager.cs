using UnityEngine;

public class GraphicalManager : MonoBehaviour
{
    [SerializeField] private int m_targetFrameRate = 60;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = m_targetFrameRate;
        
    }
}
