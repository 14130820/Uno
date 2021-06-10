using UnityEngine;

public class CommonColors : MonoBehaviour
{
	public static CommonColors Singleton;

	public Color Red, Green, Blue, Yellow;

	[ExecuteInEditMode]
	private void Awake()
	{
		Singleton = this;

		Shader.SetGlobalColor("Red", Red);
		Shader.SetGlobalColor("Green", Green);
		Shader.SetGlobalColor("Blue", Blue);
		Shader.SetGlobalColor("Yellow", Yellow);
	}
}
