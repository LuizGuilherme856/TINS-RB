#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TINS.Campaign;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using Image = UnityEngine.UI.Image;
using Button = UnityEngine.UI.Button;
using UnityEngine.UI;

public static class CampaignSkillTreeSceneSetup
{
    private const string ScenePath = "Assets/Scenes/Campaign/CampaignBase.unity";
    private const string CatalogPath = "Assets/TINS - New Mechanics/Campaign/Resources/CampaignCatalog.asset";
    private const string NodeScriptPath = "Assets/TINS - New Mechanics/Campaign/Runtime/CampaignSkillTreeNodeView.cs";

    private const string Art = "Assets/Sprites/UI/skill tree/";
    private const string Controls = "Assets/Sprites/UI/New HUD/Buttons icons 2.png";
    private const string Sounds = "Assets/SFX/UI SFX/";
    private static readonly Color Ink = new Color32(13, 10, 20, 255);
    private static readonly Color Dark = new Color32(23, 16, 32, 255);
    private static readonly Color Border = new Color32(83, 44, 72, 255);
    private static readonly Color Title = new Color32(251, 229, 219, 255);
    private static readonly Color Muted = new Color32(195, 164, 184, 255);
    private static readonly Color Accent = new Color32(235, 145, 170, 255);

    [MenuItem("Tools/There Is No Sun/Build Progressive Skill Tree")]
    public static void Build()
    {
        CampaignCatalog catalog = AssetDatabase.LoadAssetAtPath<CampaignCatalog>(CatalogPath);
        if (catalog == null || catalog.skills == null || catalog.skills.Length == 0)
            throw new System.InvalidOperationException("CampaignCatalog does not contain skill assets.");
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.isLoaded)
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        GameObject panel = FindObject(scene, "Campaign Skill Tree");
        if (panel == null) throw new System.InvalidOperationException("Campaign Skill Tree panel is missing.");

