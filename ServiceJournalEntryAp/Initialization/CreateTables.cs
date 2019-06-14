using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryAp.Initialization
{
    class CreateTables : IRunnable
    {
        public void Run(DiManager diManager)
        {
            diManager.CreateTable("RSM_SERVICE_PARAMS", SAPbobsCOM.BoUTBTableType.bott_NoObjectAutoIncrement);
        }
    }
}
