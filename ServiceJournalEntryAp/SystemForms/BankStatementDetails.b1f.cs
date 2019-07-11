using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using ServiceJournalEntryAp.Initialization;
using Application = SAPbouiCOM.Framework.Application;

namespace ServiceJournalEntryAp.SystemForms
{
    [FormAttribute("10000005", "SystemForms/BankStatementDetails.b1f")]
    class BankStatementDetails : SystemFormBase
    {
        public BankStatementDetails()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.Button0 = ((SAPbouiCOM.Button)(this.GetItem("Item_99").Specific));
            this.Button0.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.Button0_PressedAfter);
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.ActivateAfter += new ActivateAfterHandler(this.Form_ActivateAfter);
            this.ClickAfter += new ClickAfterHandler(this.Form_ClickAfter);

        }

        private SAPbouiCOM.Button Button0;

        private void OnCustomInitialize()
        {
            Button0.Item.FontSize = 10;
        }

        private void Button0_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            int successCount = 0;
            int hasNoJounralEntryCount = 0;
            int errorCount = 0;
            int totalCount = 0;
            int addedAlready = 0;
            int hasNoBp = 0;
            int notPayer = 0;
            int EmptyAmount = 0;

            Form activeForm = Application.SBO_Application.Forms.ActiveForm;
            var accountHeader = ((EditText)activeForm.Items.Item("10000013").Specific).Value;
            var bsNumber = ((EditText)activeForm.Items.Item("10000022").Specific).Value;
            string idNumber = activeForm.DataSources.DBDataSources.Item("OBNH").GetValue("IdNumber", 0);

            Recordset recSetSeries = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSetSeries.DoQuery(DiManager.QueryHanaTransalte($"SELECT OutSeri FROM DSC1 WHERE Account = N'{accountHeader}'"));
            var series = int.Parse(recSetSeries.Fields.Item("OutSeri").Value.ToString());

            var matrix = (Matrix)activeForm.Items.Item("10000036").Specific;



            for (int i = 1; i <= matrix.RowCount; i++)
            {
                if (((ComboBox)matrix.GetCellSpecific("10000037", i)).Selected == null)
                {
                    continue;
                }

                string cardCode = activeForm.DataSources.DBDataSources.Item(0).GetValue("CardCode", i - 1);
                string bplIdString = activeForm.DataSources.DBDataSources.Item(0).GetValue("BPLIdPmn", i - 1);
                string sequence = activeForm.DataSources.DBDataSources.Item("OBNK").GetValue("Sequence", i - 1);
                string order = activeForm.DataSources.DBDataSources.Item("OBNK").GetValue("VisOrder", i - 1);

                int journalEntryTransId;
                try
                {
                    journalEntryTransId = int.Parse(activeForm.DataSources.DBDataSources.Item(0).GetValue("JDTID", i - 1), CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    totalCount += 2;
                    hasNoJounralEntryCount += 2;
                    continue;
                }


                int bplId = int.Parse(bplIdString);

                if (string.IsNullOrWhiteSpace(cardCode))
                {
                    totalCount += 2;
                    hasNoBp += 2;
                    continue;
                }


                JournalEntries journalEntry = (SAPbobsCOM.JournalEntries)DiManager.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);
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

                Recordset recSet = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string bpCode = cardCode;
                recSet.DoQuery(DiManager.QueryHanaTransalte(
                    $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'"));
                bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
                recSet.DoQuery(DiManager.QueryHanaTransalte($"Select * From [@RSM_SERVICE_PARAMS]"));
                string pensionAccDr = recSet.Fields.Item("U_PensionAccDr").Value.ToString();
                string pensionAccCr = recSet.Fields.Item("U_PensionAccCr").Value.ToString();
                string pensionControlAccDr = recSet.Fields.Item("U_PensionControlAccDr").Value.ToString();
                string pensionControlAccCr = recSet.Fields.Item("U_PensionControlAccCr").Value.ToString();
                if (!isPensionPayer)
                {
                    notPayer += 2;
                    totalCount += 2;
                    continue;
                }

             

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
                    Application.SBO_Application.SetStatusBarMessage("Invalid Amount : \"Outgoing Amt - Payment Currency\"",
                        BoMessageTime.bmt_Short, true);
                }

                double pensionAmountPaymentOnAccount = Math.Round(amount / 0.784 * 0.02, 6);

                Recordset recSet2 =
                    (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes
                        .BoRecordset);
                Recordset recSet3 =
                    (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes
                        .BoRecordset);
                recSet2.DoQuery(DiManager.QueryHanaTransalte($"select * from [@RSM_BSP_HISTORY] WHERE U_BSP_ID_NUMBER = {idNumber} AND U_BSP_SEQUENCE = {sequence} AND U_BSP_ACCOUNT = N'{accountHeader}'"));

                if (!recSet2.EoF)
                {
                    addedAlready += 2;
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
                    string transId = DiManager.AddJournalEntry(DiManager.Company,
                        pensionAccCr, pensionAccDr, pensionControlAccCr, pensionControlAccDr, pensionAmountPaymentOnAccount,
                        series, "BS " + bsNumber + " " + order, postingDate,
                        bplId, curryencyString);
                    query += $"'{transId}'";
                    successCount++;
                    totalCount++;

                }
                catch (Exception e)
                {
                    Application.SBO_Application.MessageBox(e.Message);
                    errorCount++;
                }

                try
                {
                    string transId = DiManager.AddJournalEntry(DiManager.Company, pensionAccCr,
                        "", pensionControlAccCr, cardCode, pensionAmountPaymentOnAccount, series,
                        "BP " + bsNumber + " " + order, postingDate, bplId,
                        curryencyString);
                    query += $", '{transId}')";
                    successCount++;
                    totalCount++;

                }
                catch (Exception e)
                {
                    Application.SBO_Application.MessageBox(e.Message);
                    errorCount++;
                }

                recSet3.DoQuery(DiManager.QueryHanaTransalte(query));

            }

            Application.SBO_Application.MessageBox(
                $"წარმატებული : {successCount}  {Environment.NewLine}  უკვე გაგატარებული : {addedAlready}  {Environment.NewLine} არ აქვს საჟურნალო გატარება : {hasNoJounralEntryCount} {Environment.NewLine} არ აქვს ბიზნეს პარტნიორი : {hasNoBp} {Environment.NewLine} არ არის გადამხდელი : {notPayer}   { Environment.NewLine} თანხა არ არის მითითებული : {EmptyAmount} {Environment.NewLine}  წარუმატებელი : {errorCount} {Environment.NewLine} სულ : {totalCount}");
        }

        private void Form_ActivateAfter(SBOItemEventArg pVal)
        {
            Form activeForm = Application.SBO_Application.Forms.ActiveForm;
            var status = activeForm.DataSources.DBDataSources.Item("OBNH").GetValue("status", 0);
            GetItem("Item_99").Enabled = status == "E";
        }

        private void Form_ClickAfter(SBOItemEventArg pVal)
        {
            Form activeForm = Application.SBO_Application.Forms.ActiveForm;
            var status = activeForm.DataSources.DBDataSources.Item("OBNH").GetValue("status", 0);
            GetItem("Item_99").Enabled = status == "E";
        }
    }
}
