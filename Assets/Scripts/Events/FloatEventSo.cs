using UnityEngine;

[CreateAssetMenu(fileName = "FloatEvent", menuName = "Game/Float Event")]
public class FloatEventSo : ScriptableObject
{
    public event System.Action<float> OnEventRaised;

    public void RaiseEvent(float value)
    {
        OnEventRaised?.Invoke(value);
    }
}