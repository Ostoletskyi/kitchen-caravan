using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public sealed class EffectSystem : MonoBehaviour
    {
        private static EffectSystem s_instance;
        private readonly Queue<PooledEffectView> _pool = new Queue<PooledEffectView>();
        [SerializeField] private int _prewarmCount = 12;

        public static EffectSystem Instance
        {
            get
            {
                if (s_instance != null)
                {
                    return s_instance;
                }

                GameObject go = new GameObject("EffectSystem");
                s_instance = go.AddComponent<EffectSystem>();
                return s_instance;
            }
        }

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_instance = this;
            DontDestroyOnLoad(gameObject);
            Prewarm();
        }

        public void Play(DamageFeedbackType effectType, Vector3 position)
        {
            PooledEffectView view = _pool.Count > 0 ? _pool.Dequeue() : CreateView();
            view.Show(effectType, position, Release);
        }

        private void Prewarm()
        {
            for (int i = 0; i < _prewarmCount; i++)
            {
                Release(CreateView());
            }
        }

        private PooledEffectView CreateView()
        {
            GameObject go = new GameObject("PooledEffectView");
            go.transform.SetParent(transform, false);
            return go.AddComponent<PooledEffectView>();
        }

        private void Release(PooledEffectView view)
        {
            if (view == null)
            {
                return;
            }

            view.gameObject.SetActive(false);
            view.transform.SetParent(transform, false);
            _pool.Enqueue(view);
        }
    }
}
