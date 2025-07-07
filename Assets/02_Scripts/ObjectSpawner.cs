using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ObjectSpawner : MonoBehaviour
{
    //private ScreenScaler screenScaler;

    [Header("Prefabs")]
    [SerializeField] GameObject m_ringPrefab;
    [SerializeField] GameObject m_pinPrefab;

    [SerializeField] Transform[] m_forceSrouces = new Transform[2];
    public Rect m_pinSpawnRect;


    [Header("Spawn Variables")]
    [SerializeField] int m_numberOfRings = 1;
    [SerializeField] int m_numberOfPins = 1;

    [Header("Tracked Objects")]
    [SerializeField] List<GameObject> m_rings;
    [SerializeField] List<GameObject> m_pins;


    [Header("Debug")]
    [SerializeField] bool EnableDebugTools = false;
    [ConditionalField("EnableDebugTools")][SerializeField] Color m_gizmoRingColor = Color.yellow;
    [ConditionalField("EnableDebugTools")][SerializeField][Range(0.01f, 0.7f)] float m_ringDispersion = 0.7f;
    [ConditionalField("EnableDebugTools")][SerializeField] Color m_gizmoPinColor = Color.magenta;
    //[ConditionalField("EnableDebugTools")] [SerializeField] [Range(0.01f, 0.4f)] float m_pinDispersion = 0.2f;

    private Vector3[] m_ringSpawnPositions = new Vector3[2];

    // Start is called before the first frame update
    void Awake()
    {
        //screenScaler = GetComponent<ScreenScaler>();
        PositionRingSpawns();
        //PositionPinSpawnRect();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        if (!EnableDebugTools) return;

        Gizmos.color = m_gizmoRingColor;
        if (m_forceSrouces.Length != 0)
        {
            for (int i = 0; i < m_forceSrouces.Length; i++)
            {
                if (m_forceSrouces[i] == null) continue;
                Vector3 tmpVec = m_forceSrouces[i].position;
                tmpVec.y += 1.0f;
                m_ringSpawnPositions[i] = tmpVec;
                Gizmos.DrawSphere(m_ringSpawnPositions[i], m_ringDispersion);
            }
        }

        Gizmos.color = m_gizmoPinColor;
        Gizmos.DrawWireCube(m_pinSpawnRect.center, m_pinSpawnRect.size);
    }

    /// <summary>
    /// SpawnRings at a random position whithin m_ringSpawnPositions
    /// </summary>
    /// <returns>List of rings (m_rings)</returns>
    public List<GameObject> SpawnRings()
    {
        for (int i = 0; i < m_numberOfRings; i++)
        {
            int randomSpawnSide = Random.Range(0, m_ringSpawnPositions.Length);

            Vector3 randomSpawnPos = m_ringSpawnPositions[randomSpawnSide];
            randomSpawnPos.x = Random.Range(randomSpawnPos.x - m_ringDispersion, randomSpawnPos.x + m_ringDispersion);
            randomSpawnPos.y = Random.Range(randomSpawnPos.y - m_ringDispersion, randomSpawnPos.y + m_ringDispersion);
            m_rings.Add(Instantiate(m_ringPrefab, randomSpawnPos, Quaternion.identity, gameObject.transform));
        }
        return m_rings;
    }

    private void PositionRingSpawns()
    {
        for (int i = 0; i < m_forceSrouces.Length; i++)
        {
            Vector3 tmpVec = m_forceSrouces[i].position;
            //tmpVec.y += screenScaler.ScaleFloat(1.5f);
            m_ringSpawnPositions[i] = tmpVec;
        }
    }

    public List<GameObject> SpawnPins()
    {
        for (int i = 0; i < m_numberOfPins; i++)
        {
            int attempts = 0;
            //Set randPos
            Vector3 randomSpawnPos = Vector3.zero;
            randomSpawnPos.x = Random.Range(m_pinSpawnRect.xMin, m_pinSpawnRect.xMax);
            randomSpawnPos.y = Random.Range(m_pinSpawnRect.yMin, m_pinSpawnRect.yMax);

            m_pins.Add(Instantiate(m_pinPrefab, randomSpawnPos, Quaternion.identity, gameObject.transform));

            for (int j = 0; j < m_pins.Count - 1; j++)
            {
                while (m_pins[i].GetComponent<CapsuleCollider>().bounds.Intersects(m_pins[j].GetComponent<CapsuleCollider>().bounds))
                {
                    if (attempts > 200)
                    {
                        Debug.Log("Too many attemps at placing pin " + (i + 1));
                        break;
                    }
                    randomSpawnPos.x = Random.Range(m_pinSpawnRect.xMin, m_pinSpawnRect.xMax);
                    randomSpawnPos.y = Random.Range(m_pinSpawnRect.yMin, m_pinSpawnRect.yMax);
                    m_pins[i].transform.position = randomSpawnPos;
                    attempts++;
                }
            }
        }
        return m_pins;
    }

    ///// <summary>
    ///// Positions the PinSpawnRect according to the screen scale
    ///// </summary>
    //private void PositionPinSpawnRect()
    //{
    //    m_pinSpawnRect.xMin = screenScaler.ScaleFloat(m_pinSpawnRect.xMin);
    //    m_pinSpawnRect.yMin = screenScaler.ScaleFloat(m_pinSpawnRect.yMin);
    //    m_pinSpawnRect.xMax = screenScaler.ScaleFloat(m_pinSpawnRect.xMax);
    //    m_pinSpawnRect.yMax = screenScaler.ScaleFloat(m_pinSpawnRect.yMax);
    //}
}
