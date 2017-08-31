using Newtonsoft.Json;
using SyncApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyncApi.Helpers
{
    public class ServerCustomDeserialize : ICustomDeserialize
    {
        public ISyncItem Deserialize(string json, string tableName)
        {
            if (tableName == "UserPreference")
            {
                return JsonConvert.DeserializeObject<UserPreferenceEntity>(json);
            }
            else if (tableName == "UserArticleStatus")
            {
                return JsonConvert.DeserializeObject<UserArticleStatusEntity>(json);
            }
            else
            {
                throw new Exception(String.Format("Not a valid table name: {0}", tableName));
            }
        }

        public ServerCustomDeserialize() { }
    }
}