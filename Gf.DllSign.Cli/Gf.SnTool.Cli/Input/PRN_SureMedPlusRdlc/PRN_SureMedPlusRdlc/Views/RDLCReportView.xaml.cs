using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using BCO.DataReportModule;
using BCO.DataReportModule.Events;
using BCO.DataReportModule.Helpers;
using BCO.DataReportModule.Models;
using Omnicare_SureMedPlusRdlc.Models;
using CommonLib;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Reporting.WinForms;
using Common;

namespace Omnicare_SureMedPlusRdlc.Views
{
    public partial class RDLCReportView : INavigationAware, IRegionMemberLifetime, IActiveAware
    {
        private readonly IDataReportService _reportService;

        public DelegateCommand<object> PrintCommand { get; private set; }
        public DelegateCommand<object> ReprintCommand { get; private set; }
        public bool KeepAlive
        {
            get
            {
                return false;
            }
        }
        private bool _IsActive;
        public bool IsActive
        {
            get
            {
                return _IsActive;
            }

            set
            {
                _IsActive = value;
                IsActiveChanged(this, EventArgs.Empty);
            }
        }
        public event EventHandler IsActiveChanged;
        IEventAggregator _eventAggregator;
        string _currentUserName = "";
        private bool isPrint = false;
        private string labelId = Guid.Empty.ToString();
        private TrayData tray = null;
        public static string languageCode = "";
        public static string localLanguageCode = "";
        private string defaultFormatDate = "MM/dd/yyyy";
        private bool isMultiLocation = false;
        List<SureMedRdlcReportOutsideCupInfo> listCup4 = null;
        List<SureMedRdlcReportOutsideCupInfo> listCup2 = null;
        List<SureMedRdlcReportOutsideCupInfo> listCup3 = null;
        List<SureMedRdlcReportOutsideCupInfo> listCup1 = null;
        List<SureMedRdlcReportOutsidePillInfo> listPill = null;
        List<Image> listImagePill = null;

        public RDLCReportView(IDataReportService reportService
            , IEventAggregator eventAggregator)

        {

            InitializeComponent();
            _reportService = reportService;
            languageCode = _reportService.GetLanguageCode().Trim();
            localLanguageCode = (string.IsNullOrEmpty(languageCode) ? "fr-CA" : languageCode);
            _eventAggregator = eventAggregator;
            PrintCommand = new DelegateCommand<object>(OnPrintLabel, CanPrintLabel);
            ReprintCommand = new DelegateCommand<object>(OnRePrintLabel, CanPrintLabel);
            IsActiveChanged += RDLCReportView_IsActiveChanged;
        }
        private void RDLCReportView_IsActiveChanged(object sender, EventArgs e)
        {
            if (IsActive)
            {
                ViewActiveHandler();
            }
            else
            {
                ViewDeactiveHandler();
            }
        }

        private void ViewDeactiveHandler()
        {
            EventHelper<ReportCurrentModeChangedEvent, ReportCurrentModeChangedEventArgs>.UnsubscribeEvent(_eventAggregator, CurrentModeChangedHandler);
            EventHelper<ReportLoginEvent, string>.UnsubscribeEvent(_eventAggregator, LoginEventHandler);
            EventHelper<ShowModalDialogEvent, ShowModalDialogEventArgs>.UnsubscribeEvent(_eventAggregator, ShowModalDialogEventHandler);
            EventHelper<CloseModelDialogEvent, CloseModelDialogEventArgs>.UnsubscribeEvent(_eventAggregator, CloseModelDialogEventHandler);
            EventHelper<ReportDataSourceChangedEvent, ReportDataSourceChangedEventArgs>.UnsubscribeEvent(_eventAggregator, ReportDataSourceChangedEventHandler);
        }

