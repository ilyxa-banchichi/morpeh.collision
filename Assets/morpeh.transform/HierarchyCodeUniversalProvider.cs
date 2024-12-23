using Scellecs.Morpeh.Providers;
using TriInspector;
using UnityEngine;

namespace Scellecs.Morpeh
{
    public abstract class HierarchyCodeUniversalProvider : CodeUniversalProvider
    {
        public bool Initialized { get; set; } = false;

        [ShowInInspector, ShowInPlayMode]
        public string Parent
        {
            get
            {
                if (!Entity.IsNullOrDisposed())
                {
                    var data = Entity.GetComponent<TransformComponent>();
                    if (data.Parent != default)
                        return data.Parent.ToString();
                }

                return "None";
            }
        }

        [ShowInInspector, ShowInPlayMode]
        public int ChildsCount
        {
            get
            {
                if (!Entity.IsNullOrDisposed())
                {
                    var data = Entity.GetComponent<TransformComponent>();
                    var children = data.Children;

                    if (children.IsCreated)
                        return children.Length;
                }

                return 0;
            }
        }

        [ShowInInspector, ShowInPlayMode]
        public Vector3 Position
        {
            get
            {
                if (!Entity.IsNullOrDisposed())
                {
                    var data = Entity.GetComponent<TransformComponent>();
                    return data.Position();
                }

                return Vector3.zero;
            }
        }

        [ShowInInspector, ShowInPlayMode]
        public Quaternion Rotation
        {
            get
            {
                if (!Entity.IsNullOrDisposed())
                {
                    var data = Entity.GetComponent<TransformComponent>();
                    return data.Rotation();
                }

                return Quaternion.identity;
            }
        }

        [ShowInInspector, ShowInPlayMode]
        public Vector3 Scale
        {
            get
            {
                if (!Entity.IsNullOrDisposed())
                {
                    var data = Entity.GetComponent<TransformComponent>();
                    return data.Scale();
                }

                return Vector3.zero;
            }
        }

        private void Start()
        {
            if (!Initialized)
                TransformHelper.InitializeTransformComponent(gameObject);
        }

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            RegisterType<TransformComponent>();
#endif

            base.OnValidate();
        }
    }
}
