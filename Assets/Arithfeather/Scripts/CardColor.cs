using UnityEngine;

namespace Arithfeather
{
	public enum CardColor : byte
	{
		Wild,
		Red,
		Green,
		Blue,
		Yellow,
	}

	public static partial class Extensions
	{
		public static Color GetColor(this CardColor color)
		{
			switch (color)
			{
				case CardColor.Red:
					return CommonColors.Singleton.Red;
				case CardColor.Green:
					return CommonColors.Singleton.Green;
				case CardColor.Blue:
					return CommonColors.Singleton.Blue;
				case CardColor.Yellow:
					return CommonColors.Singleton.Yellow;
			}

			return Color.black;
		}
	}
}