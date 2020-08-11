using UnityEngine;
using System.Collections;

namespace FeatherWorks.Pooling {
	public static class Extensions {
		/// <summary>
		/// Despawns the specified game object.
		/// </summary>
		public static void Despawn(this GameObject gameObject) {
			FeatherPoolManager.Instance.Despawn(gameObject);
		}
	}
}