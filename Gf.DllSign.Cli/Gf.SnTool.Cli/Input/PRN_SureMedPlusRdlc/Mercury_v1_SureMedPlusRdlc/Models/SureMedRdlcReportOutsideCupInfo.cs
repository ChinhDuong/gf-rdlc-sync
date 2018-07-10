namespace Mercury_v1_SureMedPlusRdlc.Models
{
    public class SureMedRdlcReportOutsideCupInfo
    {
        public string IntakeDateTimeStr { get; set; }
        public string AbbrevDateStr { get; set; }
        public string PatientName { get; set; }
        public byte[] IntakeIcon { get; set; }
        public string IntakeTimeColor { get; set; }
        public string TimeSlotColor { get; set; }
        public string DayName { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public int cupNumber { get; set; }
        public bool IsMultiPatient { get; set; }
        public bool FirstCup { get; set; }
        public string PrintTime { get; set; }
        public string DayName1 { get; set; }
        public string DayName2 { get; set; }
        public string DayName3 { get; set; }
    }
}
