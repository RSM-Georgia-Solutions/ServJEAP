using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbouiCOM.Framework;
using ServiceJournalEntryAp.Initialization;

namespace ServiceJournalEntryAp.Forms
{
    [FormAttribute("ServiceJournalEntryAp.Forms.ListOfAccounts", "Forms/ListOfAccounts.b1f")]
    class ListOfAccounts : UserFormBase
    {

        private readonly Settings _exciseParams;
        private readonly string _AccName;

        public ListOfAccounts(Settings exciseParams, string accName)
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
            this.Grid0.ClickAfter += new SAPbouiCOM._IGridEvents_ClickAfterEventHandler(this.Grid0_ClickAfter);
            this.Grid0.DoubleClickAfter += new SAPbouiCOM._IGridEvents_DoubleClickAfterEventHandler(this.Grid0_DoubleClickAfter);
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
            Grid0.DataTable.ExecuteQuery(DiManager.QueryHanaTransalte($"SELECT AcctCode, AcctName FROM OACT WHERE Postable = 'Y'"));
        }

        private SAPbouiCOM.StaticText StaticText0;
        private SAPbouiCOM.EditText EditText0;

        private void EditText0_KeyDownAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            Grid0.DataTable.ExecuteQuery(DiManager.QueryHanaTransalte($"SELECT AcctCode, AcctName FROM OACT WHERE Postable = 'Y' AND (AcctCode Like N'%{EditText0.Value}%' OR AcctName Like N'%{EditText0.Value}%')"));
        }

        private void Grid0_DoubleClickAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            if (pVal.Row == -1)
            {
                return;
            }
            string acc = Grid0.DataTable.GetValue("AcctCode", Grid0.GetDataTableRowIndex(pVal.Row)).ToString();

            if (_AccName == "PensionAccDr")
            {
                _exciseParams.PensionAccDr = acc;
                _exciseParams.FillCflPensionAccDr();
            }
            else if(_AccName == "PensionAccCr")
            {
                _exciseParams.PensionAccCr = acc;
                _exciseParams.FillCflPensionAccCr();
            }
           
            else if (_AccName == "IncomeTaxAccDr")
            {
                _exciseParams.IncomeTaxAccDr = acc;
                _exciseParams.FillCflIncomeTaxAccDr();
            }
            else if (_AccName == "IncomeTaxAccCr")
            {
                _exciseParams.IncomeTaxAccCr = acc;
                _exciseParams.FillCflIncomeTaxAccCr();
            }
           

            Application.SBO_Application.Forms.ActiveForm.Close();
        }

        private void Grid0_ClickAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            Grid0.Rows.SelectedRows.Clear();
            if (pVal.Row == -1)
            {
                return;
            }
            Grid0.Rows.SelectedRows.Add(pVal.Row);
        }
    }
}
