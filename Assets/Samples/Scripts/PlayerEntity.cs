using NativeTrees;
using Scellecs.Morpeh;
using Unity.Collections;
using UnityEngine;

namespace Samples.Scripts
{
    public class PlayerEntity : TestEntity
    {
        [SerializeField]
        private float _range = 1f;

        
        protected override void RegisterTypes()
        {
            base.RegisterTypes();
            RegisterType<PlayerTag>();
        }

        protected void Update()
        {

        }
    }
}