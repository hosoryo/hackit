using System.Collections.Generic;
using UnityEngine;

public class AssetSpawner : MonoBehaviour
{
    [Header("�X�|�[���ʒu�̊�ƂȂ�J����")]
    public Transform cameraTransform;

    [Header("�X�|�[������")]
    public float distanceFromCamera = 2.0f;

    [Header("�o�^����v���n�u�ꗗ")]
    public List<GameObject> prefabs;

    /// <summary>
    /// UI�{�^������Ăяo���Bindex�Ԃ̃v���n�u���X�|�[������
    /// </summary>
    /// <param name="index">prefabs���X�g�̃C���f�b�N�X</param>
    public void SpawnByIndex(int index)
    {
        if (index < 0 || index >= prefabs.Count)
        {
            Debug.LogWarning($"SpawnByIndex: �����ȃC���f�b�N�X {index}");
            return;
        }

        // �J�����O���̍��W�v�Z
        Vector3 spawnPos = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
        // �J�����������I�u�W�F�N�g�ɂ���Ȃ�A���L���g�p
        Quaternion spawnRot = Quaternion.LookRotation(spawnPos - cameraTransform.position);

        Instantiate(prefabs[index], spawnPos, spawnRot);
    }
}