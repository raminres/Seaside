using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "StringEvent", menuName = "Game/String Event")]
public class StringEventSo : ScriptableObject
{
    public event System.Action<string> OnEventRaised;  // Changed from UnityAction to event Action

    public void RaiseEvent(string value)
    {
        OnEventRaised?.Invoke(value);
    }
}