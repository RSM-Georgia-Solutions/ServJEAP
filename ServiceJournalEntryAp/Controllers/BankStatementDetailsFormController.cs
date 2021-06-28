using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;
using SAPbouiCOM;
using ServiceJournalEntryAp.Extensions;
using System.Globalization;

namespace ServiceJournalEntryAp.Controllers
{
    public class BankStatementDetailsFormController : FormController
    {
        public BankStatementDetailsFormController(SAPbobsCOM.Company Company, IForm Form) : base(Company, Form)
        {
        }

        public void PostPension()
        {
            bool incomeTaxOnInvoice = false;
            int successCount = 0;
            int hasNoJounralEntryCount = 0;
            int errorCount = 0;
            int totalCount = 0;
            int addedAlready = 0;
            int hasNoBp = 0;
            int notPayer = 0;
            int EmptyAmount = 0;

            var accountHeader = ((EditText)oForm.Items.Item("10000013").Specific).Value;
            var bsNumber = ((EditText)oForm.Items.Item("10000022").Specific).Value;
            string idNumber = oForm.DataSources.DBDataSources.Item("OBNH").GetValue("IdNumber", 0);

            Recordset recSetSeries = RSM.Core.SDK.DI.DIApplication.GetRecordset();
            recSetSeries.DoQuery2($"SELECT OutSeri FROM DSC1 WHERE Account = N'{accountHeader}'");

            var series = int.Parse(recSetSeries.Fields.Item("OutSeri").Value.ToString());
            var matrix = (Matrix)oForm.Items.Item("10000036").Specific;

            for (int i = 1; i <= matrix.RowCount; i++)
            {
                if (((ComboBox)matrix.GetCellSpecific("10000037", i)).Selected == null)
                {
                    continue;
                }

                string cardCode = oForm.DataSources.DBDataSources.Item(0).GetValue("CardCode", i - 1);
                string bplIdString = oForm.DataSources.DBDataSources.Item(0).GetValue("BPLIdPmn", i - 1);
                string sequence = oForm.DataSources.DBDataSources.Item("OBNK").GetValue("Sequence", i - 1);
                string order = oForm.DataSources.DBDataSources.Item("OBNK").GetValue("VisOrder", i - 1);

                int journalEntryTransId;
                try
                {
                    journalEntryTransId = int.Parse(oForm.DataSources.DBDataSources.Item(0).GetValue("JDTID", i - 1), CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    totalCount += 2;
                    hasNoJounralEntryCount += 2;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(bplIdString))
                {
                    bplIdString = "235";
                }
                int bplId = int.Parse(bplIdString);

                if (string.IsNullOrWhiteSpace(cardCode))
                {
                    totalCount += 2;
                    hasNoBp += 2;
                    continue;
                }


                JournalEntries journalEntry = (SAPbobsCOM.JournalEntries)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);
                journalEntry.GetByKey(journalEntryTransId);
                bool hasVendor = true;

                for (int j = 0; j < journalEntry.Lines.Count; j++)
                {
                    journalEntry.Lines.SetCurrentLine(j);
                    if (journalEntry.Lines.ShortName == cardCode)
                    {
                        hasVendor = true;
                        break;
                    }
                    hasVendor = false;
                }

                if (!hasVendor)
                {
                    totalCount += 2;
                    hasNoJounralEntryCount += 2;
                    continue;
                }

                Recordset recSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                string bpCode = cardCode;
                recSet.DoQuery2($"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'");
                bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
                bool incomeTaxPayer = recSet.Fields.Item("U_IncomeTaxPayer").Value.ToString() == "01";
                recSet.DoQuery($"Select * From [@RSM_SERVICE_PARAMS]");
                string pensionAccDr = recSet.Fields.Item("U_PensionAccDr").Value.ToString();
                string pensionAccCr = recSet.Fields.Item("U_PensionAccCr").Value.ToString();
                string pensionControlAccDr = recSet.Fields.Item("U_PensionControlAccDr").Value.ToString();
                string pensionControlAccCr = recSet.Fields.Item("U_PensionControlAccCr").Value.ToString();
                incomeTaxOnInvoice = Convert.ToBoolean(recSet.Fields.Item("U_IncomeTaxOnInvoice").Value.ToString());


                double amount = 0;
                var postingDate = DateTime.ParseExact(((EditText)matrix.GetCellSpecific("10000003", i)).Value, "yyyyMMdd", CultureInfo.InvariantCulture);

                var amountCurrencyString = ((EditText)matrix.GetCellSpecific("10000045", i)).Value;

                if (string.IsNullOrWhiteSpace(amountCurrencyString))
                {
                    EmptyAmount += 2;
                    totalCount += 2;
                    continue;
                }

                var amountString = amountCurrencyString.Split(' ')[0];
                var curryencyString = amountCurrencyString.Split(' ')[1];
                try
                {
                    amount = double.Parse(amountString, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage("Invalid Amount : \"Outgoing Amt - Payment Currency\"",
                        BoMessageTime.bmt_Short, true);
                }
                double pensionAmountPaymentOnAccount = Math.Round(amount / 0.784 * 0.02, 6);
                if (!incomeTaxPayer)
                {
                    pensionAmountPaymentOnAccount = Math.Round(amount / 0.98 * 0.02, 6);
                }
                Recordset recSet2 =
                    (Recordset)oCompany.GetBusinessObject(BoObjectTypes
                        .BoRecordset);
                Recordset recSet3 =
                    (Recordset)oCompany.GetBusinessObject(BoObjectTypes
                        .BoRecordset);
                recSet2.DoQuery($"select * from [@RSM_BSP_HISTORY] WHERE U_BSP_ID_NUMBER = {idNumber} AND U_BSP_SEQUENCE = {sequence} AND U_BSP_ACCOUNT = N'{accountHeader}'");

                if (!recSet2.EoF)
                {
                    addedAlready += 2;
                    totalCount += 2;
                    continue;
                }
                if (!incomeTaxOnInvoice)
                {
                    PostIncomeTaxFromBankStatement(cardCode, amount, series, "BP " + bsNumber + " " + order, postingDate, bplId, curryencyString);
                }

                if (!isPensionPayer)
                {
                    notPayer += 2;
                    totalCount += 2;
                    continue;
                }

                string query = $@"INSERT INTO [dbo].[@RSM_BSP_HISTORY]
                        (
                        [U_BSP_ID_NUMBER],
                        [U_BSP_SEQUENCE],
                        [U_BSP_ACCOUNT],
                        [U_TRANS_ID_EMPLOYEE],
                        [U_TRANS_ID_COMPANY])
                    VALUES
                        ( '{idNumber}',
                          '{sequence}', 
                          '{accountHeader}',";

                try
                {
                    string transId = DocumentHelper.AddJournalEntry(oCompany,
                        pensionAccCr, pensionAccDr, pensionControlAccCr, pensionControlAccDr, pensionAmountPaymentOnAccount,
                        series, "BS " + bsNumber + " " + order, postingDate,
                        bplId, curryencyString);
                    query += $"'{transId}'";
                    successCount++;
                    totalCount++;

                }
                catch (Exception e)
                {
                    SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                    errorCount++;
                }

                try
                {
                    string transId = DocumentHelper.AddJournalEntry(oCompany, pensionAccCr,
                        "", pensionControlAccCr, cardCode, pensionAmountPaymentOnAccount, series,
                        "BP " + bsNumber + " " + order, postingDate, bplId,
                        curryencyString);
                    query += $", '{transId}')";
                    successCount++;
                    totalCount++;

                }
                catch (Exception e)
                {
                    SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                    errorCount++;
                }

                recSet3.DoQuery2(query);
            }

            SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(
                $"წარმატებული : {successCount}  {Environment.NewLine}  უკვე გაგატარებული : {addedAlready}  {Environment.NewLine} არ აქვს საჟურნალო გატარება : {hasNoJounralEntryCount} {Environment.NewLine} არ აქვს ბიზნეს პარტნიორი : {hasNoBp} {Environment.NewLine} არ არის გადამხდელი : {notPayer}   { Environment.NewLine} თანხა არ არის მითითებული : {EmptyAmount} {Environment.NewLine}  წარუმატებელი : {errorCount} {Environment.NewLine} სულ : {totalCount}");
        }

        public void PostIncomeTaxFromBankStatement(string cardCode, double fullAmount, int series, string comments, DateTime docDate, int bplId, string currency = "GEL")
        {
            double incomeTaxAmount;
            string bpCode = cardCode;
            Recordset recSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery2($"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'");
            bool isIncomeTaxPayer = recSet.Fields.Item("U_IncomeTaxPayer").Value.ToString() == "01";
            bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
            recSet.DoQuery2($"Select * From [@RSM_SERVICE_PARAMS]");
            string incomeTaxAccDr = recSet.Fields.Item("U_IncomeTaxAccDr").Value.ToString();
            string incomeTaxAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();
            string incomeTaxControlAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();
            string incomeTaxOnInvoice = recSet.Fields.Item("U_IncomeTaxOnInvoice").Value.ToString();

            if (Convert.ToBoolean(incomeTaxOnInvoice))
            {
                return;
            }

            SAPbobsCOM.BusinessPartners bp =
                (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            bp.GetByKey(bpCode);
            var incomeTaxPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_IncomeTaxPayerPercent").Value.ToString(),
                CultureInfo.InstalledUICulture);
            var pensionPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_PensionPayerPercent").Value.ToString());

            if (isPensionPayer)
            {
                double pensionAmount = Math.Round(fullAmount / 0.784 * pensionPayerPercent / 100,
                    6);
                if (!isIncomeTaxPayer)
                {
                    pensionAmount = Math.Round(fullAmount / 0.98 * pensionPayerPercent / 100,
                        6);
                }
                incomeTaxAmount = Math.Round((fullAmount / 0.784 - pensionAmount) * incomeTaxPayerPercent / 100, 6);

            }
            else
            {
                incomeTaxAmount = Math.Round(fullAmount / 0.8 * incomeTaxPayerPercent / 100, 6);
            }

            if (isIncomeTaxPayer)
            {
                string incomeTaxPayerTransId = DocumentHelper.AddJournalEntry(oCompany, incomeTaxAccCr,
                    incomeTaxAccDr, incomeTaxControlAccCr, cardCode, incomeTaxAmount,
                    series, comments, docDate,
                    bplId, currency);
            }


        }
    }
}
