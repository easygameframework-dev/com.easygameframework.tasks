using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Core.Event;
using EasyGameFramework.Core.Resource;
using EasyGameFramework;
using EasyGameFramework.Essentials;

namespace EasyGameFramework.Tasks
{
    public static class DataTableExtensions
    {
        private static readonly Dictionary<AssetAddress, UniTaskCompletionSource> DataTableLoadCompletedTcsByAssetAddress =
            new Dictionary<AssetAddress, UniTaskCompletionSource>();

        static DataTableExtensions()
        {
            var eventComponent = GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe<LoadDataTableSuccessEventArgs>(OnEvent);
            eventComponent.Subscribe<LoadDataTableFailureEventArgs>(OnEvent);
        }

        public static UniTask LoadDataTableAsync(
            this DataTableComponent dataTableComponent,
            Type dataRowType,
            AssetAddress assetAddress,
            string dataTableName = "",
            int? customPriority = null)
        {
            if (DataTableLoadCompletedTcsByAssetAddress.TryGetValue(assetAddress, out var tcs))
            {
                return tcs.Task;
            }

            tcs = new UniTaskCompletionSource();
            DataTableLoadCompletedTcsByAssetAddress[assetAddress] = tcs;

            var table = string.IsNullOrEmpty(dataTableName)
                ? dataTableComponent.CreateDataTable(dataRowType)
                : dataTableComponent.CreateDataTable(dataRowType, dataTableName);

            table.ReadData(assetAddress, customPriority ?? Constant.AssetPriority.DataTableAsset);

            return tcs.Task;
        }

        private static void OnEvent(object sender, LoadDataTableSuccessEventArgs e)
        {
            if (DataTableLoadCompletedTcsByAssetAddress.Remove(e.DataTableAssetAddress, out var tcs))
            {
                tcs.TrySetResult();
            }
        }

        private static void OnEvent(object sender, LoadDataTableFailureEventArgs e)
        {
            if (DataTableLoadCompletedTcsByAssetAddress.Remove(e.DataTableAssetAddress, out var tcs))
            {
                tcs.TrySetException(new Exception(e.ErrorMessage));
            }
        }
    }
}
