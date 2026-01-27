using System;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Essentials;
using UnityEngine;

namespace EasyGameFramework.Tasks
{
    public class AsyncSceneInitializer : MonoBehaviour, ISceneInitializer
    {
        void ISceneInitializer.Initialize(object userData, Action onSuccess, Action<Exception> onFailure)
        {
            InitializeAsync(userData)
                .ContinueWith(onSuccess)
                .Forget(onFailure);
        }

        protected virtual UniTask InitializeAsync(object userData)
        {
            return UniTask.CompletedTask;
        }
    }
}
