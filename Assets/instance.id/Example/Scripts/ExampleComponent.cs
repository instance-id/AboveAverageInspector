using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace instance.id.AAI
{
    [ExecuteInEditMode]
    [Serializable]
    public class ExampleComponent : MonoBehaviour
    {
        [UICategory("Character", 0, true)] public string characterName;
        [UICategory("Character", 0, true)] public string location;

        [UICategory("Stats", 1, true)] public int health;
        [UICategory("Stats", 1, true)] public int mana;
        [UICategory("Stats", 1, true)] public int lives;

        [UICategory("Combat", 2)] public List<WeaponBase> availableWeapons = new List<WeaponBase>();

        [UICategory("Configuration", 3)] public GameObject playerModel;
        [UICategory("Configuration", 3)] public bool isNPC;
        [UICategory("Configuration", 3)] public Vector3 spawnLocation;

        public bool showHat;

#if UNITY_EDITOR
        private void OnValidate()
        {
            var terms = "t:WeaponBase";
            availableWeapons = AssetDatabase.FindAssets(terms)
                .Select(guid => AssetDatabase.LoadAssetAtPath<WeaponBase>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();
        }
#endif
    }
}
