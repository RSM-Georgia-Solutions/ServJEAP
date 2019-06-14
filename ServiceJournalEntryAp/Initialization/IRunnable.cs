using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceJournalEntryAp.Initialization;

namespace ServiceJournalEntryAp
{
    interface IRunnable
    {
        void Run(DiManager diManager);
    }
}
