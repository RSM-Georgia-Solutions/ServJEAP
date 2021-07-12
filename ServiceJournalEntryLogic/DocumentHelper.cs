using RSM.Core.SDK.DI.Extension;
using SAPbobsCOM;
using ServiceJournalEntryLogic.Models;
using ServiceJournalEntryLogic.Providers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace ServiceJournalEntryLogic
{
    public class DocumentHelper : IDocumentHelper
    {
        private readonly Company oCompany;
        public SettingsProvider settingsProvider;

        public DocumentHelper(Company company, SettingsProvider settingsProvider)
        {
            oCompany = company;
            this.settingsProvider = settingsProvider;
        }
        public double GetCurrRate(string currCode, DateTime date)
        {
            if (currCode == oCompany.GetCompanyService().GetAdminInfo().LocalCurrency)
                return 1;
            SBObob vObj = (SBObob)oCompany.GetBusinessObject(BoObjectTypes.BoBridge);
            var rs = vObj.GetCurrencyRate(currCode, date);
            double result = (double)rs.Fields.Item(0).Value;
            return result;

        }
        public string AddJournalEntry(Company _comp, string creditCode, string debitCode, string creditControlCode, string debitControlCode, double amount, int series, string comment, DateTime DocDate, int BPLID = 235, string currency = "GEL")
        {
            SAPbobsCOM.JournalEntries vJE =
                (SAPbobsCOM.JournalEntries)_comp.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);

            vJE.ReferenceDate = DocDate;
            vJE.DueDate = DocDate;
            vJE.TaxDate = DocDate;
            vJE.Memo = comment.Length < 50 ? comment : comment.Substring(0, 49);

            vJE.Lines.BPLID = BPLID; //branch

            if (currency == "GEL")
            {
                vJE.Lines.Debit = amount;
                vJE.Lines.FCDebit = 0;
            }
            else
            {
                vJE.Lines.FCCurrency = currency;
                vJE.Lines.FCDebit = amount;
            }

            vJE.Lines.Credit = 0;
            vJE.Lines.FCCredit = 0;

            if (string.IsNullOrWhiteSpace(debitCode))
            {
                vJE.Lines.ShortName = debitControlCode;
            }
            else
            {
                vJE.Lines.AccountCode = debitCode;
            }
            vJE.Lines.Add();


            vJE.Lines.BPLID = BPLID;
            if (string.IsNullOrWhiteSpace(creditCode))
            {
                vJE.Lines.ShortName = creditControlCode;
            }
            else
            {
                vJE.Lines.AccountCode = creditCode;
            }
            vJE.Lines.Debit = 0;
            vJE.Lines.FCDebit = 0;
            if (currency == "GEL")
            {
                vJE.Lines.Credit = amount;
                vJE.Lines.FCCredit = 0;
            }
            else
            {
                vJE.Lines.FCCurrency = currency;
                vJE.Lines.FCCredit = amount;
            }

            vJE.Lines.Add();

            int i = vJE.Add();
            if (i == 0)
            {
                string transId = _comp.GetNewObjectKey();
                return transId;
            }
            else
            {
                throw new Exception(_comp.GetLastErrorDescription());
            }
        }


        public string AddJournalEntryFromPayment(Company _comp, Payments doc, double amount)
        {
            Settings settings = settingsProvider.Get();

            SAPbobsCOM.JournalEntries vJE =
                (SAPbobsCOM.JournalEntries)_comp.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries);
            var comment = "IN " + doc.DocNum;
            vJE.ReferenceDate = doc.DocDate;
            vJE.DueDate = doc.DocDate;
            vJE.TaxDate = doc.DocDate;
            vJE.Memo = comment.Length < 50 ? comment : comment.Substring(0, 49);

            vJE.Lines.BPLID = doc.BPLID; //branch

            if (doc.DocCurrency == "GEL")
            {
                vJE.Lines.Debit = amount;
            }
            else
            {
                vJE.Lines.FCCurrency = doc.DocCurrency;
                vJE.Lines.FCDebit = amount;
            }

            vJE.Lines.Credit = 0;
            vJE.Lines.FCCredit = 0;

            if (string.IsNullOrWhiteSpace(settings.PensionAccDr))
            {
                vJE.Lines.ShortName = doc.CardCode;

                if (settings.UseDocControllAcc)
                {
                    vJE.Lines.ControlAccount = doc.ControlAccount;
                }
            }
            else
            {
                vJE.Lines.AccountCode = settings.PensionAccDr;
            }
            vJE.Lines.Add();


            vJE.Lines.BPLID = doc.BPLID;
            if (string.IsNullOrWhiteSpace(settings.PensionAccCr))
            {
                vJE.Lines.ShortName = settings.PensionControlAccCr;
            }
            else
            {
                vJE.Lines.AccountCode = settings.PensionAccCr;
            }
            vJE.Lines.Debit = 0;
            vJE.Lines.FCDebit = 0;
            if (doc.DocCurrency == "GEL")
            {
                vJE.Lines.Credit = amount;
                vJE.Lines.FCCredit = 0;
            }
            else
            {
                vJE.Lines.FCCurrency = doc.DocCurrency;
                vJE.Lines.FCCredit = amount;
            }

            vJE.Lines.Add();

            int i = vJE.Add();
            if (i == 0)
            {
                string transId = _comp.GetNewObjectKey();
                return transId;
            }
            else
            {
                throw new Exception(_comp.GetLastErrorDescription());
            }
        }


        public IEnumerable<Result> PostIncomeTaxFromCreditMemo(string invDocEntry)
        {
            Settings settings = settingsProvider.Get();
            List<Result> results = new List<Result>();
            Documents invoiceDi = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oPurchaseCreditNotes);
            invoiceDi.GetByKey(int.Parse(invDocEntry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;
            Recordset recSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery2(
                $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'");
            bool isIncomeTaxPayer = recSet.Fields.Item("U_IncomeTaxPayer").Value.ToString() == "01";
            bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
            recSet.DoQuery2($"Select * From [@RSM_SERVICE_PARAMS]");
            string incomeTaxAccDr = recSet.Fields.Item("U_IncomeTaxAccDr").Value.ToString();
            string incomeTaxAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();
            string incomeTaxControlAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();
            string incomeTaxOnInvoice = recSet.Fields.Item("U_IncomeTaxOnInvoice").Value.ToString();

            if (!Convert.ToBoolean(incomeTaxOnInvoice))
            {
                results.Add(new Result { IsSuccessCode = false, StatusDescription = "არ არის საშემოსავლოს გადამხდელი" });
                return results;
            }

            BusinessPartners bp =
                (BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            bp.GetByKey(invoiceDi.CardCode);

            var incomeTaxPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_IncomeTaxPayerPercent").Value.ToString(),
                CultureInfo.InstalledUICulture);

            var pensionPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_PensionPayerPercent").Value.ToString());

            for (int i = 0; i < invoiceDi.Lines.Count; i++)
            {
                invoiceDi.Lines.SetCurrentLine(i);
                recSet.DoQuery2(
                    $"SELECT U_PensionLiable FROM OITM WHERE OITM.ItemCode = N'{invoiceDi.Lines.ItemCode}'");
                bool isPensionLiable = recSet.Fields.Item("U_PensionLiable").Value.ToString() == "01";
                bool isFc = invoiceDi.DocCurrency != "GEL";

                double incomeTaxAmount;

                if (!isPensionLiable)
                {
                    double lineTotal = isFc ? invoiceDi.Lines.RowTotalFC : invoiceDi.Lines.LineTotal;
                    incomeTaxAmount = Math.Round(lineTotal * incomeTaxPayerPercent / 100, 6);
                }
                else
                {
                    if (isPensionPayer)
                    {
                        double lineTotal = isFc ? invoiceDi.Lines.RowTotalFC : invoiceDi.Lines.LineTotal;
                        double pensionAmount = Math.Round(lineTotal * pensionPayerPercent / 100, 6);
                        incomeTaxAmount = Math.Round((lineTotal - pensionAmount) * incomeTaxPayerPercent / 100, 6);
                    }
                    else
                    {
                        double lineTotal = isFc ? invoiceDi.Lines.RowTotalFC : invoiceDi.Lines.LineTotal;
                        incomeTaxAmount = Math.Round((lineTotal) * incomeTaxPayerPercent / 100, 6);
                    }

                }

                if (isIncomeTaxPayer && invoiceDi.CancelStatus == CancelStatusEnum.csNo)
                {
                    try
                    {
                        //string incomeTaxPayerTransId = AddJournalEntry(oCompany, incomeTaxAccCr,
                        //    incomeTaxAccDr, incomeTaxControlAccCr, invoiceDi.CardCode, -incomeTaxAmount,
                        //    invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                        //    invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);
                        string incomeTaxPayerTransId = PostIncomeTaxvJEFromDocument(settings, invoiceDi, -incomeTaxAmount);
                        results.Add(new Result
                        {
                            IsSuccessCode = true,
                            StatusDescription = "საშემოსავლოს გატარება კრედიტ მემო",
                            CreatedDocumentEntry = incomeTaxPayerTransId,
                            ObjectType = BoObjectTypes.oJournalEntries
                        });
                    }
                    catch (Exception e)
                    {
                        results.Add(new Result { IsSuccessCode = false, StatusDescription = e.Message });
                        return results;
                    }
                }
                if (isIncomeTaxPayer && invoiceDi.CancelStatus == CancelStatusEnum.csCancellation)
                {
                    try
                    {
                        //string incomeTaxPayerTransId = AddJournalEntry(oCompany, incomeTaxAccCr,
                        //    incomeTaxAccDr, incomeTaxControlAccCr, invoiceDi.CardCode, incomeTaxAmount,
                        //    invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                        //    invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);
                        string incomeTaxPayerTransId = PostIncomeTaxvJEFromDocument(settings, invoiceDi, incomeTaxAmount);

                        results.Add(new Result
                        {
                            IsSuccessCode = true,
                            StatusDescription = "საშემოსავლოს გატარება (მაქენსელებელი) კრედიტ მემო",
                            CreatedDocumentEntry = incomeTaxPayerTransId,
                            ObjectType = BoObjectTypes.oJournalEntries
                        });
                    }
                    catch (Exception e)
                    {
                        results.Add(new Result { IsSuccessCode = false, StatusDescription = e.Message });
                        return results;
                    }
                }
            }
            return results;
        }
        public IEnumerable<Result> PostIncomeTaxFromInvoice(string invDocEntry)
        {
            List<Result> results = new List<Result>();
            Settings settings = settingsProvider.Get();
            if (!settings.IncomeTaxOnInvoice)
            {
                results.Add(new Result { IsSuccessCode = false, StatusDescription = "არ არის საშემოსავლოს გადამხდელი" });
                return results;
            }

            #region vjeDetails
            Documents invoiceDi = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
            invoiceDi.GetByKey(int.Parse(invDocEntry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;
            bool isFc = invoiceDi.DocCurrency != "GEL";

            BusinessPartners bp = (BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            bp.GetByKey(invoiceDi.CardCode);
            bool isIncomeTaxPayer = (string)bp.UserFields.Fields.Item("U_IncomeTaxPayer").Value == "01";
            bool isPensionPayer = (string)bp.UserFields.Fields.Item("U_PensionPayer").Value == "01";
            var incomeTaxPayerPercent = (double)bp.UserFields.Fields.Item("U_IncomeTaxPayerPercent").Value;
            var pensionPayerPercent = (double)bp.UserFields.Fields.Item("U_PensionPayerPercent").Value;

            var oItem = (Items)oCompany.GetBusinessObject(BoObjectTypes.oItems);
            #endregion

            for (int i = 0; i < invoiceDi.Lines.Count; i++)
            {
                invoiceDi.Lines.SetCurrentLine(i);

                oItem.GetByKey(invoiceDi.Lines.ItemCode);
                bool isPensionLiable = oItem.UserFields.Fields.Item("U_PensionLiable").Value.ToString() == "01";

                #region IncomeTaxAmount ის დათვლა
                double incomeTaxAmount;

                if (!isPensionLiable)
                {
                    double lineTotal = isFc ? invoiceDi.Lines.RowTotalFC : invoiceDi.Lines.LineTotal;
                    incomeTaxAmount = Math.Round(lineTotal * incomeTaxPayerPercent / 100, 6);
                }
                else
                {
                    if (isPensionPayer)
                    {
                        double lineTotal = isFc ? invoiceDi.Lines.RowTotalFC : invoiceDi.Lines.LineTotal;
                        double pensionAmount = Math.Round(lineTotal * pensionPayerPercent / 100, 6);
                        incomeTaxAmount = Math.Round((lineTotal - pensionAmount) * incomeTaxPayerPercent / 100, 6);
                    }
                    else
                    {
                        double lineTotal = isFc ? invoiceDi.Lines.RowTotalFC : invoiceDi.Lines.LineTotal;
                        incomeTaxAmount = Math.Round((lineTotal) * incomeTaxPayerPercent / 100, 6);
                    }

                }
                #endregion

                if (isIncomeTaxPayer && invoiceDi.CancelStatus == CancelStatusEnum.csNo)
                {
                    try
                    {
                        string incomeTaxPayerTransId;
                        //string incomeTaxPayerTransId = AddJournalEntry(_company, settings.IncomeTaxAccCr,
                        //    settings.IncomeTaxAccDr, IncomeControlTaxAccCr, invoiceDi.CardCode, incomeTaxAmount,
                        //    invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                        //    invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);

                        incomeTaxPayerTransId = PostIncomeTaxvJEFromDocument(settings, invoiceDi, incomeTaxAmount);

                        results.Add(new Result
                        {
                            IsSuccessCode = true,
                            StatusDescription = "საშემოსავლოს გატარება ინვოისი",
                            CreatedDocumentEntry = incomeTaxPayerTransId,
                            ObjectType = BoObjectTypes.oJournalEntries
                        });
                    }
                    catch (Exception e)
                    {
                        results.Add(new Result { IsSuccessCode = false, StatusDescription = e.Message });
                        return results;
                    }
                }


                if (isIncomeTaxPayer && invoiceDi.CancelStatus == CancelStatusEnum.csCancellation)
                {
                    try
                    {
                        //string incomeTaxPayerTransId = AddJournalEntry(_company, settings.IncomeTaxAccCr,
                        //    settings.IncomeTaxAccDr, invoiceDi.CardCode, invoiceDi.CardCode, -incomeTaxAmount,
                        //    invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                        //    invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);

                        string incomeTaxPayerTransId = PostIncomeTaxvJEFromDocument(settings, invoiceDi, -incomeTaxAmount);
                        results.Add(new Result
                        {
                            IsSuccessCode = true,
                            StatusDescription = "საშემოსავლოს გატარება (მაქენსელებელი) ინვოისი",
                            CreatedDocumentEntry = incomeTaxPayerTransId,
                            ObjectType = BoObjectTypes.oJournalEntries
                        });
                    }
                    catch (Exception e)
                    {
                        results.Add(new Result { IsSuccessCode = false, StatusDescription = e.Message });
                        return results;
                    }
                }

            }
            return results;
        }

        private string PostIncomeTaxvJEFromDocument(Settings settings, Documents invoiceDi, double incomeTaxAmount)
        {
            string incomeTaxPayerTransId;
            JournalEntries vJE = (JournalEntries)oCompany.GetBusinessObject(BoObjectTypes.oJournalEntries);
            vJE.ReferenceDate = invoiceDi.DocDate;
            vJE.DueDate = invoiceDi.DocDate;
            vJE.TaxDate = invoiceDi.DocDate;
            vJE.Memo = invoiceDi.Comments.PadLeft(50).Substring(0, 49);

            #region Line 1
            vJE.Lines.BPLID = invoiceDi.BPL_IDAssignedToInvoice;
            if (invoiceDi.DocCurrency == "GEL")
            {
                vJE.Lines.Debit = incomeTaxAmount;
            }
            else
            {
                vJE.Lines.FCCurrency = invoiceDi.DocCurrency;
                vJE.Lines.FCDebit = incomeTaxAmount;
            }

            if (string.IsNullOrWhiteSpace(settings.IncomeTaxAccDr))
            {
                vJE.Lines.ShortName = invoiceDi.CardCode;

                if (settings.UseDocControllAcc)
                {
                    vJE.Lines.ControlAccount = invoiceDi.ControlAccount;
                }
            }
            else
            {
                vJE.Lines.AccountCode = settings.IncomeTaxAccDr;
            }

            vJE.Lines.Add();
            #endregion


            #region Line 2
            vJE.Lines.BPLID = invoiceDi.BPL_IDAssignedToInvoice;

            if (invoiceDi.DocCurrency == "GEL")
            {
                vJE.Lines.Credit = incomeTaxAmount;
                vJE.Lines.FCCredit = 0;
            }
            else
            {
                vJE.Lines.FCCurrency = invoiceDi.DocCurrency;
                vJE.Lines.FCCredit = incomeTaxAmount;
            }

            if (string.IsNullOrWhiteSpace(settings.IncomeTaxAccCr))
            {
                vJE.Lines.ShortName = settings.IncomeControlTaxAccCr;
            }
            else
            {
                vJE.Lines.AccountCode = settings.IncomeTaxAccCr;
            }

            vJE.Lines.Add();
            #endregion

            var ret = vJE.Add();
            if (ret == 0)
            {
                incomeTaxPayerTransId = oCompany.GetNewObjectKey();
            }
            else
            {
                throw new Exception(oCompany.GetLastErrorDescription());
            }

            return incomeTaxPayerTransId;
        }


        private string PostIncomeTaxvJEFromPaymentDocument(Settings settings, Documents invoiceDi, double incomeTaxAmount, Payments paymentDi)
        {
            string incomeTaxPayerTransId;
            JournalEntries vJE = (JournalEntries)oCompany.GetBusinessObject(BoObjectTypes.oJournalEntries);
            vJE.ReferenceDate = invoiceDi.DocDate;
            vJE.DueDate = invoiceDi.DocDate;
            vJE.TaxDate = invoiceDi.DocDate;
            vJE.Memo = invoiceDi.Comments.PadLeft(50).Substring(0, 49);

            #region Line 1
            vJE.Lines.BPLID = invoiceDi.BPL_IDAssignedToInvoice;
            if (paymentDi.DocCurrency == "GEL")
            {
                vJE.Lines.Debit = incomeTaxAmount;
            }
            else
            {
                vJE.Lines.FCCurrency = paymentDi.DocCurrency;
                vJE.Lines.FCDebit = incomeTaxAmount;
            }

            if (string.IsNullOrWhiteSpace(settings.IncomeTaxAccDr))
            {
                vJE.Lines.ShortName = invoiceDi.CardCode;

                if (settings.UseDocControllAcc)
                {
                    vJE.Lines.ControlAccount = invoiceDi.ControlAccount;
                }
            }
            else
            {
                vJE.Lines.AccountCode = settings.IncomeTaxAccDr;
            }

            vJE.Lines.Add();
            #endregion


            #region Line 2
            vJE.Lines.BPLID = invoiceDi.BPL_IDAssignedToInvoice;

            if (paymentDi.DocCurrency == "GEL")
            {
                vJE.Lines.Credit = incomeTaxAmount;
                vJE.Lines.FCCredit = 0;
            }
            else
            {
                vJE.Lines.FCCurrency = paymentDi.DocCurrency;
                vJE.Lines.FCCredit = incomeTaxAmount;
            }

            if (string.IsNullOrWhiteSpace(settings.IncomeTaxAccCr))
            {
                vJE.Lines.ShortName = settings.IncomeControlTaxAccCr;
            }
            else
            {
                vJE.Lines.AccountCode = settings.IncomeTaxAccCr;
            }

            vJE.Lines.Add();
            #endregion

            var ret = vJE.Add();
            if (ret == 0)
            {
                incomeTaxPayerTransId = oCompany.GetNewObjectKey();
            }
            else
            {
                throw new Exception(oCompany.GetLastErrorDescription());
            }

            return incomeTaxPayerTransId;
        }

        public string PostPensionvJEFromInvoice(Settings settings, Documents invoiceDI, double pensionAmountPaymentOnAccount)
        {
            JournalEntries vJE = (JournalEntries)oCompany.GetBusinessObject(BoObjectTypes.oJournalEntries);
            var comment = "IN " + invoiceDI.DocNum;
            vJE.ReferenceDate = invoiceDI.DocDate;
            vJE.DueDate = invoiceDI.DocDate;
            vJE.TaxDate = invoiceDI.DocDate;
            vJE.Memo = comment.Length < 50 ? comment : comment.Substring(0, 49);
            vJE.Lines.BPLID = invoiceDI.BPL_IDAssignedToInvoice;
            if (invoiceDI.DocCurrency == "GEL")
            {
                vJE.Lines.Debit = pensionAmountPaymentOnAccount;
            }
            else
            {
                vJE.Lines.FCCurrency = invoiceDI.DocCurrency;
                vJE.Lines.FCDebit = pensionAmountPaymentOnAccount;
            }

            vJE.Lines.ShortName = invoiceDI.CardCode;

            if (settings.UseDocControllAcc)
            {
                vJE.Lines.ControlAccount = invoiceDI.ControlAccount;
            }


            vJE.Lines.Add();
            vJE.Lines.BPLID = invoiceDI.BPL_IDAssignedToInvoice;

            if (invoiceDI.DocCurrency == "GEL")
            {
                vJE.Lines.Credit = pensionAmountPaymentOnAccount;
                vJE.Lines.FCCredit = 0;
            }
            else
            {
                vJE.Lines.FCCurrency = invoiceDI.DocCurrency;
                vJE.Lines.FCCredit = pensionAmountPaymentOnAccount;
            }
            if (string.IsNullOrWhiteSpace(settings.PensionAccCr))
            {
                vJE.Lines.ShortName = settings.PensionControlAccCr;
            }
            else
            {
                vJE.Lines.AccountCode = settings.PensionAccCr;
            }
            vJE.Lines.Add();
            string transId = "";
            var ret = vJE.Add();
            if (ret == 0)
            {
                transId = oCompany.GetNewObjectKey();
            }
            else
            {
                throw new Exception(oCompany.GetLastErrorDescription());
            }
            return transId;
        }

        public IEnumerable<Result> PostIncomeTaxFromOutgoing(string invDocEntry)
        {
            List<Result> results = new List<Result>();
            Documents invoiceDi = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
            invoiceDi.GetByKey(int.Parse(invDocEntry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;

            Recordset recSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);


            recSet.DoQuery2(
                $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'");

            bool isIncomeTaxPayer = recSet.Fields.Item("U_IncomeTaxPayer").Value.ToString() == "01";
            bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
            recSet.DoQuery2($"Select * From [@RSM_SERVICE_PARAMS]");
            string incomeTaxAccDr = recSet.Fields.Item("U_IncomeTaxAccDr").Value.ToString();
            string incomeTaxAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();
            string incomeTaxControlAccCr = recSet.Fields.Item("U_IncomeTaxAccCr").Value.ToString();
            string incomeTaxOnInvoice = recSet.Fields.Item("U_IncomeTaxOnInvoice").Value.ToString();

            if (!Convert.ToBoolean(incomeTaxOnInvoice))
            {
                results.Add(new Result { IsSuccessCode = false, StatusDescription = "საშემოსავლოს გატარება ადახდაზე ალამი არ არის მონიშნული" });
                return results;
            }

            SAPbobsCOM.BusinessPartners bp =
                (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            bp.GetByKey(invoiceDi.CardCode);

            var incomeTaxPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_IncomeTaxPayerPercent").Value.ToString(),
                CultureInfo.InstalledUICulture);

            var pensionPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_PensionPayerPercent").Value.ToString());

            for (int i = 0; i < invoiceDi.Lines.Count; i++)
            {
                invoiceDi.Lines.SetCurrentLine(i);
                recSet.DoQuery2(
                    $"SELECT U_PensionLiable FROM OITM WHERE OITM.ItemCode = N'{invoiceDi.Lines.ItemCode}'");
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
                        string incomeTaxPayerTransId = AddJournalEntry(oCompany, incomeTaxAccCr,
                            incomeTaxAccDr, incomeTaxControlAccCr, invoiceDi.CardCode, incomeTaxAmount,
                            invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                            invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);
                        results.Add(new Result
                        {
                            IsSuccessCode = true,
                            StatusDescription = "საშემოსავლოს გატარება გამავალი გადახდა",
                            CreatedDocumentEntry = incomeTaxPayerTransId,
                            ObjectType = BoObjectTypes.oJournalEntries
                        });
                    }
                    catch (Exception e)
                    {
                        results.Add(new Result { IsSuccessCode = false, StatusDescription = e.Message });
                        return results;
                    }
                }
                if (isIncomeTaxPayer && invoiceDi.CancelStatus == CancelStatusEnum.csCancellation)
                {
                    try
                    {
                        string incomeTaxPayerTransId = AddJournalEntry(oCompany, incomeTaxAccCr,
                            incomeTaxAccDr, incomeTaxControlAccCr, invoiceDi.CardCode, -incomeTaxAmount,
                            invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                            invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);

                        results.Add(new Result
                        {
                            IsSuccessCode = true,
                            StatusDescription = "საშემოსავლოს გატარება გამავალი გადახდა დაქენესელებული",
                            CreatedDocumentEntry = incomeTaxPayerTransId,
                            ObjectType = BoObjectTypes.oJournalEntries
                        });
                    }
                    catch (Exception e)
                    {
                        results.Add(new Result { IsSuccessCode = false, StatusDescription = e.Message });
                        return results;
                    }
                }

            }
            return results;
        }
        public IEnumerable<Result> PostPension(string invDocEntry)
        {
            List<Result> results = new List<Result>();
            Documents invoiceDi = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
            invoiceDi.GetByKey(int.Parse(invDocEntry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;

            Recordset recSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);


            recSet.DoQuery2(
                $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'");

            bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";

            recSet.DoQuery2($"Select * From [@RSM_SERVICE_PARAMS]");
            string pensionAccDr = recSet.Fields.Item("U_PensionAccDr").Value.ToString();
            string pensionAccCr = recSet.Fields.Item("U_PensionAccCr").Value.ToString();
            string pensionControlAccDr = recSet.Fields.Item("U_PensionControlAccDr").Value.ToString();
            string pensionControlAccCr = recSet.Fields.Item("U_PensionControlAccCr").Value.ToString();

            SAPbobsCOM.BusinessPartners bp =
                (SAPbobsCOM.BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            bp.GetByKey(invoiceDi.CardCode);

            var incomeTaxPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_IncomeTaxPayerPercent").Value.ToString(),
                CultureInfo.InstalledUICulture);

            var pensionPayerPercent = double.Parse(bp.UserFields.Fields.Item("U_PensionPayerPercent").Value.ToString());

            for (int i = 0; i < invoiceDi.Lines.Count; i++)
            {
                invoiceDi.Lines.SetCurrentLine(i);
                recSet.DoQuery2(
                    $"SELECT U_PensionLiable FROM OITM WHERE OITM.ItemCode = N'{invoiceDi.Lines.ItemCode}'");
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
                        string incometaxpayertransidcomp = AddJournalEntry(oCompany,
                            pensionAccCr, pensionAccDr, pensionControlAccCr, pensionControlAccDr, pensionAmount,
                            invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                            invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);
                        results.Add(new Result
                        {
                            IsSuccessCode = true,
                            StatusDescription = "საპენსიოს გატარება ინვოისი",
                            CreatedDocumentEntry = incometaxpayertransidcomp,
                            ObjectType = BoObjectTypes.oJournalEntries
                        });
                    }
                    catch (Exception e)
                    {
                        results.Add(new Result { IsSuccessCode = false, StatusDescription = e.Message });
                        return results;
                    }

                    try
                    {
                        string incometaxpayertransid = AddJournalEntry(oCompany, pensionAccCr,
                            "", pensionControlAccCr, invoiceDi.CardCode, pensionAmount, invoiceDi.Series,
                            invoiceDi.Comments, invoiceDi.DocDate, invoiceDi.BPL_IDAssignedToInvoice,
                            invoiceDi.DocCurrency);
                        results.Add(new Result
                        {
                            IsSuccessCode = true,
                            StatusDescription = "საპენსიოს გატარება ინვოისი",
                            CreatedDocumentEntry = incometaxpayertransid,
                            ObjectType = BoObjectTypes.oJournalEntries
                        });
                    }
                    catch (Exception e)
                    {

                        results.Add(new Result { IsSuccessCode = false, StatusDescription = e.Message });
                        return results;
                    }
                }
                else
                {
                    results.Add(new Result { IsSuccessCode = false, StatusDescription = "არ არის საპენსიოს გადამხდელი" });
                    return results;
                }
            }
            return results;
        }

        public void OnPaymentAdd(string invDocEnttry, bool postIncomeTax)
        {
            var settings = settingsProvider.Get();
            //var invObjectString = pVal.ObjectKey;
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(invObjectString);
            //string invDocEnttry = string.Empty;
            //try
            //{
            //    invDocEnttry = xmlDoc.GetElementsByTagName("DocEntry").Item(0).InnerText;
            //}
            //catch (Exception e)
            //{
            //    SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage("Invalid Document Number",
            //        BoMessageTime.bmt_Short, true);
            //}

            Payments outgoingPaymentDi = (Payments)oCompany.GetBusinessObject(BoObjectTypes.oVendorPayments);
            outgoingPaymentDi.GetByKey(int.Parse(invDocEnttry, CultureInfo.InvariantCulture));
            Recordset recSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            string bpCode = outgoingPaymentDi.CardCode;
            recSet.DoQuery2($"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'");
            bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
            bool isIncomeTaxPayer = recSet.Fields.Item("U_IncomeTaxPayer").Value.ToString() == "01";
            recSet.DoQuery($"Select * From [@RSM_SERVICE_PARAMS]");
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
                CurrencyDecimalSeparator = oCompany.GetCompanyService().GetAdminInfo().DecimalSeparator,
                CurrencyGroupSeparator = oCompany.GetCompanyService().GetAdminInfo().ThousandsSeparator
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
                                string transId = AddJournalEntry(oCompany,
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
                            SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                        }

                        try
                        {
                            if (isPensionPayer)
                            {
                                //string transId = string.Empty;
                                //if (oldMethod)
                                //{
                                //    transId = DocumentHelper.AddJournalEntry(oCompany,
                                //    pensionAccCr,
                                //    "",
                                //    pensionControlAccCr,
                                //    outgoingPaymentDi.CardCode,
                                //    pensionAmountPaymentOnAccount,
                                //    outgoingPaymentDi.Series,
                                //    "OP " + outgoingPaymentDi.DocNum,
                                //    outgoingPaymentDi.DocDate,
                                //    outgoingPaymentDi.BPLID,
                                //    outgoingPaymentDi.DocCurrency);
                                //}
                                //else
                                string transId = PostvJEFromPayment(settings, outgoingPaymentDi, pensionAmountPaymentOnAccount);

                            }
                        }
                        catch (Exception e)
                        {
                            SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
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


                        //string transId2 = AddJournalEntry(oCompany,
                        //    incomeTaxAccCr,
                        //    "",
                        //    pensionControlAccCr,
                        //    outgoingPaymentDi.CardCode,
                        //    incomeTaxAmountPaymentOnAccount,
                        //    outgoingPaymentDi.Series,
                        //    "OP " + outgoingPaymentDi.DocNum,
                        //    outgoingPaymentDi.DocDate,
                        //    outgoingPaymentDi.BPLID,
                        //    outgoingPaymentDi.DocCurrency);
                       
                            string transId2 = PostIncomeTaxFromPayment(settings, outgoingPaymentDi, incomeTaxAmountPaymentOnAccount);
                        

                    }
                }
            }

            for (int i = 0; i < outgoingPaymentDi.Invoices.Count; i++)
            {

                outgoingPaymentDi.Invoices.SetCurrentLine(i);

                if (outgoingPaymentDi.Invoices.InvoiceType == BoRcptInvTypes.it_PurchaseInvoice)
                {
                    Documents invoiceDi = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
                    if (outgoingPaymentDi.Invoices.DocEntry == 0)
                    {
                        continue;
                    }
                    invoiceDi.GetByKey(outgoingPaymentDi.Invoices.DocEntry);
                    double pensionAmount = invoiceDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02, 6);

                    if (!isIncomeTaxPayer)
                    {
                        pensionAmount = invoiceDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.98 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.98 * 0.02,
                            6);
                    }
                   
                    if (outgoingPaymentDi.DocCurrency != invoiceDi.DocCurrency && outgoingPaymentDi.DocCurrency == oCompany.GetCompanyService().GetAdminInfo().LocalCurrency)
                    {
                        pensionAmount = pensionAmount * GetCurrRate(invoiceDi.DocCurrency, outgoingPaymentDi.DocDate); 
                    }

                    try
                    {
                        if (isPensionPayer)
                        {
                            string incometaxpayertransidcomp = AddJournalEntry(oCompany,
                                pensionAccCr,
                                pensionAccDr,
                                pensionControlAccCr,
                                pensionControlAccDr,
                                pensionAmount,
                                invoiceDi.Series,
                                "IN " + invoiceDi.DocNum,
                                invoiceDi.DocDate,
                                invoiceDi.BPL_IDAssignedToInvoice,
                                outgoingPaymentDi.DocCurrency);
                        }
                    }
                    catch (Exception e)
                    {
                        SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                    }

                    try
                    {
                        if (isPensionPayer)
                        {
                            //string incometaxpayertransid = DocumentHelper.AddJournalEntry(oCompany,
                            //    pensionAccCr,
                            //    "",
                            //    pensionControlAccCr,
                            //    invoiceDi.CardCode,
                            //    pensionAmount,
                            //    invoiceDi.Series,
                            //    "IN " + invoiceDi.DocNum,
                            //    invoiceDi.DocDate,
                            //    invoiceDi.BPL_IDAssignedToInvoice,
                            //    invoiceDi.DocCurrency);


                            string incometaxpayertransid = PostvJEFromPaymentInvoce(settings, invoiceDi, pensionAmount, outgoingPaymentDi);

                        }
                    }
                    catch (Exception e)
                    {
                        SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                    }

                    //legacy code
                    //for (int j = 0; j < invoiceDi.Lines.Count; j++)
                    //{
                    //    invoiceDi.Lines.SetCurrentLine(j);
                    //    recSet.DoQuery2($"SELECT U_PensionLiable FROM OITM WHERE OITM.ItemCode = N'{invoiceDi.Lines.ItemCode}'");


                    //    if (invoiceDi.DocType != BoDocumentTypes.dDocument_Service)
                    //    {
                    //        bool isPensionLiable = recSet.Fields.Item("U_PensionLiable").Value.ToString() == "01";

                    //        if (!isPensionLiable)
                    //        {
                    //            continue;
                    //        }
                    //    }

                    //    double pensionAmount = invoiceDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02, 6);

                    //    if (!isIncomeTaxPayer)
                    //    {
                    //        pensionAmount = invoiceDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.98 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.98 * 0.02,
                    //            6);
                    //    }

                    //    try
                    //    {
                    //        if (isPensionPayer)
                    //        {
                    //            string incometaxpayertransidcomp = AddJournalEntry(oCompany,
                    //                pensionAccCr,
                    //                pensionAccDr,
                    //                pensionControlAccCr,
                    //                pensionControlAccDr,
                    //                pensionAmount,
                    //                invoiceDi.Series,
                    //                "IN " + invoiceDi.DocNum,
                    //                invoiceDi.DocDate,
                    //                invoiceDi.BPL_IDAssignedToInvoice,
                    //                invoiceDi.DocCurrency);
                    //        }
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                    //    }

                    //    try
                    //    {
                    //        if (isPensionPayer)
                    //        {
                    //            //string incometaxpayertransid = DocumentHelper.AddJournalEntry(oCompany,
                    //            //    pensionAccCr,
                    //            //    "",
                    //            //    pensionControlAccCr,
                    //            //    invoiceDi.CardCode,
                    //            //    pensionAmount,
                    //            //    invoiceDi.Series,
                    //            //    "IN " + invoiceDi.DocNum,
                    //            //    invoiceDi.DocDate,
                    //            //    invoiceDi.BPL_IDAssignedToInvoice,
                    //            //    invoiceDi.DocCurrency);


                    //            string incometaxpayertransid = PostvJEFromPaymentInvoce(settings, invoiceDi, pensionAmount);

                    //        }
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                    //    }


                    //}

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
                            string incometaxpayertransidcomp = AddJournalEntry(oCompany,
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
                        SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                    }

                    try
                    {
                        if (isPensionPayer)
                        {
                            string incometaxpayertransid = AddJournalEntry(oCompany,
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
                        SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                    }
                }
                if (isIncomeTaxPayer && !incomeTaxOnInvoice && postIncomeTax)
                {
                    if (outgoingPaymentDi.Invoices.SumApplied == 0)
                    {
                        continue;
                    }
                    Recordset recSet1 = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                    recSet1.DoQuery($"select DocRate from  VPM2 where DocNum = {outgoingPaymentDi.DocEntry} and InvoiceId = {i}");
                    var rate = (double)recSet1.Fields.Item(0).Value;
                    //double pensionAmount2 = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02, 6);
                    //double taxPayerAmount = outgoingPaymentDi.DocCurrency != "GEL" ? (outgoingPaymentDi.Invoices.AppliedFC / 0.784 - pensionAmount2) * 0.2 : (outgoingPaymentDi.Invoices.SumApplied / 0.784 - pensionAmount2) * 0.2;
                    //if (!isPensionPayer)
                    //{
                    //    taxPayerAmount = outgoingPaymentDi.DocCurrency != "GEL" ? outgoingPaymentDi.Invoices.AppliedFC / 0.8 * 0.2 : outgoingPaymentDi.Invoices.SumApplied / 0.8 * 0.2;
                    //}
                    double pensionAmount2 = rate != 0 ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02, 6);
                    double taxPayerAmount = rate != 0 ? (outgoingPaymentDi.Invoices.AppliedFC / 0.784 - pensionAmount2) * 0.2 : (outgoingPaymentDi.Invoices.SumApplied / 0.784 - pensionAmount2) * 0.2;
                    if (!isPensionPayer)
                    {
                        taxPayerAmount = rate != 0 ? outgoingPaymentDi.Invoices.AppliedFC / 0.8 * 0.2 : outgoingPaymentDi.Invoices.SumApplied / 0.8 * 0.2;
                    }
                    if (rate!=0)
                    {
                        taxPayerAmount = taxPayerAmount  * rate;
                    }
                    if (outgoingPaymentDi.Invoices.InvoiceType == BoRcptInvTypes.it_PurchaseInvoice)
                    {
                        Documents invoiceDi = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
                        if (outgoingPaymentDi.Invoices.DocEntry == 0)
                        {
                            continue;
                        }
                        invoiceDi.GetByKey(outgoingPaymentDi.Invoices.DocEntry);
                        string incometaxpayertransid = PostIncomeTaxvJEFromPaymentDocument (settings, invoiceDi, taxPayerAmount, outgoingPaymentDi);
                    }
                    else
                    {
                        
                        
                            string incometaxpayertransid = AddJournalEntry(oCompany,
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

                    //string incometaxpayertransid = PostIncomeTaxvJEFromDocument(settings, invoiceDi, taxPayerAmount)

                }
            }

        }

        private string PostIncomeTaxFromPayment(Settings settings, Payments outgoingPaymentDi, double incomeTaxAmountPaymentOnAccount)
        {
            string transId2;
            var comment = "OP " + outgoingPaymentDi.DocNum;
            JournalEntries vJE = (JournalEntries)oCompany.GetBusinessObject(BoObjectTypes.oJournalEntries);
            vJE.ReferenceDate = outgoingPaymentDi.DocDate;
            vJE.DueDate = outgoingPaymentDi.DocDate;
            vJE.TaxDate = outgoingPaymentDi.DocDate;
            vJE.Memo = comment.Length < 50 ? comment : comment.Substring(0, 49);

            #region Line 1
            vJE.Lines.BPLID = outgoingPaymentDi.BPLID;
            if (outgoingPaymentDi.DocCurrency == "GEL")
            {
                vJE.Lines.Debit = incomeTaxAmountPaymentOnAccount;
            }
            else
            {
                vJE.Lines.FCCurrency = outgoingPaymentDi.DocCurrency;
                vJE.Lines.FCDebit = incomeTaxAmountPaymentOnAccount;
            }

            if (string.IsNullOrWhiteSpace(settings.IncomeTaxAccDr))
            {
                vJE.Lines.ShortName = outgoingPaymentDi.CardCode;

                if (settings.UseDocControllAcc)
                {
                    vJE.Lines.ControlAccount = outgoingPaymentDi.ControlAccount;
                }
            }
            else
            {
                vJE.Lines.AccountCode = settings.IncomeTaxAccDr;
            }

            vJE.Lines.Add();
            #endregion


            #region Line 2
            vJE.Lines.BPLID = outgoingPaymentDi.BPLID;

            if (outgoingPaymentDi.DocCurrency == "GEL")
            {
                vJE.Lines.Credit = incomeTaxAmountPaymentOnAccount;
                vJE.Lines.FCCredit = 0;
            }
            else
            {
                vJE.Lines.FCCurrency = outgoingPaymentDi.DocCurrency;
                vJE.Lines.FCCredit = incomeTaxAmountPaymentOnAccount;
            }

            if (string.IsNullOrWhiteSpace(settings.IncomeTaxAccCr))
            {
                vJE.Lines.ShortName = settings.IncomeControlTaxAccCr;
            }
            else
            {
                vJE.Lines.AccountCode = settings.IncomeTaxAccCr;
            }

            vJE.Lines.Add();
            #endregion

            var ret = vJE.Add();
            if (ret == 0)
            {
                transId2 = oCompany.GetNewObjectKey();
            }
            else
            {
                throw new Exception(oCompany.GetLastErrorDescription());
            }
            return transId2;
        }

        public void OnPaymentUpdate(string invDocEnttry)
        {
            var settings = settingsProvider.Get();

            //var invObjectString = pVal.ObjectKey;
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.LoadXml(invObjectString);
            //string invDocEnttry = string.Empty;
            //try
            //{
            //    invDocEnttry = xmlDoc.GetElementsByTagName("DocEntry").Item(0).InnerText;
            //}
            //catch (Exception e)
            //{
            //    SAPbouiCOM.Framework.Application.SBO_Application.SetStatusBarMessage("Invalid Document Number",
            //        BoMessageTime.bmt_Short);
            //}

            Payments outgoingPaymentDi =
                (Payments)oCompany.GetBusinessObject(BoObjectTypes.oVendorPayments);
            outgoingPaymentDi.GetByKey(int.Parse(invDocEnttry,
                CultureInfo.InvariantCulture));
            if (outgoingPaymentDi.Cancelled == BoYesNoEnum.tYES)
            {
                Recordset recSet = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                string bpCode = outgoingPaymentDi.CardCode;
                recSet.DoQuery2(
                    $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'");
                bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";
                bool isIncomeTaxPayer = recSet.Fields.Item("U_IncomeTaxPayer").Value.ToString() == "01";
                recSet.DoQuery2($"Select * From [@RSM_SERVICE_PARAMS]");
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
                    CurrencyDecimalSeparator = oCompany.GetCompanyService().GetAdminInfo().DecimalSeparator,
                    CurrencyGroupSeparator = oCompany.GetCompanyService().GetAdminInfo().ThousandsSeparator
                };
                var x = outgoingPaymentDi.GetAsXML();
                XmlDocument xmlDoc2 = new XmlDocument();
                xmlDoc2.LoadXml(x);
                string paymentOnAcc = xmlDoc2.GetElementsByTagName("NoDocSum").Item(0).InnerText;


                if (outgoingPaymentDi.DocCurrency != "GEL")
                {
                    string paymentOnAccFc = xmlDoc2.GetElementsByTagName("NoDocSumFC").Item(0).InnerText;
                    paymentOnAcc = paymentOnAccFc;
                }

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
                                    string transId = AddJournalEntry(oCompany,
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
                                SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                            }

                            try
                            {
                                if (isPensionPayer)
                                {
                                    //string transId = DocumentHelper.AddJournalEntry(oCompany,
                                    //    pensionAccCr,
                                    //    "",
                                    //    pensionControlAccCr,
                                    //    outgoingPaymentDi.CardCode,
                                    //    -pensionAmountPaymentOnAccount,
                                    //    outgoingPaymentDi.Series,
                                    //    "OP " + outgoingPaymentDi.DocNum,
                                    //    outgoingPaymentDi.DocDate,
                                    //    outgoingPaymentDi.BPLID,
                                    //    outgoingPaymentDi.DocCurrency);
                                    string transId = PostvJEFromPayment(settings, outgoingPaymentDi, -pensionAmountPaymentOnAccount);
                                }
                            }
                            catch (Exception e)
                            {
                                SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                            }
                        }

                        if (isIncomeTaxPayer && !incomeTaxOnInvoice)
                        {
                            if (!isPensionPayer)
                            {
                                incomeTaxAmountPaymentOnAccount = Math.Round(double.Parse(paymentOnAcc) / 0.8 * 0.2,
                                    6);
                            }

                            string transId2 = AddJournalEntry(oCompany,
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
                            (Documents)oCompany.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
                        if (outgoingPaymentDi.Invoices.DocEntry == 0)
                        {
                            continue;
                        }

                        invoiceDi.GetByKey(outgoingPaymentDi.Invoices.DocEntry);



                        double pensionAmount = invoiceDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02, 6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02, 6);

                        if (!isIncomeTaxPayer)
                        {
                            pensionAmount = invoiceDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.98 * 0.02,
                                6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.98 * 0.02,
                                6);
                        }
                        if (outgoingPaymentDi.DocCurrency != invoiceDi.DocCurrency && outgoingPaymentDi.DocCurrency == oCompany.GetCompanyService().GetAdminInfo().LocalCurrency)
                        {
                            pensionAmount = pensionAmount * GetCurrRate(invoiceDi.DocCurrency, outgoingPaymentDi.DocDate);
                        }



                        try
                        {
                            if (isPensionPayer)
                            {
                                string incometaxpayertransidcomp = AddJournalEntry(oCompany,
                                    pensionAccCr,
                                    pensionAccDr,
                                    pensionControlAccCr,
                                    pensionControlAccDr,
                                    -pensionAmount,
                                    invoiceDi.Series,
                                    "IN " + invoiceDi.DocNum,
                                    invoiceDi.DocDate,
                                    invoiceDi.BPL_IDAssignedToInvoice,
                                    outgoingPaymentDi.DocCurrency);
                            }
                        }
                        catch (Exception e)
                        {
                            SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                        }

                        try
                        {
                            if (isPensionPayer)
                            {
                                //string incometaxpayertransid = DocumentHelper.AddJournalEntry(oCompany,
                                //    pensionAccCr,
                                //    "",
                                //    pensionControlAccCr,
                                //    invoiceDi.CardCode,
                                //    -pensionAmount,
                                //    invoiceDi.Series,
                                //    "IN " + invoiceDi.DocNum,
                                //    invoiceDi.DocDate,
                                //    invoiceDi.BPL_IDAssignedToInvoice,
                                //    invoiceDi.DocCurrency);
                                string incometaxpayertransid = PostvJEFromPaymentInvoce(settings, invoiceDi, -pensionAmount, outgoingPaymentDi);
                            }
                        }
                        catch (Exception e)
                        {
                            SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
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
                                string incometaxpayertransidcomp = AddJournalEntry(oCompany,
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
                            SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                        }

                        try
                        {
                            if (isPensionPayer)
                            {
                                string incometaxpayertransid = AddJournalEntry(oCompany,
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
                            SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(e.Message);
                        }
                    }

                    if (isIncomeTaxPayer && !incomeTaxOnInvoice)
                    {
                        if (outgoingPaymentDi.Invoices.SumApplied == 0)
                        {
                            continue;
                        }
                        Recordset recSet1 = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                        recSet1.DoQuery($"select DocRate from  VPM2 where DocNum = {outgoingPaymentDi.DocEntry} and InvoiceId = {i}");
                        var rate = (double)recSet1.Fields.Item(0).Value;
                        //double pensionAmount2 = outgoingPaymentDi.DocCurrency != "GEL" ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02,
                        //    6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02,
                        //    6);



                        //double taxPayerAmount = outgoingPaymentDi.DocCurrency != "GEL" ? (outgoingPaymentDi.Invoices.AppliedFC / 0.784 - pensionAmount2) * 0.2 : (outgoingPaymentDi.Invoices.SumApplied / 0.784 - pensionAmount2) * 0.2;
                        //if (!isPensionPayer)
                        //{
                        //    taxPayerAmount = outgoingPaymentDi.DocCurrency != "GEL" ?
                        //        outgoingPaymentDi.Invoices.AppliedFC / 0.8 * 0.2 : outgoingPaymentDi.Invoices.SumApplied / 0.8 * 0.2;
                        //}
                        double pensionAmount2 = rate != 0 ? Math.Round(outgoingPaymentDi.Invoices.AppliedFC / 0.784 * 0.02,
                            6) : Math.Round(outgoingPaymentDi.Invoices.SumApplied / 0.784 * 0.02,
                            6);



                        double taxPayerAmount = rate != 0 ? (outgoingPaymentDi.Invoices.AppliedFC / 0.784 - pensionAmount2) * 0.2 : (outgoingPaymentDi.Invoices.SumApplied / 0.784 - pensionAmount2) * 0.2;
                        if (!isPensionPayer)
                        {
                            taxPayerAmount = rate != 0 ?
                                outgoingPaymentDi.Invoices.AppliedFC / 0.8 * 0.2 : outgoingPaymentDi.Invoices.SumApplied / 0.8 * 0.2;
                        }

                        if (rate != 0)
                        {
                            taxPayerAmount = taxPayerAmount * rate;
                        }

                        string incometaxpayertransid = AddJournalEntry(oCompany,
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


        private string PostvJEFromPayment(ServiceJournalEntryLogic.Models.Settings settings, Payments outgoingPaymentDi, double pensionAmountPaymentOnAccount)
        {
            JournalEntries vJE = (JournalEntries)oCompany.GetBusinessObject(BoObjectTypes.oJournalEntries);
            var comment = "OP " + outgoingPaymentDi.DocNum;
            vJE.ReferenceDate = outgoingPaymentDi.DocDate;
            vJE.DueDate = outgoingPaymentDi.DocDate;
            vJE.TaxDate = outgoingPaymentDi.DocDate;
            vJE.Memo = comment.Length < 50 ? comment : comment.Substring(0, 49);
            vJE.Lines.BPLID = outgoingPaymentDi.BPLID;
            if (outgoingPaymentDi.DocCurrency == "GEL")
            {
                vJE.Lines.Debit = pensionAmountPaymentOnAccount;
            }
            else
            {
                vJE.Lines.FCCurrency = outgoingPaymentDi.DocCurrency;
                vJE.Lines.FCDebit = pensionAmountPaymentOnAccount;
            }

            vJE.Lines.ShortName = outgoingPaymentDi.CardCode;

            if (settings.UseDocControllAcc)
            {
                vJE.Lines.ControlAccount = outgoingPaymentDi.ControlAccount;
            }


            vJE.Lines.Add();
            vJE.Lines.BPLID = outgoingPaymentDi.BPLID;

            if (outgoingPaymentDi.DocCurrency == "GEL")
            {
                vJE.Lines.Credit = pensionAmountPaymentOnAccount;
                vJE.Lines.FCCredit = 0;
            }
            else
            {
                vJE.Lines.FCCurrency = outgoingPaymentDi.DocCurrency;
                vJE.Lines.FCCredit = pensionAmountPaymentOnAccount;
            }
            if (string.IsNullOrWhiteSpace(settings.PensionAccCr))
            {
                vJE.Lines.ShortName = settings.PensionControlAccCr;
            }
            else
            {
                vJE.Lines.AccountCode = settings.PensionAccCr;
            }
            vJE.Lines.Add();
            string transId = "";
            var ret = vJE.Add();
            if (ret == 0)
            {
                transId = oCompany.GetNewObjectKey();
            }
            else
            {
                throw new Exception(oCompany.GetLastErrorDescription());
            }
            return transId;
        }
        private string PostvJEFromPaymentInvoce(ServiceJournalEntryLogic.Models.Settings settings, Documents invoiceDI, double pensionAmountPaymentOnAccount, Payments paymentDi)
        {
            JournalEntries vJE = (JournalEntries)oCompany.GetBusinessObject(BoObjectTypes.oJournalEntries);
            var comment = "IN " + invoiceDI.DocNum;
            vJE.ReferenceDate = invoiceDI.DocDate;
            vJE.DueDate = invoiceDI.DocDate;
            vJE.TaxDate = invoiceDI.DocDate;
            vJE.Memo = comment.Length < 50 ? comment : comment.Substring(0, 49);
            vJE.Lines.BPLID = invoiceDI.BPL_IDAssignedToInvoice;
            if (paymentDi.DocCurrency == "GEL")
            {
                vJE.Lines.Debit = pensionAmountPaymentOnAccount;
            }
            else
            {
                vJE.Lines.FCCurrency = paymentDi.DocCurrency;
                vJE.Lines.FCDebit = pensionAmountPaymentOnAccount;
            }

            vJE.Lines.ShortName = invoiceDI.CardCode;

            if (settings.UseDocControllAcc)
            {
                vJE.Lines.ControlAccount = invoiceDI.ControlAccount;
            }


            vJE.Lines.Add();
            vJE.Lines.BPLID = invoiceDI.BPL_IDAssignedToInvoice;

            if (paymentDi.DocCurrency == "GEL")
            {
                vJE.Lines.Credit = pensionAmountPaymentOnAccount;
                vJE.Lines.FCCredit = 0;
            }
            else
            {
                vJE.Lines.FCCurrency = paymentDi.DocCurrency;
                vJE.Lines.FCCredit = pensionAmountPaymentOnAccount;
            }
            if (string.IsNullOrWhiteSpace(settings.PensionAccCr))
            {
                vJE.Lines.ShortName = settings.PensionControlAccCr;
            }
            else
            {
                vJE.Lines.AccountCode = settings.PensionAccCr;
            }
            vJE.Lines.Add();
            string transId = "";
            var ret = vJE.Add();
            if (ret == 0)
            {
                transId = oCompany.GetNewObjectKey();
            }
            else
            {
                throw new Exception(oCompany.GetLastErrorDescription());
            }
            return transId;
        }
    }
}
