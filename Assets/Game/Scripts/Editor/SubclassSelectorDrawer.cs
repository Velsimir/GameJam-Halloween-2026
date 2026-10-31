using System;
using Game.Scripts.Configs.Scenario;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            float offset = EditorGUIUtility.labelWidth + 2f;
            var buttonRect = new Rect(position.x + offset, position.y, position.width - offset, EditorGUIUtility.singleLineHeight);
            object value = property.managedReferenceValue;
            string caption = value == null ? "(None) — выбрать действие" : ObjectNames.NicifyVariableName(value.GetType().Name);

            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(caption), FocusType.Keyboard))
                ShowMenu(property);

            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        private static void ShowMenu(SerializedProperty property)
        {
            Type baseType = ResolveType(property.managedReferenceFieldTypename);
            if (baseType == null) return;

            SerializedObject serializedObject = property.serializedObject;
            string path = property.propertyPath;
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("(None)"), false, () => Assign(serializedObject, path, null));

            foreach (Type type in TypeCache.GetTypesDerivedFrom(baseType))
            {
                if (type.IsAbstract || type.IsGenericType || typeof(UnityEngine.Object).IsAssignableFrom(type)) continue;
                if (type.GetConstructor(Type.EmptyTypes) == null) continue;

                Type captured = type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false,
                    () => Assign(serializedObject, path, Activator.CreateInstance(captured)));
            }

            menu.ShowAsContext();
        }

        private static void Assign(SerializedObject serializedObject, string path, object value)
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty(path);
            if (property == null) return;

            property.managedReferenceValue = value;
            property.isExpanded = value != null;
            serializedObject.ApplyModifiedProperties();
        }

        private static Type ResolveType(string typename)
        {
            if (string.IsNullOrEmpty(typename)) return null;

            int split = typename.IndexOf(' ');
            if (split < 0) return null;

            string assembly = typename.Substring(0, split);
            string fullName = typename.Substring(split + 1);
            return Type.GetType($"{fullName}, {assembly}");
        }
    }
}