        private void ViewActiveHandler()
        {
            EventHelper<ReportCurrentModeChangedEvent, ReportCurrentModeChangedEventArgs>.SubscribeEvent(_eventAggregator, CurrentModeChangedHandler);
            EventHelper<ReportLoginEvent, string>.SubscribeEvent(_eventAggregator, LoginEventHandler);
            EventHelper<ShowModalDialogEvent, ShowModalDialogEventArgs>.SubscribeEvent(_eventAggregator, ShowModalDialogEventHandler);
            EventHelper<CloseModelDialogEvent, CloseModelDialogEventArgs>.SubscribeEvent(_eventAggregator, CloseModelDialogEventHandler);
            EventHelper<ReportDataSourceChangedEvent, ReportDataSourceChangedEventArgs>.SubscribeEvent(_eventAggregator, ReportDataSourceChangedEventHandler);
        }
        private void ReportDataSourceChangedEventHandler(ReportDataSourceChangedEventArgs obj)
        {
            RefreshReport();
        }

        private void RefreshReport()
        {
            reportViewer.Reset();
            this._reportWindowsFormHost.Visibility = System.Windows.Visibility.Visible;
            _reportService.SetGlobalPrintCommand((PrintCommand));
            _reportService.SetGlobalRePrintCommand((ReprintCommand));
            this.reportViewer.LocalReport.DataSources.Clear();

            this.reportViewer.LocalReport.SubreportProcessing += localReport_SubreportProcessing;
            var listSubReport = new string[] { "SureMedReport2Cols_Col4", "SureMedReport2Cols_Col3",
                "SureMedReport2Cols_Col2", "SureMedReport2Cols_Col1",
                "page2", "drugDetail", "drugDetailCol1", "drugDetailCol2" };
            ReportGenerator.GenerateReportByLanguageCode(reportViewer.LocalReport, listSubReport);
            BindingDataToReport();
            reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            var setup = reportViewer.GetPageSettings();
            setup.Landscape = false;
            reportViewer.SetPageSettings(setup);
            reportViewer.ZoomMode = ZoomMode.Percent;
            reportViewer.ZoomPercent = 100;
            reportViewer.RefreshReport();
        }

        private void CloseModelDialogEventHandler(CloseModelDialogEventArgs obj)
        {
            _reportWindowsFormHost.Visibility = Visibility.Visible;
        }

