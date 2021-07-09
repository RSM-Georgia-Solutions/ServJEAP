using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;
using SAPbouiCOM;
using System.Globalization;
using ServiceJournalEntryLogic.Providers;
using RSM.Core.SDK.DI.Extension;

namespace ServiceJournalEntryAp.Controllers
{
    public class BankStatementDetailsFormController : FormController
    {
        public SettingsProvider SettingsProvider { get; private set; }
        public BspHistoryProvider BspHistoryProvider { get; private set; }
        public BankStatementDetailsFormController(SAPbobsCOM.Company Company, IForm Form, SettingsProvider settingsProvider, BspHistoryProvider bspHistoryProvider) : base(Company, Form)
        {
            SettingsProvider = settingsProvider;
            BspHistoryProvider = bspHistoryProvider;
        }

        public SAPbouiCOM.EditText BankAccountEditText { get { return (EditText)oForm.Items.Item("10000013").Specific; } }
        public SAPbouiCOM.EditText BSPNumberEditText { get { return (EditText)oForm.Items.Item("10000022").Specific; } }
        public SAPbouiCOM.DBDataSource OBNHDataSource { get { return oForm.DataSources.DBDataSources.Item("OBNH"); } }
        public SAPbouiCOM.DBDataSource OBNKDataSource { get { return oForm.DataSources.DBDataSources.Item("OBNK"); } }
        public SAPbouiCOM.Matrix oMatrix { get { return (Matrix)oForm.Items.Item("10000036").Specific; } }



