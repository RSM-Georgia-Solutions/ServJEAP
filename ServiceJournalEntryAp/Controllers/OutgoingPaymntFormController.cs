using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;
using SAPbouiCOM;
using System.Xml;
using System.Globalization;
using ServiceJournalEntryLogic.Providers;
using RSM.Core.SDK.DI.Extension;

namespace ServiceJournalEntryAp.Controllers
{
    public class OutgoingPaymntFormController : FormController
    {
        public SettingsProvider SettingsProvider { get; set; }
        public OutgoingPaymntFormController(SAPbobsCOM.Company Company, IForm Form, SettingsProvider settingsProvider) : base(Company, Form)
        {
            SettingsProvider = settingsProvider;
        }

        public void OnPaymentAdd(string invDocEnttry)
        {
            DocumentHelper.OnPaymentAdd(invDocEnttry, true);

        }

       

        public void OnPaymentUpdate(string invDocEnttry)
        {
            DocumentHelper.OnPaymentUpdate(invDocEnttry);

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
        private string PostvJEFromPaymentInvoce(ServiceJournalEntryLogic.Models.Settings settings, Documents invoiceDI, double pensionAmountPaymentOnAccount)
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
    }
}
