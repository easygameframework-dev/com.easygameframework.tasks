using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Core.Event;
using EasyGameFramework.Core.Resource;
using EasyGameFramework.Essentials;

namespace EasyGameFramework.Tasks
{
    public static class EntityExtensions
    {
        private static readonly Dictionary<int, UniTaskCompletionSource<Entity>> EntityShowCompletedTcsBySerialId =
            new Dictionary<int, UniTaskCompletionSource<Entity>>();

        static EntityExtensions()
        {
            var eventComponent = GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe<ShowEntitySuccessEventArgs>(OnEvent);
            eventComponent.Subscribe<ShowEntityFailureEventArgs>(OnEvent);
        }

        public static UniTask<Entity> ShowEntityAsync(this EntityComponent entityComponent,
            int entityId,
            Type entityLogicType,
            AssetAddress entityAssetAddress,
            string entityGroupName,
            int? customPriority = null,
            object userData = null)
        {
            if (EntityShowCompletedTcsBySerialId.TryGetValue(entityId, out var tcs))
            {
                return tcs.Task;
            }

            tcs = new UniTaskCompletionSource<Entity>();
            EntityShowCompletedTcsBySerialId.Add(entityId, tcs);

            entityComponent.ShowEntity(entityId, entityLogicType, entityAssetAddress, entityGroupName,
                customPriority ?? Constant.AssetPriority.EntityAsset, userData);

            return tcs.Task;
        }

        private static void OnEvent(object sender, ShowEntitySuccessEventArgs e)
        {
            if (EntityShowCompletedTcsBySerialId.Remove(e.Entity.Id, out var tcs))
            {
                tcs.TrySetResult(e.Entity);
            }
        }

        private static void OnEvent(object sender, ShowEntityFailureEventArgs e)
        {
            if (EntityShowCompletedTcsBySerialId.Remove(e.EntityId, out var tcs))
            {
                tcs.TrySetException(new Exception(e.ErrorMessage));
            }
        }
    }
}
