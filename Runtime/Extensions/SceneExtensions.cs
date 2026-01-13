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
        private static readonly Dictionary<string, UniTaskCompletionSource<Scene>> SceneLoadCompletedTcsByAssetPath =
            new Dictionary<string, UniTaskCompletionSource<Scene>>();

        private static readonly Dictionary<string, UniTaskCompletionSource> SceneUnloadCompletedTcsByAssetPath =
            new Dictionary<string, UniTaskCompletionSource>();
        
        static SceneExtensions()
        {
            var eventComponent = GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe<LoadSceneSuccessEventArgs>(OnEvent);
            eventComponent.Subscribe<LoadSceneFailureEventArgs>(OnEvent);
            eventComponent.Subscribe<UnloadSceneSuccessEventArgs>(OnEvent);
            eventComponent.Subscribe<UnloadSceneFailureEventArgs>(OnEvent);
        }

        public static UniTask<Scene> LoadSceneAsync(this SceneComponent sceneComponent,
            string sceneAssetName,
            string customPackageName = "",
            LoadSceneMode sceneMode = LoadSceneMode.Single,
            LocalPhysicsMode physicsMode = LocalPhysicsMode.None,
            int? customPriority = null)
        {
            var resourcesComponent = GameEntry.GetComponent<ResourceComponent>();
            var packageName = string.IsNullOrEmpty(customPackageName)
                ? resourcesComponent.DefaultPackageName
                : customPackageName;

            var key = $"{packageName}/{sceneAssetName}";

            if (SceneLoadCompletedTcsByAssetPath.TryGetValue(key, out var tcs))
            {
                return tcs.Task;
            }

            tcs = new UniTaskCompletionSource<Scene>();
            SceneLoadCompletedTcsByAssetPath[key] = tcs;

            sceneComponent.LoadScene(
                new AssetAddress(packageName, sceneAssetName),
                customPriority,
                new LoadSceneParameters(sceneMode, physicsMode));

            return tcs.Task;
        }

        public static UniTask UnloadSceneAsync(this SceneComponent sceneComponent,
            string sceneAssetName,
            string customPackageName = "")
        {
            var resourcesComponent = GameEntry.GetComponent<ResourceComponent>();
            var packageName = string.IsNullOrEmpty(customPackageName)
                ? resourcesComponent.DefaultPackageName
                : customPackageName;

            var key = $"{packageName}/{sceneAssetName}";
            if (SceneUnloadCompletedTcsByAssetPath.TryGetValue(key, out var tcs))
            {
                return tcs.Task;
            }
            tcs = new UniTaskCompletionSource();
            SceneUnloadCompletedTcsByAssetPath[key] = tcs;
            sceneComponent.UnloadScene(new AssetAddress(packageName, sceneAssetName));
            return tcs.Task;
        }

        private static void OnEvent(object sender, LoadSceneSuccessEventArgs e)
        {
            var path = $"{e.PackageName}/{e.SceneAssetName}";
            var tcs = SceneLoadCompletedTcsByAssetPath[path];

            if (e.SceneAsset is not Scene scene)
            {
                throw new Exception($"Scene asset '{path}' is not a scene.");
            }

            tcs.TrySetResult(scene);
            SceneLoadCompletedTcsByAssetPath.Remove(path);
        }

        private static void OnEvent(object sender, LoadSceneFailureEventArgs e)
        {
            var path = $"{e.PackageName}/{e.SceneAssetName}";
            var tcs = SceneLoadCompletedTcsByAssetPath[path];
            tcs.TrySetException(new Exception(e.ErrorMessage));
            SceneLoadCompletedTcsByAssetPath.Remove(path);
        }

        private static void OnEvent(object sender, UnloadSceneSuccessEventArgs e)
        {
            var path = $"{e.PackageName}/{e.SceneAssetName}";
            var tcs = SceneUnloadCompletedTcsByAssetPath[path];
            tcs.TrySetResult();
            SceneUnloadCompletedTcsByAssetPath.Remove(path);
        }

        private static void OnEvent(object sender, UnloadSceneFailureEventArgs e)
        {
            var path = $"{e.PackageName}/{e.SceneAssetName}";
            var tcs = SceneUnloadCompletedTcsByAssetPath[path];

            tcs.TrySetException(new Exception(e.ErrorMessage));
            SceneUnloadCompletedTcsByAssetPath.Remove(path);
        }
    }
}
