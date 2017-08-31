using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SyncApi.Data;

namespace SyncApi.Models
{
    public class CUDEntity : TableEntity, ICUDEntity
    {
        public string TableName { get { return PartitionKey; } set { PartitionKey = value; } }
        public string RecordId { get; set; }
        public string CustomerId { get; set; }
        public string Payload { get; set; }
        public string Operation { get; set; }
        public DateTimeOffset OperationTime { get; set; }

        public CUDEntity() { }

        public CUDEntity (string tableName, string recordId, string customerId, string payload, string operation, DateTimeOffset operationTime)
        {
            TableName = tableName;
            RecordId = recordId;
            CustomerId = customerId;
            Payload = payload;
            Operation = operation;
            OperationTime = operationTime;
            RowKey = Guid.NewGuid().ToString();
        }
    }
}