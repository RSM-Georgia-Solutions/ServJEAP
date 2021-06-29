using System;
using System.Globalization;
using Appocalypto;
using SAPbouiCOM.Framework;
using ServiceJournalEntryLogic.Services;

namespace ServiceJournalEntryAp
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application oApp = null;
                oApp = args.Length < 1 ? new Application() : new Application(args[0]);
                Menu MyMenu = new Menu();
                MyMenu.AddMenuItems();
                oApp.RegisterMenuEventHandler(MyMenu.SBO_Application_MenuEvent);

                //Connect SBO
                RSM.Core.SDK.DI.DIApplication.DIConnect((SAPbobsCOM.Company)Application.SBO_Application.Company.GetDICompany());

                Appocalypto.Mob appo = new Mob();
                appo.Run(5);
                var nfi = new NumberFormatInfo
                {
                    CurrencyDecimalSeparator = RSM.Core.SDK.DI.DIApplication.Company.GetCompanyService().GetAdminInfo().DecimalSeparator,
                    CurrencyGroupSeparator = RSM.Core.SDK.DI.DIApplication.Company.GetCompanyService().GetAdminInfo().ThousandsSeparator
                };
                CultureInfo culture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
                culture.NumberFormat = nfi;
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;


                var setupService = new SetupService();
                setupService.InitializeDb();

                Application.SBO_Application.AppEvent += new SAPbouiCOM._IApplicationEvents_AppEventEventHandler(SBO_Application_AppEvent);
                oApp.Run();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        static void SBO_Application_AppEvent(SAPbouiCOM.BoAppEventTypes EventType)
        {
            switch (EventType)
            {
                case SAPbouiCOM.BoAppEventTypes.aet_ShutDown:
                    //Exit Add-On
                    System.Windows.Forms.Application.Exit();
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_FontChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged:
                    break;
                case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition:
                    break;
                default:
                    break;
            }
        }
    }
}
