using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FeatherWorks.Pooling
{
    [AddComponentMenu("FeatherWorks/Pooling/Feather Pool")]
	[Serializable, HideMonoScript]
	public class FeatherPoolGroup : MonoBehaviour {
		#region Editor Interface
		// ----------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
		[OnInspectorGUI]
		[PropertyOrder(-1)]
		public void EditorDrag() {
			GUILayout.Space(5);

			//EditorResources.DrawTexture(EditorResources.TextureLogoFeatherWorks);

			if (!EditorApplication.isPlaying) {
				var dragBoxStyle = new GUIStyle("Box");
				dragBoxStyle.alignment = TextAnchor.MiddleCenter;
				dragBoxStyle.normal.textColor = Color.gray;
				GUILayout.Label("Drag and Drop a prefab into this box\r\nto create a new pool item.", dragBoxStyle, GUILayout.Height(50), GUILayout.ExpandWidth(true));
				var boxRect = GUILayoutUtility.GetLastRect();

				EventType eventType = Event.current.type;
				if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform) {
					var validObject = true;

					// Check dragged objects type
					for (int i = 0; i < DragAndDrop.objectReferences.Length; i++) {
						if (DragAndDrop.objectReferences[i].GetType() != typeof(GameObject)) {
							validObject = false;
						}
					}

					// Check mouse position
					if (!boxRect.Contains(Event.current.mousePosition)) {
						validObject = false;
					}

					// Show a copy icon on the drag
					if (validObject) {
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

						if (eventType == EventType.DragPerform) {
							for (int i = 0; i < DragAndDrop.objectReferences.Length; i++) {
								GameObject prefabObject = DragAndDrop.objectReferences[i] as GameObject;

								if (!HasPrefab(prefabObject)) {
									PoolEntries.Add(new FeatherPoolEntry(prefabObject));
								}
								else {
									EditorUtility.DisplayDialog("Object already exists", "Yo", "Ok");
								}
							}
							DragAndDrop.AcceptDrag();
						}

						Event.current.Use();
					}
				}
			}
		}
#endif
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Settings Struct
		// ----------------------------------------------------------------------------------------------------
		[Serializable]
		[InlineProperty, HideLabel]
		public struct PoolSettings {
			[Tooltip("Whether or not to share this pools between scenes")]
			public bool KeepBetweenScenes;

			public bool OverridePoolSize;

			[MinValue(2)]
			[ShowIf("IsPoolSizeVisible")]
			public int PoolSize;

			private bool IsPoolSizeVisible() {
				return this.OverridePoolSize;
			}
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Fields & Properties
		// ----------------------------------------------------------------------------------------------------
		// Settings
		[DisableInPlayMode]
		[BoxGroup("Settings")]
		public PoolSettings Settings = new PoolSettings();

		// Pool
		[ShowIf("IsPoolEntriesVisible")]
		[HideInPlayMode]
		[ListDrawerSettings(Expanded = true, HideAddButton = true)]
		public List<FeatherPoolEntry> PoolEntries = new List<FeatherPoolEntry>();

		/// <summary>
		/// Gets the pool list.
		/// </summary>
		public Dictionary<String, FeatherPool> PoolList { get; private set; }

		// Read-Only Pool List
#if UNITY_EDITOR
		[HideInEditorMode]
		[LabelText("Pooled Objects")]
		private List<FeatherPool> PoolListReadOnly;

		private bool IsPoolEntriesVisible() {
			return PoolEntries != null && PoolEntries.Count > 0;
		}
#endif
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Initialization
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Awakens this instance.
		/// </summary>
		private void Awake() {
			if (Settings.KeepBetweenScenes || FeatherPoolManager.Instance.GlobalSettings.KeepBetweenScenes) {
				GameObject.DontDestroyOnLoad(this);
			}

			PoolList = new Dictionary<string, FeatherPool>(PoolEntries.Count);

#if UNITY_EDITOR
			List<FeatherPoolEntry> emptyEntries = new List<FeatherPoolEntry>();
#endif

			// Go through each entry, verify, create and register pools
			foreach (FeatherPoolEntry poolEntry in PoolEntries) {
				if (poolEntry.PoolSize <= 0) {
					continue;
				}

				if (poolEntry.PrefabObject == null) {
					Debug.LogWarning(String.Format("You have an item in FeatherPool '{0}' with no prefab assigned.", this.name), this.gameObject);
#if UNITY_EDITOR
					emptyEntries.Add(poolEntry);
#endif
					continue;
				}

				AddNewPool(poolEntry);
			}

#if UNITY_EDITOR
			// If inside the Editor, remove empty entries to hide them from the inspector
			foreach (FeatherPoolEntry poolEntry in emptyEntries) {
				PoolEntries.Remove(poolEntry);
			}

			// Copy to ReadOnly List
			PoolListReadOnly = new List<FeatherPool>();
			foreach (var poolEntry in PoolList) {
				PoolListReadOnly.Add(poolEntry.Value);
			}

#else
			PoolEntries.Clear();
#endif

			FeatherPoolManager.Instance.RegisterPool(this);
		}

		/// <summary>
		/// Adds the new pool.
		/// </summary>
		/// <param name="entry">The pool entry.</param>
		private void AddNewPool(FeatherPoolEntry entry) {
			// Check if object already in a pool
			if (FeatherPoolManager.Instance.Contains(entry.PrefabObject)) {
				Debug.LogError(String.Format("An instance of '<b>{0}</b>' already exists in a Pool, skipping object.", entry.PrefabObject.name));
				return;
			}

			GameObject newPool = new GameObject(String.Format("Pool ({0})", entry.PrefabObject.name));
			FeatherPool featherPool = newPool.AddComponent<FeatherPool>();
			featherPool.transform.SetParent(this.transform);
			featherPool.PoolEntry = entry;
			featherPool.PoolEntry.ApplySettings(this.Settings);
			featherPool.Group = this;
			featherPool.Initialize();

			PoolList.Add(entry.PrefabObject.name, featherPool);

			FeatherPoolManager.Instance.RegisterPool(featherPool);
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Pool Methods
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Determines whether the specified prefab has prefab.
		/// </summary>
		/// <param name="prefab">The prefab.</param>
		private bool HasPrefab(GameObject prefab) {
			foreach (FeatherPoolEntry setting in PoolEntries) {
				if (setting.PrefabObject.name.Equals(prefab.name)) { return true; }
			}
			return false;
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion
	}
}