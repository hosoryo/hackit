using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Tooltip("生成したいPrefabをここにセット")]
    public GameObject prefab;

    [Tooltip("ワールド空間での生成位置")]
    public Transform spawnPoint;

    // Event TriggerのPointerClickで呼び出す
    public void SpawnObject()
    {
        if (prefab == null || spawnPoint == null)
        {
            Debug.LogWarning("PrefabまたはSpawnPointが設定されていません");
            return;
        }
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}