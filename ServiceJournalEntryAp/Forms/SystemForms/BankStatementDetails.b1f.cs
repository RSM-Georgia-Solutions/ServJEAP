using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using Application = SAPbouiCOM.Framework.Application;
using ServiceJournalEntryAp.Controllers;

namespace ServiceJournalEntryAp.SystemForms
{
    [FormAttribute("10000005", "Forms/SystemForms/BankStatementDetails.b1f")]
    class BankStatementDetails : SystemFormBase
    {
        public BankStatementDetailsFormController controller { get; set; }
        public BankStatementDetails()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.Button0 = ((SAPbouiCOM.Button)(this.GetItem("Item_99").Specific));
            this.Button0.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.Button0_PressedAfter);
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.ActivateAfter += new ActivateAfterHandler(this.Form_ActivateAfter);
            this.ClickAfter += new ClickAfterHandler(this.Form_ClickAfter);

        }

        private SAPbouiCOM.Button Button0;

        private void OnCustomInitialize()
        {
            Button0.Item.FontSize = 10;
            controller = new BankStatementDetailsFormController(RSM.Core.SDK.DI.DIApplication.Company, UIAPIRawForm);
        }

        private void Button0_PressedAfter(object sboObject, SAPbouiCOM.SBOItemEventArg pVal)
        {
            controller.PostPension();
        }

        private void Form_ActivateAfter(SBOItemEventArg pVal)
        {
            Form activeForm = Application.SBO_Application.Forms.ActiveForm;
            var status = activeForm.DataSources.DBDataSources.Item("OBNH").GetValue("status", 0);
            GetItem("Item_99").Enabled = status == "E";
        }

        private void Form_ClickAfter(SBOItemEventArg pVal)
        {
            Form activeForm = Application.SBO_Application.Forms.ActiveForm;
            var status = activeForm.DataSources.DBDataSources.Item("OBNH").GetValue("status", 0);
            GetItem("Item_99").Enabled = status == "E";
        }
    }
}
