using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLIoT.DataObjects
{
    public class UserTable : BindableBase , IUserTable, IAzureTableData
    {
        public string Id { get; set; }
        [JsonProperty(PropertyName = "username")]
        public string username { get; set; }
    }
}
