using System;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Core.Resource;
using EasyGameFramework.Essentials;

namespace EasyGameFramework.Tasks
{
    /// <summary>
    /// 为 GameSceneComponent 提供异步扩展方法。
    /// </summary>
    public static class GameSceneExtensions
    {
        /// <summary>
        /// 异步加载游戏场景并自动初始化。
        /// </summary>
        /// <param name="component">GameSceneComponent 实例。</param>
        /// <param name="sceneAssetAddress">要加载的场景的资源地址。</param>
        /// <param name="userData">传递给场景初始化器的可选自定义数据。</param>
        /// <param name="retryPolicy">加载失败的可选重试策略。</param>
        /// <returns>在场景加载并初始化完成时完成的 UniTask。</returns>
        /// <exception cref="SceneLoadException">场景加载失败时抛出。</exception>
        /// <exception cref="SceneInitializeException">场景初始化失败时抛出。</exception>
        public static UniTask LoadGameSceneAsync(
            this GameSceneComponent component,
            AssetAddress sceneAssetAddress,
            object userData = null,
            IRetryPolicy retryPolicy = null)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            component.LoadGameScene(
                sceneAssetAddress,
                stateChanged: null,
                onSuccess: () => tcs.TrySetResult(true),
                onFailure: exception => tcs.TrySetException(exception),
                userData: userData,
                retryPolicy: retryPolicy);

            return tcs.Task;
        }

        /// <summary>
        /// 异步加载游戏场景并自动初始化，支持进度报告。
        /// </summary>
        /// <param name="component">GameSceneComponent 实例。</param>
        /// <param name="sceneAssetAddress">要加载的场景的资源地址。</param>
        /// <param name="progressCallback">加载期间状态变化的可选回调。</param>
        /// <param name="userData">传递给场景初始化器的可选自定义数据。</param>
        /// <param name="retryPolicy">加载失败的可选重试策略。</param>
        /// <returns>在场景加载并初始化完成时完成的 UniTask。</returns>
        /// <exception cref="SceneLoadException">场景加载失败时抛出。</exception>
        /// <exception cref="SceneInitializeException">场景初始化失败时抛出。</exception>
        public static UniTask LoadGameSceneAsync(
            this GameSceneComponent component,
            AssetAddress sceneAssetAddress,
            Action<GameSceneLoadState> progressCallback,
            object userData = null,
            IRetryPolicy retryPolicy = null)
        {
            var tcs = new UniTaskCompletionSource<bool>();

            component.LoadGameScene(
                sceneAssetAddress,
                stateChanged: progressCallback,
                onSuccess: () => tcs.TrySetResult(true),
                onFailure: exception => tcs.TrySetException(exception),
                userData: userData,
                retryPolicy: retryPolicy);

            return tcs.Task;
        }
    }
}
