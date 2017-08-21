using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SyncApi.Models
{
    public class SyncInfo
    {
        public IEnumerable<CUDEntity> ClientOperations { get; set; }
        public DateTimeOffset LastSync { get; set; }
    }
}