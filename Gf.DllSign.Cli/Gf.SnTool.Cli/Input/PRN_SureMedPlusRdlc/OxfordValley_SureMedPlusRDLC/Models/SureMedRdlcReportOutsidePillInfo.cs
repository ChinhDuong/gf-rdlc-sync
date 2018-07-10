namespace OxfordValley_SureMedPlusRDLC.Models
{
    public class SureMedRdlcReportOutsidePillInfo
    {
        public string PillId { get; set; }
        public int cupNumber { get; set; }
        public int RowIdx { get; set; }
        public string PillInfoCol1 { get; set; }
        public string PillInfoCol2 { get; set; }
        public decimal Amount { get; set; }
    }
}