        public void PostPension()
        {
            var settings = SettingsProvider.Get();
            int successCount = 0;
            int hasNoJounralEntryCount = 0;
            int errorCount = 0;
            int totalCount = 0;
            int addedAlready = 0;
            int hasNoBp = 0;
            int notPayer = 0;
            int EmptyAmount = 0;

            string idNumber = OBNHDataSource.GetValue("IdNumber", 0);

            Recordset recSetSeries = RSM.Core.SDK.DI.DIApplication.GetRecordset();
            recSetSeries.DoQuery2($"SELECT OutSeri FROM DSC1 WHERE Account = N'{BankAccountEditText.Value}'");

            var series = (int)recSetSeries.Fields.Item("OutSeri").Value;


            for (int i = 1; i <= oMatrix.RowCount; i++)
            {

                #region validations & calculations
                try
                {
                    if (((ComboBox)oMatrix.GetCellSpecific("10000037", i)).Selected == null || string.IsNullOrEmpty(((ComboBox)oMatrix.GetCellSpecific("10000037", i)).Selected.Value))
                    {
                        continue;
                    }

                }
                catch
                {
                    RSM.Core.SDK.UI.UIApplication.ShowError("Error During Reading Matrix");
                    continue;
                }


                string cardCode = OBNKDataSource.GetValue("CardCode", i - 1);
                string sequence = OBNKDataSource.GetValue("Sequence", i - 1);
                string order = OBNKDataSource.GetValue("VisOrder", i - 1);
                
             

                if (BspHistoryProvider.Exists(idNumber, sequence, BankAccountEditText.Value))
                {
                    addedAlready += 2;
                    totalCount += 2;
                    continue;
                }


                int journalEntryTransId;

                try
                {
                    //თუ საჟურანლო გატარება არ აქვს
                    journalEntryTransId = int.Parse(OBNKDataSource.GetValue("JDTID", i - 1), CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    totalCount += 2;
                    hasNoJounralEntryCount += 2;
                    continue;
                }


                if (string.IsNullOrWhiteSpace(cardCode))
                {
                    totalCount += 2;
                    hasNoBp += 2;
                    continue;
                }


                JournalEntries journalEntry = (SAPbobsCOM.JournalEntries)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);
                journalEntry.GetByKey(journalEntryTransId);
                bool hasVendor = false;

                for (int j = 0; j < journalEntry.Lines.Count; j++)
                {
                    journalEntry.Lines.SetCurrentLine(j);
                    if (journalEntry.Lines.ShortName == cardCode)
                    {
                        hasVendor = true;
                        break;
                    }
                }

                var amountCurrencyString = ((EditText)oMatrix.GetCellSpecific("10000045", i)).Value;

                if (string.IsNullOrWhiteSpace(amountCurrencyString))
                {
                    EmptyAmount += 2;
                    totalCount += 2;
                    continue;
                }


                if (!hasVendor)
                {
                    totalCount += 2;
                    hasNoJounralEntryCount += 2;
                    continue;
                }
                var rame = amountCurrencyString.Split(' ')[0];
                double amount;
                var parsed = double.TryParse(amountCurrencyString.Split(' ')[0], out amount);
                if (!parsed)//გასარკვევია ეს საკითხი
                {
                    SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage("Invalid Amount : \"Outgoing Amt - Payment Currency\"",
                       BoMessageTime.bmt_Short, true);
                    continue;
                }

                int bplId = 235;
                string bplIdString = OBNKDataSource.GetValue("BPLIdPmn", i - 1);
                if (!string.IsNullOrWhiteSpace(bplIdString))
                {
                    bplId = int.Parse(bplIdString);
                }
                #endregion


                var paymentId = OBNKDataSource.GetValue("PmntID", i - 1);
                string isPaymentOnAccount = string.Empty;
                try
                {
                    recSetSeries.DoQuery2($"select PayNoDoc from OVPM where DocEntry = {paymentId}");
                    isPaymentOnAccount = recSetSeries.Fields.Item(0).Value.ToString();
                }
                catch
                {
                    throw new Exception("Invalid Payment DocEntry");
                }


                SAPbobsCOM.BusinessPartners businessPartner = (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);
                businessPartner.GetByKey(cardCode);

                bool isPensionPayer = (string)businessPartner.UserFields.Fields.Item("U_PensionPayer").Value == "01";
                bool incomeTaxPayer = (string)businessPartner.UserFields.Fields.Item("U_IncomeTaxPayer").Value == "01";

                double pensionAmountPaymentOnAccount = GetPensionAmount(amount, incomeTaxPayer);

                var curryencyString = amountCurrencyString.Split(' ')[1];

                var postingDate = DateTime.ParseExact(((EditText)oMatrix.GetCellSpecific("10000003", i)).Value, "yyyyMMdd", CultureInfo.InvariantCulture);

                if (!settings.IncomeTaxOnInvoice && incomeTaxPayer)
                {
                    var incomeTaxPayerPercent = (double)businessPartner.UserFields.Fields.Item("U_IncomeTaxPayerPercent").Value;
                    var pensionPayerPercent = (double)businessPartner.UserFields.Fields.Item("U_PensionPayerPercent").Value;
                    double incomeTaxAmount;

                    if (isPensionPayer)
                    {
                        double pensionAmount = Math.Round(amount / 0.784 * pensionPayerPercent / 100,
                            6);
                        if (!incomeTaxPayer)
                        {
                            pensionAmount = Math.Round(amount / 0.98 * pensionPayerPercent / 100,
                                6);
                        }
                        incomeTaxAmount = Math.Round((amount / 0.784 - pensionAmount) * incomeTaxPayerPercent / 100, 6);

                    }
                    else
                    {
                        incomeTaxAmount = Math.Round(amount / 0.8 * incomeTaxPayerPercent / 100, 6);
                    }

                    if (incomeTaxPayer)
                    {
                        string incomeTaxPayerTransId = DocumentHelper.AddJournalEntry(oCompany, settings.IncomeTaxAccCr,
                            settings.IncomeTaxAccDr, settings.IncomeControlTaxAccCr, cardCode, incomeTaxAmount,
                            series, "BS " + BSPNumberEditText.Value + " " + order, postingDate,
                            bplId, curryencyString);
                    }

                }



                //საპენსიოს დაპოსტვა
                if (!isPensionPayer)
                {
                    notPayer += 2;
                    totalCount += 2;
                    continue;
                }
                

                if (isPaymentOnAccount == "N" && settings.UseDocControllAcc)
                {
                    DocumentHelper.OnPaymentAdd(paymentId, false);
                    successCount += 2;
                    totalCount += 2;
                    //BspHistoryProvider.Save(new ServiceJournalEntryLogic.Models.BspHisotry()
                    //{
                    //    BSP_ACCOUNT = BankAccountEditText.Value,
                    //    BSP_ID_NUMBER = idNumber,
                    //    BSP_SEQUENCE = sequence,
                    //    TRANS_ID_EMPLOYEE = "",
                    //    TRANS_ID_COMPANY = ""
                    //});
                    continue;
                }

                string transIdEmp = string.Empty;

                try
                {
                    transIdEmp = DocumentHelper.AddJournalEntry(oCompany,
                        settings.PensionAccCr, settings.PensionAccDr, settings.PensionControlAccCr, settings.PensionControlAccDr, pensionAmountPaymentOnAccount,
                        series, "BS " + BSPNumberEditText.Value + " " + order, postingDate,
                        bplId, curryencyString);

                    successCount++;
                    totalCount++;

                }
                catch (Exception e)
                {
                    SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage(e.Message, BoMessageTime.bmt_Short, true);
                    errorCount++;
                }

                string transIdComp = "";
                try
                {
                    //if (settings.UseDocControllAcc)
                    //{
                        string transId = DocumentHelper.AddJournalEntry(oCompany, settings.PensionAccCr,
                        "", settings.PensionControlAccCr, cardCode, pensionAmountPaymentOnAccount, series,
                        "BP " + BSPNumberEditText.Value + " " + order, postingDate, bplId,
                        curryencyString);
                    //}
                    //else
                    //{
                        //var comment = "BP " + BSPNumberEditText.Value + " " + order;

                        //JournalEntries vJE = (JournalEntries)oCompany.GetBusinessObject(BoObjectTypes.oJournalEntries);
                        //vJE.ReferenceDate = postingDate;
                        //vJE.DueDate = postingDate;
                        //vJE.TaxDate = postingDate;
                        //vJE.Memo = comment.Length < 50 ? comment : comment.Substring(0, 49);
                        //vJE.Lines.BPLID = bplId;
                        //if (curryencyString == "GEL")
                        //{
                        //    vJE.Lines.Debit = 0;
                        //}
                        //else
                        //{
                        //    vJE.Lines.FCCurrency = curryencyString;
                        //    vJE.Lines.FCDebit = pensionAmountPaymentOnAccount;
                        //}
                        //vJE.Lines.ShortName = cardCode;



                        //var docId = ((EditText)oMatrix.GetCellSpecific("200000141", i)).Value;
                        //int paymentID;
                        //if (int.TryParse(docId, out paymentID))
                        //{
                        //    var doc = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
                        //    doc.GetByKey(paymentID);
                        //    vJE.Lines.ControlAccount = doc.ControlAccount;
                        //}


                        //vJE.Lines.Add();
                        //vJE.Lines.BPLID = bplId;

                        //if (curryencyString == "GEL")
                        //{
                        //    vJE.Lines.Credit = pensionAmountPaymentOnAccount;
                        //    vJE.Lines.FCCredit = 0;
                        //}
                        //else
                        //{
                        //    vJE.Lines.FCCurrency = curryencyString;
                        //    vJE.Lines.FCCredit = pensionAmountPaymentOnAccount;
                        //}
                        //if (string.IsNullOrWhiteSpace(settings.PensionAccCr))
                        //{
                        //    vJE.Lines.ShortName = settings.PensionControlAccCr;
                        //}
                        //else
                        //{
                        //    vJE.Lines.AccountCode = settings.PensionAccCr;
                        //}
                        //vJE.Lines.Add();
                        //var ret = vJE.Add();
                        //if (ret == 0)
                        //{
                        //    transIdComp = oCompany.GetNewObjectKey();
                        //}
                        //else
                        //{
                        //    throw new Exception(oCompany.GetLastErrorDescription());
                        //}
                    //}




                    successCount++;
                    totalCount++;

                }
                catch (Exception e)
                {
                    SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                    errorCount++;
                }

                BspHistoryProvider.Save(new ServiceJournalEntryLogic.Models.BspHisotry()
                {
                    BSP_ACCOUNT = BankAccountEditText.Value,
                    BSP_ID_NUMBER = idNumber,
                    BSP_SEQUENCE = sequence,
                    TRANS_ID_EMPLOYEE = transIdEmp,
                    TRANS_ID_COMPANY = transIdComp
                });
            }

            SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(
                $"წარმატებული : {successCount}  {Environment.NewLine}  უკვე გაგატარებული : {addedAlready}  {Environment.NewLine} არ აქვს საჟურნალო გატარება : {hasNoJounralEntryCount} {Environment.NewLine} არ აქვს ბიზნეს პარტნიორი : {hasNoBp} {Environment.NewLine} არ არის გადამხდელი : {notPayer}   { Environment.NewLine} თანხა არ არის მითითებული : {EmptyAmount} {Environment.NewLine}  წარუმატებელი : {errorCount} {Environment.NewLine} სულ : {totalCount}");
        }

        private static double GetPensionAmount(double amount, bool incomeTaxPayer)
        {
            double pensionAmountPaymentOnAccount = Math.Round(amount / 0.784 * 0.02, 6);

            if (!incomeTaxPayer)
            {
                pensionAmountPaymentOnAccount = Math.Round(amount / 0.98 * 0.02, 6);
            }

            return pensionAmountPaymentOnAccount;
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