        CampaignSkillTreeView tree = EnsureComponent<CampaignSkillTreeView>(panel);
        CampaignBaseController baseController = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<CampaignBaseController>(true)).FirstOrDefault();
        if (baseController != null) SetObject(baseController, "campaignSkillTree", tree);

        RectTransform rootRect = (RectTransform)panel.transform;
        rootRect.SetParent(null, false);
        rootRect.localScale = Vector3.one;
        rootRect.localRotation = Quaternion.identity;
        Image background = panel.GetComponent<Image>();
        background.sprite = null;
        background.color = Ink;
        Canvas overlay = EnsureComponent<Canvas>(panel);
        overlay.renderMode = RenderMode.ScreenSpaceOverlay;
        overlay.sortingOrder = 2000;
        overlay.pixelPerfect = true;
        CanvasScaler scaler = EnsureComponent<CanvasScaler>(panel);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0f;
        EnsureComponent<GraphicRaycaster>(panel);

        CampaignSkillTreeScroll scrollRect = EnsureSkillTreeScroll(panel.transform);
        RectTransform content = scrollRect.content;
        AddHeader(panel.transform);
        TMP_Text wallet = EnsureText(panel.transform, "Campaign Tree Wallet", "ALMAS  0", 16f, TextAlignmentOptions.MidlineRight, Title);
        Place(wallet.rectTransform, new Vector2(.39f, 1f), Vector2.one, new Vector2(12f, -82f), new Vector2(-170f, -24f));

        Image card = EnsureImage(panel.transform, "Campaign Skill Detail Card", Dark);
        Place(card.rectTransform, Vector2.right, Vector2.one, new Vector2(-332f, 84f), new Vector2(-28f, -100f));
        AddLine(card.transform, "Card Accent", Vector2.up, Vector2.one, new Vector2(0f, -2f), Vector2.zero, Accent);
        Image iconBacking = EnsureImage(card.transform, "Detail Icon Backing", new Color32(52, 30, 55, 255));
        Place(iconBacking.rectTransform, Vector2.up, Vector2.up, new Vector2(24f, -96f), new Vector2(94f, -26f));
        Image detailIcon = EnsureImage(iconBacking.transform, "Detail Icon", Color.white);
        detailIcon.preserveAspect = true;
        Place(detailIcon.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -12f));

        TMP_Text title = EnsureText(card.transform, "Campaign Skill Detail Title", "Flash de Luz", 26f, TextAlignmentOptions.BottomLeft, Title);
        Place(title.rectTransform, Vector2.up, Vector2.one, new Vector2(24f, -149f), new Vector2(-22f, -101f));
        TMP_Text details = MoveText(panel.transform, card.transform, "Campaign Skill Details", "", 17f, TextAlignmentOptions.TopLeft, Muted);
        Place(details.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 148f), new Vector2(-22f, -162f));
        details.overflowMode = TextOverflowModes.Ellipsis;
        AddLine(card.transform, "Purchase Divider", Vector2.zero, Vector2.right, new Vector2(24f, 136f), new Vector2(-24f, 137f), Border);
        TMP_Text price = EnsureText(card.transform, "Campaign Skill Price", "6 almas", 18f, TextAlignmentOptions.MidlineLeft, Title);
        Place(price.rectTransform, Vector2.zero, Vector2.right, new Vector2(24f, 99f), new Vector2(-24f, 131f));
        TMP_Text status = MoveText(panel.transform, card.transform, "Campaign Tree Status", "", 15f, TextAlignmentOptions.MidlineLeft, Accent);
        Place(status.rectTransform, Vector2.zero, Vector2.right, new Vector2(24f, 14f), new Vector2(-24f, 54f));
        Button buyButton = StyleButton(panel.transform, card.transform, "Buy Selected Campaign Skill", "COMPRAR",
            Vector2.zero, Vector2.right, new Vector2(24f, 56f), new Vector2(-24f, 94f), 17f, new Color32(162, 64, 100, 255));
        Button resetButton = StyleButton(panel.transform, panel.transform, "Reset Campaign Upgrades", "Reiniciar melhorias",
            Vector2.right, Vector2.right, new Vector2(-332f, 24f), new Vector2(-28f, 64f), 16f, Border);
        StyleButton(panel.transform, panel.transform, "Close Skill Tree", "VOLTAR", Vector2.one, Vector2.one,
            new Vector2(-148f, -72f), new Vector2(-28f, -32f), 16f, Border);

        Button zoomOut = StyleButton(panel.transform, panel.transform, "Campaign Tree Zoom Out", "−", Vector2.zero,
            Vector2.zero, new Vector2(28f, 24f), new Vector2(70f, 64f), 25f, Border);
        Button centerTree = StyleButton(panel.transform, panel.transform, "Campaign Tree Center", "Centralizar", Vector2.zero,
            Vector2.zero, new Vector2(78f, 24f), new Vector2(184f, 64f), 15f, Border);
        Button zoomIn = StyleButton(panel.transform, panel.transform, "Campaign Tree Zoom In", "+", Vector2.zero,
            Vector2.zero, new Vector2(192f, 24f), new Vector2(234f, 64f), 25f, Border);
        AddPointerHint(panel.transform, "Drag Hint", 262f, 25, "Arrastar");
        AddPointerHint(panel.transform, "Zoom Hint", 384f, 26, "Zoom");

        AudioSource audio = EnsureComponent<AudioSource>(panel);
        audio.playOnAwake = false;
        audio.spatialBlend = 0f;
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(
            "Assets/Packages B/Feel/MMTools/Core/MMAudio/MMSoundManager/Settings/MMSoundManagerAudioMixer.mixer");
        if (mixer != null) audio.outputAudioMixerGroup = mixer.FindMatchingGroups("UI").FirstOrDefault();
        SetObject(tree, "audioSource", audio);
        SetObject(tree, "hoverSound", AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "Hover over button sound 12.ogg"));
        SetObject(tree, "selectSound", AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "path select - Special Click 04.ogg"));
        SetObject(tree, "purchaseSound", AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "buy sound.ogg"));
        SetObject(tree, "deniedSound", AssetDatabase.LoadAssetAtPath<AudioClip>(Sounds + "cant buy error sound.ogg"));

        List<CampaignSkillTreeNodeView> nodes = new List<CampaignSkillTreeNodeView>();
        Dictionary<CampaignSkill, CampaignSkillTreeNodeView> bySkill = new Dictionary<CampaignSkill, CampaignSkillTreeNodeView>();
        foreach (CampaignSkill skill in catalog.skills)
        {
            if (skill == null) continue;
            Button button = FindButton(panel.transform, "Skill " + skill.id);
            if (button == null) button = CreateButton(content, "Skill " + skill.id, skill.displayName,
                new Vector2(80f, 80f), skill.editorPosition, null);
            button.transform.SetParent(content, false);
            RectTransform rect = button.GetComponent<RectTransform>();
            Vector2 half = Vector2.one * (skill.visibleAtStart ? 42f : 36f);
            AnchorAtCenter(rect, skill.editorPosition, half * 2f);
            Image hitArea = button.GetComponent<Image>();
            hitArea.sprite = null;
            hitArea.color = Color.clear;
            hitArea.raycastTarget = true;

            Image art = EnsureImage(button.transform, "Node Art", new Color32(33, 21, 39, 255));
            Place(art.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image frame = EnsureImage(art.transform, "Node Frame", Accent, SpriteAt(Art + "skill tree icons.png", "skill tree icons_7"));
            Place(frame.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image ring = EnsureImage(art.transform, "Selected Outline", Title, SpriteAt(Art + "skill tree icons.png", "skill tree icons_8"));
            Place(ring.rectTransform, Vector2.zero, Vector2.one, new Vector2(-5f, -5f), new Vector2(5f, 5f));
            ring.enabled = false;

            Image icon = button.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.name == "Campaign Skill Icon");
            if (icon == null) icon = EnsureImage(art.transform, "Campaign Skill Icon", Color.white);
            icon.transform.SetParent(art.transform, false);
            icon.sprite = skill.icon;
            icon.enabled = skill.icon != null;
            icon.preserveAspect = true;
            AnchorAtCenter(icon.rectTransform, Vector2.zero, Vector2.one * (skill.visibleAtStart ? 50f : 44f));

            TMP_Text label = button.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name != "Rank");
            if (label == null) label = EnsureText(button.transform, "Label", skill.displayName, 16f, TextAlignmentOptions.Center, Title);
            SetTextStyle(label, skill.displayName, 16f, TextAlignmentOptions.Center, Title);
            Place(label.rectTransform, new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(-83f, -31f), new Vector2(83f, -6f));
            TMP_Text rank = EnsureText(button.transform, "Rank", "0/" + skill.maximumRank, 14f, TextAlignmentOptions.Center, Accent);
            Place(rank.rectTransform, new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(-37f, -48f), new Vector2(37f, -29f));
            button.transition = Selectable.Transition.None;
            button.targetGraphic = hitArea;

            CampaignSkillTreeNodeView view = GetOrCreateNodeView(button.gameObject);
            SetObject(view, "skill", skill);
            SetString(view, "skillId", skill.id);
            SetObject(view, "button", button);
            SetObject(view, "label", label);
            SetObject(view, "rankLabel", rank);
            SetObject(view, "icon", icon);
            SetObject(view, "frame", frame);
            SetObject(view, "selectionRing", ring);
            SetObject(view, "art", art.transform);
            SetObject(view, "canvasGroup", EnsureComponent<CanvasGroup>(button.gameObject));
            nodes.Add(view);
            bySkill[skill] = view;
        }

        foreach (Transform child in content.Cast<Transform>().ToArray())
        {
            if (child.name.StartsWith("Connection ", System.StringComparison.Ordinal))
                Undo.DestroyObjectImmediate(child.gameObject);
            if (child != null && child.TryGetComponent(out CampaignSkillTreeNodeView existing) && !nodes.Contains(existing))
                child.gameObject.SetActive(false);
        }
        List<CampaignSkillTreeConnectionView> connections = new List<CampaignSkillTreeConnectionView>();
        foreach (CampaignSkill skill in catalog.skills)
        {
            if (skill == null) continue;
            HashSet<CampaignSkill> sources = new HashSet<CampaignSkill>();
            if (skill.prerequisites != null)
                foreach (CampaignSkillPrerequisite requirement in skill.prerequisites)
                    if (requirement != null && requirement.skill != null) sources.Add(requirement.skill);
            if (skill.revealPrerequisites != null)
                foreach (CampaignSkillPrerequisite requirement in skill.revealPrerequisites)
                    if (requirement != null && requirement.skill != null) sources.Add(requirement.skill);
            foreach (CampaignSkill prerequisite in sources)
            {
                if (!bySkill.ContainsKey(prerequisite)) continue;
                GameObject edgeObject = new GameObject("Connection " + prerequisite.id + " to " + skill.id,
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(CampaignSkillTreeConnectionView));
                edgeObject.transform.SetParent(content, false);
                edgeObject.transform.SetAsFirstSibling();
                RectTransform edgeRect = edgeObject.GetComponent<RectTransform>();
                edgeRect.anchorMin = Vector2.zero;
                edgeRect.anchorMax = Vector2.one;
                edgeRect.offsetMin = edgeRect.offsetMax = Vector2.zero;
                CampaignSkillTreeConnectionView edge = edgeObject.GetComponent<CampaignSkillTreeConnectionView>();
                edge.Configure(bySkill[prerequisite], bySkill[skill]);
                connections.Add(edge);
            }
        }

        SetObjectArray(tree, "nodes", nodes.Cast<Object>().ToArray());
        SetObjectArray(tree, "connections", connections.Cast<Object>().ToArray());
        SetObject(tree, "walletLabel", wallet);
        SetObject(tree, "detailsLabel", details);
        SetObject(tree, "statusLabel", status);
        SetObject(tree, "buySelectedButton", buyButton);
        SetObject(tree, "resetButton", resetButton);
        SetObject(tree, "zoomOutButton", zoomOut);
        SetObject(tree, "zoomInButton", zoomIn);
        SetObject(tree, "centerTreeButton", centerTree);
        SetObject(tree, "treeScrollRect", scrollRect);
        SetObject(tree, "detailTitle", title);
        SetObject(tree, "detailCost", price);
        SetObject(tree, "detailIcon", detailIcon);
        UpdateContentBounds(scrollRect, catalog.skills);
        EditorUtility.SetDirty(tree);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Built a progressive campaign skill tree with {nodes.Count} persistent nodes and {connections.Count} prerequisite connections.");
    }

    [MenuItem("Tools/There Is No Sun/Add Selected Skill Asset To Tree")]
    public static void AddSelectedSkill()
    {
        CampaignSkill selectedSkill = Selection.activeObject as CampaignSkill;
        if (selectedSkill == null)
        {
            Debug.LogWarning("Select a Campaign Skill ScriptableObject in the Project window first.");
            return;
        }
        CampaignCatalog catalog = AssetDatabase.LoadAssetAtPath<CampaignCatalog>(CatalogPath);
        if (catalog == null) throw new System.InvalidOperationException("CampaignCatalog is missing.");
        if (catalog.skills.Any(skill => skill != null && skill.id == selectedSkill.id))
        {
            Debug.LogWarning("A skill with this permanent ID is already in the tree: " + selectedSkill.id);
            return;
        }
        List<CampaignSkill> skills = catalog.skills != null ? catalog.skills.ToList() : new List<CampaignSkill>();
        skills.Add(selectedSkill);
        catalog.skills = skills.ToArray();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Build();
    }

    private static GameObject FindObject(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == name) return item.gameObject;
        return null;
    }

    private static Button FindButton(Transform parent, string name)
    {
        foreach (Transform item in parent.GetComponentsInChildren<Transform>(true))
            if (item.name == name && item.TryGetComponent(out Button button)) return button;
        return null;
    }

    private static TMP_Text FindText(Transform parent, string name)
    {
        foreach (Transform item in parent.GetComponentsInChildren<Transform>(true))
            if (item.name == name && item.TryGetComponent(out TMP_Text text)) return text;
        return null;
    }

    private static CampaignSkillTreeNodeView GetOrCreateNodeView(GameObject nodeObject)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(nodeObject);
        MonoScript expectedScript = AssetDatabase.LoadAssetAtPath<MonoScript>(NodeScriptPath);
        CampaignSkillTreeNodeView keep = null;
        foreach (CampaignSkillTreeNodeView candidate in nodeObject.GetComponents<CampaignSkillTreeNodeView>())
        {
            MonoScript source = MonoScript.FromMonoBehaviour(candidate);
            if (keep == null && source == expectedScript)
            {
                keep = candidate;
                continue;
            }
            Undo.DestroyObjectImmediate(candidate);
        }
        return keep != null ? keep : Undo.AddComponent<CampaignSkillTreeNodeView>(nodeObject);
    }

    private static CampaignSkillTreeScroll EnsureSkillTreeScroll(Transform panel)
    {
        GameObject viewport = EnsureRect(panel, "Campaign Skill Tree Viewport").gameObject;
        RectTransform viewRect = (RectTransform)viewport.transform;
        Place(viewRect, Vector2.zero, Vector2.one, new Vector2(28f, 84f), new Vector2(-350f, -100f));
        Image fill = EnsureComponent<Image>(viewport);
        fill.color = new Color32(17, 12, 24, 255);
        fill.raycastTarget = true;
        EnsureComponent<RectMask2D>(viewport);
        viewport.transform.SetAsFirstSibling();

        GameObject backdrop = EnsureRect(viewRect, "Graph Dots").gameObject;
        EnsureComponent<CanvasRenderer>(backdrop);
        CampaignSkillTreeBackdrop dots = EnsureComponent<CampaignSkillTreeBackdrop>(backdrop);
        Place(dots.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        dots.color = new Color32(68, 44, 72, 255);
        dots.raycastTarget = false;
        dots.transform.SetAsFirstSibling();
        AddLine(viewRect, "Graph Accent", Vector2.up, Vector2.one, new Vector2(0f, -2f), Vector2.zero, Border);

        RectTransform content = EnsureRect(viewRect, "Campaign Skill Tree Content");
        AnchorAtCenter(content, Vector2.zero, new Vector2(1100f, 700f));
        content.localScale = Vector3.one;
        content.SetAsLastSibling();

        ScrollRect previous = viewport.GetComponent<ScrollRect>();
        if (previous != null && previous.GetType() == typeof(ScrollRect)) Undo.DestroyObjectImmediate(previous);
        CampaignSkillTreeScroll scroll = EnsureComponent<CampaignSkillTreeScroll>(viewport);
        scroll.viewport = viewRect;
        scroll.content = content;
        scroll.horizontal = true;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = false;
        return scroll;
    }

    private static void UpdateContentBounds(CampaignSkillTreeScroll scroll, CampaignSkill[] skills)
    {
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;
        foreach (CampaignSkill skill in skills)
        {
            if (skill == null) continue;
            min = Vector2.Min(min, skill.editorPosition - new Vector2(95f, 103f));
            max = Vector2.Max(max, skill.editorPosition + new Vector2(95f, 75f));
        }
        SerializedObject data = new SerializedObject(scroll);
        data.FindProperty("graphBounds").rectValue = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddHeader(Transform panel)
    {
        TMP_Text title = EnsureText(panel, "Skill Panel Title", "ÁRVORE DE HABILIDADES", 34f, TextAlignmentOptions.MidlineLeft, Title);
        title.characterSpacing = 3f;
        Place(title.rectTransform, Vector2.up, new Vector2(.48f, 1f), new Vector2(28f, -75f), new Vector2(0f, -29f));
        AddLine(panel, "Header Divider", Vector2.up, Vector2.one, new Vector2(28f, -86f), new Vector2(-28f, -84f), Border);
    }

    private static void AddPointerHint(Transform panel, string name, float x, int sprite, string label)
    {
        RectTransform hint = EnsureRect(panel, name);
        Place(hint, Vector2.zero, Vector2.zero, new Vector2(x, 30f), new Vector2(x + 110f, 58f));
        Image glyph = EnsureImage(hint, "Mouse Icon", Color.white, SpriteAt(Controls, "Buttons icons 2_" + sprite));
        glyph.preserveAspect = true;
        Place(glyph.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(25f, 27f));
        TMP_Text text = EnsureText(hint, "Hint", label, 15f, TextAlignmentOptions.MidlineLeft, Muted);
        Place(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(32f, 0f), Vector2.zero);
    }

    private static Button StyleButton(Transform root, Transform parent, string name, string caption, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 min, Vector2 max, float fontSize, Color tint)
    {
        Button button = FindButton(root, name);
        if (button == null) button = CreateButton(parent, name, caption, Vector2.one, Vector2.zero, null);
        if (button.transform.parent != parent) button.transform.SetParent(parent, false);
        Place((RectTransform)button.transform, anchorMin, anchorMax, min, max);

        Image image = button.GetComponent<Image>();
        image.sprite = null;
        image.color = Color.white;
        image.raycastTarget = true;
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = tint;
        colors.pressedColor = Color.Lerp(tint, Color.black, .22f);
        colors.highlightedColor = Color.Lerp(tint, Color.white, .23f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color32(51, 38, 58, 255);
        button.colors = colors;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text == null) text = EnsureText(button.transform, "Label", caption, fontSize, TextAlignmentOptions.Center, Title);
        SetTextStyle(text, caption, fontSize, TextAlignmentOptions.Center, Title);
        Place(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 0f), new Vector2(-6f, 0f));
        return button;
    }

    private static void AddLine(Transform parent, string name, Vector2 anchorsMin, Vector2 anchorsMax,
        Vector2 min, Vector2 max, Color color)
    {
        Image line = EnsureImage(parent, name, color);
        Place(line.rectTransform, anchorsMin, anchorsMax, min, max);
    }

    private static RectTransform EnsureRect(Transform parent, string name)
    {
        if (parent.Find(name) is RectTransform existing) return existing;
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static T EnsureComponent<T>(GameObject target) where T : Component =>
        target.TryGetComponent(out T component) ? component : Undo.AddComponent<T>(target);

    private static Sprite SpriteAt(string file, string name) =>
        AssetDatabase.LoadAllAssetsAtPath(file).OfType<Sprite>().FirstOrDefault(sprite => sprite.name == name);

    private static Image EnsureImage(Transform parent, string name, Color color, Sprite sprite = null)
    {
        Image image = EnsureComponent<Image>(EnsureRect(parent, name).gameObject);
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text MoveText(Transform root, Transform parent, string name, string value, float size,
        TextAlignmentOptions alignment, Color tint)
    {
        TMP_Text text = FindText(root, name);
        if (text == null) return EnsureText(parent, name, value, size, alignment, tint);
        text.transform.SetParent(parent, false);
        SetTextStyle(text, value, size, alignment, tint);
        return text;
    }

    private static TMP_Text EnsureText(Transform parent, string name, string value, float size,
        TextAlignmentOptions alignment, Color tint)
    {
        TMP_Text text = EnsureComponent<TextMeshProUGUI>(EnsureRect(parent, name).gameObject);
        SetTextStyle(text, value, size, alignment, tint);
        return text;
    }

    private static void SetTextStyle(TMP_Text text, string value, float size, TextAlignmentOptions alignment, Color tint)
    {
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSharedMaterial = text.font != null ? text.font.material : null;
        text.text = value;
        text.fontSize = size;
        text.enableAutoSizing = true;
        text.fontSizeMin = size * .74f;
        text.fontSizeMax = size;
        text.color = tint;
        text.alignment = alignment;
        text.margin = Vector4.zero;
        text.raycastTarget = false;
    }

    private static void Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 min, Vector2 max)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(.5f, .5f);
        rect.offsetMin = min;
        rect.offsetMax = max;
    }

    private static void AnchorAtCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static Button CreateButton(Transform parent, string name, string caption, Vector2 size, Vector2 position, TMP_Text style)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(.35f, .035f, .14f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        CreateText(buttonObject.transform, "Label", caption, Vector2.zero, size, 17f, style);
        return button;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, float fontSize, TMP_Text style)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = value;
        text.font = style != null ? style.font : TMP_Settings.defaultFontAsset;
        text.fontSharedMaterial = style != null ? style.fontSharedMaterial : null;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        return text;
    }

    private static void SetObject(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string propertyName, string value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(propertyName).stringValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectArray(Object target, string propertyName, Object[] values)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
