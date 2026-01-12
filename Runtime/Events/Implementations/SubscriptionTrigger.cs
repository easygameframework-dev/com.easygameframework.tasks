using System.Collections.Generic;
using UnityEngine;

namespace EasyGameFramework.Tasks.Implementations
{
    public abstract class SubscriptionTrigger : MonoBehaviour
    {
        private readonly HashSet<ISubscription> _unsubscribes = new HashSet<ISubscription>();

        public void AddUnsubscribe(ISubscription subscription) => _unsubscribes.Add(subscription);

        public void RemoveUnsubscribe(ISubscription subscription) => _unsubscribes.Remove(subscription);

        public void Unsubscribe()
        {
            foreach (var unsubscribe in _unsubscribes)
            {
                unsubscribe.Unsubscribe();
            }

            _unsubscribes.Clear();
        }
    }

}
