using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public Menu optionsMenu;
    public GameObject optionsScreen;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                if (!optionsScreen.activeSelf)
                {
                    Resume();
                }
                else
                {
                    MenuManager.Instance.OpenMenu("pause");
                    MenuManager.Instance.CloseMenu(optionsMenu);
                }
            }
            else Pause();
            
        }
    }

    public void Resume ()
    {
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
    }
    void Pause ()
    {
        pauseMenuUI.SetActive(true);
        GameIsPaused = true;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
    }

    public void LoadMenu()
    {
        Destroy(RoomManager.Instance.gameObject);
        StartCoroutine(Launcher.Instance.DisconnectAndLoad());
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
