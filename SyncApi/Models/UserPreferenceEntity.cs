using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyncApi.Models
{
    public class UserPreferenceEntity : TableEntity
    {
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

    }
}