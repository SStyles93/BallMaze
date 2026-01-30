using UnityEngine;

[CreateAssetMenu(fileName = "Overlay Definition", menuName = "Procedural Generation/Tile Definition/Overlay")]
public class OverlayDefinition_SO : ScriptableObject
{
    [Header("Tile Parameters")]
    [SerializeField] public OverlayType overlayType;
    [SerializeField] public GameObject overlayPrefab;
    [SerializeField] public float yOffset;
    [Space(10)]
    [SerializeField] public Color overlayEditorColor;

}
