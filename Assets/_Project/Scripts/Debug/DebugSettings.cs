using UnityEngine;

public static class DebugSettings
{
    public enum UnitDebugMode
    {
        Off,
        SelectedOnly,
        AllUnits,
        SingleUnit
    }

    public static UnitDebugMode CurrentMode = UnitDebugMode.Off;
    public static GameObject SingleDebugTarget = null;
}