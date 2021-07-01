using RSM.Core.SDK.DI.Extension;
using SAPbobsCOM;
using ServiceJournalEntryLogic.Models;

namespace ServiceJournalEntryLogic.Providers
{
    public class SettingsProvider : UDTProvider
    {
        public SettingsProvider(Company oCompany) : base(oCompany)
        {
        }

        public Settings Get()
        {
            var rs = GetRecordSet();
            rs.DoQuery2("Select * From [@RSM_SERVICE_PARAMS]");

            return new Settings()
            {
                Code = (int)rs.Fields.Item("Code").Value,
                Name = (string)rs.Fields.Item("Name").Value,
                IncomeControlTaxAccCr = (string)rs.Fields.Item("U_IncomeControlTaxAccCr").Value,
                IncomeControlTaxAccDr = (string)rs.Fields.Item("U_IncomeControlTaxAccDr").Value,
                IncomeTaxAccCr = (string)rs.Fields.Item("U_IncomeTaxAccCr").Value,
                IncomeTaxAccDr = (string)rs.Fields.Item("U_IncomeTaxAccDr").Value,
                IncomeTaxOnInvoice = (string)rs.Fields.Item("U_IncomeTaxOnInvoice").Value == "True",
                PensionAccCr = (string)rs.Fields.Item("U_PensionAccCr").Value,
                PensionAccDr = (string)rs.Fields.Item("U_PensionAccDr").Value,
                PensionControlAccCr = (string)rs.Fields.Item("U_PensionControlAccCr").Value,
                PensionControlAccDr = (string)rs.Fields.Item("U_PensionControlAccDr").Value,
                UseDocControllAcc = (string)rs.Fields.Item("U_UseDocControllAcc").Value == "True",
            };
        }
    }
}
