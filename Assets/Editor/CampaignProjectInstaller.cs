#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using TINS.Campaign;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Builds the campaign catalog, configures the mission, creates the walkable Base, and groups scene objects.</summary>
public static class CampaignProjectInstaller
{
    private const string MissionPath = "Assets/Scenes/HR Floor one.unity";
    private const string BasePath = "Assets/Scenes/Campaign/CampaignBase.unity";
    private const string CatalogPath = "Assets/TINS - New Mechanics/Campaign/Resources/CampaignCatalog.asset";
    private const string DarknessMaterialPath = "Assets/TINS - New Mechanics/Campaign/Materials/Discrete Darkness Bands.mat";
    private const string MapOceanPath = "Assets/TINS - New Mechanics/Campaign/Resources/PlanetMap/Planet map - ocean.png";
    private const string MapCorruptedPath = "Assets/TINS - New Mechanics/Campaign/Resources/PlanetMap/Planet map - corrupted.png";
    private const string MapPurifiedPath = "Assets/TINS - New Mechanics/Campaign/Resources/PlanetMap/Planet map - purified.png";
    private const string MapCircleMaskPath = "Assets/TINS - New Mechanics/Campaign/Resources/PlanetMap/Planet purification circle.png";

    [MenuItem("Tools/There Is No Sun/Configure Campaign Scenes")]
    public static void InstallFromMenu() => Debug.Log(Install());

    public static string Install()
    {
        EnsureFolder("Assets/TINS - New Mechanics/Campaign");
        EnsureFolder("Assets/TINS - New Mechanics/Campaign/Materials");
        EnsureFolder("Assets/TINS - New Mechanics/Campaign/Resources");
        EnsureFolder("Assets/TINS - New Mechanics/Campaign/Resources/CampaignGuns");
        EnsureFolder("Assets/TINS - New Mechanics/Campaign/Textures");
        EnsureFolder("Assets/Scenes/Campaign");

        AssetDatabase.Refresh();
        PreparePixelMapSprite(MapOceanPath);
        PreparePixelMapSprite(MapCorruptedPath);
        PreparePixelMapSprite(MapPurifiedPath);
        EnsureCircleMaskSprite();

        CampaignCatalog catalog = EnsureCatalog();
        Material darknessMaterial = EnsureDarknessMaterial();
        CopyWeaponAsset("Assets/Prefabs/Guns & bullets/Fire & laser guns/Laser pistol/laser pistol_GunData.asset",
            "Assets/TINS - New Mechanics/Campaign/Resources/CampaignGuns/laser pistol_GunData.asset");
        CopyWeaponAsset("Assets/Prefabs/Guns & bullets/Impact Guns/automatic pistol/Automatic pistol_GunData.asset",
            "Assets/TINS - New Mechanics/Campaign/Resources/CampaignGuns/automatic pistol_GunData.asset");

        Scene mission = EditorSceneManager.OpenScene(MissionPath, OpenSceneMode.Single);
        SetupMission(mission, catalog, darknessMaterial);
        EditorSceneManager.SaveScene(mission);

        if (!File.Exists(BasePath))
        {
            if (!AssetDatabase.CopyAsset("Assets/Scenes/The Void - HUB.unity", BasePath))
                throw new System.InvalidOperationException("Could not create the Base scene from the current mission scene.");
            AssetDatabase.Refresh();
        }

        Scene baseScene = EditorSceneManager.OpenScene(BasePath, OpenSceneMode.Single);
        SetupBase(baseScene, catalog);
        if (!EditorSceneManager.SaveScene(baseScene))
            throw new System.InvalidOperationException("Unity could not save CampaignBase.");
        if (catalog.skills.Any(skill => skill != null)) CampaignSkillTreeSceneSetup.Build();
        ConfigureBuildScenes();

        EditorSceneManager.OpenScene(BasePath, OpenSceneMode.Single);
        MoveCampaignScriptsIntoRuntimeFolder();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return "Installed campaign catalog, walkable Base, mission darkness, UI, and hierarchy. Scenes: CampaignBase and HR Floor one.";
    }

