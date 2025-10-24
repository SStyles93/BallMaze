using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifePannel : MonoBehaviour
{
    [SerializeField] List<Image> lifeImages = new List<Image>();

    private void OnEnable()
    {
        LifeManager.Instance.OnRemoveLife += RemoveLife;
    }

    private void OnDisable()
    {
        LifeManager.Instance.OnRemoveLife -= RemoveLife;
    }

    public void RemoveLife()
    {
        lifeImages[LifeManager.Instance.CurrentLife].enabled = false;
    }
}
