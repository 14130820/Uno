using System.Collections.Generic;
using DG.Tweening;
using kcp2k;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arithfeather
{
	public class ConsoleMessageManager : MonoBehaviour
	{
		public static ConsoleMessageManager Singleton;

		public struct ConsoleMessage
		{
			public ConsoleMessage(string message, float time)
			{
				Message = message;
				Time = time;
			}

			public string Message { get; }
			public float Time { get; }
		}

		public float fadeSpeed = 3;

		private TextMeshProUGUI textMeshPro;
		private Image backgroundImage;
		private Canvas canvas;
		private float timer;

		private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();
		private Sequence fadeSequence;

		private void Awake()
		{
			Singleton = this;

			canvas = GetComponent<Canvas>();
			textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
			backgroundImage = GetComponent<Image>();

			Log.Warning = x => DisplayMessage(new ConsoleMessage(x, 2));
			Log.Error = x => DisplayMessage(new ConsoleMessage(x, 2));
		}

		public void DisplayMessage(ConsoleMessage message, bool replaceOld = true)
		{
			if (replaceOld)
				messageQueue.Clear();

			messageQueue.Enqueue(message);

			if (messageQueue.Count == 1)
				NextInQueue();
		}

		private void NextInQueue()
		{
			if (fadeSequence != null)
				fadeSequence.Kill();

			var message = messageQueue.Peek();

			timer = message.Time;

			textMeshPro.text = message.Message;

			textMeshPro.color = textMeshPro.color + new Color(0, 0, 0, 1);
			backgroundImage.color = backgroundImage.color + new Color(0, 0, 0, 1);

			canvas.enabled = true;
		}

		private void Update()
		{
			if (!canvas.enabled ||
			    (fadeSequence != null && fadeSequence.active))
				return;

			timer -= Time.deltaTime;

			if (timer <= 0)
			{
				messageQueue.Dequeue();

				if (messageQueue.Count != 0)
				{
					NextInQueue();
				}
				else
				{
					fadeSequence = DOTween.Sequence().Append(

							DOTween.ToAlpha(
								() => textMeshPro.color,
								x => textMeshPro.color = x,
								0, fadeSpeed))

						.Join(

							DOTween.ToAlpha(
								() => backgroundImage.color,
								x => backgroundImage.color = x,
								0, fadeSpeed)

						).OnComplete(() => canvas.enabled = false);
				}
			}
		}
	}
}