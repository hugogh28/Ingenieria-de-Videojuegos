using System;
using System.Collections.Generic;
using System.Linq;
public class ObjectPool : IObjectPool
{
    private IPoolableObject poolableObject;
    private readonly bool addNew;
    private List<IPoolableObject> pool;

    //private int activeObjects;

    public ObjectPool(IPoolableObject o, int n, bool add)
    {
        poolableObject = o;
        addNew = add;
        pool = new List<IPoolableObject>(n);

        for (int i = 0; i < n; i++)
        {
            pool.Add(Create());
        }
    }

    public IPoolableObject Get()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].Active)
            {
                pool[i].Active = true;
                //activeObjects++;
                return pool[i];
            }
        }

        if (addNew)
        {
            IPoolableObject newObject = Create();
            newObject.Active = true;
            pool.Add(newObject);

            //activeObjects++;
            return newObject;
        }

        return null;
    }

    public void Release(IPoolableObject o)
    {
        o.Active = false;
        //activeObjects--;
        o.ResetObject();
    }

    private IPoolableObject Create()
    {
        IPoolableObject newObject = poolableObject.Clone();
        return newObject;
    }

    public int GetCount()
    {
        return pool.Count;
    }

    /*public int GetActive()
    {
        return activeObjects;
    }*/
}