    private static CampaignCatalog EnsureCatalog()
    {
        CampaignCatalog catalog = AssetDatabase.LoadAssetAtPath<CampaignCatalog>(CatalogPath);
        if (catalog == null)
        {
            if (File.Exists(CatalogPath)) AssetDatabase.DeleteAsset(CatalogPath);
            catalog = ScriptableObject.CreateInstance<CampaignCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }
        if (catalog.skills != null)
        {
            foreach (CampaignSkill skill in catalog.skills)
            {
                if (skill == null) continue;
                if (skill.id == "weapon-laser-pistol") skill.unlockedWeaponResourcePath = "CampaignGuns/laser pistol_GunData";
                if (skill.id == "weapon-auto-pistol") skill.unlockedWeaponResourcePath = "CampaignGuns/automatic pistol_GunData";
            }
        }
        if (catalog.planets != null)
        {
            foreach (CampaignPlanet planet in catalog.planets)
            {
                if (planet == null) continue;
                switch (planet.id)
                {
                    case "velaria": planet.enemySpeedMultiplier = .70f; break;
                    case "nivora": planet.enemySpeedMultiplier = .72f; break;
                    case "asterim": planet.enemySpeedMultiplier = .74f; break;
                    case "dorsalia": planet.enemySpeedMultiplier = .76f; break;
                    case "varkora": planet.enemySpeedMultiplier = .78f; break;
                    case "marevia": planet.enemySpeedMultiplier = .80f; break;
                }
            }
        }
        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    private static Material EnsureDarknessMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(DarknessMaterialPath);
        if (material != null) return material;
        Shader shader = Shader.Find("TINS/Campaign/Discrete Darkness Bands");
        if (shader == null) throw new System.InvalidOperationException("The discrete darkness shader was not imported.");
        material = new Material(shader) { name = "Discrete Darkness Bands" };
        AssetDatabase.CreateAsset(material, DarknessMaterialPath);
        return material;
    }

    private static void CopyWeaponAsset(string source, string destination)
    {
        if (AssetDatabase.LoadAssetAtPath<GunData>(destination) != null) return;
        if (AssetDatabase.LoadAssetAtPath<GunData>(source) == null)
        {
            Debug.LogWarning("Campaign weapon asset not found: " + source);
            return;
        }
        AssetDatabase.CopyAsset(source, destination);
    }

