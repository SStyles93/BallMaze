using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifePannel : MonoBehaviour
{
    [SerializeField] List<Image> lifeImages = new List<Image>();

    private void OnEnable()
    {
        if (LifeManager.Instance == null) return;
        LifeManager.Instance.OnLifeRemoved += RemoveLife;
        LifeManager.Instance.OnLifeIncreased += IncreaseLife;
    }

    private void OnDisable()
    {
        if (LifeManager.Instance == null) return;
        LifeManager.Instance.OnLifeRemoved -= RemoveLife;
        LifeManager.Instance.OnLifeIncreased -= IncreaseLife;
    }

    private void Start()
    {
        LifeManager.Instance.ResetLife();

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

    public void IncreaseLife()
    {
        if (LifeManager.Instance != null)
            lifeImages[LifeManager.Instance.CurrentLife].enabled = true;
    }
}
