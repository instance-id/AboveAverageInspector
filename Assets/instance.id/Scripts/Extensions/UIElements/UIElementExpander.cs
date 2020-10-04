using System;
using JetBrains.Annotations;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace instance.id.AAI.Extensions
{
    public class UIElementExpander : VisualElement
    {
        ValueAnimation<StyleValues> foldoutAnimation;
        private readonly Toggle expandToggle;
        private readonly VisualElement expandContainerItems;
        public VisualElement shownItem;
        [CanBeNull] public Action expandTrigger;
        private int tmpAnimTime = 0;

        public bool startExpanded { get; set; } = false;
        private bool firstStart = false;

        private int m_AnimationTime = 500;

        public int animationTime
        {
            get => firstStart ? tmpAnimTime : m_AnimationTime;
            [UsedImplicitly] set => m_AnimationTime = value;
        }

        public UIElementExpander()
        {
            expandToggle = new Toggle {style = {display = DisplayStyle.None}, name = "ExpandToggle"};
            expandToggle.RegisterValueChangedCallback(ExpandContainerValueChanges);

            expandContainerItems = new VisualElement {style = {overflow = Overflow.Hidden}};
            expandContainerItems.Add(shownItem);

            Add(expandToggle);
            Add(expandContainerItems);
            Add(shownItem);

            expandContainerItems.style.display = DisplayStyle.None;
        }

        public void Activate()
        {
            expandToggle.value = !expandToggle.value;
        }

        public void Activate(bool value)
        {
            expandToggle.value = value;
        }

        public void AddToGroup(VisualElement element)
        {
            expandContainerItems.Add(element);
        }

        public void TriggerValueChange(ChangeEvent<bool> eventValue)
        {
            ExpandContainerValueChanges(eventValue);
        }

        public void TriggerValueChange(bool eventValue)
        {
            ExpandContainerValueChanges(eventValue);
        }

        public void TriggerGeometryChange(GeometryChangedEvent eventValue)
        {
            OnGeometryChangedEvent(eventValue);
        }

        void ExpandContainerValueChanges(ChangeEvent<bool> evt)
        {
            if (style.display == DisplayStyle.None) style.display = DisplayStyle.Flex;
            if (expandContainerItems.style.display == DisplayStyle.None) expandContainerItems.style.display = DisplayStyle.Flex;

            if (foldoutAnimation != null)
            {
                foldoutAnimation.Recycle();
                foldoutAnimation = null;
            }

            if (evt.newValue)
            {
                expandContainerItems.style.height = StyleKeyword.Auto;
                expandContainerItems.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            }
            else
            {
                foldoutAnimation =
                    expandContainerItems.experimental.animation.Start(new StyleValues {height = expandContainerItems.layout.height}, new StyleValues {height = 0}, animationTime);
                foldoutAnimation.KeepAlive(); // Prevent it being reused when the animation is finished else an error may be thrown when we try to check if it has finished.
            }
        }

        void ExpandContainerValueChanges(bool evt)
        {
            if (style.display == DisplayStyle.None) style.display = DisplayStyle.Flex;
            if (expandContainerItems.style.display == DisplayStyle.None) expandContainerItems.style.display = DisplayStyle.Flex;

            if (foldoutAnimation != null)
            {
                foldoutAnimation.Recycle();
                foldoutAnimation = null;
            }

            if (evt)
            {
                expandContainerItems.style.height = StyleKeyword.Auto;
                expandContainerItems.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            }
            else
            {
                foldoutAnimation =
                    expandContainerItems.experimental.animation.Start(new StyleValues {height = expandContainerItems.layout.height}, new StyleValues {height = 0}, animationTime);
                foldoutAnimation.KeepAlive(); // Prevent it being reused when the animation is finished else an error may be thrown when we try to check if it has finished.
            }
        }

        void OnGeometryChangedEvent(GeometryChangedEvent evt)
        {
            foldoutAnimation =
                expandContainerItems.experimental.animation.Start(
                    new StyleValues {height = evt.oldRect.height},
                    new StyleValues {height = evt.newRect.height},
                    firstStart ? tmpAnimTime : m_AnimationTime);

            expandContainerItems.style.height = evt.oldRect.height;

            foldoutAnimation.KeepAlive();
            expandContainerItems.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
        }
    }
}
