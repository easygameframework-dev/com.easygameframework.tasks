using System;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Essentials;
using JetBrains.Annotations;

namespace EasyGameFramework.Tasks
{
    public static partial class UIExtensions
    {
        public static UniTask BeginSpinnerBoxAsync(this UIComponent uiComponent,
            string descriptionGetter,
            int initialPercentage,
            float backgroundAlpha = 0.2f)
        {
            return BeginSpinnerBoxAsync(uiComponent, () => descriptionGetter, initialPercentage,
                backgroundAlpha,
                Constant.UIGroup.Popup);
        }

        public static UniTask BeginSpinnerBoxAsync(this UIComponent uiComponent,
            Func<string> descriptionGetter,
            int initialPercentage,
            float backgroundAlpha = 0.2f)
        {
            return BeginSpinnerBoxAsync(uiComponent, descriptionGetter, initialPercentage,
                backgroundAlpha,
                Constant.UIGroup.Popup);
        }

        public static async UniTask BeginSpinnerBoxAsync(this UIComponent uiComponent,
            Func<string> descriptionGetter,
            int initialPercentage,
            float backgroundAlpha,
            string groupName)
        {
            if (UISpinnerBox.LastSpinnerBox != null)
            {
                throw new InvalidOperationException("Last spinner box is showing.");
            }

            var assetReference = UISpinnerBoxConfigAsset.Instance.AssetReference;
            var form = await uiComponent.OpenUIFormAsync(assetReference.ToAssetAddress(), groupName);
            if (form.Logic is not UISpinnerBox spinnerBox)
            {
                throw new InvalidOperationException(
                    $"UI form logic type '{form.GetType()}' is not '{typeof(UISpinnerBox)}'.");
            }

            UISpinnerBox.LastSpinnerBox = spinnerBox;
            spinnerBox.DescriptionGetter = descriptionGetter;
            spinnerBox.Percentage = initialPercentage;
            spinnerBox.BackgroundAlpha = backgroundAlpha;
        }

        public static UniTask UpdateSpinnerBoxAsync(this UIComponent uiComponent,
            string descriptionGetter,
            float destinationPercentage,
            float duration = 0.1f,
            float? backgroundAlpha = null)
        {
            return UpdateSpinnerBoxAsync(uiComponent, () => descriptionGetter, destinationPercentage, duration,
                backgroundAlpha);
        }

        public static UniTask UpdateSpinnerBoxAsync(this UIComponent uiComponent,
            float destinationPercentage,
            float duration = 0.1f,
            float? backgroundAlpha = null)
        {
            return UpdateSpinnerBoxAsync(uiComponent, (Func<string>)null, destinationPercentage, duration,
                backgroundAlpha);
        }

        public static UniTask UpdateSpinnerBoxAsync(this UIComponent uiComponent,
            [CanBeNull] Func<string> descriptionGetter,
            float destinationPercentage,
            float duration = 0.1f,
            float? backgroundAlpha = null)
        {
            if (UISpinnerBox.LastSpinnerBox == null)
            {
                throw new InvalidOperationException("Spinner box is not showing, must use BeginSpinnerBoxAsync first.");
            }

            if (descriptionGetter != null)
            {
                UISpinnerBox.LastSpinnerBox.DescriptionGetter = descriptionGetter;
            }

            if (backgroundAlpha != null)
            {
                UISpinnerBox.LastSpinnerBox.BackgroundAlpha = backgroundAlpha.Value;
            }

            var arrivedTcs = new UniTaskCompletionSource();
            UISpinnerBox.LastSpinnerBox.SetDestinationPercentage(destinationPercentage, duration, () => { arrivedTcs.TrySetResult(); });

            return arrivedTcs.Task;
        }

        public static async UniTask EndSpinnerBoxAsync(this UIComponent uiComponent)
        {
            uiComponent.CloseUIForm(UISpinnerBox.LastSpinnerBox.UIForm);
            UISpinnerBox.LastSpinnerBox = null;
        }
    }
}
