using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "StringEvent", menuName = "Game/String Event")]
public class StringEventSo : ScriptableObject
{
    public UnityAction<string> OnEventRaised;

    public void RaiseEvent(string value)
    {
        OnEventRaised?.Invoke(value);
    }
}
