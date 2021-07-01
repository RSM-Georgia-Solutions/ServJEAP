using SAPbobsCOM;
using ServiceJournalEntryLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryLogic.Providers
{
    public class BspHistoryProvider : UDTProvider
    {
        public BspHistoryProvider(Company oCompany) : base(oCompany)
        {
        }

        public bool Exists(string IDNumber, string Sequence, string BankAccount)
        {
            Recordset recSet2 = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet2.DoQuery($"select * from [@RSM_BSP_HISTORY] WHERE U_BSP_ID_NUMBER = {IDNumber} AND U_BSP_SEQUENCE = {Sequence} AND U_BSP_ACCOUNT = N'{BankAccount}'");

            return !recSet2.EoF;
        }

        public void Save(BspHisotry bspHisotry)
        {
            var oTable = oCompany.UserTables.Item("RSM_BSP_HISTORY");
            oTable.UserFields.Fields.Item("U_BSP_ID_NUMBER").Value = bspHisotry.BSP_ID_NUMBER;
            oTable.UserFields.Fields.Item("U_BSP_SEQUENCE").Value = bspHisotry.BSP_SEQUENCE;
            oTable.UserFields.Fields.Item("U_BSP_ACCOUNT").Value = bspHisotry.BSP_ACCOUNT;
            oTable.UserFields.Fields.Item("U_TRANS_ID_EMPLOYEE").Value = bspHisotry.TRANS_ID_EMPLOYEE;
            oTable.UserFields.Fields.Item("U_TRANS_ID_COMPANY").Value = bspHisotry.TRANS_ID_COMPANY;
            var ret = oTable.Add();
            if (ret != 0)
                throw new Exception($"{oCompany.GetLastErrorDescription()}");
        }
    }
}
