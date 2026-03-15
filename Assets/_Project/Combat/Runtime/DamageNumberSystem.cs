using System.Collections.Generic;
using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public sealed class DamageNumberSystem : MonoBehaviour
    {
        private static DamageNumberSystem s_instance;

        private readonly Queue<DamageNumberView> _pool = new Queue<DamageNumberView>();
        [SerializeField] private int _prewarmCount = 16;

        public static DamageNumberSystem Instance
        {
            get
            {
                if (s_instance != null)
                {
                    return s_instance;
                }

                GameObject go = new GameObject("DamageNumberSystem");
                s_instance = go.AddComponent<DamageNumberSystem>();
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

        public void Show(DamageResult result)
        {
            DamageNumberView view = GetView();
            view.Show(result, Release);
        }

        private void Prewarm()
        {
            for (int i = 0; i < _prewarmCount; i++)
            {
                Release(CreateView());
            }
        }

        private DamageNumberView GetView()
        {
            if (_pool.Count > 0)
            {
                return _pool.Dequeue();
            }

            return CreateView();
        }

        private DamageNumberView CreateView()
        {
            GameObject go = new GameObject("DamageNumberView");
            go.transform.SetParent(transform, false);
            return go.AddComponent<DamageNumberView>();
        }

        private void Release(DamageNumberView view)
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
