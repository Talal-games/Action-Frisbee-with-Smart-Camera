using UnityEngine;

public class WorldMenuController : MonoBehaviour
{
    [SerializeField] private WorldMenuPanel pausePanel;
    [SerializeField] private WorldMenuPanel mainMenuPanel;
    [SerializeField] private WorldMenuPanel optionsPanel;

    public void OpenPauseMenu()
    {
        if (pausePanel == null) return;

        pausePanel.ShowFromAbove();
    }

    public void ClosePauseMenu()
    {
        if (pausePanel == null) return;

        pausePanel.HideUp();
    }

    public void OpenMainMenu()
    {
        if (mainMenuPanel == null) return;

        if (optionsPanel != null)
        {
            optionsPanel.SetInstantHiddenAbove();
        }

        mainMenuPanel.ShowFromAbove();
    }

    public void Play()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.HideDown();
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetInstantHiddenAbove();
        }
    }

    public void OpenOptions()
    {
        if (mainMenuPanel == null || optionsPanel == null) return;

        optionsPanel.ShowFromAbove();
        mainMenuPanel.MoveToHiddenBelow();
    }

    public void BackToMainMenu()
    {
        if (mainMenuPanel == null || optionsPanel == null) return;

        mainMenuPanel.ShowFromAbove();
        optionsPanel.MoveToHiddenBelow();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
