using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabOutlineFollow : MonoBehaviour
{
    [Header("Outline")]
    public Color outlineColor = Color.yellow;
    public float lineWidth = 0.01f;
    public Material lineMaterial; // 任意。未設定なら自動生成
    public bool useAllColliders = false;
    public bool projectToYZero = false;
    public bool showTopFaceOnly = true; // true -> 上面のみの矩形。false -> 立方体ワイヤーフレーム（12辺）

    XRGrabInteractable grab;
    GameObject lineGO;
    LineRenderer line;
    Bounds currentBounds;
    bool isShowing = false;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);

        CreateLineRenderer();
    }

    void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
            grab.selectExited.RemoveListener(OnSelectExited);
        }

        if (lineGO != null) Destroy(lineGO);
    }

    void CreateLineRenderer()
    {
        lineGO = new GameObject($"{name}_Outline");
        // ワールド空間で管理するため親は未指定（またはシーンルート）
        lineGO.transform.SetParent(null, true);

        line = lineGO.AddComponent<LineRenderer>();
        line.loop = false;
        line.useWorldSpace = true;
        line.widthMultiplier = lineWidth;
        line.numCapVertices = 0;
        line.numCornerVertices = 0;
        line.receiveShadows = false;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (lineMaterial != null) line.material = lineMaterial;
        else
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.hideFlags = HideFlags.DontSave;
            line.material = mat;
        }
        line.startColor = line.endColor = outlineColor;
        line.enabled = false;
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        UpdateBounds();
        isShowing = true;
        line.enabled = true;
        // 即座にラインをセットして、Update で追従させる
        SetLineFromBounds(currentBounds);
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        isShowing = false;
        line.enabled = false;
    }

    void Update()
    {
        if (!isShowing) return;

        // 毎フレーム bounds を再計算して追従（Rigidbody で移動する場合も対応）
        UpdateBounds();
        SetLineFromBounds(currentBounds);
    }

    void UpdateBounds()
    {
        if (useAllColliders)
        {
            var cols = GetComponentsInChildren<Collider>();
            if (cols.Length > 0)
            {
                Bounds b = cols[0].bounds;
                for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
                currentBounds = b;
                return;
            }
        }
        // 優先: このオブジェクトの Collider
        var primary = GetComponent<Collider>();
        if (primary != null)
        {
            currentBounds = primary.bounds;
            return;
        }
        // 次: Renderer
        var rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            currentBounds = rend.bounds;
            return;
        }
        // フォールバック: 位置でゼロサイズ
        currentBounds = new Bounds(transform.position, Vector3.zero);
    }

    void SetLineFromBounds(Bounds b)
    {
        if (projectToYZero)
        {
            float minX = b.min.x, maxX = b.max.x, minZ = b.min.z, maxZ = b.max.z;
            float y = 0f;
            Vector3[] pts = new Vector3[]
            {
                new Vector3(minX, y, minZ),
                new Vector3(maxX, y, minZ),
                new Vector3(maxX, y, maxZ),
                new Vector3(minX, y, maxZ),
                new Vector3(minX, y, minZ) // ループを手動で閉じる
            };
            line.positionCount = pts.Length;
            line.SetPositions(pts);
            line.startColor = line.endColor = outlineColor;
            line.widthMultiplier = lineWidth;
            return;
        }

        Vector3 c = b.center;
        Vector3 e = b.extents;

        if (showTopFaceOnly)
        {
            Vector3 p0 = new Vector3(c.x - e.x, c.y + e.y, c.z - e.z);
            Vector3 p1 = new Vector3(c.x + e.x, c.y + e.y, c.z - e.z);
            Vector3 p2 = new Vector3(c.x + e.x, c.y + e.y, c.z + e.z);
            Vector3 p3 = new Vector3(c.x - e.x, c.y + e.y, c.z + e.z);
            Vector3[] pts = new Vector3[] { p0, p1, p2, p3, p0 };
            line.positionCount = pts.Length;
            line.SetPositions(pts);
        }
        else
        {
            // 立方体ワイヤーフレーム (12辺). 頂点順序を用意してラインで接続（辺ごとに再利用）
            Vector3[] v = new Vector3[8];
            v[0] = c + new Vector3(-e.x, -e.y, -e.z);
            v[1] = c + new Vector3(e.x, -e.y, -e.z);
            v[2] = c + new Vector3(e.x, -e.y, e.z);
            v[3] = c + new Vector3(-e.x, -e.y, e.z);
            v[4] = c + new Vector3(-e.x, e.y, -e.z);
            v[5] = c + new Vector3(e.x, e.y, -e.z);
            v[6] = c + new Vector3(e.x, e.y, e.z);
            v[7] = c + new Vector3(-e.x, e.y, e.z);

            // エッジを線で繋ぐ配列（各辺を順に並べる）
            int[] order = new int[]
            {
                0,1, 1,2, 2,3, 3,0, // 底面
                4,5, 5,6, 6,7, 7,4, // 上面
                0,4, 1,5, 2,6, 3,7  // 垂直辺
            };

            Vector3[] pts = new Vector3[order.Length];
            for (int i = 0; i < order.Length; i++) pts[i] = v[order[i]];

            line.positionCount = pts.Length;
            line.SetPositions(pts);
        }

        line.startColor = line.endColor = outlineColor;
        line.widthMultiplier = lineWidth;
    }
}