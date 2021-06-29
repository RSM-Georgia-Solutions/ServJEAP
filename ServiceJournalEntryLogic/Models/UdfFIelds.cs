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
        [ValidValue(Description = "კი", Value = "01")]
        [ValidValue(Description = "არა", Value = "02")]
        [Field(Name = "IncomeTaxPayer", Description = "საშემოსავლოს გადამხდელი", Type = BoFieldTypes.db_Alpha, Size = 10, Mandatory = BoYesNoEnum.tYES, DefValue = "01")]
        public bool IncomeTaxPayer { get; set; }

        [ValidValue(Description = "კი", Value = "01")]
        [ValidValue(Description = "არა", Value = "02")]
        [Field(Name = "PensionPayer", Description = "საპენსიოს გადამხდელი", Type = BoFieldTypes.db_Alpha, Size = 10, Mandatory = BoYesNoEnum.tYES, DefValue = "01")]
        public bool PensionPayer { get; set; }

        [Field(Name = "IncomeTaxPayerPercent", Description = "საშემოსავლოს %", Type = BoFieldTypes.db_Float, SubType = BoFldSubTypes.st_Percentage)]
        public double IncomeTaxPayerPercent { get; set; }

        [Field(Name = "PensionPayerPercent", Description = "საპენსიოს %", Type = BoFieldTypes.db_Float, SubType = BoFldSubTypes.st_Percentage)]
        public double PensionPayerPercent { get; set; }
    }



    [SAPObject("OITM")]
    public class OITM_Udfs : SAPObject
    {
        [ValidValue(Description = "კი", Value = "01")]
        [ValidValue(Description = "არა", Value = "02")]
        [Field(Name = "PensionLiable", Description = "ეკუთვნის საპენსიო", Type = BoFieldTypes.db_Alpha, Size = 10, Mandatory = BoYesNoEnum.tYES, DefValue = "01")]
        public bool PensionLiable { get; set; }
    }
}
