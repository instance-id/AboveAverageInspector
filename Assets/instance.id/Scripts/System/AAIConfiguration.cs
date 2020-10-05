// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/AboveAverageInspector         --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using instance.id.AAI.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace instance.id.AAI
{
    // -- AAIConfigurationEditor.cs -------------------------------
    [Serializable]
    public class AAIConfiguration : ScriptableObject
    {
        [SerializeField, ReadOnly] public string description = "Do not delete or move! This file is used by AAI.";
        private const string typeExclusionSearch = "AAI t:script";

        // @formatter:off ------------------------ Main Settings
        // -- Main Settings ------------------------------------

                // -------------------------------------------- Settings
        // -- Settings -----------------------------------------
        [UICategory("AAI Settings", 0)] public bool enableAnimation;
        [UICategory("AAI Settings", 0)] public bool expandCategoriesByDefault;
        [UICategory("AAI Settings", 0)] public bool enableCustomEditors;

        // @formatter:off ----------------- Location Data Fields
        // -- Location Data Fields -----------------------------
        [UICategory("AAI Paths", 1), ReadOnly] public string location;
        [UICategory("AAI Paths", 1)] public string pathString;
        [UICategory("AAI Paths", 1)] public string objectPath;
        [UICategory("AAI Paths", 1)] public string stylePath;

        // ---------------------------------------------- Styles
        // -- Styles -------------------------------------------
        [UICategory("Styles",2)] [SerializeField] public List<StyleSheet> aaiStyleSheets = new List<StyleSheet>();
        [UICategory("Styles",2)] [SerializeField] [Dictionary] public SerializedDictionary<string,StyleSheet> aaiStyleSheetsDictionary = new SerializedDictionary<string,StyleSheet>();
        [UICategory("Styles",2)] [SerializeField] public List<VisualTreeAsset> aaiLayouts = new List<VisualTreeAsset>();

        [UICategory("Class Data",3)] [SerializeField] public List<string> aaiTypeStringList = new List<string>();
        [UICategory("Class Data",3)] [SerializeField] public List<Type> aaiTypeList = new List<Type>();
        [UICategory("Class Data",3)] [SerializeField] public List<string> drawerTypesList = new List<string>();


        public Dictionary<string, Type> drawerTypesDictionary = new Dictionary<string, Type>();
        [HideInInspector] public SerializedDictionary<string, ClassData> classDataDictionary = new SerializedDictionary<string, ClassData>();
        [HideInInspector] public bool refreshClassData;
        private int foundTypes;

        // @formatter:off -------------------------- Debug flags
        // -- Debug flags --------------------------------------
        [UICategory("Debug",4)]  public bool configurationDebug;
        [UICategory("Debug",4)]  public bool defaultEditorDebug;
        [UICategory("Debug",4)]  public bool idConfigDebug;

        // @formatter:on ------------------------ Method Getters
        // -- Method Getters -----------------------------------
        public string RefreshPaths() => GetPaths(true);
        public string GetLocation() => GetPaths(getLocation: true);

#if UNITY_EDITOR
        public static AAIConfiguration Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            BuildStyleSheet();
            GetPaths();
            GetTypeExclusions(true);
        }

        private void OnEnable()
        {
            BuildStyleSheet();
            GetPaths();
            GetTypeExclusions(true);
        }

        // ---------------------------------------------------------------
        // -- Reflection Testing -----------------------------------------
        public void SomeScriptToAccessFromEditors()
        {
            var types = ReflectionExtensions.GetTypesInNamespace(
                ReflectionExtensions.GetAssemblyByName("Assembly-CSharp-Editor"), "instance.id.AAI.Editors");

            Type defaultEditorDrawer = null;
            for (var index = 0; index < types.Length; index++)
            {
                var t = types[index];
                if (t.Name != "AAIDefaultEditor") continue;
                defaultEditorDrawer = t;
                break;
            }

            var drawerTypesList = types
                .Where(type => type.IsSubclassOf(
                    defaultEditorDrawer ??
                    throw new NullReferenceException("Cannot find AAIDefaultEditor Type")))
                .ToList();

            drawerTypesList.ForEach(x =>
            {
                drawerTypesDictionary.TryAddValue(x.Name, x);
                this.drawerTypesList.TryAddValue(x.Name);
            });
        }

        private void GetTypeExclusions(bool refresh = false)
        {
            var exclusionList = AssetDatabase.FindAssets(typeExclusionSearch)
                .Select(guid => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();

            aaiTypeStringList = exclusionList.ToList();
            if (foundTypes != exclusionList.Count || refresh)
            {
                // aaiTypeStringList.Clear();
                foreach (var exclusion in exclusionList) aaiTypeStringList.TryAddValue(exclusion);

                if (configurationDebug) Debug.LogWarning($"New Type Found: Prior:{foundTypes.ToString()} Found: {aaiTypeStringList.Count.ToString()}");
                foundTypes = aaiTypeStringList.Count;
            }
        }

        // ------------------------------------------------------------------------------- GetPaths
        // -- GetPaths ----------------------------------------------------------------------------
        private string GetPaths(bool refresh = false, bool getLocation = false)
        {
            description = "Do not delete or move! This file is used by AAI.";
            var currentLocation = GetAssetPath();
            if (location == currentLocation && !refresh) return currentLocation;

            location = currentLocation;
            objectPath = $"{location}/Objects/";
            stylePath = $"{location}/Scripts/Layout/Style/";

            var dataPath = Application.dataPath;
            pathString = dataPath.Substring(0, dataPath.Length - "Assets".Length);

            if (configurationDebug) Debug.Log($"AAI Location: {location}");
            return getLocation ? location : objectPath;
        }

        // -------------------------------------------------------------------------- GetStylesheet
        // -- GetStylesheet -----------------------------------------------------------------------
        public List<StyleSheet> GetStylesheet(bool refresh = false)
        {
            if (aaiStyleSheets.Count != 0 && !refresh) return aaiStyleSheets;

            var terms = "t:StyleSheet AAI";
            aaiStyleSheets = AssetDatabase.FindAssets(terms)
                .Select(guid => AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();

            var sheetString = aaiStyleSheets.Aggregate("", (current, type) => current + $"{type} ");
            if (configurationDebug) Debug.Log($"{sheetString}");

            return aaiStyleSheets;
        }

        // -------------------------------------------------------------------------- GetStyleSheet
        // -- The StyleSheet is used for styling the AAI editor windows                    --
        // -- GetStyleSheet -----------------------------------------------------------------------
        public void BuildStyleSheet(bool refresh = false)
        {
            if (aaiStyleSheets.Count != 0 && !refresh) return;
            aaiStyleSheets.Clear();
            aaiStyleSheetsDictionary.Clear();

            aaiStyleSheets = GetStylesheet(refresh);

            for (var i = 0; i < aaiStyleSheets.Count; i++)
            {
                aaiStyleSheetsDictionary.TryAddValue(aaiStyleSheets[i].name, aaiStyleSheets[i]);
            }
        }

        // --------------------------------------------------------------------------- GetAssetPath
        // -- GetAssetPath ------------------------------------------------------------------------
        private string GetAssetPath()
        {
            var dirName = Path.GetDirectoryName(CurrentPath()) + "\\..\\" + "\\..\\";
            var fPath = Path.GetFullPath(ConvertPath(dirName));
            return DetermineAssetPath(ConvertPath(fPath));
        }

        private static string DetermineAssetPath(string absolutePath)
        {
            if (absolutePath.StartsWith(Application.dataPath)) return "Assets" + absolutePath.Substring(Application.dataPath.Length);
            else throw new ArgumentException("Full path does not contain the current project's Assets folder", nameof(absolutePath));
        }

        private string CurrentPath()
        {
            var script = MonoScript.FromScriptableObject(this);
            return AssetDatabase.GetAssetPath(script);
        }

        private string ConvertPath(string aPath)
        {
            return aPath.Replace("\\", "/");
        }
#endif
    }
}
