// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/AboveAverageInspector         --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using instance.id.AAI.Extensions;
using UnityEngine;

namespace instance.id.AAI
{
    [Serializable]
    public class FieldData
    {
        public Type fieldType;
        public FieldInfo fieldInfo;
        [SerializeField] public UICategory categoryAttr;
        [SerializeField] public string name;
        [SerializeField] public string fieldTypeString;
        [SerializeField] public List<string> fieldTypeParametersString;
        [SerializeField] public List<Type> fieldTypeParameters;

        public FieldData(FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
            name = fieldInfo.Name;
            fieldType = fieldInfo.FieldType;
            fieldTypeString = fieldInfo.FieldType.GetRealTypeName();
            fieldTypeParameters = fieldInfo.FieldType
                .GetGenericArguments()
                .Select(x => x)
                .ToList();

            fieldTypeParametersString = fieldInfo.FieldType
                .GetGenericArguments()
                .Select(x => x.Name.ToString())
                .ToList();

            CheckForAttributes(fieldInfo);
        }

        private void CheckForAttributes(FieldInfo fieldInfo)
        {
            var catAttrib = (UICategory) Attribute.GetCustomAttribute(fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo)), typeof(UICategory));
            categoryAttr = catAttrib ?? new UICategory("Default");
        }
    }
}
