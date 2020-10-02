using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace instance.id.AAI.Extensions
{
    public static class FieldExtensions
    {
        // @formatter:off ----------------------------------------------- GetClassFields()
        // -- Get all fields and related data from within a particular class
        // -- GetClassFields() ----------------------------------------------------------
        public static ClassData GetClassFields(this Type type, bool sortOutput = false)
        {
            var thisType = type; // @formatter:on
            var classData = new ClassData(thisType);
            try // @formatter:off
            {
                var fieldList = thisType.GetFields();
                var fieldInfos = sortOutput
                    ? fieldList.OrderBy(x => x.Name).ToList()
                    : fieldList.ToList();

                for (var i = 0; i < fieldInfos.Count; i++)
                {
                    classData.fieldDatas.TryAddValue(fieldInfos[i].Name, new FieldData(fieldInfos[i]));
                    classData.categoryList.TryAddValue(classData.fieldDatas[fieldInfos[i].Name].categoryAttr);
                }
            } catch (Exception e) { Debug.LogException(e); }

            return classData; // @formatter:on
        }

        public static ClassData GetEditorAttributes(this object obj, bool sortOutput = false)
        {
            var thisType = obj.GetType();
            var classData = new ClassData(thisType);
            try // @formatter:off
            {
                foreach (var field in thisType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!(field.GetCustomAttributes(typeof(UICategory), true).FirstOrDefault() is UICategory att)) continue;
                    classData.fieldDatas.TryAddValue(field.Name, new FieldData(field));
                    classData.categoryList.TryAddValue(classData.fieldDatas[field.Name].categoryAttr);
                }
            } catch (Exception e) { Debug.LogException(e); }

            return classData; // @formatter:on
        }
    }
}
