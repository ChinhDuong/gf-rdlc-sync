namespace BJRX_v4_basic_SureMedPlusRdlc.Models
{
    public class SureMedRdlcReportOutsideInfo
    {
        public string PatientName { get; set; }
        public byte[] PatientPhoto { get; set; }
        public string RoomBedInfo { get; set; }
        public string PatientId { get; set; }
        public string PatientIdWithPrefix { get; set; }
        public string StartDate { get; set; }
        public string PatientDob { get; set; }
        public string FacilityAndDayOfWeekStr { get; set; }
        public string PackXofX { get; set; }
        public byte[] PatientBigPhoto { get; set; }
        public string PharmacyInfo { get; set; }
        public string PharmacyAddress { get; set; }
        public byte[] BarcodeImage { get; set; }
        public byte[] CompanyLogo { get; set; }
        public byte[] PromoLogo { get; set; }
        public string DateCreated { get; set; }
        public bool IsMultiPatient { get; set; }   
        public string Phone { get; set; }
        public string ExpireDate { get; set; }
        public string Unit { get; set; }
        public byte[] WarningImage { get; set; }
        public string Location { get; set; }
        public string PhoneLabel { get; set; }
        public string PatientStreet { get; set; }
        public string PatientZipCode { get; set; }
        public string CautionLabel { get; set; }
        public string CautionValue { get; set; }
        public string Dea { get; set; }
        public string DeaValue { get; set; }
    }
}
