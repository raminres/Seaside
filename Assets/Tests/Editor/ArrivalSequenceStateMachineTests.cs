using NUnit.Framework;

public class ArrivalSequenceStateMachineTests
{
    [Test]
    public void FreshArrival_OnlyAllowsDisembarkAfterDocking()
    {
        var sequence = new ArrivalSequenceStateMachine();

        Assert.That(sequence.Begin(arrivalAlreadyComplete: false), Is.True);
        Assert.That(sequence.CanDisembark, Is.False);
        Assert.That(sequence.Disembark(), Is.False);

        Assert.That(sequence.Dock(), Is.True);
        Assert.That(sequence.CanDisembark, Is.True);
        Assert.That(sequence.Disembark(), Is.True);
        Assert.That(sequence.State, Is.EqualTo(ArrivalSequenceState.Disembarked));
        Assert.That(sequence.IsComplete, Is.True);
    }

    [Test]
    public void CompletedArrival_SkipsApproachAndCannotReplayIt()
    {
        var sequence = new ArrivalSequenceStateMachine();

        Assert.That(sequence.Begin(arrivalAlreadyComplete: true), Is.False);
        Assert.That(sequence.State, Is.EqualTo(ArrivalSequenceState.Skipped));
        Assert.That(sequence.IsComplete, Is.True);
        Assert.That(sequence.Begin(arrivalAlreadyComplete: false), Is.False);
        Assert.That(sequence.Dock(), Is.False);
    }

    [Test]
    public void Docking_IsOneShot()
    {
        var sequence = new ArrivalSequenceStateMachine();
        sequence.Begin(arrivalAlreadyComplete: false);

        Assert.That(sequence.Dock(), Is.True);
        Assert.That(sequence.Dock(), Is.False);
        Assert.That(sequence.State, Is.EqualTo(ArrivalSequenceState.Docked));
    }
}
