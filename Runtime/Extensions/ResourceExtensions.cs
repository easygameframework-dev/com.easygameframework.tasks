using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Core.Resource;

namespace EasyGameFramework.Tasks
{
    public static class ResourceExtensions
    {
        private static readonly LoadAssetCallbacks LoadAssetCallbacks = new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFailure);

        private static readonly Dictionary<string, UniTaskCompletionSource<object>> AssetLoadCompletedTcsByPath =
            new Dictionary<string, UniTaskCompletionSource<object>>();

        public static async UniTask<T> LoadAssetAsync<T>(this ResourceComponent resourceComponent, AssetAddress assetAddress,
            int? priority = null,
            object userData = null)
            where T : UnityEngine.Object
        {
            var result = await LoadAssetAsync(resourceComponent, assetAddress, typeof(T), priority, userData);
            return (T)result;
        }

        public static async UniTask<T[]> LoadSubAssetsAsync<T>(this ResourceComponent resourceComponent,
            AssetAddress assetAddress,
            int? priority = null,
            object userData = null)
            where T : UnityEngine.Object
        {
            var result = await LoadAssetAsyncImpl(
                resourceComponent,
                assetAddress,
                typeof(T[]),
                priority,
                userData);
            if (result.GetType() != typeof(T[]))
            {
                return ((UnityEngine.Object[])result).Cast<T>().ToArray();
            }

            return (T[])result;
        }

        public static async UniTask<UnityEngine.Object[]> LoadSubAssetsAsync(this ResourceComponent resourceComponent,
            AssetAddress assetAddress,
            Type assetType = null,
            int? priority = null,
            object userData = null)
        {
            return (UnityEngine.Object[])await LoadAssetAsyncImpl(
                resourceComponent,
                assetAddress,
                assetType == null ? typeof(UnityEngine.Object[]) : assetType.MakeArrayType(),
                priority,
                userData);
        }

        public static async UniTask<UnityEngine.Object> LoadAssetAsync(this ResourceComponent resourceComponent,
            AssetAddress assetAddress,
            Type assetType = null,
            int? priority = null,
            object userData = null)
        {
            return (UnityEngine.Object)await LoadAssetAsyncImpl(resourceComponent, assetAddress, assetType, priority, userData);
        }

        private static UniTask<object> LoadAssetAsyncImpl(this ResourceComponent resourceComponent,
            AssetAddress assetAddress,
            Type assetType = null,
            int? priority = null,
            object userData = null)
        {
            var path = assetAddress.ToString();
            if (AssetLoadCompletedTcsByPath.TryGetValue(path, out var tcs))
            {
                return tcs.Task;
            }

            tcs = new UniTaskCompletionSource<object>();
            AssetLoadCompletedTcsByPath[path] = tcs;
            resourceComponent.LoadAsset(assetAddress, LoadAssetCallbacks, assetType, priority, userData);
            return tcs.Task;
        }

        private static void OnLoadAssetSuccess(AssetAddress assetAddress, object asset, float duration, object userData)
        {
            var path = assetAddress.ToString();
            var tcs = AssetLoadCompletedTcsByPath[path];
            if (asset == null)
            {
                tcs.TrySetException(new Exception($"Load asset '{path}' failed, asset is null"));
            }
            else
            {
                tcs.TrySetResult(asset);
            }

            AssetLoadCompletedTcsByPath.Remove(path);
        }

        private static void OnLoadAssetFailure(AssetAddress assetAddress, LoadResourceStatus status, string error, object userData)
        {
            var path = assetAddress.ToString();
            var tcs = AssetLoadCompletedTcsByPath[path];
            tcs.TrySetException(new Exception(error));
            AssetLoadCompletedTcsByPath.Remove(path);
        }
    }
}
