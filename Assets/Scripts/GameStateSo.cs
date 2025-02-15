using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "Game/Game State")]
public class GameStateSo : ScriptableObject
{
    public GameState CurrentState;
}

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}