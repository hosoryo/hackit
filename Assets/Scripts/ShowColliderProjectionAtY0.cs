using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ShowColliderProjectionAtY0 : MonoBehaviour
{
    [Tooltip("線の色")]
    public Color lineColor = Color.green;
    [Tooltip("線の太さ")]
    public float lineWidth = 0.02f;
    [Tooltip("表示するColliderが複数ある場合はtrueで子も含める")]
    public bool includeChildColliders = true;

    XRGrabInteractable grab;
    LineRenderer lr;
    bool isSelected = false;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnSelectEntered);
        grab.selectExited.AddListener(OnSelectExited);

        // LineRenderer を用意（プレハブ不要で自動作成）
        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = lr.endWidth = lineWidth;
        lr.positionCount = 4;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = lineColor;
        lr.enabled = false;
    }

    void OnDestroy()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
            grab.selectExited.RemoveListener(OnSelectExited);
        }
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        isSelected = true;
        UpdateProjection(); // 即時表示
        lr.enabled = true;
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        isSelected = false;
        lr.enabled = false;
    }

    void Update()
    {
        if (isSelected)
        {
            UpdateProjection();
        }
    }

    void UpdateProjection()
    {
        var bounds = CalculateCombinedBounds();
        // XZ上に矩形投影、Y を 0 に固定
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        Vector3 p0 = new Vector3(min.x, 0f, min.z);
        Vector3 p1 = new Vector3(max.x, 0f, min.z);
        Vector3 p2 = new Vector3(max.x, 0f, max.z);
        Vector3 p3 = new Vector3(min.x, 0f, max.z);

        lr.positionCount = 4;
        lr.SetPosition(0, p0);
        lr.SetPosition(1, p1);
        lr.SetPosition(2, p2);
        lr.SetPosition(3, p3);

        // 色／太さが Inspector で変わった場合に反映
        lr.startWidth = lr.endWidth = lineWidth;
        lr.startColor = lr.endColor = lineColor;
    }

    Bounds CalculateCombinedBounds()
    {
        Collider[] cols;
        if (includeChildColliders)
            cols = GetComponentsInChildren<Collider>();
        else
        {
            var single = GetComponent<Collider>();
            cols = single != null ? new Collider[] { single } : new Collider[0];
        }

        // 初期化済みフラグとBoundsの初期化
        bool initialized = false;
        Bounds combined = new Bounds(Vector3.zero, Vector3.zero);

        foreach (var c in cols)
        {
            if (c == null) continue;
            if (!initialized)
            {
                combined = c.bounds; // 最初の有効な Collider で初期化
                initialized = true;
            }
            else
            {
                combined.Encapsulate(c.bounds);
            }
        }

        if (initialized)
            return combined;

        // Collider が一つもなかった場合のフォールバック
        var rend = GetComponentInChildren<Renderer>();
        if (rend != null) return rend.bounds;

        // 最終フォールバック: オブジェクト位置を中心とした小さなBounds
        return new Bounds(transform.position, Vector3.one * 0.1f);
    }

}