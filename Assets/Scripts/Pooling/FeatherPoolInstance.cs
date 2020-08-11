using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace FeatherWorks.Pooling {
	[TypeInfoBox("Holds runtime information for the FeatherPool system.")]
	public class FeatherPoolInstance : MonoBehaviour {
		#region Fields & Properties
		// ----------------------------------------------------------------------------------------------------
		[HideInInspector]
		public FeatherPool Pool { get; set; }

		// Cached Interfaces
		private List<ISpawnable> cachedSpawnables = new List<ISpawnable>(10);
		private List<IDespawnable> cachedDespawnables = new List<IDespawnable>(10);
		private List<IResettable> cachedResettables = new List<IResettable>(10);
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Initialization
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Recaches the component list.
		/// </summary>
		private void Awake() {
			this.GetComponentsInChildren<IResettable>(true, cachedResettables);

			this.GetComponentsInChildren<ISpawnable>(true, cachedSpawnables);

			this.GetComponentsInChildren<IDespawnable>(true, cachedDespawnables);
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Invoke Cached Interfaces
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Invokes OnReset
		/// </summary>
		public void InvokeResettable() {
			float count = cachedResettables.Count;
			for (int i = 0; i < count; i++) {
				cachedResettables[i].ResetState();
			}
		}

		/// <summary>
		/// Invokes OnSpawning
		/// </summary>
		public void InvokeOnSpawning() {
			float count = cachedSpawnables.Count;
			for (int i = 0; i < count; i++) {
				cachedSpawnables[i].OnSpawning();
			}
		}

		/// <summary>
		/// Invokes OnSpawned
		/// </summary>
		public void InvokeOnSpawned() {
			float count = cachedSpawnables.Count;
			for (int i = 0; i < count; i++) {
				cachedSpawnables[i].OnSpawned();
			}
		}

		/// <summary>
		/// Invokes InvokeOnDespawning
		/// </summary>
		public void InvokeOnDespawning() {
			float count = cachedDespawnables.Count;
			for (int i = 0; i < count; i++) {
				cachedDespawnables[i].OnDespawning();
			}
		}

		/// <summary>
		/// Invokes OnDespawned
		/// </summary>
		public void InvokeOnDespawned() {
			float count = cachedDespawnables.Count;
			for (int i = 0; i < count; i++) {
				cachedDespawnables[i].OnDespawned();
			}
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion

		#region Public Transform Methods
		// ----------------------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the transform.
		/// </summary>
		/// <param name="newTransform">The transform.</param>
		/// <param name="applyScale">Whether or not to apply the scale.</param>
		public void SetTransform(Transform newTransform, bool applyScale = true) {
			this.transform.position = newTransform.position;
			this.transform.rotation = newTransform.rotation;
			if (applyScale) {
				this.transform.localScale = newTransform.localScale;
			}
		}

		/// <summary>
		/// Sets the position.
		/// </summary>
		/// <param name="position">The position.</param>
		public void SetPosition(Vector3 position) {
			this.transform.position = position;
		}

		/// <summary>
		/// Sets the rotation.
		/// </summary>
		/// <param name="rotation">The rotation.</param>
		public void SetRotation(Quaternion rotation) {
			this.transform.rotation = rotation;
		}

		/// <summary>
		/// Sets the scale.
		/// </summary>
		/// <param name="scale">The scale.</param>
		public void SetScale(Vector3 scale) {
			this.transform.localScale = scale;
		}

		/// <summary>
		/// Resets the transform.
		/// </summary>
		public void ResetTransform() {
			var instanceTransform = this.transform;

			instanceTransform.localRotation = Quaternion.identity;
			instanceTransform.localPosition = Vector3.zero;
			instanceTransform.localScale = Vector3.one;
		}
		// ----------------------------------------------------------------------------------------------------
		#endregion
	}
}