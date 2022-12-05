using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class JsonObjectAttribute : Attribute
{
    public static implicit operator bool(JsonObjectAttribute _attribute)
    {
        return _attribute != null;
    }
}

public static class TableDatabase
{
    private static readonly char FieldDelimiter;
    private static readonly BindingFlags FieldFlags;
    private static readonly int MainKey;
    
    private static readonly Hashtable HashTables;

    private class FieldInfoEx
    {
        public FieldInfo FieldInfo;
        public int TypeAttrCount;
        public int FieldAttrCount;
    }

    static TableDatabase()
    {
        MainKey = 0;
        FieldDelimiter = '\t';
        FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      
        HashTables = Hashtable.Synchronized(new Hashtable());
    }


    private static Table LoadByString(Type type, string reader, string tableName, string md5 = "")
    {
        var lineIndex = 0;
        var sep = "\r\n";
        var text = string.Empty;
        
        while (true)
        {
            var n = reader.IndexOf(sep, lineIndex, StringComparison.Ordinal);
            if (n == -1)
            {
                break;
            }
            var len = n - lineIndex;
            text = reader.Substring(lineIndex, len);
            lineIndex = n + 2;
            
            if (string.IsNullOrEmpty(text) || text[0] != '#')
            {
                break;
            }
        }

        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var fieldNames = text.Split(FieldDelimiter);
        var count = fieldNames.Length;
        if (count <= MainKey)
        {
            return null;
        }

        for (var i = 0; i < fieldNames.Length; ++i)
        {
            if (fieldNames[i].StartsWith("sz_") || fieldNames[i].StartsWith("js_"))
            {
                fieldNames[i] = fieldNames[i].Remove(0, 3);
            }
            else if (fieldNames[i].StartsWith("b_"))
            {
                fieldNames[i] = fieldNames[i].Remove(0, 2);
            }

            if (fieldNames[i].EndsWith("__s"))
            {
                fieldNames[i] = fieldNames[i].Remove(fieldNames[i].Length - 3, 3);
            }
        }

        var fieldInfos = new FieldInfoEx[count];
        for (var i = 0; i < count; ++i)
        {
            var fieldInfoEx = new FieldInfoEx { FieldInfo = type.GetField(fieldNames[i], FieldFlags) };
            if (fieldInfoEx.FieldInfo != null)
            {
                fieldInfoEx.TypeAttrCount = fieldInfoEx.FieldInfo.FieldType.GetCustomAttributes(typeof(JsonObjectAttribute), false).Length;
                fieldInfoEx.FieldAttrCount = fieldInfoEx.FieldInfo.GetCustomAttributes(typeof(JsonObjectAttribute), false).Length;
            }

            fieldInfos[i] = fieldInfoEx;
        }

        var keyField = fieldInfos[MainKey];
        if (keyField.FieldInfo == null)
        {
            Debug.LogError(string.Format("{0} mainkey 不存在!!!", tableName));
            return null;
        }

        var table = new Table();
        while (true)
        {
            var n = reader.IndexOf(sep, lineIndex, StringComparison.Ordinal);
            if (n == -1)
            {
                break;
            }
            
            var len = n - lineIndex;
            text = reader.Substring(lineIndex, len);
            lineIndex = n + 2;
            if (text.Length <= 0 || text[0] == '#')
            {
                continue;
            }
            
            var valueStrs = text.Split(FieldDelimiter);
            if (valueStrs.Length <= 0 || string.IsNullOrEmpty(valueStrs[MainKey]))
            {
                continue;
            }

            var section = Activator.CreateInstance(type);

            for (var i = 0; i < valueStrs.Length && i < fieldInfos.Length; ++i)
            {
                var str = valueStrs[i];
                var info = fieldInfos[i];
                if (info.FieldInfo != null && !string.IsNullOrEmpty(str))
                {
                    str = str.Replace("\\n", "\n").Replace("\\t", "\t");
                    /*
                    if (tableName != "ui/language/Language" && (info.FieldInfo.FieldType.IsArray || info.FieldInfo.FieldType == typeof(string)))
                    {
                        var newstr = G.R(str);
                        if (newstr != "")
                        {
                            str = newstr;
                        }
                    }
                    */
                    SetField(section, info, str);
                }
            }

            var key = keyField.FieldInfo.GetValue(section);
            if (key != null)
            {
                //若已有key存在，则报错
                if (XPlatform.InEditor)
                {
                    if (table.GetSection(key) != null)
                    {
                        Debug.LogError("Table Key is not the only! {\"tableName\": " + tableName + ",  \"key\": " + key);
                    }
                }
                table.SetSection(key, section);
            }
        }

        table.type = type;
        table.md5 = md5 != "" ? md5 : CalcMD5CRC(reader);
        return table;
    }

