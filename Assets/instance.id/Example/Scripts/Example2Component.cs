using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace instance.id.AAI
{
    [ExecuteInEditMode]
    [Serializable]
    public class Example2Component : MonoBehaviour
    {
        public string characterName;
        public string location;
        public int health;
        public int mana;
        public int lives;
        public List<WeaponBase> availableWeapons = new List<WeaponBase>();
        public GameObject playerModel;
        public bool isNPC;
        public Vector3 spawnLocation;
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
