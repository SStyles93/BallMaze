using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PxP.Tools
{
    public class UVMod_Data : ScriptableObject
    {
        // --- Mesh and UV Data ---
        public GameObject SelectedGameObject;
        public MeshFilter MeshFilter;
        public Mesh Mesh;
        public Vector2[] InitialUvs;
        public Vector2[] WorkingUvs;
        public Dictionary<int, Vector2> DragStartIslandUVs;

        // --- Transform States ---
        public Vector2 UvOffset = Vector2.zero;
        public Vector2 UvScale = Vector2.one;
        public Vector2 IslandTransformOffset = Vector2.zero;
        public Vector2 IslandTransformScale = Vector2.one;
        public Dictionary<int, Vector2> SelectionStartUVs;
        public Vector2 SelectionCenter;

        // --- UV Modification Settings ---
        public int SelectedUVChannel = 0;
        public int SelectedSubmeshIndex = -1;
        public Color SelectedVertexColor = Color.white;
        public bool UseVertexColorFilter = false;

        // --- UV Editor State ---
        public Texture2D UvTexturePreview;
        public List<List<int>> UvIslands;
        public List<int> SelectedUVIsslandIndices = new List<int>();

        // --- Interaction State ---
        public int ActiveUVHandle = -1;
        public Vector2 DragStartMousePos;
        public bool IsDraggingSelectionRect = false;
        public Rect SelectionRect;
        public Vector2 SelectionRectStartPos;
        public bool IsPickingColor = false;

        // --- Viewport Pan and Zoom State ---
        public float Zoom = 1.0f;
        public Vector2 ScrollPosition = Vector2.zero;
        public bool IsPanning = false;
        public Vector2 PanStartMousePos;
        public const float MinZoom = 0.1f;
        public const float MaxZoom = 20.0f;

        public void LoadDataFromSelection(GameObject newSelection)
        {
            if (Mesh != null && InitialUvs != null && WorkingUvs != null && !Enumerable.SequenceEqual(InitialUvs, WorkingUvs))
            {
                if (EditorUtility.DisplayDialog("Unsaved UV Changes",
                    "You have unsaved UV modifications. Would you like to revert them?", "Revert", "Keep"))
                {
                    ResetUVs();
                }
            }

            SelectedGameObject = newSelection;
            ResetData();
            InitializeData();
        }

        public void OverrideMesh()
        {
            if (EditorUtility.DisplayDialog("UV Data Override",
                    "You are about to save UV modifications done to the current object. Are you sure you want to apply changes", "Override", "Revert"))
            {
                InitializeData();
            }
        }

        public void ResetUVs()
        {
            if (InitialUvs == null || Mesh == null) return;
            WorkingUvs = (Vector2[])InitialUvs.Clone();
            Mesh.SetUVs(SelectedUVChannel, new List<Vector2>(WorkingUvs));
            EditorUtility.SetDirty(Mesh);
            UvOffset = Vector2.zero;
            UvScale = Vector2.one;
            SelectedUVIsslandIndices.Clear();
        }

        public void LoadUVsFromChannel(int channel, bool resetInitial = false)
        {
            if (Mesh == null) return;
            List<Vector2> tempUvs = new List<Vector2>();
            Mesh.GetUVs(channel, tempUvs);
            if (resetInitial)
            {
                InitialUvs = tempUvs.ToArray();
            }
            WorkingUvs = tempUvs.ToArray();
        }

        public void DetectUVIsslands()
        {
            UvIslands = new List<List<int>>();
            if (Mesh == null || WorkingUvs == null || WorkingUvs.Length == 0) return;

            var visited = new bool[Mesh.vertexCount];
            var adj = new Dictionary<int, List<int>>();
            for (int i = 0; i < Mesh.vertexCount; i++) adj[i] = new List<int>();

            for (int i = 0; i < Mesh.subMeshCount; i++)
            {
                int[] triangles = Mesh.GetTriangles(i);
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    int v1 = triangles[j], v2 = triangles[j + 1], v3 = triangles[j + 2];
                    adj[v1].Add(v2); adj[v1].Add(v3);
                    adj[v2].Add(v1); adj[v2].Add(v3);
                    adj[v3].Add(v1); adj[v3].Add(v2);
                }
            }

            for (int i = 0; i < Mesh.vertexCount; i++)
            {
                if (!visited[i])
                {
                    var island = new List<int>();
                    var q = new Queue<int>();
                    q.Enqueue(i);
                    visited[i] = true;
                    while (q.Count > 0)
                    {
                        int u = q.Dequeue();
                        island.Add(u);
                        foreach (int v in adj[u].Distinct())
                        {
                            if (!visited[v])
                            {
                                visited[v] = true;
                                q.Enqueue(v);
                            }
                        }
                    }
                    UvIslands.Add(island);
                }
            }
        }

        private void ResetData()
        {
            MeshFilter = null;
            Mesh = null;
            InitialUvs = null;
            WorkingUvs = null;
            UvOffset = Vector2.zero;
            UvScale = Vector2.one;
            SelectedUVChannel = 0;
            SelectedSubmeshIndex = -1;
            SelectedVertexColor = Color.white;
            UseVertexColorFilter = false;
            UvTexturePreview = null;
            ActiveUVHandle = -1;
            UvIslands = null;
            SelectedUVIsslandIndices = new List<int>();
            IsDraggingSelectionRect = false;
            IsPickingColor = false;
            Zoom = 1.0f;
            ScrollPosition = Vector2.zero;
        }

        private void InitializeData()
        {
            if (SelectedGameObject != null)
            {
                MeshFilter = SelectedGameObject.GetComponent<MeshFilter>();
                if (MeshFilter != null)
                {
                    Mesh = MeshFilter.sharedMesh;
                    if (Mesh != null)
                    {
                        LoadUVsFromChannel(SelectedUVChannel, true);
                        DetectUVIsslands();

                        Renderer renderer = SelectedGameObject.GetComponent<Renderer>();
                        if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture != null)
                        {
                            UvTexturePreview = renderer.sharedMaterial.mainTexture as Texture2D;
                        }
                    }
                }
            }
        }
    }
}
