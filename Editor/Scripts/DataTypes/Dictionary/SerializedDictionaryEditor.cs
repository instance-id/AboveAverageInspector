using System.Collections.Generic;
using instance.id.AAI.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace instance.id.AAI.Editors
{
    // -- SerializedDictionary.cs ----------------------------------------
    [CustomEditor(typeof(DictionaryAttribute))] // @formatter:off
    public class SerializedDictEditor : AAIDefaultEditor {} // @formatter:on

    [CustomPropertyDrawer(typeof(DictionaryAttribute))]
    public class SerializedDictionaryDrawer : PropertyDrawer
    {
        private StyleSheet styleSheet;
        private VisualElement container;
        private SerializedProperty propertyKeyField;
        private SerializedProperty propertyValueField;

        private List<string> objectTypes = new List<string>
        {
            "LayerTag",
            "PPtr<$LayerTag>",
            "SerializedScriptableObject",
            "ScriptableObject",
            "Object",
            "PPtr<$Object>"
        };

        private List<string> guidTypes = new List<string>
        {
            "GuidComponent",
            "PPtr<$GuidComponent>",
        };

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            container = new VisualElement();
            if (styleSheet is null)
            {
                styleSheet = idConfig.GetStyleSheet("AAICustomProperties");
                if (!(styleSheet is null)) container.styleSheets.Add(styleSheet);
            }

            propertyKeyField = property.FindPropertyRelative(SerializedDictionary<object, object>.KeyProperty);
            propertyValueField = property.FindPropertyRelative(SerializedDictionary<object, object>.ValueProperty);
            propertyKeyField.serializedObject.ApplyModifiedProperties();
            propertyValueField.serializedObject.ApplyModifiedProperties();

            var box = new Box();
            var scroller = new ScrollView {name = "serialDictScroller"};

            for (int i = 0; i < propertyKeyField.arraySize; i++)
            {
                // ReSharper disable once UnusedVariable
                var keyType = propertyKeyField.GetArrayElementAtIndex(i).type;
                var valueType = propertyValueField.GetArrayElementAtIndex(i).type;

                // ----------------------------------------------------------- Dictionary Container
                // -- Dictionary Container --------------------------------------------------------
                var layerEntry = new VisualElement {focusable = true, name = $"Entry: {i}"};
                layerEntry.AddToClassList("serialDictionaryContainer");

                // ----------------------------------------------------------------- Dictionary Key
                // -- Dictionary Key --------------------------------------------------------------
                var keyTextField = new TextField
                {
                    bindingPath = propertyKeyField.propertyPath,
                    label = null,
                    name = "keyTextField"
                };

                switch (valueType)
                {
                    case string a when a.Contains("GuidReference"):
                    case string b when b.Contains("GuidComponent"):
                        keyTextField.AddToClassList("serialDictionaryKeyGuid");
                        keyTextField.AddToClassList("unity-base-field--no-label");
                        break;
                    default:
                        keyTextField.AddToClassList("serialDictionaryKey");
                        keyTextField.AddToClassList("unity-base-field--no-label");
                        break;
                }

                keyTextField.SetValueWithoutNotify(propertyKeyField.GetArrayElementAtIndex(i).stringValue);
                keyTextField.SetEnabled(false);
                // keyTextField.Q(null, "unity-base-text-field__input").RemoveFromClassList("unity-base-text-field__input");

                keyTextField.Q(null, "unity-disabled")
                    .RemoveFromClassList("unity-disabled");

                keyTextField.AddToClassList("serialDictionaryKeyLocator");
                keyTextField.SetEnabled(false);

                VisualElement listKey = keyTextField;
                layerEntry.Add(listKey);

                // --------------------------------------------------------------- Dictionary Value
                // -- Dictionary Value ------------------------------------------------------------
                VisualElement listValue = new PropertyField(propertyValueField.GetArrayElementAtIndex(i))
                {
                    name = "valueObjectField"
                };
                listValue.SetEnabled(true);

                switch (valueType)
                {
                    case string a when objectTypes.Contains(a):
                        var objectField = new ObjectField
                        {
                            bindingPath = propertyValueField.propertyPath,
                            label = null,
                            name = "valueObjectField"
                        };
                        objectField.SetValueWithoutNotify(propertyValueField.GetArrayElementAtIndex(i).objectReferenceValue);
                        objectField.AddToClassList("serialDictionaryValue");
                        objectField.AddToClassList("unity-base-field--no-label");
                        objectField.Q(null, "unity-object-field__selector")
                            .RemoveFromClassList("unity-object-field__selector");
                        // objectField.Q(null, "unity-object-field__input").RemoveFromClassList("unity-object-field__input");
                        listValue = objectField;
                        break;
                    case string b when guidTypes.Contains(b):
                        var guidObjectField = new ObjectField
                        {
                            bindingPath = propertyValueField.propertyPath,
                            label = null,
                            name = "valueObjectField"
                        };
                        guidObjectField.SetValueWithoutNotify(propertyValueField.GetArrayElementAtIndex(i).objectReferenceValue);
                        guidObjectField.AddToClassList("serialDictionaryGuidValue");
                        guidObjectField.AddToClassList("unity-base-field--no-label");
                        guidObjectField.Q(null, "unity-object-field__selector")
                            .RemoveFromClassList("unity-object-field__selector");
                        // objectField.Q(null, "unity-object-field__input").RemoveFromClassList("unity-object-field__input");
                        listValue = guidObjectField;
                        break;
                    case string d when d.Contains("int"):

                        var valueTextField = new TextField
                        {
                            bindingPath = propertyValueField.propertyPath,
                            label = null
                        };
                        valueTextField.SetValueWithoutNotify(propertyValueField.GetArrayElementAtIndex(i).intValue.ToString());
                        valueTextField.SetEnabled(false);
                        listValue = valueTextField;
                        listValue.AddToClassList("serialDictionaryValue");
                        listValue.RemoveFromClassList("unity-base-text-field__input");
                        break;
                    case string d when d.Contains("Type"):

                        var valueTypeTextField = new TextField
                        {
                            bindingPath = propertyValueField.propertyPath,
                            label = null
                        };
                        valueTypeTextField.SetValueWithoutNotify(propertyValueField.GetArrayElementAtIndex(i).stringValue);
                        valueTypeTextField.SetEnabled(false);
                        listValue = valueTypeTextField;
                        listValue.AddToClassList("serialDictionaryValue");
                        listValue.RemoveFromClassList("unity-base-text-field__input");
                        break;
                    default:
                        listValue.AddToClassList("serialDictionaryValue");
                        listValue.AddToClassList("unity-base-field--no-label");
                        break;
                }

                listValue.AddToClassList("serialDictionaryValueLocator");
                layerEntry.Add(listValue);

                scroller.Add(layerEntry);
            }

            box.Add(scroller);
            container.Add(box);


            return container;
        }
    }
}
