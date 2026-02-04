using System;
using UnityEngine;

public class PowerUpDistanceChecker : MonoBehaviour
{
    public static event Action<bool> OnPowerUpBlocked;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") ||
            other.gameObject.CompareTag("Rocket") ||
            other.gameObject.CompareTag("Ufo"))
        {
            OnPowerUpBlocked?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") ||
            other.gameObject.CompareTag("Rocket") ||
            other.gameObject.CompareTag("Ufo"))
        {
            OnPowerUpBlocked?.Invoke(false);
        }
    }
}
