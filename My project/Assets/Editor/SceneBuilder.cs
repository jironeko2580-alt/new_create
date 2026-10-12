using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// メニュー Tools > Merge Puzzle > Build Game Scene から、
/// スプライト・Prefab・ゲームシーン・UI を一括生成する。
/// 何度実行しても上書きで作り直される。
/// </summary>
public static class SceneBuilder
{
    const string ArtDir = "Assets/Art";
    const string PrefabDir = "Assets/Prefabs";
    const string ScenePath = "Assets/Scenes/Game.unity";

    // 盤面レイアウト（ワールド座標）
    const float InnerLeft = -3f;
    const float InnerRight = 3f;
    const float FloorTop = -4.2f;
    const float WallTop = 2.8f;
    const float WallThickness = 0.4f;
    const float DeadLineY = 2.0f;
    const float DropperY = 3.6f;

    static readonly Color BackgroundColor = new Color(0.98f, 0.94f, 0.85f);
    static readonly Color BoxColor = new Color(0.55f, 0.38f, 0.25f);

    [MenuItem("Tools/Merge Puzzle/Build Game Scene")]
    public static void Build()
    {
        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
        {
            EditorUtility.DisplayDialog(
                "TextMeshPro が未導入です",
                "先に Window > TextMeshPro > Import TMP Essential Resources を実行してから、もう一度このメニューを実行してください。",
                "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // 先にシーンを切り替える（後にすると、作成したアセットへの参照がシーン切替時に外れる）
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        EnsureFolder(ArtDir);
        EnsureFolder(PrefabDir);

        var circle = CreateSprite($"{ArtDir}/Circle.png", 256, true);
        var square = CreateSprite($"{ArtDir}/Square.png", 4, false);
        var material = CreatePhysicsMaterial();
        var ballPrefab = CreateBallPrefab(circle, material);

        CreateCamera();
        CreateBox(square, material);
        var deadLine = CreateDeadLine(square);
        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        CreateDropper();
        CreateUI(gm);
        Set(gm, "ballPrefab", ballPrefab);
        if (ballPrefab == null) Debug.LogError("[SceneBuilder] Ball prefab の参照に失敗しました");

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        Selection.activeObject = deadLine;
        Debug.Log("[SceneBuilder] Game scene built: " + ScenePath);
    }

    // ---------- Assets ----------

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        AssetDatabase.CreateFolder(Path.GetDirectoryName(path), Path.GetFileName(path));
    }

    static Sprite CreateSprite(string path, int size, bool circle)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) * 0.5f;
        float radius = size * 0.5f - 1f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            if (!circle)
            {
                tex.SetPixel(x, y, Color.white);
                continue;
            }
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            float alpha = Mathf.Clamp01(radius - d + 0.5f);
            // 外周を少し暗くして輪郭を出す（色はSpriteRendererで乗算）
            float shade = d > radius * 0.9f ? 0.7f : 1f;
            tex.SetPixel(x, y, new Color(shade, shade, shade, alpha));
        }
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size; // 1ユニット = スプライト1枚
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = circle ? FilterMode.Bilinear : FilterMode.Point;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static PhysicsMaterial2D CreatePhysicsMaterial()
    {
        const string path = ArtDir + "/BallPhysics.physicsMaterial2D";
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("BallPhysics");
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.friction = 0.3f;
        mat.bounciness = 0.1f;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Ball CreateBallPrefab(Sprite circle, PhysicsMaterial2D material)
    {
        var go = new GameObject("Ball");
        go.AddComponent<SpriteRenderer>().sprite = circle;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.sharedMaterial = material;
        rb.useAutoMass = true; // 大きいボールほど重くなる
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        go.AddComponent<Ball>();

        const string path = PrefabDir + "/Ball.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<Ball>(path);
    }

    // ---------- Scene objects ----------

    static void CreateCamera()
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0f, 0f, -10f);
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.2f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BackgroundColor;
    }

    static void CreateBox(Sprite square, PhysicsMaterial2D material)
    {
        var box = new GameObject("Box").transform;
        float bottom = FloorTop - WallThickness;
        float wallHeight = WallTop - bottom;
        float wallCenterY = (WallTop + bottom) * 0.5f;

        CreateBlock("LeftWall", box, square, material,
            new Vector2(InnerLeft - WallThickness * 0.5f, wallCenterY), new Vector2(WallThickness, wallHeight));
        CreateBlock("RightWall", box, square, material,
            new Vector2(InnerRight + WallThickness * 0.5f, wallCenterY), new Vector2(WallThickness, wallHeight));
        CreateBlock("Floor", box, square, material,
            new Vector2((InnerLeft + InnerRight) * 0.5f, FloorTop - WallThickness * 0.5f),
            new Vector2(InnerRight - InnerLeft + WallThickness * 2f, WallThickness));
    }

    static void CreateBlock(string name, Transform parent, Sprite square, PhysicsMaterial2D material,
        Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.color = BoxColor;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.sharedMaterial = material;
    }

    static GameObject CreateDeadLine(Sprite square)
    {
        float width = InnerRight - InnerLeft;
        float centerX = (InnerLeft + InnerRight) * 0.5f;

        // 超過検知用トリガー：ラインから上方向に広めに取る
        const float triggerHeight = 2f;
        var go = new GameObject("DeadLine");
        go.transform.position = new Vector3(centerX, DeadLineY + triggerHeight * 0.5f, 0f);
        var trigger = go.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(width, triggerHeight);

        // 警告ライン表示
        var line = new GameObject("Line");
        line.transform.SetParent(go.transform);
        line.transform.position = new Vector3(centerX, DeadLineY, 0f);
        line.transform.localScale = new Vector3(width, 0.06f, 1f);
        var sr = line.AddComponent<SpriteRenderer>();
        sr.sprite = square;
        sr.sortingOrder = 2;

        var dl = go.AddComponent<DeadLine>();
        Set(dl, "lineRenderer", sr);
        return go;
    }

    static void CreateDropper()
    {
        var go = new GameObject("Dropper");
        go.transform.position = new Vector3(0f, DropperY, 0f);
        var dropper = go.AddComponent<Dropper>();
        var so = new SerializedObject(dropper);
        so.FindProperty("minX").floatValue = InnerLeft;
        so.FindProperty("maxX").floatValue = InnerRight;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ---------- UI ----------

    static void CreateUI(GameManager gm)
    {
        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960f, 600f); // unityroom 標準サイズ
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

        // スコア（左上）
        var score = CreateText("ScoreText", canvasGo.transform, "SCORE: 0000", 36, TextAlignmentOptions.TopLeft);
        var srt = score.rectTransform;
        srt.anchorMin = srt.anchorMax = srt.pivot = new Vector2(0f, 1f);
        srt.anchoredPosition = new Vector2(24f, -20f);
        srt.sizeDelta = new Vector2(400f, 60f);
        score.color = BoxColor;

        // ゲームオーバーパネル
        var panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGo.transform, false);
        var prt = (RectTransform)panel.transform;
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = prt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        var title = CreateText("GameOverText", panel.transform, "GAME OVER", 72, TextAlignmentOptions.Center);
        Place(title.rectTransform, new Vector2(0f, 110f), new Vector2(600f, 100f));

        var finalScore = CreateText("FinalScoreText", panel.transform, "SCORE: 0000", 44, TextAlignmentOptions.Center);
        Place(finalScore.rectTransform, new Vector2(0f, 10f), new Vector2(600f, 70f));

        var buttonGo = new GameObject("RetryButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGo.transform.SetParent(panel.transform, false);
        Place((RectTransform)buttonGo.transform, new Vector2(0f, -100f), new Vector2(240f, 70f));
        buttonGo.GetComponent<Image>().color = new Color(1f, 0.6f, 0.15f);
        var label = CreateText("Label", buttonGo.transform, "RETRY", 36, TextAlignmentOptions.Center);
        var lrt = label.rectTransform;
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;

        Set(gm, "scoreText", score);
        Set(gm, "gameOverPanel", panel);
        Set(gm, "finalScoreText", finalScore);
        Set(gm, "retryButton", buttonGo.GetComponent<Button>());
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size,
        TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.font = TMP_Settings.defaultFontAsset;
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = Color.white;
        t.fontStyle = FontStyles.Bold;
        return t;
    }

    static void Place(RectTransform rt, Vector2 position, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
