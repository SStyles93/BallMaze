using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.SceneManagement;

namespace PxP.Tools
{

    [ExecuteAlways]
    public class ObjectPlacement : MonoBehaviour
    {
        [SerializeField] Vector3 initialPosition = Vector3.zero;
        [SerializeField]
        Vector2 spacingOffset = new Vector2(0.5f, 0.5f);
        [SerializeField][Min(0.0f)] float boundExpansion = 0.0f;
        [SerializeField] Vector2Int desiredColumnsAndRows = Vector2Int.zero;


        [SerializeField] List<GameObject> m_objects = new List<GameObject>();
        [SerializeField] bool m_autoAssignMaterials = false;
        [SerializeField] Material[] m_materials = new Material[2];

        [SerializeField] bool DrawObjectBounds = false;

        [Space(10)]
        [SerializeField] PlacementAxes placementAxis = PlacementAxes.XmZp;
        [Space(10)]

        [SerializeField][HideInInspector] List<GameObject> m_placedObjects = new List<GameObject>();
        [HideInInspector] Vector3 m_beginningPosition = Vector3.zero;
        [HideInInspector] List<ObjectBounds> m_objectBounds = new List<ObjectBounds>();

        [System.Serializable]
        public class ObjectBounds
        {
            public ObjectBounds(Bounds bounds)
            {
                Bounds = bounds;
                Color = Color.cyan;
            }

            public Bounds Bounds;
            public Color Color;
        }


        public enum PlacementAxes
        {
            XpZp,
            XpZm,
            XpYp,
            XpYm,
            XmZp,
            XmZm,
            XmYp,
            XmYm
        }

        void Update()
        {
            m_beginningPosition = transform.localPosition;
            m_beginningPosition += initialPosition;
            if (spacingOffset.x < 0.01f)
            {
                spacingOffset.x = 0.01f;
                UnityEngine.Debug.Log("Spacing Offset X must not be under 0.1. Under that value, the script will throw an infinite loop");
            }
            if (spacingOffset.y < 0.01f)
            {
                spacingOffset.y = 0.01f;
                UnityEngine.Debug.Log("Spacing Offset Y must not be under 0.1. Under that value, the script will throw an infinite loop");
            }

            if (m_objects.Count > (desiredColumnsAndRows.x * desiredColumnsAndRows.y))
            {
                int tmpCol = desiredColumnsAndRows.x;
                int tmpRow = desiredColumnsAndRows.y;

                while (m_objects.Count > (desiredColumnsAndRows.x * desiredColumnsAndRows.y))
                {
                    if (desiredColumnsAndRows.y < 1)
                        desiredColumnsAndRows.y = 1;
                    desiredColumnsAndRows.x += 1;
                }
                UnityEngine.Debug.LogWarning(
                    $"The number of objects to place [{m_objects.Count}]" +
                    $" is supperior to the number of Columns [{tmpCol}] x Rows[{tmpRow}]: [{tmpCol * tmpRow}]\n" +
                    $"Updated Places [{m_objects.Count}/{desiredColumnsAndRows.x * desiredColumnsAndRows.y}]\n" +
                    $"New values: Col [{desiredColumnsAndRows.x}], Row [{desiredColumnsAndRows.y}]");
            }
        }

