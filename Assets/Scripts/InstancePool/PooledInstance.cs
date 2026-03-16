using UnityEngine;
using System;

namespace PotatoGameDev.Pool
{
    public class PooledInstance : MonoBehaviour
    {
        internal event Action<PooledInstance> OnRelease;

        public void Init()
        {
        }

        public void Release()
        {
            OnRelease?.Invoke(this);
        }
    }
}
