using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPbouiCOM.Framework;
using Application = SAPbouiCOM.Framework.Application;
using ServiceJournalEntryAp.Controllers;
using ServiceJournalEntryLogic.Extensions;
using ServiceJournalEntryLogic.Providers;

namespace ServiceJournalEntryAp.Forms
{
    [FormAttribute("ServiceJournalEntryAp.Forms.Settings", "Forms/Settings.b1f")]
    class Settings : UserFormBase
    {
        public SettingsFormController controller { get; set; }

        public Settings()
        {
        }
        public string PensionAccDr { get; set; }
        public string PensionAccControlDr { get; set; }
        public string PensionAccCr { get; set; }
        public string PensionAccControlCr { get; set; }
        public string IncomeTaxAccDr { get; set; }
        public string IncomeTaxAccCr { get; set; }
        public string IncomeTaxControlAccDr { get; set; }
        public string IncomeTaxControlAccCr { get; set; }
        public string IncomeTaxOnInvoice { get; set; }
        public string UseDocControllAcc { get; set; }
        public SettingsProvider SettingsProvider { get; set; }


        /// <summary>
        /// Initialize components. Called by framework after form created.
        /// </summary>
        public override void OnInitializeComponent()
        {
            this.StaticText0 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_0").Specific));
            this.EditText0 = ((SAPbouiCOM.EditText)(this.GetItem("Item_1").Specific));
            this.EditText0.ValidateAfter += new SAPbouiCOM._IEditTextEvents_ValidateAfterEventHandler(this.EditText0_ValidateAfter);
            this.EditText0.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText0_ChooseFromListBefore);
            this.StaticText1 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_2").Specific));
            this.EditText1 = ((SAPbouiCOM.EditText)(this.GetItem("Item_3").Specific));
            this.EditText1.ValidateAfter += new SAPbouiCOM._IEditTextEvents_ValidateAfterEventHandler(this.EditText1_ValidateAfter);
            this.EditText1.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText1_ChooseFromListBefore);
            this.Button0 = ((SAPbouiCOM.Button)(this.GetItem("Item_4").Specific));
            this.Button0.PressedAfter += new SAPbouiCOM._IButtonEvents_PressedAfterEventHandler(this.Button0_PressedAfter);
            this.StaticText2 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_5").Specific));
            this.EditText2 = ((SAPbouiCOM.EditText)(this.GetItem("Item_6").Specific));
            this.EditText2.ValidateAfter += new SAPbouiCOM._IEditTextEvents_ValidateAfterEventHandler(this.EditText2_ValidateAfter);
            this.EditText2.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText2_ChooseFromListBefore);
            this.StaticText3 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_7").Specific));
            this.EditText3 = ((SAPbouiCOM.EditText)(this.GetItem("Item_8").Specific));
            this.EditText3.ValidateAfter += new SAPbouiCOM._IEditTextEvents_ValidateAfterEventHandler(this.EditText3_ValidateAfter);
            this.EditText3.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText3_ChooseFromListBefore);
            this.StaticText4 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_9").Specific));
            this.EditText4 = ((SAPbouiCOM.EditText)(this.GetItem("Item_10").Specific));
            this.EditText4.ValidateAfter += new SAPbouiCOM._IEditTextEvents_ValidateAfterEventHandler(this.EditText4_ValidateAfter);
            this.EditText4.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText4_ChooseFromListBefore);
            this.StaticText5 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_11").Specific));
            this.EditText5 = ((SAPbouiCOM.EditText)(this.GetItem("Item_12").Specific));
            this.EditText5.ValidateAfter += new SAPbouiCOM._IEditTextEvents_ValidateAfterEventHandler(this.EditText5_ValidateAfter);
            this.EditText5.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText5_ChooseFromListBefore);
            this.StaticText6 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_13").Specific));
            this.EditText6 = ((SAPbouiCOM.EditText)(this.GetItem("Item_14").Specific));
            this.EditText6.ValidateAfter += new SAPbouiCOM._IEditTextEvents_ValidateAfterEventHandler(this.EditText6_ValidateAfter);
            this.EditText6.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText6_ChooseFromListBefore);
            this.StaticText7 = ((SAPbouiCOM.StaticText)(this.GetItem("Item_15").Specific));
            this.EditText7 = ((SAPbouiCOM.EditText)(this.GetItem("Item_16").Specific));
            this.EditText7.ValidateAfter += new SAPbouiCOM._IEditTextEvents_ValidateAfterEventHandler(this.EditText7_ValidateAfter);
            this.EditText7.ChooseFromListBefore += new SAPbouiCOM._IEditTextEvents_ChooseFromListBeforeEventHandler(this.EditText7_ChooseFromListBefore);
            this.CheckBox0 = ((SAPbouiCOM.CheckBox)(this.GetItem("Item_17").Specific));
            this.CheckBox1 = ((SAPbouiCOM.CheckBox)(this.GetItem("Item_18").Specific));

            this.OnCustomInitialize();

        }

        /// <summary>
        /// Initialize form event. Called by framework before form creation.
        /// </summary>
        public override void OnInitializeFormEvents()
        {
            this.VisibleAfter += new VisibleAfterHandler(this.Form_VisibleAfter);

        }

        private Form _paramsForm;

        private SAPbouiCOM.StaticText StaticText0;

        private void OnCustomInitialize()
        {
            controller = new SettingsFormController(RSM.Core.SDK.DI.DIApplication.Company, UIAPIRawForm);
            SettingsProvider = new SettingsProvider(RSM.Core.SDK.DI.DIApplication.Company);
        }

        private SAPbouiCOM.EditText EditText0;
        private SAPbouiCOM.StaticText StaticText1;
        private SAPbouiCOM.EditText EditText1;
        private SAPbouiCOM.Button Button0;
        public void FillCflPensionAccDr()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_1").ValueEx = PensionAccDr;
        }
        public void FillCflPensionAccCr()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_3").ValueEx = PensionAccCr;
        }

        public void FillCflPensionControlAccDr()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_4").ValueEx = PensionAccControlDr;
        }

        public void FillCflPensionControlAccCr()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_5").ValueEx = PensionAccControlCr;
        }


        public void FillCflIncomeTaxAccDr()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_2").ValueEx = IncomeTaxAccDr;
        }
        public void FillCflIncomeTaxAccCr()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_6").ValueEx = IncomeTaxAccCr;
        }
        public void FillCflIncomeTaxControlAccDr()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_7").ValueEx = IncomeTaxControlAccDr;
        }
        public void FillCflIncomeTaxControlAccCr()
        {
            _paramsForm.DataSources.UserDataSources.Item("UD_8").ValueEx = IncomeTaxControlAccCr;
        }




        private void Form_VisibleAfter(SAPbouiCOM.SBOItemEventArg pVal)
        {
            if (SAPbouiCOM.Framework.Application.SBO_Application.Forms.ActiveForm.Title == "პარამეტრები")
            {
                _paramsForm = SAPbouiCOM.Framework.Application.SBO_Application.Forms.ActiveForm;

                var settings = SettingsProvider.Get();
                if (settings != null)
                {
                    PensionAccDr = settings.PensionAccDr;
                    PensionAccCr = settings.PensionAccCr;
                    PensionAccControlDr = settings.PensionControlAccDr;
                    PensionAccControlCr = settings.PensionControlAccCr;
                    IncomeTaxAccDr = settings.IncomeTaxAccDr;
                    IncomeTaxAccCr = settings.IncomeTaxAccCr;
                    IncomeTaxControlAccDr = settings.IncomeControlTaxAccDr;
                    IncomeTaxControlAccCr = settings.IncomeControlTaxAccCr;
                    IncomeTaxOnInvoice = settings.IncomeTaxOnInvoice.ToString();
                    UseDocControllAcc = settings.UseDocControllAcc.ToString();

                    _paramsForm.DataSources.UserDataSources.Item("UD_1").ValueEx = PensionAccDr;
                    _paramsForm.DataSources.UserDataSources.Item("UD_2").ValueEx = IncomeTaxAccDr;
                    _paramsForm.DataSources.UserDataSources.Item("UD_3").ValueEx = PensionAccCr;
                    _paramsForm.DataSources.UserDataSources.Item("UD_4").ValueEx = PensionAccControlDr;
                    _paramsForm.DataSources.UserDataSources.Item("UD_5").ValueEx = PensionAccControlCr;
                    _paramsForm.DataSources.UserDataSources.Item("UD_6").ValueEx = IncomeTaxAccCr;
                    _paramsForm.DataSources.UserDataSources.Item("UD_7").ValueEx = IncomeTaxControlAccDr;
                    _paramsForm.DataSources.UserDataSources.Item("UD_8").ValueEx = IncomeTaxControlAccCr;
                    _paramsForm.DataSources.UserDataSources.Item("UD_9").ValueEx = IncomeTaxOnInvoice == "True"? "Y":"N";
                    _paramsForm.DataSources.UserDataSources.Item("UD_10").ValueEx = UseDocControllAcc == "True"? "Y":"N";

                }
            }
        }

        private void Button0_PressedAfter(object sboObject, SBOItemEventArg pVal)
        {
            //if (string.IsNullOrWhiteSpace(_paramsForm.DataSources.UserDataSources.Item("UD_1").ValueEx) || string.IsNullOrWhiteSpace(_paramsForm.DataSources.UserDataSources.Item("UD_2").ValueEx))
            //{
            //    Application.SBO_Application.SetStatusBarMessage("შეავსეთ ყველა ველი",
            //        BoMessageTime.bmt_Short, true);
            //    return;
            //}

            if (!string.IsNullOrWhiteSpace(PensionAccDr) && !string.IsNullOrWhiteSpace(PensionAccControlDr))
            {
                Application.SBO_Application.SetStatusBarMessage("აირჩიეთ ან საკონტროლო ან სტანდარტული ანგარიში",
                    BoMessageTime.bmt_Short, true);
                return; ;
            }
            if (!string.IsNullOrWhiteSpace(PensionAccCr) && !string.IsNullOrWhiteSpace(PensionAccControlCr))
            {
                Application.SBO_Application.SetStatusBarMessage("აირჩიეთ ან საკონტროლო ან სტანდარტული ანგარიში",
                    BoMessageTime.bmt_Short, true);
                return; ;
            }
            if (!string.IsNullOrWhiteSpace(IncomeTaxAccDr) && !string.IsNullOrWhiteSpace(IncomeTaxControlAccDr))
            {
                Application.SBO_Application.SetStatusBarMessage("აირჩიეთ ან საკონტროლო ან სტანდარტული ანგარიში",
                    BoMessageTime.bmt_Short, true);
                return; ;
            }
            if (!string.IsNullOrWhiteSpace(IncomeTaxAccCr) && !string.IsNullOrWhiteSpace(IncomeTaxControlAccCr))
            {
                Application.SBO_Application.SetStatusBarMessage("აირჩიეთ ან საკონტროლო ან სტანდარტული ანგარიში",
                    BoMessageTime.bmt_Short, true);
                return; ;
            }

            IncomeTaxOnInvoice = CheckBox0.Checked.ToString();
            UseDocControllAcc = CheckBox1.Checked.ToString();

            Recordset recSet = (Recordset)controller.oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            recSet.DoQuery2($"Select * From [@RSM_SERVICE_PARAMS]");
            if (recSet.EoF)
            {
                recSet.DoQuery2($"INSERT INTO [@RSM_SERVICE_PARAMS] (U_PensionAccDr, U_PensionAccCr, U_PensionControlAccDr, U_PensionControlAccCr, U_IncomeTaxAccDr, U_IncomeTaxAccCr, U_IncomeControlTaxAccDr, U_IncomeControlTaxAccCr,U_IncomeTaxOnInvoice, U_UseDocControllAcc) VALUES (N'{PensionAccDr}',N'{PensionAccCr}',N'{PensionAccControlDr}',N'{PensionAccControlCr}',N'{IncomeTaxAccDr}',N'{IncomeTaxAccCr}',N'{IncomeTaxControlAccDr}',N'{IncomeTaxControlAccCr}', N'{IncomeTaxOnInvoice}', N'{UseDocControllAcc}')");
            }
            else
            {
                recSet.DoQuery2($"UPDATE [@RSM_SERVICE_PARAMS] SET U_PensionAccDr = N'{PensionAccDr}', U_PensionAccCr = N'{PensionAccCr}', U_PensionControlAccDr = N'{PensionAccControlDr}', U_PensionControlAccCr = N'{PensionAccControlCr}', U_IncomeTaxAccDr = N'{IncomeTaxAccDr}', U_IncomeTaxAccCr = N'{IncomeTaxAccCr}', U_IncomeControlTaxAccDr = N'{IncomeTaxControlAccDr}', U_IncomeControlTaxAccCr = N'{IncomeTaxControlAccCr}', U_IncomeTaxOnInvoice = N'{IncomeTaxOnInvoice}', U_UseDocControllAcc = N'{UseDocControllAcc}'");
            }

            SAPbouiCOM.Framework.Application.SBO_Application.StatusBar.SetSystemMessage("Success", BoMessageTime.bmt_Short, BoStatusBarMessageType.smt_Success);
        }

        private StaticText StaticText2;
        private EditText EditText2;
        private StaticText StaticText3;
        private EditText EditText3;
        private StaticText StaticText4;
        private EditText EditText4;
        private StaticText StaticText5;
        private EditText EditText5;
        private StaticText StaticText6;
        private EditText EditText6;
        private StaticText StaticText7;
        private EditText EditText7;


        private void EditText0_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            ListOfAccounts list = new ListOfAccounts(this, "PensionAccDr");
            list.Show();

        }



        private void EditText2_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            ListOfAccounts list = new ListOfAccounts(this, "PensionAccCr");
            list.Show();
        }

        private void EditText3_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            BusinessPartners list = new BusinessPartners(this, "PensionControlAccDr");
            list.Show();
        }

        private void EditText4_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            BusinessPartners list = new BusinessPartners(this, "PensionControlAccCr");
            list.Show();
        }

        private void EditText1_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            ListOfAccounts list = new ListOfAccounts(this, "IncomeTaxAccDr");
            list.Show();

        }

        private void EditText5_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            ListOfAccounts list = new ListOfAccounts(this, "IncomeTaxAccCr");
            list.Show();
        }

        private void EditText6_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            BusinessPartners list = new BusinessPartners(this, "IncomeTaxControlAccDr");
            list.Show();
        }

        private void EditText7_ChooseFromListBefore(object sboObject, SBOItemEventArg pVal, out bool BubbleEvent)
        {
            BubbleEvent = false;
            BusinessPartners list = new BusinessPartners(this, "IncomeTaxControlAccCr");
            list.Show();
        }

        private void EditText0_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            PensionAccDr = EditText0.Value;
        }

        private void EditText2_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            PensionAccCr = EditText2.Value;
        }

        private void EditText3_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            PensionAccControlDr = EditText3.Value;
        }

        private void EditText4_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            PensionAccControlCr = EditText4.Value;
        }

        private void EditText1_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            IncomeTaxAccDr = EditText1.Value;
        }

        private void EditText5_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            IncomeTaxAccCr = EditText5.Value;
        }

        private void EditText6_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            IncomeTaxControlAccDr = EditText6.Value;
        }

        private void EditText7_ValidateAfter(object sboObject, SBOItemEventArg pVal)
        {
            IncomeTaxControlAccCr = EditText7.Value;

        }

        private CheckBox CheckBox0;
        private CheckBox CheckBox1;

   
    }
}
