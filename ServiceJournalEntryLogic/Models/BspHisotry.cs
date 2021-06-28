using RSM.Core.SDK.Attributes;
using RSM.Core.SDK.DI.Models;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryLogic.Models
{
    [Table(Name = "RSM_SERVICE_PARAMS", Description = "Service Entry Settings", Type = BoUTBTableType.bott_NoObjectAutoIncrement)]
    public class BspHisotry : Table
    {
        [Field(Description = "ParentId", Type = BoFieldTypes.db_Alpha, Size = 202)]
        public string BSP_ID_NUMBER { get; set; }
        [Field(Description = "SEQUENCE", Type = BoFieldTypes.db_Alpha, Size = 202)]
        public string BSP_SEQUENCE { get; set; }
        [Field(Description = "ACCOUNT", Type = BoFieldTypes.db_Alpha, Size = 202)]
        public string BSP_ACCOUNT { get; set; }
        [Field(Description = "Transaction Id", Type = BoFieldTypes.db_Alpha, Size = 202)]
        public string TRANS_ID_EMPLOYEE { get; set; }
        [Field(Description = "TRANS_ID_COMPANY Id", Type = BoFieldTypes.db_Alpha, Size = 202)]
        public string TRANS_ID_COMPANY { get; set; }
    }
}
