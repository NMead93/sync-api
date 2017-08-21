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
    public class AzureTableService
    {
        private static string connectionString = "DefaultEndpointsProtocol=https;AccountName=nicksandbox;AccountKey=NxgXZJdg7NUg8GC4XLXfYAd2d7iYklzOO2jZEIl/PseWumvfXBwtYsYc461SOiz8JQyGYESl3q4+DiqecBETiw==;EndpointSuffix=core.windows.net";

        public static IEnumerable<UserPreferenceEntity> GetPreferences()
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


        public static void AddPreference(UserPreferenceEntity preference)
        {
            TableOperation insertOperation = TableOperation.Insert(preference);

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("UserPreference");

            try
            {
                table.Execute(insertOperation);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static void UpdatePreference(UserPreferenceEntity updatedPreference)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("UserPreference");
            
            TableOperation retrieveOperation = TableOperation.Retrieve<UserPreferenceEntity>(updatedPreference.TableName, updatedPreference.PreferenceName);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            UserPreferenceEntity retrievedPreference = (UserPreferenceEntity)retrievedResult.Result;

            if (retrievedPreference != null)
            {
                retrievedPreference.PreferenceValue = updatedPreference.PreferenceValue;
                TableOperation updateOperation = TableOperation.Replace(retrievedPreference);
                table.Execute(updateOperation);

                
            }
            else
            {
                throw new Exception("Preference could not be retrieved");
            }
            
        }

        public static void DeletePreference(UserPreferenceEntity deletePreference)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("UserPreference");

            TableOperation retrieveOperation = TableOperation.Retrieve<UserPreferenceEntity>(deletePreference.TableName, deletePreference.PreferenceName);
            TableResult retrievedResult = table.Execute(retrieveOperation);
            UserPreferenceEntity retrievedPreference = (UserPreferenceEntity)retrievedResult.Result;

            if (retrievedPreference != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(retrievedPreference);
                table.Execute(deleteOperation);
            }
            else
            {
                throw new Exception("Preference could not be retrieved");
            }
        }

        public static IEnumerable<CUDEntity> UpdateServerStorage(IEnumerable<CUDEntity> clientOperations, DateTimeOffset lastSync)
        {
            if (clientOperations != null)
            {
                foreach(CUDEntity operation in clientOperations)
                {
                    IEnumerable<CUDEntity> serverCUD = GetCUD();
                    UserPreferenceEntity preferencePayload = JsonConvert.DeserializeObject<UserPreferenceEntity>(operation.Payload);

                    // Get latest CUD with same record id
                    CUDEntity lastOperation = serverCUD.Where(entity => entity.RecordId == operation.CustomerId + "_" + preferencePayload.PreferenceName).OrderByDescending(x => x.OperationTime).FirstOrDefault();

                    if (operation.Operation == "Create")
                    {
                    
                        //No previous operation on preference
                        if (lastOperation == null)
                        {
                            AddPreference(preferencePayload);
                            InsertCUDEntity(operation.TableName, operation.CustomerId + "_" + preferencePayload.PreferenceName, operation.CustomerId, operation.Payload, operation.Operation, operation.OperationTime);
                        }
                        else if (DateTimeOffset.Compare(lastOperation.OperationTime, operation.OperationTime) < 0)
                        {
                            //client operation is latest and preference doesn't exist
                            if (lastOperation.Operation == "Delete")
                            {
                                AddPreference(preferencePayload);
                                InsertCUDEntity(operation.TableName, operation.CustomerId + "_" + preferencePayload.PreferenceName, operation.CustomerId, operation.Payload, operation.Operation, operation.OperationTime);
                            }
                            //client operation is latest and preference exists, so turn to update
                            else
                            {
                                UpdatePreference(preferencePayload);
                                InsertCUDEntity(operation.TableName, operation.CustomerId + "_" + preferencePayload.PreferenceName, operation.CustomerId, operation.Payload, "Update", operation.OperationTime);
                            }
                        }
                        else
                        {
                            //Discard change
                        }
                    }
                    else if (operation.Operation == "Update")
                    {
                        if (lastOperation == null)
                        {
                            throw new Exception("Error Occurred. There should be a previous operation when we start calculating a client update operation");
                        }
                        //client operation is the latest, but preference doesn't exist anymore on server
                        else if (DateTimeOffset.Compare(lastOperation.OperationTime, operation.OperationTime) < 0 && lastOperation.Operation == "Delete")
                        {
                            AddPreference(preferencePayload);
                            InsertCUDEntity(operation.TableName, operation.CustomerId + "_" + preferencePayload.PreferenceName, operation.CustomerId, operation.Payload, "Create", operation.OperationTime);
                        }
                        else if (DateTimeOffset.Compare(lastOperation.OperationTime, operation.OperationTime) < 0)
                        {
                            UpdatePreference(preferencePayload);
                            InsertCUDEntity(operation.TableName, operation.CustomerId + "_" + preferencePayload.PreferenceName, operation.CustomerId, operation.Payload, "Update", operation.OperationTime);
                        }
                        else
                        {
                            //discard
                        }
                    }
                    else
                    {
                        if (lastOperation == null)
                        {
                            throw new Exception("Error Occurred. There should be a previous operation when we start calculating a client delete operation");
                        }
                        //last operation before client operation was create or update
                        else if (DateTimeOffset.Compare(lastOperation.OperationTime, operation.OperationTime) < 0 && (lastOperation.Operation == "Create" || lastOperation.Operation == "Update"))
                        {
                            DeletePreference(preferencePayload);
                            InsertCUDEntity(operation.TableName, operation.CustomerId + "_" + preferencePayload.PreferenceName, operation.CustomerId, operation.Payload, "Delete", operation.OperationTime);

                        }
                        else
                        {
                            //discard
                        }
                    }
                }
            }

            return CUDSinceSync(lastSync);
        }

        public static IEnumerable<CUDEntity> CUDSinceSync(DateTimeOffset lastSync)
        {
            IEnumerable<CUDEntity> serverCUD = GetCUD();
            //Get all CUD operations merged in after last sync
            return serverCUD.Where(x => x.Timestamp > lastSync);
        }

        public static void InsertCUDEntity(string tableName, string recordId, string customerId, string payload, string operation, DateTimeOffset operationDate)
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

        private static IEnumerable<CUDEntity> GetCUD()
        {
            List<CUDEntity> serverCUD = new List<CUDEntity>();
            TableQuery<CUDEntity> query = new TableQuery<CUDEntity>();

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("CUD");

            try
            {
                foreach (CUDEntity operation in table.ExecuteQuery(query))
                {
                    serverCUD.Add(operation);
                }

                return serverCUD;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return new List<CUDEntity>();
            }
        }
    }
}