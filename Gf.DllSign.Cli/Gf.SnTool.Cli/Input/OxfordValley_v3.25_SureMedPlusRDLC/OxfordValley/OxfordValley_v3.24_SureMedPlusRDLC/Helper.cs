using BCO.DataReportModule.Models;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Helper
    {
        //Show 2 or 3 character day name of week
        public static string SetDayForDisplayModel(string day, string languageCode, int? charShow = 2)
        {
            var result = "";
            var dateTimeInfo = DateTimeFormatInfo.GetInstance(new CultureInfo(languageCode));
            var names = charShow != null ? dateTimeInfo.AbbreviatedDayNames : dateTimeInfo.ShortestDayNames;
            switch (day)
            {
                case "Monday":
                    result = names[(int)DayOfWeek.Monday];
                    break;
                case "Tuesday":
                    result = names[(int)DayOfWeek.Tuesday];
                    break;
                case "Wednesday":
                    result = names[(int)DayOfWeek.Wednesday];
                    break;
                case "Thursday":
                    result = names[(int)DayOfWeek.Thursday];
                    break;
                case "Friday":
                    result = names[(int)DayOfWeek.Friday];
                    break;
                case "Saturday":
                    result = names[(int)DayOfWeek.Saturday];
                    break;
                case "Sunday":
                    result = names[(int)DayOfWeek.Sunday];
                    break;
            }
            return result;
        }

        public static string getDateNameOfWeek(DateTime date, string languageCode)
        {
            var dateTimeInfo = DateTimeFormatInfo.GetInstance(new CultureInfo(languageCode));
            var shortestDayNames = dateTimeInfo.ShortestDayNames;
            return shortestDayNames[(int)date.DayOfWeek];
        }

        public static DateTime CalculateExpireDate(TrayData tray, CupData cupFirst, int minimumExpirePeriod)
        {
            return cupFirst.IntakeDateTime.AddDays(tray.MinimumExpirePeriod == 0
                ? minimumExpirePeriod
                : tray.MinimumExpirePeriod);
        }

        public static string CalculateIntakeTime(TrayData tray, int number, bool? isTray57 = false)
        {
            var result = "";
            var cup = tray.Cups.FirstOrDefault(o => o.Number % (isTray57 == true ? 5 : 4) == number &&
                                                    !string.IsNullOrEmpty(o.IntakeTimeDescription));
            if (cup != null)
            {
                var listDoseDispense = new List<string> { "a", "b", "c", "d", "e" };
                var isDoesDispen = listDoseDispense.Contains(cup.TimeSlotCode);
                result = isDoesDispen ? cup.TimeSlotDescription : cup.IntakeTimeDescription;
            }
            return result;
        }

        public static string CalculateColorIntakeTime(TrayData tray, int number, bool? isTray57 = false)
        {
            var result = "";
            var cup = tray.Cups.FirstOrDefault(o => o.Number % (isTray57 == true ? 5 : 4) == number &&
                                                    !string.IsNullOrEmpty(o.IntakeTimeDescription));
            if (cup != null)
            {
                var listDoseDispense = new List<string> { "a", "b", "c", "d", "e" };
                var isDoesDispen = listDoseDispense.Contains(cup.TimeSlotCode);
                result = isDoesDispen ? cup.TimeSlotColor : cup.IntakeTimeColor;
            }
            return result ?? "black";
        }

        public static MedicineData GetMedicineData(TrayData tray, string pillId)
        {
            MedicineData medicine = null;
            medicine = tray.Cups.SelectMany(x => x.Medicines).FirstOrDefault(x => x.PillId.Contains(pillId));
            if (medicine == null)
            {
                medicine = tray.Cups.SelectMany(x => x.Medicines).FirstOrDefault(x => x.PillInfo != null
                && ((FragmentData)x.PillInfo).WholePill != null
                && ((FragmentData)x.PillInfo).WholePill.PillId == pillId);
            }
            return medicine;
        }

        public static Bitmap RotateImg(Image bmp, float angle, Color bkColor)
        {
            angle = angle % 360;
            if (angle > 180)
                angle -= 360;

            var pf = default(PixelFormat);
            if (bkColor == Color.Transparent)
                pf = PixelFormat.Format32bppArgb;
            else
                pf = bmp.PixelFormat;

            var sin = (float)Math.Abs(Math.Sin(angle * Math.PI / 180.0)); // this function takes radians
            var cos = (float)Math.Abs(Math.Cos(angle * Math.PI / 180.0)); // this one too
            var newImgWidth = sin * bmp.Height + cos * bmp.Width;
            var newImgHeight = sin * bmp.Width + cos * bmp.Height;
            var originX = 0f;
            var originY = 0f;

            if (angle > 0)
            {
                if (angle <= 90)
                {
                    originX = sin * bmp.Height;
                }
                else
                {
                    originX = newImgWidth;
                    originY = newImgHeight - sin * bmp.Width;
                }
            }
            else
            {
                if (angle >= -90)
                {
                    originY = sin * bmp.Width;
                }
                else
                {
                    originX = newImgWidth - sin * bmp.Height;
                    originY = newImgHeight;
                }
            }

            var newImg = new Bitmap((int)newImgWidth, (int)newImgHeight, pf);
            var g = Graphics.FromImage(newImg);
            g.Clear(bkColor);
            g.TranslateTransform(originX, originY); // offset the origin to our calculated values
            g.RotateTransform(angle); // set up rotate
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.DrawImageUnscaled(bmp, 0, 0); // draw the image at 0, 0
            g.Dispose();

            return newImg;
        }

        public static Bitmap Convert_Text_to_Image(string txt, string fontname, int fontsize, FontStyle fontstyle, Color textColor)
        {
            //creating bitmap image
            var bmp = new Bitmap(1, 1);
            if (!string.IsNullOrEmpty(txt))
            {
                //FromImage method creates a new Graphics from the specified Image.
                var graphics = Graphics.FromImage(bmp);
                // Create the Font object for the image text drawing.
                var font = new Font(fontname, fontsize, fontstyle);
                var stringSize = new SizeF(320, 89);
                bmp = new Bitmap(bmp, (int)stringSize.Width, (int)stringSize.Height);
                graphics = Graphics.FromImage(bmp);
                graphics.SmoothingMode = SmoothingMode.HighQuality; // Default: [None]
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality; // Default: [Default]
                graphics.CompositingMode = CompositingMode.SourceOver; // Default: [SourceOver]
                graphics.CompositingQuality =
                    CompositingQuality.HighQuality; // Default: [Default]
                graphics.InterpolationMode =
                    InterpolationMode.HighQualityBilinear; // Default: [Bilinear]

                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                //Draw Specified text with specified format 
                graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, (int)stringSize.Width,
                    (int)stringSize.Height);
                Brush colorBrushes = new SolidBrush(textColor);
                graphics.DrawString(txt, font, colorBrushes, 0, 0);

                font.Dispose();
                graphics.Flush();
                graphics.Dispose();
            }
            return bmp;
        }

        public static List<PillDispense> GetChargeAndExpireDate(TrayData tray)
        {
            var medicinesInTray = tray.Cups.SelectMany(x => x.Medicines).Where(x => x.PillInfo != null);
            var query =
            (from x in medicinesInTray
             group x
             by x.PillId
                into g
             select new
             {
                 PillId = g.Key,
                 BcoRefill = g.SelectMany(x => x.BcoRefill),
                 CartridgeDispense = g.SelectMany(x => x.CartridgeDispense),
                 MdaDispense = g.SelectMany(x => x.MdaDispense)
             }).ToList();
            var dispenseList = new List<PillDispense>();

            foreach (var x in query)
            {
                var chargeList = new List<string>();
                var expireDate = DateTime.MaxValue;
                var isMultipleDay = false;
                var expireDateCount = 0;
                foreach (var b in x.BcoRefill)
                {
                    chargeList.Add(b.ChargeNumber);
                    if (expireDate != b.ExpireDate)
                        ++expireDateCount;
                    if (expireDate > b.ExpireDate)
                    {
                        expireDate = b.ExpireDate;                        
                    }                    
                }

                foreach (var c in x.CartridgeDispense)
                {
                    chargeList.Add(c.ChargeNumber);
                    if (expireDate != c.ExpireDate)
                        ++expireDateCount;
                    if (expireDate > c.ExpireDate)
                    {
                        expireDate = c.ExpireDate;
                    }
                   
                }

                foreach (var m in x.MdaDispense)
                {
                    chargeList.Add(m.ChargeNumber);
                    if (expireDate != m.ExpireDate)
                        ++expireDateCount;
                    if (expireDate > m.ExpireDate)
                    {
                        expireDate = m.ExpireDate;
                    }
                    
                }
                var chargeListText =
                    string.Join(",",
                        chargeList.Distinct().ToArray().Take(3));
                dispenseList.Add(new PillDispense
                {
                    PillId = x.PillId,
                    ChargeNumberList = chargeListText,
                    ExipreDate = expireDate,
                    isMultipleExpDay = expireDateCount > 1 ? true : false
                });
            }
            return dispenseList;
        }

        public static string BuildCustomerAddress(AddressData customerAddress)
        {
            var result = "";
            result = result + (string.IsNullOrEmpty(customerAddress.Street)
                         ? ""
                         : Utils.TrimStringWithLength(customerAddress.Street, 40) + Environment.NewLine);
            var zipPostCode =
                (string.IsNullOrEmpty(customerAddress.ZipPostalCode) ? "" : customerAddress.ZipPostalCode) +
                (string.IsNullOrEmpty(customerAddress.City) ? "" : " " + customerAddress.City) +
                (string.IsNullOrEmpty(customerAddress.StateProvince) ? "" : ", " + customerAddress.StateProvince);
            result = result + Utils.TrimStringWithLength(zipPostCode, 40) + Environment.NewLine;
            if (customerAddress.Customer != null)
            {
                result = result + (string.IsNullOrEmpty(customerAddress.Customer.Phone1)
                             ? ""
                             : Utils.TrimStringWithLength(customerAddress.Customer.Phone1, 40) + Environment.NewLine);
                result = result + (string.IsNullOrEmpty(customerAddress.Customer.Email1)
                             ? ""
                             : Utils.TrimStringWithLength(customerAddress.Customer.Email1, 40));
            }
            return result;
        }

        public static string buildCustomerAddressMcKession(AddressData customerAddress)
        {
            var result = "";
            result = result + (string.IsNullOrEmpty(customerAddress.City)
                         ? ""
                         : customerAddress.City +
                           (string.IsNullOrEmpty(customerAddress.StateProvince)
                               ? ""
                               : ", " + customerAddress.StateProvince) +
                           (string.IsNullOrEmpty(customerAddress.ZipPostalCode)
                               ? ""
                               : ", " + customerAddress.ZipPostalCode) + Environment.NewLine);
            result = result + (string.IsNullOrEmpty(customerAddress.Street)
                         ? ""
                         : customerAddress.Street + Environment.NewLine);
            return result;
        }

        public static string buildDescriptionPillInfo(BasePillData medicinePillInfo)
        {
            return medicinePillInfo.Shape + (medicinePillInfo.Shape != null ? " | " : "") + medicinePillInfo.Color +
                   (medicinePillInfo.Color != null ? " | " : "") + medicinePillInfo.Imprint;
        }

        public static string BuildBedRoomInfo(PatientData patient, bool isMultiPatient, string errorTitle)
        {
            var result = string.IsNullOrEmpty(patient.Room)
                ? ""
                : (isMultiPatient ? errorTitle : patient.Room);
            return result;
        }
    }

    public class PillDispense
    {
        public string PillId { get; set; }
        public string ChargeNumberList { get; set; }
        public DateTime ExipreDate { get; set; }
        public bool isMultipleExpDay { get; set; }
    }
}
