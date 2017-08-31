using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SyncApi.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using SyncApi.SyncEnums;
using SyncApi.Helpers;

namespace SyncApi.Data
{
    public class DataSyncManager
    {
        private ICUDDataStore CUDStoreService;
        private ICustomDeserialize CustomDeserializeService;
        private DateTimeOffset UpdatedSyncTime;
        private readonly Enums.DataLocation Location;

        public DataSyncManager(ICUDDataStore dataStore, ICustomDeserialize deserializer, Enums.DataLocation location, DateTimeOffset updatedSyncTime)
        {
            CUDStoreService = dataStore;
            CustomDeserializeService = deserializer;
            Location = location;
            UpdatedSyncTime = updatedSyncTime;
        }

        public async Task Sync(IEnumerable<ICUDEntity> operations)
        {
            if (Location == Enums.DataLocation.Server)
            {
                await ServerSync(operations);
            }
            else
            {
                await LocalSync(operations);
            }
        }

        private async Task ServerSync(IEnumerable<ICUDEntity> clientOperations)
        {
            if (clientOperations != null)
            {
                foreach (ICUDEntity operation in clientOperations)
                {
                    IEnumerable<ICUDEntity> serverCUD = await CUDStoreService.GetCUD();
                    ISyncItem deserializedItem = CustomDeserializeService.Deserialize(operation.Payload, operation.TableName);

                    // Get latest CUD with same record id
                    ICUDEntity lastOperation = serverCUD.Where(entity => entity.RecordId == RecordIdHelper.CreateRecordId(deserializedItem)).OrderByDescending(x => x.OperationTime).FirstOrDefault();

                    if (operation.Operation == "Create")
                    {
                        //No previous operation on preference
                        if (lastOperation == null)
                        {
                            await deserializedItem.Create();
                            CUDStoreService.AddCUDEntity(operation.TableName, RecordIdHelper.CreateRecordId(deserializedItem), operation.CustomerId, operation.Payload, operation.Operation, operation.OperationTime);
                        }
                        else if (DateTimeOffset.Compare(lastOperation.OperationTime, operation.OperationTime) < 0)
                        {
                            //client operation is latest and preference doesn't exist
                            if (lastOperation.Operation == "Delete")
                            {
                                await deserializedItem.Create();
                                CUDStoreService.AddCUDEntity(operation.TableName, RecordIdHelper.CreateRecordId(deserializedItem), operation.CustomerId, operation.Payload, operation.Operation, operation.OperationTime);
                            }
                            //client operation is latest and preference exists, so turn to update
                            else
                            {
                                await deserializedItem.Update();
                                CUDStoreService.AddCUDEntity(operation.TableName, RecordIdHelper.CreateRecordId(deserializedItem), operation.CustomerId, operation.Payload, "Update", operation.OperationTime);
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
                            await deserializedItem.Create();
                            CUDStoreService.AddCUDEntity(operation.TableName, RecordIdHelper.CreateRecordId(deserializedItem), operation.CustomerId, operation.Payload, "Create", operation.OperationTime);
                        }
                        else if (DateTimeOffset.Compare(lastOperation.OperationTime, operation.OperationTime) < 0)
                        {
                            await deserializedItem.Update();
                            CUDStoreService.AddCUDEntity(operation.TableName, RecordIdHelper.CreateRecordId(deserializedItem), operation.CustomerId, operation.Payload, "Update", operation.OperationTime);
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
                            await deserializedItem.Delete();
                            CUDStoreService.AddCUDEntity(operation.TableName, RecordIdHelper.CreateRecordId(deserializedItem), operation.CustomerId, operation.Payload, "Delete", operation.OperationTime);
                        }
                        else
                        {
                            //discard
                        }
                    }
                }
            }
        }

        private async Task LocalSync(IEnumerable<ICUDEntity> changesFromServer)
        {
            foreach (ICUDEntity operation in changesFromServer)
            {
                //Get Sync Item stored in server operation
                ISyncItem operationPayload = CustomDeserializeService.Deserialize(operation.Payload, operation.TableName);
                ISyncItem localQueriedItem = await operationPayload.FindLatestItemCopy();

                if (operation.Operation == "Create")
                {
                    if (localQueriedItem == null)
                    {
                        await operationPayload.Create();
                    }
                    if (localQueriedItem != null && !localQueriedItem.IsEqual(operationPayload))
                    {
                        //find same local item and update values
                        await operationPayload.Update();
                    }
                }
                else if (operation.Operation == "Update")
                {
                    if (localQueriedItem == null)
                    {
                        await operationPayload.Create();
                    }
                    if (localQueriedItem != null && !localQueriedItem.IsEqual(operationPayload))
                    {
                        await operationPayload.Update();
                    }
                }
                else
                {
                    if (localQueriedItem != null)
                    {
                        await operationPayload.Delete();
                    }
                }
            }

            //clear CUD and Set sync time
            CUDStoreService.CleanUp(UpdatedSyncTime);
        }
    }
}