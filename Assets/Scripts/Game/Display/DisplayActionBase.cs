using System;
using ObjectPooling;

namespace Uno.Game.Display
{
    /// <summary>
    /// Create a class and implement this.
    /// DisplayManager.QueueAction(this) to add it to the queue.
    /// </summary>
    [System.Serializable]
	public abstract class DisplayActionBase : ObjectPoolItem
	{
        public int ActionID { get; set; }

        public bool Ended { get; set; }

		/// <summary>
		/// Sends actions back to the manager
		/// </summary>
		/// <param name="action">"this"</param>
		protected static void SendActionToManager(DisplayActionBase action, ActionFlags events = ActionFlags.All) => DisplayManager.Instance.ReceiveDisplayAction(action, events);
	
		/// <summary>
		/// Called when the action starts in the queue
		/// </summary>
		public abstract void ActionStart();

		/// <summary>
		/// Called by the manager on a standard Unity Update.
		/// Returns if the action is removed.
		/// </summary>
		public abstract void ActionUpdate();
        
        /// <summary>
        /// Used for sending an event to the manager.
        /// </summary>
        [Flags]
		public enum ActionFlags : byte
		{
			EndAction = 1 << 0,
			StartNextAction = 1 << 1,
			All = EndAction | StartNextAction
		}

        protected void QueueThis() => DisplayManager.Instance.QueueAction(this);
	}
}