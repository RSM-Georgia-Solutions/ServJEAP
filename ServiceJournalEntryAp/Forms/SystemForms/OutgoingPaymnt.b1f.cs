using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using ServiceJournalEntryAp.Helpers;
using ServiceJournalEntryAp.Initialization;
using Application = SAPbouiCOM.Framework.Application;

namespace ServiceJournalEntryAp.SystemForms
{
    [FormAttribute("426", "Forms/SystemForms/OutgoingPaymnt.b1f")]
    class OutgoingPaymnt : SystemFormBase
    {

        public OutgoingPaymnt()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.DataAddAfter += new SAPbouiCOM.Framework.FormBase.DataAddAfterHandler(this.Form_DataAddAfter);
            this.CloseAfter += new SAPbouiCOM.Framework.FormBase.CloseAfterHandler(this.Form_CloseAfter);
            this.DataUpdateAfter += new DataUpdateAfterHandler(this.Form_DataUpdateAfter);

        }

        private void Form_DataAddAfter(ref SAPbouiCOM.BusinessObjectInfo pVal)
        {
            if (pVal.ActionSuccess)
            {
                var invObjectString = pVal.ObjectKey;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(invObjectString);
                string invDocEnttry = string.Empty;
                try
                {
                    invDocEnttry = xmlDoc.GetElementsByTagName("DocEntry").Item(0).InnerText;
                }
                catch (Exception e)
                {
                    SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage("Invalid Document Number",
                        BoMessageTime.bmt_Short, true);
                }

                Payments outgoingPaymentDi = (Payments)DiManager.Company.GetBusinessObject(BoObjectTypes.oVendorPayments);
                outgoingPaymentDi.GetByKey(int.Parse(invDocEnttry, CultureInfo.InvariantCulture));
                Recordset recSet = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string bpCode = outgoingPaymentDi.CardCode;
                recSet.DoQuery(DiManager.QueryHanaTransalte(
                    $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'"));
                bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
                bool isIncomeTaxPayer = recSet.Fields.Item("U_IncomeTaxPayer").Value.ToString() == "01";
                recSet.DoQuery(DiManager.QueryHanaTransalte($"Select * From [@RSM_SERVICE_PARAMS]"));
                string pensionAccDr = recSet.Fields.Item("U_PensionAccDr").Value.ToString();
                string incomeTaxAccDr = recSet.Fields.Item("U_IncomeTaxAccDr").Value.ToString();
                string incomeTaxAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();
                string pensionAccCr = recSet.Fields.Item("U_PensionAccCr").Value.ToString();
                string pensionControlAccDr = recSet.Fields.Item("U_PensionControlAccDr").Value.ToString();
                string pensionControlAccCr = recSet.Fields.Item("U_PensionControlAccCr").Value.ToString();
                string incomeControlTaxAccDr = recSet.Fields.Item("U_IncomeControlTaxAccDr").Value.ToString();
                string incomeControlTaxAccCr = recSet.Fields.Item("U_IncomeControlTaxAccCr").Value.ToString();
                bool incomeTaxOnInvoice = Convert.ToBoolean(recSet.Fields.Item("U_IncomeTaxOnInvoice").Value.ToString());


                var x = outgoingPaymentDi.GetAsXML();
                XmlDocument xmlDoc2 = new XmlDocument();
                xmlDoc2.LoadXml(x);
                string paymentOnAcc = xmlDoc2.GetElementsByTagName("NoDocSum").Item(0).InnerText;
                string paymentOnAccFc = xmlDoc2.GetElementsByTagName("NoDocSumFC").Item(0).InnerText;

                var price = "122$00";
                var nfi = new NumberFormatInfo
                {
                    CurrencyDecimalSeparator = DiManager.Company.GetCompanyService().GetAdminInfo().DecimalSeparator,
                    CurrencyGroupSeparator = DiManager.Company.GetCompanyService().GetAdminInfo().ThousandsSeparator
                };

                // var ok = decimal.Parse(price, NumberStyles.Currency, nfi);



                if (!string.IsNullOrWhiteSpace(paymentOnAcc))
                {
                    if (decimal.Parse(paymentOnAcc, NumberStyles.Currency, nfi) != 0)
                    {
                        double pensionAmountPaymentOnAccount;
                        double incomeTaxAmountPaymentOnAccount;
                        if (outgoingPaymentDi.DocCurrency != "GEL")
                        {
                            pensionAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAccFc) / 0.784 * 0.02,
                             6);
                            incomeTaxAmountPaymentOnAccount = (double.Parse(paymentOnAccFc) / 0.784 - pensionAmountPaymentOnAccount) * 0.2;

                            if (!isIncomeTaxPayer)
                            {
                                pensionAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAccFc) / 0.98 * 0.02,
                                    6);
                            }
                        }
                        else
                        {
                              pensionAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAcc) / 0.784 * 0.02,
                            6);
                              incomeTaxAmountPaymentOnAccount =
                                (double.Parse(paymentOnAcc) / 0.784 - pensionAmountPaymentOnAccount) * 0.2;

                            if (!isIncomeTaxPayer)
                            {
                                pensionAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAcc) / 0.98 * 0.02,
                                    6);
                            }
                        }


                        if (pensionAmountPaymentOnAccount != 0)
                        {
                            try
                            {
                                if (isPensionPayer)
                                {
                                    string transId = DiManager.AddJournalEntry(DiManager.Company,
                                        pensionAccCr,
                                        pensionAccDr,
                                        pensionControlAccCr,
                                        pensionControlAccDr,
                                        pensionAmountPaymentOnAccount,
                                        outgoingPaymentDi.Series,
                                        "OP " + outgoingPaymentDi.DocNum,
                                        outgoingPaymentDi.DocDate,
                                        outgoingPaymentDi.BPLID,
                                        outgoingPaymentDi.DocCurrency);
                                }
                            }
                            catch (Exception e)
                            {
                                Application.SBO_Application.MessageBox(e.Message);
                            }

                            try
                            {
                                if (isPensionPayer)
                                {
                                    string transId = DiManager.AddJournalEntry(DiManager.Company,
                                        pensionAccCr,
                                        "",
                                        pensionControlAccCr,
                                        outgoingPaymentDi.CardCode,
                                        pensionAmountPaymentOnAccount,
                                        outgoingPaymentDi.Series,
                                        "OP " + outgoingPaymentDi.DocNum,
                                        outgoingPaymentDi.DocDate,
                                        outgoingPaymentDi.BPLID,
                                        outgoingPaymentDi.DocCurrency);
                                }
                            }
                            catch (Exception e)
                            {
                                Application.SBO_Application.MessageBox(e.Message);
                            }
                        }

                        if (isIncomeTaxPayer && !incomeTaxOnInvoice)
                        {
                            if (outgoingPaymentDi.DocCurrency != "GEL")
                            {
                                if (!isPensionPayer)
                                {
                                    incomeTaxAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAccFc) / 0.8 * 0.2,
                                        6);
                                }
                            }
                            else
                            {
                                if (!isPensionPayer)
                                {
                                    incomeTaxAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAcc) / 0.8 * 0.2,
                                        6);
                                }
                            }
                           

                            string transId2 = DiManager.AddJournalEntry(DiManager.Company,
                                incomeTaxAccCr,
                                "",
                                pensionControlAccCr,
                                outgoingPaymentDi.CardCode,
                                incomeTaxAmountPaymentOnAccount,
                                outgoingPaymentDi.Series,
                                "OP " + outgoingPaymentDi.DocNum,
                                outgoingPaymentDi.DocDate,
                                outgoingPaymentDi.BPLID,
                                outgoingPaymentDi.DocCurrency);
                        }
                    }
                }

                for (int i = 0; i < outgoingPaymentDi.Invoices.Count; i++)
                {
                    outgoingPaymentDi.Invoices.SetCurrentLine(i);

                    if (outgoingPaymentDi.Invoices.InvoiceType == BoRcptInvTypes.it_PurchaseInvoice)
                    {
                        Documents invoiceDi = (Documents)DiManager.Company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
                        if (outgoingPaymentDi.Invoices.DocEntry == 0)
                        {
                            continue;
                        }
                        invoiceDi.GetByKey(outgoingPaymentDi.Invoices.DocEntry);

                        for (int j = 0; j < invoiceDi.Lines.Count; j++)
                        {
                            invoiceDi.Lines.SetCurrentLine(j);
                            recSet.DoQuery(DiManager.QueryHanaTransalte(
                                $"SELECT U_PensionLiable FROM OITM WHERE OITM.ItemCode = N'{invoiceDi.Lines.ItemCode}'"));


                            if (invoiceDi.DocType != BoDocumentTypes.dDocument_Service)
                            {
                                bool isPensionLiable = recSet.Fields.Item("U_PensionLiable").Value.ToString() == "01";

                                if (!isPensionLiable)
                                {
                                    continue;
                                }
                            }

                            double pensionAmount = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02, 6);

                            if (!isIncomeTaxPayer)
                            {
                                pensionAmount = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.98 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.98 * 0.02,
                                    6);
                            }

                            try
                            {
                                if (isPensionPayer)
                                {
                                    string incometaxpayertransidcomp = DiManager.AddJournalEntry(DiManager.Company,
                                        pensionAccCr,
                                        pensionAccDr,
                                        pensionControlAccCr,
                                        pensionControlAccDr,
                                        pensionAmount,
                                        invoiceDi.Series,
                                        "IN " + invoiceDi.DocNum,
                                        invoiceDi.DocDate,
                                        invoiceDi.BPL_IDAssignedToInvoice,
                                        invoiceDi.DocCurrency);
                                }
                            }
                            catch (Exception e)
                            {
                                Application.SBO_Application.MessageBox(e.Message);
                            }

                            try
                            {
                                if (isPensionPayer)
                                {
                                    string incometaxpayertransid = DiManager.AddJournalEntry(DiManager.Company,
                                        pensionAccCr,
                                        "",
                                        pensionControlAccCr,
                                        invoiceDi.CardCode,
                                        pensionAmount,
                                        invoiceDi.Series,
                                        "IN " + invoiceDi.DocNum,
                                        invoiceDi.DocDate,
                                        invoiceDi.BPL_IDAssignedToInvoice,
                                        invoiceDi.DocCurrency);
                                }
                            }
                            catch (Exception e)
                            {
                                Application.SBO_Application.MessageBox(e.Message);
                            }


                        }

                    }

                    else
                    {
                        double pensionAmount = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02, 6);
                        if (!isIncomeTaxPayer)
                        {
                            pensionAmount = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.98 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.98 * 0.02,
                                6);
                        }
                        try
                        {
                            if (isPensionPayer)
                            {
                                string incometaxpayertransidcomp = DiManager.AddJournalEntry(DiManager.Company,
                                    pensionAccCr,
                                    pensionAccDr,
                                    pensionControlAccCr,
                                    pensionControlAccDr,
                                    pensionAmount,
                                    outgoingPaymentDi.Series,
                                    outgoingPaymentDi.Invoices.InvoiceType + " " + outgoingPaymentDi.DocNum,
                                    outgoingPaymentDi.DocDate,
                                    outgoingPaymentDi.BPLID,
                                    outgoingPaymentDi.DocCurrency);
                            }
                        }
                        catch (Exception e)
                        {
                            Application.SBO_Application.MessageBox(e.Message);
                        }

                        try
                        {
                            if (isPensionPayer)
                            {
                                string incometaxpayertransid = DiManager.AddJournalEntry(DiManager.Company,
                                    pensionAccCr,
                                    "",
                                    pensionControlAccCr,
                                    outgoingPaymentDi.CardCode,
                                    pensionAmount,
                                    outgoingPaymentDi.Series,
                                    outgoingPaymentDi.Invoices.InvoiceType + " " + outgoingPaymentDi.DocNum,
                                    outgoingPaymentDi.DocDate,
                                    outgoingPaymentDi.BPLID,
                                    outgoingPaymentDi.DocCurrency);
                            }
                        }
                        catch (Exception e)
                        {
                            Application.SBO_Application.MessageBox(e.Message);
                        }
                    }
                    if (isIncomeTaxPayer && !incomeTaxOnInvoice)
                    {
                        if (outgoingPaymentDi.Invoices.SumApplied == 0)
                        {
                            continue;
                        }
                        double pensionAmount2 = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02, 6);
                        double taxPayerAmount = outgoingPaymentDi.DocCurrency != "GEL" ? (outgoingPaymentDi.Invoices.AppliedFC / 0.784 - pensionAmount2) * 0.2 : (outgoingPaymentDi.Invoices.SumApplied / 0.784 - pensionAmount2) * 0.2;
                        if (!isPensionPayer)
                        {
                            taxPayerAmount = outgoingPaymentDi.DocCurrency != "GEL" ? outgoingPaymentDi.Invoices.AppliedFC / 0.8 * 0.2 : outgoingPaymentDi.Invoices.SumApplied / 0.8 * 0.2;
                        }

                        string incometaxpayertransid = DiManager.AddJournalEntry(DiManager.Company,
                            incomeTaxAccCr,
                            "",
                            incomeControlTaxAccCr,
                            outgoingPaymentDi.CardCode,
                            taxPayerAmount,
                            outgoingPaymentDi.Series,
                            outgoingPaymentDi.Invoices.InvoiceType + " " + outgoingPaymentDi.DocNum,
                            outgoingPaymentDi.DocDate,
                            outgoingPaymentDi.BPLID,
                            outgoingPaymentDi.DocCurrency);
                    }
                }

            }
        }

        private void OnCustomInitialize()
        {

        }

        private void Form_CloseAfter(SBOItemEventArg pVal)
        {

        }

        private void Form_DataUpdateAfter(ref BusinessObjectInfo pVal)
        {
            if (pVal.ActionSuccess)
            {
                var invObjectString = pVal.ObjectKey;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(invObjectString);
                string invDocEnttry = string.Empty;
                try
                {
                    invDocEnttry = xmlDoc.GetElementsByTagName("DocEntry").Item(0).InnerText;
                }
                catch (Exception e)
                {
                    Application.SBO_Application.SetStatusBarMessage("Invalid Document Number",
                        BoMessageTime.bmt_Short);
                }

                Payments outgoingPaymentDi =
                    (Payments)DiManager.Company.GetBusinessObject(BoObjectTypes.oVendorPayments);
                outgoingPaymentDi.GetByKey(int.Parse(invDocEnttry,
                    CultureInfo.InvariantCulture));
                if (outgoingPaymentDi.Cancelled == BoYesNoEnum.tYES)
                {
                    Recordset recSet = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    string bpCode = outgoingPaymentDi.CardCode;
                    recSet.DoQuery(DiManager.QueryHanaTransalte(
                        $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'"));
                    bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
                    bool isIncomeTaxPayer = recSet.Fields.Item("U_IncomeTaxPayer").Value.ToString() == "01";
                    recSet.DoQuery(DiManager.QueryHanaTransalte($"Select * From [@RSM_SERVICE_PARAMS]"));
                    string pensionAccDr = recSet.Fields.Item("U_PensionAccDr").Value.ToString();
                    string incomeTaxAccDr = recSet.Fields.Item("U_IncomeTaxAccDr").Value.ToString();
                    string incomeTaxAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();
                    string pensionAccCr = recSet.Fields.Item("U_PensionAccCr").Value.ToString();
                    string pensionControlAccDr = recSet.Fields.Item("U_PensionControlAccDr").Value.ToString();
                    string pensionControlAccCr = recSet.Fields.Item("U_PensionControlAccCr").Value.ToString();
                    string incomeControlTaxAccDr = recSet.Fields.Item("U_IncomeControlTaxAccDr").Value.ToString();
                    string incomeControlTaxAccCr = recSet.Fields.Item("U_IncomeControlTaxAccCr").Value.ToString();
                    bool incomeTaxOnInvoice =
                        Convert.ToBoolean(recSet.Fields.Item("U_IncomeTaxOnInvoice").Value.ToString());

                    var nfi = new NumberFormatInfo
                    {
                        CurrencyDecimalSeparator = DiManager.Company.GetCompanyService().GetAdminInfo().DecimalSeparator,
                        CurrencyGroupSeparator = DiManager.Company.GetCompanyService().GetAdminInfo().ThousandsSeparator
                    };
                    var x = outgoingPaymentDi.GetAsXML();
                    XmlDocument xmlDoc2 = new XmlDocument();
                    xmlDoc2.LoadXml(x);
                    string paymentOnAcc = xmlDoc2.GetElementsByTagName("NoDocSum").Item(0).InnerText;
                    if (!string.IsNullOrWhiteSpace(paymentOnAcc))
                    {
                        if (decimal.Parse(paymentOnAcc,
                                NumberStyles.Currency,
                                nfi) != 0)
                        {
                            double pensionAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAcc) / 0.784 * 0.02,
                                6);
                            double incomeTaxAmountPaymentOnAccount =
                                (double.Parse(paymentOnAcc) / 0.784 - pensionAmountPaymentOnAccount) * 0.2;

                            if (!isIncomeTaxPayer)
                            {
                                pensionAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAcc) / 0.98 * 0.02,
                                    6);
                            }

                            if (pensionAmountPaymentOnAccount != 0)
                            {
                                try
                                {
                                    if (isPensionPayer)
                                    {
                                        string transId = DiManager.AddJournalEntry(DiManager.Company,
                                            pensionAccCr,
                                            pensionAccDr,
                                            pensionControlAccCr,
                                            pensionControlAccDr,
                                            -pensionAmountPaymentOnAccount,
                                            outgoingPaymentDi.Series,
                                            "OP " + outgoingPaymentDi.DocNum,
                                            outgoingPaymentDi.DocDate,
                                            outgoingPaymentDi.BPLID,
                                            outgoingPaymentDi.DocCurrency);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Application.SBO_Application.MessageBox(e.Message);
                                }

                                try
                                {
                                    if (isPensionPayer)
                                    {
                                        string transId = DiManager.AddJournalEntry(DiManager.Company,
                                            pensionAccCr,
                                            "",
                                            pensionControlAccCr,
                                            outgoingPaymentDi.CardCode,
                                            -pensionAmountPaymentOnAccount,
                                            outgoingPaymentDi.Series,
                                            "OP " + outgoingPaymentDi.DocNum,
                                            outgoingPaymentDi.DocDate,
                                            outgoingPaymentDi.BPLID,
                                            outgoingPaymentDi.DocCurrency);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Application.SBO_Application.MessageBox(e.Message);
                                }
                            }

                            if (isIncomeTaxPayer && !incomeTaxOnInvoice)
                            {
                                if (!isPensionPayer)
                                {
                                    incomeTaxAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAcc) / 0.8 * 0.2,
                                        6);
                                }

                                string transId2 = DiManager.AddJournalEntry(DiManager.Company,
                                    incomeTaxAccCr,
                                    "",
                                    incomeControlTaxAccCr,
                                    outgoingPaymentDi.CardCode,
                                    -incomeTaxAmountPaymentOnAccount,
                                    outgoingPaymentDi.Series,
                                    "OP " + outgoingPaymentDi.DocNum,
                                    outgoingPaymentDi.DocDate,
                                    outgoingPaymentDi.BPLID,
                                    outgoingPaymentDi.DocCurrency);
                            }
                        }
                    }

                    for (int i = 0; i < outgoingPaymentDi.Invoices.Count; i++)
                    {
                        outgoingPaymentDi.Invoices.SetCurrentLine(i);

                        if (outgoingPaymentDi.Invoices.InvoiceType == BoRcptInvTypes.it_PurchaseInvoice)
                        {
                            Documents invoiceDi =
                                (Documents)DiManager.Company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
                            if (outgoingPaymentDi.Invoices.DocEntry == 0)
                            {
                                continue;
                            }

                            invoiceDi.GetByKey(outgoingPaymentDi.Invoices.DocEntry);

                            for (int j = 0; j < invoiceDi.Lines.Count; j++)
                            {
                                invoiceDi.Lines.SetCurrentLine(j);
                                recSet.DoQuery(DiManager.QueryHanaTransalte(
                                    $"SELECT U_PensionLiable FROM OITM WHERE OITM.ItemCode = N'{invoiceDi.Lines.ItemCode}'"));


                                if (invoiceDi.DocType != BoDocumentTypes.dDocument_Service)
                                {
                                    bool isPensionLiable = recSet.Fields.Item("U_PensionLiable").Value.ToString() == "01";

                                    if (!isPensionLiable)
                                    {
                                        continue;
                                    }
                                }

                                double pensionAmount = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02, 6);

                                if (!isIncomeTaxPayer)
                                {
                                    pensionAmount = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.98 * 0.02,
                                        6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.98 * 0.02,
                                        6);
                                }

                                try
                                {
                                    if (isPensionPayer)
                                    {
                                        string incometaxpayertransidcomp = DiManager.AddJournalEntry(DiManager.Company,
                                            pensionAccCr,
                                            pensionAccDr,
                                            pensionControlAccCr,
                                            pensionControlAccDr,
                                            -pensionAmount,
                                            invoiceDi.Series,
                                            "IN " + invoiceDi.DocNum,
                                            invoiceDi.DocDate,
                                            invoiceDi.BPL_IDAssignedToInvoice,
                                            invoiceDi.DocCurrency);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Application.SBO_Application.MessageBox(e.Message);
                                }

                                try
                                {
                                    if (isPensionPayer)
                                    {
                                        string incometaxpayertransid = DiManager.AddJournalEntry(DiManager.Company,
                                            pensionAccCr,
                                            "",
                                            pensionControlAccCr,
                                            invoiceDi.CardCode,
                                            -pensionAmount,
                                            invoiceDi.Series,
                                            "IN " + invoiceDi.DocNum,
                                            invoiceDi.DocDate,
                                            invoiceDi.BPL_IDAssignedToInvoice,
                                            invoiceDi.DocCurrency);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Application.SBO_Application.MessageBox(e.Message);
                                }


                            }

                        }

                        else
                        {
                            double pensionAmount = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02,
                                6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02,
                                6);
                            if (!isIncomeTaxPayer)
                            {
                                pensionAmount = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.98 * 0.02,
                                    6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.98 * 0.02,
                                    6);
                            }
                            try
                            {
                                if (isPensionPayer)
                                {
                                    string incometaxpayertransidcomp = DiManager.AddJournalEntry(DiManager.Company,
                                        pensionAccCr,
                                        pensionAccDr,
                                        pensionControlAccCr,
                                        pensionControlAccDr,
                                        -pensionAmount,
                                        outgoingPaymentDi.Series,
                                        outgoingPaymentDi.Invoices.InvoiceType + " " + outgoingPaymentDi.DocNum,
                                        outgoingPaymentDi.DocDate,
                                        outgoingPaymentDi.BPLID,
                                        outgoingPaymentDi.DocCurrency);
                                }
                            }
                            catch (Exception e)
                            {
                                Application.SBO_Application.MessageBox(e.Message);
                            }

                            try
                            {
                                if (isPensionPayer)
                                {
                                    string incometaxpayertransid = DiManager.AddJournalEntry(DiManager.Company,
                                        pensionAccCr,
                                        "",
                                        pensionControlAccCr,
                                        outgoingPaymentDi.CardCode,
                                        -pensionAmount,
                                        outgoingPaymentDi.Series,
                                        outgoingPaymentDi.Invoices.InvoiceType + " " + outgoingPaymentDi.DocNum,
                                        outgoingPaymentDi.DocDate,
                                        outgoingPaymentDi.BPLID,
                                        outgoingPaymentDi.DocCurrency);
                                }
                            }
                            catch (Exception e)
                            {
                                Application.SBO_Application.MessageBox(e.Message);
                            }
                        }

                        if (isIncomeTaxPayer && !incomeTaxOnInvoice)
                        {
                            if (outgoingPaymentDi.Invoices.SumApplied == 0)
                            {
                                continue;
                            }

                            double pensionAmount2 = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02,
                                6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02,
                                6);
                            double taxPayerAmount = outgoingPaymentDi.DocCurrency != "GEL" ? (outgoingPaymentDi.Invoices.AppliedFC / 0.784 - pensionAmount2) * 0.2 : (outgoingPaymentDi.Invoices.SumApplied / 0.784 - pensionAmount2) * 0.2;
                            if (!isPensionPayer)
                            {
                                taxPayerAmount = outgoingPaymentDi.DocCurrency != "GEL" ?
                                    outgoingPaymentDi.Invoices.AppliedFC / 0.8 * 0.2 : outgoingPaymentDi.Invoices.SumApplied / 0.8 * 0.2;
                            }

                            string incometaxpayertransid = DiManager.AddJournalEntry(DiManager.Company,
                                incomeTaxAccCr,
                                "",
                                incomeControlTaxAccCr,
                                outgoingPaymentDi.CardCode,
                                -taxPayerAmount,
                                outgoingPaymentDi.Series,
                                outgoingPaymentDi.Invoices.InvoiceType + " " + outgoingPaymentDi.DocNum,
                                outgoingPaymentDi.DocDate,
                                outgoingPaymentDi.BPLID,
                                outgoingPaymentDi.DocCurrency);
                        }
                    }
                }
            }
        }
    }
}
