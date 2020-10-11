using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using instance.id.AAI.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace instance.id.AAI.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true, isFallback = true)]
    public class AAIDefaultEditor : Editor
    {
        // -- Visual Elements --------------------------------------------
        protected VisualElement defaultRoot;

        // -- Settings Data ----------------------------------------------
        public bool defaultEditorDebug;
        private bool isAnimated;
        private float animationTime;
        private float cascadeDelay;

        // -- Containers for custom elements from deriving classes -------
        public VisualElement beforeDefaultElements;
        public VisualElement afterDefaultElements;
        public StyleSheet defaultStyleSheet;
        private List<dynamic> foldout = new List<dynamic>();
        public List<string> excludedFields = new List<string>();

        // -- PropertyField VisualElement Items --------------------------
        // ReSharper disable once NotAccessedField.Local
        private string m_IMGUIPropNeedsRelayout;
        private ScrollView m_ScrollView;

        private List<VisualElement> categoryList = new List<VisualElement>();
        protected List<VisualElement> editorElements = new List<VisualElement>();

        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<VisualElement> expanders = new List<VisualElement>();
        private List<string> keyData = new List<string>();

        private SerializedDictionary<string, ClassData>
            classDataDictionary = new SerializedDictionary<string, ClassData>();

        // -- Begin For future implementation ----------------------------
        private static readonly List<AAIDefaultEditor> s_ActiveInspectors;

        static AAIDefaultEditor()
        {
            s_ActiveInspectors = new List<AAIDefaultEditor>();
        }

        public static void RepaintAll()
        {
            foreach (var inspector in s_ActiveInspectors)
            {
                inspector.Repaint();
            }
        }
        // -- End For future implementation ------------------------------

        // @formatter:off -------------------------------------- Accessors
        // -- Virtual methods to be called from child classes           --
        // Accessors -----------------------------------------------------
        protected virtual void ExecuteDeferredTask() {}
        protected virtual void ExecutePostBuildTask() {}
        protected virtual void BaseAwake() {}
        protected virtual void BaseOnEnable(){}
        private void Awake()
        {
            var config = idConfig.AAIConfiguration();
            try { defaultEditorDebug = config.defaultEditorDebug; }
            catch (Exception) { idConfig.CheckExistence(); }
            BaseAwake(); // @formatter:on
        }

        private void OnEnable() // @formatter:on
        {
            s_ActiveInspectors.Add(this);
            defaultStyleSheet ??= idConfig.GetStyleSheet("AAIDefaultEditorStyle");
            BaseOnEnable();
            if (idConfig.AAIConfiguration().enableCustomEditors)
                classDataDictionary = GetFieldData();
            animationTime = idConfig.AAIConfiguration().animationTime;
            cascadeDelay = idConfig.AAIConfiguration().cascadeDelay;
        }

        // ------------------------------------------------------------ GetFieldData
        // -- Get field and type data from the editor target class                --
        // -- GetFieldData ---------------------------------------------------------
        private SerializedDictionary<string, ClassData> GetFieldData()
        {
            if (idConfig.AAIConfiguration().refreshClassData) idConfig.AAIConfiguration().classDataDictionary = new SerializedDictionary<string, ClassData>();
            var needsRefresh = idConfig.AAIConfiguration().refreshClassData;
            var thisName = this.GetType().Name;
            var targetName = target.GetType().Name;
            var classDict = new SerializedDictionary<string, ClassData>();

            // --------------------------------------------- Experimental
            // -- Check for existing saved Target data ------------------
            if (idConfig.AAIConfiguration().classDataDictionary.TryGetValue(targetName, out var tmpClassData) && !needsRefresh)
            {
                classDict.TryAddValue(targetName, tmpClassData);
                if (defaultEditorDebug) Debug.Log($"Target: {classDict[targetName].typeName} Retrieved from Config");
            }
            // ----------------------------------------- Current Default
            // -- If it does not exist, create it ----------------------
            else
            {
                var attributes = target.GetType().GetClassFields(true);
                // idConfig.AAIConfiguration().classDataDictionary.TryAddValue(targetName, attributes);
                // idConfig.SaveAssetData();
                classDict.TryAddValue(targetName, attributes);
            }

            // --------------------------------------------- Experimental
            // -- Check for existing saved Editor data ------------------
            if (idConfig.AAIConfiguration().classDataDictionary.TryGetValue(thisName, out var tmpEditorClassData) && !needsRefresh)
            {
                classDict.TryAddValue(thisName, tmpEditorClassData);
                if (defaultEditorDebug) Debug.Log($"Editor: {classDict[thisName].typeName} Retrieved from Config");
            }
            // ----------------------------------------- Current Default
            // -- If it does not exist, create it ----------------------
            else
            {
                var attributes = this.GetEditorAttributes();
                // idConfig.AAIConfiguration().classDataDictionary.TryAddValue(thisName, attributes);
                // idConfig.SaveAssetData();
                classDict.TryAddValue(thisName, attributes);
            }

            // -- Configure categories -----------------------------------
            var classData = classDict[targetName];

            // -- Locate Default category, remove it, reorder the --------
            // -- categories as desired, replace Default at the end ------
            UICategory defaultCategory;
            try
            {
                classData.categoryList = classData.categoryList.Where(x => !(x is null) || !x.category.StartsWith(" ")).ToList();
                defaultCategory = classData.categoryList.Find(x => x.category == "Default");
            }
            catch (Exception e)
            {
                Debug.Log(e);
                idConfig.AAIConfiguration().refreshClassData = true;
                GetFieldData();
                return null;
            }

            isAnimated = idConfig.AAIConfiguration().enableAnimation;
            if (classData.categoryList.Count < 1) return classDict;
            {
                classData.categoryList.RemoveAll(x => Equals(x, defaultCategory));
                classData.categoryList = classData.categoryList.OrderBy(x => x.order).ToList();
                classData.categoryList.TryAddValue(defaultCategory);

                classData.categoryList.ForEach(x =>
                {
                    bool expand;
                    expand = idConfig.AAIConfiguration().expandCategoriesByDefault || x.expand || classData.categoryList.Count == 1;

                    if (isAnimated)
                    {
                        var element = new AnimatedFoldout {name = x.category, text = x.category, value = false};
                        if (!categoryList.Exists(x => x.name == element.name)) categoryList.TryAddValue(element);
                    }
                    else
                    {
                        var element = new Foldout {name = x.category, text = x.category, value = expand};
                        if (!categoryList.Exists(x => x.name == element.name)) categoryList.TryAddValue(element);
                    }
                });
            }
            return classDict;
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (Selection.activeObject is null || Selection.objects.Length == 0) return base.CreateInspectorGUI();
            if (!GetType().IsSubclassOf(typeof(ScriptableObject)) || categoryList is null || categoryList.Count == 0) return base.CreateInspectorGUI();
            if (!idConfig.AAIConfiguration().enableCustomEditors) return base.CreateInspectorGUI();

            var baseStyleSheet = idConfig.GetStyleSheet("AAIDefaultEditorBase");
            defaultStyleSheet ??= idConfig.GetStyleSheet("AAIDefaultEditorStyle");
            if (defaultStyleSheet is null) Debug.Log("Could not locate AAIDefaultEditorStyle");

            serializedObject.Update();

            defaultRoot = new VisualElement();

            defaultRoot.styleSheets.Add(baseStyleSheet);
            defaultRoot.styleSheets.Add(defaultStyleSheet);
            defaultRoot.AddToClassList("rootContainer");

            var boxContainer = new Box();
            boxContainer.AddToClassList("mainBoxContainer");

            beforeDefaultElements ??= new VisualElement();
            beforeDefaultElements.name = "beforeDefaultElements";
            beforeDefaultElements.AddToClassList("beforeDefaultElements");
            defaultRoot.Add(beforeDefaultElements);

            categoryList.ForEach(x =>
            {
                if (x is null) return;
                x.AddToClassList("categoryFoldout");
                boxContainer.Add(x);
            });

            m_ScrollView = new ScrollView();
            boxContainer.Add(m_ScrollView);

            keyData = classDataDictionary.Keys.ToList();

            #region Property Iteration

            var property = serializedObject.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    // -- Shortening name for ease of typing -------------
                    var propPath = property.propertyPath;

                    // -- Skip over excluded fields ----------------------
                    if (excludedFields.Contains(propPath) && serializedObject.targetObject != null)
                    {
                        continue;
                    }

                    // -- Property row VisualElement ---------------------
                    var propertyRow = new VisualElement();
                    var propertyColumn = new VisualElement();
                    propertyRow.AddToClassList("propertyRow");
                    propertyColumn.AddToClassList("propertyColumn");

                    // -- Property fallback field ------------------------
                    var propertyField = new PropertyField(property) {name = "PropertyField:" + propPath};

                    // -- Determine if current property is field data ----
                    if (!classDataDictionary[Enumerable.First(keyData)].fieldDatas.Keys.Contains(propPath))
                    {
                        switch (propPath)
                        {
                            case "m_Script" when serializedObject.targetObject != null:
                                propertyField.visible = false; // @formatter:off
                                propertyField.SetEnabled(false);
                                break;
                            default:
                                if (property.IsReallyArray() && serializedObject.targetObject != null)
                                {
                                    var copiedProperty = property.Copy();
                                    var imDefaultProperty = new IMGUIContainer(() =>
                                        {
                                            DoDrawDefaultIMGUIProperty(serializedObject, copiedProperty);
                                        }) {name = propPath};
                                    m_ScrollView.Add(imDefaultProperty);
                                }
                                break; // @formatter:on
                        }
                    }
                    else
                    {
                        var propertyData = classDataDictionary[Enumerable.First(keyData)].fieldDatas[propPath];
                        switch (propertyData.fieldInfo)
                        {
                            // -- String/TextField Elements --------------
                            case { } a when a.FieldType == typeof(string):
                            case { } b when b.FieldType == typeof(PropertyName):
                                if (defaultEditorDebug) Debug.Log($"String: {propPath}");

                                var propertyTextLabel = new Label(property.displayName);
                                propertyTextLabel.name = $"{propPath}Label";
                                var propertyTextField = new TextField
                                {
                                    bindingPath = propPath,
                                    name = $"{propPath}Text"
                                };

                                if (propertyData.categoryAttr.toolTip != "")
                                {
                                    propertyTextLabel.tooltip = propertyData.categoryAttr.toolTip;
                                    propertyTextField.tooltip = propertyData.categoryAttr.toolTip;
                                }

                                propertyTextLabel.AddToClassList("propertyTextLabel");
                                propertyTextField.AddToClassList("propertyTextField");
                                propertyRow.Add(propertyTextLabel);
                                propertyRow.Add(propertyTextField);
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyRow);
                                break;

                            // -- Integer Elements -----------------------
                            case { } a when a.FieldType == typeof(int):
                                if (defaultEditorDebug) Debug.Log($"Integer: {propPath}");
                                var propertyIntegerLabel = new Label(property.displayName);
                                propertyIntegerLabel.name = $"{propPath}Label";
                                var propertyIntegerField = new IntegerField
                                {
                                    bindingPath = propPath,
                                    name = $"{propPath}Integer"
                                };

                                if (propertyData.categoryAttr.toolTip != "")
                                {
                                    propertyIntegerLabel.tooltip = propertyData.categoryAttr.toolTip;
                                    propertyIntegerField.tooltip = propertyData.categoryAttr.toolTip;
                                }

                                propertyIntegerLabel.AddToClassList("propertyIntegerLabel");
                                propertyIntegerField.AddToClassList("propertyIntegerField");
                                propertyRow.Add(propertyIntegerLabel);
                                propertyRow.Add(propertyIntegerField);
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyRow);
                                break;

                            // -- Float Elements -------------------------
                            case { } a when a.FieldType == typeof(float):
                                if (defaultEditorDebug) Debug.Log($"Float: {propPath}");

                                var propertyFloatLabel = new Label(property.displayName);
                                propertyFloatLabel.name = $"{propPath}Label";
                                var propertyFloatField = new FloatField
                                {
                                    bindingPath = propPath,
                                    name = $"{propPath}Float"
                                };

                                if (propertyData.categoryAttr.toolTip != "")
                                {
                                    propertyFloatLabel.tooltip = propertyData.categoryAttr.toolTip;
                                    propertyFloatField.tooltip = propertyData.categoryAttr.toolTip;
                                }

                                propertyFloatLabel.AddToClassList("propertyFloatLabel");
                                propertyFloatField.AddToClassList("propertyFloatField");
                                propertyRow.Add(propertyFloatLabel);
                                propertyRow.Add(propertyFloatField);
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyRow);
                                break;

                            // -- Bool/Toggle Elements -------------------
                            case { } a when a.FieldType == typeof(bool):
                            case { } b when b.FieldType == typeof(Toggle):
                                if (defaultEditorDebug) Debug.Log($"Toggle: {propPath}");

                                var propertyToggleLabel = new Label(property.displayName);
                                propertyToggleLabel.name = $"{propPath}ToggleLabel";
                                var propertyToggleSpacer = new VisualElement();
                                var propertyToggleField = new Toggle
                                {
                                    bindingPath = propPath,
                                    name = $"{propPath}ToggleField"
                                };

                                if (propertyData.categoryAttr.toolTip != "")
                                {
                                    propertyToggleLabel.tooltip = propertyData.categoryAttr.toolTip;
                                    propertyToggleField.tooltip = propertyData.categoryAttr.toolTip;
                                }

                                propertyToggleLabel.AddToClassList("propertyToggleLabel");
                                propertyToggleLabel.AddToClassList("propertyToggleSpacer");
                                propertyToggleField.AddToClassList("propertyToggleField");
                                propertyRow.Add(propertyToggleLabel);
                                propertyRow.Add(propertyToggleField);
                                propertyRow.Add(propertyToggleSpacer);
                                propertyRow.RemoveFromClassList("propertyRow");
                                propertyRow.AddToClassList("propertyToggleRow");
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyRow);
                                break;

                            // -- Dictionary Elements --------------------
                            case { } a when typeof(IDictionary).IsAssignableFrom(a.FieldType):
                            case { } b when typeof(IDictionary).IsSubclassOf(b.FieldType):
                                var dictionaryFoldout = new Foldout {text = property.displayName};
                                dictionaryFoldout.AddToClassList("arrayFoldout");
                                dictionaryFoldout.value = false;

                                if (propertyData.categoryAttr.toolTip != "")
                                {
                                    dictionaryFoldout.tooltip = propertyData.categoryAttr.toolTip;
                                    propertyColumn.tooltip = propertyData.categoryAttr.toolTip;
                                }

                                dictionaryFoldout.Add(propertyField);
                                propertyColumn.Add(dictionaryFoldout);
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyColumn);
                                break;

                            // -- List/Set Elements ----------------------
                            case { } a when typeof(IList).IsAssignableFrom(a.FieldType):
                            case { } b when typeof(IList).IsSubclassOf(b.FieldType):
                            case { } c when typeof(ISet<>).IsAssignableFrom(c.FieldType):
                            case { } d when typeof(ISet<>).IsSubclassOf(d.FieldType):
                                var arrayElementBuilder = new ArrayElementBuilder(property, propertyData);

                                if (propertyData.categoryAttr.toolTip != "")
                                {
                                    propertyRow.tooltip = propertyData.categoryAttr.toolTip;
                                }

                                propertyRow.Add(arrayElementBuilder);
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyRow);
                                break;

                            // -- Object Elements ----------------------
                            case { } a when a.FieldType == typeof(Object):
                            case { } b when typeof(Object).IsSubclassOf(b.FieldType):
                            case { } c when typeof(Object).IsAssignableFrom(c.FieldType):
                                var propertyObjectLabel = new Label(property.displayName);
                                propertyObjectLabel.name = $"{propPath}ObjectLabel";
                                var propertyObjectField = new ObjectField
                                {
                                    objectType = propertyData.fieldType,
                                    bindingPath = propPath,
                                    name = $"{propPath}ObjectField"
                                };

                                if (propertyData.categoryAttr.toolTip != "")
                                {
                                    propertyObjectLabel.tooltip = propertyData.categoryAttr.toolTip;
                                    propertyObjectField.tooltip = propertyData.categoryAttr.toolTip;
                                }

                                propertyObjectLabel.AddToClassList("propertyObjectLabel");
                                propertyObjectField.AddToClassList("propertyObjectField");
                                propertyRow.Add(propertyObjectLabel);
                                propertyRow.Add(propertyObjectField);
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyRow);
                                if (defaultEditorDebug)
                                    Debug.Log($"Fallback Test: Name: {propPath} Type: {property.type} Array: {property.isArray} : {property.propertyType}");
                                break;
                            default:
                                if (property.IsReallyArray())
                                {
                                    propertyColumn.Add(propertyField);
                                    boxContainer.Q(propertyData.categoryAttr.category).Add(propertyColumn);
                                }
                                else propertyColumn.Add(propertyField);

                                if (propertyData.categoryAttr.toolTip != "")
                                {
                                    propertyColumn.tooltip = propertyData.categoryAttr.toolTip;
                                }

                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyColumn);
                                break;
                        }
                    }
                } while (property.NextVisible(false));
            }

            #endregion

            foreach (var foldoutList in m_ScrollView.Query<Foldout>().ToList())
            {
                foldoutList.RegisterValueChangedCallback(e =>
                {
                    if (!(e.target is Foldout fd)) return;
                    var path = fd.bindingPath;
                    var container = m_ScrollView.Q<IMGUIContainer>(path);
                    RecomputeSize(container);
                });
            }

            VisualElement defaultCategory = null;
            for (var i = 0; i < categoryList.Count; i++)
            {
                VisualElement x;
                if (isAnimated)
                    x = categoryList[i].Q<AnimatedFoldout>();
                else x = categoryList[i].Q<Foldout>();
                if (x.name != "Default") continue;
                defaultCategory = x;
                break;
            }

            if (defaultCategory.childCount == 0) defaultCategory.style.display = DisplayStyle.None;

            if (isAnimated)
            {
                var listItems = boxContainer.Query<AnimatedFoldout>().ToList();
                listItems.ForEach(x => foldout.Add((AnimatedFoldout) x));
            }
            else
            {
                var listItems = boxContainer.Query<Foldout>().ToList();
                listItems.ForEach(x => foldout.Add((Foldout) x));
            }

            foldout.ForEach(x =>
            {
                Toggle toggleItem;
                if (isAnimated)
                {
                    var item = (AnimatedFoldout) x;
                    var contentItem = item.Q(null, AnimatedFoldout.expanderUssClassName);
                    contentItem.ToggleInClassList("categoryFoldoutClosed");
                    item.Q(null, "unity-toggle__checkmark").AddToClassList("toggleCheckmark");
                    item.RegisterCallback((ChangeEvent<bool> evt) =>
                    {
                        if (evt.target == item)
                        {
                            item.expander.Activate(evt.newValue);
                            if (evt.newValue) item.contentContainer.style.display = DisplayStyle.Flex;

                            if (!evt.newValue) // @formatter:off
                            {
                                item.schedule.Execute(() =>
                                {
                                    item.contentContainer.style.display = DisplayStyle.None;
                                }).StartingIn(0);
                                item.schedule.Execute(() =>
                                {
                                    contentItem.style.display = DisplayStyle.None;
                                }).StartingIn(500); // @formatter:on
                            }
                        }
                        else item.expander.TriggerExpanderResize(true);
                    }); // @formatter:on
                }
                else
                {
                    var item = (Foldout) x;
                    toggleItem = item.Q<Toggle>();
                    toggleItem.ToggleInClassList("categoryFoldoutClosed");
                    item.Q(null, "unity-toggle__checkmark").AddToClassList("toggleCheckmark");
                }
            });

            serializedObject.ApplyModifiedProperties();

            afterDefaultElements ??= new VisualElement();
            afterDefaultElements.name = "afterDefaultElements";
            afterDefaultElements.AddToClassList("afterDefaultElements");

            boxContainer.Add(afterDefaultElements);
            defaultRoot.Add(boxContainer);

            defaultRoot.RegisterCallback<GeometryChangedEvent>(ExecutePostBuildTask);
            defaultRoot.schedule.Execute(ExecuteLocalDeferredTask).StartingIn(0);

            return defaultRoot;
        }

        // ------------------------------------------ ExecutePostBuildTask
        // -- Allows for execution of tasks after properties have       --
        // -- been built, but before the layout is actually drawn       --
        // -- ExecutePostBuildTask ---------------------------------------
        private void ExecutePostBuildTask(GeometryChangedEvent evt)
        {
            defaultRoot.UnregisterCallback<GeometryChangedEvent>(ExecutePostBuildTask);

            // -- Locates buttons added to CustomEditor which need categorization ----
            var buttons = defaultRoot.Query<Button>().ToList();
            var secondData = classDataDictionary[keyData[1]];
            if (buttons.Count > 0)
            {
                secondData?.fieldDatas.ForEach(x =>
                {
                    var category = defaultRoot.Q<Foldout>(x.Value.categoryAttr.category);
                    Button button = null;
                    for (var i = 0; i < buttons.Count; i++)
                    {
                        var b = buttons[i];
                        if (b.name != x.Value.name) continue;
                        button = b;
                        break;
                    }

                    if (button == null) return;

                    button.RemoveFromHierarchy();
                    category?.Add(button);
                });
            }

            ExecutePostBuildTask();
        }

        // -------------------------------------- ExecuteLocalDeferredTask
        // -- Allows for execution of tasks after first frame has drawn --
        // -- ExecuteLocalDeferredTask -----------------------------------
        private void ExecuteLocalDeferredTask()
        {
            // -- Calls tasks to be ran from child classes ---------------
            ExecuteDeferredTask();

            if (!idConfig.AAIConfiguration().enableAnimation) return;
            var index = 0;
            categoryList.ForEach(x =>
            {
                if (x is null) return;
                dynamic categoryFoldout;
                if (isAnimated) categoryFoldout = (AnimatedFoldout) x;
                else categoryFoldout = (Foldout) x;
                var catList = classDataDictionary[keyData[0]].categoryList;
                UICategory category = null;
                for (var i = 0; i < catList.Count; i++)
                {
                    var e = catList[i];
                    if (e.category != categoryFoldout.name) continue;
                    category = e;
                    break;
                }

                var isExpanded = category != null && category.expand || categoryList.Count == 1;
                categoryFoldout.SetValueWithoutNotify(idConfig.AAIConfiguration().expandCategoriesByDefault || isExpanded);

                // -- This creates a cascading expansion effect starting with the first category
                // -- delaying the expansion of the subsequent categories by the delayValue * 1000 (equates to milliseconds)
                var delayedTime = (long) (index * cascadeDelay * 1000); // @formatter:on
                defaultRoot.schedule.Execute(e =>
                {
                    if (isAnimated)
                    {
                        var foldoutItem = (AnimatedFoldout) categoryFoldout;
                        foldoutItem.SetValueWithoutNotify(categoryFoldout.value);
                        foldoutItem.expander.Activate(categoryFoldout.value);
                    }
                    else
                    {
                        var foldoutItem = (Foldout) categoryFoldout;
                        foldoutItem.SetValueWithoutNotify(categoryFoldout.value);
                    }
                }).StartingIn(delayedTime);
                index++; // @formatter:on
            });
        }

        // @formatter:off ------------------------------------- AddEntries
        // -- AddEntries -------------------------------------------------
        [UsedImplicitly]
        private void AddEntries(ArrayElementBuilder elementBuilder, int count) =>
            elementBuilder.schedule.Execute(() => elementBuilder.value += count);

        // @formatter:off ----------------------------------- LayerChanged
        // -- LayerChanged -----------------------------------------------
        #region IMGUI
        public void RecomputeSize(IMGUIContainer container) // @formatter:on
        {
            if (container == null) return;
            var parent = container.parent;
            container.RemoveFromHierarchy();
            parent.Add(container);
        }

        public void DoDrawDefaultIMGUIProperty(SerializedObject serializedObj, SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            serializedObj.Update();
            bool wasExpanded = property.isExpanded;
            EditorGUILayout.PropertyField(property, true);
            if (property.isExpanded != wasExpanded) m_IMGUIPropNeedsRelayout = property.propertyPath;
            serializedObj.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }

        #endregion

        private void OnDisable()
        {
            s_ActiveInspectors.Remove(this);
        }
    }
}
