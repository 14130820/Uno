using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[InlineProperty, HideLabel]
public class ProgressBar {
	#region Fields & Properties
	// ----------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
	[OnInspectorGUI]
	[TitleGroup("Active Instances")]
	private void DrawProgressBar() {
		Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
		UnityEditor.EditorGUI.ProgressBar(rect, this.ProgressValue, this.ToString());
	}
#endif

	public float Value {
		get { return this.value; }
		set { this.value = Math.Min(Math.Max(value, 0), this.maximum); }
	}
	private float value;

	public float Maximum {
		get { return this.maximum; }
		set { this.maximum = Math.Max(1, value); }
	}
	private float maximum;

	public float ProgressValue {
		get {
			return Value / Maximum;
		}
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region Initialization
	// ----------------------------------------------------------------------------------------------------
	public ProgressBar() : this(0, 1) { }

	public ProgressBar(float maximum) : this(0, maximum) { }

	public ProgressBar(float value, float maximum) {
		Value = value;
		Maximum = maximum;
	}
	// ----------------------------------------------------------------------------------------------------
	#endregion

	#region ToString Method
	// ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Returns a <see cref="System.String" /> that represents this instance.
	/// </summary>
	public override string ToString() {
		return String.Format("{0} / {1} ({2:P1})", this.Value, this.Maximum, this.ProgressValue);
	} 
	  // ----------------------------------------------------------------------------------------------------
	#endregion
}
