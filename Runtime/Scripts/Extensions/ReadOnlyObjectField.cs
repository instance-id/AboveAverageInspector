// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/AboveAverageInspector         --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace instance.id.AAI
{
    // ------- Snippets originally created by: 5argon ---------
    // -- https://gametorrahod.com/uielements-custom-marker/ --
    // --------------------------------------------------------

    /// <summary>
    /// Removed background image and the selection dot on the right from object field.
    /// </summary>
    public class ReadOnlyObjectField<T> : ObjectField where T : Object
    {
        public ReadOnlyObjectField(T obj) : base()
        {
            SetValueWithoutNotify(obj);
            this.Q(null, "unity-object-field__selector").RemoveFromClassList("unity-object-field__selector");
            this.Q(null, "unity-object-field-display").RemoveFromClassList("unity-object-field-display");
        }

        public ReadOnlyObjectField(T obj, string label) : base(label)
        {
            SetValueWithoutNotify(obj);
            this.Q(null, "unity-object-field__selector").RemoveFromClassList("unity-object-field__selector");
            this.Q(null, "unity-object-field-display").RemoveFromClassList("unity-object-field-display");
        }

        public sealed override void SetValueWithoutNotify(Object newValue)
        {
            base.SetValueWithoutNotify(newValue);
        }

        public float MeasureWidth()
        {
            var icon = this.Q<Image>();
            var label = this.Q<Label>();
            var labelText = label.text;
            return icon.style.maxWidth.value.value + EditorStyles.objectField.CalcSize(new GUIContent(labelText)).x;
        }
        
    }
    

}
