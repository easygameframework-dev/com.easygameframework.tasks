
namespace EasyGameFramework.Tasks.Implementations
{
    public class SubscriptionOnDestroyTrigger : SubscriptionTrigger
    {
        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
