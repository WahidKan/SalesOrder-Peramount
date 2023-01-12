using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesOrder_Paramount.Models.Setting
{
    public class Setting
    {
        public string DbConnection { get; set; }
        public string Server { get; set; }
        public string CompanyDB { get; set; }
        public string DbUserName { get; set; }
        public string DbPassword { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LicenseServer { get; set; }
        public bool UseTrusted { get; set; }
    }
}
