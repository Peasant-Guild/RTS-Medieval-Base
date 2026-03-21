using UnityEngine;
using UnityEngine.InputSystem;

public class DebugToggleController : MonoBehaviour
{
    [SerializeField] private Key _toggleKey = Key.F3;

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current[_toggleKey].wasPressedThisFrame)
        {
            CycleDebugMode();
        }
    }

    private void CycleDebugMode()
    {
        DebugSettings.CurrentMode = DebugSettings.CurrentMode switch
        {
            DebugSettings.UnitDebugMode.Off => DebugSettings.UnitDebugMode.SelectedOnly,
            DebugSettings.UnitDebugMode.SelectedOnly => DebugSettings.UnitDebugMode.AllUnits,
            DebugSettings.UnitDebugMode.AllUnits => DebugSettings.UnitDebugMode.SingleUnit,
            DebugSettings.UnitDebugMode.SingleUnit => DebugSettings.UnitDebugMode.Off,
            _ => DebugSettings.UnitDebugMode.Off
        };

        Debug.Log($"Unit debug mode: {DebugSettings.CurrentMode}");
    }
}