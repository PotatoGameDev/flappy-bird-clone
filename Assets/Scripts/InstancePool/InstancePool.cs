using UnityEngine;
using System.Collections.Generic;
using System;

namespace PotatoGameDev.Pool
{
    public class InstancePool<T> where T : PooledInstance
    {
        private readonly T prefab;
        private readonly Queue<T> pool = new();
        private readonly Transform parent;

        public InstancePool(T prefab, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        public void Preheat(int prewarm)
        {
            for (int i = 0; i < prewarm; i++)
            {
                pool.Enqueue(Create());
            }
        }

        private T Create()
        {
            var obj = GameObject.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);

            obj.TryGetComponent<PooledInstance>(out var pooled);
            if (pooled == null) throw new ArgumentException("Add PooledInstance script to the prefab: " + prefab);

            pooled.OnRelease += (o) => Release((T)o);
            return obj;
        }

        public T Get()
        {
            if (pool.Count == 0)
            {
                pool.Enqueue(Create());
            }

            var obj = pool.Dequeue();

            obj.transform.SetParent(parent);
            // TODO I think init should go here... Not called in the user code.
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Release(T inst)
        {
            inst.gameObject.SetActive(false);
            pool.Enqueue(inst);
        }
    }
}
