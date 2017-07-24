using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Matrix4x4))]
public class Matrix4x4Drawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		position.height = EditorGUIUtility.singleLineHeight;
		property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
		position.y += position.height;

		if(property.isExpanded)
		{
			EditorGUI.indentLevel++;

			float windowWidth = position.width;
			float startX = position.x;

			position.width = windowWidth / 4f;

			for(int c = 0; c < 4; c++)
			{
				for(int r = 0; r < 4; r++)
				{
					property.Next(true);
					property.floatValue = EditorGUI.FloatField(position, property.floatValue);
					position.x += position.width;
				}
				position.x = startX;
				position.y += position.height;
			}

			EditorGUI.indentLevel--;
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float result = 0;
		float lineHeight = EditorGUIUtility.singleLineHeight;
		result += lineHeight;

		if(property.isExpanded)
		{
			for (int c = 0; c < 4; c++)
			{
				for (int r = 0; r < 4; r++)
				{
					property.Next(true);
				}
				result += lineHeight;
			}
		}

		return result;
	}
}