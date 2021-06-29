using System;
using System.Xml;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using Application = SAPbouiCOM.Framework.Application;
using ServiceJournalEntryAp.Controllers;
using System.Linq;

namespace ServiceJournalEntryAp.SystemForms
{
    [Form("141", "Forms/SystemForms/ApInvoice.b1f")]
    class ApInvoice : SystemFormBase
    {
        public ApInvoiceFormController controller { get; set; }

        public ApInvoice()
        {
        }

        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            OnCustomInitialize();
        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.DataAddAfter += new DataAddAfterHandler(this.Form_DataAddAfter);
        }

        private void Form_DataAddAfter(ref BusinessObjectInfo pVal)
        {
            if (pVal.ActionSuccess)
            {
                var invObjectString = pVal.ObjectKey;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(invObjectString);
                string invDocEnttry = string.Empty;
                try
                {
                    invDocEnttry = xmlDoc.GetElementsByTagName("DocEntry").Item(0).InnerText;
                }
                catch (Exception e)
                {
                    Application.SBO_Application.SetStatusBarMessage("Invalid Document Number",
                        BoMessageTime.bmt_Short);
                }
                //DocumentHelper.PostIncomeTaxFromInvoice(invDocEnttry);
                var res = controller.DocumentHelper.PostIncomeTaxFromInvoice(invDocEnttry);
                if(res.Any(x => x.IsSuccessCode == false))
                {
                    SAPbouiCOM.Framework.Application.SBO_Application.MessageBox(res.First(x => x.IsSuccessCode == false).StatusDescription);
                }
            }
        }

        private void OnCustomInitialize()
        {
            controller = new ApInvoiceFormController(RSM.Core.SDK.DI.DIApplication.Company, UIAPIRawForm);
        }
    }
}
