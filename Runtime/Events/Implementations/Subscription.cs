using System;

namespace EasyGameFramework.Tasks.Implementations
{
    public class Subscription : ISubscription
    {
        private readonly Action _onUnsubscribe;

        public Subscription(Action onUnsubscribe)
        {
            _onUnsubscribe = onUnsubscribe;
        }

        public void Unsubscribe()
        {
            _onUnsubscribe?.Invoke();
        }
    }
}
