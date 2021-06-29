using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbobsCOM;
using SAPbouiCOM;

namespace ServiceJournalEntryAp.Controllers
{
    public class BusinessPartnersFormController : FormController
    {
        public BusinessPartnersFormController(SAPbobsCOM.Company Company, IForm Form) : base(Company, Form)
        {
        }
    }
}
