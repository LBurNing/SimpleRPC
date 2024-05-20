using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

interface IObject
{
    void Reset();
    void Dispose();
}

class ObjectFactory<T> where T : IObject, new()
{
    private int _capacity = 0;

    private List<T> _objs = new List<T>();

    public ObjectFactory(int capacity = 256)
    {
        _capacity = capacity;
    }

    public T Get()
    {
        T obj;
        lock (_objs)
        {
            if (_objs.Count > 0)
            {

                int lastIndex = _objs.Count - 1;
                obj = _objs[lastIndex];
                _objs.RemoveAt(lastIndex);
            }
            else
            {
                obj = new T();
            }
        }
        return obj;
    }

    public void Put(T obj)
    {
        lock (_objs)
        {
            obj.Reset();
            if (_objs.Count >= _capacity)
            {
                obj.Dispose();
                return;
            }
            _objs.Add(obj);
        }
    }

    public void Clear()
    {
        lock (_objs)
        {
            for (int i = 0; i < _objs.Count; i++)
            {
                _objs[i].Dispose();
            }
            _objs.Clear();
        }
    }

}