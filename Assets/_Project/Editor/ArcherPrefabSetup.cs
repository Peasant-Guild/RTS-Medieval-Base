using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class ArcherPrefabSetup
{
    private const string MarkerFilePath = "ArcherPrefabSetup.marker.txt";
    private const string UnitPrefabPath = "Assets/_Project/Prefabs/Unit.prefab";
    private const string TestArcherPrefabPath = "Assets/_Project/Prefabs/TestArcher.prefab";
    private const string UnitControllerPath = "Assets/_Project/Animation/UnitController.controller";
    private const string VendorHumanAnimationsRootPath = "Assets/Kevin Iglesias/Human Animations";
    private const string IsolatedArcherRootPath = "Assets/Outside Assets/Human Archer Visuals";
    private const string ArcherModelPath = "Assets/Outside Assets/Human Archer Animations FREE/Models/HumanM_Model.fbx";
    private const string VendorArcherVisualPrefabPath = "Assets/Kevin Iglesias/Human Animations/Unity Demo Scenes/Human Archer Animations/Prefabs/HumanM_Archer.prefab";
    private const string ArcherVisualPrefabPath = "Assets/Outside Assets/Human Archer Visuals/Prefabs/HumanM_Archer.prefab";
    private const string IdleClipPath = "Assets/Outside Assets/Human Archer Animations FREE/Animations/Male/Idles/HumanM@Idle01.fbx";
    private const string MoveClipPath = "Assets/Outside Assets/Human Archer Animations FREE/Animations/Male/Movement/Run/HumanM@Run01_Forward.fbx";
    private const string AttackClipPath = "Assets/Outside Assets/Human Archer Animations FREE/Animations/Male/Combat/Bow/HumanM@BowShot01 - Release.fbx";
    private const string TestArcherDefinitionPath = "Assets/_Project/ScriptableObjects/Units/TestArcher.asset";
    private const string ArcherBodyRendererName = "HumanM_BodyMesh";
    private const string VendorColorPaletteTexturePath = "Assets/Kevin Iglesias/Human Animations/Textures/HumanAnimations_ColorPalette.png";
    private const string IsolatedColorPaletteTexturePath = "Assets/Outside Assets/Human Archer Visuals/Textures/HumanAnimations_ColorPalette.png";

    public static void Run()
    {
        try
        {
            Log("Run started");
            EnsureFolders();
            Log("Folders ensured");
            EnsureIsolatedArcherVisualAssets();
            Log("Archer visual assets isolated");
            ConfigureSharedController();
            Log("Controller configured");
            EnsureUnitPrefabIsCompatible();
            Log("Unit prefab updated");
            CreateTestArcherPrefab();
            Log("TestArcher prefab created");
            CreateTestArcherDefinition();
            Log("TestArcher definition created");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Log("Run completed");
        }
        catch (System.Exception exception)
        {
            Log($"Run failed: {exception}");
            Debug.LogException(exception);
            throw;
        }
    }

    private static void EnsureFolders()
    {
        CreateFolderIfMissing("Assets/_Project", "Editor");
        CreateFolderIfMissing("Assets/_Project", "ScriptableObjects");
        CreateFolderIfMissing("Assets/_Project/ScriptableObjects", "Units");
        CreateFolderIfMissing("Assets/Outside Assets", "Human Archer Visuals");
        CreateFolderIfMissing(IsolatedArcherRootPath, "Materials");
        CreateFolderIfMissing(IsolatedArcherRootPath, "Models");
        CreateFolderIfMissing(IsolatedArcherRootPath, "Textures");
        CreateFolderIfMissing(IsolatedArcherRootPath, "Prefabs");
    }

    private static void CreateFolderIfMissing(string parent, string name)
    {
        string path = $"{parent}/{name}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static void EnsureParentFolders(string assetPath)
    {
        string directoryPath = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
        if (string.IsNullOrEmpty(directoryPath) || AssetDatabase.IsValidFolder(directoryPath))
        {
            return;
        }

        string[] parts = directoryPath.Split('/');
        string currentPath = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string nextPath = $"{currentPath}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, parts[i]);
            }

            currentPath = nextPath;
        }
    }

    private static void ConfigureSharedController()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(UnitControllerPath);
        AnimationClip idleClip = LoadClip(IdleClipPath);
        AnimationClip moveClip = LoadClip(MoveClipPath);
        AnimationClip attackClip = LoadClip(AttackClipPath);

        if (controller == null || idleClip == null || moveClip == null || attackClip == null)
        {
            throw new FileNotFoundException("Missing controller or imported archer clips.");
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        SetStateMotion(stateMachine, "Idle", idleClip);
        SetStateMotion(stateMachine, "Moving", moveClip);
        SetStateMotion(stateMachine, "Attack", attackClip);
        EditorUtility.SetDirty(controller);
    }

    private static void EnsureIsolatedArcherVisualAssets()
    {
        GameObject vendorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VendorArcherVisualPrefabPath);
        if (vendorPrefab == null)
        {
            throw new FileNotFoundException("Missing vendor archer visual prefab for isolation.");
        }

        string[] dependencies = AssetDatabase.GetDependencies(VendorArcherVisualPrefabPath, true)
            .Where(path => path.StartsWith(VendorHumanAnimationsRootPath))
            .Where(ShouldCopyArcherDependency)
            .ToArray();

        if (AssetDatabase.IsValidFolder(IsolatedArcherRootPath))
        {
            AssetDatabase.DeleteAsset(IsolatedArcherRootPath);
        }

        CreateFolderIfMissing("Assets/Outside Assets", "Human Archer Visuals");
        CreateFolderIfMissing(IsolatedArcherRootPath, "Materials");
        CreateFolderIfMissing(IsolatedArcherRootPath, "Models");
        CreateFolderIfMissing(IsolatedArcherRootPath, "Textures");
        CreateFolderIfMissing(IsolatedArcherRootPath, "Prefabs");

        foreach (string sourcePath in dependencies)
        {
            string targetPath = sourcePath.Replace(
                VendorHumanAnimationsRootPath,
                IsolatedArcherRootPath
            );

            EnsureParentFolders(targetPath);

            if (AssetDatabase.LoadAssetAtPath<Object>(targetPath) == null &&
                !AssetDatabase.CopyAsset(sourcePath, targetPath))
            {
                throw new IOException($"Failed to copy archer dependency from '{sourcePath}' to '{targetPath}'.");
            }
        }

        RemapCopiedMaterialAssetReferences();
        RemapCopiedMaterialGuidReferences();
        CreateBakedIsolatedArcherPrefab();
    }

    private static void EnsureUnitPrefabIsCompatible()
    {
        GameObject unitRoot = PrefabUtility.LoadPrefabContents(UnitPrefabPath);

        try
        {
            UnitStateController stateController = unitRoot.GetComponent<UnitStateController>();
            Animator animator = unitRoot.GetComponent<Animator>();

            if (stateController != null && animator != null)
            {
                SerializedObject stateControllerObject = new SerializedObject(stateController);
                stateControllerObject.FindProperty("_animator").objectReferenceValue = animator;
                stateControllerObject.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(unitRoot, UnitPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(unitRoot);
        }
    }

    private static void CreateTestArcherPrefab()
    {
        GameObject unitRoot = PrefabUtility.LoadPrefabContents(UnitPrefabPath);

        try
        {
            unitRoot.name = "TestArcher";

            RemoveObsoleteDefinitionApplier(unitRoot);

            MeshRenderer meshRenderer = unitRoot.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            MeshFilter meshFilter = unitRoot.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = null;
            }

            GameObject existingVisual = unitRoot.transform.Find("ArcherVisual")?.gameObject;
            if (existingVisual != null)
            {
                Object.DestroyImmediate(existingVisual);
            }

            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArcherVisualPrefabPath);
            GameObject visualRoot = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab);
            visualRoot.name = "ArcherVisual";
            visualRoot.transform.SetParent(unitRoot.transform, false);
            visualRoot.transform.localPosition = new Vector3(0f, -1f, 0f);
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one;

            RemoveVendorGameplayComponents(visualRoot);

            Animator animator = unitRoot.GetComponent<Animator>();
            if (animator != null)
            {
                animator.avatar = LoadAvatar();
            }

            Unit unit = unitRoot.GetComponent<Unit>();
            if (unit != null)
            {
                SerializedObject unitObject = new SerializedObject(unit);
                unitObject.FindProperty("_definition").objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnitDefinition>(TestArcherDefinitionPath);
                unitObject.ApplyModifiedPropertiesWithoutUndo();
            }

            TeamMember teamMember = unitRoot.GetComponent<TeamMember>();
            if (teamMember != null)
            {
                Renderer bodyRenderer = unitRoot.GetComponentsInChildren<Renderer>(true)
                    .FirstOrDefault(renderer => renderer != null && renderer.name == ArcherBodyRendererName);
                SerializedObject teamMemberObject = new SerializedObject(teamMember);
                SerializedProperty teamRenderersProperty = teamMemberObject.FindProperty("_teamRenderers");
                teamRenderersProperty.arraySize = bodyRenderer != null ? 1 : 0;

                if (bodyRenderer != null)
                {
                    teamRenderersProperty.GetArrayElementAtIndex(0).objectReferenceValue = bodyRenderer;
                }

                teamMemberObject.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(unitRoot, TestArcherPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(unitRoot);
        }
    }

    private static void CreateTestArcherDefinition()
    {
        UnitDefinition definition = AssetDatabase.LoadAssetAtPath<UnitDefinition>(TestArcherDefinitionPath);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<UnitDefinition>();
            AssetDatabase.CreateAsset(definition, TestArcherDefinitionPath);
        }

        SerializedObject definitionObject = new SerializedObject(definition);
        definitionObject.FindProperty("_prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(TestArcherPrefabPath);
        definitionObject.FindProperty("_animatorController").objectReferenceValue = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(UnitControllerPath);
        definitionObject.FindProperty("_maxHealth").floatValue = 8f;
        definitionObject.FindProperty("_moveSpeed").floatValue = 3.75f;
        definitionObject.FindProperty("_detectionRange").floatValue = 18f;
        definitionObject.FindProperty("_attackRange").floatValue = 9f;
        definitionObject.FindProperty("_attackDamage").floatValue = 3f;
        definitionObject.FindProperty("_attackInterval").floatValue = 1.1f;
        definitionObject.ApplyModifiedPropertiesWithoutUndo();

        GameObject archerRoot = PrefabUtility.LoadPrefabContents(TestArcherPrefabPath);

        try
        {
            Unit unit = archerRoot.GetComponent<Unit>();
            if (unit != null)
            {
                SerializedObject unitObject = new SerializedObject(unit);
                unitObject.FindProperty("_definition").objectReferenceValue = definition;
                unitObject.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(archerRoot, TestArcherPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(archerRoot);
        }
    }

    private static AnimationClip LoadClip(string path)
    {
        string clipName = Path.GetFileNameWithoutExtension(path);
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AnimationClip>()
            .FirstOrDefault(clip => clip.name == clipName);
    }

    private static Avatar LoadAvatar()
    {
        return AssetDatabase.LoadAllAssetsAtPath(ArcherModelPath).OfType<Avatar>().FirstOrDefault();
    }

    private static void RemoveVendorGameplayComponents(GameObject visualRoot)
    {
        Component[] components = visualRoot.GetComponentsInChildren<Component>(true);

        foreach (Component component in components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is Transform || component is Renderer || component is MeshFilter || component is SkinnedMeshRenderer)
            {
                continue;
            }

            if (component is Animator)
            {
                Object.DestroyImmediate(component);
                continue;
            }

            string componentTypeName = component.GetType().Name;
            if (componentTypeName.StartsWith("HumanArcher") || componentTypeName == "SpineProxy" || componentTypeName == "UpperBodyAnimationsProxy")
            {
                Object.DestroyImmediate(component);
            }
        }
    }

    private static void RemoveObsoleteDefinitionApplier(GameObject root)
    {
        Component[] obsoleteComponents = root.GetComponents<Component>()
            .Where(component => component != null && component.GetType().Name == "UnitDefinitionApplier")
            .ToArray();

        foreach (Component obsoleteComponent in obsoleteComponents)
        {
            Object.DestroyImmediate(obsoleteComponent);
        }
    }

    private static bool ShouldCopyArcherDependency(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return false;
        }

        if (path.EndsWith(".cs") || path.EndsWith(".mask") || path.EndsWith(".controller"))
        {
            return false;
        }

        if (path.Contains("/Animations/") || path.Contains("/Scripts/") || path.Contains("/AnimatorControllers/") || path.Contains("/Models/Avatar Masks/"))
        {
            return false;
        }

        return true;
    }

    private static void CreateBakedIsolatedArcherPrefab()
    {
        GameObject vendorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VendorArcherVisualPrefabPath);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(vendorPrefab);

        if (instance == null)
        {
            throw new FileNotFoundException("Failed to instantiate vendor archer visual prefab.");
        }

        try
        {
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            instance.name = "HumanM_Archer";

            RemoveVendorGameplayComponents(instance);
            RemapIsolatedVisualAssetReferences(instance);

            PrefabUtility.SaveAsPrefabAsset(instance, ArcherVisualPrefabPath);
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }

    private static void RemapCopiedMaterialAssetReferences()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { IsolatedArcherRootPath });

        foreach (string materialGuid in materialGuids)
        {
            string materialPath = AssetDatabase.GUIDToAssetPath(materialGuid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                continue;
            }

            bool changed = false;
            int propertyCount = ShaderUtil.GetPropertyCount(material.shader);

            for (int i = 0; i < propertyCount; i++)
            {
                if (ShaderUtil.GetPropertyType(material.shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    continue;
                }

                string propertyName = ShaderUtil.GetPropertyName(material.shader, i);
                Texture sourceTexture = material.GetTexture(propertyName);
                Texture remappedTexture = RemapTexture(sourceTexture);

                if (sourceTexture != remappedTexture)
                {
                    material.SetTexture(propertyName, remappedTexture);
                    changed = true;
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(material);
            }
        }
    }

    private static void RemapCopiedMaterialGuidReferences()
    {
        string vendorGuid = AssetDatabase.AssetPathToGUID(VendorColorPaletteTexturePath);
        string isolatedGuid = AssetDatabase.AssetPathToGUID(IsolatedColorPaletteTexturePath);

        if (string.IsNullOrEmpty(vendorGuid) || string.IsNullOrEmpty(isolatedGuid))
        {
            return;
        }

        string[] materialPaths = Directory.GetFiles(IsolatedArcherRootPath, "*.mat", SearchOption.AllDirectories)
            .Select(path => path.Replace("\\", "/"))
            .ToArray();

        foreach (string materialPath in materialPaths)
        {
            string contents = File.ReadAllText(materialPath);
            string updatedContents = Regex.Replace(contents, $@"guid:\s*{Regex.Escape(vendorGuid)}", $"guid: {isolatedGuid}");

            if (contents == updatedContents)
            {
                continue;
            }

            File.WriteAllText(materialPath, updatedContents);
            AssetDatabase.ImportAsset(materialPath, ImportAssetOptions.ForceUpdate);
        }
    }

    private static void RemapIsolatedVisualAssetReferences(GameObject root)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] remappedMaterials = renderer.sharedMaterials
                .Select(RemapMaterial)
                .ToArray();

            renderer.sharedMaterials = remappedMaterials;
        }

        foreach (MeshFilter meshFilter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            meshFilter.sharedMesh = RemapMesh(meshFilter.sharedMesh);
        }

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            skinnedMeshRenderer.sharedMesh = RemapMesh(skinnedMeshRenderer.sharedMesh);
        }
    }

    private static Material RemapMaterial(Material sourceMaterial)
    {
        if (sourceMaterial == null)
        {
            return null;
        }

        string sourcePath = AssetDatabase.GetAssetPath(sourceMaterial);
        if (string.IsNullOrEmpty(sourcePath) || !sourcePath.StartsWith(VendorHumanAnimationsRootPath))
        {
            return sourceMaterial;
        }

        string targetPath = sourcePath.Replace(VendorHumanAnimationsRootPath, IsolatedArcherRootPath);
        Material targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
        return targetMaterial != null ? targetMaterial : sourceMaterial;
    }

    private static Mesh RemapMesh(Mesh sourceMesh)
    {
        if (sourceMesh == null)
        {
            return null;
        }

        string sourcePath = AssetDatabase.GetAssetPath(sourceMesh);
        if (string.IsNullOrEmpty(sourcePath) || !sourcePath.StartsWith(VendorHumanAnimationsRootPath))
        {
            return sourceMesh;
        }

        string targetPath = sourcePath.Replace(VendorHumanAnimationsRootPath, IsolatedArcherRootPath);
        Mesh targetMesh = AssetDatabase.LoadAllAssetsAtPath(targetPath)
            .OfType<Mesh>()
            .FirstOrDefault(mesh => mesh.name == sourceMesh.name);

        return targetMesh != null ? targetMesh : sourceMesh;
    }

    private static Texture RemapTexture(Texture sourceTexture)
    {
        if (sourceTexture == null)
        {
            return null;
        }

        string sourcePath = AssetDatabase.GetAssetPath(sourceTexture);
        if (string.IsNullOrEmpty(sourcePath) || !sourcePath.StartsWith(VendorHumanAnimationsRootPath))
        {
            return sourceTexture;
        }

        string targetPath = sourcePath.Replace(VendorHumanAnimationsRootPath, IsolatedArcherRootPath);
        Texture targetTexture = AssetDatabase.LoadAssetAtPath<Texture>(targetPath);
        return targetTexture != null ? targetTexture : sourceTexture;
    }


    private static void SetStateMotion(AnimatorStateMachine stateMachine, string stateName, Motion motion)
    {
        ChildAnimatorState childState = stateMachine.states.FirstOrDefault(state => state.state.name == stateName);
        if (childState.state == null)
        {
            throw new FileNotFoundException($"Missing animator state '{stateName}' in shared controller.");
        }

        childState.state.motion = motion;
        EditorUtility.SetDirty(childState.state);
    }

    private static void Log(string message)
    {
        string line = $"[ArcherPrefabSetup] {message}";
        Debug.Log(line);
        File.AppendAllText(MarkerFilePath, line + System.Environment.NewLine);
    }
}
