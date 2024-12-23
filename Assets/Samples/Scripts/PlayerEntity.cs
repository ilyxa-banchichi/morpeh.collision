namespace Samples.Scripts
{
    public class PlayerEntity : TestEntity
    {
        protected override void RegisterTypes()
        {
            base.RegisterTypes();
            RegisterType<PlayerTag>();
        }
    }
}