        #region Public Methods
        /// <summary>
        /// Places the objects contained in the object list in the scene and spaces them according to parameters
        /// </summary>
        public void PlaceObjectsInScene()
        {
            //Init size of array (ref to placed objs.
            m_placedObjects.Capacity = m_objects.Count + m_placedObjects.Count;

            int desiredCol = 0;
            int desiredRow = 0;

            m_objectBounds.Clear();
            string placementString = placementAxis.ToString();
            for (int i = 0; i < m_objects.Count; i++)
            {
                bool hasIntersected = false;

                //Place object on a temporary position
                #region Temporary Position
                Vector3 currentObjectPosition = m_beginningPosition;
                switch (placementString.Substring(0, 2))
                {
                    case "Xp":
                        currentObjectPosition += new Vector3(desiredCol * spacingOffset.x, 0, 0);
                        break;

                    case "Xm":
                        currentObjectPosition += new Vector3(-(desiredCol * spacingOffset.x), 0, 0);
                        break;
                }
                switch (placementString.Substring(2, 2))
                {
                    case "Yp":
                        currentObjectPosition += new Vector3(0, desiredRow * spacingOffset.y, 0);
                        break;

                    case "Ym":
                        currentObjectPosition += new Vector3(0, -(desiredRow * spacingOffset.y), 0);
                        break;

                    case "Zp":
                        currentObjectPosition += new Vector3(0, 0, desiredRow * spacingOffset.y);
                        break;

                    case "Zm":
                        currentObjectPosition += new Vector3(0, 0, -(desiredRow * spacingOffset.y));
                        break;

                }
                #endregion

                //Instantiate obj assign materials and keep ref.
                #region Object Initialization
                GameObject currentObject = Instantiate(m_objects[i], currentObjectPosition, Quaternion.identity, transform);
                currentObject.name = m_objects[i].name;

                //Assign MeshRenderer to currentObject
                foreach (MeshRenderer mr in currentObject.GetComponentsInChildren<MeshRenderer>())
                {
                    //Resize the scripts material array if there are more materials on an object than the number previously defined
                    if (mr.sharedMaterials.Length > m_materials.Length)
                    {
                        m_materials = new Material[mr.sharedMaterials.Length];
                    }

                    //Assign materials for objects
                    if (m_autoAssignMaterials)
                    {
                        if (m_materials.Length > 2)
                            GetMaterialInFolder(2, "PXPUniversal_Emissive");
                        if (m_materials.Length > 1)
                            GetMaterialInFolder(1, "PXPUniversal_Transparent");
                        if (m_materials.Length > 0)
                            GetMaterialInFolder(0, "PXPUniversal");

                        Material[] sharedMats = mr.sharedMaterials;
                        for (int matIndex = 0; matIndex < sharedMats.Length; matIndex++)
                        {
                            sharedMats[matIndex] = m_materials[matIndex];
                        }
                        mr.sharedMaterials = sharedMats;
                    }
                }

                //Gets the combines bound of the current obj.
                ObjectBounds currentObjectBound = new ObjectBounds(GetCombinedBoundOfObject(currentObject));
                #endregion

                //Check if object intersects with ground and reposition
                #region Ground Position Check
                switch (placementString.Substring(2, 2))
                {
                    case "Ym":
                        //Reposition object under initial position
                        if (currentObjectBound.Bounds.max.y > initialPosition.y)
                            currentObjectPosition.y -= Mathf.Abs(initialPosition.y - currentObjectBound.Bounds.max.y);
                        break;

                    default:
                        //Ensure item is not under ground
                        if (currentObjectBound.Bounds.min.y < initialPosition.y)
                            currentObjectPosition.y += Mathf.Abs(initialPosition.y - currentObjectBound.Bounds.min.y);
                        break;
                }
                #endregion

                //Check if obj. bounds intersects with previously placed objects' bounds
                #region Collision Check
                for (int j = 0; j < m_objectBounds.Count; j++)
                {
                    //Bounds of previously placed objects
                    Bounds EvaluatedBound = m_objectBounds[j].Bounds;
                    //Expand for spaced placement
                    EvaluatedBound.Expand(boundExpansion);
                    //Check intersection
                    while (currentObjectBound.Bounds.Intersects(EvaluatedBound) || EvaluatedBound.Intersects(currentObjectBound.Bounds))
                    {
                        if (i <= desiredColumnsAndRows.x - 1)
                        {
                            MoveObjectAllongRow(placementString, ref currentObjectPosition);
                            hasIntersected = true;
                        }
                        else
                        {
                            //Placed object (i) minus 1 equals the index of previous object (j-1)
                            if (i - 1 == j)
                            {
                                MoveObjectAllongRow(placementString, ref currentObjectPosition);
                                hasIntersected = true;
                                currentObjectBound.Color = Color.green;
                            }
                            //Placed object (i) with object at the previous column
                            else if (i - desiredColumnsAndRows.x == j)
                            {
                                MoveObjectAllongCollumn(placementString, ref currentObjectPosition);
                                hasIntersected = true;
                                currentObjectBound.Color = Color.yellow;
                            }
                            //Placed object with any other object
                            else
                            {
                                MoveObjectAllongRow(placementString, ref currentObjectPosition);
                                hasIntersected = true;
                                currentObjectBound.Color = Color.red;
                            }
                        }

                        currentObject.transform.position = currentObjectPosition;
                        currentObjectBound.Bounds = GetCombinedBoundOfObject(currentObject);


                    }
                    //Restart the check to insure obj doesn't intersect with any previously checked objs
                    if (hasIntersected)
                    {
                        hasIntersected = false;
                        j = 0;
                        continue;
                    }
                    //                    j--;
                }
                #endregion

                //Update obj. Position, Bounds and Col / Row position
                #region Increment
                currentObject.transform.position = currentObjectPosition;
                currentObjectBound.Bounds = GetCombinedBoundOfObject(currentObject);

                m_objectBounds.Add(currentObjectBound);
                m_placedObjects.Add(currentObject);

                if (desiredCol < desiredColumnsAndRows.x - 1)
                {
                    desiredCol++;
                }
                //If the column value is met, reset Col and increment Row
                else
                {
                    desiredRow++;
                    desiredCol = 0;
                }
                #endregion
            }
        }

