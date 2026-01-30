using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AimState", menuName = "Aim/AimStateChannel", order = 0)]
public class AimStateChannel : ScriptableObject
{
    [SerializeField] private bool debugLog = false;

    private bool isAiming;

    public bool IsAiming => isAiming;

    public event Action<bool> AimingChanged;

    public void SetAiming(bool value)
    {
        if (isAiming == value) return;

        isAiming = value;

        if (debugLog)
        {
            Debug.Log($"[AimStateChannel] IsAiming changed: {isAiming}");
        }

        AimingChanged?.Invoke(isAiming);
    }
}
