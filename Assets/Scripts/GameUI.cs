using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameEventSo onGamePauseEvent;
    [SerializeField] private GameEventSo onGameResumeEvent;
    [SerializeField] private GameObject pauseMenuUI;

    private void OnEnable()
    {
        onGamePauseEvent.OnEventRaised += ShowPauseMenu;
        onGameResumeEvent.OnEventRaised += HidePauseMenu;
    }

    private void OnDisable()
    {
        onGamePauseEvent.OnEventRaised -= ShowPauseMenu;
        onGameResumeEvent.OnEventRaised -= HidePauseMenu;
    }

    private void ShowPauseMenu()
    {
        pauseMenuUI.SetActive(true);
    }

    private void HidePauseMenu()
    {
        pauseMenuUI.SetActive(false);
    }
}