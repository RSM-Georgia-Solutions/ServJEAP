using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryLogic.Services
{
    public class BankStatementsServiceHelper
    {

        public SAPbobsCOM.Company oCompany { get; set; }
        public BankStatementsServiceHelper(SAPbobsCOM.Company company)
        {
            oCompany = company;
        }

        public SAPbobsCOM.BankStatement Get(int InternalNumber)
        {
            SAPbobsCOM.BankStatementsService oBnkStSrv;
            SAPbobsCOM.CompanyService oCmpSrv;
            SAPbobsCOM.BankStatementParams Params;

            oCmpSrv = oCompany.GetCompanyService();

            //Get Bank Statement Service
            oBnkStSrv = (SAPbobsCOM.BankStatementsService)oCmpSrv.GetBusinessService(SAPbobsCOM.ServiceTypes.BankStatementsService);

            Params = (SAPbobsCOM.BankStatementParams)oBnkStSrv.GetDataInterface(SAPbobsCOM.BankStatementsServiceDataInterfaces.bssBankStatementParams);
            Params.InternalNumber = 28;

            //Get Bank Statement List

           return oBnkStSrv.GetBankStatement(Params);
        }
    }
}
