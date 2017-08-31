using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncApi.Models
{
    public interface ISyncItem
    {
        string TableName { get; set; }

        string CustomerId { get; set; }

        Task Create();

        Task<IEnumerable<ISyncItem>> GetAll();

        Task Update();

        Task Delete();

        bool IsEqual(ISyncItem otherItem);

        Task<ISyncItem> FindLatestItemCopy();
    }
}
