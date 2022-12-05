
using System;
using System.Collections;
using System.Collections.Generic;

public class Table 
{
	public Type type;
	public string md5;
    private readonly Dictionary<object, object> _sections;

    public object this[object id]
    {
        get => GetSection(id);
        set => SetSection(id, value);
    }

    public int count => _sections.Count;

    public Table()
    {
        _sections = new Dictionary<object, object>();
    }
   
    public object GetSection(object id)
    {
        object section = null;
        if (id == null) 
        {
            UnityEngine.Debug.LogError("GetSection Id Obj = null");
        }
        else
        {
            _sections.TryGetValue(id, out section);
        }
        return section;
    }

    /// <returns>
    /// 若有当前id,则返回当前id,不然则返回小于当前id中的最大id
    /// </returns>
    public object GetSectionEqualOrLess(object id)
    {
        _sections.TryGetValue(id, out var section);
        if (section != null)
        {
            return section;
        }

        var maxKey = 0;
        var maxId = Convert.ToInt32(id);
        
        //查找最近的那一个
        foreach(var key in _sections.Keys)
        {
            var tempKey = Convert.ToInt32(key);
            if (tempKey > maxKey && tempKey < maxId)
            {
                maxKey = tempKey;
            }
        }
        
        _sections.TryGetValue(maxKey, out section);
        return section;
    }

    public void SetSection(object id, object _section)
    {
        if (id != null)
        {
            _sections[id] = _section;
        }
    }

    public IEnumerator<object> GetEnumerator()
    {
        return _sections.Values.GetEnumerator();
    }
}

public class TableT<T> where T : class
{
    private Table table;
    
    public TableT(Table _table)
    {
        table = _table;
    }

    public T this[object id]
    {
        get => table?[id] as T;
        set
        {
            if (table != null)
            {
                table[id] = value;
            }
        }
    }

    public int Count => table.count;

    public T GetSection(object id)
    {
        return table?.GetSection(id) as T;
    }
    
    /// <returns>
    /// 若有当前id,则返回当前id,不然则返回小于当前id中的最大id
    /// </returns>
    public T GetSectionEqualOrLess(object id)
    {
        return table?.GetSectionEqualOrLess(id) as T;
    }

    public void SetSection(object id, T _section)
    {
        table?.SetSection(id, _section);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator<T>(table.GetEnumerator());
    }

    [Serializable]
    public struct Enumerator<Q> : IEnumerator<Q> where Q : class
    {
        private IEnumerator<object> enumerator;

        public Enumerator(IEnumerator<object> _enumerator)
        {
            enumerator = _enumerator;
        }

        public Q Current => enumerator.Current as Q;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return enumerator.MoveNext();
        }

        public void Reset()
        {
            enumerator.Reset();
        }
    }
}

public abstract class BaseDeploy
{
    
}