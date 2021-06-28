using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryAp.Controllers
{
    public class FormController
    {
        public SAPbobsCOM.Company oCompany { get; private set; }
        public SAPbouiCOM.IForm oForm { get; private set; }
        public ServiceJournalEntryLogic.DocumentHelper DocumentHelper { get; private set; }


        public FormController(SAPbobsCOM.Company Company, SAPbouiCOM.IForm Form)
        {
            oCompany = Company;
            oForm = Form;
            DocumentHelper = new ServiceJournalEntryLogic.DocumentHelper(oCompany);
        }
    }
}
