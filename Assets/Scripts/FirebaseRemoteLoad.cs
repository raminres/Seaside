using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Firebase;
using Firebase.Extensions;

public class FirebaseRemoteLoad : MonoBehaviour
{
    public string boatPrefabAddress = "BoatPrefabAddress"; // Address of the prefab
    public Transform spawnPoint; // Spawn location for the prefab
    public string remoteCatalogUrl = "gs://seaside-unity.firebasestorage.app/Seaside-Addressables/catalog_2024.11.18.05.09.52.json";

    private void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully.");
                InitializeAddressables();
            }
            else
            {
                Debug.LogError("Could not resolve Firebase dependencies: " + task.Result);
            }
        });
    }

    private void InitializeAddressables()
    {
        Addressables.InitializeAsync().Completed += (initHandle) =>
        {
            if (initHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Addressables initialized successfully.");
                UpdateCatalogAndLoadBoat();
            }
            else
            {
                Debug.LogError("Failed to initialize Addressables.");
            }
        };
    }

    private void UpdateCatalogAndLoadBoat()
    {
        Debug.Log($"Loading remote catalog from: {remoteCatalogUrl}");
        Addressables.LoadContentCatalogAsync(remoteCatalogUrl).Completed += (catalogHandle) =>
        {
            if (catalogHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Remote catalog loaded successfully.");
                LoadBoatFromRemote();
            }
            else
            {
                Debug.LogError("Failed to load remote catalog.");
            }
        };
    }

    private void LoadBoatFromRemote()
    {
        Debug.Log($"Loading prefab: {boatPrefabAddress}");
        Addressables.LoadAssetAsync<GameObject>(boatPrefabAddress).Completed += (prefabHandle) =>
        {
            if (prefabHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Prefab loaded successfully.");
                GameObject boatInstance = Instantiate(prefabHandle.Result, spawnPoint.position, spawnPoint.rotation);
                boatInstance.transform.parent = spawnPoint; // Optional: parent to spawn point
            }
            else
            {
                Debug.LogError($"Failed to load prefab: {boatPrefabAddress}");
            }
        };
    }
}
