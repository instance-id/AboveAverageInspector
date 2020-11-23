using UnityEngine.UIElements;

namespace instance.id.AAI.Extensions
{
    /// <summary>
    ///   <para>Collapsable section of UI.</para>
    /// </summary>
    public class AnimatedFoldout : BindableElement, INotifyValueChanged<bool>
    {
        internal static readonly string ussFoldoutDepthClassName = "unity-foldout--depth-";
        internal static readonly int ussFoldoutMaxDepth = 4;
        private Toggle m_Toggle;
        private VisualElement m_Container;
        public Expander expander;
        private bool m_Value;

        /// <summary>
        ///   <para>USS class name of elements of this type.</para>
        /// </summary>
        public static readonly string ussClassName = "unity-foldout";

        /// <summary>
        ///   <para>USS class name of toggle elements in elements of this type.</para>
        /// </summary>
        public static readonly string toggleUssClassName = ussClassName + "__toggle";

        /// <summary>
        ///   <para>USS class name of content element in a AnimatedFoldout.</para>
        /// </summary>
        public static readonly string contentUssClassName = ussClassName + "__content";

        /// <summary>
        ///   <para>USS class name of expander element in a AnimatedFoldout.</para>
        /// </summary>
        public static readonly string expanderUssClassName = ussClassName + "__expander";

        public override VisualElement contentContainer => m_Container;

        public string text
        {
            get => m_Toggle.text;
            set => m_Toggle.text = value;
        }

        /// <summary>
        ///   <para>Contains the collapse state. True if the AnimatedFoldout is open and the contents are visible. False if it's collapsed.</para>
        /// </summary>
        public bool value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using (ChangeEvent<bool> pooled = ChangeEvent<bool>.GetPooled(m_Value, value))
                {
                    pooled.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(pooled);
                }
            }
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            m_Value = newValue;
            m_Toggle.value = m_Value;
        }

        public AnimatedFoldout()
        {
            m_Value = true;
            AddToClassList(ussClassName);
            expander = new Expander();
            expander.AddToClassList(expanderUssClassName);
            Toggle toggle = new Toggle {value = true};

            m_Toggle = toggle;
            m_Toggle.RegisterValueChangedCallback(evt =>
            {
                value = m_Toggle.value;
                expander.Activate(value);
                evt.StopPropagation();
            });

            m_Toggle.AddToClassList(toggleUssClassName);
            hierarchy.Add(m_Toggle);
            m_Container = new VisualElement
            {
                name = "unity-content"
            };
            m_Container.AddToClassList(contentUssClassName);
            expander.AddToExpansionGroup(m_Container);
            hierarchy.Add(expander);
            RegisterCallback(new EventCallback<AttachToPanelEvent>(OnAttachToPanel));
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            int num = 0;
            for (int index = 0; index <= ussFoldoutMaxDepth; ++index)
                RemoveFromClassList(ussFoldoutDepthClassName + index);
            RemoveFromClassList(ussFoldoutDepthClassName + "max");
            if (parent != null)
            {
                for (VisualElement getParent = parent; getParent != null; getParent = getParent.parent)
                {
                    if (getParent.GetType() == typeof(AnimatedFoldout))
                        ++num;
                }
            }

            if (num > ussFoldoutMaxDepth)
                AddToClassList(ussFoldoutDepthClassName + "max");
            else
                AddToClassList(ussFoldoutDepthClassName + num);
        }

        /// <summary>
        ///   <para>Instantiates a AnimatedFoldout using the data read from a UXML file.</para>
        /// </summary>
        public new class UxmlFactory : UxmlFactory<AnimatedFoldout, UxmlTraits>
        {
        }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            private UxmlStringAttributeDescription m_Text;
            private UxmlBoolAttributeDescription m_Value;

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                if (!(ve is AnimatedFoldout foldout))
                    return;
                foldout.text = m_Text.GetValueFromBag(bag, cc);
                foldout.SetValueWithoutNotify(m_Value.GetValueFromBag(bag, cc));
            }

            public UxmlTraits()
            {
                UxmlStringAttributeDescription attributeDescription1 = new UxmlStringAttributeDescription();
                attributeDescription1.name = "text";
                m_Text = attributeDescription1;
                UxmlBoolAttributeDescription attributeDescription2 = new UxmlBoolAttributeDescription();
                attributeDescription2.name = "value";
                attributeDescription2.defaultValue = true;
                m_Value = attributeDescription2;
                // ISSUE: explicit constructor call
            }
        }
    }
}
