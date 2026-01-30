using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Core.Event;
using EasyGameFramework.Core.Resource;
using EasyGameFramework.Essentials;
using UnityEngine.SceneManagement;

namespace EasyGameFramework.Tasks
{
    public static class SceneExtensions
    {
        private static readonly Dictionary<AssetAddress, UniTaskCompletionSource<Scene>> SceneLoadCompletedTcsByAssetAddress =
            new Dictionary<AssetAddress, UniTaskCompletionSource<Scene>>();

        private static readonly Dictionary<AssetAddress, UniTaskCompletionSource> SceneUnloadCompletedTcsByAssetAddress =
            new Dictionary<AssetAddress, UniTaskCompletionSource>();

        static SceneExtensions()
        {
            var eventComponent = GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe<LoadSceneSuccessEventArgs>(OnEvent);
            eventComponent.Subscribe<LoadSceneFailureEventArgs>(OnEvent);
            eventComponent.Subscribe<UnloadSceneSuccessEventArgs>(OnEvent);
            eventComponent.Subscribe<UnloadSceneFailureEventArgs>(OnEvent);
        }

        public static UniTask<Scene> LoadSceneAsync(this SceneComponent sceneComponent,
            AssetAddress sceneAssetAddress,
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            int? customPriority = null)
        {
            if (SceneLoadCompletedTcsByAssetAddress.TryGetValue(sceneAssetAddress, out var tcs))
            {
                return tcs.Task;
            }

            tcs = new UniTaskCompletionSource<Scene>();
            SceneLoadCompletedTcsByAssetAddress[sceneAssetAddress] = tcs;

            sceneComponent.LoadScene(
                sceneAssetAddress,
                customPriority ?? Constant.AssetPriority.SceneAsset,
                new LoadSceneParameters(sceneMode, physicsMode));

            return tcs.Task;
        }

        public static UniTask UnloadSceneAsync(this SceneComponent sceneComponent,
            AssetAddress sceneAssetAddress)
        {
            if (SceneUnloadCompletedTcsByAssetAddress.TryGetValue(sceneAssetAddress, out var tcs))
            {
                return tcs.Task;
            }
            tcs = new UniTaskCompletionSource();
            SceneUnloadCompletedTcsByAssetAddress[sceneAssetAddress] = tcs;
            sceneComponent.UnloadScene(sceneAssetAddress);
            return tcs.Task;
        }

        /// <summary>
        /// 异步加载游戏场景，支持完整的初始化功能。
        /// </summary>
        /// <param name="sceneComponent">SceneComponent 实例。</param>
        /// <param name="sceneAssetAddress">要加载的场景的资源地址。</param>
        /// <param name="userData">传递给场景初始化器的可选自定义数据。</param>
        /// <param name="retryPolicy">加载失败的可选重试策略。</param>
        /// <returns>在场景加载并初始化完成时完成的 UniTask。</returns>
        public static UniTask LoadGameSceneAsync(
            this SceneComponent sceneComponent,
            AssetAddress sceneAssetAddress,
            object userData = null,
            IRetryPolicy retryPolicy = null)
        {
            return GameEntry.GetComponent<GameSceneComponent>()
                .LoadGameSceneAsync(sceneAssetAddress, userData, retryPolicy);
        }

        private static void OnEvent(object sender, LoadSceneSuccessEventArgs e)
        {
            var tcs = SceneLoadCompletedTcsByAssetAddress[e.SceneAssetAddress];

            if (e.SceneAsset is not Scene scene)
            {
                throw new Exception($"Scene asset '{e.SceneAssetAddress}' is not a scene.");
            }

            tcs.TrySetResult(scene);
            SceneLoadCompletedTcsByAssetAddress.Remove(e.SceneAssetAddress);
        }

        private static void OnEvent(object sender, LoadSceneFailureEventArgs e)
        {
            var tcs = SceneLoadCompletedTcsByAssetAddress[e.SceneAssetAddress];
            tcs.TrySetException(new Exception(e.ErrorMessage));
            SceneLoadCompletedTcsByAssetAddress.Remove(e.SceneAssetAddress);
        }

        private static void OnEvent(object sender, UnloadSceneSuccessEventArgs e)
        {
            var tcs = SceneUnloadCompletedTcsByAssetAddress[e.SceneAssetAddress];
            tcs.TrySetResult();
            SceneUnloadCompletedTcsByAssetAddress.Remove(e.SceneAssetAddress);
        }

        private static void OnEvent(object sender, UnloadSceneFailureEventArgs e)
        {
            var tcs = SceneUnloadCompletedTcsByAssetAddress[e.SceneAssetAddress];

            tcs.TrySetException(new Exception(e.ErrorMessage));
            SceneUnloadCompletedTcsByAssetAddress.Remove(e.SceneAssetAddress);
        }
    }
}
