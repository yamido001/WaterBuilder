using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(WaterBuilder))]
public class WaterBuilderEditor : Editor {

	SerializedProperty waterHeight;
	WaterBuilder mWater = null;

	private float mWaterHeight = 0f;
	private bool mAutoRebuild = false;

	void OnEnable()
	{
		waterHeight = serializedObject.FindProperty ("waterHeight");
		mWater = target as WaterBuilder;

		mWaterHeight = waterHeight.floatValue;

	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update ();
		EditorGUILayout.PropertyField (waterHeight, new GUIContent ("Water  Height"));
		EditorGUILayout.Space ();

		mAutoRebuild = EditorGUILayout.Toggle("Auto build water", mAutoRebuild);
		if (!mAutoRebuild) {
			if (GUILayout.Button ("Build water")) {
				mWater.BuildWater ();
			}
		}

		serializedObject.ApplyModifiedProperties ();

		if (GUI.changed && mAutoRebuild) {
			bool hasChanged = false;
			if (mWaterHeight != waterHeight.floatValue) {
				mWaterHeight = waterHeight.floatValue;
				hasChanged = true;
			}
			if (hasChanged)
				mWater.BuildWater ();
		}
	}
}
