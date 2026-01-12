using System.Collections.Generic;
using UnityEngine;

namespace EasyGameFramework.Tasks
{
    public static class SubscriptionExtensions
    {
        public static ISubscription UnsubscribeWhenDestroyed(
            this ISubscription subscription,
            GameObject gameObject)
        {
            var trigger = gameObject.GetOrAddComponent<Implementations.SubscriptionOnDestroyTrigger>();
            trigger.AddUnsubscribe(subscription);
            return subscription;
        }

        public static ISubscription UnsubscribeWhenDisabled(
            this ISubscription subscription,
            GameObject gameObject)
        {
            var trigger = gameObject.GetOrAddComponent<Implementations.SubscriptionOnDisableTrigger>();
            trigger.AddUnsubscribe(subscription);
            return subscription;
        }
    }
}
