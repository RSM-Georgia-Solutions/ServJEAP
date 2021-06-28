using RSM.Core.SDK.DI.DAO;
using ServiceJournalEntryLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryLogic.Services
{
    public class SetupService
    {
        #region UDFs
        private SAPObjectDAO<OCRD_Udfs> ocrdUdfs;
        private SAPObjectDAO<OITM_Udfs> oitmUDfs;
        #endregion


        private TableDAO<Settings> settingsUdt;
        private TableDAO<BspHisotry> bspHistoryUdt;

        public SetupService()
        {
            ocrdUdfs = new SAPObjectDAO<OCRD_Udfs>();
            oitmUDfs = new SAPObjectDAO<OITM_Udfs>();
            settingsUdt = new TableDAO<Settings>();
            bspHistoryUdt = new TableDAO<BspHisotry>();
        }

        public void InitializeDb()
        {
            ocrdUdfs.InitializeUserFields();
            oitmUDfs.InitializeUserFields();
            settingsUdt.Initialize();
            bspHistoryUdt.Initialize();
        }
    }
}
