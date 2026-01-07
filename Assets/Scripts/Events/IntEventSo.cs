using UnityEngine;

[CreateAssetMenu(fileName = "IntEvent", menuName = "Game/Int Event")]
public class IntEventSo : ScriptableObject
{
    public event System.Action<int> OnEventRaised;

    public void RaiseEvent(int value)
    {
        OnEventRaised?.Invoke(value);
    }
}