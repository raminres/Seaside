using UnityEngine;

namespace Seaside.Core
{
    /// <summary>
    /// Lightweight singleton base class for managers.
    /// Survives scene loads and prevents duplicates.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        /// <summary>
        /// Override to return false if this singleton should not persist between scenes.
        /// </summary>
        protected virtual bool Persistent => true;

        protected virtual void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as T;

            if (Persistent)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
