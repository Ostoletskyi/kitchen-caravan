using UnityEngine;

namespace KitchenCaravan.VerticalSlice
{
    public sealed class DroneVisualController : MonoBehaviour
    {
        [SerializeField] private float _rotorSpeed = 1080f;
        [SerializeField] private float _armThrowAngle = 18f;
        [SerializeField] private float _armThrowFrequency = 12f;

        private Transform _body;
        private Transform _leftRotor;
        private Transform _rightRotor;
        private Transform _leftArm;
        private Transform _rightArm;
        private Transform _shadow;

        private void Awake()
        {
            EnsureVisualTree();
        }

        private void Update()
        {
            EnsureVisualTree();

            float rotorDelta = _rotorSpeed * Time.deltaTime;
            _leftRotor.Rotate(0f, 0f, rotorDelta);
            _rightRotor.Rotate(0f, 0f, -rotorDelta);

            float throwOscillation = Mathf.Sin(Time.time * _armThrowFrequency) * _armThrowAngle;
            _leftArm.localRotation = Quaternion.Euler(0f, 0f, throwOscillation);
            _rightArm.localRotation = Quaternion.Euler(0f, 0f, -throwOscillation);

            float bob = Mathf.Sin(Time.time * 4f) * 0.03f;
            _body.localPosition = new Vector3(0f, bob, 0f);
        }

        private void EnsureVisualTree()
        {
            _shadow = GetOrCreatePart("Shadow", new Vector3(0f, -0.52f, 0.2f), new Vector3(1.05f, 0.24f, 1f), new Color(0f, 0f, 0f, 0.22f));
            _body = GetOrCreatePart("Body", Vector3.zero, new Vector3(0.82f, 0.62f, 1f), new Color(0.34f, 0.84f, 1f, 1f));
            _leftRotor = GetOrCreatePart("RotorLeft", new Vector3(-0.45f, 0.35f, -0.05f), new Vector3(0.18f, 0.72f, 1f), new Color(0.95f, 0.95f, 1f, 0.95f));
            _rightRotor = GetOrCreatePart("RotorRight", new Vector3(0.45f, 0.35f, -0.05f), new Vector3(0.18f, 0.72f, 1f), new Color(0.95f, 0.95f, 1f, 0.95f));
            _leftArm = GetOrCreatePart("ArmLeft", new Vector3(-0.28f, -0.08f, -0.02f), new Vector3(0.14f, 0.58f, 1f), new Color(0.85f, 0.9f, 1f, 1f));
            _rightArm = GetOrCreatePart("ArmRight", new Vector3(0.28f, -0.08f, -0.02f), new Vector3(0.14f, 0.58f, 1f), new Color(0.85f, 0.9f, 1f, 1f));
        }

        private Transform GetOrCreatePart(string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            Transform child = transform.Find(name);
            SpriteRenderer renderer;
            if (child == null)
            {
                GameObject go = new GameObject(name);
                go.transform.SetParent(transform, false);
                child = go.transform;
                renderer = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                renderer = child.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = child.gameObject.AddComponent<SpriteRenderer>();
                }
            }

            child.localPosition = localPosition;
            child.localScale = localScale;
            renderer.sprite = RuntimeSpriteFactory.WhiteSquare;
            renderer.color = color;
            return child;
        }
    }
}
