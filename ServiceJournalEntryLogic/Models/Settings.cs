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
    public class Settings : Table
    {

        [Field(Description = "საპენსიოს ანგარიში", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public string PensionAccDr { get; set; }
        [Field(Description = "საპენსიოს ანგარიში", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public string PensionAccCr { get; set; }
        [Field(Description = "საპენსიოს ანგარიში", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public string PensionControlAccDr { get; set; }
        [Field(Description = "საპენსიოს ანგარიში", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public string PensionControlAccCr { get; set; }
        [Field(Description = "საშემოსავლოს ანგარიში", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public string IncomeTaxAccDr { get; set; }
        [Field(Description = "საშემოსავლოს ანგარიში", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public string IncomeTaxAccCr { get; set; }
        [Field(Description = "საშემოსავლოს ანგარიში", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public string IncomeControlTaxAccDr { get; set; }
        [Field(Description = "საშემოსავლოს ანგარიში", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public string IncomeControlTaxAccCr { get; set; }
        [Field(Description = "საშემოსავლო ინვოისზე", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public bool IncomeTaxOnInvoice { get; set; }
        [Field(Description = "გამოიყენე დოკუმენტის ანგარიში", Type = BoFieldTypes.db_Alpha, Size = 20)]
        public bool UseDocControllAcc { get; set; }

        //diManager.AddField("RSM_SERVICE_PARAMS", "PensionAccDr", "საპენსიოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
        //diManager.AddField("RSM_SERVICE_PARAMS", "PensionAccCr", "საპენსიოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
        //diManager.AddField("RSM_SERVICE_PARAMS", "PensionControlAccDr", "საპენსიოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
        //diManager.AddField("RSM_SERVICE_PARAMS", "PensionControlAccCr", "საპენსიოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
        //diManager.AddField("RSM_SERVICE_PARAMS", "IncomeTaxAccDr", "საშემოსავლოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
        //diManager.AddField("RSM_SERVICE_PARAMS", "IncomeTaxAccCr", "საშემოსავლოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
        //diManager.AddField("RSM_SERVICE_PARAMS", "IncomeControlTaxAccDr", "საშემოსავლოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
        //diManager.AddField("RSM_SERVICE_PARAMS", "IncomeControlTaxAccCr", "საშემოსავლოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
        //diManager.AddField("RSM_SERVICE_PARAMS", "IncomeTaxOnInvoice", "საშემოსავლო ინვოისზე", BoFieldTypes.db_Alpha, 20, false);
    }
}
