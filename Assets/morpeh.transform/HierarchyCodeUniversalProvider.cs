using Scellecs.Morpeh.Hierarchy.Systems;
using Scellecs.Morpeh.Providers;
using Scellecs.Morpeh.Transform.Components;
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
                if (!Entity.IsNullOrDisposed() && Entity.Has<ParentComponent>())
                {
                    var data = Entity.GetComponent<ParentComponent>();
                    if (data.Value != default)
                        return data.Value.ToString();
                }

                return "None";
            }
        }

        [ShowInInspector, ShowInPlayMode]
        public int ChildsCount
        {
            get
            {
                if (!Entity.IsNullOrDisposed() && Entity.Has<ChildComponent>())
                {
                    var data = Entity.GetComponent<ChildComponent>();
                    var children = data.Value;
                
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
