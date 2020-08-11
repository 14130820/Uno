using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FeatherWorks.Pooling
{
    [DisallowMultipleComponent]
    [AddComponentMenu("FeatherWorks/Pooling/Feather Pool Manager")]
    [Serializable, HideMonoScript]
    public class FeatherPoolManager : MonoBehaviour
    {
        #region Singleton Instance
        /// <summary>
        /// Gets the HUDManager singleton instance.
        /// </summary>
        public static FeatherPoolManager Instance
        {
            get
            {
                if (!hasInstance)
                {
                    instance = (FeatherPoolManager)FindObjectOfType(typeof(FeatherPoolManager));
                    if (instance == null)
                    {
                        Debug.LogError("[FeatherPoolManager] Cannot find an instance of HUDManager!");
                        return null;
                    }
                    hasInstance = true;
                }
                return instance;
            }
        }
        private static FeatherPoolManager instance;
        private static bool hasInstance = false;
        #endregion

        #region Settings Struct
        // ----------------------------------------------------------------------------------------------------
        [Serializable]
        [InlineProperty, HideLabel]
        public struct PoolSettings
        {
            [Tooltip("Whether or not to share all pools between scenes")]
            public bool KeepBetweenScenes;

            [Tooltip("Automatically resizes the pool if more items are requested than available.")]
            public bool AutoGrowPools;

            [Tooltip("Parents all FeatherPool Groups to the manager upon startup.")]
            public bool ParentGroups;
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region Fields & Properties
        // ----------------------------------------------------------------------------------------------------
        // Global Settings
        [BoxGroup("Settings")]
        public PoolSettings GlobalSettings;

        /// <summary>
        /// The dictionary containing all pooled instances and prefabs.
        /// </summary>
        private Dictionary<int, FeatherPool> PoolDictionary = new Dictionary<int, FeatherPool>();

        /// <summary>
        /// The dictionary containing all pooled instances and prefabs.
        /// </summary>
        private Dictionary<string, FeatherPool> NamedPoolDictionary = new Dictionary<string, FeatherPool>();

        // Read-Only Pool List
#if UNITY_EDITOR
        [ShowIf("IsPoolGroupVisible")]
        [LabelText("Pooled Objects")]
        private List<FeatherPool> PoolListReadOnly = new List<FeatherPool>();

        private bool IsPoolGroupVisible()
        {
            return (UnityEditor.EditorApplication.isPlaying);
        }
#endif
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region Pool Registration
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Registers a new pool group.
        /// </summary>
        /// <param name="featherPoolGroup">The feather pool group.</param>
        public void RegisterPool(FeatherPoolGroup featherPoolGroup)
        {
            // Re-parent Pool to Manager
            if (GlobalSettings.ParentGroups)
            {
                if (featherPoolGroup.transform.parent != this.transform)
                {
                    featherPoolGroup.transform.SetParent(this.transform);
                }
            }
        }

        /// <summary>
        /// Registers a new pool.
        /// </summary>
        /// <param name="featherPool">The pool.</param>
        public void RegisterPool(FeatherPool featherPool)
        {
            //Debug.LogWarning(String.Format("Registering Pool '{0}'.", featherPool.name), featherPool.gameObject);

            int itemId = featherPool.PrefabID;

            // Check for duplicates
            if (PoolDictionary.ContainsKey(itemId))
            {
                Debug.LogError(String.Format("An instance of '<b>{0}</b>' already exists in Pool '<b>{1}</b>', skipping instance.", featherPool.PrefabName, featherPool.name));
                return;
            }

            PoolDictionary.Add(featherPool.PrefabID, featherPool);
            NamedPoolDictionary.Add(featherPool.PrefabName, featherPool);

#if UNITY_EDITOR
            PoolListReadOnly.Add(featherPool);
#endif

            //Debug.LogWarning(String.Format("Registered Pool '{0}' with {1} Pool Items.", featherPool.name, featherPool.TotalInstances), featherPool.gameObject);
        }

        /// <summary>
        /// Registers a new pool instance.
        /// </summary>
        public void RegisterInstance(GameObject poolObject, FeatherPool pool)
        {
            PoolDictionary.Add(poolObject.GetInstanceID(), pool);
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region Pool Handling
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the pool holding the specified object.
        /// </summary>
        public FeatherPool GetPool(string objectName)
        {
            FeatherPool featherPool = null;
            NamedPoolDictionary.TryGetValue(objectName, out featherPool);
            return featherPool;
        }

        /// <summary>
        /// Get the pool holding the specified object.
        /// </summary>
        public FeatherPool GetPool(int objectID)
        {
            FeatherPool featherPool = null;
            PoolDictionary.TryGetValue(objectID, out featherPool);
            return featherPool;
        }

        /// <summary>
        /// Get the pool holding the specified object.
        /// </summary>
        public FeatherPool GetPool(GameObject gameObject)
        {
            FeatherPool featherPool = null;
            PoolDictionary.TryGetValue(gameObject.GetInstanceID(), out featherPool);
            return featherPool;
        }

        #region Contains Methods
        /// <summary>
        /// Determines whether pool contains the specified game object.
        /// </summary>
        /// <param name="prefabTransform">The game object.</param>
        /// <returns><c>true</c> if pool contains the specified game object; otherwise, <c>false</c>.</returns>
        public bool Contains(Transform prefabTransform)
        {
            return Contains(prefabTransform.gameObject);
        }

        /// <summary>
        /// Determines whether pool contains the specified game object.
        /// </summary>
        /// <param name="prefabObject">The game object.</param>
        /// <returns><c>true</c> if pool contains the specified game object; otherwise, <c>false</c>.</returns>
        public bool Contains(GameObject prefabObject)
        {
            return PoolDictionary.ContainsKey(prefabObject.GetInstanceID());
        }
        #endregion

        /// <summary>
        /// Gets the prefab of this instance.
        /// </summary>
        public GameObject GetPrefab(GameObject prefabObject)
        {
            return GetPrefabByID(prefabObject.GetInstanceID());
        }

        /// <summary>
        /// Gets the prefab of this instance.
        /// </summary>
        public GameObject GetPrefabByID(int prefabID)
        {
            return PoolDictionary[prefabID].PrefabObject;
        }

        /// <summary>
        /// This method will tell you how many different items are set up in FeatherPool.
        /// </summary>
        public int PoolSize
        {
            get
            {
                return PoolDictionary.Count;
            }
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region Spawning Methods
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        /// <param name="poolTransform">The pool transform.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <returns>Transform.</returns>
        public FeatherPoolInstance Spawn(Transform poolTransform, Vector3 position, Quaternion rotation)
        {
            return Spawn(poolTransform.gameObject, position, rotation);
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(int poolObjectId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            FeatherPool itemPool = GetPool(poolObjectId);
            if (itemPool == null)
            {
                Debug.LogError(String.Format("The object ID '{0}' passed is not in the FeatherPool.", poolObjectId));
                return null;
            }

            // Create a new instance
            var poolInstance = itemPool.Spawn(position, rotation, scale);

            if (poolInstance == null)
            {
                Debug.LogWarning(String.Format("One or more of the prefab ID '{0}' in FeatherPool has been destroyed. You should never destroy objects in the Pool. Despawn instead. Not spawning anything for this call.", poolObjectId));
                return null;
            }

            return poolInstance;
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(GameObject poolObject, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (poolObject == null)
            {
                Debug.LogError("No Transform passed to Spawn method.", poolObject.gameObject);
                return null;
            }

            return Spawn(poolObject.GetInstanceID(), position, rotation, scale);
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(int poolObjectId, Vector3 position, Quaternion rotation)
        {
            return Spawn(poolObjectId, position, rotation, Vector3.one);
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(GameObject poolObject, Vector3 position, Quaternion rotation)
        {
            return Spawn(poolObject, position, rotation, Vector3.one);
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(int poolObjectId, Vector3 position)
        {
            return Spawn(poolObjectId, position, Quaternion.identity, Vector3.one);
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(GameObject poolObject, Vector3 position)
        {
            return Spawn(poolObject, position, Quaternion.identity, Vector3.one);
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(int poolObjectId, Vector3 position, Vector3 scale)
        {
            return Spawn(poolObjectId, position, Quaternion.identity, scale);
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(GameObject poolObject, Vector3 position, Vector3 scale)
        {
            return Spawn(poolObject, position, Quaternion.identity, scale);
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(int poolObjectId)
        {
            FeatherPool itemPool = GetPool(poolObjectId);
            if (itemPool == null)
            {
                Debug.LogError(String.Format("The object ID '{0}' passed is not in the FeatherPool.", poolObjectId));
                return null;
            }

            // Create a new instance
            var poolInstance = itemPool.Spawn();

            if (poolInstance == null)
            {
                Debug.LogWarning(String.Format("One or more of the prefab ID '{0}' in FeatherPool has been destroyed. You should never destroy objects in the Pool. Despawn instead. Not spawning anything for this call.", poolObjectId));
                return null;
            }

            return poolInstance;
        }

        /// <summary>
        /// Spawns a new instance of a given prefab.
        /// </summary>
        public FeatherPoolInstance Spawn(GameObject poolObject)
        {
            return Spawn(poolObject, Vector3.zero, Quaternion.identity, Vector3.one);
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region Despawning Methods
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Despawns the specified object.
        /// </summary>
        /// <param name="poolObject">The object to despawn.</param>
        public void Despawn(Transform poolObject)
        {
            Despawn(poolObject.gameObject);
        }

        /// <summary>
        /// Despawns the specified object.
        /// </summary>
        /// <param name="poolObjectId">The object to despawn.</param>
        public void Despawn(int poolObjectId)
        {
            FeatherPool featherPool = GetPool(poolObjectId);
            if (featherPool == null)
            {
                Debug.LogError(String.Format("The object ID '{0}' passed is not in the FeatherPool.", poolObjectId));
                return;
            }

            // Despawn object
            featherPool.Despawn(poolObjectId);

            //FeatherLogManager.LogNotice(Instance.gameObject, String.Format("FeatherPool despawned '{0}' at {1}", itemName, Time.time), poolObject.gameObject);
        }

        /// <summary>
        /// Despawns the specified object.
        /// </summary>
        /// <param name="poolObject">The object to despawn.</param>
        public void Despawn(GameObject poolObject)
        {
            Despawn(poolObject.GetInstanceID());
        }

        /// <summary>
        /// Clears this FeatherPool Manager by despawning all managed objects.
        /// </summary>
        public void Clear()
        {
            foreach (var poolItem in PoolDictionary)
            {
                poolItem.Value.DespawnAll();
            }
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion
    }
}