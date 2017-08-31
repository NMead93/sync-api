using Microsoft.WindowsAzure.Storage.Table;
using SyncApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SyncApi.SyncEnums;
using Microsoft.WindowsAzure.Storage;
using System.Net.Http;
using System.Net;
using System.Web.Http;
using System.Threading.Tasks;

namespace SyncApi.Models
{
    public class UserArticleStatusEntity : TableEntity, ISyncItem
    {
        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=nicksandbox;AccountKey=NxgXZJdg7NUg8GC4XLXfYAd2d7iYklzOO2jZEIl/PseWumvfXBwtYsYc461SOiz8JQyGYESl3q4+DiqecBETiw==;EndpointSuffix=core.windows.net";

        public string TableName { get { return PartitionKey; } set { PartitionKey = value; } }

        public string CustomerId { get; set; }

        public string ArticleCode { get { return RowKey; } set { RowKey = value; } }

        public Enums.ArticleStatus ArticleStatus { get; set; }

        public UserArticleStatusEntity() { }

        public UserArticleStatusEntity(string userId, string articleCode, Enums.ArticleStatus articleStatus)
        {
            TableName = "UserArticleStatus";
            CustomerId = userId;
            ArticleCode = articleCode;
            ArticleStatus = articleStatus;
        }

        public async Task Create()
        {
            TableOperation insertOperation = TableOperation.Insert(this);

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("UserArticleStatus");

            try
            {
                await table.ExecuteAsync(insertOperation);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<IEnumerable<ISyncItem>> GetAll()
        {
            List<UserArticleStatusEntity> userArticleStatusRecords = new List<UserArticleStatusEntity>();
            TableQuery<UserArticleStatusEntity> query = new TableQuery<UserArticleStatusEntity>();

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("UserArticleStatus");

            try
            {
                foreach (UserArticleStatusEntity userArticle in table.ExecuteQuery(query))
                {
                    userArticleStatusRecords.Add(userArticle);
                }

                return userArticleStatusRecords;
            }
            catch (Exception ex)
            {
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                message.Content = new StringContent(ex.Message);
                throw new HttpResponseException(message);
            }
        }

        public async Task Update()
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("UserArticleStatus");

            TableOperation retrieveOperation = TableOperation.Retrieve<UserArticleStatusEntity>(TableName, ArticleCode);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            UserArticleStatusEntity retrievedArticleStatus = (UserArticleStatusEntity)retrievedResult.Result;

            if (retrievedArticleStatus != null)
            {
                retrievedArticleStatus.ArticleStatus = ArticleStatus;
                TableOperation updateOperation = TableOperation.Replace(retrievedArticleStatus);
                await table.ExecuteAsync(updateOperation);
            }
            else
            {
                throw new Exception("Preference could not be retrieved");
            }
        }

        public async Task Delete()
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("UserArticleStatus");

            TableOperation retrieveOperation = TableOperation.Retrieve<UserArticleStatusEntity>(TableName, ArticleCode);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            UserArticleStatusEntity retrievedArticleStatus = (UserArticleStatusEntity)retrievedResult.Result;

            if (retrievedArticleStatus != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(retrievedArticleStatus);
                await table.ExecuteAsync(deleteOperation);
            }
            else
            {
                throw new Exception("Preference could not be retrieved");
            }
        }

        public bool IsEqual(ISyncItem otherItem)
        {
            UserArticleStatusEntity castedOtherItem = (UserArticleStatusEntity)otherItem;
            return (ArticleCode == castedOtherItem.ArticleCode) && (ArticleStatus == castedOtherItem.ArticleStatus);
        }

        public Task<ISyncItem> FindLatestItemCopy()
        {
            throw new NotImplementedException();
        }
    }
}