using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private WorldMenuPanel pausePanel;

    private bool isPaused;

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetInstantHiddenAbove();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void ResetScene()
    {
        if (isPaused)
        {
            TogglePause();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Resume()
    {
        if (!isPaused) return;

        TogglePause();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void TogglePause()
    {
        if (pausePanel != null && pausePanel.IsMoving) return;

        isPaused = !isPaused;

        if (pausePanel != null)
        {
            if (isPaused)
            {
                pausePanel.ShowFromAbove();
            }
            else
            {
                pausePanel.HideUp();
            }
        }

        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }
}
