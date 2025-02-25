using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void Load(string scn)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scn);
    }
}
