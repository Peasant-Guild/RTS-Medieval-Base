using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CheckArcherIsolation
{
    private const string ArcherVisualPrefabPath = "Assets/Outside Assets/Human Archer Visuals/Prefabs/HumanM_Archer.prefab";

    public static void Run()
    {
        string[] dependencies = AssetDatabase.GetDependencies(ArcherVisualPrefabPath, true)
            .OrderBy(path => path)
            .ToArray();

        string[] vendorDependencies = dependencies
            .Where(path => path.StartsWith("Assets/Kevin Iglesias/"))
            .ToArray();

        Debug.Log($"Archer dependency count: {dependencies.Length}");

        if (vendorDependencies.Length == 0)
        {
            Debug.Log("Archer isolation check passed. No Kevin Iglesias dependencies.");
            return;
        }

        Debug.LogError($"Archer isolation check failed. Vendor dependency count: {vendorDependencies.Length}");
        foreach (string vendorDependency in vendorDependencies)
        {
            Debug.LogError(vendorDependency);
        }
    }
}
