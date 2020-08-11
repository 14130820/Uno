using ObjectPooling;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Uno.Game.Display.DisplayActionBase;

namespace Uno.Game.Display
{
	public class DisplayManager : MonoBehaviour
	{
		public static DisplayManager Instance { get; private set; }

        public const int DISPLAY_ACTION_ARRAY_CAPACITY = 70;

		private void Awake() => Instance = this;

        public void Reset()
        {
            IsRunningActions = false;
            HasActionQueued = false;
            
            var length = DisplayActions.Count;
            for (int i = 0; i < length; i++)
            {
                DisplayActions.Dequeue().PoolThis();
            }
            length = RunningActions.Count;
            for (int i = 0; i < length; i++)
            {
                RunningActions[i].PoolThis();
            }

            RunningActions.Clear();

            StartQueue.Clear();
            EndQueue.Clear();
            itemInStartQueue = false;
            itemInEndQueue = false;
            actionIDIndex = 0;
        }
        
        private Queue<DisplayActionBase> DisplayActions { get; } = new Queue<DisplayActionBase>(DISPLAY_ACTION_ARRAY_CAPACITY);[ShowInInspector]
        
        private bool IsRunningActions { get; set; }
        private bool HasActionQueued { get; set; }
        
        /// <summary>
        /// Current Actions getting Updated every Unity Update call.
        /// </summary>
        private List<DisplayActionBase> RunningActions { get; } = new List<DisplayActionBase>(DISPLAY_ACTION_ARRAY_CAPACITY);

        /// <summary>
        /// Pseudo OnEnable
        /// </summary>
        private Queue<DisplayActionBase> StartQueue { get; set; } = new Queue<DisplayActionBase>(DISPLAY_ACTION_ARRAY_CAPACITY);
		private bool itemInStartQueue = false;

        /// <summary>
        /// Pseudo OnDisable
        /// </summary>
        private Queue<DisplayActionBase> EndQueue { get; set; } = new Queue<DisplayActionBase>(DISPLAY_ACTION_ARRAY_CAPACITY);
		private bool itemInEndQueue = false;

        /// <summary>
        /// Custom index for easy lookup.
        /// </summary>
        private int actionIDIndex = 0;

		/// <summary>
		/// Adds an action to the queue.
		/// </summary>
		public void QueueAction(DisplayActionBase action)
        {
            action.Ended = false;
            action.ActionID = actionIDIndex;
            actionIDIndex++;

			if (!HasActionQueued)
            {
                IsRunningActions = true;
                HasActionQueued = true;

                InitializeAction(action);
            }
			else
			{
                DisplayActions.Enqueue(action);
            }
		}
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeAction(DisplayActionBase action)
		{
            RunningActions.Add(action);

            StartQueue.Enqueue(action);
            itemInStartQueue = true;
		}
		
		private void Update()
		{
			if (IsRunningActions)
			{
				var actionCount = RunningActions.Count;

				if (itemInStartQueue)
				{
					itemInStartQueue = false;

					var length = StartQueue.Count;
					for (int i = 0; i < length; i++)
					{
						StartQueue.Dequeue().ActionStart();
					}
				}
				
				for (int i = 0; i < actionCount; i++)
				{
                    var action = RunningActions[i];
					if (!action.Ended) action.ActionUpdate();
				}

				if (itemInEndQueue)
				{
					itemInEndQueue = false;

					var length2 = EndQueue.Count;
					for (int i = 0; i < length2; i++)
                    {
                        RemoveAction(EndQueue.Dequeue());
                    }

					if (RunningActions.Count == 0)
					{
						IsRunningActions = false;
					}
				}
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveAction(DisplayActionBase action)
        {
            var length = RunningActions.Count;
            for (int i = 0; i < length; i++)
            {
                if (RunningActions[i].ActionID == action.ActionID)
                {
                    RunningActions.RemoveAt(i);
                    return;
                }
            }
        }
        
		public void ReceiveDisplayAction(DisplayActionBase action, ActionFlags flags)
		{
			if (flags.HasFlag(ActionFlags.StartNextAction))
            {
                if (DisplayActions.Count != 0)
				{
					InitializeAction(DisplayActions.Dequeue());
				}
                else
                {
                    HasActionQueued = false;
                }
			}

			if (flags.HasFlag(ActionFlags.EndAction))
            {
                action.Ended = true;
                EndQueue.Enqueue(action);
                itemInEndQueue = true;
            }
		}
	}
}