using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Uno
{
	public static class GenericExtensions
	{
		public static void Shuffle<T>(this List<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = Random.Range(0, n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static List<T> GetCopy<T>(this List<T> list)
        {
            var length = list.Count;

            List<T> copy = new List<T>(length);
            
			for (int i = 0; i < length; i++)
			{
				copy.Add(list[i]);
			}

			return copy;
		}
	}
}
