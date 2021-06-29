using SAPbouiCOM.Framework;
using ServiceJournalEntryAp.Forms;
using ServiceJournalEntryAp.Controllers;
using ServiceJournalEntryLogic.Extensions;

namespace ServiceJournalEntryAp
{
    [FormAttribute("ServiceJournalEntryAp.BusinessPartners", "Forms/BusinessPartners.b1f")]
    class BusinessPartners : UserFormBase
    {
        public BusinessPartnersFormController controller { get; set; }
        private readonly Settings _exciseParams;
        private readonly string _AccName;
        public BusinessPartners(Settings exciseParams, string accName)
        {
            _exciseParams = exciseParams;
            _AccName = accName;
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.Grid0 = ((SAPbouiCOM.Grid)(this.GetItem("Item_0").Specific));
            this.Grid0.DoubleClickAfter += new SAPbouiCOM._IGridEvents_DoubleClickAfterEventHandler(this.Grid0_DoubleClickAfter);
            this.Grid0.ClickAfter += new SAPbouiCOM._IGridEvents_ClickAfterEventHandler(this.Grid0_ClickAfter);
            this.StaticText0 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_1").Specific));
            this.EditText0 = ((SAPbouiCOM.EditText)(this.GetItem("Item_2").Specific));
            this.EditText0.KeyDownAfter += new SAPbouiCOM._IEditTextEvents_KeyDownAfterEventHandler(this.EditText0_KeyDownAfter);
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
        }

        private SAPbouiCOM.Grid Grid0;

        private void OnCustomInitialize()
        {
            controller = new BusinessPartnersFormController(RSM.Core.SDK.DI.DIApplication.Company, UIAPIRawForm);

            string query = $"SELECT CardCode, CardName FROM OCRD WHERE CardType = 'S'";

            if (controller.oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
            {
                query = RecordSetExtensions.TranslateQueryToHana(null, query);
            }

            Grid0.DataTable.ExecuteQuery(query);
        }

        private SAPbouiCOM.StaticText StaticText0;
        private SAPbouiCOM.EditText EditText0;

        private void Grid0_ClickAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            Grid0.Rows.SelectedRows.Clear();
            Grid0.Rows.SelectedRows.Add(pVal.Row);
        }

        private void Grid0_DoubleClickAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            string acc = Grid0.DataTable.GetValue("CardCode", Grid0.GetDataTableRowIndex(pVal.Row)).ToString();
            if (pVal.Row == -1)
            {
                return;
            }
            else if (_AccName == "PensionControlAccDr")
            {
                _exciseParams.PensionAccControlDr = acc;
                _exciseParams.FillCflPensionControlAccDr();
            }
            else if (_AccName == "PensionControlAccCr")
            {
                _exciseParams.PensionAccControlCr = acc;
                _exciseParams.FillCflPensionControlAccCr();
            }
            else if (_AccName == "IncomeTaxControlAccDr")
            {
                _exciseParams.IncomeTaxControlAccDr = acc;
                _exciseParams.FillCflIncomeTaxControlAccDr();
            }
            else if (_AccName == "IncomeTaxControlAccCr")
            {
                _exciseParams.IncomeTaxControlAccCr = acc;
                _exciseParams.FillCflIncomeTaxControlAccCr();
            }
            Application.SBO_Application.Forms.ActiveForm.Close();

        }

        private void EditText0_KeyDownAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            string query = $"SELECT CardCode, CardName FROM OCRD WHERE CardType = 'S' AND (CardCode Like N'%{EditText0.Value}%' OR CardName Like N'%{EditText0.Value}%')";
            if (controller.oCompany.DbServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
            {
                query = RecordSetExtensions.TranslateQueryToHana(null, query);
            }

            Grid0.DataTable.ExecuteQuery(query);
        }
    }
}
