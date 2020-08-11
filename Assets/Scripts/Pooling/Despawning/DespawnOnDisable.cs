using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace FeatherWorks.Pooling {
	[Serializable, HideMonoScript]
	[TypeInfoBox("This component despawns this object upon disable.")]
	public class DespawnOnDisable : MonoBehaviour {
		#region Fields & Properties
		// ----------------------------------------------------------------------------------------------------
		private FeatherPool poolGroup;
		private bool hasPool = false;
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Initialization
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Called upon starting this behavior.
		/// </summary>
		private void Start() {
			this.poolGroup = FeatherPoolManager.Instance.GetPool(this.name);
			this.hasPool = poolGroup != null;
		}

		private void OnDisable() {
			if (hasPool) {
				poolGroup.Despawn(this.gameObject);
			}
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion
	}
}
