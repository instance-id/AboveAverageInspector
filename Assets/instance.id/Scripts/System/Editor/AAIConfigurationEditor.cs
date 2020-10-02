using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace instance.id.AAI.Editors
{
    [CustomEditor(typeof(AAIConfiguration))]
    public class AAIConfigurationEditor : AAIDefaultEditor
    {
        private AAIConfiguration configuration;
        private VisualElement rootContainer;

        [UICategory("Locator Paths")] private Button refreshPaths;
        [UICategory("Styles")] private Button getStyleSheets;

        // ---------------------------------------------------------- Initialization
        // -- BaseAwake ------------------------------------------------------------
        protected override void BaseAwake()
        {
            configuration = target as AAIConfiguration;
        }

        // ------------------------------------------------------------ BaseOnEnable
        // -- BaseOnEnable ---------------------------------------------------------
        protected override void BaseOnEnable()
        {
            beforeDefaultElements = new VisualElement();

            excludedFields.Add("description");
            rootContainer = new VisualElement();

            var headerLabel = new Label("Above-Average Inspector");
            headerLabel.AddToClassList("headerLabel");
            beforeDefaultElements.Add(headerLabel);

            var descriptionText = new TextField
            {
                bindingPath = serializedObject.FindProperty("description").propertyPath
            };

            descriptionText.SetEnabled(false);
            descriptionText.AddToClassList("descriptionTextField");
            beforeDefaultElements.Add(descriptionText);

            rootContainer.AddToClassList("rootContainer");

            // --- Primary Box Container -----------------------
            var container = new Box();

            // --- Path Refresh Button -------------------------
            refreshPaths = new Button(RefreshPaths) {text = "Refresh Paths", name = "refreshPaths"};
            refreshPaths.AddToClassList("interfaceButton");

            // --- Get Events Button ---------------------------
            getStyleSheets = new Button(GetStyleSheets) {text = "Refresh StyleSheets", name = "getStyleSheets"};
            getStyleSheets.AddToClassList("interfaceButton");

            container.Add(refreshPaths);
            container.Add(getStyleSheets);
            rootContainer.Add(container);
            afterDefaultElements = new VisualElement();
            afterDefaultElements.Add(rootContainer);
        }

        // ------------------------------------------------------------ RefreshPaths
        // -- RefreshPaths ---------------------------------------------------------
        private void RefreshPaths()
        {
            configuration.RefreshPaths();
            Debug.Log("Refreshing Paths");
            EditorUtility.SetDirty(configuration);
            AssetDatabase.SaveAssets();
        }

        // ---------------------------------------------------------- GetStyleSheets
        // -- GetStyleSheets -------------------------------------------------------
        private void GetStyleSheets() { idConfig.AAIConfiguration().BuildStyleSheet(true); }
    }
}
