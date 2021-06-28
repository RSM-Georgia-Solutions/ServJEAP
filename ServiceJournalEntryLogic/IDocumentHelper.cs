using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryLogic
{
    public interface IDocumentHelper
    {
        IEnumerable<Result> PostIncomeTaxFromCreditMemo(string invDocEnttry);
        IEnumerable<Result> PostIncomeTaxFromInvoice(string invDocEnttry);
        IEnumerable<Result> PostIncomeTaxFromOutgoing(string invDocEnttry);
        IEnumerable<Result> PostPension(string invoiceDocentry);
        string AddJournalEntry(Company _comp, string creditCode, string debitCode, string creditControlCode, string debitControlCode, double amount, int series, string comment, DateTime DocDate, int BPLID = 235,
           string currency = "GEL");
    }
}
