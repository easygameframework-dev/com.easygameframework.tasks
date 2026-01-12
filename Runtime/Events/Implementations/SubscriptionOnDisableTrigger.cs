
namespace EasyGameFramework.Tasks.Implementations
{
    public class SubscriptionOnDisableTrigger : SubscriptionTrigger
    {
        private void OnDisable()
        {
            Unsubscribe();
        }
    }
}