        private void ShowModalDialogEventHandler(ShowModalDialogEventArgs obj)
        {
            _reportWindowsFormHost.Visibility = Visibility.Hidden;
        }
        private void LoginEventHandler(string userName)
        {
            //Reserved code
            _currentUserName = userName;
            if (!string.IsNullOrEmpty(_currentUserName))
            {
                this._reportWindowsFormHost.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this._reportWindowsFormHost.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        private void CurrentModeChangedHandler(ReportCurrentModeChangedEventArgs currentMode)
        {
            //Reserved code
            switch (currentMode.ModeName)
            {
                case "ModeQueue":
                case "ModeViewOnly":
                case "DetailedSearchAction":
                    this._reportWindowsFormHost.Visibility = System.Windows.Visibility.Hidden;
                    break;
                default:
                    if (!string.IsNullOrEmpty(currentMode.UserName))
                    {
                        this._reportWindowsFormHost.Visibility = System.Windows.Visibility.Visible;
                    }
                    break;
            }
        }

        void BindingDataToReport()
        {
            tray = _reportService.GetTrayNew();
            if (tray != null)
            {
                var cartonLabel = tray.Carton != null ? tray.Carton.Label : null;
                if (isPrint)
                {
                    labelId = Guid.NewGuid().ToString();
                }
                else
                {
                    labelId = cartonLabel != null ? cartonLabel.LabelId : Guid.Empty.ToString();
                }
                isMultiLocation = tray.Locations.Count > 1;
                var models = new List<SureMedRdlcReportOutsideInfo>();
                var isMultiPatient = tray.Patients.Count() > 1;
                var patient = tray.Patients.FirstOrDefault().Value;
                var cupFirst = tray.Cups.OrderBy(x => x.IntakeDateTime).FirstOrDefault();
                var location = tray.Locations.FirstOrDefault().Value;
                var customerAdrress = location.Customer.Addresses.FirstOrDefault(o => ("Medication packing address").Equals(o.AddressType));
                Bitmap imagetemp = null;
                if (patient.PhotoImage != null)
                {
                    imagetemp = (Bitmap)patient.PhotoImage.Clone();
                }
                var model = new SureMedRdlcReportOutsideInfo()
                {
                    PatientName = isMultiPatient ? Linguist.Phrase("ErrorDataTitle") : patient.FullName.Length > 25 ? patient.FullName.Substring(0,25): patient.FullName,
                    PatientPhoto = patient.PhotoImage != null && !isMultiPatient ? Utils.imageToByteArray(imagetemp) : new byte[0],
                    RoomBedInfo = buildBedRoomInfo(patient, isMultiPatient),
                    PatientId = isMultiPatient ? Linguist.Phrase("ErrorDataTitle") : patient.PatientId,
                    PatientIdWithPrefix = Linguist.Phrase("PatientIdWithColonTitle") + " " + patient.PatientId,
                    StartDate = cupFirst.IntakeDateTime.ToString(defaultFormatDate),
                    PatientDob = isMultiPatient
                        ? Linguist.Phrase("ErrorDataTitle")
                        : (patient.DateOfBirth != DateTime.MinValue ? Linguist.Phrase("DOBTitle") + patient.DateOfBirth.ToString(defaultFormatDate) : ""),
                    Facility = isMultiPatient ? Linguist.Phrase("ErrorDataTitle") : cupFirst.Location.Institution.Name,
                    DayOfWeekStartDatestr = isMultiPatient ? Linguist.Phrase("ErrorDataTitle") : cupFirst.IntakeDateTime.ToString("dddd") + " " + cupFirst.IntakeDateTime.ToString("MM/dd/yyyy"),
                    DayOfWeek = cupFirst.IntakeDateTime.ToString("dddd"),
                    PackXofX = Linguist.Phrase("PackTitle") + " " + (tray.SequenceTrayNumberByPatient <= 0 ? 1 : tray.SequenceTrayNumberByPatient) + " " + Linguist.Phrase("OfTitle") + " " + (tray.TotalTrayInBatchByPatient <= 0 ? 1 : tray.TotalTrayInBatchByPatient),
                    PatientBigPhoto = patient.PhotoImage != null && !isMultiPatient ? Utils.imageToByteArray(imagetemp) : new byte[0],
                    PharmacyInfo = location.Customer.Name,
                    PharmacyAddress = customerAdrress != null ? buildCustomerAddress(customerAdrress) : "",
                    BarcodeImage = Utils.CreateQRCode(labelId),
                    CompanyLogo = tray.Batch.Order.CompanyLogo != null ? Utils.imageToByteArray(tray.Batch.Order.CompanyLogo) : new byte[0],
                    PromoLogo = tray.Batch.Order.Template1 != null ? Utils.imageToByteArray(Utils.rotateInternalFunction(tray.Batch.Order.Template1, RotateFlipType.Rotate270FlipNone)) : new byte[0],
                    DateCreated = Linguist.Phrase("DatePackedWithColonTitle") + " " + tray.DateCreated.ToString(defaultFormatDate),
                    IsMultiPatient = isMultiPatient,
                    Phone = location.Customer.Phone1,
                    ExpireDate = Linguist.Phrase("ExpireDateWithColonTitle") + " " + Helper.CalculateExpireDate(tray, cupFirst, Settings.Default.MinimumExpirePeriod).ToString(defaultFormatDate),
                    Unit = patient.Unit,
                    Location = isMultiLocation ? Linguist.Phrase("ErrorDataTitle") : tray.Locations.FirstOrDefault().Value.LocationId,
                    PhoneLabel = Linguist.Phrase("PhoneWithColonTitle"),
                    Dea = Linguist.Phrase("Dea#WithColonTitle"),
                    DeaValue = location.Customer.DeaNumber,
                    CautionValue = Linguist.Phrase("Warning") + Environment.NewLine + Linguist.Phrase("Caution"),
                    PatientStreet = patient.Addresses.FirstOrDefault() != null ? patient.Addresses.FirstOrDefault().Street : "",
                    PatientZipCode = patient.Addresses.FirstOrDefault() != null ? patient.Addresses.FirstOrDefault().ZipPostalCode + " " + patient.Addresses.FirstOrDefault().City : ""
                };

                models.Add(model);
                ReportDataSource reportDataSource1 =
                    new Microsoft.Reporting.WinForms.ReportDataSource();
                reportDataSource1.Name = "SureMedPlusCardOutsideInfoDataset";
                reportDataSource1.Value = models;
                this.reportViewer.LocalReport.DataSources.Add(reportDataSource1);
                buildDataSubReport();
            }
        }

        private void buildDataSubReport()
        {
            listCup4 = new List<SureMedRdlcReportOutsideCupInfo>();
            listCup3 = new List<SureMedRdlcReportOutsideCupInfo>();
            listCup2 = new List<SureMedRdlcReportOutsideCupInfo>();
            listCup1 = new List<SureMedRdlcReportOutsideCupInfo>();
            listPill = new List<SureMedRdlcReportOutsidePillInfo>();
            #region list cup            
            var rowNum = 0;
            var colNum1 = 0;
            var colNum2 = 0;
            var colNum3 = 0;
            var colNum4 = 0;

            var isMultiPatient = tray.Patients.Count() > 1;
            var firstCupByIntakeDateTime = tray.Cups.OrderBy(o => o.IntakeDateTime).FirstOrDefault();
            for (int i = 1; i <= 28; i++)
            {
                var cupInfo = new SureMedRdlcReportOutsideCupInfo();
                var cup = tray.Cups.FirstOrDefault(x => x.Number == i);
                if (cup != null)
                {
                    var listDoseDispense = new List<string> { "a", "b", "c", "d" };
                    var isDoesDispen = listDoseDispense.Contains(cup.TimeSlotCode);
                    if (cup.Number == firstCupByIntakeDateTime.Number)
                    {
                        cupInfo.FirstCup = true;
                    }

                    //defaultFormatDate = languageCode == "bg-BG" ? "dd.MM.yyyy" : "dd-MM-yy";
                    cupInfo.IntakeDateTimeStr = (isDoesDispen ? cup.TimeSlotDescription : cup.IntakeTimeDescription);
                    cupInfo.IntakeIcon = isDoesDispen ? (cup.TimeSlotIcon != null ? Utils.imageToByteArray(cup.TimeSlotIcon) : new byte[0]) : (cup.IntakeTimeIcon != null ? Utils.imageToByteArray(cup.IntakeTimeIcon) : new byte[0]);
                    cupInfo.PatientName = isMultiPatient ? Linguist.Phrase("ErrorDataTitle") : cup.Patient.FullName.ToUpper();
                    cupInfo.TimeSlotColor = isDoesDispen ? cup.TimeSlotColor : cup.IntakeTimeColor;
                    cupInfo.PrintTime = isDoesDispen ? cup.TimeSlotDescription : cup.IntakeTimeDescription;
                    cupInfo.IsMultiPatient = isMultiPatient;
                    cupInfo.DayName = cup.IntakeDateTime
                        .ToString("dddd");
                    #region list pill info
                    var medicines = CalculateWholePillsInCup(cup, isDoesDispen);
                    listPill.AddRange(medicines);
                    #endregion
                }

                cupInfo.cupNumber = i;
                if (i % 4 == 0)
                {
                    cupInfo.row = rowNum + 1;
                    cupInfo.col = colNum4 + 1;
                    colNum4++;
                    listCup4.Add(cupInfo);
                }
                if (i % 4 == 3)
                {
                    cupInfo.row = rowNum + 2;
                    cupInfo.col = colNum3 + 1;
                    colNum3++;
                    listCup3.Add(cupInfo);
                }
                if (i % 4 == 2)
                {
                    cupInfo.row = rowNum + 3;
                    cupInfo.col = colNum2 + 1;
                    colNum2++;
                    listCup2.Add(cupInfo);
                }
                if (i % 4 == 1)
                {
                    cupInfo.row = rowNum + 4;
                    cupInfo.col = colNum1 + 1;
                    colNum1++;
                    listCup1.Add(cupInfo);
                }
            }
            foreach (var item in listCup1)
            {
                listCup4.FirstOrDefault(x => x.col == item.col).DayName1 = item.DayName;
            }

            foreach (var item in listCup2)
            {
                listCup4.FirstOrDefault(x => x.col == item.col).DayName2 = item.DayName;
            }

            foreach (var item in listCup3)
                listCup4.FirstOrDefault(x => x.col == item.col).DayName3 = item.DayName;

            foreach (var item in listCup4)
            {
                var listDayName = new List<string>();
                if (!string.IsNullOrEmpty(item.DayName))
                    listDayName.Add(item.DayName);
                if (!string.IsNullOrEmpty(item.DayName1))
                    listDayName.Add(item.DayName1);
                if (!string.IsNullOrEmpty(item.DayName2))
                    listDayName.Add(item.DayName2);
                if (!string.IsNullOrEmpty(item.DayName3))
                    listDayName.Add(item.DayName3);
                var count = listDayName.Distinct().Count();
                item.DayName = count > 1 ? "" : listDayName.Distinct().FirstOrDefault();
            }
            #endregion
        }

        private decimal calculateExpectedAmount(int expectedAmount, BasePillData pillInfo)
        {
            var decimalRepresentation = pillInfo != null && !"".Equals(pillInfo.DecimalRepresentation) ? Convert.ToDecimal(pillInfo.DecimalRepresentation) : 0;
            return expectedAmount + decimalRepresentation;
        }

        private void localReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            #region process sub report
            if (e.ReportPath == "SureMedReport2Cols_Col4")
            {
                e.DataSources.Clear();
                e.DataSources.Add(new ReportDataSource("CupsInfoDataSet", listCup4));
            }
            if (e.ReportPath == "SureMedReport2Cols_Col3")
            {
                e.DataSources.Clear();
                e.DataSources.Add(new ReportDataSource("CupsInfoDataSetCol3", listCup3));
            }
            if (e.ReportPath == "SureMedReport2Cols_Col2")
            {
                e.DataSources.Clear();
                e.DataSources.Add(new ReportDataSource("CupsInfoDataSetCol2", listCup2));
            }
            if (e.ReportPath == "SureMedReport2Cols_Col1")
            {
                e.DataSources.Clear();
                e.DataSources.Add(new ReportDataSource("CupsInfoDataSetCol1", listCup1));
            }
            if (e.ReportPath == "drugDetail" || e.ReportPath == "drugDetailCol1" || e.ReportPath == "drugDetailCol2")
            {
                int cupNum = Convert.ToInt32(e.Parameters[0].Values[0]);
                e.DataSources.Clear();
                var pillInfoList = listPill.FindAll(element => element.cupNumber == cupNum);
                if (pillInfoList.Count() <= 7)
                {
                    for (int i = pillInfoList.Count(); i <= 7; i++)
                    {
                        var cupDetailItem = new SureMedRdlcReportOutsidePillInfo();
                        cupDetailItem.PillInfoCol1 = " ";
                        cupDetailItem.RowIdx = i;
                        cupDetailItem.cupNumber = cupNum;
                        pillInfoList.Add(cupDetailItem);
                    }
                }
                e.DataSources.Add(new ReportDataSource
                {
                    Name = "PillInfoDataSet",
                    Value = pillInfoList.Take(7)
                });
            }
            if (e.ReportPath == "page2")
            {
                e.DataSources.Clear();
                var listPillInside = buildPillInside();
                e.DataSources.Add(new ReportDataSource("PillInfoPage2DataSet", listPillInside.Take(12)));
            }
            #endregion
        }

        string GetColNameSub(string colName)
        {
            if (!string.IsNullOrEmpty(colName))
            {
                switch (colName)
                {
                    case "morning":
                        return Linguist.Phrase("mornSub");
                    case "noon":
                        return Linguist.Phrase("noonSub");
                    case "evening":
                        return Linguist.Phrase("eveSub");
                    case "night":
                        return Linguist.Phrase("nightSub");
                }

            }
            return "";
        }
        private List<MedicineData> buildMedicines(CupData cup)
        {
            var medicines = cup.Medicines;
            var listMedicineTemp = new List<MedicineData>();
            var listMedicineFragmentTemp = new List<MedicineData>();
            foreach (var item in medicines)
            {
                var isFragment = item.PillId.Split('_').Count() > 1;
                if (item.PillInfo != null)
                {
                    if (("").Equals(item.PillInfo.FragmentSuffix))
                    {
                        listMedicineTemp.Add(item);
                    }
                    else if (isFragment)
                    {
                        listMedicineFragmentTemp.Add(item);
                    }
                }
            }
            foreach (var item in listMedicineFragmentTemp)
            {
                var pillId = item.PillId.Split('_')[0];
                var medicine = listMedicineTemp.FirstOrDefault(x => x.PillId == pillId);
                if (medicine != null)
                {
                    listMedicineTemp.FirstOrDefault(x => x.PillId == pillId).PillInfo.DecimalRepresentation = item.PillInfo.DecimalRepresentation;
                    //medicine.PillInfo.FragmentSysnosis = medicine.PillInfo.FragmentSuffix == "" ? "" : (Convert.ToDecimal(medicine.PillInfo.DecimalRepresentation) + Convert.ToDecimal(item.PillInfo.DecimalRepresentation)).ToString();
                    //medicine.PillInfo.DecimalRepresentation = item.PillInfo.DecimalRepresentation;
                }
            }
            return listMedicineTemp;
        }

        private List<SureMedRdlcReportInsideInfo> buildPillInside()
        {
            var listPillGroup = listPill.GroupBy(p => new { p.PillId }, (key, group) => new { pillId = key.PillId, pillInfo = group });
            var listPillInside = new List<SureMedRdlcReportInsideInfo>();
            listImagePill = new List<Image>();
            foreach (var item in listPillGroup.OrderBy(p => p.pillId))
            {
                if (item.pillId != null)
                {
                    var pillGroup = listPillGroup.FirstOrDefault(o => o.pillId == item.pillId);

                    var medicine = Helper.GetMedicineData(tray, item.pillId);
                    var pillInside = new SureMedRdlcReportInsideInfo();
                    pillInside.NationalDrugCode = Linguist.Phrase("NDCPrefix") + item.pillId.ToUpper();
                    var listDispense = GetChargeAndExpireDate();
                    var dispenseInfo = listDispense.FirstOrDefault(x => x.PillId.Contains(item.pillId));
                    var LotTemp = dispenseInfo != null ? dispenseInfo.ChargeNumberList : "";
                    var col1Name = Helper.CalculateIntakeTime(tray, 1);
                    var col2Name = Helper.CalculateIntakeTime(tray, 2);
                    var col3Name = Helper.CalculateIntakeTime(tray, 3);
                    var col4Name = Helper.CalculateIntakeTime(tray, 0);
                    pillInside.Col1Name = col1Name.Length > 5 ? col1Name.Substring(0, 5) : col1Name;
                    pillInside.Col2Name = col2Name.Length > 5 ? col2Name.Substring(0, 5) : col2Name;
                    pillInside.Col3Name = col3Name.Length > 5 ? col3Name.Substring(0, 5) : col3Name;
                    pillInside.Col4Name = col4Name.Length > 5 ? col4Name.Substring(0, 5) : col4Name;
                    if (medicine.PillInfo != null)
                    {
                        pillInside.Manufacturer = (Linguist.Phrase("MgfWithColonTitle") + " " + medicine.PillInfo.ManufactureName).Length> 30? (Linguist.Phrase("MgfWithColonTitle") + " " + medicine.PillInfo.ManufactureName).Substring(0,30): (Linguist.Phrase("MgfWithColonTitle") + " " + medicine.PillInfo.ManufactureName);
                        pillInside.BrandName = Linguist.Phrase("BrandWithColonTitle") + " " + medicine.PillInfo.BrandNameShort;
                        pillInside.DrugName = medicine.PillInfo.ItemShortName;
                        pillInside.Description = medicine.PillInfo.Description;
                        pillInside.Shape = medicine.PillInfo.Shape;
                        pillInside.Color = medicine.PillInfo.Color;
                        pillInside.Imprint = medicine.PillInfo.Imprint;
                        pillInside.LotNumber = Linguist.Phrase("LotWithColonTitle") + "" + LotTemp;
                        pillInside.RxNumber = Linguist.Phrase("RxWithColonTitle") + " " + medicine.PrescriptionNumber;
                        pillInside.RxOrigBarCode = Utils.imageToByteArray(GenCode128.Code128Rendering.MakeBarcodeImage(medicine.PrescriptionNumberOriginal, 2, true));
                        pillInside.DrName = Linguist.Phrase("DrNameWithColonTitle") + " " + medicine.PrescribingPhysician;
                        pillInside.Instructions = medicine.MedicationsInstruction;
                        pillInside.Model1 = medicine.PillInfo.Model1 != null ? Utils.imageToByteArray(medicine.PillInfo.Model1) : new byte[0];
                        pillInside.Model2 = (medicine.PillInfo.Model2 != null) ? Utils.imageToByteArray(medicine.PillInfo.Model2) : new byte[0];
                        //if (medicine.PillInfo.Model1 != null)
                        //{
                            //listImagePill.Add(medicine.PillInfo.Model1);
                            //pillInside.ImgModel1 = medicine.PillInfo.Model1;
                        //}
                        //if (medicine.PillInfo.Model2 != null)
                        //{
                            //listImagePill.Add(medicine.PillInfo.Model2);
                            //pillInside.ImgModel2 = medicine.PillInfo.Model2;
                        //}
                        //if (medicine.PillInfo.Model3 != null)
                        //{
                            //listImagePill.Add(medicine.PillInfo.Model3);
                            //pillInside.ImgModel3 = medicine.PillInfo.Model3;
                        //}
                        //if (medicine.PillInfo.Model4 != null)
                        //{
                            //listImagePill.Add(medicine.PillInfo.Model4);
                            //pillInside.ImgModel4 = medicine.PillInfo.Model4;
                        //}
                    }
                    if (pillGroup != null && pillGroup.pillInfo != null)
                    {
                        foreach (var pill in pillGroup.pillInfo)
                        {
                            if (pill != null)
                            {
                                switch ((pill.cupNumber + 3) % 4)
                                {
                                    case 0:
                                        pillInside.NumMorn += pill.Amount;
                                        break;
                                    case 1:
                                        pillInside.NumNoon += pill.Amount;
                                        break;
                                    case 2:
                                        pillInside.NumEve += pill.Amount;
                                        break;
                                    case 3:
                                        pillInside.NumNight += pill.Amount;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    listPillInside.Add(pillInside);
                }
              }
            //var size = ProcessImageModelPill.SetNewImageSize(listImagePill);

            //foreach (var item in listPillInside)
            //{
                //item.Model1 = ProcessImageModelPill.ResizePillModels(item.ImgModel1, size);
                //item.Model2 = ProcessImageModelPill.ResizePillModels(item.ImgModel2, size);
            //}
            return listPillInside;
        }
        private List<PillsDispense> GetChargeAndExpireDate()
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
            var dispenseList = new List<PillsDispense>();

            foreach (var x in query)
            {
                var chargeList = new List<string>();
                var expireDate = DateTime.MaxValue;

                foreach (var b in x.BcoRefill)
                {
                    chargeList.Add(b.ChargeNumber);
                    if (expireDate > b.ExpireDate)
                        expireDate = b.ExpireDate;
                }

                foreach (var c in x.CartridgeDispense)
                {
                    chargeList.Add(c.ChargeNumber);
                    if (expireDate > c.ExpireDate)
                        expireDate = c.ExpireDate;
                }

                foreach (var m in x.MdaDispense)
                {
                    chargeList.Add(m.ChargeNumber);
                    if (expireDate > m.ExpireDate)
                        expireDate = m.ExpireDate;
                }
                var chargeListText =
                    string.Join(",",
                        chargeList.Distinct().ToArray().Take(3));
                dispenseList.Add(new PillsDispense
                {
                    PillId = x.PillId,
                    ChargeNumberList = chargeListText,
                    ExipreDate = expireDate
                });
            }
            return dispenseList;
        }

        private string buildBedRoomInfo(PatientData patient, bool isMultiPatient)
        {
            var result = string.IsNullOrEmpty(patient.Room) ? "" : (isMultiPatient ? Linguist.Phrase("ErrorDataTitle") : patient.Room);
            return result;
        }
        private string buildCustomerAddress(AddressData customerAddress)
        {
            var result = "";
            result = result + (string.IsNullOrEmpty(customerAddress.Street) ? "" : customerAddress.Street + Environment.NewLine);
            result = result + (string.IsNullOrEmpty(customerAddress.City) ? "" : customerAddress.City + (string.IsNullOrEmpty(customerAddress.StateProvince) ? "" : ", " + customerAddress.StateProvince) + (string.IsNullOrEmpty(customerAddress.ZipPostalCode) ? "" : " " + customerAddress.ZipPostalCode) + Environment.NewLine);
            return result;
        }
        private bool CanPrintLabel(object arg)
        {
            return true;
        }
        
        private void Print()
        {
            RefreshReport();
            string printerName = _reportService.GetPrinterNameForReport();
            PaperSize NewSize = new PaperSize();
            NewSize.RawKind = (int)PaperKind.Custom;
            NewSize.Width = 950;
            NewSize.Height = 1620;
            RDLCPrinter rdlcPrinter = new RDLCPrinter(reportViewer.LocalReport);
            rdlcPrinter.Print(printerName, 0, Duplex.Horizontal, NewSize, true);
            _reportService.SetReportEvidence(rdlcPrinter.GetPDFArray(), labelId);
        }

        private void OnPrintLabel(object obj)
        {
            isPrint = true;
            Print();
            isPrint = false;
        }
        private void OnRePrintLabel(object obj)
        {
            Print();
        }
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            RefreshReport();
        }
        private List<SureMedRdlcReportOutsidePillInfo> CalculateWholePillsInCup(CupData cup, bool isDoesDispen)
        {
            List<SureMedRdlcReportOutsidePillInfo> result = new List<SureMedRdlcReportOutsidePillInfo>();
            var sortedMedicineByPillId = cup.Medicines.OrderBy(x => x.PillId);
            int currentPill = 0;
            string currentPillId = string.Empty;
            decimal currentCount = 0;
            Regex regex = new Regex(@"^\w*(?=_)");
            var index = 0;
            var description = string.Empty;
            foreach (var medicine in sortedMedicineByPillId)
            {
                if (medicine.PillInfo != null)
                {
                    Match matchPillId = regex.Match(medicine.PillId);
                    currentPill++;
                    decimal number = (string.IsNullOrEmpty(medicine.PillInfo.DecimalRepresentation) ? 1 : decimal.Parse(medicine.PillInfo.DecimalRepresentation, CultureInfo.InvariantCulture))
                                     * medicine.ExpectedAmount;
                    if (string.IsNullOrEmpty(currentPillId))
                    {
                        currentPillId = matchPillId.Value != "" ? matchPillId.Value : medicine.PillId;
                        currentCount = number;
                        description = medicine.PillInfo.Description.ToUpper();
                    }
                    else if (currentPillId == medicine.PillId)
                    {
                        currentCount = currentCount + number;
                    }
                    else if (currentPillId != medicine.PillId)
                    {
                        if (matchPillId.Success && matchPillId.Value == currentPillId) // fragment
                        {
                            currentCount = currentCount + number;
                        }
                        else
                        {
                            result.Add(new SureMedRdlcReportOutsidePillInfo
                            {
                                Amount = currentCount,
                                cupNumber = cup.Number,
                                PillId = currentPillId,
                                PillInfoCol1 = currentCount.ToString(),
                                PillInfoCol2 = description,
                                RowIdx = index++
                            });
                            currentPillId = matchPillId.Value != "" ? matchPillId.Value : medicine.PillId;
                            currentCount = number;
                            description = medicine.PillInfo.Description.ToUpper();
                        }
                    }
                    if (currentPill == sortedMedicineByPillId.Count())
                    {
                        result.Add(new SureMedRdlcReportOutsidePillInfo
                        {
                            Amount = currentCount,
                            cupNumber = cup.Number,
                            PillId = currentPillId,
                            PillInfoCol1 = currentCount.ToString(),
                            PillInfoCol2 = description,
                            RowIdx = index++
                        });
                    }
                }
            }
            return result;
        }
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _reportService.SetGlobalPrintCommand(null);
            _reportService.SetGlobalRePrintCommand(null);
            this._reportWindowsFormHost.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
