using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;
using SAPbouiCOM.Framework;
using ServiceJournalEntryAp.Initialization;

namespace ServiceJournalEntryAp.Helpers
{
    public static class DocumentHelper
    {
        public static void PostIncomeTax(string invDocEnttry)
        {
            Documents invoiceDi = (Documents)DiManager.Company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
            invoiceDi.GetByKey(int.Parse(invDocEnttry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;

            Recordset recSet = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);


            recSet.DoQuery(DiManager.QueryHanaTransalte(
                $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'"));

            bool isIncomeTaxPayer = recSet.Fields.Item("U_IncomeTaxPayer").Value.ToString() == "01";
            bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
            recSet.DoQuery(DiManager.QueryHanaTransalte($"Select * From [@RSM_SERVICE_PARAMS]"));
            string incomeTaxAccDr = recSet.Fields.Item("U_IncomeTaxAccDr").Value.ToString();
            string incomeTaxAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();
            string incomeTaxControlAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();


            SAPbobsCOM.BusinessPartners bp =
                (SAPbobsCOM.BusinessPartners)DiManager.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            bp.GetByKey(invoiceDi.CardCode);

            var incomeTaxPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_IncomeTaxPayerPercent").Value.ToString(),
                CultureInfo.InstalledUICulture);

            var pensionPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_PensionPayerPercent").Value.ToString());

            for (int i = 0; i < invoiceDi.Lines.Count; i++)
            {
                invoiceDi.Lines.SetCurrentLine(i);
                recSet.DoQuery(DiManager.QueryHanaTransalte(
                    $"SELECT U_PensionLiable FROM OITM WHERE OITM.ItemCode = N'{invoiceDi.Lines.ItemCode}'"));
                bool isPensionLiable = recSet.Fields.Item("U_PensionLiable").Value.ToString() == "01";


                double incomeTaxAmount;

                if (!isPensionLiable)
                {
                    double lineTotal = invoiceDi.Lines.LineTotal;
                    incomeTaxAmount = Math.Round(lineTotal * incomeTaxPayerPercent / 100, 6);
                }
                else
                {
                    if (isPensionPayer)
                    {
                        double lineTotal = invoiceDi.Lines.LineTotal;
                        double pensionAmount = Math.Round(lineTotal * pensionPayerPercent / 100, 6);
                        incomeTaxAmount = Math.Round((lineTotal - pensionAmount) * incomeTaxPayerPercent / 100, 6);
                    }
                    else
                    {
                        double lineTotal = invoiceDi.Lines.LineTotal;
                        incomeTaxAmount = Math.Round((lineTotal) * incomeTaxPayerPercent / 100, 6);
                    }

                }



                if (isIncomeTaxPayer && invoiceDi.CancelStatus == CancelStatusEnum.csNo)
                {
                    try
                    {
                        string incomeTaxPayerTransId = DiManager.AddJournalEntry(DiManager.Company, incomeTaxAccCr,
                            incomeTaxAccDr, incomeTaxControlAccCr, invoiceDi.CardCode, incomeTaxAmount,
                            invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                            invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);
                    }
                    catch (Exception e)
                    {
                        Application.SBO_Application.MessageBox(e.Message);
                    }
                }
                if (isIncomeTaxPayer && invoiceDi.CancelStatus == CancelStatusEnum.csCancellation)
                {
                    try
                    {
                        string incomeTaxPayerTransId = DiManager.AddJournalEntry(DiManager.Company, incomeTaxAccCr,
                            incomeTaxAccDr, incomeTaxControlAccCr, invoiceDi.CardCode, -incomeTaxAmount,
                            invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                            invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);
                    }
                    catch (Exception e)
                    {
                        Application.SBO_Application.MessageBox(e.Message);
                    }
                }

            }
        }

        public static void PostPension(string invoiceDocentry)
        {
            Documents invoiceDi = (Documents)DiManager.Company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
            invoiceDi.GetByKey(int.Parse(invoiceDocentry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;

            Recordset recSet = (Recordset)DiManager.Company.GetBusinessObject(BoObjectTypes.BoRecordset);


            recSet.DoQuery(DiManager.QueryHanaTransalte(
                $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'"));

            bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";

            recSet.DoQuery(DiManager.QueryHanaTransalte($"Select * From [@RSM_SERVICE_PARAMS]"));
            string pensionAccDr = recSet.Fields.Item("U_PensionAccDr").Value.ToString();
            string pensionAccCr = recSet.Fields.Item("U_PensionAccCr").Value.ToString();
            string pensionControlAccDr = recSet.Fields.Item("U_PensionControlAccDr").Value.ToString();
            string pensionControlAccCr = recSet.Fields.Item("U_PensionControlAccCr").Value.ToString();

            SAPbobsCOM.BusinessPartners bp =
                (SAPbobsCOM.BusinessPartners)DiManager.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            bp.GetByKey(invoiceDi.CardCode);

            var incomeTaxPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_IncomeTaxPayerPercent").Value.ToString(),
                CultureInfo.InstalledUICulture);

            var pensionPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_PensionPayerPercent").Value.ToString());

            for (int i = 0; i < invoiceDi.Lines.Count; i++)
            {
                invoiceDi.Lines.SetCurrentLine(i);
                recSet.DoQuery(DiManager.QueryHanaTransalte(
                    $"SELECT U_PensionLiable FROM OITM WHERE OITM.ItemCode = N'{invoiceDi.Lines.ItemCode}'"));
                bool isPensionLiable = recSet.Fields.Item("U_PensionLiable").Value.ToString() == "01";
                if (!isPensionLiable)
                {
                    continue;
                }

                double lineTotal = invoiceDi.Lines.LineTotal;
                double pensionAmount = Math.Round(lineTotal * pensionPayerPercent / 100, 6);

                if (isPensionPayer)
                {
                    //invoiceDi.CancelStatus == CancelStatusEnum.csNo
                    try
                    {
                        string incometaxpayertransidcomp = DiManager.AddJournalEntry(DiManager.Company,
                            pensionAccCr, pensionAccDr, pensionControlAccCr, pensionControlAccDr, pensionAmount,
                            invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                            invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);
                    }
                    catch (Exception e)
                    {
                        Application.SBO_Application.MessageBox(e.Message);
                    }

                    try
                    {
                        string incometaxpayertransid = DiManager.AddJournalEntry(DiManager.Company, pensionAccCr,
                            "", pensionControlAccCr, invoiceDi.CardCode, pensionAmount, invoiceDi.Series,
                            invoiceDi.Comments, invoiceDi.DocDate, invoiceDi.BPL_IDAssignedToInvoice,
                            invoiceDi.DocCurrency);
                    }
                    catch (Exception e)
                    {
                        Application.SBO_Application.MessageBox(e.Message);
                    }
                }
            }
        }

    }
}