        /// <summary>
        /// Removes all the objects in the scene
        /// </summary>
        public void RemoveObjectsFromScene()
        {
            foreach (GameObject obj in m_placedObjects)
            {
                DestroyImmediate(obj);
            }
            m_placedObjects.Clear();
            m_objectBounds.Clear();
        }

        /// <summary>
        /// Sorts the m_objects list according to the volume of combined meshes
        /// </summary>
        public void SortObjectByBBoxVolume()
        {
            m_objects = m_objects.OrderByDescending(go => GetCombinedMeshBoundVolume(go)).ToList();
            //m_objects = m_objects.OrderBy(go => go.name).ToList();
        }

        /// <summary>
        /// Reconnects the prefabs to their variant in the asset folder
        /// </summary>
        public void ReconnectPrefabToVariant()
        {
            foreach (GameObject go in m_placedObjects)
            {
                string[] guids1 = UnityEditor.AssetDatabase.FindAssets(go.name);

                foreach (string guid in guids1)
                {
                    string currentAssetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    if (currentAssetPath.Contains(".fbx")) continue;
                    //string folderPath = AssetDatabase.GetAssetPath(m_objects[0]);
                    string folderPath = SceneManager.GetActiveScene().path;
                    string firstPart = folderPath.Substring(0, folderPath.IndexOf('/') + 1);
                    string tmp = folderPath.Substring(folderPath.IndexOf('/') + 1);
                    firstPart = firstPart + tmp.Substring(0, tmp.IndexOf('/') + 1);

                    if (!currentAssetPath.Contains(firstPart)) continue;

                    var asset = UnityEditor.AssetDatabase.LoadMainAssetAtPath(currentAssetPath);
                    UnityEditor.ConvertToPrefabInstanceSettings settings = new UnityEditor.ConvertToPrefabInstanceSettings();
                    UnityEditor.PrefabUtility.ConvertToPrefabInstance(go, (GameObject)asset, settings, UnityEditor.InteractionMode.AutomatedAction);
                    break;
                }
            }
        }

