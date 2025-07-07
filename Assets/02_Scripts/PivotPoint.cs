using UnityEngine;

public class PivotPoint : MonoBehaviour
{
    [SerializeField] private Vector2 m_angleLimit = new Vector2(17.0f, 17.0f);
    [SerializeField] private float m_lerpSpeed = 5.0f;
    private Vector3 m_pivotAngle = Vector3.zero;



    private void OnEnable()
    {
        GravityManager.GravityChanged += UpdatePivotAngle;
    }

    private void OnDisable()
    {
        GravityManager.GravityChanged -= UpdatePivotAngle;
    }

    void Update()
    {
        //Limit the pivot angle
        LimitPivotAngle();
        
        //Assign pivot value to transform
        Vector3 rotation = new Vector3(m_pivotAngle.x, 0.0f, m_pivotAngle.z);
        
        //transform.rotation = Quaternion.Euler(rotation);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotation), Time.deltaTime * m_lerpSpeed);


    }

    private void UpdatePivotAngle(Vector3 pivotAngle)
    {
        Vector3 swap = new Vector3(pivotAngle.z, pivotAngle.y, -pivotAngle.x);
        m_pivotAngle = swap;
    }

    private void LimitPivotAngle()
    {
        //Limit the pivots rotation X at (-17 / 17);        
        m_pivotAngle.x = m_pivotAngle.x > m_angleLimit.x ? m_angleLimit.x : m_pivotAngle.x;
        m_pivotAngle.x = m_pivotAngle.x < -m_angleLimit.x ? -m_angleLimit.x : m_pivotAngle.x;
        //Limit the pivots rotation  Z at (-17 / 17);        
        m_pivotAngle.z = m_pivotAngle.z > m_angleLimit.y ? m_angleLimit.y : m_pivotAngle.z;
        m_pivotAngle.z = m_pivotAngle.z < -m_angleLimit.y ? -m_angleLimit.y : m_pivotAngle.z;
    }
}
