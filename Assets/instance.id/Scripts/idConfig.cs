using System;
using System.Collections.Generic;
using System.Linq;
using instance.id.AAI.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace instance.id.AAI
{
    public static class idConfig
    {
        private static string aaiConfigurationType = "AAIConfiguration";
        static string stylesheetTerms = "t:StyleSheet AAI";
        static string layoutTerms = "t:VisualTreeAsset AAI";
        private static AAIConfiguration aaiConfiguration = null;
        public static string idPath;
        public static string objectPath;
        public static StyleSheet[] stylesheets;
        public static VisualTreeAsset[] layouts;
        private static Dictionary<string, StyleSheet> styleSheetDict = new Dictionary<string, StyleSheet>();
        private static Dictionary<string, VisualTreeAsset> layoutDict = new Dictionary<string, VisualTreeAsset>();

        public static List<string> GetExclusionList() => AAIConfiguration().aaiTypeStringList;
        public static AAIConfiguration AAIConfiguration() // @formatter:off
        {
            try
            {
                if (!(aaiConfiguration is null))
                    return aaiConfiguration;
            }
            catch (Exception) { GetConfiguration(); }
            return aaiConfiguration;
        } // @formatter:on

        // @formatter:off ------------------------------------------------------------------------- Debug flags
        //  -- Debug flags ------------------------------------------------------------------------------------
        #region Debug
        public static bool configurationDebug
        { get { if (!(aaiConfiguration is null)) return aaiConfiguration.configurationDebug; return false; }
            set => aaiConfiguration.configurationDebug = value; }
        #endregion

        public static void CheckExistence() // @formatter:on
        {
            GetConfiguration();
            LoadLayouts(true);
            LoadStyleSheets();
        }

        static idConfig()
        {
            GetConfiguration();
            LoadLayouts(true);
            LoadStyleSheets();
        }

        private static bool debug;

        private static void Assignments()
        {
            // -- Assign path values ---------------------------
            idPath = aaiConfiguration.GetLocation();
            objectPath = aaiConfiguration.objectPath;

            // -- Get saved debug flags ------------------------
            if (!(aaiConfiguration is null))
                debug = aaiConfiguration.idConfigDebug;
        }


        // ----------------------------------------------------------------------------- GetLocator
        // -- The AAIConfiguration keeps track of the current path of the AAI Asset as     --
        // -- well as stores various other debug and configuration data needed by AAI      --
        // -- GetLocator --------------------------------------------------------------------------
        private static void GetConfiguration()
        {
            if (!(aaiConfiguration is null)) return;
            aaiConfiguration = AssetDatabase.FindAssets($"t:{aaiConfigurationType}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<AAIConfiguration>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault();

            if (!(aaiConfiguration is null)) Assignments();
            else
            {
                var locator = ScriptableObject.CreateInstance<AAIConfiguration>();
                idPath = locator.GetLocation();
                Object.DestroyImmediate(locator);
                var assetName = $"{aaiConfigurationType}.asset";
                var path = $"{idPath}/{assetName}";
                if (debug) Debug.Log($"Path: {path}");
                aaiConfiguration = ScriptableObjectExtensions.CreateAsset<AAIConfiguration>(path);
                aaiConfiguration.enableCustomEditors = true;
                Assignments();
                Debug.LogWarning("Could not locate the AAIConfiguration object. AAI will attempt to recreate it.");
            }
        }

        #region Editor Style Data

        // @formatter:off ----------------------------------------------------------------- Layouts
        // -- Layouts -----------------------------------------------------------------------------
        #region Layouts
        // ---------------------------------------------------------------------------- LoadLayouts
        // -- Initial loading of the AAI Layouts                                           --
        // -- LoadLayouts -------------------------------------------------------------------------
        private static void LoadLayouts(bool force = false) // @formatter:on
        {
            if (aaiConfiguration == null) GetConfiguration();
            if (layoutDict.Count > 0) return;
            try
            {
                layouts = aaiConfiguration.aaiLayouts.ToArray();
                if (debug) Debug.Log($"Layout: {layouts.Length.ToString()}");
                Assert.IsNotNull(layouts);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError($"Layout not found in Layout cache. Loading from disk");
                layouts = SearchLayouts();
            }

            if (force) layouts = SearchLayouts();

            layoutDict = new Dictionary<string, VisualTreeAsset>();
            foreach (var layout in layouts)
            {
                if (debug) Debug.Log($"{layout.name}:{layout}");
                layoutDict.TryAddValue(layout.name, layout);
            }
        }

        // -------------------------------------------------------------------------- SearchLayouts
        // -- SearchLayouts -----------------------------------------------------------------------
        private static VisualTreeAsset[] SearchLayouts()
        {
            aaiConfiguration.aaiLayouts.Clear();
            var layout = AssetDatabase.FindAssets(layoutTerms).Select(guid => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
            aaiConfiguration.aaiLayouts = layout.ToList();
            SaveAssetData(aaiConfiguration);
            return layout;
        }

        // ------------------------------------------------------------------------------ GetLayout
        // -- The Layout is used for building the AAI editor windows                       --
        // -- GetLayouts --------------------------------------------------------------------------
        public static VisualTreeAsset GetLayouts(string layoutName)
        {
            if (debug) Debug.Log($"Get Style: {layoutName}");
            if (aaiConfiguration == null) GetConfiguration();
            return ReturnLayouts(layoutName);
        }

        // -------------------------------------------------------------------------- ReturnLayouts
        // -- Return the requested Layout if it is loaded. If not, load and return it.           --
        // -- ReturnLayouts -----------------------------------------------------------------------
        private static VisualTreeAsset ReturnLayouts(string layoutName)
        {
            if (debug) Debug.Log($"ReturnLayout {layoutName}");
            if (layoutDict.TryGetValue(layoutName, out var layout)) return layout;
            if (debug) Debug.Log($"{layoutName} not in layoutDict, trying next step");
            LoadLayouts(true);
            if (layoutDict.TryGetValue(layoutName, out layout)) return layout;
            return null;
        }

        #endregion

        // @formatter:off ------------------------------------------------------------- StyleSheets
        // -- StyleSheets -------------------------------------------------------------------------
        #region StyleSheets
        // ------------------------------------------------------------------------ LoadStyleSheets
        // -- Initial loading of the AAI StyleSheets                                       --
        // -- LoadStyleSheets ---------------------------------------------------------------------
        private static void LoadStyleSheets(bool force = false) // @formatter:on
        {
            if (aaiConfiguration == null) GetConfiguration();
            if (styleSheetDict.Count > 0 || !force) return;
            try
            {
                stylesheets = aaiConfiguration.aaiStyleSheets.ToArray();
                if (debug) Debug.Log($"stylesheets: {stylesheets.Length.ToString()}");
                Assert.IsNotNull(stylesheets);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError($"StyleSheets not found in StyleSheet cache. Loading from disk");
                stylesheets = SearchSheets();
            }

            if (force)
            {
                stylesheets = SearchSheets();
            }

            styleSheetDict = new Dictionary<string, StyleSheet>();
            foreach (var stylesheet in stylesheets)
            {
                if (debug) Debug.Log($"{stylesheet.name}:{stylesheet}");
                styleSheetDict.Add(stylesheet.name, stylesheet);
            }
        }

        private static StyleSheet[] SearchSheets()
        {
            var sheets = AssetDatabase.FindAssets(stylesheetTerms).Select(guid => AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
            aaiConfiguration.aaiStyleSheets = sheets.ToList();
            SaveAssetData();
            return sheets;
        }

        // -------------------------------------------------------------------------- GetStyleSheet
        // -- The StyleSheet is used for styling the AAI editor windows                    --
        // -- GetStyleSheet -----------------------------------------------------------------------
        public static StyleSheet GetStyleSheet(this Type type, string styleName = null)
        {
            if (!(styleName is null))
            {
                if (debug) Debug.Log($"Get Style: {styleName}");
                if (aaiConfiguration == null) GetConfiguration();
                return ReturnStyleSheet(styleName);
            }

            styleName = $"{type.Name}Style";
            if (debug) Debug.Log($"Get Style: {styleName}");
            if (aaiConfiguration == null) GetConfiguration();
            return ReturnStyleSheet(styleName);
        }

        public static StyleSheet GetStyleSheet(string styleName)
        {
            if (debug) Debug.Log($"Get Style: {styleName}");
            if (aaiConfiguration == null) GetConfiguration();
            return ReturnStyleSheet(styleName);
        }

        // ----------------------------------------------------------------------- ReturnStyleSheet
        // -- Return the requested StyleSheet if it is loaded. If not, load and return it.       --
        // -- ReturnStyleSheet --------------------------------------------------------------------
        private static StyleSheet ReturnStyleSheet(string styleName)
        {
            if (debug) Debug.Log($"ReturnStyleSheet {styleName}");
            if (styleSheetDict.TryGetValue(styleName, out var style)) return style;
            if (debug) Debug.Log($"{styleName} not in styleSheetDict, trying next step");
            LoadStyleSheets(true);
            if (styleSheetDict.TryGetValue(styleName, out style)) return style;
            return null;
        }

        #endregion

        #endregion

        // @formatter:off ---------------------------------------------------------- IsExcludedType
        // -- Checks if the object is a of the excluded types                                    --
        // -- IsExcludedType ----------------------------------------------------------------------
        public static bool IsExcludedType(this GameObject gameObject)
        {
            var exclusionList = GetExclusionList();
            var list = gameObject.GetComponents(typeof(Component));
            for (var i = 0; i < list.Length; i++)
            {
                if (exclusionList.Contains(list[i].GetType().Name)) {return true;}
            }
            return false; // @formatter:on
        }

        // -------------------------------------------------------------------------- SaveAssetData
        // -- SaveAssetData -----------------------------------------------------------------------
        public static void SaveAssetData(Object obj = null)
        {
            obj ??= AAIConfiguration();
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
