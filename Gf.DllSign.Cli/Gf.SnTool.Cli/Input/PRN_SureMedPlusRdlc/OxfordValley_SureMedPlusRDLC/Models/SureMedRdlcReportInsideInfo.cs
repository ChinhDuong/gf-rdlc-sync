using System.Drawing;

namespace OxfordValley_SureMedPlusRDLC.Models
{
    public class SureMedRdlcReportInsideInfo
    {
        public string DrugName { get; set; }
        public byte[] Model1 { get; set; }
        public byte[] Model2 { get; set; }
        public Image ImgModel1 { get; set; }
        public Image ImgModel2 { get; set; }
        public Image ImgModel3 { get; set; }
        public Image ImgModel4 { get; set; }
        public string Strength { get; set; }
        public string DinNumber { get; set; }
        public string CommercialDrugName { get; set; }
        public string Manufacturer { get; set; }
        public string ExpiredDateStr { get; set; }
        public string LotNumber { get; set; }
        public string Instructions { get; set; }
        public string Notes { get; set; }
        public string RxNumber { get; set; }
        public string DrName { get; set; }
        public string Description { get; set; }
        public string DoseName { get; set; }
        public string NationalDrugCode { get; set; }
        public decimal NumEve { get; set; }
        public decimal NumMorn { get; set; }
        public decimal NumNight { get; set; }
        public decimal NumNoon { get; set; }
        public string BrandName { get; set; }
        public string Col1Name { get; set; }
        public string Col2Name { get; set; }
        public string Col3Name { get; set; }
        public string Col4Name { get; set; }
    }
}
