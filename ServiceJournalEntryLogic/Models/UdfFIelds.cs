using RSM.Core.SDK.Attributes;
using RSM.Core.SDK.DI.Models;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceJournalEntryLogic.Models
{
    [SAPObject("OCRD")]
    public class OCRD_Udfs : SAPObject
    {
        [ValidValue(Description = "01", Value = "კი")]
        [ValidValue(Description = "02", Value = "არა")]
        [Field(Name = "IncomeTaxPayer", Description = "საშემოსავლოს გადამხდელი", Type = BoFieldTypes.db_Alpha, Size = 10, Mandatory = BoYesNoEnum.tYES)]
        public string IncomeTaxPayer { get; set; }

        [ValidValue(Description = "01", Value = "კი")]
        [ValidValue(Description = "02", Value = "არა")]
        [Field(Name = "PensionPayer", Description = "საპენსიოს გადამხდელი", Type = BoFieldTypes.db_Alpha, Size = 10, Mandatory = BoYesNoEnum.tYES)]
        public string PensionPayer { get; set; }

        [ValidValue(Description = "01", Value = "კი")]
        [ValidValue(Description = "02", Value = "არა")]
        [Field(Name = "PensionLiable", Description = "ეკუთვნის საპენსიო", Type = BoFieldTypes.db_Alpha, Size = 10, Mandatory = BoYesNoEnum.tYES)]
        public double PensionLiable { get; set; }


        [Field(Name = "IncomeTaxPayerPercent", Description = "საშემოსავლოს %", Type = BoFieldTypes.db_Float)]
        public double IncomeTaxPayerPercent { get; set; }

        [Field(Name = "PensionPayerPercent", Description = "საპენსიოს %", Type = BoFieldTypes.db_Float)]
        public double PensionPayerPercent { get; set; }

       
    }

    [SAPObject("OITM")]
    public class OITM_Udfs : SAPObject
    {
        [ValidValue(Description = "01", Value = "კი")]
        [ValidValue(Description = "02", Value = "არა")]
        [Field(Name = "PensionLiable", Description = "ეკუთვნის საპენსიო", Type = BoFieldTypes.db_Alpha, Size = 10, Mandatory = BoYesNoEnum.tYES)]
        public string PensionLiable { get; set; }
    }
}
