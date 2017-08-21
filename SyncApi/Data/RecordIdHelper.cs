using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using SyncApi.Models;

namespace SyncApi.Data
{
    public class RecordIdHelper
    {
        public static string CreateRecordId(string payload)
        {
            UserPreferenceEntity deserializedPayload = JsonConvert.DeserializeObject<UserPreferenceEntity>(payload);

            return deserializedPayload.CustomerId + "_" + deserializedPayload.PreferenceName;
        }
    }
}