using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabOutlineProjectToGround : MonoBehaviour
{
    [Header("Appearance")]
    public Color outlineColor = Color.yellow;
    public float lineWidth = 0.01f;
    public Material lineMaterial;
    [Header("Behaviour")]
    public bool useAllColliders = false;
    public bool closeLoop = true; // ループで閉じるか（地面矩形は閉じるのが一般的）

    XRGrabInteractable grab;
    GameObject lineGO;
    LineRenderer line;
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
        lineGO = new GameObject($"{name}_GroundOutline");
        // シーンルートに置いてワールド空間で管理
        lineGO.transform.SetParent(null, true);

        line = lineGO.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = false;
        line.widthMultiplier = lineWidth;
        line.numCapVertices = 0;
        line.numCornerVertices = 0;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.enabled = false;

        if (lineMaterial != null) line.material = lineMaterial;
        else line.material = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.DontSave };
        line.startColor = line.endColor = outlineColor;
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        isShowing = true;
        line.enabled = true;
        UpdateAndSetLine();
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        isShowing = false;
        line.enabled = false;
    }

    void Update()
    {
        if (!isShowing) return;
        UpdateAndSetLine();
    }

    void UpdateAndSetLine()
    {
        Bounds b = GetRelevantBounds();
        if (b.size == Vector3.zero)
        {
            line.positionCount = 0;
            return;
        }

        // Bounds の 8 頂点を取得し、各頂点を Y=0 に投影（y を 0 にする）
        Vector3 c = b.center;
        Vector3 e = b.extents;

        Vector3[] corners = new Vector3[8]
        {
            c + new Vector3(-e.x, -e.y, -e.z),
            c + new Vector3(e.x, -e.y, -e.z),
            c + new Vector3(e.x, -e.y, e.z),
            c + new Vector3(-e.x, -e.y, e.z),
            c + new Vector3(-e.x, e.y, -e.z),
            c + new Vector3(e.x, e.y, -e.z),
            c + new Vector3(e.x, e.y, e.z),
            c + new Vector3(-e.x, e.y, e.z)
        };

        // 投影後の頂点を XZ 平面に沿って使いやすい矩形の順序に整理する
        // 合成 bounds の場合、投影すると重複や凹形が出ることがあるが、簡潔に外枠を得るため
        // X 最小/最大、Z 最小/最大 を使って外接矩形を作る（これが "コライダーの外枠" 相当）
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 v = corners[i];
            if (v.x < minX) minX = v.x;
            if (v.x > maxX) maxX = v.x;
            if (v.z < minZ) minZ = v.z;
            if (v.z > maxZ) maxZ = v.z;
        }

        float y = 0f; // 地面 Y 座標
        Vector3[] pts;
        if (closeLoop)
        {
            pts = new Vector3[]
            {
                new Vector3(minX, y, minZ),
                new Vector3(maxX, y, minZ),
                new Vector3(maxX, y, maxZ),
                new Vector3(minX, y, maxZ),
                new Vector3(minX, y, minZ)
            };
        }
        else
        {
            pts = new Vector3[]
            {
                new Vector3(minX, y, minZ),
                new Vector3(maxX, y, minZ),
                new Vector3(maxX, y, maxZ),
                new Vector3(minX, y, maxZ)
            };
        }

        line.positionCount = pts.Length;
        line.SetPositions(pts);
        // 見た目更新
        line.startColor = line.endColor = outlineColor;
        line.widthMultiplier = lineWidth;
    }

    Bounds GetRelevantBounds()
    {
        if (useAllColliders)
        {
            Collider[] cols = GetComponentsInChildren<Collider>();
            if (cols != null && cols.Length > 0)
            {
                Bounds b = cols[0].bounds;
                for (int i = 1; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
                return b;
            }
        }

        Collider primary = GetComponent<Collider>();
        if (primary != null) return primary.bounds;

        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null) return rend.bounds;

        return new Bounds(transform.position, Vector3.zero);
    }
}