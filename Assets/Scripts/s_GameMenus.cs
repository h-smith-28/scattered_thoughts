using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class s_GameMenus : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject losePanel;
    public GameObject winPanel;
    s_ComputerStation[] stations;
    s_PlayerCamera playerCamera;

    bool isPaused;
    bool gameOver;

    void Start()
    {
        stations = FindObjectsByType<s_ComputerStation>();
        playerCamera = FindAnyObjectByType<s_PlayerCamera>();
    }   

    void Update()
    {
        if (gameOver) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                foreach (var station in stations)
                    if (station.IsBusy) return;
                if (isPaused) Resume();
                else Pause();
            }
    }
    
    public void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Freeze(true);
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Freeze(false);
    }

    void Freeze(bool on)
    {
        Time.timeScale = on ? 0f : 1f;
        Cursor.lockState = on ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = on;
        if (playerCamera != null)
            playerCamera.enabled = !on;
    }
    
    public void ShowLose() => EndGame(losePanel);
    public void ShowWin() => EndGame(winPanel);

    void EndGame(GameObject panel)
    {
        if (gameOver) return;
        gameOver = true;
        pausePanel.SetActive(false);
        panel.SetActive(true);
        Freeze(true);
    }
    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}