using System;
using UnityEngine;

public static class GameSignals
{
    public static event Action<Vector3, Vector3> FireRequested;

    public static void RaiseFireRequested(Vector3 pos, Vector3 dir)
    {
        FireRequested?.Invoke(pos, dir);
    }
}