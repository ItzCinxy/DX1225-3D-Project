using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [Header("Screens")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject controlPanel;
    [SerializeField] private GameObject SettingsPanel;

    void Update()
    {
        if (controlPanel.activeSelf || SettingsPanel.activeSelf) return;    

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanel();
            SoundManager.Instance.TogglePause();
        }
    }

    void TogglePanel()
    {
        menu.SetActive(!menu.activeSelf);
        HandleGamePause(menu.activeSelf);
    }

    //public void ResetAll()
    //{
    //    ResetAllNoLoad();
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    //}

    //public void ResetAllNoLoad()
    //{
    //    PlayerPrefs.DeleteAll();
    //    HandleGamePause(false);
    //}

    void HandleGamePause(bool isPaused)
    {
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReturnBackToMenu()
    {
        SettingsPanel.SetActive(false);
        controlPanel.SetActive(false);
        menu.SetActive(true);
    }

    public void ControlBtn()
    {
        controlPanel.SetActive(true);
        SettingsPanel.SetActive(false);
        menu.SetActive(false);
    }

    public void SettingsBtn()
    {
        SettingsPanel.SetActive(true);
        controlPanel.SetActive(false);
        menu.SetActive(false);
    }

    public void ExitBtn()
    {
        PlayerPrefs.DeleteAll();

        HandleGamePause(false);

        SceneManager.LoadScene("Main Menu");
    }
}
