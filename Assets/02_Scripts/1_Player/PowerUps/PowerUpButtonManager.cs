using System;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject rocketButton;
    [SerializeField] private GameObject ufoButton;
    [SerializeField] private Image powerUpBackground;

    [SerializeField] private PowerUpDisabledState states = PowerUpDisabledState.None;

    [Flags]
    private enum PowerUpDisabledState
    {
        None = 0,
        Rocket = 1 << 0,
        Ufo = 1 << 1,
        All = ~0
    }

    void Start()
    {
        if (CoinManager.Instance.GetCoinAmount(CoinType.ROCKET) < 1 && !TutorialManager.Instance.IsTutorialRocketComplete)
        {
            rocketButton.gameObject.SetActive(false);
            states |= PowerUpDisabledState.Rocket;
        }

        if (CoinManager.Instance.GetCoinAmount(CoinType.UFO) < 1 && !TutorialManager.Instance.IsTutorialUfoComplete)
        {
            ufoButton.gameObject.SetActive(false);
            states |= PowerUpDisabledState.Ufo;
        }

        if(states != PowerUpDisabledState.None && powerUpBackground != null)
        {
            powerUpBackground.enabled = false;
        }
    }
}
