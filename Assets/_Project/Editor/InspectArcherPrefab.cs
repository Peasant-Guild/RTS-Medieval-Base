using UnityEditor;
using UnityEngine;

public static class InspectArcherPrefab
{
    private const string ArcherVisualPrefabPath = "Assets/Outside Assets/Human Archer Visuals/Prefabs/HumanM_Archer.prefab";

    public static void Run()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherVisualPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("Archer visual prefab not found.");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(ArcherVisualPrefabPath);

        try
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                string materials = "";
                Material[] sharedMaterials = renderer.sharedMaterials;

                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    string materialName = sharedMaterials[i] != null ? sharedMaterials[i].name : "null";
                    materials += i == 0 ? materialName : $", {materialName}";
                }

                Debug.Log($"{renderer.GetType().Name} | {GetPath(renderer.transform)} | Materials: [{materials}]");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static string GetPath(Transform transform)
    {
        string path = transform.name;

        while (transform.parent != null)
        {
            transform = transform.parent;
            path = $"{transform.name}/{path}";
        }

        return path;
    }
}