    private static Table Load(Type type, string tableName)
    {
        var cacheObj = HashTables[tableName];
        Table table = null;

        if (cacheObj != null)
        {
            table = (Table)cacheObj;
            return table;
        }

        //   Dbg.Log("Load Table : " + tableName);

        try
        {
            var tabPath = XPath.TablesPath + tableName + XPath.TableExtension;
            var code = XResource.LoadTableText(tabPath);
            if (code != null)
            {
                if (XPlatform.InEditor)
                {
                    code = RemoveComment(code);    
                }
                table = LoadByString(type, code, tableName);
                HashTables[tableName] = table;
            }
            else
            {
                Debug.LogError("GetTextResource failed:" + tabPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("GetTextResource Exception failed: " + tableName + " error: " + e);
        }

        return table;
    }

    public static TableT<T> Load<T>(string tableName) where T : class
    {
        var table = Load(typeof(T), tableName);
        var tab = table != null ? new TableT<T>(table) : null;
        if (tab == null)
        {
            Debug.LogError("配置表不存在:" + tableName);
        }

        return tab;
    }

    public static void UnLoad(string tableName)
    {
        HashTables.Remove(tableName);
    }

    private static void SetField(object section, FieldInfoEx fieldInfoEx, string valueStr)
    {
        var fieldInfo = fieldInfoEx.FieldInfo;
        var type = fieldInfo.FieldType;

        if (type == typeof(LitJson.JsonData))
        {
            try
            {
                var value = LitJson.JsonMapper.ToObject(valueStr);
                fieldInfo.SetValue(section, value);
            }
            catch (Exception)
            {
                Debug.Log("error json format: " + section + " " + fieldInfo + " " + valueStr);
            }

            return;
        }

        //GetCustomAttributes函數太耗，先在循環外緩存.lq
        //if (type.GetCustomAttributes(typeof(JsonObjectAttribute), false).Length > 0 || fieldInfo.GetCustomAttributes(typeof(JsonObjectAttribute), false).Length > 0)
        if (fieldInfoEx.TypeAttrCount > 0 || fieldInfoEx.FieldAttrCount > 0)
        {
            try
            {
                var value = LitJson.JsonMapper.ToObject(type, valueStr);
                fieldInfo.SetValue(section, value);
            }
            catch (Exception)
            {
                Debug.Log("error json format: " + section + " " + fieldInfo + " " + valueStr);
            }

            return;
        }

        if (type.IsArray || typeof(IList).IsAssignableFrom(type))
        {
            valueStr = trimSpace(valueStr, "[", "]");
        }
        else if ((type.IsClass && type != typeof(string)) ||  (type.IsValueType && !type.IsPrimitive && !type.IsEnum))
        {
            valueStr = trimSpace(valueStr, "{", "}");
        }

        if (!Formatter.AssignObject(section, fieldInfo, valueStr))
        {
            Debug.LogError("配置表配置错误  section:" + section + " fieldInfo:" + fieldInfo + " valueStr:" + valueStr);
        }
    }

    private static string trimSpace(string valueStr, string splitCh1, string splitCh2)
    {
        var i = 0;
        var length = valueStr.Length;
        while (i < length)
        {
            if (!char.IsWhiteSpace(valueStr, i))
            {
                break;
            }
            ++i;
        }

        if (i >= length || valueStr[i] != splitCh1[0])
        {
            valueStr = splitCh1 + valueStr + splitCh2;
        }
        return valueStr;
    }


    //去tab表注释
    public static string RemoveComment(string txtTab, bool remove__s = false)
    {
        var buffer = Encoding.UTF8.GetBytes(txtTab);
        var text = txtTab;
        if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
        {
            text = Encoding.UTF8.GetString(buffer, 3, buffer.Length - 3);
        }

        var sb = new StringBuilder();
        var cols = new List<string>();
        foreach (var line in text.Split(new[] { "\r\n" }, StringSplitOptions.None))
        {
            if (line.StartsWith("#"))
            {
                continue;
            }

            if (line == "")
            {
                continue;
            }

            var list = line.Split(new[] { "\t" }, StringSplitOptions.None);
            if (cols.Count == 0)
            {
                cols.AddRange(list);
            }

            if (list.Length != cols.Count)
            {
                Debug.LogError("wrong line:" + line + " " + list.Length + "!=" + cols.Count);
            }

            for (var i = 0; i < cols.Count; i++)
            {
                var colName = cols[i];
                if (colName != "" && !(remove__s && colName.EndsWith("__s")))
                {
                    sb.Append(list[i]);
                    sb.Append("\t");
                }
            }

            sb.Append("\r\n");
        }

        return sb.ToString();
    }

    private static string CalcMD5CRC(string input)
    {
        // step 1, calculate MD5 hash from input  
        var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hash = md5.ComputeHash(inputBytes);

        // step 2, convert byte array to hex string  
        var sb = new StringBuilder();
        for (var i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("X2"));
        }

        return sb.ToString();
    }
}
