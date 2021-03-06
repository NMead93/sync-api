﻿using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SyncApi.Data;
using Microsoft.WindowsAzure.Storage;
using System.Net.Http;
using System.Net;
using System.Web.Http;
using System.Threading.Tasks;

namespace SyncApi.Models
{
    public class UserPreferenceEntity : TableEntity, ISyncItem
    {
        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=nicksandbox;AccountKey=NxgXZJdg7NUg8GC4XLXfYAd2d7iYklzOO2jZEIl/PseWumvfXBwtYsYc461SOiz8JQyGYESl3q4+DiqecBETiw==;EndpointSuffix=core.windows.net";

        public string TableName { get { return PartitionKey; } set { PartitionKey = value; } }

        public string CustomerId { get; set; }

        public string PreferenceName { get { return RowKey; } set { RowKey = value; } }

        public string PreferenceValue { get; set; }

        public UserPreferenceEntity() { }

        public UserPreferenceEntity(string customerId, string preferenceName, string preferenceValue)
        {
            TableName = "UserPreference";
            CustomerId = customerId;
            PreferenceName = preferenceName;
            PreferenceValue = preferenceValue;
        }

        public async Task Create()
        {
            
            TableOperation insertOperation = TableOperation.Insert(this);

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("UserPreference");

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
            List<UserPreferenceEntity> preferenceRecords = new List<UserPreferenceEntity>();
            TableQuery<UserPreferenceEntity> query = new TableQuery<UserPreferenceEntity>();

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("UserPreference");

            try
            {
                foreach (UserPreferenceEntity customer in table.ExecuteQuery(query))
                {
                    preferenceRecords.Add(customer);
                }

                return preferenceRecords;
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
            CloudTable table = tableClient.GetTableReference("UserPreference");

            TableOperation retrieveOperation = TableOperation.Retrieve<UserPreferenceEntity>(TableName, PreferenceName);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            UserPreferenceEntity retrievedPreference = (UserPreferenceEntity)retrievedResult.Result;

            if (retrievedPreference != null)
            {
                retrievedPreference.PreferenceValue = PreferenceValue;
                TableOperation updateOperation = TableOperation.Replace(retrievedPreference);
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
            CloudTable table = tableClient.GetTableReference("UserPreference");

            TableOperation retrieveOperation = TableOperation.Retrieve<UserPreferenceEntity>(TableName, PreferenceName);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            UserPreferenceEntity retrievedPreference = (UserPreferenceEntity)retrievedResult.Result;

            if (retrievedPreference != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(retrievedPreference);
                await table.ExecuteAsync(deleteOperation);
            }
            else
            {
                throw new Exception("Preference could not be retrieved");
            }
        }

        public bool IsEqual(ISyncItem otherItem)
        {
            UserPreferenceEntity castedOtherItem = (UserPreferenceEntity)otherItem;
            return (PreferenceName == castedOtherItem.PreferenceName) && (PreferenceValue == castedOtherItem.PreferenceValue);
        }

        public Task<ISyncItem> FindLatestItemCopy()
        {
            throw new NotImplementedException();
        }
    }
}