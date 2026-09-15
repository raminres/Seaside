/// <summary>
/// Pure progression rules for the authored boat arrival. Keeping this outside
/// MonoBehaviour makes the one-shot transition rules testable without a scene.
/// </summary>
public enum ArrivalSequenceState
{
    NotStarted,
    Approaching,
    Docked,
    Disembarked,
    Skipped
}

public sealed class ArrivalSequenceStateMachine
{
    public ArrivalSequenceState State { get; private set; } = ArrivalSequenceState.NotStarted;
    public bool IsComplete => State == ArrivalSequenceState.Disembarked || State == ArrivalSequenceState.Skipped;
    public bool CanDisembark => State == ArrivalSequenceState.Docked;

    public bool Begin(bool arrivalAlreadyComplete)
    {
        if (arrivalAlreadyComplete)
        {
            State = ArrivalSequenceState.Skipped;
            return false;
        }

        if (State != ArrivalSequenceState.NotStarted) return false;

        State = ArrivalSequenceState.Approaching;
        return true;
    }

    public bool Dock()
    {
        if (State != ArrivalSequenceState.Approaching) return false;

        State = ArrivalSequenceState.Docked;
        return true;
    }

    public bool Disembark()
    {
        if (!CanDisembark) return false;

        State = ArrivalSequenceState.Disembarked;
        return true;
    }
}
