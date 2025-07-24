using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PxP.Tools
{
    public class UVMod_Data
    {
        // --- Mesh and UV Data ---
        public GameObject SelectedGameObject { get; private set; }
        public MeshFilter MeshFilter { get; private set; }
        public Mesh Mesh { get; private set; }
        public Vector2[] InitialUvs { get; private set; }
        public Vector2[] WorkingUvs { get; set; } // Public setter for modification
        public Dictionary<int, Vector2> DragStartIslandUVs { get; set; }

        // --- Transform States ---
        public Vector2 UvOffset { get; set; } = Vector2.zero;
        public Vector2 UvScale { get; set; } = Vector2.one;
        public Vector2 IslandTransformOffset { get; set; } = Vector2.zero;
        public Vector2 IslandTransformScale { get; set; } = Vector2.one;
        public Dictionary<int, Vector2> SelectionStartUVs { get; set; }
        public Vector2 SelectionCenter { get; set; }

        // --- UV Modification Settings ---
        public int SelectedUVChannel { get; set; } = 0;
        public int SelectedSubmeshIndex { get; set; } = -1;
        public Color SelectedVertexColor { get; set; } = Color.white;
        public bool UseVertexColorFilter { get; set; } = false;

        // --- UV Editor State ---
        public Texture2D UvTexturePreview { get; private set; }
        public List<List<int>> UvIslands { get; private set; }
        public List<int> SelectedUVIsslandIndices { get; set; } = new List<int>();

        // --- Interaction State ---
        public int ActiveUVHandle { get; set; } = -1;
        public Vector2 DragStartMousePos { get; set; }
        public bool IsDraggingSelectionRect { get; set; } = false;
        public Rect SelectionRect { get; set; }
        public Vector2 SelectionRectStartPos { get; set; }
        public bool IsPickingColor { get; set; } = false;

        // --- Viewport Pan and Zoom State ---
        public float Zoom { get; set; } = 1.0f;
        public Vector2 ScrollPosition { get; set; } = Vector2.zero;
        public bool IsPanning { get; set; } = false;
        public Vector2 PanStartMousePos { get; set; }
        public const float MinZoom = 0.1f;
        public const float MaxZoom = 20.0f;

        /// <summary>
        /// Loads all necessary data from the newly selected GameObject.
        /// </summary>
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

        /// <summary>
        /// Overrides the current's object mesh
        /// </summary>
        public void OverrideMesh()
        {
            if (EditorUtility.DisplayDialog("UV Data Override",
                    "You are about to save UV modifications done to the current object. Are you sure you want to apply changes", "Override", "Revert"))
            {
                ResetData();
                InitializeData();
            }
        }

        /// <summary>
        /// Resets the UVs of the current mesh.
        /// Current UVs = Initial UVs
        /// </summary>
        public void ResetUVs()
        {
            if (InitialUvs == null || Mesh == null) return;
            WorkingUvs = (Vector2[])InitialUvs.Clone();
            Mesh.SetUVs(SelectedUVChannel, new List<Vector2>(WorkingUvs));
            EditorUtility.SetDirty(Mesh);
            UvOffset = Vector2.zero;
            UvScale = Vector2.one;
            SelectedUVIsslandIndices = new List<int>();
        }

        /// <summary>
        /// Loads the UVs from the currently selected UV channel
        /// </summary>
        /// <param name="channel">The selected UV channel</param>
        /// <param name="resetInitial">When True:Sets the Initial UVs to the current ones</param>
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

        /// <summary>
        /// Algorithm to detect the different UV Islands
        /// </summary>
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

        /// <summary>
        /// Resets the current Data of the UVMod_Data
        /// </summary>
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

        /// <summary>
        /// Initializes the data of the currently selected object
        /// </summary>
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