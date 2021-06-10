using UnityEngine;

namespace Arithfeather
{
	/// <summary>
	/// Disables a game object after it has initialized.
	/// This is mainly so Game Objects can pre-load.
	/// </summary>
	public class DisableAfterInitialization : MonoBehaviour
	{
		[SerializeField] private Operation disableOperation = Operation.Canvas;

		public enum Operation
		{
			Canvas,
		}

		private void Awake()
		{
			if (disableOperation == Operation.Canvas)
				GetComponent<Canvas>().enabled = false;
		}
	}
}