using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace instance.id.AAI.Editors
{
    // -- Original concept by Unity via : uMathiu ----------------------------------------------------------------------
    // -- https://forum.unity.com/threads/propertydrawer-with-uielements-changes-in-array-dont-refresh-inspector.747467/
    public class ArrayElementBuilder : BindableElement, INotifyValueChanged<int>
    {
        private readonly SerializedObject boundObject;
        private readonly string arrayPath;
        private Func<string, int, bool, VisualElement> makeItem { get; set; }
        private Action<string, int, bool> onAddItem { get; set; }
        public override VisualElement contentContainer => baseContainer;
        private readonly VisualElement baseContainer;
        private int arraySize;
        private Label arrayFooterLabel;
        private int addNewArraySize;
        private FieldData fieldType;

        public SerializedProperty ArrayProperty => m_ArrayProperty;
        private readonly SerializedProperty m_ArrayProperty;
        private Button addButton;

        public ArrayElementBuilder(SerializedProperty arrayProperty, FieldData fieldType,
            Func<string, int, bool, VisualElement> makeItem = null, Action<string, int, bool> on_add_item = null)
        {
            m_ArrayProperty = arrayProperty;
            this.fieldType = fieldType;
            m_ArrayProperty.serializedObject.Update();
            addNewArraySize = m_ArrayProperty.arraySize;
            AddToClassList("arrayElementBuilder");

            // -------------------------------------------- arrayFoldout
            var arrayFoldout = new Foldout {text = arrayProperty.displayName, name = "arrayFoldout"};
            arrayFoldout.AddToClassList("arrayFoldout");
            arrayFoldout.value = false;

            // -------------------------------------------- baseContainer
            baseContainer = new VisualElement {name = "arrayBaseContainer"};
            baseContainer.AddToClassList("arrayBaseContainer");

            arrayFoldout.Add(baseContainer);
            hierarchy.Add(arrayFoldout);

            // -------------------------------------------- addButton
            addButton = new Button(AddNewItem) {text = "+", name = "arrayAddButton"};
            addButton.AddToClassList("arrayAddButton");

            arrayPath = arrayProperty.propertyPath;
            boundObject = arrayProperty.serializedObject;
            this.makeItem = makeItem;
            var property = arrayProperty.Copy();
            var endProperty = property.GetEndProperty();

            // -------------------------------------------- iterator
            property.NextVisible(true);
            do
            {
                if (SerializedProperty.EqualContents(property, endProperty)) break;
                if (property.propertyType == SerializedPropertyType.ArraySize)
                {
                    arraySize = property.intValue;
                    bindingPath = property.propertyPath;
                    break;
                }
            } while (property.NextVisible(false));

            arrayProperty.serializedObject.SetIsDifferentCacheDirty();
            UpdateCreatedItems();

            // -------------------------------------------- arrayFooter
            var arrayFooter = new VisualElement {name = "ArrayFooter"};
            arrayFooter.AddToClassList("arrayFooter");
            arrayFooterLabel = new Label($"Total: {childCount.ToString()}");
            arrayFooterLabel.AddToClassList("arrayFooterLabel");
            arrayFooter.Add(arrayFooterLabel);

            onAddItem = on_add_item;

            arrayFooter.Add(addButton);
            arrayFoldout.Add(arrayFooter);
            m_ArrayProperty.serializedObject.ApplyModifiedProperties();
        }

        private void AddNewItem()
        {
            m_ArrayProperty.serializedObject.Update();
            m_ArrayProperty.InsertArrayElementAtIndex(m_ArrayProperty.arraySize);
            m_ArrayProperty.serializedObject.ApplyModifiedProperties();
            onAddItem?.Invoke($"{m_ArrayProperty.propertyPath}.Array.data[{m_ArrayProperty.arraySize.ToString()}]", m_ArrayProperty.arraySize, true);
            m_ArrayProperty.serializedObject.ApplyModifiedProperties();
        }

        // -------------------------------------------- addButton
        private VisualElement AddItem(SerializedProperty property, string propertyPath, int index, bool hideSelector = true)
        {
            VisualElement child;
            if (makeItem != null) child = makeItem(propertyPath, index, hideSelector);
            else
            {
                switch (fieldType.fieldTypeParameters[0])
                {
                    case { } a when a == typeof(string):
                    case { } b when b == typeof(PropertyName):
                        child = MakeStringItem(property, propertyPath, index, hideSelector);
                        break;
                    case { } a when a == typeof(bool):
                        child = MakeToggleItem(property, propertyPath, index, hideSelector);
                        break;
                    case { } a when a == typeof(Object):
                    case { } b when typeof(Object).IsSubclassOf(b):
                    case { } c when typeof(Object).IsAssignableFrom(c):
                        child = MakeObjectItem(property, propertyPath, index, hideSelector);
                        break;
                    default:
                        child = MakePropertyItem(property, propertyPath, index, hideSelector);
                        break;
                }
            }

            Add(child);
            return child;
        }

        // ---------------------------------------------------- Build String Objects 
        // -- Build String Objects -------------------------------------------------
        private VisualElement MakeStringItem(SerializedProperty property, string propertyPath, int index, bool hideSelector = true)
        {
            var container = new VisualElement();
            container.AddToClassList("makeStringItemContainer");

            var pf = new TextField
            {
                bindingPath = propertyPath,
                label = null,
                name = "valueTextField"
            }; // @formatter:off

            try {  pf.SetValueWithoutNotify(property.GetArrayElementAtIndex(index).stringValue); }
            catch (Exception e) // @formatter:on
            {
                Debug.Log($"Item: {propertyPath} : {e}");
                throw;
            }

            pf.AddToClassList("makeStringItem");
            pf.AddToClassList("unity-base-field--no-label");
            pf.bindingPath = propertyPath;
            pf.style.flexGrow = new StyleFloat(1.0f);

            BuildContainers(container, index);
            container.Add(pf);
            return container;
        }

        // ---------------------------------------------------- Build Object Objects
        // -- Build Object Objects -------------------------------------------------
        private VisualElement MakeObjectItem(SerializedProperty property, string propertyPath, int index, bool hideSelector = true)
        {
            var container = new VisualElement();
            container.AddToClassList("makeObjectItemContainer");

            var pf = new ObjectField
            {
                bindingPath = propertyPath,
                label = null,
                name = "valueObjectField",
                objectType = typeof(Object)
            }; // @formatter:off

            try { pf.SetValueWithoutNotify(property.GetArrayElementAtIndex(index).objectReferenceValue); } 
            catch (Exception e)  // @formatter:on
            {
                Debug.Log($"Item: {propertyPath} : {e}");
                throw;
            }

            pf.AddToClassList("makeObjectItem");
            pf.AddToClassList("unity-base-field--no-label");
            pf.bindingPath = propertyPath;
            pf.style.flexGrow = new StyleFloat(1.0f);

            BuildContainers(container, index);
            container.Add(pf);

            if (hideSelector)
                container
                    .Q(null, "unity-object-field__selector")
                    .RemoveFromClassList("unity-object-field__selector");

            return container;
        }

        // ---------------------------------------------------- Build Object Objects
        // -- Build Object Objects -------------------------------------------------
        private VisualElement MakeToggleItem(SerializedProperty property, string propertyPath, int index, bool hideSelector = true)
        {
            var container = new VisualElement();
            container.AddToClassList("makeToggleItemContainer");

            var pf = new Toggle
            {
                bindingPath = propertyPath,
                label = null,
                name = "makeToggleItem"
            }; // @formatter:off
            
            try {  pf.SetValueWithoutNotify(property.GetArrayElementAtIndex(index).objectReferenceValue); } 
            catch (Exception e)  // @formatter:on
            {
                Debug.Log($"Item: {propertyPath} : {e}");
                throw;
            }

            pf.AddToClassList("unity-base-field--no-label");
            pf.AddToClassList("makeToggleItem");
            pf.bindingPath = propertyPath;
            pf.style.flexGrow = new StyleFloat(1.0f);

            BuildContainers(container, index);
            container.Add(pf);

            return container;
        }
        
        // ---------------------------------------------------- Build Object Objects
        // -- Build Object Objects -------------------------------------------------
        private VisualElement MakePropertyItem(SerializedProperty property, string propertyPath, int index, bool hideSelector = true)
        {
            var container = new VisualElement();
            container.AddToClassList("makePropertyItemContainer");

            var pf = new PropertyField(property.GetArrayElementAtIndex(index))
            {
                name = $"{propertyPath}PropertyField",
            }; // @formatter:off

            pf.AddToClassList("makePropertyItem");
            pf.AddToClassList("unity-base-field--no-label");
            pf.bindingPath = propertyPath;
            pf.style.flexGrow = new StyleFloat(1.0f);

            BuildContainers(container, index);
            container.Add(pf);

            // if (hideSelector)
            //     container
            //         .Q(null, "unity-object-field__selector")
            //         .RemoveFromClassList("unity-object-field__selector");

            return container;
        }

        // ------------------------------------------------- Build Container Objects
        // -- Build Container Objects ----------------------------------------------
        private void BuildContainers(VisualElement container, int index)
        {
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexGrow = new StyleFloat(1.0f);
            var remove_button = new Button(() =>
            {
                m_ArrayProperty.DeleteArrayElementAtIndex(index);
                m_ArrayProperty.serializedObject.ApplyModifiedProperties();
            });
            remove_button.AddToClassList("arrayRemoveButton");
            remove_button.text = "-";
            container.Add(remove_button);
        }


        // -------------------------------------------- UpdateCreatedItems
        private bool UpdateCreatedItems()
        {
            var currentSize = childCount;
            var targetSize = arraySize;
            if (targetSize < currentSize)
                for (var i = currentSize - 1; i >= targetSize; --i)
                    RemoveAt(i);
            else if (targetSize > currentSize)
            {
                for (var i = currentSize; i < targetSize; ++i) AddItem(m_ArrayProperty, $"{arrayPath}.Array.data[{i}]", i);
                return true;
            }

            return false;
        }

        // -------------------------------------------- SetValueWithoutNotify
        public void SetValueWithoutNotify(int newSize)
        {
            arraySize = newSize;
            if (UpdateCreatedItems()) this.Bind(boundObject);
        }

        public int value
        {
            get => arraySize;
            set
            {
                if (arraySize == value) return;
                if (panel != null)
                {
                    using (var evt = ChangeEvent<int>.GetPooled(arraySize, value))
                    {
                        evt.target = this;
                        arraySize = value;
                        SendEvent(evt);
                        SetValueWithoutNotify(value);
                    }
                }
                else
                {
                    SetValueWithoutNotify(value);
                }
            }
        }
    }
}
