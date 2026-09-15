using NUnit.Framework;

public class JourneyProgressStoreTests
{
    [Test]
    public void CompleteAction_IsOneShotAndRetainsStableId()
    {
        var store = new JourneyProgressStore();

        Assert.That(store.CompleteAction(JourneyActionIds.ArrivalComplete), Is.True);
        Assert.That(store.CompleteAction(JourneyActionIds.ArrivalComplete), Is.False);
        Assert.That(store.HasCompletedAction(JourneyActionIds.ArrivalComplete), Is.True);
        Assert.That(store.Data.completedActionIds, Has.Count.EqualTo(1));
    }

    [Test]
    public void ProgressSurvivesStoreReconstruction()
    {
        var firstStore = new JourneyProgressStore();
        firstStore.SetCheckpoint("lighthouse_landing", 18.5f);
        firstStore.DiscoverNote("note_keeper_letter");
        firstStore.AddInventoryItem("matches");

        var restoredStore = new JourneyProgressStore(firstStore.Data);

        Assert.That(restoredStore.Data.checkpointId, Is.EqualTo("lighthouse_landing"));
        Assert.That(restoredStore.Data.timeOfDay, Is.EqualTo(18.5f));
        Assert.That(restoredStore.HasDiscoveredNote("note_keeper_letter"), Is.True);
        Assert.That(restoredStore.HasInventoryItem("matches"), Is.True);
    }

    [Test]
    public void NewJourneyClearsAllRouteState()
    {
        var store = new JourneyProgressStore();
        store.CompleteAction(JourneyActionIds.ArrivalComplete);
        store.SetCheckpoint("lighthouse_landing", 18.5f);
        store.AddInventoryItem("matches");

        store.Reset();

        Assert.That(store.Data.checkpointId, Is.Empty);
        Assert.That(store.Data.completedActionIds, Is.Empty);
        Assert.That(store.Data.inventoryItemIds, Is.Empty);
    }
}
