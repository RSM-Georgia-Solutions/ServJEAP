using SAPbobsCOM;

namespace ServiceJournalEntryLogic
{
    public class Result
    {
        public string CreatedDocumentEntry { get; set; }
        public bool IsSuccessCode { get; set; }
        public string StatusDescription { get; set; }
        public BoObjectTypes ObjectType { get; set; }
    }
}
