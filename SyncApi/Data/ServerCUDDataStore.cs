using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SyncApi.Models;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SyncApi.Data
{
    public class ServerCUDDataStore : ICUDDataStore
    {
        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=nicksandbox;AccountKey=NxgXZJdg7NUg8GC4XLXfYAd2d7iYklzOO2jZEIl/PseWumvfXBwtYsYc461SOiz8JQyGYESl3q4+DiqecBETiw==;EndpointSuffix=core.windows.net";

        public async Task<IEnumerable<CUDEntity>> CUDSinceSync(DateTimeOffset lastSync)
        {
            IEnumerable<ICUDEntity> serverCUDBeforeConversion =  await GetCUD();
            IEnumerable<CUDEntity> serverCUD = (IEnumerable<CUDEntity>)serverCUDBeforeConversion;

            //Get all CUD operations merged in after last sync
            return serverCUD.Where(x => x.Timestamp > lastSync);
        }

        public void AddCUDEntity(string tableName, string recordId, string customerId, string payload, string operation, DateTimeOffset operationDate)
        {
            CUDEntity newCUDEntity = new CUDEntity(tableName, recordId, customerId, payload, operation, operationDate);
            TableOperation insertOperation = TableOperation.Insert(newCUDEntity);

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("CUD");

            try
            {
                table.Execute(insertOperation);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<ICUDEntity>> GetCUD()
        {
            //List<CUDEntity> serverCUD = new List<CUDEntity>();
            //TableQuery<CUDEntity> query = new TableQuery<CUDEntity>();

            //var account = CloudStorageAccount.Parse(connectionString);
            //var tableClient = account.CreateCloudTableClient();
            //CloudTable table = tableClient.GetTableReference("CUD");

            //try
            //{
            //    foreach (CUDEntity operation in table.ExecuteQuery(query))
            //    {
            //        serverCUD.Add(operation);
            //    }

            //    return serverCUD;
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);

            //    return new List<CUDEntity>();
            //}

            List<CUDEntity> serverCUD = new List<CUDEntity>();
            TableQuery<CUDEntity> query = new TableQuery<CUDEntity>();
            TableContinuationToken continuationToken = null;

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("CUD");

            do
            {
                // Execute the query async until there is no more result
                TableQuerySegment<CUDEntity> tableQueryResult = await table.ExecuteQuerySegmentedAsync(query, continuationToken);

                continuationToken = tableQueryResult.ContinuationToken;
                serverCUD.AddRange(tableQueryResult);
            } while (continuationToken != null);

            return serverCUD;
        }

        public void CleanUp(DateTimeOffset updatedSyncTime) { }
    }
}