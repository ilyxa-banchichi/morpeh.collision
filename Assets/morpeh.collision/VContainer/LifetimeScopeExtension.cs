#if MORPEH_ADDONS_VCONTAINER
using Scellecs.Morpeh.Collision;
using VContainer;

namespace ProjectD.Input
{
    public static class LifetimeScopeExtension
    {
        public static IContainerBuilder RegisterCollision(this IContainerBuilder builder)
        {
            builder.Register<ICollisionService, CollisionService>(Lifetime.Singleton);
            return builder;
        }
    }
}
#endif
