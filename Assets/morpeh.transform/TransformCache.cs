using Scellecs.Morpeh.Transform.Components;

namespace Scellecs.Morpeh
{
    public static class TransformCache
    {
        public static World World { get; private set; }
        public static Stash<TransformComponent> Stash { get; private set; }

        public static void RefreshCache(World world)
        {
            World = world;
            Stash = World.GetStash<TransformComponent>();
        }
    }
}
