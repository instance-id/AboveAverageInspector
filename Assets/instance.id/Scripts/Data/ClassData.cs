using System;
using System.Collections.Generic;
using UnityEngine;

namespace instance.id.AAI
{
    [Serializable]
    public class ClassData
    {
        [SerializeField] public string typeName;
        [SerializeField] public Dictionary<string, FieldData> fieldDatas = new Dictionary<string, FieldData>();
        [SerializeField] public List<UICategory> categoryList = new List<UICategory>();

        public ClassData(Type type)
        {
            typeName = type.Name;
        }
    }
}
