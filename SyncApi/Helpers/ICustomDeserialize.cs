using SyncApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncApi.Helpers
{
    public interface ICustomDeserialize
    {
        ISyncItem Deserialize(string json, string tableName);
    }
}
