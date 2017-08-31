using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncApi.Models
{
    public interface ICUDEntity
    {
        string Operation { get; set; }
        string CustomerId { get; set; }
        string RecordId { get; set; }
        string TableName { get; set; }
        string Payload { get; set; }
        DateTimeOffset OperationTime { get; set; }
    }
}
