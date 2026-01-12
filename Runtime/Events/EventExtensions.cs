using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using EasyGameFramework.Core.Event;

namespace EasyGameFramework.Tasks
{
    public static class EventExtensions
    {
        private static readonly Dictionary<Delegate, EventHandler<GameEventArgs>> Handlers =
            new Dictionary<Delegate, EventHandler<GameEventArgs>>();

        private static int s_nextEventId = 1000;
        private static readonly ConcurrentDictionary<Type, int> EventIdByType = new ConcurrentDictionary<Type, int>();

        private struct EventIdFastGetter<T>
        {
            public static readonly int EventId = GetEventId(typeof(T));
        }

        public static int GetEventId(Type eventType)
        {
            return EventIdByType.GetOrAdd(eventType, _ =>
            {
                // compatible with old events
                var eventIdField = eventType.GetField("EventId", BindingFlags.Static | BindingFlags.Public);
                if (eventIdField != null)
                {
                    return (int)eventIdField.GetValue(null);
                }

                return Interlocked.Increment(ref s_nextEventId);
            });
        }

        public static ISubscription SubscribeWeak(this EventComponent eventComponent, Type eventType,
            EventHandler<GameEventArgs> handler)
        {
            if (!Handlers.TryAdd(handler, handler))
            {
                throw new ArgumentException($"Handler '{handler}' already exists.");
            }

            int eventId = GetEventId(eventType);
            eventComponent.Subscribe(eventId, handler);
            return new Implementations.Subscription(() =>
            {
                eventComponent.Unsubscribe(eventId, handler);
                Handlers.Remove(handler);
            });
        }

        public static ISubscription Subscribe<T>(this EventComponent eventComponent, EventHandler<T> handler)
            where T : GameEventArgs
        {
            if (!Handlers.TryAdd(handler, Handler))
            {
                throw new ArgumentException($"Handler '{handler}' already exists.");
            }

            int eventId = EventIdFastGetter<T>.EventId;
            eventComponent.Subscribe(eventId, Handler);
            return new Implementations.Subscription(() =>
            {
                eventComponent.Unsubscribe(eventId, Handler);
                Handlers.Remove(handler);
            });

            void Handler(object sender, GameEventArgs e)
            {
                handler(sender, (T)e);
            }
        }

        public static void UnsubscribeWeak(this EventComponent eventComponent, Type eventType,
            EventHandler<GameEventArgs> handler)
        {
            if (Handlers.TryGetValue(handler, out var eventHandler))
            {
                int eventId = GetEventId(eventType);
                eventComponent.Unsubscribe(eventId, eventHandler);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsubscribe<{eventType}> must corresponds to Subscribe<{eventType}>");
            }
        }

        public static void Unsubscribe<T>(this EventComponent eventComponent, EventHandler<T> handler)
            where T : GameEventArgs
        {
            if (Handlers.TryGetValue(handler, out var eventHandler))
            {
                int eventId = EventIdFastGetter<T>.EventId;
                eventComponent.Unsubscribe(eventId, eventHandler);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsubscribe<{typeof(T)}> must corresponds to Subscribe<{typeof(T)}>");
            }
        }
    }
}
