// ----------------------------------------------------------------------------
// -- Project : https://github.com/instance-id/AboveAverageInspector         --
// -- instance.id 2020 | http://github.com/instance-id | http://instance.id  --
// ----------------------------------------------------------------------------

using System;
using System.Reflection;
using UnityEngine;
using System.ComponentModel;

namespace instance.id.AAI.Extensions
{
	// -- https://github.com/Magicolo/PseudoFramework  --
	// --------------------------------------------------
	public static class TypeExtensions
	{
		public static bool HasConstructor(this Type type)
		{
			return type.GetConstructors().Length > 0;
		}

		public static bool HasEmptyConstructor(this Type type)
		{
			return type.GetConstructor(Type.EmptyTypes) != null;
		}

		public static bool HasDefaultConstructor(this Type type)
		{
			return type.IsValueType || type.HasEmptyConstructor();
		}

		public static bool HasInterface(this Type type, Type interfaceType)
		{
			return interfaceType.IsAssignableFrom(type) || Array.Exists(type.GetInterfaces(), t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType);
		}

		public static bool Is<T>(this Type type)
		{
			return type.Is(typeof(T));
		}

		public static bool Is(this Type type, Type otherType, params Type[] genericArguments)
		{
			if (genericArguments.Length > 0 && otherType.IsGenericType)
				return type.Is(otherType.MakeGenericType(genericArguments));
			else
				return type.Is(otherType);
		}

		public static bool Is(this Type type, Type otherType)
		{
			return otherType.IsAssignableFrom(type);
		}

		public static bool IsNumerical(this Type type)
		{
			return
				type == typeof(sbyte) ||
				type == typeof(byte) ||
				type == typeof(short) ||
				type == typeof(ushort) ||
				type == typeof(int) ||
				type == typeof(uint) ||
				type == typeof(long) ||
				type == typeof(ulong) ||
				type == typeof(float) ||
				type == typeof(double) ||
				type == typeof(decimal);
		}

		public static bool IsVector(this Type type)
		{
			return
				type == typeof(Vector2) ||
				type == typeof(Vector3) ||
				type == typeof(Vector4);
		}

		public static bool IsConcrete(this Type type)
		{
			return
				!type.IsAbstract &&
				!type.IsInterface &&
				!type.IsGenericTypeDefinition;
		}

		public static bool IsGeneric(this Type type)
		{
			return
				type.IsGenericType ||
				type.IsGenericTypeDefinition ||
				type.IsGenericParameter;
		}

		public static bool IsMetadata(this Type type)
		{
			return
				type == typeof(Type) ||
				type == typeof(Assembly) ||
				type == typeof(AppDomain) ||
				type == typeof(ParameterInfo) ||
				type.Is<MemberInfo>();
		}

		public static string GetName(this Type type)
		{
			return type.Name.Split('.').Last().GetRange('`');
		}
	}
}
