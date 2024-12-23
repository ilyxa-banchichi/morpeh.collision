using Scellecs.Morpeh;
using Scellecs.Morpeh.Collision.Requests;

namespace Samples.Scripts
{
    public class TestEntity : HierarchyCodeUniversalProvider
    {
        protected override void RegisterTypes()
        {
            RegisterType<CreateBoxColliderRequest>();
            RegisterType<GameObjectComponent>();
        }
    }
}