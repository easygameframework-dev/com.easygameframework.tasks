using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Core.Event;

namespace EasyGameFramework.Tasks
{
    public static class UIExtensions
    {
        private static readonly Dictionary<int, UniTaskCompletionSource<UIForm>> UIFormOpenCompletedTcsBySerialId =
            new Dictionary<int, UniTaskCompletionSource<UIForm>>();

        static UIExtensions()
        {
            var eventComponent = GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe<OpenUIFormSuccessEventArgs>(OnEvent);
            eventComponent.Subscribe<OpenUIFormFailureEventArgs>(OnEvent);
        }

        /// <summary>
        /// 异步打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面。</returns>
        public static UniTask<UIForm> OpenUIFormAsync(this UIComponent uiComponent,
            string uiFormAssetName,
            string uiGroupName,
            string customPackageName = "",
            bool pauseCoveredUIForm = false,
            int? customPriority = null,
            object userData = null)
        {
            var serialId = uiComponent.OpenUIForm(uiFormAssetName, uiGroupName, customPackageName, customPriority,
                pauseCoveredUIForm, userData);
            if (UIFormOpenCompletedTcsBySerialId.TryGetValue(serialId, out var tcs))
            {
                return tcs.Task;
            }

            tcs = new UniTaskCompletionSource<UIForm>();
            UIFormOpenCompletedTcsBySerialId.Add(serialId, tcs);
            return tcs.Task;
        }

        private static void OnEvent(object sender, OpenUIFormSuccessEventArgs e)
        {
            if (UIFormOpenCompletedTcsBySerialId.Remove(e.UIForm.SerialId, out var tcs))
            {
                tcs.TrySetResult(e.UIForm);
            }
        }

        private static void OnEvent(object sender, OpenUIFormFailureEventArgs e)
        {
            if (UIFormOpenCompletedTcsBySerialId.Remove(e.SerialId, out var tcs))
            {
                tcs.TrySetException(new Exception(e.ErrorMessage));
            }
        }
    }
}
