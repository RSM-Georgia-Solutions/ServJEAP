using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;

namespace ServiceJournalEntryLogic.Providers
{
    public class JournalEntriesProvider 
    {
        
        public JournalEntriesProvider(SAPbobsCOM.Company Company)
        {
            oCompany = Company;
        }

        public Company oCompany { get; private set; }

    }
}
