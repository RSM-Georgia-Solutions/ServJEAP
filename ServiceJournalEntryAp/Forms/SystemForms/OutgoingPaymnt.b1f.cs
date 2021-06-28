﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using ServiceJournalEntryAp.Helpers;
using ServiceJournalEntryAp.Initialization;
using Application = SAPbouiCOM.Framework.Application;
using ServiceJournalEntryAp.Controllers;

namespace ServiceJournalEntryAp.SystemForms
{
    [FormAttribute("426", "Forms/SystemForms/OutgoingPaymnt.b1f")]
    class OutgoingPaymnt : SystemFormBase
    {
        public OutgoingPaymntFormController controller { get; set; }
        public OutgoingPaymnt()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.DataAddAfter += new SAPbouiCOM.Framework.FormBase.DataAddAfterHandler(this.Form_DataAddAfter);
            this.CloseAfter += new SAPbouiCOM.Framework.FormBase.CloseAfterHandler(this.Form_CloseAfter);
            this.DataUpdateAfter += new DataUpdateAfterHandler(this.Form_DataUpdateAfter);

        }

        private void Form_DataAddAfter(ref SAPbouiCOM.BusinessObjectInfo pVal)
        {
            if (pVal.ActionSuccess)
            {
                controller.OnPaymentAdd(ref pVal);
            }
        }

        private void OnCustomInitialize()
        {
            controller = new OutgoingPaymntFormController(RSM.Core.SDK.DI.DIApplication.Company, UIAPIRawForm);
        }

        private void Form_CloseAfter(SBOItemEventArg pVal)
        {

        }

        private void Form_DataUpdateAfter(ref BusinessObjectInfo pVal)
        {
            if (pVal.ActionSuccess)
            {
                controller.OnPaymentUpdate(ref pVal);
            }
        }
    }
}
