using System.Collections.Generic;
using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    [System.Serializable]
    public class RotatingObject 
    {
        public GameObject background;
        public float rotationSpeed;
    }

    [SerializeField] private float speedFactor = 20000;
    [SerializeField] private List<RotatingObject> starBackground = new List<RotatingObject>();

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < starBackground.Count; i++)
        {
            starBackground[i].background.transform.Rotate(Vector3.up, starBackground[i].rotationSpeed/speedFactor * Time.time);
        }
    }
}
