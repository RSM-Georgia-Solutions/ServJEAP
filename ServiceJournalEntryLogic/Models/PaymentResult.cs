using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryLogic.Models
{
    public class PaymentResult
    {
        public List<string> TransIdComp { get; set; }
        public List<string> TransIdEmp { get; set; }
        public List<string> TransIdIncomeTax { get; set; }
        
        public PaymentResult()
        {
            TransIdComp = new List<string>();
            TransIdEmp = new List<string>();
            TransIdIncomeTax = new List<string>();
        }

    }
}
