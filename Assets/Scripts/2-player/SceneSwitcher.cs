using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) // N = Next
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
