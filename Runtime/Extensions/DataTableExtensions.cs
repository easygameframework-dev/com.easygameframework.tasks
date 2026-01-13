using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EasyGameFramework.Core.Event;

namespace EasyGameFramework.Tasks
{
    public static class DataTableExtensions
    {
        private static readonly Dictionary<string, UniTaskCompletionSource> DataTableLoadCompletedTcsByAssetName =
            new Dictionary<string, UniTaskCompletionSource>();

        static DataTableExtensions()
        {
            var eventComponent = GameEntry.GetComponent<EventComponent>();
            eventComponent.Subscribe<LoadDataTableSuccessEventArgs>(OnEvent);
            eventComponent.Subscribe<LoadDataTableFailureEventArgs>(OnEvent);
        }

        public static UniTask LoadDataTableAsync(
            this DataTableComponent dataTableComponent,
            Type dataRowType,
            string assetName,
            string dataTableName = "",
            string customPackageName = "",
            int? customPriority = null)
        {
            //TODO key结合packageName
            if (DataTableLoadCompletedTcsByAssetName.TryGetValue(assetName, out var tcs))
            {
                return tcs.Task;
            }

            tcs = new UniTaskCompletionSource();
            DataTableLoadCompletedTcsByAssetName[assetName] = tcs;

            var table = string.IsNullOrEmpty(dataTableName)
                ? dataTableComponent.CreateDataTable(dataRowType)
                : dataTableComponent.CreateDataTable(dataRowType, dataTableName);

            if (!string.IsNullOrEmpty(customPackageName))
            {
                var resourcesComponent = GameEntry.GetComponent<ResourceComponent>();
                resourcesComponent.CurrentPackageName = customPackageName;
            }

            table.ReadData(assetName, customPriority ?? 0);

            return tcs.Task;
        }

        private static void OnEvent(object sender, LoadDataTableSuccessEventArgs e)
        {
            if (DataTableLoadCompletedTcsByAssetName.Remove(e.DataTableAssetName, out var tcs))
            {
                tcs.TrySetResult();
            }
        }

        private static void OnEvent(object sender, LoadDataTableFailureEventArgs e)
        {
            if (DataTableLoadCompletedTcsByAssetName.Remove(e.DataTableAssetName, out var tcs))
            {
                tcs.TrySetException(new Exception(e.ErrorMessage));
            }
        }
    }
}
