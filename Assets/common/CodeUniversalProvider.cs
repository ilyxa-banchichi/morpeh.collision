using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;

#if UNITY_EDITOR
using Scellecs.Morpeh.Editor;
#endif

namespace Scellecs.Morpeh.Providers
{
    public abstract class CodeUniversalProvider : UniversalProvider
    {
#if UNITY_EDITOR
        private List<Type> _types = new List<Type>();
#endif

        protected override void OnEnable()
        {
            base.OnEnable();

#if UNITY_EDITOR
            viewer.world = World.Default;
            viewer.entity = this.Entity;
#endif
        }

        protected abstract void RegisterTypes();

        protected void RegisterType<TComponent>() where TComponent : IComponent
        {
#if UNITY_EDITOR
            var type = typeof(TComponent);
            if (!_types.Contains(type))
                _types.Add(type);
#endif
        }

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            RegisterTypes();

            if (_types.Count != serializedComponents.Length)
            {
                var temp = serializedComponents;
                serializedComponents = new IComponent[_types.Count];

                int i = 0;
                foreach (var type in _types)
                {
                    var exist = temp.FirstOrDefault(c => c.GetType() == type);
                    if (exist != null)
                        serializedComponents[i] = exist;
                    else
                        serializedComponents[i] = (IComponent)Activator.CreateInstance(type);

                    i++;
                }
            }
#endif

            base.OnValidate();
        }
        
        protected virtual void OnDrawGizmos()
        {
            for (var i = 0; i < serializedComponents.Length; i++)
            {
                var component = serializedComponents[i];
                if (component is IDrawGizmos drawable) 
                    drawable.OnDrawGizmos(gameObject);
            }
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            for (var i = 0; i < serializedComponents.Length; i++)
            {
                var component = serializedComponents[i];
                if (component is IDrawGizmosSelected drawable) 
                    drawable.OnDrawGizmosSelected(gameObject);
            }
        }

#if UNITY_EDITOR
        [ShowInInspector, HideReferencePicker, SerializeField, InlineProperty, HideLabel]
        [Title("Debug Info", HorizontalLine = true)]
        private EntityViewer viewer = new EntityViewer();
#endif
    }
}
