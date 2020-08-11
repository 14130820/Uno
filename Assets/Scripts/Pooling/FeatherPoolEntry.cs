using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace FeatherWorks.Pooling {
	[Serializable, HideMonoScript]
	public class FeatherPoolEntry {
		#region Fields & Properties
		// ----------------------------------------------------------------------------------------------------
		[AssetsOnly]
		[HorizontalGroup("Prefabs", LabelWidth = 90, PaddingLeft = 10)]
		public GameObject PrefabObject;

		[MinValue(5)]
		[HorizontalGroup("Prefabs", LabelWidth = 40, PaddingLeft = 5f)]
		[SuffixLabel("Instances", true)]
		public int PoolSize = 5;
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Initialization
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="FeatherPoolEntry"/> class.
		/// </summary>
		public FeatherPoolEntry() : this(null, 10) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FeatherPoolEntry"/> class.
		/// </summary>
		/// <param name="prefabObject">The prefab object.</param>
		public FeatherPoolEntry(GameObject prefabObject) : this(prefabObject, 10) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FeatherPoolEntry"/> class.
		/// </summary>
		/// <param name="prefabTransform">The prefab transform.</param>
		/// <param name="instancesToPreload">The instances to preload.</param>
		public FeatherPoolEntry(GameObject prefabTransform, int instancesToPreload) {
			PrefabObject = prefabTransform;
			PoolSize = instancesToPreload;
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Public Methods
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Applies the settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public void ApplySettings(FeatherPoolGroup.PoolSettings settings) {
			if (settings.OverridePoolSize) {
				this.PoolSize = settings.PoolSize;
			}
		} 
		  // ----------------------------------------------------------------------------------------------------
		#endregion

		#region ToString
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		public override string ToString() {
			if (this.PrefabObject == null) { return "[Empty]"; }
			//return String.Format("{0} ({1:X})", this.PrefabObject.name, this.PrefabObject.GetInstanceID());
			return String.Format("{0} ({1})", this.PrefabObject.name, this.PoolSize);
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion
	}
}