        /// <summary>
        /// Fills the list of placed objects
        /// </summary>
        public void GetPlacedObjects()
        {
            m_placedObjects.Clear();
            m_objects.Clear();
            foreach (Transform child in transform)
            {
                m_placedObjects.Add(child.gameObject);
                m_objects.Add(child.gameObject);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the global bound of an object
        /// </summary>
        /// <param name="currentObject">The object for which we want the combined bounds</param>
        /// <returns>The combined bounds of the given object</returns>
        private Bounds GetCombinedBoundOfObject(GameObject currentObject)
        {
            MeshRenderer firstRenderer = currentObject.GetComponentInChildren<MeshRenderer>();
            Bounds objectBound = firstRenderer.bounds;
            foreach (MeshRenderer meshRenderer in currentObject.GetComponentsInChildren<MeshRenderer>())
            {
                if (meshRenderer != firstRenderer)
                {
                    objectBound.Encapsulate(meshRenderer.bounds);
                }
            }
            return objectBound;
        }

        /// <summary>
        /// Gets the volume of the combined meshes
        /// </summary>
        /// <param name="gameObject">The GameObject from which the meshes are combined and evaluated </param>
        /// <returns>The volume of the combined meshes</returns>
        private float GetCombinedMeshBoundVolume(GameObject gameObject)
        {
            MeshFilter firstFilter = gameObject.GetComponentInChildren<MeshFilter>();
            Bounds objectBound = firstFilter.sharedMesh.bounds;
            foreach (MeshFilter meshFilter in gameObject.GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter != firstFilter)
                {
                    objectBound.Encapsulate(meshFilter.sharedMesh.bounds);
                }
            }
            return objectBound.size.x * objectBound.size.y * objectBound.size.z;
        }

        /// <summary>
        /// Assigns the given material name to the materials at the given index
        /// </summary>
        /// <param name="materialIndex">Index of the material</param>
        /// <param name="materialName">Name of the material to assign</param>
        private void GetMaterialInFolder(int materialIndex, string materialName)
        {
            //Get default lit if no material is assigned
            if (m_materials[materialIndex] == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets(materialName);
                string currentAssetPath = "";
                Material tmpMat = null;
                foreach (string guid in guids)
                {
                    currentAssetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    if (!currentAssetPath.Contains(".mat")) continue;
                    tmpMat = (Material)UnityEditor.AssetDatabase.LoadMainAssetAtPath(currentAssetPath);
                    break;
                }
                //Fallback on default "Lit" material if mat not found
                if (tmpMat == null)
                {
                    m_materials[materialIndex] =
                        (Material)UnityEditor.AssetDatabase.
                        LoadMainAssetAtPath("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat");
                    UnityEngine.Debug.Log("Material " + materialName + " was not found in folder");
                }
                else
                {
                    m_materials[materialIndex] = tmpMat;
                }
            }
        }

        /// <summary>
        /// Moves the placement of an obj. in it's row according to the Type of placement
        /// </summary>
        /// <param name="placementString">Selected PlacementAxis in a string format</param>
        /// <param name="currentObjectPosition">Reference to the object's current position</param>
        private void MoveObjectAllongRow(string placementString, ref Vector3 currentObjectPosition)
        {
            switch (placementString.Substring(0, 2))
            {
                case "Xp":
                    //Object is at LEFT of the currentObject, move on X axis
                    currentObjectPosition.x += spacingOffset.x / 10;
                    break;

                case "Xm":
                    //Object is at RIGHT of the currentObject, move on X axis
                    currentObjectPosition.x -= spacingOffset.x / 10;
                    break;
            }
        }
        /// <summary>
        /// Moves the placement of an obj. in it's collumn according to the Type of placement
        /// </summary>
        /// <param name="placementString">Selected PlacementAxis in a string format</param>
        /// <param name="currentObjectPosition">Reference to the object's current position</param>
        private void MoveObjectAllongCollumn(string placementString, ref Vector3 currentObjectPosition)
        {
            switch (placementString.Substring(2, 2))
            {
                case "Yp":
                    currentObjectPosition.y += spacingOffset.y / 10;
                    break;

                case "Ym":
                    currentObjectPosition.y -= spacingOffset.y / 10;
                    break;

                case "Zp":
                    currentObjectPosition.z += spacingOffset.y / 10;
                    break;

                case "Zm":
                    currentObjectPosition.z -= spacingOffset.y / 10;
                    break;
            }
        }

        #endregion

        #region Gizmos
        public void OnDrawGizmos()
        {
            if (DrawObjectBounds)
            {

                foreach (ObjectBounds ob in m_objectBounds)
                {
                    Gizmos.color = ob.Color;
                    Gizmos.DrawWireCube(ob.Bounds.center, ob.Bounds.size);
                }
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_beginningPosition, 0.1f);
        }
        #endregion
    }

    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(ObjectPlacement))]
    public class ObjectPlacementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ObjectPlacement _target = (ObjectPlacement)target;

            GUILayout.Space(20);

            if (GUILayout.Button("Sort Objects in List"))
            {
                if (EditorUtility.DisplayDialog("SORT OBJECTS ?",
                    "Click on \"Sort Objects\" if you wish to sort the objects in the list according to mesh bounds size",
                    "Sort Objects", "Cancel"))
                {
                    _target.SortObjectByBBoxVolume();
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Place Objects"))
            {
                _target.PlaceObjectsInScene();
            }

            if (GUILayout.Button("Remove Objects in Scene"))
            {
                if (EditorUtility.DisplayDialog("REMOVE OBJECTS ?",
                    "Click on \"Remove Objects\" if you wish to delete the objects previously placed on the scene",
                    "Remove Objects", "Cancel"))
                {
                    _target.RemoveObjectsFromScene();
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Reconnect Objects to prefabs"))
            {
                _target.ReconnectPrefabToVariant();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Get placed objects"))
            {
                _target.GetPlacedObjects();
            }
        }
    }
#endif
    #endregion
}