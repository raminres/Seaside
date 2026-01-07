using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent", menuName = "Game/Game Event")]
public class GameEventSo : ScriptableObject
{
    public event System.Action OnEventRaised;  // Changed from UnityAction to event Action

    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}