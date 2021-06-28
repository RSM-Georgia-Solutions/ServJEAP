using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translator;

namespace ServiceJournalEntryAp.Extensions
{
    public static class RecordSetExtensions
    {
        public static void DoQuery2(this SAPbobsCOM.Recordset RecordSet, string Query, SAPbobsCOM.BoDataServerTypes DBServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL)
        {
            if(DBServerType == SAPbobsCOM.BoDataServerTypes.dst_HANADB)
            {
                int numOfStatements;
                int numOfErrors;
                TranslatorTool TranslateTool = new TranslatorTool();
                Query = TranslateTool.TranslateQuery(Query, out numOfStatements, out numOfErrors);
            }
            RecordSet.DoQuery(Query);
        }

        public static string TranslateQueryToHana(this SAPbobsCOM.Recordset RecordSet, string Query)
        {
            int numOfStatements;
            int numOfErrors;
            TranslatorTool TranslateTool = new TranslatorTool();
            Query = TranslateTool.TranslateQuery(Query, out numOfStatements, out numOfErrors);

            return Query;
        }

    }
}
