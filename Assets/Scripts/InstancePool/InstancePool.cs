using UnityEngine;
using System.Collections.Generic;
using System;

namespace PotatoGameDev.Pool
{
    public class InstancePool<T> where T : PooledInstance
    {
        private readonly T prefab;
        private readonly Queue<T> pool = new();
        private readonly HashSet<T> active = new();
        private readonly Transform parent;

        public InstancePool(T prefab, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        public T Prefab { get => prefab; }

        public void Preheat(int prewarm)
        {
            for (int i = 0; i < prewarm; i++)
            {
                pool.Enqueue(Create());
            }
        }

        public void ReleaseAll()
        {
            // Copy first: Release() mutates the active set while we iterate.
            var snapshot = new List<T>(active);
            foreach (var inst in snapshot)
            {
                inst.Release();
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
            active.Add(obj);

            obj.transform.SetParent(parent);
            // TODO I think init should go here... Not called in the user code.
            // EDIT Actually not, init sometimes starts some stuff like coroutines for autodestruct...
            // In this circumstances init shoult not go here
            obj.gameObject.SetActive(true);
            obj.enabled = true;
            return obj;
        }

        public void Release(T inst)
        {
            // Guard against double-release corrupting the queue with duplicates.
            if (!active.Remove(inst)) return;

            inst.gameObject.SetActive(false);
            inst.enabled = false;
            inst.transform.SetParent(parent);
            pool.Enqueue(inst);
        }
    }
}
