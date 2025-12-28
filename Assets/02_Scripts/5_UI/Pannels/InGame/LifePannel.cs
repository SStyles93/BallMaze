using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifePannel : MonoBehaviour
{
    [SerializeField] List<Image> lifeImages = new List<Image>();

    private void OnEnable()
    {
        if(LifeManager.Instance != null)
        LifeManager.Instance.OnRemoveLife += RemoveLife;
    }

    private void OnDisable()
    {
        if (LifeManager.Instance != null)
            LifeManager.Instance.OnRemoveLife -= RemoveLife;
    }

    private void Start()
    {
        LifeManager.Instance.SetLife();

        if(LifeManager.Instance != null)
            for (int i = 0; i < lifeImages.Count; i++)
            {
                if(i < LifeManager.Instance.CurrentLife)
                    lifeImages[i].enabled = true;
                else 
                    lifeImages[i].enabled = false;
            }
    }

    public void RemoveLife()
    {
        if (LifeManager.Instance != null)
            lifeImages[LifeManager.Instance.CurrentLife].enabled = false;
    }
}