    private static void SetupMission(Scene scene, CampaignCatalog catalog, Material material)
    {
        Transform systems = OrganizeHierarchy(scene);
        foreach (FSGameManager legacy in FindAll<FSGameManager>(scene)) legacy.gameObject.SetActive(false);
        PlayerHolder holder = FindFirst<PlayerHolder>(scene);
        PlayerInputHandler missionInput = FindAll<PlayerInputHandler>(scene).FirstOrDefault(input =>
            input.gameObject.activeInHierarchy && input.GetComponentInParent<Health>() != null && !input.GetIsClone());
        if (missionInput == null && holder != null) missionInput = holder.GetPlayerInputHandler();
        if (missionInput == null) throw new System.InvalidOperationException("Mission player is missing.");
        GameObject player = missionInput.gameObject;
        if (!player.TryGetComponent(out BlueCrystalInventory souls)) souls = player.AddComponent<BlueCrystalInventory>();
        if (!player.TryGetComponent(out BeamMeleeUsesManager charges)) charges = player.AddComponent<BeamMeleeUsesManager>();
        if (!player.TryGetComponent(out BeamMeleeAttack flash)) flash = player.AddComponent<BeamMeleeAttack>();
        SetObject(flash, "playerInput", missionInput);
        SetObject(flash, "usesManager", charges);
        SetObject(flash, "beamOrigin", player.transform);
        EnemySpawnManager spawner = FindFirst<EnemySpawnManager>(scene);
        if (spawner == null) throw new System.InvalidOperationException("Mission spawner is missing.");
        SetObject(spawner, "enemyPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Demoninho/Demoninho no path finding.prefab"));
        SetObject(spawner, "elitePrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Demoninho fire/Demoninho fire.prefab"));
        foreach (var check in FindAll<GraveyardShift.Gameplay.SkillCheckBar>(scene))
        {
            var serializedCheck = new SerializedObject(check);
            serializedCheck.FindProperty("startOnAwake").boolValue = false;
            serializedCheck.ApplyModifiedPropertiesWithoutUndo();
        }
        CampaignMissionRuntime runtime = FindFirst<CampaignMissionRuntime>(scene);
        GameObject managerObject = runtime != null ? runtime.gameObject : new GameObject("Campaign Mission Manager");
        if (runtime == null) runtime = managerObject.AddComponent<CampaignMissionRuntime>();
        managerObject.transform.SetParent(systems, false);

        TowerController tower = FindFirst<TowerController>(scene);
        TowerCrystalDeposit deposit = FindFirst<TowerCrystalDeposit>(scene);
        PurificationReveal reveal = FindFirst<PurificationReveal>(scene);
        Health playerHealth = player.GetComponentInParent<Health>() ?? holder?.GetPlayerHealth();
        BlueCrystalInventory inventory = FindFirst<BlueCrystalInventory>(scene);
        GunInventory gunInventory = player.GetComponentInChildren<GunInventory>(true) ?? holder?.GetGunInventory() ?? FindFirst<GunInventory>(scene);
        EnsureStartingPistol startingPistol = FindFirst<EnsureStartingPistol>(scene);
        if (startingPistol == null && gunInventory != null)
            startingPistol = gunInventory.gameObject.AddComponent<EnsureStartingPistol>();
        GunData simplePistol = AssetDatabase.LoadAssetAtPath<GunData>("Assets/TINS - New Mechanics/Campaign/Resources/CampaignGuns/simple Pistol_GunData.asset");
        if (startingPistol != null)
        {
            SetObject(startingPistol, "gunInventory", gunInventory);
            SetObject(startingPistol, "startingPistol", simplePistol);
        }
        if (tower == null || deposit == null || reveal == null || playerHealth == null || gunInventory == null)
            throw new System.InvalidOperationException($"Mission references: tower={tower}, deposit={deposit}, reveal={reveal}, health={playerHealth}, guns={gunInventory}, input={missionInput}.");
        Canvas canvas = CreateCampaignCanvas(systems);
        if (canvas != null) canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        EnsureEventSystem(scene);
        TMP_Text missionText = FindOrCreateText(canvas, "Campaign Mission Title", new Vector2(34f, -90f), new Vector2(660f, 38f), 25f, TextAlignmentOptions.TopLeft);
        TMP_Text objectiveText = FindOrCreateText(canvas, "Campaign Mission Objective", new Vector2(34f, -132f), new Vector2(660f, 48f), 18f, TextAlignmentOptions.TopLeft);
        SetObject(runtime, "catalog", catalog);
        SetObject(runtime, "tower", tower);
        SetObject(runtime, "deposit", deposit);
        SetObject(runtime, "lightReveal", reveal);
        SetObject(runtime, "playerHealth", playerHealth);
        SetObject(runtime, "soulInventory", inventory);
        CampaignFlashHUD flashHud = FindFirst<CampaignFlashHUD>(scene);
        if (flashHud == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TINS - New Mechanics/UI/Campaign Flash HUD.prefab");
            if (prefab == null) throw new System.InvalidOperationException("Campaign Flash HUD prefab is missing.");
            var flashObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            flashHud = flashObject.GetComponent<CampaignFlashHUD>();
        }
        SetObject(runtime, "flashHud", flashHud);
        CampaignTowerHealthHUD towerHealthHud = FindFirst<CampaignTowerHealthHUD>(scene);
        if (towerHealthHud == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TINS - New Mechanics/UI/Campaign Tower Health HUD.prefab");
            if (prefab == null) throw new System.InvalidOperationException("Campaign Tower Health HUD prefab is missing.");
            var hudObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            towerHealthHud = hudObject.GetComponent<CampaignTowerHealthHUD>();
        }
        SetObject(runtime, "towerHealthHud", towerHealthHud);
        SetObject(runtime, "missionLabel", missionText);
        SetObject(runtime, "objectiveLabel", objectiveText);

        GameObject overlayObject = FindInScene(scene, "Campaign Darkness Overlay");
        if (overlayObject == null)
        {
            overlayObject = new GameObject("Campaign Darkness Overlay", typeof(SpriteRenderer));
            overlayObject.transform.SetParent(FindOrCreateGroup(scene, "03_ENVIRONMENT"), false);
        }
        SpriteRenderer renderer = overlayObject.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = short.MaxValue;
        renderer.maskInteraction = SpriteMaskInteraction.None;
        CampaignDarknessBands darkness = overlayObject.GetComponent<CampaignDarknessBands>();
        if (darkness == null) darkness = overlayObject.AddComponent<CampaignDarknessBands>();
        SetObject(darkness, "lightReveal", reveal);
        SetObject(darkness, "player", playerHealth != null ? playerHealth.transform : null);
        SetObject(darkness, "playerBody", playerHealth != null ? playerHealth.GetComponent<Rigidbody2D>() : null);
        SetObject(darkness, "targetCamera", Camera.main);
        SetObject(darkness, "darknessMaterial", material);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static Canvas CreateCampaignCanvas(Transform parent)
    {
        Transform existing = parent.Find("Campaign Screen Canvas");
        if (existing != null) return existing.GetComponent<Canvas>();
        var root = new GameObject("Campaign Screen Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(parent, false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 900);
        return canvas;
    }

    private static void SetupBase(Scene scene, CampaignCatalog catalog)
    {
        RemoveNamed(scene, "Enemy Spawn Manager", "Enemy Spawn Point", "Spawn Points", "FSGameManager", "LightArea", "Orbital Upgrade", "Campaign Mission Manager", "Campaign Darkness Overlay");
        foreach (TowerController tower in FindAll<TowerController>(scene)) Object.DestroyImmediate(tower);
        foreach (TowerCrystalDeposit deposit in FindAll<TowerCrystalDeposit>(scene)) Object.DestroyImmediate(deposit);
        foreach (PurificationReveal reveal in FindAll<PurificationReveal>(scene)) Object.DestroyImmediate(reveal);
        foreach (CampaignMissionRuntime mission in FindAll<CampaignMissionRuntime>(scene)) Object.DestroyImmediate(mission);

        Transform systems = OrganizeHierarchy(scene);
        Canvas canvas = CreateCampaignCanvas(systems);
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(systems, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        else canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        EnsureEventSystem(scene);

        foreach (HubManager legacyHub in FindAll<HubManager>(scene)) legacyHub.enabled = false;
        var holder = FindFirst<PlayerHolder>(scene);
        PlayerInputHandler input = holder != null ? holder.GetPlayerInputHandler() : null;
        if (input == null) input = FindAll<PlayerInputHandler>(scene).FirstOrDefault(p => p.gameObject.activeInHierarchy && !p.GetIsClone());
        PlayerMovement movement = input != null ? input.GetComponent<PlayerMovement>() : null;
        if (movement == null) movement = FindFirst<PlayerMovement>(scene);
        Transform center = FindInScene(scene, "TowerComGrafico")?.transform;
        Vector3 centerPosition = center != null ? center.position : Vector3.zero;
        if (input != null) centerPosition = input.transform.position;

        CampaignBaseController controller = FindFirst<CampaignBaseController>(scene);
        if (controller == null)
        {
            GameObject controllerObject = new GameObject("Campaign Base Controller");
            controllerObject.transform.SetParent(systems, false);
            controller = controllerObject.AddComponent<CampaignBaseController>();
        }

        GameObject missionPanel = FindInScene(scene, "Campaign Mission Board");
        GameObject skillPanel = FindInScene(scene, "Campaign Skill Tree");
        Button[] planetButtons = new Button[0];
        TMP_Text[] planetLabels = new TMP_Text[0];
        CampaignPurificationMapMask[] planetPurificationMasks = new CampaignPurificationMapMask[0];
        RectTransform planetMap = null;
        Button[] skillButtons = new Button[0];
        TMP_Text[] skillLabels = new TMP_Text[0];
        Button resetButton = null;
        Button closeMission = null;
        Button closeSkill = null;
        TMP_Text soulsLabel = null;
        TMP_Text resourceLabel = null;
        TMP_Text statusLabel = null;

        GameObject profileBar = FindInScene(scene, "Campaign Base Profile Bar");
        if (profileBar == null)
        {
            profileBar = new GameObject("Campaign Base Profile Bar", typeof(RectTransform), typeof(Image));
            profileBar.transform.SetParent(canvas.transform, false);
            RectTransform profileRect = profileBar.GetComponent<RectTransform>();
            profileRect.anchorMin = profileRect.anchorMax = new Vector2(0.5f, 1f);
            profileRect.pivot = new Vector2(0.5f, 1f);
            profileRect.anchoredPosition = new Vector2(0f, -8f);
            profileRect.sizeDelta = new Vector2(760f, 82f);
            profileBar.GetComponent<Image>().color = new Color(0.02f, 0.025f, 0.04f, 0.78f);
        }
        soulsLabel = FindOrCreateChildText(profileBar.transform, "Souls Label", "ALMAS  0", new Vector2(-190f, -20f), new Vector2(330f, 28f), 18f, TextAlignmentOptions.Left);
        resourceLabel = FindOrCreateChildText(profileBar.transform, "Resources Label", "MATERIAIS  —", new Vector2(205f, -20f), new Vector2(360f, 28f), 16f, TextAlignmentOptions.Right);
        statusLabel = FindOrCreateChildText(profileBar.transform, "Campaign Status", "", new Vector2(0f, -56f), new Vector2(700f, 24f), 14f, TextAlignmentOptions.Center);

        if (missionPanel == null || skillPanel == null)
        {
            missionPanel = CreatePanel(canvas.transform, "Campaign Mission Board", new Vector2(880f, 600f));
            skillPanel = CreatePanel(canvas.transform, "Campaign Skill Tree", new Vector2(840f, 760f));
            CreateText(missionPanel.transform, "Mission Panel Title", "MAPA DE EXPEDIÇÕES", new Vector2(0f, 258f), new Vector2(740f, 48f), 30f, TextAlignmentOptions.Center);
            planetMap = CreatePanel(missionPanel.transform, "Planet Map", new Vector2(740f, 405f)).GetComponent<RectTransform>();
            planetMap.anchoredPosition = new Vector2(0f, -40f);
            planetMap.GetComponent<Image>().color = new Color(0.035f, 0.10f, 0.16f, 0.96f);
            planetButtons = new Button[catalog.planets.Length];
            planetLabels = new TMP_Text[catalog.planets.Length];
            for (int i = 0; i < planetButtons.Length; i++)
            {
                Button button = CreateButton(planetMap, "Planet " + catalog.planets[i].id, new Vector2(105f, 56f));
                planetButtons[i] = button;
                planetLabels[i] = button.GetComponentInChildren<TMP_Text>();
            }
            closeMission = CreateButton(missionPanel.transform, "Close Mission Board", new Vector2(150f, 42f));
            closeMission.GetComponentInChildren<TMP_Text>().text = "FECHAR";
            closeMission.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -280f);

            CreateText(skillPanel.transform, "Skill Panel Title", "ÁRVORE DE HABILIDADES", new Vector2(0f, 330f), new Vector2(700f, 48f), 30f, TextAlignmentOptions.Center);
            skillButtons = new Button[catalog.skills.Length];
            skillLabels = new TMP_Text[catalog.skills.Length];
            for (int i = 0; i < skillButtons.Length; i++)
            {
                Button button = CreateButton(skillPanel.transform, "Skill " + catalog.skills[i].id, new Vector2(720f, 58f));
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 220f - i * 62f);
                skillButtons[i] = button;
                skillLabels[i] = button.GetComponentInChildren<TMP_Text>();
                skillLabels[i].alignment = TextAlignmentOptions.Left;
                skillLabels[i].fontSize = 17f;
                skillLabels[i].margin = new Vector4(14f, 5f, 12f, 5f);
            }
            resetButton = CreateButton(skillPanel.transform, "Reset Campaign Upgrades", new Vector2(220f, 42f));
            resetButton.GetComponentInChildren<TMP_Text>().text = "REINICIAR MELHORIAS";
            resetButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-145f, -340f);
            closeSkill = CreateButton(skillPanel.transform, "Close Skill Tree", new Vector2(140f, 42f));
            closeSkill.GetComponentInChildren<TMP_Text>().text = "FECHAR";
            closeSkill.GetComponent<RectTransform>().anchoredPosition = new Vector2(190f, -340f);
            missionPanel.SetActive(false);
            skillPanel.SetActive(false);
        }
        else
        {
            planetMap = missionPanel.transform.Find("Planet Map") as RectTransform;
            planetButtons = GetNamedButtons(planetPanel: missionPanel, prefix: "Planet ");
            planetLabels = GetButtonLabels(planetButtons);
            closeMission = FindNamedButton(missionPanel, "Close Mission Board");
            skillButtons = GetNamedButtons(skillPanel, "Skill ");
            skillLabels = GetButtonLabels(skillButtons);
            resetButton = FindNamedButton(skillPanel, "Reset Campaign Upgrades");
            closeSkill = FindNamedButton(skillPanel, "Close Skill Tree");
        }

        planetMap = missionPanel.GetComponentsInChildren<RectTransform>(true).FirstOrDefault(rect => rect.name == "Planet Map");
        if (planetMap == null) throw new System.InvalidOperationException("The Base mission board has no Planet Map.");
        planetMap.sizeDelta = new Vector2(740f, 405f);
        planetButtons = catalog.planets.Select(planet =>
            planetMap.Find("Planet " + planet.id).GetComponent<Button>()).ToArray();
        planetLabels = GetButtonLabels(planetButtons);
        planetPurificationMasks = EnsurePlanetMapLayers(planetMap, catalog);

        var originalTree = FindFirst<ShowSkillTreeManager>(scene);
        if (originalTree != null)
        {
            Button originalButton = FindInScene(scene, "Open Original Skill Tree")?.GetComponent<Button>();
            if (originalButton == null) originalButton = CreateButton(skillPanel.transform, "Open Original Skill Tree", new Vector2(245f, 42f));
            originalButton.GetComponentInChildren<TMP_Text>().text = "ÁRVORE ORIGINAL";
            originalButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -280f);
            SetObject(controller, "originalSkillTree", originalTree);
            SetObject(controller, "originalSkillTreeButton", originalButton);
        }
        SetObject(controller, "catalog", catalog);
        SetObject(controller, "playerInput", input);
        SetObject(controller, "playerMovement", movement);
        SetObject(controller, "missionPanel", missionPanel);
        SetObject(controller, "skillPanel", skillPanel);
        SetObject(controller, "soulsLabel", soulsLabel);
        SetObject(controller, "resourceLabel", resourceLabel);
        SetObject(controller, "statusLabel", statusLabel);
        SetObjectArray(controller, "planetButtons", planetButtons);
        SetObjectArray(controller, "planetLabels", planetLabels);
        SetObjectArray(controller, "planetPurificationMasks", planetPurificationMasks);
        SetObject(controller, "planetMap", planetMap);
        SetObjectArray(controller, "skillButtons", skillButtons);
        SetObjectArray(controller, "skillLabels", skillLabels);
        SetObject(controller, "resetSkillsButton", resetButton);
        SetObject(controller, "closeButtonsMission", closeMission);
        SetObject(controller, "closeButtonsSkill", closeSkill);

        Transform stations = FindOrCreateGroup(scene, "04_STATIONS");
        CreateStation(scene, stations, "Mission Board Station", "QUADRO DE MISSÕES", BaseStationType.MissionBoard,
            centerPosition + new Vector3(-2f, 1f, 0f), new Color(0.92f, 0.65f, 0.18f, 1f), controller);
        CreateStation(scene, stations, "Skill Tree Station", "ÁRVORE DE HABILIDADES", BaseStationType.SkillTree,
            centerPosition + new Vector3(2f, 1f, 0f), new Color(0.24f, 0.76f, 0.86f, 1f), controller);

        TMP_Text welcome = FindOrCreateText(canvas, "Campaign Base Instructions", new Vector2(0f, -18f), new Vector2(900f, 32f), 19f, TextAlignmentOptions.Center);
        if (welcome != null) welcome.text = "EXPLORE A BASE  ·  APROXIME-SE DAS ESTAÇÕES E INTERAJA";
        welcome.color = new Color(0.94f, 0.9f, 0.74f, 1f);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void CreateStation(Scene scene, Transform parent, string objectName, string label, BaseStationType type, Vector3 position, Color color, CampaignBaseController controller)
    {
        GameObject station = FindInScene(scene, objectName);
        if (station == null) station = new GameObject(objectName);
        station.transform.SetParent(parent, false);
        station.transform.position = position;
        SpriteRenderer renderer = station.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = station.AddComponent<SpriteRenderer>();
        if (renderer.sprite == null) renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = color;
        renderer.sortingOrder = 20;
        CircleCollider2D trigger = station.GetComponent<CircleCollider2D>();
        if (trigger == null) trigger = station.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 1.1f;

        GameObject textObject = FindInScene(scene, objectName + " Prompt");
        if (textObject == null)
        {
            textObject = new GameObject(objectName + " Prompt", typeof(TextMeshPro));
            textObject.transform.SetParent(station.transform, false);
        }
        textObject.transform.localPosition = new Vector3(0f, 1.45f, 0f);
        TextMeshPro text = textObject.GetComponent<TextMeshPro>();
        text.text = label;
        text.fontSize = 2.7f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;


        BaseStationInteractable interactable = station.GetComponent<BaseStationInteractable>();
        if (interactable == null) interactable = station.AddComponent<BaseStationInteractable>();
        SetEnum(interactable, "stationType", (int)type);
        SetObject(interactable, "baseController", controller);
        SetObject(interactable, "prompt", text);
    }

    private static CampaignPurificationMapMask[] EnsurePlanetMapLayers(RectTransform mapRect, CampaignCatalog catalog)
    {
        if (mapRect == null || catalog == null || catalog.planets == null) return new CampaignPurificationMapMask[0];
        Image panel = mapRect.GetComponent<Image>();
        if (panel != null) panel.color = Color.white;

        Sprite ocean = AssetDatabase.LoadAssetAtPath<Sprite>(MapOceanPath);
        Sprite corrupted = AssetDatabase.LoadAssetAtPath<Sprite>(MapCorruptedPath);
        Sprite purified = AssetDatabase.LoadAssetAtPath<Sprite>(MapPurifiedPath);
        Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MapCircleMaskPath);

        if (ocean == null || corrupted == null || purified == null || circleSprite == null)
            throw new System.InvalidOperationException("One or more map sprites were not imported.");
        EnsureMapImage(mapRect, "Map Ocean Background", ocean, 0);
        EnsureMapImage(mapRect, "Map Corrupted Overlay", corrupted, 1);

        CampaignPurificationMapMask[] masks = new CampaignPurificationMapMask[catalog.planets.Length];
        for (int i = 0; i < catalog.planets.Length; i++)
        {
            CampaignPlanet planet = catalog.planets[i];
            if (planet == null) continue;
            string objectName = "Purified Map Circle - " + planet.id;
            Transform existing = mapRect.Find(objectName);
            GameObject circle = existing != null ? existing.gameObject : new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Mask));
            if (existing == null) circle.transform.SetParent(mapRect, false);

            RectTransform circleRect = circle.GetComponent<RectTransform>();
            Vector2 anchor = new Vector2(planet.mapX, planet.mapY);
            circleRect.anchorMin = circleRect.anchorMax = anchor;
            circleRect.pivot = new Vector2(0.5f, 0.5f);
            circleRect.anchoredPosition = Vector2.zero;
            circleRect.sizeDelta = Vector2.zero;

            Image maskImage = circle.GetComponent<Image>();
            maskImage.sprite = circleSprite;
            maskImage.color = Color.white;
            maskImage.raycastTarget = false;
            Mask mask = circle.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            Image greenImage = EnsureMapImage(circle.transform, "Green Map Texture", purified, 0);
            RectTransform greenRect = greenImage.GetComponent<RectTransform>();
            greenRect.anchorMin = greenRect.anchorMax = new Vector2(0.5f, 0.5f);
            greenRect.pivot = new Vector2(0.5f, 0.5f);
            greenRect.sizeDelta = mapRect.rect.size;
            greenRect.anchoredPosition = -Vector2.Scale(anchor - new Vector2(0.5f, 0.5f), mapRect.rect.size);

            CampaignPurificationMapMask progressMask = circle.GetComponent<CampaignPurificationMapMask>();
            if (progressMask == null) progressMask = circle.AddComponent<CampaignPurificationMapMask>();
            SetObject(progressMask, "mapRect", mapRect);
            SetObject(progressMask, "maskRect", circleRect);
            SetFloat(progressMask, "maximumRadiusFraction", 0.13f);
            progressMask.SetProgress(0f);
            circle.transform.SetSiblingIndex(2 + i);
            masks[i] = progressMask;
        }
        if (masks.Any(mask => mask == null)) throw new System.InvalidOperationException("The map contains an unconfigured planet mask.");
        return masks;
    }

    private static Image EnsureMapImage(Transform parent, string name, Sprite sprite, int siblingIndex)
    {
        Transform existing = parent.Find(name);
        GameObject layer = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
        if (existing == null) layer.transform.SetParent(parent, false);
        RectTransform rect = layer.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        Image image = layer.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        layer.transform.SetSiblingIndex(siblingIndex);
        return image;
    }

    private static void PreparePixelMapSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (importer.spritePixelsPerUnit != 16f) { importer.spritePixelsPerUnit = 16f; changed = true; }
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
        if (changed) importer.SaveAndReimport();
    }

    private static void EnsureCircleMaskSprite()
    {
        if (File.Exists(MapCircleMaskPath))
        {
            PreparePixelMapSprite(MapCircleMaskPath);
            return;
        }
        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[32 * 32];
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
            {
                float dx = x - 15.5f;
                float dy = y - 15.5f;
                pixels[y * 32 + x] = dx * dx + dy * dy <= 15.5f * 15.5f
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(255, 255, 255, 0);
            }
        texture.SetPixels32(pixels);
        texture.Apply();
        File.WriteAllBytes(MapCircleMaskPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(MapCircleMaskPath, ImportAssetOptions.ForceSynchronousImport);
        PreparePixelMapSprite(MapCircleMaskPath);
    }

    private static Transform OrganizeHierarchy(Scene scene)
    {
        Transform systems = FindOrCreateGroup(scene, "00_SYSTEMS");
        Transform actors = FindOrCreateGroup(scene, "01_ACTORS");
        Transform defense = FindOrCreateGroup(scene, "02_DEFENSE");
        Transform environment = FindOrCreateGroup(scene, "03_ENVIRONMENT");
        GameObject oldInterface = FindInScene(scene, "05_INTERFACE");
        if (oldInterface != null)
        {
            for (int i = oldInterface.transform.childCount - 1; i >= 0; i--)
                oldInterface.transform.GetChild(i).SetParent(systems, true);
            Object.DestroyImmediate(oldInterface);
        }
        HashSet<string> systemRoots = new HashSet<string> { "MANAGER & CANVAS", "FSGameManager", "TempAudio", "Orbital Upgrade", "Campaign Mission Manager", "Campaign Base Controller" };
        HashSet<string> actorRoots = new HashSet<string> { "Player_PREFAB", "PlayerMouseBeam", "Enemy Spawn Manager", "Enemy Spawn Point", "Spawn Points" };
        HashSet<string> defenseRoots = new HashSet<string> { "TowerComGrafico", "LightArea" };
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            if (root == null || root.transform == systems || root.transform == actors || root.transform == defense || root.transform == environment ||
                root.name == "04_STATIONS") continue;
            Transform destination = systemRoots.Contains(root.name) ? systems : actorRoots.Contains(root.name) ? actors : defenseRoots.Contains(root.name) ? defense : environment;
            root.transform.SetParent(destination, true);
        }

        return systems;
    }

    private static Transform FindOrCreateGroup(Scene scene, string name)
    {
        GameObject group = FindInScene(scene, name);
        if (group != null) return group.transform;
        group = new GameObject(name);
        SceneManager.MoveGameObjectToScene(group, scene);
        return group.transform;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = new Color(0.025f, 0.035f, 0.055f, 0.97f);
        return panel;
    }

    private static Button CreateButton(Transform parent, string name, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.25f, 0.22f, 0.18f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        TMP_Text label = CreateText(buttonObject.transform, name + " Label", name, Vector2.zero, size - new Vector2(12f, 8f), 16f, TextAlignmentOptions.Center);
        label.raycastTarget = false;
        return button;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = value;
        text.font = GetDefaultFont();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static TMP_Text FindOrCreateChildText(Transform parent, string name, string value, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(name);
        TMP_Text text = existing != null ? existing.GetComponent<TMP_Text>() : null;
        if (text == null) text = CreateText(parent, name, value, position, size, fontSize, alignment);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        text.text = value;
        text.font = GetDefaultFont();
        text.fontSize = fontSize;
        text.alignment = alignment;
        return text;
    }

    private static TMP_Text FindOrCreateText(Canvas canvas, string name, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        if (canvas == null) return null;
        Transform existing = canvas.transform.Find(name);
        TMP_Text text = existing != null ? existing.GetComponent<TMP_Text>() : null;
        if (text == null) text = CreateText(canvas.transform, name, "", position, size, fontSize, alignment);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        text.font = GetDefaultFont();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static TMP_FontAsset GetDefaultFont()
    {
        if (TMP_Settings.defaultFontAsset != null) return TMP_Settings.defaultFontAsset;
        string[] ids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        if (ids.Length > 0) return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(ids[0]));
        return null;
    }

    private static void EnsureEventSystem(Scene scene)
    {
        if (FindAll<EventSystem>(scene).Any(system => system.isActiveAndEnabled)) return;
        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(eventSystem, scene);
        eventSystem.transform.SetParent(FindOrCreateGroup(scene, "00_SYSTEMS"), false);
    }

    private static void ConfigureBuildScenes()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(BasePath, true),
            new EditorBuildSettingsScene(MissionPath, true)
        };
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.enabled && scene.path != BasePath && scene.path != MissionPath) scenes.Add(scene);
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void MoveCampaignScriptsIntoRuntimeFolder()
    {
        string[] scripts =
        {
            "Assets/TINS - New Mechanics/TowerCrystalDeposit.cs", "Assets/TINS - New Mechanics/BlueCrystalInventory.cs", "Assets/TINS - New Mechanics/BlueCrystalPickup.cs",
            "Assets/TINS - New Mechanics/BeamMeleeAttack.cs", "Assets/TINS - New Mechanics/BeamMeleeUsesManager.cs", "Assets/TINS - New Mechanics/BeamMeleeUsesHUD.cs",
            "Assets/TINS - New Mechanics/PurificationReveal.cs", "Assets/TINS - New Mechanics/EnsureStartingPistol.cs", "Assets/TINS - New Mechanics/LightRadiusSkillCheckActivator.cs",
            "Assets/TINS - New Mechanics/TowerAudioFeedbacks.cs"
        };
        foreach (string source in scripts)
        {
            if (!File.Exists(source)) continue;
            string destination = "Assets/TINS - New Mechanics/Campaign/Runtime/" + Path.GetFileName(source);
            if (AssetDatabase.LoadAssetAtPath<Object>(destination) != null) continue;
            string error = AssetDatabase.MoveAsset(source, destination);
            if (!string.IsNullOrEmpty(error)) Debug.LogWarning("Could not organize " + source + ": " + error);
        }
    }

    private static TMP_Text[] GetButtonLabels(Button[] buttons)
    {
        TMP_Text[] labels = new TMP_Text[buttons.Length];
        for (int i = 0; i < buttons.Length; i++) labels[i] = buttons[i] != null ? buttons[i].GetComponentInChildren<TMP_Text>(true) : null;
        return labels;
    }

    private static Button[] GetNamedButtons(GameObject planetPanel, string prefix)
    {
        List<Button> buttons = new List<Button>();
        if (planetPanel == null) return buttons.ToArray();
        foreach (Button button in planetPanel.GetComponentsInChildren<Button>(true))
            if (button != null && button.name.StartsWith(prefix, System.StringComparison.Ordinal)) buttons.Add(button);
        return buttons.ToArray();
    }

    private static Button FindNamedButton(GameObject parent, string objectName)
    {
        Transform child = parent != null ? parent.transform.Find(objectName) : null;
        return child != null ? child.GetComponent<Button>() : null;
    }

    private static void SetObject(Object target, string propertyName, Object value)
    {
        if (target == null) return;
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) return;
        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectArray(Object target, string propertyName, Object[] values)
    {
        if (target == null) return;
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null || !property.isArray) return;
        property.arraySize = values != null ? values.Length : 0;
        for (int i = 0; i < property.arraySize; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Object target, string propertyName, float value)
    {
        if (target == null) return;
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) return;
        property.floatValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetEnum(Object target, string propertyName, int value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null) return;
        property.enumValueIndex = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T FindFirst<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T component = root.GetComponentInChildren<T>(true);
            if (component != null) return component;
        }
        return null;
    }

    private static T[] FindAll<T>(Scene scene) where T : Component
    {
        List<T> results = new List<T>();
        foreach (GameObject root in scene.GetRootGameObjects()) results.AddRange(root.GetComponentsInChildren<T>(true));
        return results.ToArray();
    }

    private static GameObject FindInScene(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name) return root;
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children) if (child.name == name) return child.gameObject;
        }
        return null;
    }

    private static void RemoveNamed(Scene scene, params string[] names)
    {
        foreach (string name in names)
        {
            GameObject item = FindInScene(scene, name);
            if (item != null) Object.DestroyImmediate(item);
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
