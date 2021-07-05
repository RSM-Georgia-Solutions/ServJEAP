﻿using RSM.Core.SDK.DI.Extension;
using SAPbobsCOM;
using ServiceJournalEntryLogic.Models;
using ServiceJournalEntryLogic.Providers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ServiceJournalEntryLogic
{
    public class DocumentHelper : IDocumentHelper
    {
        private readonly Company _company;
        public SettingsProvider settingsProvider;

        public DocumentHelper(Company company, SettingsProvider settingsProvider)
        {
            _company = company;
            this.settingsProvider = settingsProvider;
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
            List<Result> results = new List<Result>();
            Documents invoiceDi = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseCreditNotes);
            invoiceDi.GetByKey(int.Parse(invDocEntry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;
            Recordset recSet = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);
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
                (BusinessPartners)_company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
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
                        string incomeTaxPayerTransId = AddJournalEntry(_company, incomeTaxAccCr,
                            incomeTaxAccDr, incomeTaxControlAccCr, invoiceDi.CardCode, -incomeTaxAmount,
                            invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                            invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);
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
                        string incomeTaxPayerTransId = AddJournalEntry(_company, incomeTaxAccCr,
                            incomeTaxAccDr, incomeTaxControlAccCr, invoiceDi.CardCode, incomeTaxAmount,
                            invoiceDi.Series, invoiceDi.Comments, invoiceDi.DocDate,
                            invoiceDi.BPL_IDAssignedToInvoice, invoiceDi.DocCurrency);

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
            Documents invoiceDi = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
            invoiceDi.GetByKey(int.Parse(invDocEntry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;
            bool isFc = invoiceDi.DocCurrency != "GEL";

            BusinessPartners bp = (BusinessPartners)_company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
            bp.GetByKey(invoiceDi.CardCode);
            bool isIncomeTaxPayer = (string)bp.UserFields.Fields.Item("U_IncomeTaxPayer").Value == "01";
            bool isPensionPayer = (string)bp.UserFields.Fields.Item("U_PensionPayer").Value == "01";
            var incomeTaxPayerPercent = (double)bp.UserFields.Fields.Item("U_IncomeTaxPayerPercent").Value;
            var pensionPayerPercent = (double)bp.UserFields.Fields.Item("U_PensionPayerPercent").Value;

            var oItem = (Items)_company.GetBusinessObject(BoObjectTypes.oItems); 
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

                        incomeTaxPayerTransId = PostIncomeTaxvJEFromInvoice(settings, invoiceDi, incomeTaxAmount);

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

                        string incomeTaxPayerTransId = PostIncomeTaxvJEFromInvoice(settings, invoiceDi, incomeTaxAmount);
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

        private string PostIncomeTaxvJEFromInvoice(Settings settings, Documents invoiceDi, double incomeTaxAmount)
        {
            string incomeTaxPayerTransId;
            JournalEntries vJE = (JournalEntries)_company.GetBusinessObject(BoObjectTypes.oJournalEntries);
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
                incomeTaxPayerTransId = _company.GetNewObjectKey();
            }
            else
            {
                throw new Exception(_company.GetLastErrorDescription());
            }

            return incomeTaxPayerTransId;
        }


        public string PostPensionvJEFromInvoice(Settings settings, Documents invoiceDI, double pensionAmountPaymentOnAccount)
        {
            JournalEntries vJE = (JournalEntries)_company.GetBusinessObject(BoObjectTypes.oJournalEntries);
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
                transId = _company.GetNewObjectKey();
            }
            else
            {
                throw new Exception(_company.GetLastErrorDescription());
            }
            return transId;
        }

        public IEnumerable<Result> PostIncomeTaxFromOutgoing(string invDocEntry)
        {
            List<Result> results = new List<Result>();
            Documents invoiceDi = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
            invoiceDi.GetByKey(int.Parse(invDocEntry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;

            Recordset recSet = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);


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
                (SAPbobsCOM.BusinessPartners)_company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
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
                        string incomeTaxPayerTransId = AddJournalEntry(_company, incomeTaxAccCr,
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
                        string incomeTaxPayerTransId = AddJournalEntry(_company, incomeTaxAccCr,
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
            Documents invoiceDi = (Documents)_company.GetBusinessObject(BoObjectTypes.oPurchaseInvoices);
            invoiceDi.GetByKey(int.Parse(invDocEntry, CultureInfo.InvariantCulture));
            string bpCode = invoiceDi.CardCode;

            Recordset recSet = (Recordset)_company.GetBusinessObject(BoObjectTypes.BoRecordset);


            recSet.DoQuery2(
                $"SELECT U_IncomeTaxPayer, U_PensionPayer FROM OCRD WHERE OCRD.CardCode = N'{bpCode}'");

            bool isPensionPayer = recSet.Fields.Item("U_PensionPayer").Value.ToString() == "01";

            recSet.DoQuery2($"Select * From [@RSM_SERVICE_PARAMS]");
            string pensionAccDr = recSet.Fields.Item("U_PensionAccDr").Value.ToString();
            string pensionAccCr = recSet.Fields.Item("U_PensionAccCr").Value.ToString();
            string pensionControlAccDr = recSet.Fields.Item("U_PensionControlAccDr").Value.ToString();
            string pensionControlAccCr = recSet.Fields.Item("U_PensionControlAccCr").Value.ToString();

            SAPbobsCOM.BusinessPartners bp =
                (SAPbobsCOM.BusinessPartners)_company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
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
                        string incometaxpayertransidcomp = AddJournalEntry(_company,
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
                        string incometaxpayertransid = AddJournalEntry(_company, pensionAccCr,
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
    }
}
