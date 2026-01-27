using System;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Essentials;

namespace EasyGameFramework.Tasks
{
    /// <summary>
    /// 提供 UniTask 支持的异步重试策略基类。
    /// </summary>
    /// <remarks>
    /// 这是一个用户辅助类，用于简化需要延迟或异步决策的重试策略实现。
    /// 继承此类并重写 ShouldRetryAsync 方法即可实现自定义的异步重试策略。
    /// </remarks>
    public abstract class AsyncRetryPolicy : IRetryPolicy
    {
        /// <summary>
        /// 确定操作失败后是否应该重试（异步版本）。
        /// </summary>
        /// <param name="retryCount">已进行的重试次数。</param>
        /// <param name="exception">导致失败的异常。</param>
        /// <returns>如果应该重试操作则为 true；否则为 false。</returns>
        protected abstract UniTask<bool> ShouldRetryAsync(int retryCount, Exception exception);

        /// <summary>
        /// 确定操作失败后是否应该重试。
        /// </summary>
        /// <param name="retryCount">已进行的重试次数。</param>
        /// <param name="exception">导致失败的异常。</param>
        /// <param name="onDecision">决策回调，true 表示重试，false 表示不重试。</param>
        /// <param name="onRetryPolicyFailed">当重试策略本身执行失败时调用的回调。</param>
        public void ShouldRetry(
            int retryCount,
            Exception exception,
            Action<bool> onDecision,
            Action<Exception> onRetryPolicyFailed)
        {
            if (onDecision == null)
                throw new ArgumentNullException(nameof(onDecision));
            if (onRetryPolicyFailed == null)
                throw new ArgumentNullException(nameof(onRetryPolicyFailed));

            TryExecuteShouldRetryAsync(retryCount, exception, onDecision, onRetryPolicyFailed).Forget();
        }

        private async UniTaskVoid TryExecuteShouldRetryAsync(
            int retryCount,
            Exception exception,
            Action<bool> onDecision,
            Action<Exception> onRetryPolicyFailed)
        {
            try
            {
                bool shouldRetry = await ShouldRetryAsync(retryCount, exception);
                onDecision(shouldRetry);
            }
            catch (Exception ex)
            {
                onRetryPolicyFailed(ex);
            }
        }
    }
}
