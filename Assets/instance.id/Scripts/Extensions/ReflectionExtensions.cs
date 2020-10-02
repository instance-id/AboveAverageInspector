using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace instance.id.AAI.Extensions
{
    public static class ReflectiveEnumerator // @formatter:off
    {
        static ReflectiveEnumerator() { } // @formatter:on

        // -------------------------------------------------------- GetEnumerableOfType()
        // -- GetEnumerableOfType() -----------------------------------------------------
        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
        {
            var objects = new List<T>();
            foreach (var type in Assembly.GetAssembly(typeof(T)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add((T) Activator.CreateInstance(type, constructorArgs));
            }

            objects.Sort();
            return objects;
        }
    }

    public static class ReflectionExtensions
    {
        // ----------------------------------------------------------------- BindingFlags
        // -- BindingFlags --------------------------------------------------------------
        public const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        public const BindingFlags PublicFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        public const BindingFlags NonPublicFlags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        public const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        public const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        // -------------------------------------------------------------- GetFieldViaPath
        // -- GetFieldViaPath -----------------------------------------------------------
        public static FieldInfo GetFieldViaPath(this Type type, string path)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var parent = type;
            var fieldInfo = parent.GetField(path, flags);
            var paths = path.Split('.');
            for (var i = 0; i < paths.Length; i++)
            {
                if (parent is null) continue;
                fieldInfo = parent.GetField(paths[i], flags);
                switch (fieldInfo)
                {
                    // there are only two container field type that can be serialized:
                    // Array and List<T>
                    case { } when fieldInfo.FieldType.IsArray:
                        parent = fieldInfo.FieldType.GetElementType();
                        i += 2;
                        continue;
                    case { } when fieldInfo.FieldType.IsGenericType:
                        parent = fieldInfo.FieldType.GetGenericArguments()[0];
                        i += 2;
                        continue;
                }

                if (fieldInfo != null)
                {
                    parent = fieldInfo.FieldType;
                }
                else
                {
                    return null;
                }
            }

            return fieldInfo;
        }

        // ------------------------------------------------------------- CreateInstance()
        // -- CreateInstance() ----------------------------------------------------------
        public static object CreateInstance(string typeFullName)
        {
            var type = FindType(typeFullName);
            return type != null ? Activator.CreateInstance(type) : null;
        }

        // ------------------------------------------------------------------- FindType()
        // -- FindType() ----------------------------------------------------------------
        public static Type FindType(this string typeFullName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.SelectMany(assembly => assembly.GetTypes()).FirstOrDefault(type => type.FullName == null || type.FullName.Equals(typeFullName));
        }

        // ------------------------------------------------------------------- FindType()
        // -- FindType() ----------------------------------------------------------------
        public static Type GetTypeOut(this object obj, out Type typeOut)
        {
            return typeOut = obj.GetType();
        }

        // -------------------------------------------------------- GetTypesInNamespace()
        // -- GetTypesInNamespace() -----------------------------------------------------
        public static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        // ----------------------------------------------------------- GetPropertyValue()
        // -- GetPropertyValue() --------------------------------------------------------
        public static object GetPropertyValue(object src, string propName, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public)
        {
            return src.GetType().GetProperty(propName, bindingAttr)?.GetValue(src, null);
        }

        // ----------------------------------------------------------- SetPropertyValue()
        // -- SetPropertyValue() --------------------------------------------------------
        public static void SetPropertyValue<T>(object src, string propName, T propValue, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public)
        {
            src.GetType().GetProperty(propName, bindingAttr)?.SetValue(src, propValue.ToString());
        }

        public static T GetValueFromMemberAtPath<T>(this object obj, string memberPath)
        {
            return (T) obj.GetValueFromMemberAtPath(memberPath);
        }

        public static string GetTypeName(this object obj)
        {
            return obj.GetType().GetName();
        }

        public static MemberInfo GetMemberInfoAtPath(this object obj, string memberPath)
        {
            int offset = 0;
            int separatorIndex = memberPath.IndexOf('.');
            if (separatorIndex == -1) return obj.GetFieldOrPropertyInfo(memberPath);
            var value = obj.GetValueFromMember(memberPath.Substring(offset, separatorIndex - offset));
            offset = separatorIndex + 1;
            separatorIndex = memberPath.IndexOf('.', offset);
            while (separatorIndex != -1)
            {
                value = value.GetValueFromMember(memberPath.Substring(offset, separatorIndex - offset));
                offset = separatorIndex + 1;
                separatorIndex = memberPath.IndexOf('.', offset);
            }

            return value.GetFieldOrPropertyInfo(memberPath.Substring(offset));
        }

        public static object GetValueFromMemberAtPath(this object obj, string memberPath)
        {
            var member = obj.GetMemberInfoAtPath(memberPath);
            var pathSplit = memberPath.Split('.');
            if (pathSplit.Length <= 1)
            {
                return obj.GetValueFromMember(pathSplit.Pop(out pathSplit));
            }

            int index;
            if (int.TryParse(pathSplit.Last(), out index))
            {
                Array.Resize(ref pathSplit, pathSplit.Length - 1);
                return ((IList) obj.GetValueFromMemberAtPath(pathSplit.Concat(".")))[index];
            }

            Array.Resize(ref pathSplit, pathSplit.Length - 1);
            object container = obj.GetValueFromMemberAtPath(pathSplit.Concat("."));
            return member.GetMemberValue(container);
        }

        public static object GetMemberValue(this MemberInfo member, object obj)
        {
            var field = member as FieldInfo;
            if (field != null) return field.GetValue(obj);
            var property = member as PropertyInfo;
            if (property != null) return property.GetValue(obj, null);
            return null;
        }

        // @formatter:off --------------------------------------- GetValueDerivedFields()
        // -- If you want to find derived fields and properties when fetching the value -
        // -- GetValueDerivedFields() ---------------------------------------------------
        public static Tuple<List<FieldInfo>, List<PropertyInfo>> GetValueDerivedFields(
            this object source, bool isType = false) // @formatter:on
        {
            if (source == null) return null;
            var fieldInfoList = new List<FieldInfo>();
            var propertyInfoList = new List<PropertyInfo>();
            var type = isType ? source as Type : source.GetType();
            var allFields = type.GetFields();
            if (allFields.Length == 0)
            {
                Debug.LogWarning($"Type: {source.ToString()} has no results!");
                return null;
            }

            for (var i = 0; i < allFields.Length; i++)
            {
                var fieldInfo = FindFieldInTypeHierarchy(type, allFields[i].Name);
                if (!(fieldInfo is null)) fieldInfoList.Add(fieldInfo);
                else
                {
                    var propertyInfo = FindPropertyInTypeHierarchy(type, allFields[i].Name);
                    if (!(propertyInfo is null)) propertyInfoList.Add(propertyInfo);
                }
            }

            return new Tuple<List<FieldInfo>, List<PropertyInfo>>(fieldInfoList, propertyInfoList);
        }

        // @formatter:off ---------------------------------------- GetValueDerivedField()
        // -- If you want to find derived fields and properties when fetching the value -
        // -- GetValueDerivedField() ----------------------------------------------------
        #region GetValues
        public static object GetValueDerivedField(this object source, string name) // @formatter:on
        {
            if (source == null) return null;
            var type = source.GetType();
            var f = FindFieldInTypeHierarchy(type, name);
            if (f == null)
            {
                var p = FindPropertyInTypeHierarchy(type, name);
                if (p == null) return null;
                return p.GetValue(source, null);
            }

            return f.GetValue(source);
        }

        // ------------------------
        public static FieldInfo FindFieldInTypeHierarchy(Type providedType, string fieldName)
        {
            var field = providedType.GetField(fieldName, (BindingFlags) (-1));
            while (field == null && providedType.BaseType != null)
            {
                providedType = providedType.BaseType;
                field = providedType.GetField(fieldName, (BindingFlags) (-1));
            }

            return field;
        }

        // ------------------------
        public static PropertyInfo FindPropertyInTypeHierarchy(Type providedType, string propertyName)
        {
            var property = providedType.GetProperty(propertyName, (BindingFlags) (-1));
            while (property == null && providedType.BaseType != null)
            {
                providedType = providedType.BaseType;
                property = providedType.GetProperty(propertyName, (BindingFlags) (-1));
            }

            return property;
        }

        #endregion

        // @formatter:off -------------------------------------------- GetInspectedType()
        // -- Get all fields and related data from within a particular class           --
        // -- GetInspectedType() --------------------------------------------------------
        private static Lazy<FieldInfo> m_InspectedType = new Lazy<FieldInfo>(() => typeof(CustomEditor)
                .GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.NonPublic));

        public static Type GetInspectedType(this CustomEditor attribute)
        {
            return (Type) m_InspectedType.Value.GetValue(attribute);
        } // @formatter:on

        // @formatter:off ------------------------------------------------ GetAttribute()
        // -- GetAttribute() ------------------------------------------------------------
        public static T GetAttribute<T>(Type rObjectType)
        {
#if !UNITY_EDITOR && (NETFX_CORE || WINDOWS_UWP || UNITY_WP8 || UNITY_WP_8_1 || UNITY_WSA || UNITY_WSA_8_0 || UNITY_WSA_8_1 || UNITY_WSA_10_0)
            System.Collections.Generic.IEnumerable<System.Attribute> lInitialAttributes = rObjectType.GetTypeInfo().GetCustomAttributes(typeof(T), true);
            object[] lAttributes = lInitialAttributes.ToArray();
#else
            var lAttributes = rObjectType.GetCustomAttributes(typeof(T), true);
#endif
            if (lAttributes == null || lAttributes.Length == 0)
            {
                return default(T);
            }

            return (T) lAttributes[0];
        } // @formatter:on

        // ------------------------------------------------------------ GetRealTypeName()
        // -- GetRealTypeName() ---------------------------------------------------------
        public static string GetRealTypeName(this Type t, bool parameters = false)
        {
            if (!t.IsGenericType) return t.Name;
            var sb = new StringBuilder();
            sb.Append(t.Name.Substring(0, t.Name.IndexOf('`')));
            if (!parameters) return sb.ToString();
            sb.Append('<');
            var appendComma = false;
            foreach (var arg in t.GetGenericArguments())
            {
                if (appendComma) sb.Append(',');
                sb.Append(GetRealTypeName(arg));
                appendComma = true;
            }

            sb.Append('>');
            return sb.ToString();
        }

        public static MemberInfo GetFieldOrPropertyInfo(this object obj, string memberName)
        {
            var field = obj.GetType().GetField(memberName, AllFlags);
            if (field != null) return field;
            var property = obj.GetType().GetProperty(memberName, AllFlags);
            if (property != null) return property;
            return null;
        }

        public static object GetValueFromMember(this object obj, string memberName)
        {
            if (obj is IList) return ((IList) obj)[int.Parse(memberName)];
            var member = obj.GetFieldOrPropertyInfo(memberName);
            return member.GetMemberValue(obj);
        }

        // --------------------------------------------------------- TestGetClassFields()
        // -- TestGetClassFields() ------------------------------------------------------
        public static ClassData TestGetClassFields(this Type type, bool sortOutput = false)
        {
            var thisType = type;
            var fieldInfos = new List<FieldInfo>();
            var classData = new ClassData(thisType);
            try
            {
                var fieldList = thisType.GetFields();
                fieldInfos = sortOutput ? fieldList.OrderBy(x => x.Name).ToList() : fieldList.ToList();
                for (var i = 0; i < fieldInfos.Count; i++)
                {
                    var fieldData = new FieldData(fieldInfos[i]);
                    fieldData.fieldType = fieldInfos[i].FieldType;
                    fieldData.fieldTypeParameters = fieldInfos[i].FieldType.GetGenericArguments().Select(x => x).ToList();
                    fieldData.fieldTypeParametersString = fieldInfos[i].FieldType.GetGenericArguments().Select(x => x.Name.ToString()).ToList();
                    classData.fieldDatas.Add(fieldInfos[i].Name, fieldData);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return classData;
        }

        // ---------------------------------------------------------- GetAssemblyByName()
        // -- GetAssemblyByName() -------------------------------------------------------
        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Single(assembly => assembly.GetName().Name == name);
        }

        static bool IsDynamic(Assembly assembly)
        {
            return assembly is AssemblyBuilder || assembly.GetType().FullName == "System.Reflection.Emit.InternalAssemblyBuilder";
        }

        // ----------------------------------------------------------- GetExportedTypes()
        // -- GetExportedTypes() --------------------------------------------------------
        private static IEnumerable<Type> GetExportedTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types;
            }
            catch (NotSupportedException)
            {
                return new Type[0];
            }
        }

        // ------------------------------------------------------------------- AllTypes()
        // -- AllTypes() ----------------------------------------------------------------
        public static IEnumerable<Type> AllTypes
        {
            get { return AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsDynamic(a)).SelectMany(GetExportedTypes).ToArray(); }
        }

        // ---------------------------------------------------------------- GetAllTypes()
        // -- GetAllTypes() -------------------------------------------------------------
        private static IEnumerable<Type> GetAllTypes(Type inspectedType)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => !IsDynamic(a)).SelectMany(GetExportedTypes).ToArray();
        }

        // ---------------------------------------------------------------- GetSubtypes()
        // -- GetSubtypes() -------------------------------------------------------------
        public static IEnumerable<Type> GetSubtypes(Type inspectedType, IEnumerable<Assembly> assemblies) // @formatter:off
        {
            var assemblyTypes = assemblies
                .SelectMany(asm => asm.GetExportedTypes())
                .Where(t => { try { return t != null && t != inspectedType; }catch{ return false; } });
            
            if (inspectedType.IsInterface) // @formatter:on
            {
                var implementers = assemblyTypes.Where(t => inspectedType.IsAssignableFrom(t));
                foreach (var implementer in implementers)
                {
                    if (implementer.BaseType == null) yield return implementer;
                    else if (!implementer.BaseType.IsAssignableFrom(inspectedType)) yield return implementer;
                }
            }
            else
            {
                var subTypes = assemblyTypes.Where(asmType => asmType.BaseType == inspectedType);
                foreach (var type in subTypes) yield return type;
            }
        }
    }
}
