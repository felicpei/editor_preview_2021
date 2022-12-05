using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public static class Formatter
{
    private static class Token
    {
        internal const char ListStart = '[';
        internal const char ListEnd = ']';
        internal const char StringScope = '"';
        internal const char StringEscape = '\\';
        internal const char DictionaryStart = '{';
        internal const char DictionaryEnd = '}';
        internal const char FragmentSeparator = ',';
        internal const char DictionarySeparator = '=';
    }

    public static T ToObject<T>(string formatStr, out bool success)
    {
        return (T)ToObject(typeof(T), formatStr, out success);
    }

    private static object ToObject(Type type, string formatStr, out bool success)
    {
        var fmtObject = Format(formatStr);
        success = true;
        return fmtObject != null ? ToObject(type, fmtObject, out success) : null;
    }

    private static object ToObject(Type type, object fmtObject, out bool success)
    {
        object obj = null;
        success = true;
        
        if (type.IsArray)
        {
            if (fmtObject is List<object> list)
            {
                var count = list.Count;
                var arr = Activator.CreateInstance(type, count) as Array;
                var elementType = type.GetElementType();
                for (var i = 0; i < count; ++i)
                {
                    if (arr != null)
                    {
                        arr.SetValue(ToObject(elementType, list[i], out success), i);
                    }
                }
                obj = arr;
            }
        }
        else if (typeof(IList).IsAssignableFrom(type))
        {
            if (fmtObject is List<object> list)
            {
                var count = list.Count;
                var arr = Activator.CreateInstance(type) as IList;
                var elementType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);
                for (var i = 0; i < count; ++i)
                {
                    if (arr != null)
                    {
                        arr.Add(ToObject(elementType, list[i], out success));
                    }
                }

                obj = arr;
            }
        }
        else if (typeof(IDictionary).IsAssignableFrom(type))
        {
            if (fmtObject is Dictionary<string, object> dict)
            {
                var arr = Activator.CreateInstance(type) as IDictionary;
                if (arr != null && dict.Count > 0)
                {
                    var keyType = typeof(object);
                    var valueType = typeof(object);

                    if (type.IsGenericType)
                    {
                        var kvType = type.GetGenericArguments();
                        if (kvType.Length >= 2)
                        {
                            keyType = kvType[0];
                            valueType = kvType[1];
                        }
                    }

                    using var e = dict.GetEnumerator();
                    while (e.MoveNext())
                    {
                        var key = ConvertType(e.Current.Key, keyType, out success);
                        if (key != null)
                        {
                            arr.Add(key, ToObject(valueType, e.Current.Value, out success));
                        }
                    }
                }

                obj = arr;
            }
        }
        else if (type == typeof(string))
        {
            obj = fmtObject as string;
        }
        else if (type.IsEnum)
        {
            var str = fmtObject as string;
            if (!string.IsNullOrEmpty(str))
            {
                obj = ConvertType(fmtObject, type, out success);
            }
        }
        else if (type.IsClass || (type.IsValueType && !type.IsPrimitive))
        {
            if (fmtObject is Dictionary<string, object> dict)
            {
                obj = Activator.CreateInstance(type);
                if (obj != null && dict.Count > 0)
                {
                    AssignObject(obj, dict, null, out success);
                }
            }
        }
        else
        {
            obj = ConvertType(fmtObject, type, out success);
        }

        return obj;
    }

    public static void AssignObject(object _object, string formatStr, Type type, out bool success)
    {
        success = true;
        if (Format(formatStr) is Dictionary<string, object> dict && dict.Count > 0)
        {
            AssignObject(_object, dict, type, out success);
        }
    }

    private static object ConvertType(object objValue, Type type, out bool success)
    {
        object objRet = null;
        try
        {
            success = true;
            objRet = type.IsEnum ? Enum.Parse(type, objValue.ToString(), true) : Convert.ChangeType(objValue, type, NumberFormatInfo.InvariantInfo);
        }
        catch
        {
            success = false;
            //Debug.LogError(string.Format("objValue:{0} type:{1} enum:{2} <=> {3} {4}", objValue.ToString(), type, type.IsEnum, exp.Message, exp.StackTrace));
        }

        return objRet;
    }

    public static bool AssignObject(object _object, FieldInfo field, string fmtstr, Type type = null)
    {
        type ??= field.FieldType;
        object value = null;
        var success = true;

        if (type.IsPrimitive)
        {
            value = ConvertType(fmtstr, type, out success);
        }
        else if (type == typeof(string))
        {
            value = fmtstr;
        }
        else if (type.IsEnum)
        {
            if (!string.IsNullOrEmpty(fmtstr))
            {
                value = ConvertType(fmtstr, type, out success);
            }
        }
        else
        {
            value = ToObject(type, fmtstr, out success);
        }

        if (value != null)
        {
            field.SetValue(_object, value);
        }

        return success;
    }

    private static void AssignObject(object _object, Dictionary<string, object> dict, Type type, out bool success)
    {
        success = true;

        if (type == null)
        {
            type = _object.GetType();
        }
        
        var isObject = type.IsClass || (type.IsValueType && !type.IsPrimitive);
        if (!isObject ||
            !type.IsInstanceOfType(_object) ||
            type == typeof(string) ||
            typeof(IList).IsAssignableFrom(type) ||
            typeof(IDictionary).IsAssignableFrom(type))
            throw new Exception("type error");

        var fieldList = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var length = fieldList.Length;
        for (var i = 0; i < length; ++i)
        {
            dict.TryGetValue(fieldList[i].Name, out var value);
            if (value != null)
            {
                fieldList[i].SetValue(_object, ToObject(fieldList[i].FieldType, value, out success));
            }
        }
    }

    private static object Format(string formatStr)
    {
        return Format(formatStr, 0, out _);
    }

    private static object Format(string formatStr, int begin, out int end)
    {
        var length = formatStr.Length;
        while (begin < length && char.IsWhiteSpace(formatStr, begin))
        {
            ++begin;
        }
        
        end = begin;
        
        if (begin >= length)
        {
            return null;
        }
        
        object obj;

        switch (formatStr[begin])
        {
            case Token.DictionaryStart:
            {
                var dict = new Dictionary<string, object>();
                if (++end < length)
                {
                    while (true)
                    {
                        begin = end;
                        if ((end = formatStr.IndexOf(Token.DictionarySeparator, end)) == -1)
                        {
                            end = length;
                        }
                        else
                        {
                            dict[formatStr.Substring(begin, end - begin).Trim()] = Format(formatStr, ++end, out end);
                        }

                        while (end < length && char.IsWhiteSpace(formatStr, end))
                        {
                            ++end;
                        }

                        if (end >= length || formatStr[end] == Token.DictionaryEnd)
                        {
                            break;
                        }
                    }
                }

                if (end < length && formatStr[end] == Token.DictionaryEnd)
                {
                    ++end;
                }
                obj = dict;
                
                break;
            }
           
            case Token.ListStart:
            {
                var list = new List<object>();
                if (++end < length)
                {
                    while (true)
                    {
                        if (formatStr[end] == Token.ListEnd && end == length - 1)
                        {
                            break;
                        }
                        
                        list.Add(Format(formatStr, end, out end));
                        while (end < length && char.IsWhiteSpace(formatStr, end))
                        {
                            ++end;
                        }

                        if (end >= length || formatStr[end] == Token.ListEnd)
                        {
                            break;
                        }
                    }
                }

                if (end < length && formatStr[end] == Token.ListEnd)
                {
                    ++end;
                }
                obj = list;
                
                break;
            }
               
            case Token.StringScope:
            {
                var buildStr = new StringBuilder();
                while (++end < length)
                {
                    var code = formatStr[end];
                    if (code == Token.StringEscape)
                    {
                        if (++end < length)
                        {
                            if (formatStr[end] == Token.StringScope)
                                buildStr.Append(Token.StringScope);
                            else
                            {
                                buildStr.Append(code);
                                buildStr.Append(formatStr[end]);
                            }
                        }
                        else
                        {
                            buildStr.Append(code);
                            break;
                        }
                    }
                    else if (code == Token.StringScope)
                    {
                        break;
                    }
                    else
                    {
                        buildStr.Append(code);
                    }
                }

                if (end < length && formatStr[end] == Token.StringScope)
                {
                    ++end;
                }
                obj = buildStr.ToString();
                
                break;
            }
               
            default:
            {
                while (++end < length)
                {
                    var c = formatStr[end];
                    if (c == Token.ListEnd || c == Token.DictionaryEnd || c == Token.FragmentSeparator)
                    {
                        break;
                    }
                }

                obj = formatStr.Substring(begin, end - begin);
                break;
            }
        }

        while (end < length && char.IsWhiteSpace(formatStr, end))
        {
            ++end;
        }

        if (end < length && formatStr[end] == Token.FragmentSeparator)
        {
            ++end;
        }
        
        return obj;
    }
}