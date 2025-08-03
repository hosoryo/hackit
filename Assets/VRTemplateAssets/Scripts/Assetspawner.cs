using System.Collections.Generic;
using UnityEngine;

public class AssetSpawner : MonoBehaviour
{
    [Header("スポーン位置の基準となるカメラ")]
    public Transform cameraTransform;

    [Header("スポーン距離")]
    public float distanceFromCamera = 2.0f;

    [Header("登録するプレハブ一覧")]
    public List<GameObject> prefabs;

    /// <summary>
    /// UIボタンから呼び出す。index番のプレハブをスポーンする
    /// </summary>
    /// <param name="index">prefabsリストのインデックス</param>
    public void SpawnByIndex(int index)
    {
        if (index < 0 || index >= prefabs.Count)
        {
            Debug.LogWarning($"SpawnByIndex: 無効なインデックス {index}");
            return;
        }

        // カメラ前方の座標計算
        Vector3 spawnPos = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        // カメラを向くオブジェクトにするなら、下記を使用
        Quaternion spawnRot = Quaternion.LookRotation(spawnPos - cameraTransform.position);

        Instantiate(prefabs[index], spawnPos, spawnRot);
    }
}