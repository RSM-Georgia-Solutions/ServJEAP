using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using ServiceJournalEntryAp.Helpers;
using Application = SAPbouiCOM.Framework.Application;

namespace ServiceJournalEntryAp.Forms.SystemForms
{
    [FormAttribute("181", "Forms/SystemForms/ApCreditMemo.b1f")]
    class ApCreditMemo : SystemFormBase
    {
        public ApCreditMemo()
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
            this.DataAddAfter += new DataAddAfterHandler(this.Form_DataAddAfter);

        }

        private void Form_DataAddAfter(ref SAPbouiCOM.BusinessObjectInfo pVal)
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
                DocumentHelper.PostIncomeTaxFromCreditMemo(invDocEnttry);
            }
        }

        private void OnCustomInitialize()
        {

        }
    }
}
