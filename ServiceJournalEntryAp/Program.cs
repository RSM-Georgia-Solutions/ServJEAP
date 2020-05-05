using System;
using System.Collections.Generic;
using System.Globalization;
using Appocalypto;
using SAPbouiCOM.Framework;
using ServiceJournalEntryAp.Initialization;

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
                Initial init = new  Initial();
                DiManager dimanager = new DiManager();
                init.Run(dimanager);
                MyMenu.AddMenuItems();
                oApp.RegisterMenuEventHandler(MyMenu.SBO_Application_MenuEvent);
                Appocalypto.Mob appo = new Mob();
                appo.Run(5);
                var nfi = new NumberFormatInfo
                {
                    CurrencyDecimalSeparator = DiManager.Company.GetCompanyService().GetAdminInfo().DecimalSeparator,
                    CurrencyGroupSeparator = DiManager.Company.GetCompanyService().GetAdminInfo().ThousandsSeparator
                };
                CultureInfo culture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
                culture.NumberFormat = nfi;
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;

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
