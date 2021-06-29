using ServiceJournalEntryLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryLogic.Providers
{
    public abstract class UDTProvider
    {
        public SAPbobsCOM.Company oCompany { get; private set; }
        public UDTProvider(SAPbobsCOM.Company oCompany)
        {
            this.oCompany = oCompany;
        }

        public SAPbobsCOM.Recordset GetRecordSet()
        {
            return (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
        }
        
    }
}
