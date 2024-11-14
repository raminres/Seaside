using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BoatLoader : MonoBehaviour
{
    public string boatPrefabAddress = "BoatPrefabAddress"; // Address of the boat prefab in Addressables
    public Transform spawnPoint; // Empty GameObject's transform for positioning

    private void Start()
    {
        LoadBoat();
    }

    private void LoadBoat()
    {
        Addressables.LoadAssetAsync<GameObject>(boatPrefabAddress).Completed += OnBoatLoaded;
    }

    private void OnBoatLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject boatInstance = Instantiate(handle.Result, spawnPoint.position, spawnPoint.rotation);
            boatInstance.transform.parent = spawnPoint; // Optional: parent it to the spawn point
        }
        else
        {
            Debug.LogError("Failed to load boat prefab.");
        }
    }
}