using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Core.Event;
using EasyGameFramework.Core.Resource;
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
                customPriority,
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
