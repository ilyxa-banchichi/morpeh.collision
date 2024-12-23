using Scellecs.Morpeh.Hierarchy.Systems;

namespace Scellecs.Morpeh
{
    public static class HierarchyStashCache
    {
        public static World World { get; private set; }
        public static Stash<ParentComponent> ParentStash { get; private set; }
        public static Stash<PreviousParentComponent> PreviousParentStash { get; private set; }
        public static Stash<ChildComponent> ChildStash { get; private set; }
        public static Stash<ParentChangedRequest> ParentChangedRequestStash { get; private set; }

        public static void RefreshCache(World world)
        {
            World = world;
            ParentStash = World.GetStash<ParentComponent>();
            PreviousParentStash = World.GetStash<PreviousParentComponent>();
            ChildStash = World.GetStash<ChildComponent>();
            ParentChangedRequestStash = World.GetStash<ParentChangedRequest>();
        }
    }
}
