
//////////////////////////////////////////////////////////////////////////
//
//   FileName : Common.cs
//     Author : Chiyer
// CreateTime : 2014-03-12
//       Desc :
//
//////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using LitJson;

public static class Common
{
    public static T Default<T>()
    {
        return default(T);
    }

    public static object Default(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }

    public static Type GetType(string typeName)
    {
        if (string.IsNullOrEmpty(typeName))
        {
            return null;
        }
        return Type.GetType(typeName);
    }

    public static bool IsType(Type to, Type from)
    {
        return from == to || from.IsSubclassOf(to);
    }

    public static object CreateInstance(Type type, params object[] arguments)
    {
        return Activator.CreateInstance(type, arguments);
    }

    public static object CreateInstance(string typeName, params object[] arguments)
    {
        return Assembly.GetExecutingAssembly().CreateInstance(typeName, false, BindingFlags.Default, null, arguments, null, null);
    }

    public static T[] Pack<T>(params T[] values)
    {
        return values;
    }
}