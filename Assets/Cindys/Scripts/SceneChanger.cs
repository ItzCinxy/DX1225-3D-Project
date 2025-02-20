using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] public string sceneName;  // The field to hold the scene name

    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneName);  // Use the value of sceneName to load the scene
    }
}