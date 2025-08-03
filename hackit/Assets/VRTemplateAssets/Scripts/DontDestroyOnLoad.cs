using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneWarpTrigger : MonoBehaviour
{
    [Tooltip("‘JˆÚæƒV[ƒ“–¼‚ğw’è")]
    public string targetSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
        }
    }
}