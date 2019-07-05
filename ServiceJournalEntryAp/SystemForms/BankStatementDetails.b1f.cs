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
        }

        private SAPbouiCOM.Button Button0;

        private void OnCustomInitialize()
        {

        }

        private void Button0_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            Form activeForm = SAPbouiCOM.Framework.Application.SBO_Application.Forms.ActiveForm;
            var accountHeader = ((EditText)activeForm.Items.Item("10000013").Specific).Value;
            var bsNumber = ((EditText)activeForm.Items.Item("10000022").Specific).Value;
            string idNumber = activeForm.DataSources.DBDataSources.Item("OBNH").GetValue("IdNumber", 0);

            Recordset recSetSeries =(Recordset) DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSetSeries.DoQuery(DiManager.QueryHanaTransalte($"SELECT OutSeri FROM DSC1 WHERE Account = N'{accountHeader}'"));
            var series =  int.Parse(recSetSeries.Fields.Item("OutSeri").Value.ToString());

            var matrix = (Matrix)activeForm.Items.Item("10000036").Specific;
            activeForm.Freeze(true);

            for (int i = 1; i <= matrix.RowCount; i++)
            {
                if (((ComboBox)matrix.GetCellSpecific("10000037", i)).Selected == null)
                {
                    continue;
                }

                string cardCode = activeForm.DataSources.DBDataSources.Item(0).GetValue("CardCode", i - 1);
                string bplIdString = activeForm.DataSources.DBDataSources.Item(0).GetValue("BPLIdPmn", i - 1);
                string sequence = activeForm.DataSources.DBDataSources.Item("OBNK").GetValue("Sequence", i - 1);
            

                int bplId = int.Parse(bplIdString);
                if (string.IsNullOrWhiteSpace(cardCode))
                {
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
                    return;
                }

                double amount = 0;
                var postingDate = DateTime.ParseExact(((EditText)matrix.GetCellSpecific("10000003", i)).Value, "yyyyMMdd",CultureInfo.InvariantCulture);
                var amountCurrencyString = ((EditText)matrix.GetCellSpecific("10000045", i)).Value;
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
                    (Recordset) DiManager.Company .GetBusinessObject(BoObjectTypes
                        .BoRecordset);
                Recordset recSet3 =
                    (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes
                        .BoRecordset);
                recSet2.DoQuery(DiManager.QueryHanaTransalte($"select * from [@RSM_BSP_HISTORY] WHERE U_BSP_ID_NUMBER = {idNumber} AND U_BSP_SEQUENCE = {sequence} AND U_BSP_ACCOUNT = N'{accountHeader}'"));

                if (!recSet2.EoF)
                {
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
                        series, "BS " + bsNumber, postingDate,
                        bplId, curryencyString);
                    query += $"'{transId}'";

                }
                catch (Exception e)
                {
                    Application.SBO_Application.MessageBox(e.Message);
                }

                try
                {
                    string transId = DiManager.AddJournalEntry(DiManager.Company, pensionAccCr,
                        "", pensionControlAccCr, cardCode, pensionAmountPaymentOnAccount, series,
                        "OP " + bsNumber, postingDate, bplId,
                        curryencyString);
                    query += $", '{transId}')";
                }
                catch (Exception e)
                {
                    Application.SBO_Application.MessageBox(e.Message);
                }

                recSet3.DoQuery(DiManager.QueryHanaTransalte(query));

            }
        }
    }
}
