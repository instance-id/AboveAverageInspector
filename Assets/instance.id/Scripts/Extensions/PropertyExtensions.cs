using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace instance.id.AAI.Extensions
{
    public static class PropertyExtensions
    {
        public static object GetValue(this SerializedProperty property)
        {
            var parentType = property.serializedObject.targetObject.GetType();
            var fi = parentType.GetField(property.propertyPath);
            return fi.GetValue(property.serializedObject.targetObject);
        }

        public static Type GetTypeExt(this SerializedProperty property)
        {
            var parentType = property.serializedObject.targetObject.GetType();
            var fi = parentType.GetField(property.propertyPath);
            return fi.ReflectedType;
        }

        // -- IsArray sometimes lies -------------------------------------
        public static bool IsReallyArray(this SerializedProperty property)
        {
            return property.isArray && property.propertyType != SerializedPropertyType.String;
        }

        public static Type GetTypeReflection(this SerializedProperty prop)
        {
            var parentType = prop.serializedObject.targetObject.GetType();
            var fields = parentType.GetFields();
            Type result = null;
            for (var i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType.IsGenericType)
                {
                    var genericArgs = fields[i].FieldType.GetGenericArguments();
                    for (var j = 0; j < genericArgs.Length; j++)
                    {
                        var genericArg = genericArgs[j];
                        if (prop.IsType(genericArgs[j])) return genericArgs[j];
                    }
                }
                else if (prop.IsType(fields[i].FieldType)) return fields[i].FieldType;
            }

            return result;
        }

        private static bool IsType(this SerializedProperty prop, Type type)
        {
            return prop.type == type.ToString();
        }

        public static object GetValueByType(this SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: return property.intValue;
                case SerializedPropertyType.Boolean: return property.boolValue;
                case SerializedPropertyType.Float: return property.floatValue;
                case SerializedPropertyType.String: return property.stringValue;
                case SerializedPropertyType.Color: return property.colorValue;
                case SerializedPropertyType.ObjectReference: return property.objectReferenceValue;
                case SerializedPropertyType.LayerMask: return (LayerMask) property.intValue;
                case SerializedPropertyType.Enum: return property.enumValueIndex;
                case SerializedPropertyType.Vector2: return property.vector2Value;
                case SerializedPropertyType.Vector3: return property.vector3Value;
                case SerializedPropertyType.Vector4: return property.vector4Value;
                case SerializedPropertyType.Quaternion: return property.quaternionValue;
                case SerializedPropertyType.Rect: return property.rectValue;
                case SerializedPropertyType.ArraySize: return property.intValue;
                case SerializedPropertyType.Character: return (char) property.intValue;
                case SerializedPropertyType.AnimationCurve: return property.animationCurveValue;
                case SerializedPropertyType.Bounds: return property.boundsValue;
                case SerializedPropertyType.Generic:
                    property.serializedObject.ApplyModifiedProperties();
                    return property.serializedObject.targetObject.GetValueFromMemberAtPath(property.GetAdjustedPath());
            }

            return null;
        }

        public static object GetSerializedType(this SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: return SerializedPropertyType.Integer.GetTypeName();
                case SerializedPropertyType.Boolean: return SerializedPropertyType.Boolean;
                case SerializedPropertyType.Float: return SerializedPropertyType.Float;
                case SerializedPropertyType.String: return SerializedPropertyType.String;
                case SerializedPropertyType.Color: return SerializedPropertyType.Color;
                case SerializedPropertyType.ObjectReference: return SerializedPropertyType.ObjectReference.GetTypeName();
                case SerializedPropertyType.LayerMask: return SerializedPropertyType.LayerMask;
                case SerializedPropertyType.Enum: return SerializedPropertyType.Enum;
                case SerializedPropertyType.Vector2: return SerializedPropertyType.Vector2;
                case SerializedPropertyType.Vector3: return SerializedPropertyType.Vector3;
                case SerializedPropertyType.Vector4: return SerializedPropertyType.Vector4;
                case SerializedPropertyType.Quaternion: return SerializedPropertyType.Quaternion;
                case SerializedPropertyType.Rect: return SerializedPropertyType.Rect;
                case SerializedPropertyType.ArraySize: return SerializedPropertyType.ArraySize;
                case SerializedPropertyType.Character: return SerializedPropertyType.Character;
                case SerializedPropertyType.AnimationCurve: return SerializedPropertyType.AnimationCurve;
                case SerializedPropertyType.Bounds: return SerializedPropertyType.Bounds;
                case SerializedPropertyType.Generic:
                    property.serializedObject.ApplyModifiedProperties();
                    return property.serializedObject.targetObject.GetValueFromMemberAtPath(property.GetAdjustedPath());
            }

            return null;
        }

        public static bool IsNumerical(this SerializedProperty property)
        {
            var propertyType = property.propertyType;
            switch (propertyType)
            {
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.FixedBufferSize: return true;
                default: return false;
            }
        }

        public static bool IsObject(this SerializedProperty property)
        {
            var propertyType = property.propertyType;
            switch (propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.ManagedReference: return true;
                default: return false;
            }
        }

        public static string GetAdjustedPath(this SerializedProperty property)
        {
            return property.propertyPath.Replace("Array.data", "").Replace("[", "").Replace("]", "");
        }

        public static Type GetPropertyType(this SerializedProperty serializedProperty)
        {
            var slices = serializedProperty.propertyPath.Split('.');
            var type = serializedProperty.serializedObject.targetObject.GetType();
            for (var i = 0; i < slices.Length; i++)
            {
                if (slices[i] == "Array")
                {
                    i++; //skips "data[x]"
                    if (type.IsArray)
                    {
                        type = type.GetElementType(); //gets info on array elements
                    }
                    else
                    {
                        type = type.GetGenericArguments()[0];
                    }
                }
                else
                {
                    type = type.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance).FieldType;
                }
            }

            return type;
        }

        public static Type GetParentType(this SerializedProperty serializedProperty, int parentDepth = 1)
        {
            var targetObject = serializedProperty.serializedObject.targetObject;
            var targetObjectType = targetObject.GetType();
            if (serializedProperty.depth > 0)
            {
                var path = serializedProperty.propertyPath.Split('.');
                var currentType = targetObjectType;
                var i = 0;
                while (i < path.Length - parentDepth)
                {
                    if (path[i] == "Array")
                    {
                        i++; //skips "data[x]"
                        if (currentType.IsArray)
                        {
                            currentType = currentType.GetElementType(); //gets info on array elements
                        }
                        else
                        {
                            currentType = currentType.GetGenericArguments()[0];
                        }
                    }
                    else
                    {
                        currentType = currentType.GetField(path[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance).FieldType;
                    }

                    i++;
                }

                return currentType;
            }
            else
            {
                return targetObjectType;
            }
        }
    }
}
