using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RangeFloat))]
public class RangeFloatDrawer : PropertyDrawer
{
	private static GUIContent[] emptyContent = new GUIContent[2] { new GUIContent(" "), new GUIContent(" ") };

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{

		int lines = EditorGUIUtility.wideMode ? 1 : 2;
		position.height = lines * EditorGUIUtility.singleLineHeight;

		SerializedProperty r1Prop = property.FindPropertyRelative("r1");
		SerializedProperty r2Prop = property.FindPropertyRelative("r2");
		float[] r = new float[2] { r1Prop.floatValue, r2Prop.floatValue };

		EditorGUI.BeginChangeCheck();
		EditorGUI.MultiFloatField(position, label, emptyContent, r);
		if(EditorGUI.EndChangeCheck())
		{
			r1Prop.floatValue = r[0];
			r2Prop.floatValue = r[1];
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		int lines = EditorGUIUtility.wideMode ? 1 : 2;
		return lines * EditorGUIUtility.singleLineHeight;
	}
}