using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using instance.id.AAI.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Analytics;
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
        public bool defaultEditorDebug;

        // -- Containers for custom elements from deriving classes -------
        public VisualElement beforeDefaultElements;
        public VisualElement afterDefaultElements;
        public StyleSheet defaultStyleSheet;
        private List<Foldout> foldout;

        public bool showScript;
        public List<string> excludedFields = new List<string>();

        // ReSharper disable once NotAccessedField.Local
        private string m_IMGUIPropNeedsRelayout;
        private ScrollView m_ScrollView;

        protected SerializedDictionary<string, ClassData> classDataDictionary = new SerializedDictionary<string, ClassData>();
        protected List<VisualElement> categoryList = new List<VisualElement>();
        protected List<VisualElement> editorElements = new List<VisualElement>();
        private List<VisualElement> expanders = new List<VisualElement>();
        private List<string> keyData = new List<string>();

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
        }

        // ------------------------------------------------------------ GetFieldData
        // -- Get field and type data from the editor target class                --
        // -- GetFieldData ---------------------------------------------------------
        private SerializedDictionary<string, ClassData> GetFieldData(bool needsRefresh = true, bool displayData = false)
        {
            if (idConfig.AAIConfiguration().refreshClassData) idConfig.AAIConfiguration().classDataDictionary = new SerializedDictionary<string, ClassData>();
            needsRefresh = idConfig.AAIConfiguration().refreshClassData;
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
            var fieldData = classData.fieldDatas;

            // -- Locate Default category, remove it, reorder the --------
            // -- categories as desired, replace Default at the end ------
            UICategory defaultCategory = new UICategory("Default");
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

            var isAnimated = idConfig.AAIConfiguration().enableAnimation;
            if (classData.categoryList.Count < 1) return classDict;
            {
                classData.categoryList.RemoveAll(x => Equals(x, defaultCategory));
                classData.categoryList = classData.categoryList.OrderBy(x => x.order).ToList();
                classData.categoryList.Add(defaultCategory);

                classData.categoryList.ForEach(x =>
                {
                    bool expand;
                    expand = idConfig.AAIConfiguration().expandCategoriesByDefault || x.expand;

                    VisualElement element = isAnimated
                        ? new Foldout {name = x.category, text = x.category, value = false}
                        : new Foldout {name = x.category, text = x.category, value = expand};
                    categoryList.Add(element);
                });
            }
            return classDict;
        }

        // public void OnInspectorGUI(bool isScriptInspector, RectOffset margins, EditorWindow currentInspector)
        // {
        //     if (currentInspector)
        //         this.currentInspector = currentInspector;
        // }

        public override VisualElement CreateInspectorGUI()
        {
            if (Selection.activeObject is null || Selection.objects.Length == 0) return base.CreateInspectorGUI();
            if (!GetType().IsSubclassOf(typeof(ScriptableObject)) || categoryList is null || categoryList.Count == 0) return base.CreateInspectorGUI();

            if (!idConfig.AAIConfiguration().enableCustomEditors)
            {
                return base.CreateInspectorGUI();
            }

            var baseStyleSheet = idConfig.GetStyleSheet($"AAIDefaultEditorBase");
            defaultStyleSheet ??= idConfig.GetStyleSheet($"AAIDefaultEditorStyle");
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
                            case "m_Script" when serializedObject.targetObject
                                                 != null:

                                propertyField.visible = false;
                                propertyField.SetEnabled(false);
                                break;
                            default:
                                if (property.IsReallyArray() && serializedObject.targetObject != null)
                                {
                                    var copiedProperty = property.Copy();
                                    var imDefaultProperty = new IMGUIContainer(() => { DoDrawDefaultIMGUIProperty(serializedObject, copiedProperty); })
                                        {name = propPath};

                                    m_ScrollView.Add(imDefaultProperty);
                                    continue;
                                }

                                break;
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
                                if (defaultEditorDebug) Debug.Log($"Dictionary: {propPath}");

                                var dictionaryFoldout = new Foldout {text = property.displayName};
                                dictionaryFoldout.AddToClassList("arrayFoldout");
                                dictionaryFoldout.value = false;

                                dictionaryFoldout.Add(propertyField);
                                propertyColumn.Add(dictionaryFoldout);
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyColumn);
                                break;

                            // -- List/Set Elements ----------------------
                            case { } a when typeof(IList).IsAssignableFrom(a.FieldType):
                            case { } b when typeof(IList).IsSubclassOf(b.FieldType):
                            case { } c when typeof(ISet<>).IsAssignableFrom(c.FieldType):
                            case { } d when typeof(ISet<>).IsSubclassOf(d.FieldType):
                                if (defaultEditorDebug) Debug.Log($"List: {propPath}");

                                var arrayElementBuilder = new ArrayElementBuilder(property, propertyData);
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
                                propertyObjectLabel.AddToClassList("propertyObjectLabel");
                                propertyObjectField.AddToClassList("propertyObjectField");
                                propertyRow.Add(propertyObjectLabel);
                                propertyRow.Add(propertyObjectField);
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyRow);
                                if (defaultEditorDebug) Debug.Log($"Objects: {propPath}");
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

                                if (defaultEditorDebug) Debug.Log($"Fallback: {propPath}");
                                boxContainer.Q(propertyData.categoryAttr.category).Add(propertyColumn);
                                break;
                        }
                    }
                } while (property.NextVisible(false));
            }

            #endregion


            foreach (var foldout in m_ScrollView.Query<Foldout>().ToList())
            {
                foldout.RegisterValueChangedCallback(e =>
                {
                    var fd = e.target as Foldout;
                    if (fd == null) return;
                    var path = fd.bindingPath;
                    var container = m_ScrollView.Q<IMGUIContainer>(path);
                    RecomputeSize(container);
                });
            }

            var defaultCategory = categoryList
                .FirstOrDefault(x => x.name == "Default");
            if (defaultCategory.childCount == 0) defaultCategory.style.display = DisplayStyle.None;

            foldout = boxContainer.Query<Foldout>().ToList();
            foldout.ForEach(x =>
            {
                x.Q<Toggle>().ToggleInClassList("categoryFoldoutClosed");
                x.Q(null, "unity-toggle__checkmark").AddToClassList("toggleCheckmark");

                if (idConfig.AAIConfiguration().enableAnimation)
                {
                    var categoryFoldout = x;
                    var content = categoryFoldout.Children().ToList();
                    if (content.Count == 0) return;

                    var categoryExpander = new UIElementExpander();
                    content.ForEach(c =>
                    {
                        c.RemoveFromHierarchy();
                        categoryExpander.AddToGroup(c);
                    });
                    categoryExpander.name = $"{x.name}Expander";

                    x.RegisterCallback((ChangeEvent<bool> evt) =>
                    {
                        if (evt.target == x)
                        {
                            categoryExpander.Activate(evt.newValue);
                            evt.StopPropagation();
                        }
                        else categoryExpander.TriggerValueChange(true);
                    });

                    x.ToggleInClassList("categoryFoldoutClosed");
                    x.Add(categoryExpander);
                }
            });

            // if (idConfig.AAIConfiguration().enableAnimation)
            // {
            //     categoryList.ForEach(x =>
            //     {
            //         if (x is null) return;
            //         var categoryFoldout = (Foldout) x;
            //
            //         var content = categoryFoldout.Children().ToList();
            //         if (content.Count == 0) return;
            //
            //         var categoryExpander = new UIElementExpander();
            //         content.ForEach(c =>
            //         {
            //             c.RemoveFromHierarchy();
            //             categoryExpander.AddToGroup(c);
            //         });
            //         categoryExpander.name = $"{x.name}Expander";
            //
            //         x.RegisterCallback((ChangeEvent<bool> evt) =>
            //         {
            //             if (evt.target == x)
            //             {
            //                 categoryExpander.Activate(evt.newValue);
            //                 evt.StopPropagation();
            //             }
            //
            //             else categoryExpander.TriggerValueChange(true);
            //         });
            //
            //         x.ToggleInClassList("categoryFoldoutClosed");
            //
            //         // var testFoldout = new Foldout() {text = x.name};
            //         // categoryExpander.shownItem = testFoldout;
            //         x.Add(categoryExpander);
            //         // categoryFoldout.Add(testFoldout);
            //     });
            // }

            // var foldoutToggle = categoryFoldout.Q<Toggle>(className: Foldout.toggleUssClassName);
            // var foldoutLabel = foldoutToggle.Q<Label>(className: Toggle.textUssClassName);
            // x.RegisterCallback((GeometryChangedEvent evt) =>
            // {
            //     if (evt.currentTarget != x)
            //     {
            //         categoryExpander.TriggerGeometryChange(evt);
            //         evt.StopPropagation();
            //     }
            // });

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
                if (!(secondData is null))
                    secondData.fieldDatas.ForEach(x =>
                    {
                        var category = defaultRoot.Q<Foldout>(x.Value.categoryAttr.category);
                        var button = buttons.FirstOrDefault(b => b.name == x.Value.name);
                        if (button == null) return;

                        button.RemoveFromHierarchy();
                        category.Add(button);
                    });

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
                var categoryFoldout = (Foldout) x;
                var catList = classDataDictionary[keyData[0]].categoryList;
                UICategory category = null;
                for (var index = 0; index < catList.Count; index++)
                {
                    var e = catList[index];
                    if (e.category != categoryFoldout.name) continue;
                    category = e;
                    break;
                }

                var isExpanded = category != null && category.expand;
                categoryFoldout.SetValueWithoutNotify(idConfig.AAIConfiguration().expandCategoriesByDefault || isExpanded);

                var delayedTime = (long) (index * 0.13 * 1000); // @formatter:off
                defaultRoot.schedule.Execute(e =>
                {
                    categoryFoldout.Q<UIElementExpander>().Activate(categoryFoldout.value);
                }).StartingIn(delayedTime);
                index++;
                // @formatter:on
            });
        }

        // @formatter:off ------------------------------- categoryToggleCB
        // -- In place of using a hover state, as this actually worked  --
        // -- categoryToggleCB -------------------------------------------
        private void categoryToggleCB(ChangeEvent<bool> evt)
        {
            if (defaultEditorDebug) Debug.Log($"Event: {evt.newValue.ToString()}");
            if (evt.newValue)
                foldout.ForEach(x => x.RemoveFromClassList("categoryFoldoutClosed"));
            else foldout.ForEach(x => x.AddToClassList("categoryFoldoutClosed"));
        } // @formatter:on


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
