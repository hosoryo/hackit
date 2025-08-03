using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Tooltip("����������Prefab�������ɃZ�b�g")]
    public GameObject prefab;

    [Tooltip("���[���h��Ԃł̐����ʒu")]
    public Transform spawnPoint;

    // Event Trigger��PointerClick�ŌĂяo��
    public void SpawnObject()
    {
        if (prefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Prefab�܂���SpawnPoint���ݒ肳��Ă��܂���");
            return;
        }
        Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}