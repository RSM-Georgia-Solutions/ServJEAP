using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace ServiceJournalEntryAp.Initialization
{
    class CreateFields : IRunnable
    {
        public void Run(DiManager diManager)
        {

            Dictionary<string, string> validValues = new Dictionary<string, string>()
            {
                {"01", "კი"},
                {"02", "არა"}
            };

            diManager.AddField("OCRD", "IncomeTaxPayer", "საშემოსავლოს გადამხდელი", BoFieldTypes.db_Alpha, 250, validValues, true,true);
            diManager.AddField("OCRD", "PensionPayer", "საპენსიოს გადამხდელი", BoFieldTypes.db_Alpha, 250, validValues, true, true);

            diManager.AddField("OCRD", "IncomeTaxPayerPercent", "საშემოსავლოს %", BoFieldTypes.db_Float, 33, false,
                true);
            diManager.AddField("OCRD", "PensionPayerPercent", "საპენსიოს %", BoFieldTypes.db_Float, 33, false,
                true);

            diManager.AddField("RSM_SERVICE_PARAMS", "PensionAccDr", "საპენსიოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
            diManager.AddField("RSM_SERVICE_PARAMS", "PensionAccCr", "საპენსიოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
            diManager.AddField("RSM_SERVICE_PARAMS", "PensionControlAccDr", "საპენსიოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
            diManager.AddField("RSM_SERVICE_PARAMS", "PensionControlAccCr", "საპენსიოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
            diManager.AddField("RSM_SERVICE_PARAMS", "IncomeTaxAccDr", "საშემოსავლოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
            diManager.AddField("RSM_SERVICE_PARAMS", "IncomeTaxAccCr", "საშემოსავლოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
            diManager.AddField("RSM_SERVICE_PARAMS", "IncomeControlTaxAccDr", "საშემოსავლოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
            diManager.AddField("RSM_SERVICE_PARAMS", "IncomeControlTaxAccCr", "საშემოსავლოს ანგარიში", BoFieldTypes.db_Alpha, 20, false);
        }
    }
}
