using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace FeatherWorks.Pooling
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent()]
    [Serializable, HideMonoScript]
    public class FeatherPool : MonoBehaviour
    {
        #region Fields & Properties
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the pool group.
        /// </summary>
        /// <value>The pool group.</value>
        public FeatherPoolGroup Group { get; set; }

        /// <summary>
        /// Gets the pool item.
        /// </summary>
        public FeatherPoolEntry PoolEntry { get; set; }

        /// <summary>
        /// Gets the prefab object.
        /// </summary>
        public GameObject PrefabObject { get { return this.PoolEntry.PrefabObject; } }

        /// <summary>
        /// Gets the name of the prefab.
        /// </summary>
        /// <value>The name of the prefab.</value>
        public string PrefabName
        {
            get { return this.PrefabObject.name; }
        }

        /// <summary>
        /// Gets the prefab identifier.
        /// </summary>
        /// <value>The prefab identifier.</value>
        public int PrefabID
        {
            get { return this.PrefabObject.GetInstanceID(); }
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        public Transform Parent
        {
            get { return this.transform; }
        }

        /// <summary>
        /// Gets the next available instance.
        /// </summary>
        public FeatherPoolInstance NextAvailableInstance
        {
            get
            {
                if (!HasInstancesLeft)
                {
                    if (!FeatherPoolManager.Instance.GlobalSettings.AutoGrowPools)
                    {
                        Debug.LogError(String.Format("The Transform '{0}' has no available clones left to Spawn in the FeatherPool. Please increase your Preload Quantity.", this.PrefabObject.name), this.PrefabObject);
                        return null;
                    }
                    else
                    {
                        // Instantiate five more objects
                        AddNewInstances(5);

                        Debug.LogWarning(String.Format("Instantiated an extra five '{0}' because there were none left in the Pool.", this.PrefabObject.name), this.PrefabObject);
                    }
                }

                FeatherPoolInstance poolInstance = this.availableInstances.Pop();
                this.activeInstances.Add(poolInstance);

#if UNITY_EDITOR
                UpdateProgressBar();
#endif

                return poolInstance;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has instances left.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this pool has instances left; otherwise, <c>false</c>.
        /// </value>
        public bool HasInstancesLeft
        {
            get { return this.availableInstances.Count > 0; }
        }

        /// <summary>
        /// Gets the number of available instances.
        /// </summary>
        public int AvailableInstances
        {
            get { return this.availableInstances.Count; }
        }

        /// <summary>
        /// Gets the number of available instances.
        /// </summary>
        public int ActiveInstances
        {
            get { return this.activeInstances.Count; }
        }

        /// <summary>
        /// Gets the number of total instances.
        /// </summary>
        public int TotalInstances
        {
            get { return this.instanceDictionary.Count; }
        }

        // Private Fields
        private Stack<FeatherPoolInstance> availableInstances;
        private HashSet<FeatherPoolInstance> activeInstances = new HashSet<FeatherPoolInstance>();
        private Dictionary<int, FeatherPoolInstance> instanceDictionary;
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region Initialization
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            // Initialize collections
            int capacity = PoolEntry.PoolSize;
            availableInstances = new Stack<FeatherPoolInstance>(capacity);
            instanceDictionary = new Dictionary<int, FeatherPoolInstance>(capacity);

            PrewarmInstances();
            PrewarmHashSet();
        }

        /// <summary>
        /// Prewarms the hash set.
        /// </summary>
        private void PrewarmHashSet()
        {
            foreach (FeatherPoolInstance instance in availableInstances)
            {
                activeInstances.Add(instance);
            }
            activeInstances.Clear();
        }

        /// <summary>
        /// Prewarms the instances.
        /// </summary>
        private void PrewarmInstances()
        {
            //Debug.LogWarning(String.Format("[FeatherPool] Preloading Pool Item {0} with {1} instances.", PrefabObject.name, PoolEntry.PoolSize));
            AddNewInstances(PoolEntry.PoolSize);

#if UNITY_EDITOR
            UpdateProgressBar();
#endif
        }

        /// <summary>
        /// Instantiates the new object.
        /// </summary>
        /// <returns></returns>
        public GameObject InstantiateNewObject()
        {
            GameObject clonedObject = GameObject.Instantiate<GameObject>(PrefabObject);
            clonedObject.name = PrefabObject.name;

            FeatherPoolManager.Instance.RegisterInstance(clonedObject, this);

            return clonedObject;
        }

        /// <summary>
        /// Adds a new instance.
        /// </summary>
        private void AddNewInstances(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                AddNewInstance(InstantiateNewObject());
            }
        }

        /// <summary>
        /// Adds a new instance.
        /// </summary>
        private void AddNewInstance()
        {
            AddNewInstance(InstantiateNewObject());
        }

        /// <summary>
        /// Adds a new instance.
        /// </summary>
        /// <param name="newInstance">The new instance.</param>
        private void AddNewInstance(GameObject newInstance)
        {
            Transform instanceTransform = newInstance.transform;
            instanceTransform.name = PrefabObject.name;
            instanceTransform.SetParent(this.transform);
            newInstance.SetActive(false);

            FeatherPoolInstance newItemInstance = newInstance.AddComponent<FeatherPoolInstance>();
            newItemInstance.Pool = this;

            availableInstances.Push(newItemInstance);

            instanceDictionary.Add(newInstance.GetInstanceID(), newItemInstance);
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region Pool Methods
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the pool contains the specified game object.
        /// </summary>
        /// <param name="pooledObjectID">The game object.</param>
        /// <returns><c>true</c> if pool contains the specified game object; otherwise, <c>false</c>.</returns>
        public bool Contains(int pooledObjectID)
        {
            return this.instanceDictionary.ContainsKey(pooledObjectID);
        }

        /// <summary>
        /// Determines whether the pool contains the specified game object.
        /// </summary>
        /// <param name="pooledObject">The game object.</param>
        /// <returns><c>true</c> if pool contains the specified game object; otherwise, <c>false</c>.</returns>
        public bool Contains(GameObject pooledObject)
        {
            return this.instanceDictionary.ContainsKey(pooledObject.GetInstanceID());
        }

        /// <summary>
        /// Determines whether the specified game object is active.
        /// </summary>
        /// <param name="pooledObjectID">The game object.</param>
        /// <returns><c>true</c> if the specified game object is active; otherwise, <c>false</c>.</returns>
        public bool IsActive(int pooledObjectID)
        {
            FeatherPoolInstance instance = TryGetInstance(pooledObjectID);

            if (instance != null)
            {
                return this.activeInstances.Contains(instance);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified game object is active.
        /// </summary>
        /// <param name="pooledObject">The game object.</param>
        /// <returns><c>true</c> if the specified game object is active; otherwise, <c>false</c>.</returns>
        public bool IsActive(GameObject pooledObject)
        {
            FeatherPoolInstance instance = TryGetInstance(pooledObject);

            if (instance != null)
            {
                return this.activeInstances.Contains(instance);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified game object is available.
        /// </summary>
        /// <param name="pooledObjectID">The game object.</param>
        /// <returns><c>true</c> if the specified game object is available; otherwise, <c>false</c>.</returns>
        public bool IsAvailable(int pooledObjectID)
        {
            FeatherPoolInstance instance = TryGetInstance(pooledObjectID);

            if (instance != null)
            {
                return this.availableInstances.Contains(instance);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified game object is available.
        /// </summary>
        /// <param name="pooledObject">The game object.</param>
        /// <returns><c>true</c> if the specified game object is available; otherwise, <c>false</c>.</returns>
        public bool IsAvailable(GameObject pooledObject)
        {
            FeatherPoolInstance instance = TryGetInstance(pooledObject);

            if (instance != null)
            {
                return this.availableInstances.Contains(instance);
            }

            return false;
        }

        /// <summary>
        /// Tries the get a FeatherPoolInstance containing the GameObject.
        /// </summary>
        /// <param name="pooledObjectID">The game object.</param>
        /// <returns>FeatherPoolInstance.</returns>
        private FeatherPoolInstance TryGetInstance(int pooledObjectID)
        {
            FeatherPoolInstance instance = null;
            this.instanceDictionary.TryGetValue(pooledObjectID, out instance);
            return instance;
        }

        /// <summary>
        /// Tries the get a FeatherPoolInstance containing the GameObject.
        /// </summary>
        /// <param name="pooledObject">The game object.</param>
        /// <returns>FeatherPoolInstance.</returns>
        private FeatherPoolInstance TryGetInstance(GameObject pooledObject)
        {
            FeatherPoolInstance instance = null;
            this.instanceDictionary.TryGetValue(pooledObject.GetInstanceID(), out instance);
            return instance;
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region Spawning Methods
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Spawns a new instance.
        /// </summary>
        public FeatherPoolInstance Spawn(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // Fetch next available instance
            FeatherPoolInstance poolInstance = NextAvailableInstance;
            if (poolInstance == null) { return null; }

            // Signal if implementing
            //poolInstance.InvokeOnSpawning();

            //// Set Transform
            //poolInstance.SetPosition(position);
            //poolInstance.SetRotation(rotation);
            //poolInstance.SetScale(scale);

            //// Signal if implementing
            //poolInstance.InvokeResettable();

            //// Signal if implementing
            //poolInstance.InvokeOnSpawned();

            // Activate instance
            poolInstance.gameObject.SetActive(true);

            // Return instance
            return poolInstance;
        }

        /// <summary>
        /// Spawns a new instance.
        /// </summary>
        public FeatherPoolInstance Spawn(Vector3 position, Quaternion rotation)
        {
            return Spawn(position, rotation, Vector3.one);
        }

        /// <summary>
        /// Spawns a new instance.
        /// </summary>
        public FeatherPoolInstance Spawn(Vector3 position, Vector3 scale)
        {
            return Spawn(position, Quaternion.identity, scale);
        }

        /// <summary>
        /// Spawns a new instance.
        /// </summary>
        public FeatherPoolInstance Spawn(Vector3 position)
        {
            return Spawn(position, Quaternion.identity, Vector3.one);
        }

        /// <summary>
        /// Spawns a new instance.
        /// </summary>
        public FeatherPoolInstance Spawn()
        {
            // Fetch next available instance
            FeatherPoolInstance poolInstance = NextAvailableInstance;
            if (poolInstance == null) { return null; }
            
            // Activate instance
            poolInstance.gameObject.SetActive(true);

            // Return instance
            return poolInstance;
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region Despawn Methods
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Despawns and returns an instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void Despawn(FeatherPoolInstance instance)
        {
            // Check if object already despwned
            if (!this.activeInstances.Contains(instance)) { return; }

            // Remove instance from active instances and makes it available
            this.activeInstances.Remove(instance);
            this.availableInstances.Push(instance);

            // Signal if implementing
            //instance.InvokeOnDespawning();

            // Reset Object
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(this.transform);
            //instance.ResetTransform();

            // Signal if implementing
            //instance.InvokeOnDespawning();

#if UNITY_EDITOR
            UpdateProgressBar();
#endif
        }

        /// <summary>
        /// Despawns and returns an instance.
        /// </summary>
        /// <param name="objectTransform">The transform.</param>
        public void Despawn(Transform objectTransform)
        {
            Despawn(transform.gameObject);
        }

        /// <summary>
        /// Despawns and returns an instance.
        /// </summary>
        /// <param name="gameObjectID">The gameObject.</param>
        public void Despawn(int gameObjectID)
        {
            FeatherPoolInstance instance = TryGetInstance(gameObjectID);

            if (instance != null)
            {
                this.Despawn(instance);
            }
            else
            {
                Debug.LogError("[FeatherPool] Tried despawning object <b>" + gameObject.name + "</b>, but object not part of this pool.");
            }
        }

        /// <summary>
        /// Despawns and returns an instance.
        /// </summary>
        /// <param name="pooledObject">The gameObject.</param>
        public void Despawn(GameObject pooledObject)
        {
            FeatherPoolInstance instance = TryGetInstance(pooledObject);

            if (instance != null)
            {
                this.Despawn(instance);
            }
            else
            {
                Debug.LogError("[FeatherPool] Tried despawning object <b>" + gameObject.name + "</b>, but object not part of this pool.");
            }
        }

        /// <summary>
        /// Despawns all.
        /// </summary>
        public void DespawnAll()
        {
            if (this.activeInstances.Count > 0)
            {
                foreach (var instance in this.activeInstances)
                {
                    Despawn(instance);
                }
            }
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region DataChanged Interface
        // ----------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
        [ShowInInspector]
        [LabelText("Active Instances")]
        [HideReferenceObjectPicker]
        private ProgressBar activeInstancesBar = new ProgressBar();

        private void KillLast()
        {
            Despawn(this.activeInstances.First());
        }

        private void KillAll()
        {
            DespawnAll();
        }

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        private void UpdateProgressBar()
        {
            activeInstancesBar.Value = ActiveInstances;
            activeInstancesBar.Maximum = TotalInstances;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        // ----------------------------------------------------------------------------------------------------
        #endregion

        #region ToString Override
        // ----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
#if UNITY_EDITOR
            //return String.Format("{0} [{1}]", this.PrefabName, this.activeInstancesBar.ToString());
            return this.PrefabName;
#else
			return String.Empty;
#endif
        }
        // ----------------------------------------------------------------------------------------------------
        #endregion
    }
}