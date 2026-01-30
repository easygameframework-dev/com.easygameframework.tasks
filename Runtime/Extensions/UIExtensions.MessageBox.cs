using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Essentials;

namespace EasyGameFramework.Tasks
{
    public enum UIMessageBoxButtons
    {
        Ok,
        OkCancel,
        Yes,
        YesNo,
        YesNoCancel,
    }

    public enum UIMessageBoxType
    {
        Tip,
        Warn,
        Error,
        Fatal
    }

    public enum UIMessageBoxStyle
    {
        Builtin
    }

    public static partial class UIExtensions
    {
        public static UniTask<int> ShowMessageBoxAsync(this UIComponent uiComponent,
            string message,
            UIMessageBoxButtons buttons = UIMessageBoxButtons.Ok)
        {
            return uiComponent.ShowMessageBoxAsync(message, UIMessageBoxType.Tip, buttons);
        }

        public static UniTask<int> ShowMessageBoxAsync(this UIComponent uiComponent,
            string message,
            UIMessageBoxType type,
            UIMessageBoxButtons buttons = UIMessageBoxButtons.Ok)
        {
            return uiComponent.ShowMessageBoxAsync(message, Constant.UIGroup.Popup, type,
                buttons);
        }

        public static UniTask<int> ShowMessageBoxAsync(this UIComponent uiComponent,
            string message,
            string groupName,
            UIMessageBoxType type,
            UIMessageBoxButtons buttons = UIMessageBoxButtons.Ok)
        {
            return uiComponent.ShowMessageBoxAsync(message, GetMessageBoxTitle(type), groupName, type,
                GetMessageBoxButtonTexts(buttons));
        }

        public static UniTask<int> ShowMessageBoxAsync(this UIComponent uiComponent,
            string message,
            string title,
            params string[] buttonTexts)
        {
            return uiComponent.ShowMessageBoxAsync(message, title, UIMessageBoxType.Tip, buttonTexts);
        }

        public static UniTask<int> ShowMessageBoxAsync(this UIComponent uiComponent,
            string message,
            string title,
            UIMessageBoxType type,
            params string[] buttonTexts)
        {
            return uiComponent.ShowMessageBoxAsync(message, title, Constant.UIGroup.Popup, type, buttonTexts);
        }

        public static async UniTask<int> ShowMessageBoxAsync(this UIComponent uiComponent,
            string message,
            string title,
            string groupName,
            UIMessageBoxType type,
            params string[] buttonTexts)
        {
            var assetReference = UIMessageBoxConfigAsset.Instance.AssetReference;
            var form = await uiComponent.OpenUIFormAsync(assetReference.ToAssetAddress(), groupName);

            if (form.Logic is not UIMessageBox messageBox)
            {
                throw new InvalidOperationException(
                    $"UI form logic type '{form.GetType()}' is not '{typeof(UIMessageBox)}'.");
            }

            messageBox.Message = message;
            messageBox.Title = title;

            var clickCompletedTcs = new UniTaskCompletionSource<int>();
            for (int i = 0; i < buttonTexts.Length; i++)
            {
                int localI = i;
                messageBox.AddButton(buttonTexts[i], _ => { clickCompletedTcs.TrySetResult(localI); });
            }

            var index = await clickCompletedTcs.Task;
            uiComponent.CloseUIForm(form);
            return index;
        }

        private static string[] GetMessageBoxButtonTexts(UIMessageBoxButtons buttons)
        {
            var config = UIMessageBoxConfigAsset.Instance;
            switch (buttons)
            {
                case UIMessageBoxButtons.Ok:
                    return config.OkTexts.ToArray();
                case UIMessageBoxButtons.OkCancel:
                    return config.OkCancelTexts.ToArray();
                case UIMessageBoxButtons.Yes:
                    return config.YesTexts.ToArray();
                case UIMessageBoxButtons.YesNo:
                    return config.YesNoTexts.ToArray();
                case UIMessageBoxButtons.YesNoCancel:
                    return config.YesNoCancelTexts.ToArray();
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttons), buttons, null);
            }
        }

        private static string GetMessageBoxTitle(UIMessageBoxType type)
        {
            var config = UIMessageBoxConfigAsset.Instance;
            switch (type)
            {
                case UIMessageBoxType.Tip:
                    return config.TipTitle;
                case UIMessageBoxType.Warn:
                    return config.WarnTitle;
                case UIMessageBoxType.Error:
                    return config.ErrorTitle;
                case UIMessageBoxType.Fatal:
                    return config.FatalTitle;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
