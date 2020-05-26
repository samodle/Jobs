using Analytics;
using DataInterface;
using ForkAnalyticsSettings;
using Microsoft.VisualBasic;
using ProductionLines;
using RawData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Telerik.Charting;
using Telerik.Windows.Controls.ChartView;
using Windows_Desktop.Properties;
using static DataPersistancy.GeneralIO;
using static ForkAnalyticsSettings.GlobalConstants;
using static Windows_Desktop.Window_Dashboard_Settings;
using Excel = Microsoft.Office.Interop.Excel;


namespace Windows_Desktop
{
    public partial class dashboardwindow : Window
    {
        #region Variables

        public bool DEMO_MODE = true; //if true, generate random data
                                      //  Dashboard_Intermediate_Single intermediateSheet;
        public int TierAscrolloffset = 0;                             //Scrolloffset default values for each Card. Left click decreases value by 1, right click increments by 1
        public int TierBscrolloffset = 0;                             //Scrolloffset default values for each Card. Left click decreases value by 1, right click increments by 1
        public int TierCscrolloffset = 0;                             //Scrolloffset default values for each Card. Left click decreases value by 1, right click increments by 1

        public int clickedlabelIndex_scroll_TierA = -1;                 //run-time saved scrolloffset for a selectedlabel
        public int clickedlabelIndex_scroll_TierB = -1;
        public int clickedlabelIndex_scroll_TierC = -1;

        public int clickedlabelindex_labelpos_TierA = -1;               //run-time saved index of clicked UI label number for each card 
        public int clickedlabelindex_labelpos_TierB = -1;
        public int clickedlabelindex_labelpos_TierC = -1;

        public static Thickness OriginalThickness_DowntimeChartsViewBoxA;  // original positions for tier A chart
        public static Thickness OriginalThickness_DowntimeChartsViewBoxB;  // original positions for tier B chart
        public static Thickness OriginalThickness_DowntimeChartsViewBoxC;  // original positions for tier C chart
        public static Thickness OriginalThickness_TopLineCharts;           // original position of top line charts (planned, un planned, rate loss)

        public static Thickness OriginalThickness_Dailyincontrol_Lossinfocanvas;  // original positions for incontrol daily lossinfo canvas

        public static double Originalwidth_Dailyincontrolborder;
        public static Thickness OriginalThickness_Dailyincontrol_divider;
        public static double OriginalWIdth_Dailyincontrolgraphicsareacanvas;

        public int mousepositionX = -1;
        public int mousepositionY = -1;

        public bool[] KPIlock = new bool[5];                          // 5 KPIs on the right. If KPI is locked, value is true.
        public string losscompass_primarykpi = "";                    // Primary KPIs are one of 5 KPIs on the right. 
        public string losscompass_secondarykpi = "";                  // Secondary KPIs are selected from a dropdown when 'Include a Secondary KPI' is clicked
        public int losscompass_secondarykpiselected = -1;
        public bool ISSecondaryAxisOn = false;                       // If secondary KPI is on, shapes show on the cards in addition to bars.
        Dashboard_Intermediate_Single intermediate;                          // intermediate sheet contains all pre calculated values used in the UI

        public int Menuitemclicked_number = -1;

        public int LineTrends_analysistimeperiod = 1;               // For trends chart - if we want daily chart - value is 1, weekly is 7, monthly is 30
        public int LineTrends_Mode_analysistimeperiod = 1;            // Same as Line trends, but for selected failure mode
        public int LineTrends_Step_analysistimeperiod = 1;

        public bool IScrystallballON = false;
        public string ActiveToolTip_FailureModename = "";
        public string ActiveToolTip_Cardname = "";
        public CardTier ActiveToolTip_Card = CardTier.NA;
        public CardTier TempCardTier_ForSimulator = CardTier.NA;
        public bool IsMultiLineActive = true; //false by default, controls tool tip behavior

        #endregion

        #region UIConstants
        public static double TierACanvaswidth = 510;
        public static double TierBCanvaswidth = 510;
        public static double TierCCanvaswidth = 1025;

        public static double TierACanvasheight = 270;
        public static double TierBCanvasheight = 270;
        public static double TierCCanvasheight = 275;

        public static int TierAmaxVisibleLabels = 6;
        public static int TierBmaxVisibleLabels = 6;
        public static int TierCmaxVisibleLabels = 8;


        public static double Tiers_fieldlabelwidth = 74;
        public static double Tiers_fieldlabelheight = 20;
        public static double Tiers_fieldlabeloffset = 15;
        public static double Tiers_fieldlabel_POS_top = 230;
        public static double Tiers_bubble_POS_top = 225;
        public static double Tiers_column_POS_top = 227;

        public static double Tiers_bubbleheight = 15;
        public static double Tiers_bubblewidth = 15;

        public static double TierAdatalabeltopoffset = 57;
        public static double TierBdatalabeltopoffset = 57;
        public static double TierCdatalabeltopoffset = 50;

        public static double TierAbubbletopoffset = 57;
        public static double TierBbubbletopoffset = 57;
        public static double TierCbubbletopoffset = 50;



        public static double Tiers_columnwidth = 35;
        public static double Tiers_columnheight = 160;
        public double Tiers_columnoffset = Tiers_fieldlabeloffset + (Tiers_fieldlabelwidth - Tiers_columnwidth) / 2;

        public static double Tiers_datalabelwidth = 30;
        public static double Tiers_datalabelheight = 20;
        public double Tiers_datalabeloffset = Tiers_fieldlabeloffset + (Tiers_fieldlabelwidth - Tiers_datalabelwidth) / 2;

        public double Tiers_bubbleoffset = Tiers_fieldlabeloffset + (Tiers_fieldlabelwidth - Tiers_bubblewidth) / 2;

        public CardTier MappingOriginCard = CardTier.A;

        //toggle between column and line
        public bool StepTrends_isLineGraph = true;
        public bool LineTrends_isLineGraph = true;
        public bool LossTrends_isLineGraph = true;

        //toggle on and off rollup values
        public bool StepTrends_showRollUp = true;
        public bool LineTrends_showRollUp = false;
        public bool LossTrends_showRollUp = false;

        public bool StepTrends_showRollUpOnly = false;
        public bool LineTrends_showRollUpOnly = false;
        public bool LossTrends_showRollUpOnly = false;


        public List<DowntimeMetrics> ListofSelectedKPI_LineTrends = new List<DowntimeMetrics>();
        public List<DowntimeMetrics> ListofSelectedKPI_ModeTrends = new List<DowntimeMetrics>();
        public List<DowntimeMetrics> ListofSelectedKPI_StepTrends { get; set; } = new List<DowntimeMetrics>();

        public List<string> ListofSelectedFailureModeTrends_unplanned = new List<string>();
        public List<string> ListofSelectedFailureModeTrends_planned = new List<string>();

        public List<string> Trends_Step_SelectedFailureModes_Unplanned = new List<string>();
        public List<string> Trends_Step_SelectedFailureModes_Planned = new List<string>();

        //forkstory
        public double verticalgapbetweencards = 10;
        public double heightofcard = 250;
        public double widthofcard = 750;
        public double LeftPosSettingsIcon = 730;
        public double TopPosSettingsicon = 10;
        public double LeftPoscardnamelabel = 10;
        public double TopPoscardnamelabel = 20;
        public double cardnamelabelheight = 25;
        public double cardnamelabelwidth = 600;
        public double LeftPoscardsubheadinglabel = 50;
        public double TopPoscardsubheadinglabel = 10;
        public double LeftPosmaincontentcard = 10;
        public double subheadinglabelheight = 10;
        public double subheadinglabelwidth = 500;
        public double TopPosmaincontentcard = 65;

        public int LiveLineTrends_TimeFrame = 1;
        public double LossNetwork_MaxRadius = 0;

        public List<string> ListofLossTrendsLegends = new List<string>();
        public List<string> ListofStepChangeTrendsLegends = new List<string>();
        public List<string> ListofLineTrendsLegends = new List<string>();

        public bool ISGlidepathOn = false;
        public string StepChange_selectedmode = "DT%";

        //Canvasyourresultsyourway
        public bool IsCanvasOn = false;
        public int LastPickedCanvas = 0;
        public int LastInsertedAnnotation = 0;
        public bool IsCanvasLaunchedFirstTime = true;


        //Gap Analysis
        public List<double> ListofLevel2ContainerHeight = new List<double>();
        public string gapanalysis_clickedlevel = "one";
        public int gapanalysis_activefmno = -1;
        public DowntimeMetrics gapanalysis_activeDTmetric;

        #endregion

        public void fork_onload(object sender, RoutedEventArgs e)
        {
            InitializeComponent();

            LaunchCanvas.Visibility = Visibility.Visible;

            MakeLaunchReady();

            forkmtdoption_MouseDown();
        }
        public void MakeReportsReady()
        {
            OriginalThickness_Dailyincontrol_Lossinfocanvas = ToplineresultsCanvas.Margin;

            OriginalThickness_Dailyincontrol_Lossinfocanvas = Dailyincontrol_LossInfoCanvas.Margin; // this is for the program to remember what are the default positions of incontrol bubbles canvas
            OriginalThickness_Dailyincontrol_divider = Dailyincontrol_graphicsarea_divider.Margin;
            Originalwidth_Dailyincontrolborder = DailyincontrolBorder.Width;
            OriginalWIdth_Dailyincontrolgraphicsareacanvas = DailyincontrolGraphicsArea.Width;

            CloseMenu(MenuCanvas, Publics.f);
            ManageUIDate(intermediate.startTime, intermediate.endTime);      // Header dates on right are told what their values should be
            Generate_LossCompass_MainCards("A", 6);
            Generate_LossCompass_MainCards("B", 6);
            Generate_LossCompass_MainCards("C", 8);

            LossCompass_OnLoad();                          // Loss compass initialization 

            ManageScreenResolution();                      // To make it fit for use on any screen - maximize the program if screen resolution of device is less than a threshold, to make the UI legible
            Glidepath_Turnoff();                           // Glidepath in Trends window are turned off by default
            ManageFloatingToolTip_forMultiline();          // The tool tip's buttons are adjusted based on whether multiline is ON or OFF. 

            OriginalThickness_DowntimeChartsViewBoxA = getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Margin;    // this is for the program to remember what are the default positions of Tier A, B and C cards
            OriginalThickness_DowntimeChartsViewBoxB = getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Margin;    // this is for the program to remember what are the default positions of Tier A, B and C cards
            OriginalThickness_DowntimeChartsViewBoxC = getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Margin;    // this is for the program to remember what are the default positions of Tier A, B and C cards

            // ToggleShowHide_LossCompass(LossCompassCanvas, Publics.f);
            ToggleShowHide_LiveLine(LiveLineCanvas, Publics.f);
        }
        public void ManageScreenResolution()     // To make it fit for use on any screen - maximize the program if screen resolution of device is less than a threshold, to make the UI legible
        {
            //Height = "706" Width = "1250"
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            if (screenWidth < 1250 | screenHeight < 706)
                this.WindowState = System.Windows.WindowState.Maximized;


        }


        #region Header             // Date management, TOP Kpis clicked


        public void ManageUIDate(DateTime startdate, DateTime enddate)
        {
            FromDateCanvas.Visibility = Visibility.Visible;
            ToDateCanvas.Visibility = Visibility.Visible;

            FROMdaylabel.Content = startdate.ToString("dd", CultureInfo.InvariantCulture);
            FROMmonthlabel.Content = startdate.ToString("MMM", CultureInfo.InvariantCulture).ToUpper();
            FROMyearlabel.Content = startdate.ToString("yyyy", CultureInfo.InvariantCulture);
            FROMtimelabel.Content = startdate.ToString("hh:mm tt", CultureInfo.InvariantCulture);

            TOdaylabel.Content = enddate.ToString("dd", CultureInfo.InvariantCulture);
            TOmonthlabel.Content = enddate.ToString("MMM", CultureInfo.InvariantCulture).ToUpper();
            TOyearlabel.Content = enddate.ToString("yyyy", CultureInfo.InvariantCulture);
            TOtimelabel.Content = enddate.ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }
        public void InitiateHeadervalues()
        {

            Label lbl;
            int j;
            Label templabel;
            Canvas dep = HeaderKPIcanvas;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(lbl);
                }

            }

            GenerateLabelUI(HeaderKPIcanvas, "header1Val", 40, 65, 0, 0, null, BrushColors.mybrushfontgray, 23, LaunchMultiLine_HeaderKPI, Generalmousemove, Generalmouseleave, -1, Math.Round(intermediate.LossCompass_OEE * 100, 1) + "%", false);
            GenerateLabelUI(HeaderKPIcanvas, "header1header", 45, 50, 70, 0, null, BrushColors.mybrushfontgray, 14, LaunchMultiLine_HeaderKPI, Generalmousemove, Generalmouseleave, -1, "Jobs", true);
            templabel = getMenuItem_Label_fromitemindex(HeaderKPIcanvas, -1, "header1Val");
            AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, templabel);

            GenerateLabelUI(HeaderKPIcanvas, "header2Val", 40, 40, 180, 0, null, BrushColors.mybrushfontgray, 23, LaunchMultiLine_HeaderKPI, Generalmousemove, Generalmouseleave, -1, "10", false);
            GenerateLabelUI(HeaderKPIcanvas, "header2header", 45, 70, 220, 0, null, BrushColors.mybrushfontgray, 14, LaunchMultiLine_HeaderKPI, Generalmousemove, Generalmouseleave, -1, "Skills", true);
            templabel = getMenuItem_Label_fromitemindex(HeaderKPIcanvas, -1, "header2Val");
            AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, templabel);

            GenerateLabelUI(HeaderKPIcanvas, "header3Val", 40, 50, 360, 0, null, BrushColors.mybrushfontgray, 23, LaunchMultiLine_HeaderKPI, Generalmousemove, Generalmouseleave, -1, "9000", false);
            GenerateLabelUI(HeaderKPIcanvas, "header3header", 45, 50, 415, 0, null, BrushColors.mybrushfontgray, 14, LaunchMultiLine_HeaderKPI, Generalmousemove, Generalmouseleave, -1, "Applicants", true);
            templabel = getMenuItem_Label_fromitemindex(HeaderKPIcanvas, -1, "header3Val");
            AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, templabel);


        }
        public void InitiateHeadervalues_Sim()
        {

            Label lbl;
            int j;
            Label templabel;
            Canvas dep = HeaderKPIcanvas;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(lbl);
                }

            }

            GenerateLabelUI(HeaderKPIcanvas, "header1Val", 25, 65, 0, 0, null, BrushColors.mybrushfontgray, 20, KPIHeader1Clicked, Generalmousemove, Generalmouseleave, -1, Math.Round(intermediate.LossCompass_OEE * 100, 1) + "%", false);
            GenerateLabelUI(HeaderKPIcanvas, "header1Sim", 25, 65, 0, 22, null, BrushColors.mybrushSelectedCriteria, 20, KPIHeader1Clicked, Generalmousemove, Generalmouseleave, -1, Math.Round(intermediate.LossCompass_OEE_Sim * 100, 1) + "%", false);
            GenerateLabelUI(HeaderKPIcanvas, "header1header", 45, 50, 70, 0, null, BrushColors.mybrushfontgray, 14, KPIHeader1Clicked, Generalmousemove, Generalmouseleave, -1, "Jobs", true);
            templabel = getMenuItem_Label_fromitemindex(HeaderKPIcanvas, -1, "header1Sim");
            AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, templabel);

            GenerateLabelUI(HeaderKPIcanvas, "header2Val", 25, 40, 180, 0, null, BrushColors.mybrushfontgray, 20, null, null, null, -1, "10", false);
            GenerateLabelUI(HeaderKPIcanvas, "header2Sim", 25, 40, 180, 22, null, BrushColors.mybrushSelectedCriteria, 20, null, null, null, -1, "20", false);
            GenerateLabelUI(HeaderKPIcanvas, "header2header", 45, 70, 220, 0, null, BrushColors.mybrushfontgray, 14, null, null, null, -1, "Skills", true);
            templabel = getMenuItem_Label_fromitemindex(HeaderKPIcanvas, -1, "header2Sim");
            AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, templabel);

            GenerateLabelUI(HeaderKPIcanvas, "header3Val", 25, 50, 360, 0, null, BrushColors.mybrushfontgray, 20, null, null, null, -1, "9000", false);
            GenerateLabelUI(HeaderKPIcanvas, "header3Sim", 25, 50, 360, 22, null, BrushColors.mybrushSelectedCriteria, 20, null, null, null, -1, "2000", false);
            GenerateLabelUI(HeaderKPIcanvas, "header3header", 45, 50, 415, 0, null, BrushColors.mybrushfontgray, 14, null, null, null, -1, "Applicants", true);
            templabel = getMenuItem_Label_fromitemindex(HeaderKPIcanvas, -1, "header3Sim");
            AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, templabel);
        }

        public void LaunchMultiLine_HeaderKPI(object sender, MouseButtonEventArgs e)
        {
            Label tempsender = new Label();
            string KPIname = "";
            if (sender.GetType().ToString().IndexOf("Label") > -1)
            {
                tempsender = (Label)sender;

            }

            switch (Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString())))
            {
                case 1:
                    KPIname = "Jobs";
                    break;
                case 2:
                    KPIname = "Stops/Day";
                    break;
                case 3:
                    KPIname = "Production";
                    break;
                default:
                    break;
            }


            MultiLineSplashCanvas.Visibility = Visibility.Visible;
            ActivateMultiLine_SummaryCanvas(MultiLineSummaryButton, Publics.f);
            MultiLineChartHeaderLabel.Content = KPIname;
        }

        public void LaunchLineSelectionMenu(object sender, MouseButtonEventArgs e)
        {
            LineSelectionCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, LineSelectionCanvas);
        }


        // #MULTILINE
        public void ConfirmLineSelectionClicked(object sender, MouseButtonEventArgs e)
        {
            var masterEventList = new List<List<DTevent>>();
            var selectedLines = new List<string>();
            foreach (object Name in LineSelectionlistBox.SelectedItems)
            {
                selectedLines.Add(Name.ToString());
            }



            LineSelectionCanvas.Visibility = Visibility.Hidden;

            List<string> tmpList = intermediate.Multi_getDataNeededLines(selectedLines);
            if (tmpList.Count > 0)
            {
                for (int i = 0; i < tmpList.Count; i++)
                {

                    List<DTevent> rawDataList = DemoMode.getDemoData(CurrentEndTime, tmpList[i]);

                    masterEventList.Add(rawDataList);
                }
                intermediate.Multi_AddDataForNewLines(masterEventList, tmpList, CurrentLineConfig);

                //Implement logic to update appropriate tool depending on lines selected
                if (LossNetworkCanvas.Visibility == Visibility.Visible)
                {
                    intermediate.LossNetwork_initialize();
                    LossNetwork_onload(null, Publics.f);
                }

                //Adding tooltip to show names of lines selected
                int k;
                LineNameLabel.ToolTip = "";
                if (selectedLines.Count > 1)
                {
                    LineNameLabel.Content = "Multiline";
                    LineNameLabel.ToolTip = "Multiple Lines/Systems Selected" + Environment.NewLine;
                    for (k = 0; k < intermediate.Multi_CurrentLineNames.Count; k++)
                    {
                        LineNameLabel.ToolTip = LineNameLabel.ToolTip + Environment.NewLine + intermediate.Multi_CurrentLineNames[k].ToString();
                    }
                }
                else if (selectedLines.Count == 1)
                {
                    LineNameLabel.Content = selectedLines[0];
                    LineNameLabel.ToolTip = "One Line/System Selected" + Environment.NewLine + Environment.NewLine + selectedLines[0].ToString();

                }



                intermediate.initializeTrends();                // refresh all Trends charts
                LineTrends_UpdateChartFromIntermediateSheet();  // refresh legends for line trends
                ModeTrends_UpdateChartFromIntermediateSheet();  // refresh legends for loss trends
                StepChange_UpdateChartFromIntermediateSheet();
            }

            //now update the UI accordingly
            InitiateChartsvalues();



            //Legends
            TrendsLineTrends_PrepareLegendsList();
            TrendsMode_PrepareLegendsList();
            TrendsStepChange_PrepareLegendsList();

        }

        public void CloseLineSelectionMenu(object sender, MouseButtonEventArgs e)
        {
            LineSelectionCanvas.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Menu
        public void LaunchMenu(object sender, MouseButtonEventArgs e)
        {
            MenuCanvas.Visibility = Visibility.Visible;
            AnimateMenuOpening();
            PopulateMenuItems();
            MenuSplashRectangle.Visibility = Visibility.Visible;
        }

        public void AnimateMenuOpening()
        {
            AnimateZoomUIElement_Margin(new Thickness(-280, 0, 1218, -4), new Thickness(0, 0, 938, -4), 0.15, MarginProperty, MenuCanvas);
        }
        public void AnimateMenuClosing()
        {
            //  AnimateZoomUIElement_Margin(new Thickness(0, 0, 938, -4), new Thickness(-280, 0, 1218, -4), 0.15, MarginProperty, MenuCanvas);
        }
        public void PopulateMenuItems()
        { }
        public void DestroyMenuItems()
        { }
        public void CloseMenu(object sender, MouseButtonEventArgs e)
        {
            MenuSplashRectangle.Visibility = Visibility.Hidden;
            AnimateMenuClosing();
            System.Windows.Forms.Application.DoEvents();
            MenuCanvas.Visibility = Visibility.Hidden;


        }
        public void Menuitemmousemove(object sender, MouseEventArgs e)
        {
            int menuitem = -1;
            if (sender.GetType().ToString().IndexOf("Image") > -1)
            {
                Image tempsender = (Image)sender;
                menuitem = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
                if (menuitem != -1)
                {

                    getMenuItem_Label_fromitemindex(getMenuItem_Canvas_fromitemindex(Menu_InternalInfiniteCanvas, -1, "", "Menu" + menuitem), -1, "", "Menu" + menuitem + "Label").Foreground = Brushes.Orange;
                    getMenuItem_Canvas_fromitemindex(Menu_InternalInfiniteCanvas, -1, "", "Menu" + menuitem).Background = BrushColors.mybrushverylightgray_forcardbackground;
                }
            }
            else if (sender.GetType().ToString().IndexOf("Label") > -1)
            {
                Label tempsender = (Label)sender;
                menuitem = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
                tempsender.Foreground = Brushes.Orange;
                //  getMenuItem_Image_fromitemindex(Menu_InternalInfiniteCanvas, menuitem);
                getMenuItem_Canvas_fromitemindex(Menu_InternalInfiniteCanvas, -1, "", "Menu" + menuitem).Background = BrushColors.mybrushverylightgray_forcardbackground;

            }


        }
        public void Menuitemmouseleave(object sender, MouseEventArgs e)
        {
            int menuitem = -1;
            if (sender.GetType().ToString().IndexOf("Image") > -1)
            {
                Image tempsender = (Image)sender;
                menuitem = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
                if (menuitem != Menuitemclicked_number)
                {
                    if (menuitem != -1)
                    {

                        getMenuItem_Label_fromitemindex(getMenuItem_Canvas_fromitemindex(Menu_InternalInfiniteCanvas, -1, "", "Menu" + menuitem), -1, "", "Menu" + menuitem + "Label").Foreground = BrushColors.mybrushfontgray;
                        getMenuItem_Canvas_fromitemindex(Menu_InternalInfiniteCanvas, -1, "", "Menu" + menuitem).Background = Brushes.White;
                    }
                }

            }
            else if (sender.GetType().ToString().IndexOf("Label") > -1)
            {
                Label tempsender = (Label)sender;
                menuitem = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
                if (menuitem != Menuitemclicked_number)
                {
                    tempsender.Foreground = BrushColors.mybrushfontgray;
                    //  getMenuItem_Image_fromitemindex(Menu_InternalInfiniteCanvas, menuitem);
                    getMenuItem_Canvas_fromitemindex(Menu_InternalInfiniteCanvas, -1, "", "Menu" + menuitem).Background = Brushes.White;
                }
            }
        }
        public void Menuitemclicked(object sender, MouseButtonEventArgs e)
        {
            restore_allmenuitems_color();


            int menuitem = -1;
            if (sender.GetType().ToString().IndexOf("Image") > -1)
            {
                Image tempsender = (Image)sender;
                menuitem = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
                if (menuitem != -1)
                {
                    Canvas tempcanvas;
                    tempcanvas = getMenuItem_Canvas_fromitemindex(Menu_InternalInfiniteCanvas, menuitem);
                    tempcanvas.Background = BrushColors.mybrushverylightgray_forcardbackground;
                    getMenuItem_Label_fromitemindex(tempcanvas, menuitem).Foreground = Brushes.Orange;
                }
            }
            else if (sender.GetType().ToString().IndexOf("Label") > -1)
            {
                Label tempsender = (Label)sender;
                menuitem = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
                //  getMenuItem_Image_fromitemindex(Menu_InternalInfiniteCanvas, menuitem);
                tempsender.Foreground = Brushes.Orange;
                getMenuItem_Canvas_fromitemindex(Menu_InternalInfiniteCanvas, menuitem).Background = BrushColors.mybrushverylightgray_forcardbackground;

            }
            Menuitemclicked_number = menuitem;
            CloseMenu(MenuCanvas, Publics.f);
        }

        public void restore_allmenuitems_color()
        {
            Menu1.Background = Brushes.White;
            Menu1Label.Foreground = BrushColors.mybrushfontgray;

            Menu2.Background = Brushes.White;
            Menu2Label.Foreground = BrushColors.mybrushfontgray;

            Menu3.Background = Brushes.White;
            Menu3Label.Foreground = BrushColors.mybrushfontgray;

            Menu4.Background = Brushes.White;
            Menu4Label.Foreground = BrushColors.mybrushfontgray;

            Menu5.Background = Brushes.White;
            Menu5Label.Foreground = BrushColors.mybrushfontgray;

            Menu6.Background = Brushes.White;
            Menu6Label.Foreground = BrushColors.mybrushfontgray;

            Menu7.Background = Brushes.White;
            Menu7Label.Foreground = BrushColors.mybrushfontgray;

            Menu8.Background = Brushes.White;
            Menu8Label.Foreground = BrushColors.mybrushfontgray;


            Menu9.Background = Brushes.White;
            Menu9Label.Foreground = BrushColors.mybrushfontgray;

        }

        #endregion

        #region Loss Compass    

        public void Generate_LossCompass_MainCards(string cardname, int maxnumberofbars = 6)
        {
            int j = -1;
            double temp_gapwidthbetweenlabels = 0;
            double temp_gapwidthbetweencolumns = 0;


            double canvasheight = 0;
            double canvaswidth = 0;
            double canvasTopPos = 0;
            double canvasLeftPos = 0;

            double columnwidth = 0;
            double columnheight = 0;
            double columnoffset = 0;
            double fieldlabelwidth = 0;
            double fieldlabelheight = 0;
            double fieldlabeloffset = 0;
            double datalabelwidth = 0;
            double datalabelheight = 0;
            double datalabeloffset = 0;
            double bubblewidth = Tiers_bubblewidth;
            double bubbleheight = Tiers_bubbleheight;
            double bubbleoffset = 0;


            int CanvasZindex = 0;
            int BarZindex = 0;
            int FieldLabelZindex = 0;
            int DataLabelZindex = 0;
            int bubbleZindex = 0;
            int RectangleZindex = 0;
            int HeaderZindex = 0;
            int SplashZindex = 0;
            string viewboxname = "";
            string chartname = "";
            string rectanglename = "";
            string barname = "";
            string fieldlabelname = "";
            string datalabelname = "";
            string bubblename = "";
            string searchstring = "Canvas";
            string splashsearchstring = "SplashTier";

            MouseButtonEventHandler TierScrollClick = null;


            switch (cardname)
            {
                case "A":
                    viewboxname = "DowntimeChartViewBoxA";
                    chartname = "DowntimeChartCanvasA";
                    rectanglename = "CanvasAMainRect";
                    canvasheight = TierACanvasheight;
                    canvaswidth = TierACanvaswidth;
                    canvasTopPos = 13;
                    canvasLeftPos = 0;
                    CanvasZindex = 1;
                    RectangleZindex = 2;
                    HeaderZindex = 3;
                    FieldLabelZindex = 75;
                    DataLabelZindex = 50;
                    BarZindex = 100;
                    bubbleZindex = 125;
                    SplashZindex = 190;

                    barname = "Bar_Rect_A_";
                    fieldlabelname = "Bar_Label_A_";
                    datalabelname = "DataLabel_A_";
                    bubblename = "SecondaryKPIbubble_A";
                    searchstring = "CanvasA";



                    columnwidth = Tiers_columnwidth;
                    columnheight = Tiers_columnheight;
                    columnoffset = Tiers_columnoffset;
                    fieldlabelheight = Tiers_fieldlabelheight;
                    fieldlabelwidth = Tiers_fieldlabelwidth;
                    fieldlabeloffset = Tiers_fieldlabeloffset;
                    datalabelheight = Tiers_datalabelheight;
                    datalabelwidth = Tiers_datalabelwidth;
                    datalabeloffset = Tiers_datalabeloffset;
                    bubbleoffset = Tiers_bubbleoffset;


                    TierScrollClick = TierAScrollClick;

                    break;

                case "B":
                    viewboxname = "DowntimeChartViewBoxB";
                    chartname = "DowntimeChartCanvasB";
                    rectanglename = "CanvasBMainRect";
                    canvasheight = TierBCanvasheight;
                    canvaswidth = TierBCanvaswidth;
                    canvasTopPos = 13;
                    canvasLeftPos = TierACanvaswidth + 5;
                    CanvasZindex = 201;
                    RectangleZindex = 202;
                    HeaderZindex = 203;
                    FieldLabelZindex = 275;
                    DataLabelZindex = 250;
                    BarZindex = 300;
                    bubbleZindex = 325;
                    SplashZindex = 390;

                    barname = "Bar_Rect_B_";
                    fieldlabelname = "Bar_Label_B_";
                    datalabelname = "DataLabel_B_";
                    bubblename = "SecondaryKPIbubble_B";
                    searchstring = "CanvasB";


                    columnwidth = Tiers_columnwidth;
                    columnheight = Tiers_columnheight;
                    columnoffset = Tiers_columnoffset;
                    fieldlabelheight = Tiers_fieldlabelheight;
                    fieldlabelwidth = Tiers_fieldlabelwidth;
                    fieldlabeloffset = Tiers_fieldlabeloffset;
                    datalabelheight = Tiers_datalabelheight;
                    datalabelwidth = Tiers_datalabelwidth;
                    datalabeloffset = Tiers_datalabeloffset;

                    bubbleoffset = Tiers_bubbleoffset;

                    TierScrollClick = TierBScrollClick;
                    break;
                case "C":
                    viewboxname = "DowntimeChartViewBoxC";
                    chartname = "DowntimeChartCanvasC";
                    rectanglename = "CanvasCMainRect";
                    canvasheight = TierCCanvasheight;
                    canvaswidth = TierCCanvaswidth;
                    canvasTopPos = 287;
                    canvasLeftPos = 0;
                    CanvasZindex = 1201;
                    RectangleZindex = 1202;
                    HeaderZindex = 1203;
                    FieldLabelZindex = 1275;
                    DataLabelZindex = 1250;
                    BarZindex = 1300;
                    bubbleZindex = 1325;
                    SplashZindex = 1390;

                    barname = "Bar_Rect_C_";
                    fieldlabelname = "Bar_Label_C_";
                    datalabelname = "DataLabel_C_";
                    bubblename = "SecondaryKPIbubble_C";
                    searchstring = "CanvasC";

                    columnwidth = 40;
                    columnheight = 150;
                    fieldlabelheight = Tiers_fieldlabelheight;
                    fieldlabelwidth = 110;
                    fieldlabeloffset = Tiers_fieldlabeloffset;
                    datalabelheight = Tiers_datalabelheight;
                    datalabelwidth = Tiers_datalabelwidth;

                    columnoffset = Tiers_fieldlabeloffset + (fieldlabelwidth - columnwidth) / 2;
                    datalabeloffset = Tiers_fieldlabeloffset + (fieldlabelwidth - datalabelwidth) / 2;

                    bubbleoffset = Tiers_fieldlabeloffset + (fieldlabelwidth - bubblewidth) / 2;

                    TierScrollClick = TierCScrollClick;
                    break;

            }


            Canvas tempTiercanvas;
            Canvas tempSplashcanvas;
            Viewbox tempTierviewbox;
            Ellipse tempbubble;

            //Generate viewbox
            GenerateViewBoxUI(LossCompass_MainChartsarea, viewboxname, canvasheight, canvaswidth, canvasLeftPos, canvasTopPos);
            System.Windows.Forms.Application.DoEvents();


            //get the viewbox object that was generated and then generate canvas inside it
            tempTierviewbox = getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, viewboxname);
            GenerateCanvasUI(tempTierviewbox, chartname, canvasheight, canvaswidth, 0, 0, CanvasZindex);
            System.Windows.Forms.Application.DoEvents();

            //get the canvas object that was generated and then generate rectangle and bars,labels, images etc inside it.
            tempTiercanvas = (Canvas)tempTierviewbox.Child; //getMenuItem_Canvas_fromitemindex(tempTierviewbox, -1, searchstring);
            GenerateRectangleUI(tempTiercanvas, rectanglename, canvasheight, canvaswidth, 0, 0, null, Brushes.LightGray, 1, null, Generalmousemove, Generalmouseleave, 0, RectangleZindex);

            // Generating field labels in canvas
            temp_gapwidthbetweenlabels = ((canvaswidth - (maxnumberofbars * fieldlabelwidth)) - (2 * fieldlabeloffset)) / (maxnumberofbars - 1);

            for (j = 1; j <= maxnumberofbars; j++)
            {
                GenerateLabelUI(tempTiercanvas, fieldlabelname + j, fieldlabelheight, fieldlabelwidth, fieldlabeloffset + ((j - 1) * temp_gapwidthbetweenlabels) + ((j - 1) * fieldlabelwidth), Tiers_fieldlabel_POS_top, BrushColors.mybrushdarkgray, Brushes.White, 9, LossClicked, LossLabelMove, LossLabelLeave, FieldLabelZindex + j);
            }


            // Generating columns (bars) in canvas
            temp_gapwidthbetweencolumns = ((canvaswidth - (maxnumberofbars * columnwidth)) - (2 * columnoffset)) / (maxnumberofbars - 1);
            for (j = 1; j <= maxnumberofbars; j++)
            {


                GenerateRectangleUI(tempTiercanvas, barname + j, columnheight, columnwidth, columnoffset + ((j - 1) * temp_gapwidthbetweencolumns) + (j * columnwidth), Tiers_column_POS_top, BrushColors.mybrushSelectedCriteria, Brushes.White, 0, BarClicked, BarMouseMove, BarMouseLeave, 180, BarZindex + j);
            }


            // Generating datalabels in canvas
            temp_gapwidthbetweenlabels = ((canvaswidth - (maxnumberofbars * datalabelwidth)) - (2 * datalabeloffset)) / (maxnumberofbars - 1);
            for (j = 1; j <= maxnumberofbars; j++)
            {
                GenerateLabelUI(tempTiercanvas, datalabelname + j, datalabelheight, datalabelwidth, datalabeloffset + ((j - 1) * temp_gapwidthbetweenlabels) + ((j - 1) * datalabelwidth), Tiers_fieldlabel_POS_top, null, BrushColors.mybrushfontgray, 10, BarClicked, LossLabelMove, LossLabelLeave, DataLabelZindex + j, "9.0%");
            }


            //Generating secondaryKPI bubbles in canvas
            temp_gapwidthbetweenlabels = ((canvaswidth - (maxnumberofbars * bubblewidth)) - (2 * bubbleoffset)) / (maxnumberofbars - 1);
            for (j = 1; j <= maxnumberofbars; j++)
            {
                GenerateEllipseUI(tempTiercanvas, bubblename + j, bubbleheight, bubblewidth, bubbleoffset + ((j - 1) * temp_gapwidthbetweenlabels) + (j * bubblewidth), Tiers_fieldlabel_POS_top, BrushColors.mybrushLIGHTBLUEGREEN, null, 0, null, Generalmousemove, Generalmouseleave, 180, bubbleZindex);
                tempbubble = getMenuItem_Ellipse_fromitemindex(tempTiercanvas, -1, "", bubblename + j);
                tempbubble.Visibility = Visibility.Hidden;
            }


            //Generating navigation scroll button images in canvas
            GenerateImageUI(tempTiercanvas, "NavigationLeft_Tier" + cardname, 15, 15, fieldlabeloffset / 2, canvasheight / 2, AppDomain.CurrentDomain.BaseDirectory + @"\Leftarrow.png", TierScrollClick, null, null);
            GenerateImageUI(tempTiercanvas, "NavigationRight_Tier" + cardname, 15, 15, canvaswidth - (3 * fieldlabeloffset / 2), canvasheight / 2, AppDomain.CurrentDomain.BaseDirectory + @"\Rightarrow.png", TierScrollClick, null, null);

            //Generating mapping button images in canvas
            GenerateImageUI(tempTiercanvas, "Tier" + cardname + "Mapping_Btn", 25, 25, tempTiercanvas.Width - 40, 10, AppDomain.CurrentDomain.BaseDirectory + @"\MappingIcon.png", LaunchMappingSplash, Generalmousemove, Generalmouseleave, "(Re) Map you data fields");
            GenerateLabelUI(tempTiercanvas, "Remap" + cardname + "Label", 15, 38, tempTiercanvas.Width - 45, 35, null, BrushColors.mybrushfontgray, 8, null, null, null, -1, "Re-map");

            // Generating header and sub-header labels in canvas
            GenerateLabelUI(tempTiercanvas, "Tier" + cardname + "Header", 30, 300, 10, 5, null, BrushColors.mybrushfontgray, 20, null, null, null, HeaderZindex, "Tier" + cardname + "Header", true);
            GenerateLabelUI(tempTiercanvas, "Tier" + cardname + "MiniHeader", 15, 300, 10, 35, null, Brushes.LightGray, 9, null, null, null, HeaderZindex, "", true);

            // Generating splash canvases inside canvas to show manual drill down choices //name - SplashTierA
            if (cardname != "A")
            {
                GenerateCanvasUI(tempTiercanvas, "SplashTier" + cardname, tempTiercanvas.Height, tempTiercanvas.Width, 0, 0, SplashZindex);
                System.Windows.Forms.Application.DoEvents();
                splashsearchstring = "SplashTier" + cardname;
                tempSplashcanvas = getMenuItem_Canvas_fromitemindex(tempTiercanvas, -1, splashsearchstring);
                GenerateRectangleUI(tempSplashcanvas, "SplashBorderRectangle" + cardname, tempSplashcanvas.Height, tempSplashcanvas.Width, 0, 0, Brushes.Black, null, 0, null, null, null, -0, -1, 0.5);
                GenerateComboBoxUI(tempSplashcanvas, "DrillDown_ManualCombobox" + cardname, 20, 150, canvaswidth / 2 - 75, canvasheight / 2 - 30, null, null);
                GenerateLabelUI(tempSplashcanvas, "DrillDown_ManualButtonTier" + cardname, 25, 150, canvaswidth / 2 - 75, canvasheight / 2, BrushColors.mybrushSelectedCriteria, Brushes.White, 11, DrillDown_RemapClicked, Generalmousemove, Generalmouseleave, SplashZindex + 2, "Select Field");
                GenerateLabelUI(tempSplashcanvas, "DrillDown_Manual_Header" + cardname, 30, 300, canvaswidth / 2 - 150, canvasheight / 2 - 100, null, Brushes.White, 12, null, null, null, SplashZindex + 3, "To drill down further, select a mapping field.");

                tempSplashcanvas.Visibility = Visibility.Hidden;
            }



        }

        #region LossCompassDynamics_Hide/Show/Animate/Load
        public void LossCompass_OnLoad()                                      // Main initialization for loss compass
        {

            LossLabelResetColors((getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child));                  //Loss Label are the gray colored labels who contains field names for  unit-ops or failremodes. Their colors are set to default gray
            LossLabelResetColors(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);                  //Loss Label are the gray colored labels who contains field names for  unit-ops or failremodes. Their colors are set to default gray
            LossLabelResetColors(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);                  //Loss Label are the gray colored labels who contains field names for  unit-ops or failremodes. Their colors are set to default gray


            //Initiation of charts, headers, primary KPIs, Scroll
            InitiateChartsvalues();                                                          // This is the function call to get the charts to show actual loss values
            InitiateHeadervalues();
            checkiffurtherscrollisneeded();
            RawDataSplashCanvas.Visibility = Visibility.Hidden;
            MappingSplashCanvas.Visibility = Visibility.Hidden;
            FilterSplashCanvas.Visibility = Visibility.Hidden;

            // ENd initiation

            // Criteria 1 KP1 Canvas Management
            CriteriaCanvasClicked(Dimension1Canvas, Publics.f);              // default primary KPI selection.
            //End Criteria 1 KPI Canvas Mgmt

            // HideTierSplashCanvas(AddSecondaryAxis_Image_C, Publics.f);        // Tier Splash Canvas needs to be hidden. they should be shown only when circular plus icon is clicked.
            ChangeState("A", "A");                                            // State Change determines what animation will be used and what positions will different cards show
            LossBottomLineExpander.IsExpanded = true;                         // Loss bottom lines shows top line break up of OEE - Planned, Unplanned and Rate loss/Scrap.. by default it should shown when only Tier A card is shown
            ManageUnplannedPlannedBar(1, 0, 0.25, 0.15, 0);                   // This manages the size of OEE break-ups - planned, un planned , rate loss etc... based on the intermediate sheets' values
            Set_Default_TierScrolls();                                        // based on scroll offset, it determines whether left / right navigation arrows should be shown or not.
            PopulateSecondaryAxisCombo();                                     // Secondary axis combobox is on the right legend section. It needs to be populated with all KPIs available for analysis.
            ManageLossCompassToolTips();                                      // Tooltips are not coded into XAML. they are programatically generated in C# based on selected language
            LockClicked(KPI1LockOFF, Publics.f);                               //Sets KPI1's 1st dimension locked by default
            FloatingToolTipCanvas.Visibility = Visibility.Hidden;             // Hide floating tooltip
            FloatingSimulatorCanvas.Visibility = Visibility.Hidden;            //Hide Simulator
            Generatetoplineresults_charts();                                   //Top line charts show unplanned, planned, rate loss, scrap etc as a component of 100%
            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "TierAHeader").Content = intermediate.LossCompass_TopLineResults_Names[0];


            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, LossCompassCanvas);

        }
        public void ToggleShowHide_LossCompass(object sender, MouseButtonEventArgs e)                // when Loss compass icon on the left is clicked
        {
            if (LossCompassCanvas.Visibility == Visibility.Visible)
            {
                //LossCompassCanvas.Visibility = Visibility.Hidden;

            }
            else
            {
                HideAllDashboards();
                LossCompassCanvas.Visibility = Visibility.Visible;
                LossCompass_OnLoad();
            }
        }




        public void SizetoActualDowntimeCanvasCharts()
        {

            getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Height = getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").ActualHeight;
            getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Width = getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").ActualWidth;
            getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Height = getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").ActualHeight;
            getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Width = getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").ActualWidth;
            // DowntimeChartViewBoxC.Height = DowntimeChartViewBoxC.ActualHeight;
            //DowntimeChartViewBoxC.Width = DowntimeChartViewBoxC.ActualWidth;
            getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Stretch = Stretch.Uniform;
            getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Stretch = Stretch.Uniform;
            getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Margin = new Thickness(OriginalThickness_DowntimeChartsViewBoxA.Left, OriginalThickness_DowntimeChartsViewBoxA.Top, 0, 0);
            getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Margin = new Thickness(OriginalThickness_DowntimeChartsViewBoxB.Left, OriginalThickness_DowntimeChartsViewBoxB.Top, 0, 0);
            getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Margin = new Thickness(OriginalThickness_DowntimeChartsViewBoxC.Left, OriginalThickness_DowntimeChartsViewBoxC.Top, 0, 0);


        }

        public void ChangeState(String Statefrom, String Stateto)
        {
            switch (Stateto)
            {
                case (string)("A"):        // Only Tier A is visible in its max dimensions
                    SizetoActualDowntimeCanvasCharts();
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Visibility = Visibility.Visible;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Visibility = Visibility.Hidden;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Visibility = Visibility.Hidden;
                    // CanvasAChart_ExpandIcon.Visibility = Visibility.Hidden;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Stretch = Stretch.UniformToFill;
                    AnimateZoomUIElement(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").MaxHeight, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").MaxHeight, 0.2, Viewbox.HeightProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA"));
                    AnimateZoomUIElement(0, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").MaxWidth, 0.2, Viewbox.WidthProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA"));
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Stretch = Stretch.UniformToFill;
                    // ToplineresultsCanvas.Margin = new Thickness(OriginalThickness_TopLineCharts.Left, OriginalThickness_TopLineCharts.Top, 0, 0);
                    Settings.Default.LossCompassState = (int)Globals.LossCompassStates.A;
                    break;
                case (string)("B"):      // Tier A and B are visible. Tier A in its small dimensions where as Tier B is in max dimension
                    SizetoActualDowntimeCanvasCharts();
                    LossBottomLineExpander.IsExpanded = false;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Visibility = Visibility.Visible;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Visibility = Visibility.Visible;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Visibility = Visibility.Hidden;
                    // CanvasAChart_ExpandIcon.Visibility = Visibility.Visible;
                    //CanvasBChart_ExpandIcon.Visibility = Visibility.Hidden;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Stretch = Stretch.UniformToFill;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Stretch = Stretch.UniformToFill;
                    //AnimateZoomUIElement(DowntimeChartViewBoxA.Height, 270, 0.2, Viewbox.HeightProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA"));
                    //AnimateZoomUIElement(DowntimeChartViewBoxA.Width, 510, 0.2, Viewbox.WidthProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA"));
                    AnimateZoomUIElement(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").MaxHeight, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").MaxHeight, 0.2, Viewbox.HeightProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB"));
                    AnimateZoomUIElement(0, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").MaxWidth, 0.2, Viewbox.WidthProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB"));
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Stretch = Stretch.Uniform;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Stretch = Stretch.UniformToFill;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Margin = new Thickness(OriginalThickness_DowntimeChartsViewBoxC.Left, OriginalThickness_DowntimeChartsViewBoxC.Top, 0, 0);
                    //ToplineresultsCanvas.Margin = new Thickness(OriginalThickness_TopLineCharts.Left, OriginalThickness_TopLineCharts.Top, 0, 0);

                    Settings.Default.LossCompassState = (int)Globals.LossCompassStates.B;
                    break;
                case (string)("C"):  // Tier A,B and C are visible. All in their original dimensions
                    SizetoActualDowntimeCanvasCharts();
                    LossBottomLineExpander.IsExpanded = false;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Visibility = Visibility.Visible;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Visibility = Visibility.Visible;

                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Margin = new Thickness(OriginalThickness_DowntimeChartsViewBoxB.Left, OriginalThickness_DowntimeChartsViewBoxB.Top, 0, 0);


                    var translate = (Point)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").RenderTransformOrigin;

                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").RenderTransformOrigin = new Point(0, -0.5);

                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Visibility = Visibility.Visible;
                    //AnimateZoomUIElement(299, DowntimeChartViewBoxC.MaxHeight, 0.4, Viewbox.HeightProperty, DowntimeChartViewBoxC);
                    AnimateZoomUIElement(0, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").MaxWidth, 0.3, Viewbox.WidthProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC"));

                    //ToplineresultsCanvas.Margin = new Thickness(OriginalThickness_TopLineCharts.Left + 13, OriginalThickness_TopLineCharts.Top, 0, 0);
                    Settings.Default.LossCompassState = (int)Globals.LossCompassStates.C;
                    break;
                case (string)("D"):
                    SizetoActualDowntimeCanvasCharts();
                    LossBottomLineExpander.IsExpanded = false;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Visibility = Visibility.Visible;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Visibility = Visibility.Visible;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Visibility = Visibility.Hidden;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Stretch = Stretch.UniformToFill;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Stretch = Stretch.UniformToFill;

                    AnimateZoomUIElement(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Height, 270, 0.2, Viewbox.HeightProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA"));
                    AnimateZoomUIElement(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Width, 510, 0.2, Viewbox.WidthProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA"));

                    AnimateZoomUIElement(0, 270, 0.2, Viewbox.HeightProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB"));
                    AnimateZoomUIElement(0, 510, 0.2, Viewbox.WidthProperty, getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB"));

                    Settings.Default.LossCompassState = (int)Globals.LossCompassStates.D;
                    break;
                case (string)("E"):
                    LossBottomLineExpander.IsExpanded = false;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Visibility = Visibility.Visible;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Visibility = Visibility.Visible;
                    getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Visibility = Visibility.Visible;

                    Settings.Default.LossCompassState = (int)Globals.LossCompassStates.E;


                    break;
            }
        }
        #endregion

        #region "PlannedUnplannedBar"
        public void ManageUnplannedPlannedBar(int selectedmode, double prvalue, double unplannedvalue, double plannedvalue, double ratelossvalue)
        {
            LossBottomLineExpander.IsExpanded = true;

            //Set Widths
            UnplannedBar.Visibility = Visibility.Visible;
            UnplannedBar.Width = unplannedvalue * prBar.Width;
            PlannedBar.Width = plannedvalue * prBar.Width;
            RatelossBar.Width = ratelossvalue * prBar.Width;

            //Locate the bars
            Thickness Unplbar_location;
            Unplbar_location = UnplannedBar.Margin;
            PlannedBar.Margin = new Thickness(UnplannedBar.Width, Unplbar_location.Top, 0, 0);

            Thickness Plbar_location;
            Plbar_location = PlannedBar.Margin;
            RatelossBar.Margin = new Thickness(PlannedBar.Width, Plbar_location.Top, 0, 0);

            UnplannedBar.Content = "UP - " + Math.Round(unplannedvalue * 100, 2) + "%";
            PlannedBar.Content = "P - " + Math.Round(plannedvalue * 100, 2) + "%";
            RatelossBar.Content = "Rate - " + Math.Round(ratelossvalue * 100, 2) + "%";

            //Highlight selected bar and reset other bar colors
            switch (selectedmode)
            {
                case 1:
                    UnplannedBar.Background = BrushColors.mybrushSelectedCriteria;
                    PlannedBar.Background = BrushColors.mybrushLossLabelDefaultColors;
                    RatelossBar.Background = BrushColors.mybrushlightgray;

                    ChangeState("A", "A");
                    break;
                case 2:
                    UnplannedBar.Background = BrushColors.mybrushLossLabelDefaultColors;
                    PlannedBar.Background = BrushColors.mybrushSelectedCriteria;
                    RatelossBar.Background = BrushColors.mybrushlightgray;
                    ChangeState("A", "D");
                    break;
                case 3:
                    UnplannedBar.Background = BrushColors.mybrushLossLabelDefaultColors;
                    PlannedBar.Background = BrushColors.mybrushlightgray;
                    RatelossBar.Background = BrushColors.mybrushSelectedCriteria;
                    break;
            }



        }
        public void PlannedUnplannedBarClicked(object sender, MouseButtonEventArgs e)

        {
            Label tmpsender;
            tmpsender = (Label)sender;
            if (tmpsender.Name.IndexOf("Unplanned") > -1)
            {
                ManageUnplannedPlannedBar(1, 0, 0.25, 0.15, 0);
            }
            else if (tmpsender.Name.IndexOf("Rate") > -1)
            {
                ManageUnplannedPlannedBar(3, 0, 0.25, 0.15, 0);
            }
            else
            {
                ManageUnplannedPlannedBar(2, 0, 0.25, 0.15, 0);
            }
        }

        public void LossBottomLineExpanded(object sender, RoutedEventArgs e)
        {
            LossbottomlineExpandergrid.Visibility = Visibility.Visible;
            prBar.Visibility = Visibility.Visible;
            RatelossBar.Visibility = Visibility.Visible;
            PlannedBar.Visibility = Visibility.Visible;
            UnplannedBar.Visibility = Visibility.Visible;
        }
        public void LossBottomLineCollapsed(object sender, RoutedEventArgs e)
        {

            LossbottomlineExpandergrid.Visibility = Visibility.Hidden;
            prBar.Visibility = Visibility.Hidden;
            RatelossBar.Visibility = Visibility.Hidden;
            PlannedBar.Visibility = Visibility.Hidden;
            UnplannedBar.Visibility = Visibility.Hidden;
        }


        #endregion

        #region "TopLineResults"

        public void Generatetoplineresults_charts()
        {
            clearallrectanglesandlabels_in_toplineresultscanvas();
            var ListofTopLineResults_values = new List<double>();
            var ListofTopLineResults_names = new List<string>();
            var ListofChartColors = new List<SolidColorBrush>();
            var ListofLabelColors = new List<SolidColorBrush>();
            ToplineresultsCanvas.Visibility = Visibility.Visible;

            ListofTopLineResults_names = intermediate.LossCompass_TopLineResults_Names;
            ListofTopLineResults_values = intermediate.LossCompass_TopLineResults_Values;



            // colors are hard coded
            ListofChartColors.Add(new SolidColorBrush(Color.FromRgb(6, 197, 180)));
            ListofChartColors.Add(new SolidColorBrush(Color.FromRgb(50, 209, 195)));
            ListofChartColors.Add(new SolidColorBrush(Color.FromRgb(78, 228, 225)));
            ListofChartColors.Add(new SolidColorBrush(Color.FromRgb(100, 240, 235)));
            ListofChartColors.Add(new SolidColorBrush(Color.FromRgb(130, 255, 245)));
            ListofChartColors.Add(new SolidColorBrush(Color.FromRgb(210, 255, 255)));

            ListofLabelColors.Add(new SolidColorBrush(Color.FromRgb(255, 255, 255)));
            ListofLabelColors.Add(new SolidColorBrush(Color.FromRgb(255, 255, 255)));
            ListofLabelColors.Add(new SolidColorBrush(Color.FromRgb(90, 90, 90)));
            ListofLabelColors.Add(new SolidColorBrush(Color.FromRgb(80, 80, 80)));
            ListofLabelColors.Add(new SolidColorBrush(Color.FromRgb(60, 60, 60)));
            ListofLabelColors.Add(new SolidColorBrush(Color.FromRgb(30, 30, 30)));

            DependencyObject dep = ToplineresultsCanvas;
            Canvas DEP1 = ToplineresultsCanvas;
            int j;
            Rectangle rect;
            Label lbl;

            // first remove all rectangles and labels from toplineresultscanvas

            while (VisualTreeHelper.GetChildrenCount(DEP1) != 0)
            {
                if (VisualTreeHelper.GetChild(DEP1, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(DEP1, 0);

                    DEP1.Children.Remove(rect);
                }
                if (VisualTreeHelper.GetChild(DEP1, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(DEP1, 0);
                    DEP1.Children.Remove(lbl);
                }

            }


            for (j = 0; j < 5; j++)
            {
                //ListofTopLineResults_values.Add((rnd.Next(1, 99)) / 100);
                ListofTopLineResults_names.Add("LossName" + j);
                // ListofChartColors.Add(new SolidColorBrush(Color.FromRgb(Convert.ToByte(rnd.Next(255)), Convert.ToByte(rnd.Next(255)), Convert.ToByte(rnd.Next(255)))));

            }
            create_rectangles_fortoplineresults(ToplineresultsCanvas, ListofTopLineResults_values, ListofTopLineResults_names, ListofChartColors, ListofLabelColors, "TopLineLoss", TopLineCharts_barclicked, Generalmousemove, Generalmouseleave);
            ToplineresultsCanvas.Visibility = Visibility.Visible;



            //Setting default topline result .. can be improved


            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, j);

                    if (rect.Name.IndexOf("TopLine_0") > -1)
                    {
                        rect.StrokeThickness = 2;
                        rect.Stroke = Brushes.DarkOliveGreen;
                    }
                }

            }


        }

        public void create_rectangles_fortoplineresults(Canvas dep, List<double> lossvalue, List<string> lossname, List<SolidColorBrush> rectcolor, List<SolidColorBrush> labelcolor, string objectname, MouseButtonEventHandler mousedownact, MouseEventHandler mousemoveact, MouseEventHandler mouseleaveact)
        {
            int i = 0;
            double temptotalwidth = 0;
            double temptotallossvalue = 0;
            int templabelZindex = 50;
            int temprectZindex = 0;
            string lossnametemp = "";
            for (i = 0; i <= lossvalue.Count - 1; i++)
            {
                lossnametemp = lossname[i] + " : " + Math.Round(lossvalue[i] * 100, 1) + "%";

                Label l;
                l = new Label();
                dep.Children.Add(l);
                l.Height = 19;
                l.Width = lossvalue[i] * Globals.LossCompass_chart_toplineresults_maxwidth;
                l.Name = "TopLineLabel_" + i;
                l.ToolTip = lossnametemp;
                Canvas.SetLeft(l, temptotalwidth + 5);
                Canvas.SetTop(l, 8);
                l.Content = lossnametemp;
                l.FontSize = 13;
                l.Padding = new Thickness(0, 0, 0, 0);
                l.Foreground = labelcolor[i];
                Canvas.SetZIndex(l, templabelZindex + i);


                // Make OEE rectangle unclickable
                if (!l.Content.ToString().Contains("Jobs"))
                {
                    //l.MouseDown += null;
                    //l.Cursor = Cursors.Arrow;
                    l.MouseDown += mousedownact;
                    l.Cursor = Cursors.Hand;
                }

                if (l.Width < 50)
                {
                    l.Content = "";
                }


                Rectangle r;
                r = new Rectangle();
                dep.Children.Add(r);
                r.RenderTransform = new RotateTransform(180, 0, 0);
                r.Height = 20;
                r.Width = lossvalue[i] * Globals.LossCompass_chart_toplineresults_maxwidth;
                temptotalwidth = temptotalwidth + r.Width;
                temptotallossvalue = temptotallossvalue + lossvalue[i];
                //r.Stroke = Brushes.Gray;
                //r.StrokeThickness = 0.1;
                r.Fill = rectcolor[i];
                r.Name = "TopLine_" + i;
                r.ToolTip = lossnametemp;
                Canvas.SetLeft(r, temptotalwidth);
                Canvas.SetTop(r, dep.Height);
                r.MouseMove += mousemoveact;
                r.MouseLeave += mouseleaveact;
                Canvas.SetZIndex(r, temprectZindex + i);
                if (!l.Content.ToString().Contains("Jobs"))
                {
                    r.MouseDown += mousedownact;
                    r.Cursor = Cursors.Hand;
                }




            }

            if (temptotallossvalue < 1)
            {

            }

        }

        public void TopLineCharts_barclicked(object sender, MouseButtonEventArgs e)
        {

            if (Settings.Default.LossCompassState != (int)Globals.LossCompassStates.A)
            { ChangeState("A", "A"); }


            int j;
            DependencyObject dep = ToplineresultsCanvas;
            Rectangle rect;

            // Clear selection of all top line rectangles
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, j);

                    if (rect.Name.IndexOf("TopLine_") > -1)
                    {
                        rect.StrokeThickness = 0;
                    }
                }

            }


            if (sender.GetType().ToString().IndexOf("Rectangle") > -1)
            {
                Rectangle tempsender = (Rectangle)sender;
                tempsender.StrokeThickness = 2;
                tempsender.Stroke = Brushes.DarkOliveGreen;
                string indexoflabel = GlobalFcns.onlyDigits(tempsender.Name);
                intermediate.LossCompass_TopLineRefresh(Convert.ToInt32(indexoflabel) - 1);

                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "TierAHeader").Content = intermediate.LossCompass_TopLineResults_Names[Convert.ToInt32(indexoflabel) - 1];
            }
            else
            {
                Label tempsender = (Label)sender;
                string indexoflabel = GlobalFcns.onlyDigits(tempsender.Name);
                intermediate.LossCompass_TopLineRefresh(Convert.ToInt32(indexoflabel));
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "TierAHeader").Content = intermediate.LossCompass_TopLineResults_Names[Convert.ToInt32(indexoflabel)];
                for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
                {
                    if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Rectangle") > -1)
                    {
                        rect = (Rectangle)VisualTreeHelper.GetChild(dep, j);

                        if (rect.Name.IndexOf(indexoflabel) > -1)
                        {
                            rect.StrokeThickness = 2;
                            rect.Stroke = Brushes.DarkOliveGreen;

                        }
                    }

                }

            }


            InitiateChartsvalues();
            AnimateZoomUIElement(0.8, 1.0, 0.1, OpacityProperty, LossCompass_MainChartsarea);
        }

        public void clearallrectanglesandlabels_in_toplineresultscanvas()
        {
            Canvas dep = ToplineresultsCanvas;
            Rectangle rect;
            Label lbl;
            int j;
            try
            {
                while (VisualTreeHelper.GetChildrenCount(dep) != 0)
                {
                    if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                    {
                        rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);
                        dep.Children.Remove(rect);
                    }
                    if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                    {
                        lbl = (Label)VisualTreeHelper.GetChild(dep, 0);
                        dep.Children.Remove(lbl);
                    }

                }
            }
            catch (WebException ex)
            { }
        }
        #endregion

        #region "BarMove/Leave/Clicked"
        public void BarMouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
            Rectangle tempsender = (Rectangle)sender;
            tempsender.Opacity = 0.8;
            //tempsender.Fill = BrushColors.mybrushhighlightedbargreencolor;


        }

        public void BarMouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            var tempsender = (Rectangle)sender;
            tempsender.Opacity = 1.0;
        }

        public List<double> LossCompass_SparkChart_DataValues = new List<double>();

        public void BarClicked(object sender, MouseEventArgs e)
        {
            string failureModeName = "";
            string[] splits1;
            CardTier tempcardname = CardTier.NA;

            ////DISPLAY  Floating tool tip or Simulator
            string[] splits2;
            splits2 = Mouse.GetPosition(this).ToString().Split(',');

            if (!IScrystallballON)
            {
                if (Convert.ToDouble(splits2[1]) > (this.Height - 1.3 * FloatingToolTipCanvas.Height))
                {
                    splits2[1] = (this.Height - (FloatingToolTipCanvas.Height * 1.3 * (this.Height / this.ActualHeight))).ToString();
                }


                if (sender.GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    var tempsender = (Rectangle)sender;
                    splits1 = tempsender.Name.ToString().Split('_');
                    // ---->>>>> use this function-->  GetLossName_forCardandIndex(splits1[2], Convert.ToInt32(splits1[3]));
                    failureModeName = GetLossName_forCardandIndex(splits1[2], Convert.ToInt32(splits1[3]));
                    splits1 = tempsender.Name.ToString().Split('_');
                    ActiveToolTip_Cardname = splits1[2];
                    ActiveToolTip_FailureModename = failureModeName;
                    if (splits1[2] == "A") { tempcardname = CardTier.A; } else if (splits1[2] == "B") { tempcardname = CardTier.B; } else if (splits1[2] == "C") { tempcardname = CardTier.C; }
                    ActiveToolTip_Card = tempcardname;
                }
                else if (sender.GetType().ToString().IndexOf("Label") > -1)
                {
                    var tempsender = (Label)sender;
                    splits1 = tempsender.Name.ToString().Split('_');
                    // ---->>>>> use this function-->  GetLossName_forCardandIndex(splits1[2], Convert.ToInt32(splits1[3]));
                    failureModeName = GetLossName_forCardandIndex(splits1[1], Convert.ToInt32(splits1[2]));
                    splits1 = tempsender.Name.ToString().Split('_');
                    ActiveToolTip_Cardname = splits1[1];
                    ActiveToolTip_FailureModename = failureModeName;
                    if (splits1[1] == "A") { tempcardname = CardTier.A; } else if (splits1[1] == "B") { tempcardname = CardTier.B; } else if (splits1[1] == "C") { tempcardname = CardTier.C; }
                    ActiveToolTip_Card = tempcardname;
                }


                //////////////////
                var tmpData = new List<double>();

                // intermediate.LossCompass_SparkData_Update(failureModeName, tempcardname);  // Sam to fix 
                LossCompass_SparkChart_DataValues.Clear();
                for (int i = 0; i < intermediate.LossCompass_SparkData_Values.Count; i++)
                {
                    tmpData.Add(intermediate.LossCompass_SparkData_Values[i]);
                }

                LossCompass_SparkChart.ItemsSource = tmpData;
                double xx = LossCompass_SparkChart.ActualWidth;

                FloatingToolTipCanvas.Margin = new Thickness(((Convert.ToDouble(splits2[0]) * (this.Height / this.ActualHeight))), ((Convert.ToDouble(splits2[1]) * (this.Width / this.ActualWidth) - 65)), 0, 0);
                FloatingToolTipCanvas.Visibility = Visibility.Visible;
                AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, FloatingToolTipCanvas);
                System.Windows.Forms.Application.DoEvents();


            }
            else
            {
                if (Convert.ToDouble(splits2[1]) > (this.Height - 1.3 * FloatingSimulatorCanvas.Height))
                {
                    splits2[1] = (this.Height - (FloatingSimulatorCanvas.Height * 1.3 * (this.Height / this.ActualHeight))).ToString();
                }
                FloatingSimulatorCanvas.Margin = new Thickness(((Convert.ToDouble(splits2[0]) * (this.Height / this.ActualHeight))), ((Convert.ToDouble(splits2[1]) * (this.Width / this.ActualWidth) - 65)), 0, 0);
                FloatingSimulatorCanvas.Visibility = Visibility.Visible;
                AnimateZoomUIElement(0.5, 1.0, 0.2, OpacityProperty, FloatingSimulatorCanvas);
                System.Windows.Forms.Application.DoEvents();
            }



            // SETUP Rawdata window and Loss Trends data model
            if (sender.GetType().ToString().IndexOf("Label") > -1)
            {
                var tempsender = (Label)sender;
                splits1 = tempsender.Name.ToString().Split('_');

                Tooltip_failuremodenamelabel.Content = GetLossName_forCardandIndex(splits1[1], Convert.ToInt32(splits1[2]));
                SimulationFailureModeName.Content = GetLossName_forCardandIndex(splits1[1], Convert.ToInt32(splits1[2]));
                if (splits1[1] == "A") { tempcardname = CardTier.A; } else if (splits1[1] == "B") { tempcardname = CardTier.B; } else if (splits1[1] == "C") { tempcardname = CardTier.C; }

                TempCardTier_ForSimulator = tempcardname;
                if (IScrystallballON) { LoadCurrentValuesto_FloatingSimulator(TempCardTier_ForSimulator, SimulationFailureModeName.Content.ToString()); }
            }
            else if (sender.GetType().ToString().IndexOf("Rectangle") > -1)
            {
                var tempsender = (Rectangle)sender;
                splits1 = tempsender.Name.ToString().Split('_');

                Tooltip_failuremodenamelabel.Content = GetLossName_forCardandIndex(splits1[2], Convert.ToInt32(splits1[3]));
                SimulationFailureModeName.Content = GetLossName_forCardandIndex(splits1[2], Convert.ToInt32(splits1[3]));
                if (splits1[2] == "A") { tempcardname = CardTier.A; } else if (splits1[2] == "B") { tempcardname = CardTier.B; } else if (splits1[2] == "C") { tempcardname = CardTier.C; }

                TempCardTier_ForSimulator = tempcardname;
                if (IScrystallballON)
                { LoadCurrentValuesto_FloatingSimulator(TempCardTier_ForSimulator, SimulationFailureModeName.Content.ToString()); }
            }



        }

        public string GetLossName_forCardandIndex(string cardname, int index)
        {
            switch (cardname)
            {
                case "A":
                    return intermediate.TierA_Names[index - 1];
                case "B":
                    return intermediate.TierB_Names[index - 1];
                case "C":
                    return intermediate.TierC_Names[index - 1];

            }
            return "";

        }

        public void Generalmousemove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
            if (sender.GetType().ToString().IndexOf("Label") > -1)
            {
                Label tempsender = (Label)sender;
                tempsender.Opacity = 0.8;
            }
            else if (sender.GetType().ToString().IndexOf("Image") > -1)
            {
                Image tempsender = (Image)sender;
                tempsender.Opacity = 0.8;
            }
            else if (sender.GetType().ToString().IndexOf("Rectangle") > -1)
            {
                Rectangle tempsender = (Rectangle)sender;
                tempsender.Opacity = 0.8;
            }
            else if (sender.GetType().ToString().IndexOf("Canvas") > -1)
            {
                Canvas tempsender = (Canvas)sender;
                tempsender.Opacity = 0.8;
            }
        }
        public void Generalmouseleave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            if (sender.GetType().ToString().IndexOf("Label") > -1)
            {
                Label tempsender = (Label)sender;
                tempsender.Opacity = 1.0;
            }
            else if (sender.GetType().ToString().IndexOf("Image") > -1)
            {
                Image tempsender = (Image)sender;
                tempsender.Opacity = 1.0;
            }
            else if (sender.GetType().ToString().IndexOf("Rectangle") > -1)
            {
                Rectangle tempsender = (Rectangle)sender;
                tempsender.Opacity = 1.0;
            }
            else if (sender.GetType().ToString().IndexOf("Canvas") > -1)
            {
                Canvas tempsender = (Canvas)sender;
                tempsender.Opacity = 1.0;
            }
        }
        #endregion

        #region "LossLabelMove/Leave/Clicked"
        public void LossLabelMove(object sender, MouseEventArgs e)
        {
            Control tempsender = (Control)sender;
            tempsender.Opacity = 0.8;


        }
        public void LossLabelLeave(object sender, MouseEventArgs e)
        {
            Control tempsender = (Control)sender;
            tempsender.Opacity = 1.0;
        }
        public void LossClicked(object sender, MouseButtonEventArgs e)  //When a loss label in Tier A , B or C is clicked, failure mode's corresponding drill down info is showed. Tier A -> Tier B -> Tier C
        {
            //Preparation
            Label tmpsender;
            tmpsender = (Label)sender;

            //Determining the origin of clicked loss label (Tier name, Loss name).  Accordingly intermediate sheet will be refreshed.
            if (tmpsender.Name.IndexOf("Label_A") > -1)
            {
                LossLabelResetColors(this.getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child);
                LossLabelResetColors(this.getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);
                LossLabelResetColors(this.getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);

                switch (Settings.Default.LossCompassState)
                {
                    case (int)(Globals.LossCompassStates.A):
                        ChangeState("A", "B");
                        break;
                    case (int)(Globals.LossCompassStates.B):
                        break;
                    case (int)(Globals.LossCompassStates.C):
                        break;
                    case (int)(Globals.LossCompassStates.D):
                        break;
                    case (int)(Globals.LossCompassStates.E):
                        break;
                }

                intermediate.LossCompass_drillDown(tmpsender.Content.ToString(), CardTier.A);  // this is where intermediate sheet gets refreshed and is told which loss label was clicked.

                //Setting Tier B MAIN header
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "TierBHeader").Content = tmpsender.Content.ToString();

                TierBscrolloffset = 0;
                intermediate.LossCompass_drillDown(intermediate.TierB_Names[0].ToString(), CardTier.B);

                //Setting Tier C MAIN header
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCHeader").Content = intermediate.TierB_Names[0].ToString();

                //Setting Tier C mini header - 
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCMiniHeader").Content = getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "TierAHeader").Content + " > " + getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "TierBHeader").Content + " > " + intermediate.TierB_Names[0].ToString();

                //Setting Tier B mini header - 
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "TierBMiniHeader").Content = getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "TierAHeader").Content + " > " + getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "TierBHeader").Content;

                TierCscrolloffset = 0;

                //Check if tier C is visible then, set tier B's first label color
                if (getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Visibility == Visibility.Visible)
                {
                    Label templabel = getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "_Label_B_1");

                    templabel.Background = BrushColors.mybrushLIGHTBLUEGREEN;
                    templabel.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
                    templabel.BorderBrush = BrushColors.mybrushLIGHTGRAY;
                }

                clickedlabelIndex_scroll_TierA = TierAscrolloffset;
                clickedlabelIndex_scroll_TierB = TierBscrolloffset;
                clickedlabelIndex_scroll_TierC = TierCscrolloffset;

                clickedlabelindex_labelpos_TierA = Convert.ToInt32(GlobalFcns.onlyDigits(tmpsender.Name.ToString()));
                clickedlabelindex_labelpos_TierB = -1;
                clickedlabelindex_labelpos_TierC = -1;


                // this is the trigger for manual drilldown - shown on a splash screen inside the canvas of the chart
                //get splash canvas
                Canvas tempcanvas;
                tempcanvas = getMenuItem_Canvas_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "TierB");
                tempcanvas.Visibility = Visibility.Visible;


                if (intermediate.LossCompass_drillDown(tmpsender.Content.ToString(), CardTier.A) == false)
                {
                    // show the splash canvas

                    getMenuItem_ComboBox_fromitemindex(tempcanvas, -1, "Combo").ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.A, false);
                }
                else
                {
                    //hide the splash canvas

                    tempcanvas.Visibility = Visibility.Hidden;
                }

            }
            if (tmpsender.Name.IndexOf("Label_B") > -1)
            {


                LossLabelResetColors(this.getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);
                LossLabelResetColors(this.getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);

                switch (Settings.Default.LossCompassState)
                {
                    case (int)(Globals.LossCompassStates.A):

                        break;
                    case (int)(Globals.LossCompassStates.B):
                        ChangeState("B", "C");
                        break;
                    case (int)(Globals.LossCompassStates.C):
                        break;
                    case (int)(Globals.LossCompassStates.D):
                        ChangeState("D", "E");
                        break;
                    case (int)(Globals.LossCompassStates.E):
                        break;
                }
                intermediate.LossCompass_drillDown(tmpsender.Content.ToString(), CardTier.B);   // this is where intermediate sheet gets refreshed and is told which loss label was clicked.
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCHeader").Content = tmpsender.Content.ToString();
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCMiniHeader").Content = getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "TierAHeader").Content + " > " + getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "TierBHeader").Content + " > " + getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCHeader").Content;


                TierCscrolloffset = 0;



                clickedlabelIndex_scroll_TierB = TierBscrolloffset;
                clickedlabelIndex_scroll_TierC = TierCscrolloffset;

                clickedlabelindex_labelpos_TierB = Convert.ToInt32(GlobalFcns.onlyDigits(tmpsender.Name.ToString()));
                clickedlabelindex_labelpos_TierC = -1;


                // this is the trigger for manual drilldown - shown on a splash screen inside the canvas of the chart
                //get splash canvas
                Canvas tempcanvas;
                tempcanvas = getMenuItem_Canvas_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierC");
                tempcanvas.Visibility = Visibility.Visible;


                if (intermediate.LossCompass_drillDown(tmpsender.Content.ToString(), CardTier.B) == false)
                {
                    // show the splash canvas

                    getMenuItem_ComboBox_fromitemindex(tempcanvas, -1, "Combo").ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.B, false);
                }
                else
                {
                    tempcanvas.Visibility = Visibility.Hidden;
                }




            }
            if (tmpsender.Name.IndexOf("Label_C") > -1)
            {
                if (tmpsender.Content.ToString() == "")
                {
                    return;
                }
                LossLabelResetColors(this.getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);

                clickedlabelIndex_scroll_TierC = TierCscrolloffset;
                clickedlabelindex_labelpos_TierC = Convert.ToInt32(GlobalFcns.onlyDigits(tmpsender.Name.ToString()));
                intermediate.LossCompass_TierA_Level = intermediate.LossCompass_TierA_Level + 1;
                intermediate.LossCompass_drillDown(tmpsender.Content.ToString(), CardTier.C);   // this is where intermediate sheet gets refreshed and is told which loss label was clicked.
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCHeader").Content = tmpsender.Content.ToString();

                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "TierAHeader").Content = intermediate.TierA_Header.ToString();
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "TierBHeader").Content = intermediate.TierB_Header.ToString();
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCHeader").Content = intermediate.TierC_Header.ToString();




                Canvas Tempcanvasa;
                Tempcanvasa = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child;
                TierA_StackingLevels(Tempcanvasa, intermediate.LossCompass_TierA_Level);

            }

            tmpsender.Background = BrushColors.mybrushLIGHTBLUEGREEN;
            tmpsender.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
            tmpsender.BorderBrush = BrushColors.mybrushLIGHTGRAY;

            //We have to initialize our charts so that the charts get the latest values for all tiers from intermediate sheet.
            InitiateChartsvalues();
        }

        public void TierA_StackingLevels(Canvas dep, int LevelNumber)
        {
            int k;
            CreateBitmapFromVisual(dep, "Mini" + LevelNumber);

            //Delete all existing mini's
            for (k = 2; k <= LevelNumber; k++)
            {
                if (getMenuItem_Rectangle_fromitemindex(LossCompass_MainChartsarea, -1, "Mini") != null)
                {
                    LossCompass_MainChartsarea.Children.Remove(getMenuItem_Rectangle_fromitemindex(LossCompass_MainChartsarea, -1, "Mini"));
                }
            }

            //Generate new mini's
            for (k = 1; k < LevelNumber; k++)
            {
                GenerateRectangleUI(LossCompass_MainChartsarea, "Mini" + k, 10, 10, 65 + (k - 1) * 30, 0, Brushes.DarkGray, null, 0, MiniMouseDown, MiniMouseMove, MiniMouseLeave);
            }

            PreviousCardsLabel.Visibility = Visibility.Visible;

        }

        public void MiniMouseMove(object sender, MouseEventArgs e)
        {

            Cursor = Cursors.Hand;
            Rectangle tempsender = (Rectangle)sender;
            Thickness tempthickness;
            double SizeFraction = 0.67;
            tempsender.Opacity = 0.8;
            tempthickness = tempsender.Margin;
            if (getMenuItem_Image_fromitemindex(LossCompass_MainChartsarea, -1, "ImageMini") == null)
            {
                GenerateRectangleUI(LossCompass_MainChartsarea, "BorderofImage", TierACanvasheight * SizeFraction, TierACanvaswidth * SizeFraction, tempthickness.Left + 10, tempthickness.Top - 20, Brushes.White, Brushes.DarkSlateGray, 2, null, null, null);
                GenerateImageUI(LossCompass_MainChartsarea, "ImageMini", TierACanvasheight * SizeFraction, TierACanvaswidth * SizeFraction, tempthickness.Left + 10, tempthickness.Top - 20, Globals.HTML.SERVER_FOLDER_PATH + tempsender.Name + ".png", null, null, null);
            }
            else
            {
                Image tempImage = getMenuItem_Image_fromitemindex(LossCompass_MainChartsarea, -1, "ImageMini");
                Rectangle tempborder = getMenuItem_Rectangle_fromitemindex(LossCompass_MainChartsarea, -1, "BorderofImage");
                tempImage.Source = new BitmapImage(new Uri(Globals.HTML.SERVER_FOLDER_PATH + tempsender.Name + ".png"));
                Canvas.SetLeft(tempImage, tempthickness.Left + 30);
                Canvas.SetTop(tempImage, tempthickness.Top + 20);
                Canvas.SetLeft(tempborder, tempthickness.Left + 30);
                Canvas.SetTop(tempborder, tempthickness.Top + 20);
                tempImage.Visibility = Visibility.Visible;
                tempborder.Visibility = Visibility.Visible;
            }
        }
        public void MiniMouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            Rectangle tempsender = (Rectangle)sender;
            tempsender.Opacity = 1.0;
            Image tempImage = getMenuItem_Image_fromitemindex(LossCompass_MainChartsarea, -1, "ImageMini");
            Rectangle tempborder = getMenuItem_Rectangle_fromitemindex(LossCompass_MainChartsarea, -1, "BorderofImage");
            if (tempImage != null)
            {
                tempImage.Visibility = Visibility.Hidden;
                tempborder.Visibility = Visibility.Hidden;
                tempImage.Source = null;
            }
        }
        public void MiniMouseDown(object sender, MouseEventArgs e)
        {
            int templevelnumber;
            Rectangle tempsender = (Rectangle)sender;
            templevelnumber = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
            if (templevelnumber == 1)
            {
                PreviousCardsLabel.Visibility = Visibility.Hidden;
            }
            intermediate.LossCompass_TierA_Level = templevelnumber;
            intermediate.LossCompass_drillDown();   // this is where intermediate sheet gets refreshed and is told which loss label was clicked.
            InitiateChartsvalues();
            AnimateZoomUIElement(0.8, 1.0, 0.1, OpacityProperty, LossCompass_MainChartsarea);

            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "TierAHeader").Content = intermediate.TierA_Header.ToString();
            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "TierBHeader").Content = intermediate.TierB_Header.ToString();
            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCHeader").Content = intermediate.TierC_Header.ToString();
            if (templevelnumber != 1)
            {
                getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCMiniHeader").Content = "... " + intermediate.TierA_Header + " > " + intermediate.TierB_Header + " > " + intermediate.TierC_Header;
            }
            Image tempImage = getMenuItem_Image_fromitemindex(LossCompass_MainChartsarea, -1, "ImageMini");
            tempImage.Source = null;
            LossCompass_MainChartsarea.Children.Remove(getMenuItem_Rectangle_fromitemindex(LossCompass_MainChartsarea, -1, "Mini" + templevelnumber));
            DeleteImagefromHarddrive(Globals.HTML.SERVER_FOLDER_PATH + tempsender.Name + ".png");

        }
        public void LossLabelResetColors(DependencyObject dep, int onlyselected = 0, string exception = "")
        {
            int j = 0;
            Label lbl;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, j);

                    if (lbl.Name.IndexOf("Bar_Label") > -1)
                    {

                        //if (lbl.Content.ToString() != "")
                        //{
                        lbl.Background = BrushColors.mybrushLossLabelDefaultColors;
                        lbl.BorderThickness = new Thickness(0, 0, 0, 0);
                        //}


                        if (onlyselected != 0)
                        {
                            if (lbl.Name.ToString().IndexOf(onlyselected.ToString()) > -1)
                            {
                                lbl.Background = BrushColors.mybrushLIGHTBLUEGREEN;
                            }
                        }

                    }
                }


            }
        }

        #endregion

        #region "CriteriaCanvasMove/Leave"
        public void CriteriaCanvasMouseMove(object sender, MouseEventArgs e)
        {
            Canvas tempsender = (Canvas)sender;
            tempsender.Opacity = 0.8;


        }
        public void CriteriaCanvasMouseLeave(object sender, MouseEventArgs e)
        {
            Canvas tempsender = (Canvas)sender;
            tempsender.Opacity = 1.0;
        }

        #endregion

        #region "CardMove/Leave/Clicked"

        public void Cardmousemove(object sender, MouseEventArgs e)
        {
            Image tempsender = (Image)sender;
            tempsender.Opacity = 0.8;

        }

        public void Cardmouseleave(object sender, MouseEventArgs e)
        {
            Image tempsender = (Image)sender;
            tempsender.Opacity = 1.0;

        }

        public void ZoomCard(object sender, MouseButtonEventArgs e)
        {
            Image tempsender = (Image)sender;
            if (tempsender.Name.ToString().IndexOf("CanvasA") > -1)
            {
                ChangeState("A", "A");
            }
            else if (tempsender.Name.ToString().IndexOf("CanvasB") > -1)
            {
                ChangeState("A", "B");
            }
            else if (tempsender.Name.ToString().IndexOf("CanvasC") > -1)
            {
            }

        }
        #endregion
        #region "PriroityIindex"

        public void addPriorities()
        {
            PriorityIndex_ComboBox.Items.Add("None");
            PriorityIndex_ComboBox.Items.Add("Chronicity");
            PriorityIndex_ComboBox.Items.Add("Survivability");
        }
        public void ManagePriorities(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region "TierScrollManagement"
        public void checkiffurtherscrollisneeded()
        {
            Image tempnavigationleftTierA = getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "Left_TierA");
            Image tempnavigationrightTierA = getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "Right_TierA");
            Image tempnavigationleftTierB = getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "Left_TierB");
            Image tempnavigationrightTierB = getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "Right_TierB");
            Image tempnavigationleftTierC = getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "Left_TierC");
            Image tempnavigationrightTierC = getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "Right_TierC");

            if (intermediate.TierA_NumberOfItems - TierAscrolloffset < 7)
            {

                tempnavigationrightTierA.Visibility = Visibility.Hidden;
                tempnavigationleftTierA.Visibility = Visibility.Visible;
            }
            else
            {
                tempnavigationrightTierA.Visibility = Visibility.Visible;
            }
            if (intermediate.TierB_NumberOfItems - TierBscrolloffset < 7)
            {
                tempnavigationrightTierB.Visibility = Visibility.Hidden;
                tempnavigationleftTierB.Visibility = Visibility.Visible;
            }
            else
            {
                tempnavigationrightTierB.Visibility = Visibility.Visible;
            }
            if (intermediate.TierC_NumberOfItems - TierCscrolloffset < 9)
            {
                tempnavigationrightTierC.Visibility = Visibility.Hidden;
                tempnavigationleftTierC.Visibility = Visibility.Visible;
            }
            else
            {
                tempnavigationrightTierC.Visibility = Visibility.Visible;
            }


            if (TierAscrolloffset == 0)
            {
                tempnavigationleftTierA.Visibility = Visibility.Hidden;
            }
            if (TierBscrolloffset == 0)
            {
                tempnavigationleftTierB.Visibility = Visibility.Hidden;
            }
            if (TierCscrolloffset == 0)
            {
                tempnavigationleftTierC.Visibility = Visibility.Hidden;
            }


        }

        public void TierCScrollClick(object sender, MouseButtonEventArgs e)
        {
            Image tempsender = (Image)sender;
            tempsender.Opacity = 0.8;

            if (tempsender.Name.IndexOf("Left") > -1 && TierCscrolloffset != 0)
            {

                TierCscrolloffset -= 1;


            }
            if (tempsender.Name.IndexOf("Right") > -1)
            {
                TierCscrolloffset += 1;
            }



            //Hide or Unhide Left Arrow
            if (TierCscrolloffset == 0)
            {
                getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "Left_TierC").Visibility = Visibility.Hidden;
            }
            else
            {
                getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "Left_TierC").Visibility = Visibility.Visible;
            }

            intermediate.LossCompass_Scroll(CardTier.C, TierCscrolloffset);
            SetupDowntimeCanvasCharts(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);
            SetupDowntimeCanvasCharts_Sim(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);
            checkiffurtherscrollisneeded();

            LossLabelResetColors(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);
            int labeltosetcolor = getselectedlabelindex_onscroll(clickedlabelIndex_scroll_TierC, clickedlabelindex_labelpos_TierC, TierBscrolloffset, 8);
            if (labeltosetcolor != -1)
            {
                LossLabelResetColors(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, labeltosetcolor);
            }
        }

        public void TierBScrollClick(object sender, MouseButtonEventArgs e)
        {
            Image tempsender = (Image)sender;
            tempsender.Opacity = 0.8;

            if (tempsender.Name.IndexOf("Left") > -1 && TierBscrolloffset != 0)
            {

                TierBscrolloffset -= 1;
            }
            if (tempsender.Name.IndexOf("Right") > -1)
            {
                TierBscrolloffset += 1;
            }


            //Hide or Unhide Left Arrow
            if (TierBscrolloffset == 0)
            {
                getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "Left_TierB").Visibility = Visibility.Hidden;
            }
            else
            {
                getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "Left_TierB").Visibility = Visibility.Visible;
            }

            intermediate.LossCompass_Scroll(CardTier.B, TierBscrolloffset);
            SetupDowntimeCanvasCharts(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);
            SetupDowntimeCanvasCharts_Sim(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);
            checkiffurtherscrollisneeded();

            LossLabelResetColors(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);
            int labeltosetcolor = getselectedlabelindex_onscroll(clickedlabelIndex_scroll_TierB, clickedlabelindex_labelpos_TierB, TierBscrolloffset, 6);
            if (labeltosetcolor != -1)
            {

                LossLabelResetColors(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, labeltosetcolor);
            }
        }

        public void TierAScrollClick(object sender, MouseButtonEventArgs e)
        {
            Image tempsender = (Image)sender;
            tempsender.Opacity = 0.8;

            if (tempsender.Name.IndexOf("Left") > -1 && TierAscrolloffset != 0)
            {

                TierAscrolloffset -= 1;
            }
            if (tempsender.Name.IndexOf("Right") > -1)
            {
                TierAscrolloffset += 1;
            }


            //Hide or Unhide Left Arrow
            if (TierAscrolloffset == 0)
            {
                getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "Left_TierA").Visibility = Visibility.Hidden;
            }
            else
            {
                getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "Left_TierA").Visibility = Visibility.Visible;
            }

            intermediate.LossCompass_Scroll(CardTier.A, TierAscrolloffset);
            SetupDowntimeCanvasCharts(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child);
            SetupDowntimeCanvasCharts_Sim(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child);
            checkiffurtherscrollisneeded();

            LossLabelResetColors(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child);
            int labeltosetcolor = getselectedlabelindex_onscroll(clickedlabelIndex_scroll_TierA, clickedlabelindex_labelpos_TierA, TierAscrolloffset, 6);
            if (labeltosetcolor != -1)
            {
                LossLabelResetColors(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, labeltosetcolor);
            }
        }

        public void Set_Default_TierScrolls()
        {
            if (TierAscrolloffset == 0)
            {
                getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "Left_TierA").Visibility = Visibility.Hidden;
            }
            if (TierBscrolloffset == 0)
            {
                getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "Left_TierB").Visibility = Visibility.Hidden;
            }
            if (TierCscrolloffset == 0)
            {
                getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "Left_TierC").Visibility = Visibility.Hidden;
            }

        }

        public int getselectedlabelindex_onscroll(int savedscrolloffset, int labelindex, int currentscrolloffset, int cardsize)
        {

            int actualindexoflabel = -1;
            int UIindexoflabel = -1;

            actualindexoflabel = savedscrolloffset + labelindex;
            UIindexoflabel = actualindexoflabel - currentscrolloffset;
            if (UIindexoflabel < 1 || UIindexoflabel > cardsize)
            {
                return -1;
            }
            else
            {
                return UIindexoflabel;
            }

        }

        #endregion

        #region "ChartDynamics"

        public void sizemybar(object sender, double barheight)
        {
            if (barheight >= 0)
            {
                Rectangle mybar = (Rectangle)sender;
                mybar.Height = barheight;
            }
        }
        public double calculatemybarheight(double lossvalue, double maxlossvalue, double barmaxsize)
        {
            if (maxlossvalue <= 0)
            {
                return 0;

            }
            else return (lossvalue / maxlossvalue) * barmaxsize;




        }
        public void locatemydatalabel(ref Label sender, double lossvalue, double maxlossvalue, double baseheight, double barmaxsize, double datalabeltopoffset)
        {
            Thickness datalabelposition = default(Thickness);
            datalabelposition = sender.Margin;
            Canvas.SetTop(sender, datalabeltopoffset + baseheight - ((lossvalue / maxlossvalue) * barmaxsize));
            //sender.Margin = new Thickness(datalabelposition.Left, baseheight - ((lossvalue / maxlossvalue) * barmaxsize), 0, 0);
        }
        public void updatemydatalabeltext(ref Label sender, double lossvalue)
        {
            sender.Content = Math.Round(lossvalue, 1);

            switch (Settings.Default.LossCompassDefaultView)

            {
                case (int)Globals.LossCompassViews.DTpct:
                    sender.Content = sender.Content + "%";
                    break;
                case (int)Globals.LossCompassViews.DTmin:
                    sender.Content = sender.Content;
                    break;
                case (int)Globals.LossCompassViews.SPD:

                    break;
                case (int)Globals.LossCompassViews.MTBFmin:
                    sender.Content = sender.Content;
                    break;
                case (int)Globals.LossCompassViews.StopsActual:
                    sender.Content = Math.Round(lossvalue, 0);
                    break;

            }

        }
        public void updatemyLOSSlabeltext(ref Label sender, string lossname)  // Loss label text and tooltips
        {
            sender.Content = lossname;
            sender.ToolTip = lossname;


        }

        public void locatemysecondarykpibubble(ref Ellipse sender, double lossvalue, double maxlossvalue, double baseheight, double bubblemaxheight)
        {
            Thickness secondarykpibubbleposition = default(Thickness);
            secondarykpibubbleposition = sender.Margin;
            //sender.Margin = new Thickness(secondarykpibubbleposition.Left, baseheight - ((lossvalue / maxlossvalue) * bubblemaxheight + sender.Height + 80), 0, 0);
            Canvas.SetTop(sender, baseheight - ((lossvalue / maxlossvalue) * bubblemaxheight));
            // Canvas.SetTop(sender, datalabeltopoffset + baseheight - ((lossvalue / maxlossvalue) * barmaxsize));

            sender.Visibility = Visibility.Visible;
            //ToolTip tp = new ToolTip();
            System.Windows.Forms.ToolTip tp = new System.Windows.Forms.ToolTip();
            tp.InitialDelay = 0;
            //tp.SetToolTip(sender, GetKPIName((DowntimeMetrics)losscompass_secondarykpiselected) + " : " + Math.Round(lossvalue, 1).ToString());
            sender.ToolTip = tp;
            sender.ToolTip = GetKPIName((DowntimeMetrics)losscompass_secondarykpiselected) + " : " + Math.Round(lossvalue, 1).ToString();

        }


        public void SetupDowntimeCanvasCharts(DependencyObject dep)

        {
            double[] LossValues;
            double[] Lossvalues_2;
            string[] LossNames;
            double maxlossvalue;
            double maxlossvalue_2;

            // offloading intermediate values for each card into local lists
            if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child)
            {
                LossValues = intermediate.TierA_Values;
                Lossvalues_2 = intermediate.TierA_Values_2;
                LossNames = intermediate.TierA_Names;
                maxlossvalue = intermediate.TierA_Max;
                maxlossvalue_2 = intermediate.TierA_Max_2;
            }
            else if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child)
            {
                LossValues = intermediate.TierB_Values;
                Lossvalues_2 = intermediate.TierB_Values_2;
                LossNames = intermediate.TierB_Names;
                maxlossvalue = intermediate.TierB_Max;
                maxlossvalue_2 = intermediate.TierB_Max_2;
            }
            else
            {
                LossValues = intermediate.TierC_Values;
                Lossvalues_2 = intermediate.TierC_Values_2;
                LossNames = intermediate.TierC_Names;
                maxlossvalue = intermediate.TierC_Max;
                maxlossvalue_2 = intermediate.TierC_Max_2;

            }

            // handling the case for maxloss is 0 to avoid NaN
            if (maxlossvalue == 0)
            {
                maxlossvalue = 1;
            }
            if (maxlossvalue_2 == 0)
            {
                maxlossvalue_2 = 1;
            }


            int i = 0;
            int j = 0;
            int k = 0;
            Label Lbl;
            Rectangle Rect;
            Ellipse Elp;

            // Size main loss compass bars
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    Rect = (Rectangle)VisualTreeHelper.GetChild(dep, j);

                    if (Rect.Name.IndexOf("Bar_Rect") > -1)
                    {
                        i = Convert.ToInt32(GlobalFcns.onlyDigits(Rect.Name)) - 1;

                        if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child)
                        {
                            sizemybar(Rect, calculatemybarheight(LossValues[i], maxlossvalue, Globals.LossCompass_chart_barMaxSizeC));
                        }
                        else
                        {
                            sizemybar(Rect, calculatemybarheight(LossValues[i], maxlossvalue, Globals.LossCompass_chart_barMaxSize));
                        }
                    }
                }

                // Locate data labels for Loss Compass bars

                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Label") > -1)
                {
                    Lbl = (Label)VisualTreeHelper.GetChild(dep, j);

                    if (Lbl.Name.IndexOf("DataLabel") > -1)
                    {
                        i = Convert.ToInt32(GlobalFcns.onlyDigits(Lbl.Name)) - 1;
                        if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child)
                        {
                            locatemydatalabel(ref Lbl, LossValues[i], maxlossvalue, Globals.LossCompass_datalabel_baseheight, Globals.LossCompass_chart_barMaxSizeC, TierCdatalabeltopoffset);
                        }
                        else
                        {
                            locatemydatalabel(ref Lbl, LossValues[i], maxlossvalue, Globals.LossCOmpass_chart_TierC_labelbaseheight, Globals.LossCompass_chart_barMaxSize, TierAdatalabeltopoffset);
                        }


                        updatemydatalabeltext(ref Lbl, LossValues[i]);
                    }

                    if (Lbl.Name.IndexOf("Bar_Label") > -1)
                    {
                        i = Convert.ToInt32(GlobalFcns.onlyDigits(Lbl.Name)) - 1;

                        updatemyLOSSlabeltext(ref Lbl, LossNames[i]);



                    }
                }

                // check if secondary axis was clicked on and then locate the secondary KPI bubbles
                if (ISSecondaryAxisOn == true)
                {
                    if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Ellipse") > -1)
                    {
                        Elp = (Ellipse)VisualTreeHelper.GetChild(dep, j);

                        if (Elp.Name.IndexOf("bubble") > -1)
                        {
                            i = Convert.ToInt32(GlobalFcns.onlyDigits(Elp.Name));

                            if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child)
                            {
                                locatemysecondarykpibubble(ref Elp, Lossvalues_2[i - 1], maxlossvalue_2, Globals.LossCompass_bubble_baseheight, TierCbubbletopoffset);
                            }
                            else
                            {
                                locatemysecondarykpibubble(ref Elp, Lossvalues_2[i - 1], maxlossvalue_2, Globals.LossCompass_bubble_baseheight, TierAbubbletopoffset);
                            }

                            Elp.Visibility = Visibility.Visible;

                        }
                    }
                }








            }
        }

        public void SetupDowntimeCanvasCharts_Sim(DependencyObject dep)

        {
            if (IScrystallballON == false)
            {
                return;
            }


            double[] LossValues;
            double[] Lossvalues_2;
            string[] LossNames;
            double maxlossvalue;
            double maxlossvalue_2;


            if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child)
            {

                LossValues = intermediate.TierA_Values_Sim;
                Lossvalues_2 = intermediate.TierA_Values_2_Sim;
                LossNames = intermediate.TierA_Names;
                maxlossvalue = intermediate.TierA_Max;
                maxlossvalue_2 = intermediate.TierA_Max_2;
            }
            else if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child)
            {
                LossValues = intermediate.TierB_Values_Sim;
                Lossvalues_2 = intermediate.TierB_Values_2_Sim;
                LossNames = intermediate.TierB_Names;
                maxlossvalue = intermediate.TierB_Max;
                maxlossvalue_2 = intermediate.TierB_Max_2;
            }
            else
            {
                LossValues = intermediate.TierC_Values_Sim;
                Lossvalues_2 = intermediate.TierC_Values_2_Sim;
                LossNames = intermediate.TierC_Names;
                maxlossvalue = intermediate.TierC_Max;
                maxlossvalue_2 = intermediate.TierC_Max_2;

            }

            if (maxlossvalue == 0)
            {
                maxlossvalue = 1;
            }
            if (maxlossvalue_2 == 0)
            {
                maxlossvalue_2 = 1;
            }


            int i = 0;
            int j = 0;
            int k = 0;
            Label Lbl;
            Rectangle Rect;
            Ellipse Elp;

            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    Rect = (Rectangle)VisualTreeHelper.GetChild(dep, j);

                    if (Rect.Name.IndexOf("Sim_Rect") > -1)
                    {
                        i = Convert.ToInt32(GlobalFcns.onlyDigits(Rect.Name)) - 1;

                        if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child)
                        {
                            sizemybar(Rect, calculatemybarheight(LossValues[i], maxlossvalue, Globals.LossCompass_chart_barMaxSizeC));
                        }
                        else
                        {
                            sizemybar(Rect, calculatemybarheight(LossValues[i], maxlossvalue, Globals.LossCompass_chart_barMaxSize));
                        }

                        AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, Rect);
                        //Thread.Sleep(50);
                    }
                }


                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Label") > -1)
                {
                    Lbl = (Label)VisualTreeHelper.GetChild(dep, j);

                    if (Lbl.Name.IndexOf("Sim_Label") > -1)
                    {
                        i = Convert.ToInt32(GlobalFcns.onlyDigits(Lbl.Name)) - 1;
                        if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child)
                        {
                            locatemydatalabel(ref Lbl, LossValues[i], maxlossvalue, Globals.LossCompass_datalabel_baseheight, Globals.LossCompass_chart_barMaxSizeC, TierCdatalabeltopoffset);
                        }
                        else
                        {
                            locatemydatalabel(ref Lbl, LossValues[i], maxlossvalue, Globals.LossCOmpass_chart_TierC_labelbaseheight, Globals.LossCompass_chart_barMaxSize, TierAdatalabeltopoffset);
                        }


                        updatemydatalabeltext(ref Lbl, LossValues[i]);
                    }

                    /* if (Lbl.Name.IndexOf("Bar_Label") > -1)
                     {
                         i = Convert.ToInt32(GlobalFcns.onlyDigits(Lbl.Name)) - 1;

                         updatemyLOSSlabeltext(ref Lbl, LossNames[i]);



                     }*/
                }

                /*
                if (ISSecondaryAxisOn == true)
                {
                    if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Ellipse") > -1)
                    {
                        Elp = (Ellipse)VisualTreeHelper.GetChild(dep, j);

                        if (Elp.Name.IndexOf("bubble") > -1)
                        {
                            i = Convert.ToInt32(GlobalFcns.onlyDigits(Elp.Name));

                            if (dep == getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child)
                            {
                                locatemysecondarykpibubble(ref Elp, Lossvalues_2[i - 1], maxlossvalue_2, 1195, Globals.LossCompass_chart_barMaxSizeC);
                            }
                            else
                            {
                                locatemysecondarykpibubble(ref Elp, Lossvalues_2[i - 1], maxlossvalue_2, Globals.LossCOmpass_chart_TierC_labelbaseheight, Globals.LossCompass_chart_barMaxSize);
                            }

                            Elp.Visibility = Visibility.Visible;

                        }
                    }
                }*/








            }
        }
        #endregion

        #region "Criteria/KPI/Axis"

        public void CriteriaCanvasClicked(object sender, MouseButtonEventArgs e)
        {
            Canvas tempsender = (Canvas)sender;

            //Setting the background colors WHITE
            SetCanvasBackgroundcolorwhite(Dimension1Canvas);
            SetCanvasBackgroundcolorwhite(Dimension2Canvas);
            SetCanvasBackgroundcolorwhite(Dimension3Canvas);
            SetCanvasBackgroundcolorwhite(Dimension4Canvas);
            SetCanvasBackgroundcolorwhite(Dimension5Canvas);
            SetCriteriaCanvasForegroundColors(tempsender);
            //


            switch (tempsender.Name)                                                                    //Determines which canvas was clicked. Changes Color. Gets the KPI number
            {
                case (string)"Dimension1Canvas":
                    tempsender.Background = BrushColors.mybrushSelectedCriteria;
                    Settings.Default.LossCompassDefaultView = (int)Globals.LossCompassViews.DTpct;
                    ColumnLegendLabel.Content = GetKPIName((DowntimeMetrics)0).ToString();
                    break;
                case (string)"Dimension2Canvas":
                    tempsender.Background = BrushColors.mybrushSelectedCriteria;
                    Settings.Default.LossCompassDefaultView = (int)Globals.LossCompassViews.DTmin;
                    ColumnLegendLabel.Content = GetKPIName((DowntimeMetrics)1).ToString();
                    break;
                case (string)"Dimension3Canvas":
                    tempsender.Background = BrushColors.mybrushSelectedCriteria;
                    Settings.Default.LossCompassDefaultView = (int)Globals.LossCompassViews.SPD;
                    ColumnLegendLabel.Content = GetKPIName((DowntimeMetrics)2).ToString();
                    break;
                case (string)"Dimension4Canvas":
                    tempsender.Background = BrushColors.mybrushSelectedCriteria;
                    Settings.Default.LossCompassDefaultView = (int)Globals.LossCompassViews.MTBFmin;
                    ColumnLegendLabel.Content = GetKPIName((DowntimeMetrics)3).ToString();
                    break;
                case (string)"Dimension5Canvas":
                    tempsender.Background = BrushColors.mybrushSelectedCriteria;
                    Settings.Default.LossCompassDefaultView = (int)Globals.LossCompassViews.StopsActual;
                    ColumnLegendLabel.Content = GetKPIName((DowntimeMetrics)4).ToString();
                    break;

            }

            intermediate.Criteria_1_SelectionChanged(Settings.Default.LossCompassDefaultView);             // Reseting the intermediate sheet. Telling it which KPI is selected
            InitiateChartsvalues();                                                                        // Charts are initiated to get new intermediate sheet values
            AnimateZoomUIElement(0.8, 1.0, 0.1, OpacityProperty, LossCompass_MainChartsarea);

            HideAllLocks();
            DisplayspecificLock(GetCurrentLock_ON() + 1, "ON");
            if (GetCurrentLock_ON() + 1 != Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name)))
            {
                DisplayspecificLock(Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name)), "OFF");
            }
        }

        public void SetCriteriaCanvasForegroundColors(DependencyObject dep)
        {

            Dimension1_Magnitude.Foreground = BrushColors.mybrushfontgray;
            Dimension1_Description.Foreground = BrushColors.mybrushfontgray;
            Dimension2_Magnitude.Foreground = BrushColors.mybrushfontgray;
            Dimension2_Description.Foreground = BrushColors.mybrushfontgray;
            Dimension3_Magnitude.Foreground = BrushColors.mybrushfontgray;
            Dimension3_Description.Foreground = BrushColors.mybrushfontgray;
            Dimension4_Magnitude.Foreground = BrushColors.mybrushfontgray;
            Dimension4_Description.Foreground = BrushColors.mybrushfontgray;
            Dimension5_Magnitude.Foreground = BrushColors.mybrushfontgray;
            Dimension5_Description.Foreground = BrushColors.mybrushfontgray;

            int j = 0;
            Label lbl;

            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, j);

                    lbl.Foreground = BrushColors.mybrushlanguagewhite;

                }
            }
        }


        public int GetCurrentLock_ON()
        {
            int i;

            for (i = 0; i <= KPIlock.Length - 1; i++)
            {
                if (KPIlock[i] == true)
                {
                    return i;

                }
            }
            return -1;
        }
        public void DisplayspecificLock(int LockNum, string LockStatus)
        {

            if (LockNum < 0)
            {
                return;
            }

            /// Show Selected Lock
            int j = 0;
            Image Lock;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(this.DowntimeChartCanvas_CriteriaOne) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(this.DowntimeChartCanvas_CriteriaOne, j).GetType().ToString().IndexOf("Image") > -1)
                {
                    Lock = (Image)VisualTreeHelper.GetChild(this.DowntimeChartCanvas_CriteriaOne, j);

                    if (Lock.Name.ToString().IndexOf(Convert.ToString(LockNum)) > -1 && Lock.Name.ToString().IndexOf(LockStatus) > -1)
                    {
                        Lock.Visibility = Visibility.Visible;
                    }

                }
            }
        }

        public void LockClicked(object sender, MouseButtonEventArgs e)
        {
            HideAllLocks();

            Image tempsender;
            tempsender = (Image)sender;
            int selectedlockindex = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
            if (tempsender.Name.IndexOf("OFF") > -1)
            {


                KPIlock[selectedlockindex - 1] = true;                        //Setting the selected lock as true 
                intermediate.LossCompass_Criteria_1_Lock(selectedlockindex - 1);        // Letting intermediate sheet know which KPI is locked. 
                DisplayspecificLock(selectedlockindex, "ON");              // KPI's lock is showed as ON.

                //Setting all other locks as false
                int j;

                for (j = 0; j < selectedlockindex - 1; j++)
                {
                    KPIlock[j] = false;
                }
                for (j = selectedlockindex; j <= 4; j++)
                {
                    KPIlock[j] = false;
                }


            }
            else       // when user intendedn to switch off a lock
            {
                KPIlock[selectedlockindex - 1] = false;                   // selected KPI's lock is set to faulse                
                DisplayspecificLock(selectedlockindex, "OFF");          // KPI's lock is showed as OFF.
                intermediate.LossCompass_Criteria_1_Unlock();
            }





        }

        public void LockMouseMove(object sender, MouseEventArgs e)
        {
            Image tempsender = (Image)sender;
            tempsender.Opacity = 0.8;


        }
        public void LockMouseLeave(object sender, MouseEventArgs e)
        {
            Image tempsender = (Image)sender;
            tempsender.Opacity = 1.0;
        }
        public void HideAllLocks(int exceptionNum = -1)
        {


            KPI1LockON.Visibility = Visibility.Hidden;
            KPI2LockON.Visibility = Visibility.Hidden;
            KPI3LockON.Visibility = Visibility.Hidden;
            KPI4LockON.Visibility = Visibility.Hidden;
            KPI5LockON.Visibility = Visibility.Hidden;
            KPI1LockOFF.Visibility = Visibility.Hidden;
            KPI2LockOFF.Visibility = Visibility.Hidden;
            KPI3LockOFF.Visibility = Visibility.Hidden;
            KPI4LockOFF.Visibility = Visibility.Hidden;
            KPI5LockOFF.Visibility = Visibility.Hidden;





        }
        private void SetCanvasBackgroundcolorwhite(Canvas cCanvas)
        {
            cCanvas.Background = BrushColors.mybrushNOTSelectedCriteria;
        }

        private void PopulateSecondaryAxisCombo()
        {
            if (AddSecondaryAxis_ComboBox.Items.Count == 0)
            {
                var SecKPIList = new List<string>();
                int i = 0;

                for (i = 0; i < 5; i++)    // 5 is hardcoded number of KPIs for the plant 
                {
                    // if (i != Settings.Default.LossCompassDefaultView)
                    //{
                    SecKPIList.Add(GetKPIName((DowntimeMetrics)i));
                    //}

                }

                AddSecondaryAxis_ComboBox.ItemsSource = SecKPIList;
                AddSecondaryAxis_ComboBox.Visibility = Visibility.Hidden;
                InitiateSecondaryAxisVisibility();
            }
        }

        private void InitiateSecondaryAxisVisibility()
        {
            BubbleLegendLabel.MouseDown += AddSecondaryAxisClicked;

            if (ISSecondaryAxisOn == true)
            {
                AddSecondaryAxis_Image.Visibility = Visibility.Hidden;
                RemoveSecondaryAxis_Image.Visibility = Visibility.Visible;
                BubbleLegendLabel.Content = losscompass_secondarykpi;


            }
            else
            {
                RemoveSecondaryAxis_Image.Visibility = Visibility.Hidden;
                AddSecondaryAxis_Image.Visibility = Visibility.Visible;
                BubbleLegendLabel.Content = "Add secondary KPI";
                //                BubbleLegendLabel.MouseDown += AddSecondaryAxisClicked;

            }
        }



        private void SecondaryAxisSelected(object sender, SelectionChangedEventArgs e)
        {

            ISSecondaryAxisOn = true;

            losscompass_secondarykpi = (string)AddSecondaryAxis_ComboBox.SelectedItem;
            losscompass_secondarykpiselected = AddSecondaryAxis_ComboBox.SelectedIndex;


            if (losscompass_secondarykpiselected != -1)
            {


                intermediate.LossCompass_SetSecondaryKPI((DowntimeMetrics)losscompass_secondarykpiselected);

                ShowSecondaryKPIbubbles();
                ISSecondaryAxisOn = true;
                AddSecondaryAxis_Image.Visibility = Visibility.Hidden;
                RemoveSecondaryAxis_Image.Visibility = Visibility.Visible;
                BubbleLegendLabel.Content = losscompass_secondarykpi;
                BubbleLegendLabel.Visibility = Visibility.Visible;
            }
            else
            {

                ISSecondaryAxisOn = false;
                AddSecondaryAxis_Image.Visibility = Visibility.Visible;
                RemoveSecondaryAxis_Image.Visibility = Visibility.Hidden;
                BubbleLegendLabel.Content = "Add secondary KPI";
                BubbleLegendLabel.Visibility = Visibility.Visible;
            }

            AddSecondaryAxis_ComboBox.Visibility = Visibility.Hidden;
        }
        private void RemoveSecondaryAxisClicked(object sender, MouseButtonEventArgs e)

        {
            ISSecondaryAxisOn = false;
            AddSecondaryAxis_ComboBox.SelectedIndex = -1;
            InitiateSecondaryAxisVisibility();


            //Actual remove secondary axes

            HideSecondaryKPI_bubbles_and_datalabels(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child);
            HideSecondaryKPI_bubbles_and_datalabels(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);
            HideSecondaryKPI_bubbles_and_datalabels(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);

        }
        private void AddSecondaryAxisClicked(object sender, MouseButtonEventArgs e)
        {
            BubbleLegendLabel.Visibility = Visibility.Hidden;
            AddSecondaryAxis_ComboBox.Visibility = Visibility.Visible;
            AddSecondaryAxis_ComboBox.IsDropDownOpen = true;

        }

        private void Cancel_HideTierSplashCanvas(object sender, MouseButtonEventArgs e)
        {
            //losscompass_secondarykpi = Globals.AddSecondaryKPI_defaultcomboboxstring;
            HideTierSplashCanvas(AddSecondaryAxis_ComboBox, Publics.f);


        }






        private void HideSecondaryKPI_bubbles_and_datalabels(DependencyObject dep)
        {
            Ellipse Elp;
            Label lbl;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Ellipse") > -1)
                {
                    Elp = (Ellipse)VisualTreeHelper.GetChild(dep, j);

                    if (Elp.Name.IndexOf("bubble") > -1)
                    {

                        Elp.Visibility = Visibility.Hidden;

                    }
                }

                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, j);

                    if (lbl.Name.IndexOf("bubble") > -1)
                    {

                        lbl.Visibility = Visibility.Hidden;

                    }
                }
            }
        }

        private void HideTierSplashCanvas(object sender, MouseButtonEventArgs e)
        {
            //  TierASplash.Visibility = Visibility.Hidden;
            // TierBSplash.Visibility = Visibility.Hidden;
            //TierCSplash.Visibility = Visibility.Hidden;
        }

        private void ShowSecondaryKPIDataLabel(object sender, MouseButtonEventArgs e)
        {

        }

        private void ShowSecondaryKPIbubbles()
        {
            SetupDowntimeCanvasCharts(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child);
            SetupDowntimeCanvasCharts(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);    // on first loss compass load, these charts may contain zero values. Null will throw error
            SetupDowntimeCanvasCharts(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);  // on first loss compass load, these charts may contain zero values. Null will throw error

        }

        #endregion


        #region ManageToolTips

        public void ManageLossCompassToolTips()
        {

            switch (Settings.Default.LanguageActive)

            {
                case 0: //English


                    // Secondary KPI stuff

                    AddSecondaryAxis_Image.ToolTip = "Add a secondary axis to compare failure modes against 2 KPIs.";
                    RemoveSecondaryAxis_Image.ToolTip = "Remove secondary KPI";

                    // Losslabels and values

                    break;

                case 1:  //Chinese

                    break;


            }



        }


        #endregion


        //Names of KPIs
        public string GetKPIName(DowntimeMetrics index)
        {
            switch (index)
            {

                case DowntimeMetrics.DTpct:
                    return "Downtime Percent";
                case DowntimeMetrics.DT:
                    return "Downtime (min)";
                case DowntimeMetrics.SPD:
                    return "Stops per day";
                case DowntimeMetrics.MTBF:
                    return "MTBF (min)";
                case DowntimeMetrics.Stops:
                    return "Actual stops";
                case DowntimeMetrics.MTTR:
                    return "MTTR";
            }
            return "";
        }
        public void InitiateChartsvalues()
        {

            //This is the main initializatation function. It sets charts' properties such as height of bars, loss names, values, data labels for each Tier chart.
            // Intermediate sheet refresh is not done in this function. It always adapts to whatever current initermediate sheet values are.
            checkiffurtherscrollisneeded();
            SetupDowntimeCanvasCharts(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child);
            SetupDowntimeCanvasCharts(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);    // on first loss compass load, these charts may contain zero values. Null will throw error
            SetupDowntimeCanvasCharts(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);  // on first loss compass load, these charts may contain zero values. Null will throw error

            SetupDowntimeCanvasCharts_Sim(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child);
            SetupDowntimeCanvasCharts_Sim(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child);    // on first loss compass load, these charts may contain zero values. Null will throw error
            SetupDowntimeCanvasCharts_Sim(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child);  // on first loss compass load, these charts may contain zero values. Null will throw error


            InitiateKPIvalues();                                   // Main KPI like DT%, DTmin, SPD, Actual Stops, MTBF on the right side of loss compass canvas are initiated with values from intermediate sheet



        }

        public void InitiateKPIvalues()
        {

            Dimension1_Magnitude.Content = Math.Round(100 * intermediate.LossCompass_KPI1, 1).ToString() + "%";
            Dimension2_Magnitude.Content = Math.Round(intermediate.LossCompass_KPI2).ToString();
            Dimension3_Magnitude.Content = Math.Round(intermediate.LossCompass_KPI3, 2).ToString();
            Dimension4_Magnitude.Content = Math.Round(intermediate.LossCompass_KPI4, 2).ToString();
            Dimension5_Magnitude.Content = intermediate.LossCompass_KPI5.ToString();

            //Manage font size of label content based on number of digits in KPI magnitude
            if (intermediate.LossCompass_KPI2 / 100000 > 1)
            {
                Dimension2_Magnitude.FontSize = 18;
            }
            if (intermediate.LossCompass_KPI3 / 100000 > 1)
            {
                Dimension3_Magnitude.FontSize = 18;
            }
            if (intermediate.LossCompass_KPI4 / 100000 > 1)
            {
                Dimension4_Magnitude.FontSize = 18;
            }
            if (intermediate.LossCompass_KPI5 / 100000 > 1)
            {
                Dimension5_Magnitude.FontSize = 18;
            }



        }

        public void KPIHeader1Clicked(object sender, MouseButtonEventArgs e)
        {
            string[] splits2;
            splits2 = Mouse.GetPosition(this).ToString().Split(',');   // find mouse position
            FloatingToolTipCanvas.Margin = new Thickness(((Convert.ToDouble(splits2[0]) * (this.Height / this.ActualHeight))), ((Convert.ToDouble(splits2[1]) * (this.Width / this.ActualWidth) - 65)), 0, 0);
            FloatingToolTipCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, FloatingToolTipCanvas);
            System.Windows.Forms.Application.DoEvents();

            Tooltip_failuremodenamelabel.Content = "Jobs";
            //populateRawDataWindow(GetLossName_forCardandIndex(splits1[1], Convert.ToInt32(splits1[2])), splits1[1]);
            //populateLossTrendsfromLossCompass(intermediate.LossCompass_GetMapping_A(tempcardname), GetLossName_forCardandIndex(splits1[1], Convert.ToInt32(splits1[2])));



        }
        #endregion

        #region CrystalBall

        public void LaunchCrystallBall(object sender, MouseButtonEventArgs e)
        {
            if (CrystallBallOnIcon.Visibility != Visibility.Visible)
            {
                // CrystalBallLaunchCanvas.Background = BrushColors.mybrushCRYSTALLBALLselected;
                //CrystallBallLaunchLabel.Foreground = Brushes.White;
                ShowChangeLog_Label.Visibility = Visibility.Visible;
                IScrystallballON = true;
                BLUECrystallBallLaunchIcon.Visibility = Visibility.Visible;
                CrystallBallOnIcon.Visibility = Visibility.Visible;
                InitiateCrystallBall_UI();
                return;
            }
            else
            {
                // CrystalBallLaunchCanvas.Background = BrushColors.mybrushCRYSTALLBALL_NOT_selected;
                // CrystallBallLaunchLabel.Foreground = BrushColors.mybrushfontgray;
                IScrystallballON = false;
                BLUECrystallBallLaunchIcon.Visibility = Visibility.Hidden;
                ShowChangeLog_Label.Visibility = Visibility.Hidden;
                CrystallBallOnIcon.Visibility = Visibility.Hidden;
                ShutDownCrystallBall_UI();
                CrystallBall_ChangeLog_Canvas.Visibility = Visibility.Hidden;
                DowntimeChartCanvas_CriteriaOne.Visibility = Visibility.Visible;
            }

        }

        public void ShowChangeLog(object sender, MouseButtonEventArgs e)
        {
            if (CrystallBall_ChangeLog_Canvas.Visibility == Visibility.Hidden)
            {
                ShowChangeLog_Label.Content = "Hide Simulation Log";
                CrystallBall_ChangeLog_Canvas.Visibility = Visibility.Visible;
                DowntimeChartCanvas_CriteriaOne.Visibility = Visibility.Hidden;
            }
            else
            {
                ShowChangeLog_Label.Content = "Show Simulation Log";
                CrystallBall_ChangeLog_Canvas.Visibility = Visibility.Hidden;
                DowntimeChartCanvas_CriteriaOne.Visibility = Visibility.Visible;

            }
        }

        public void InitiateCrystallBall_UI()
        {

            intermediate.LossCompass_CrystalBall_TurnOn();
            //Hide all mapping icons and labels in each ViewBox
            //A
            getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "Mapping_Btn").Visibility = Visibility.Hidden;
            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "Remap").Visibility = Visibility.Hidden;
            //B
            getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "Mapping_Btn").Visibility = Visibility.Hidden;
            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "Remap").Visibility = Visibility.Hidden;
            //C
            getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "Mapping_Btn").Visibility = Visibility.Hidden;
            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "Remap").Visibility = Visibility.Hidden;

            //Coloring all bars gray
            Canvas TempCanvas;
            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child;
            RecolorAllChartColumns(BrushColors.mybrushlightgray, TempCanvas);
            Generate_CrystalBall_Bars(TempCanvas, 6);
            SetupDowntimeCanvasCharts_Sim(TempCanvas);


            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child;
            RecolorAllChartColumns(BrushColors.mybrushlightgray, TempCanvas);
            Generate_CrystalBall_Bars(TempCanvas, 6);
            SetupDowntimeCanvasCharts_Sim(TempCanvas);

            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child;
            RecolorAllChartColumns(BrushColors.mybrushlightgray, TempCanvas);
            Generate_CrystalBall_Bars(TempCanvas, 8);
            SetupDowntimeCanvasCharts_Sim(TempCanvas);


        }

        public void ShutDownCrystallBall_UI()
        {
            intermediate.LossCompass_CrystalBall_TurnOff();
            FloatingSimulatorCanvas.Visibility = Visibility.Hidden;

            //Show all mapping icons and labels in each ViewBox
            //A
            getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "Mapping_Btn").Visibility = Visibility.Visible;
            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "Remap").Visibility = Visibility.Visible;
            //B
            getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "Mapping_Btn").Visibility = Visibility.Visible;
            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "Remap").Visibility = Visibility.Visible;
            //C
            getMenuItem_Image_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "Mapping_Btn").Visibility = Visibility.Visible;
            getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "Remap").Visibility = Visibility.Visible;

            //Coloring all bars blue
            Canvas TempCanvas;
            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child;
            RecolorAllChartColumns(BrushColors.mybrushSelectedCriteria, TempCanvas);
            Remove_CrystalBall_Bars(TempCanvas, 6);

            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child;
            RecolorAllChartColumns(BrushColors.mybrushSelectedCriteria, TempCanvas);
            Remove_CrystalBall_Bars(TempCanvas, 6);

            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child;
            RecolorAllChartColumns(BrushColors.mybrushSelectedCriteria, TempCanvas);
            Remove_CrystalBall_Bars(TempCanvas, 8);
            InitiateHeadervalues();
        }

        public void LoadCurrentValuesto_FloatingSimulator(CardTier cardname, string Failuremodename)
        {
            Simulation_OriginalVal1_Label.Content = intermediate.LossCompass_CrystalBall_OriginalStopsForFailureMode(Failuremodename, cardname).ToString();
            Simulation_OriginalVal2_Label.Content = intermediate.LossCompass_CrystalBall_OriginalMTTRForFailureMode(Failuremodename, cardname).ToString();
            Simulation_Input1_TextBox.Text = intermediate.LossCompass_CrystalBall_NewStopsForFailureMode(Failuremodename, cardname).ToString();
            Simulation_Input2_TextBox.Text = intermediate.LossCompass_CrystalBall_NewMTTRForFailureMode(Failuremodename, cardname).ToString();
            if ((DateTime)intermediate.LossCompass_CrystalBall_GetDueDate(cardname, Failuremodename) != new DateTime(0))
            {
                Simulation_DatePicker.SelectedDate = intermediate.LossCompass_CrystalBall_GetDueDate(cardname, Failuremodename);
            }



        }
        public void SimulationChangeLog_Edit(object sender, RoutedEventArgs e)
        {
            if (CrystallBallChangeLog_ListView.SelectedIndex != -1)
            {

                string fmname;
                CardTier cardname;

                fmname = CrystalBallChangeLog[CrystallBallChangeLog_ListView.SelectedIndex].Name;
                cardname = CrystalBallChangeLog[CrystallBallChangeLog_ListView.SelectedIndex].OriginalCardTier;
                LoadCurrentValuesto_FloatingSimulator(cardname, fmname);
                SimulationFailureModeName.Content = fmname;
                FloatingSimulatorCanvas.Visibility = Visibility.Visible;
            }

        }

        public void SimulationChangeLog_Delete(object sender, RoutedEventArgs e)
        {
            if (CrystallBallChangeLog_ListView.Items.Count > 0)
            {

            }

        }

        public void SimulationChangeLogselectionchanged(object sender, RoutedEventArgs e)
        {

            // LocateFloatingSimulator(cardname, fmname);
        }
        public void SimulationStarted(object sender, MouseButtonEventArgs e)
        {
            intermediate.LossCompass_CrystalBall_Simulate(SimulationFailureModeName.Content.ToString(), TempCardTier_ForSimulator, Convert.ToDouble(Simulation_Input1_TextBox.Text), Convert.ToDouble(Simulation_Input2_TextBox.Text), Convert.ToDateTime(Simulation_DatePicker.SelectedDate));
            Canvas TempCanvas;
            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child;
            SetupDowntimeCanvasCharts_Sim(TempCanvas);
            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child;
            SetupDowntimeCanvasCharts_Sim(TempCanvas);
            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child;
            SetupDowntimeCanvasCharts_Sim(TempCanvas);
            CloseFloatingSimulator(FloatingSimulator_Closebtn, Publics.f);


            InitiateHeadervalues_Sim();
            PopulateCrystalBall_ChangeLog();
        }

        public void SimulationCleared(object sender, MouseButtonEventArgs e)
        {

            intermediate.LossCompass_CrystalBall_ClearSimulation(SimulationFailureModeName.Content.ToString(), TempCardTier_ForSimulator);
            Canvas TempCanvas;
            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child;
            SetupDowntimeCanvasCharts_Sim(TempCanvas);
            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child;
            SetupDowntimeCanvasCharts_Sim(TempCanvas);
            TempCanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child;
            SetupDowntimeCanvasCharts_Sim(TempCanvas);
            CloseFloatingSimulator(FloatingSimulator_Closebtn, Publics.f);


            InitiateHeadervalues_Sim();
            PopulateCrystalBall_ChangeLog();
        }

        public void ShowFloatingSimulator(object sender, MouseButtonEventArgs e)
        {
            FloatingSimulatorCanvas.Visibility = Visibility.Visible;

        }
        public void CloseFloatingSimulator(object sender, MouseButtonEventArgs e)
        {
            FloatingSimulatorCanvas.Visibility = Visibility.Hidden;

        }

        private void LocateFloatingSimulator(CardTier cardname, string fmname)
        {
            int fmno = 0;
            int i;
            Rectangle temprect;
            Viewbox tempTierviewbox;
            if (cardname == CardTier.A)
            {
                for (i = 0; i < intermediate.TierA_NumberOfItems; i++)
                {
                    if (intermediate.TierA_Names[i] == fmname)
                    {
                        fmno = i + 1;
                    }
                }
            }
            else if (cardname == CardTier.B)
            {
                for (i = 0; i < intermediate.TierB_NumberOfItems; i++)
                {
                    if (intermediate.TierB_Names[i] == fmname)
                    {
                        fmno = i + 1;
                    }
                }
            }
            else if (cardname == CardTier.C)
            {
                for (i = 0; i < intermediate.TierC_NumberOfItems; i++)
                {
                    if (intermediate.TierC_Names[i] == fmname)
                    {
                        fmno = i + 1;
                    }
                }
            }

            string searchstringbarname = "Bar_Rect_" + cardname + "_" + fmno;
            tempTierviewbox = getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "DowntimeChartViewBox" + cardname);

            temprect = getMenuItem_Rectangle_fromitemindex((Canvas)tempTierviewbox.Child, -1, "", "Bar_Rect_" + cardname + "_" + fmno);

            // FloatingSimulatorCanvas
        }

        private ObservableCollection<CrystalBallSimulation> _CrystalBallChangeLog = new ObservableCollection<CrystalBallSimulation>();
        public ObservableCollection<CrystalBallSimulation> CrystalBallChangeLog { get { return _CrystalBallChangeLog; } }

        public void PopulateCrystalBall_ChangeLog()
        {

            _CrystalBallChangeLog.Clear();

            List<CrystalBallSimulation> tmpList = intermediate.LossCompass_CrystalBall_Changelog;
            for (int i = 0; i < tmpList.Count; i++) { _CrystalBallChangeLog.Add(tmpList[i]); }

        }






        #endregion

        #region xSigma
        private bool xSigma_Unplanned_EventsOn = true;
        private bool xSigma_Planned_EventsOn = true;
        //event handler for unplanned gridview
        public void xSigma_Unplanned_Gridview_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            if (e.AddedItems.Count > 0 && xSigma_Unplanned_EventsOn) //if there are items && we want to handle events
            {
                xSigma_DisplayEvent tmpEvent = (xSigma_DisplayEvent)e.AddedItems[0];
                string modeName = tmpEvent.Name;
                int tmpIndex = -1;
                for (int i = 0; i < intermediate.xSigma_Daily_Names.Count; i++)
                {
                    if (intermediate.xSigma_Daily_Names[i] == modeName) { tmpIndex = i; break; }
                }
                if (tmpIndex > -1)
                {
                    //   intermediate.xSigma_Daily_FailureModeSelected(tmpIndex);
                    xSigma_Unplanned_EventsOn = false;
                    CS_Bubbles_Clicked(getMenuItem_Ellipse_fromitemindex(DailyincontrolGraphicsArea, -1, "", "CSbubble" + (tmpIndex + 1)), Publics.f);
                    xSigma_Unplanned_EventsOn = true;
                }
                else
                {
                    MessageBox.Show(modeName);
                }
            }
        }

        public void xSigma_Planned_Gridview_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            if (e.AddedItems.Count > 0 && xSigma_Planned_EventsOn) //if there are items && we want to handle events
            {
                xSigma_DisplayEvent tmpEvent = (xSigma_DisplayEvent)e.AddedItems[0];
                string modeName = tmpEvent.Name;
                int tmpIndex = -1;
                for (int i = 0; i < intermediate.xSigma_Daily_Names.Count; i++)
                {
                    if (intermediate.xSigma_Daily_Names[i] == modeName) { tmpIndex = i; break; }
                }
                if (tmpIndex > -1)
                {
                    //   intermediate.xSigma_Daily_FailureModeSelected(tmpIndex);
                    xSigma_Planned_EventsOn = false;
                    OnTarget_Bubbles_Clicked(getMenuItem_Ellipse_fromitemindex(DailyincontrolGraphicsArea, -1, "", "CSbubble" + (tmpIndex + 1)), Publics.f);
                    xSigma_Planned_EventsOn = true;
                }
                else
                {
                    MessageBox.Show("Error #377 - " + modeName + " not found!");
                }
            }
        }


        public void ToggleShowHide_ChronicSporadic(object sender, MouseButtonEventArgs e)                // when Chronic-Sporadic icon on the left is clicked
        {
            if (ChronicSporadicCanvas.Visibility != Visibility.Visible)
            {
                HideAllDashboards();
                ChronicSporadicCanvas.Visibility = Visibility.Visible;
                xSigma_Onload();
            }
        }

        public void xSigma_Onload()
        {
            //Show some
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, ChronicSporadicCanvas);

            //for the trend charts
            SetupTrendincontrolCharts();

            //get the bubbles ready and sized
            SetupBubbleCharts(DailyincontrolGraphicsArea, OriginalWIdth_Dailyincontrolgraphicsareacanvas);
        }

        public void CSUnplannedClicked(object sender, MouseButtonEventArgs e)
        {
            intermediate.xSigma_PlannedUnplanned_ModeRefresh(TopLevelSelected.Unplanned);
            CSSelectionBar2.Visibility = Visibility.Hidden;
            CSSelectionBar1.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0, 95, 0.2, WidthProperty, CSSelectionBar1);
            CS_Unplanned_Canvas.Visibility = Visibility.Visible;
            CS_Planned_Canvas.Visibility = Visibility.Hidden;
        }
        public void CSPlannedClicked(object sender, MouseButtonEventArgs e)
        {
            intermediate.xSigma_PlannedUnplanned_ModeRefresh(TopLevelSelected.Planned);
            CSSelectionBar1.Visibility = Visibility.Hidden;
            CSSelectionBar2.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0, 73, 0.2, WidthProperty, CSSelectionBar2);
            CS_Planned_Canvas.Visibility = Visibility.Visible;
            CS_Unplanned_Canvas.Visibility = Visibility.Hidden;
            OnTarget_CreateBellCurveDots();

            xSigma_Planned_UpdateChartFromIntermediateSheet();
            SetupBubbleCharts(OntargetGraphicsArea, OriginalWIdth_Dailyincontrolgraphicsareacanvas);
            xSigma_Planned_VarianceCharts_Generate();
        }

        public void CS_remapClicked(object sender, MouseButtonEventArgs e)
        {
            Mapping1_Combobox.Items.Clear();

            MappingSplashCanvas.Visibility = Visibility.Visible;
            MappingDestinationLabel.Content = "xSigma Mapping";

            Mapping1_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.A);
            Mapping2_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.B);

            Mapping1_Combobox.SelectedItem = getStringForEnum(intermediate.xSigma_Mapping_A);
            Mapping2_Combobox.SelectedItem = getStringForEnum(intermediate.xSigma_Mapping_B);


            AnimateZoomUIElement(0.1, 1.0, 0.3, OpacityProperty, MappingSplashCanvas);
        }

        #region Trend xSigma
        public void SetupTrendincontrolCharts()
        {
            Trendsincontrol_Header.Content = "Chronic and sporadic job activity over last " + intermediate.xSigma_Trend_NumberOfDays.ToString() + " days.";
            var top_rect_values = new List<double>();
            var middle_rect_values = new List<double>();
            var bottom_rect_values = new List<double>();
            double maxvalue = Math.Min(100, intermediate.xSigma_Trend_MaxUnplannedLoss + 10);
            //Dummy
            int j = 0;
            var rnd = new Random();
            for (j = 0; j < intermediate.xSigma_Trend_NumberOfDays; j++)
            {
                bottom_rect_values.Add(intermediate.xSigma_TrendBottom_Values[j]);
                middle_rect_values.Add(intermediate.xSigma_TrendMiddle_Values[j] + intermediate.xSigma_TrendBottom_Values[j]);
                top_rect_values.Add(intermediate.xSigma_TrendTop_Values[j] + intermediate.xSigma_TrendMiddle_Values[j] + intermediate.xSigma_TrendBottom_Values[j]);
            }




            createbargraphs(true, Trendsincontrol_ChartCanvas, intermediate.xSigma_Trend_NumberOfDays, top_rect_values, maxvalue, Brushes.OrangeRed, CS_Overviewbars_Clicked, CS_Overviewbars_Move, CS_Overviewbars_Leave, "topbar");
            createbargraphs(false, Trendsincontrol_ChartCanvas, intermediate.xSigma_Trend_NumberOfDays, middle_rect_values, maxvalue, Brushes.DimGray, CS_Overviewbars_Clicked, CS_Overviewbars_Move, CS_Overviewbars_Leave, "middlebar");
            createbargraphs(false, Trendsincontrol_ChartCanvas, intermediate.xSigma_Trend_NumberOfDays, bottom_rect_values, maxvalue, BrushColors.mybrushSelectedCriteria, CS_Overviewbars_Clicked, CS_Overviewbars_Move, CS_Overviewbars_Leave, "bottombar");
            int m;
            Rectangle rect1;
            Canvas dep = Trendsincontrol_ChartCanvas;
            for (m = 0; m <= VisualTreeHelper.GetChildrenCount(dep) - 1; m++)
            {
                if (VisualTreeHelper.GetChild(dep, m).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect1 = (Rectangle)VisualTreeHelper.GetChild(dep, m);

                    AnimateZoomUIElement(0.2, 1.0, 0.05, OpacityProperty, rect1);
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(1);
                }

            }

            CS_Overviewbars_Clicked(getMenuItem_Rectangle_fromitemindex(Trendsincontrol_ChartCanvas, intermediate.xSigma_Trend_NumberOfDays - 1), Publics.f);

        }

        public void TrendsIncontrolDailyTimeFrameClicked(object sender, MouseButtonEventArgs e)
        {
            Trendsincontrol_dailybutton.Background = BrushColors.mybrushSelectedCriteria;
            Trendsincontrol_weeklybutton.Background = BrushColors.mybrushLIGHTGRAY;
            intermediate.xSigma_Trends_SetTimePeriodResolution(1);
            SetupTrendincontrolCharts();
        }
        public void TrendsIncontrolWeeklyTimeFrameClicked(object sender, MouseButtonEventArgs e)
        {
            Trendsincontrol_weeklybutton.Background = BrushColors.mybrushSelectedCriteria;
            Trendsincontrol_dailybutton.Background = BrushColors.mybrushLIGHTGRAY;
            intermediate.xSigma_Trends_SetTimePeriodResolution(7);
            SetupTrendincontrolCharts();
        }
        public void CS_Trends_CanvasClicked(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType().ToString().Contains("Canvas"))
            {
                int m;
                Rectangle rect1;
                Canvas dep = Trendsincontrol_ChartCanvas;
                for (m = 0; m <= VisualTreeHelper.GetChildrenCount(dep) - 1; m++)
                {
                    if (VisualTreeHelper.GetChild(dep, m).GetType().ToString().IndexOf("Rectangle") > -1)
                    {
                        rect1 = (Rectangle)VisualTreeHelper.GetChild(dep, m);
                        rect1.Opacity = 1.0;
                        //rect1.StrokeThickness = 0;
                    }

                }
            }

        }

        public void CS_Overviewbars_Clicked(object sender, MouseButtonEventArgs e)
        {
            int barclicked_number;

            Rectangle tempsender = (Rectangle)sender;
            barclicked_number = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
            intermediate.xSigma_NewDateSelected(barclicked_number);


            int m;
            Rectangle rect1;
            Canvas dep = Trendsincontrol_ChartCanvas;
            for (m = 0; m <= VisualTreeHelper.GetChildrenCount(dep) - 1; m++)
            {
                if (VisualTreeHelper.GetChild(dep, m).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect1 = (Rectangle)VisualTreeHelper.GetChild(dep, m);
                    rect1.Opacity = 0.1;
                    rect1.StrokeThickness = 0;

                }

            }

            tempsender.Opacity = 1.0;
            tempsender.Stroke = Brushes.Black;
            tempsender.StrokeThickness = 0.8;

            SetupBubbleCharts(DailyincontrolGraphicsArea, 587, barclicked_number);

            //functions for updating the respective raw data views
            CS_PopulateUnplannedDataWindow();
            CS_Planned_PopulateUnplannedDataWindow();
        }

        public void CS_Overviewbars_Move(object sender, MouseEventArgs e)
        {
            Rectangle tempsender = (Rectangle)sender;
            DateTime tempdate;
            int tempsendernumber = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            tempsender.Opacity = 0.7;
            tempdate = intermediate.xSigma_Trend_Dates[tempsendernumber];


            try
            {
                ChronicLegend.Content = "Chronic " + Math.Round(intermediate.xSigma_TrendBottom_Values[tempsendernumber], 1) + "%";
                SporadicLegend.Content = "Sporadic " + Math.Round(intermediate.xSigma_TrendTop_Values[tempsendernumber], 1) + "%";
                NeutralLegend.Content = "Neutral " + Math.Round(intermediate.xSigma_TrendMiddle_Values[tempsendernumber], 1) + "%";
                IncontrolTrendsDateLegend.Content = "Date: " + tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture) + " " + tempdate.ToString("yyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                return;
            }

        }

        public void CS_Overviewbars_Leave(object sender, MouseEventArgs e)
        {
            Rectangle tempsender = (Rectangle)sender;

            tempsender.Opacity = 1.0;
            ChronicLegend.Content = "Chronic";
            SporadicLegend.Content = "Sporadic";
            NeutralLegend.Content = "Neutral";
            IncontrolTrendsDateLegend.Content = "";

        }
        #endregion
        #region Daily xSigma


        #region IncontrolspecificbubblechartandSPCfunctions

        public void CS_Bubbles_Clicked(object sender, MouseButtonEventArgs e)
        {

            int j = 0;
            Ellipse el;
            DependencyObject dep = DailyincontrolGraphicsArea;

            //Removing all ellipses' strokes
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Ellipse") > -1)
                {
                    el = (Ellipse)VisualTreeHelper.GetChild(dep, j);
                    el.StrokeThickness = 0.0;
                }
            }
            //
            Ellipse tempsender = (Ellipse)sender;

            //Selected ellipse is given a stroke
            tempsender.StrokeThickness = 1.5;

            // update label & selection info
            int Bubbleclicked_number = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString())) - 1;
            intermediate.xSigma_Daily_FailureModeSelected(Bubbleclicked_number);
            Dailyincontrol_failuremodenamelabel.Content = intermediate.xSigma_Daily_Names[Bubbleclicked_number];

            //select corresponding gridview item
            if (xSigma_Unplanned_EventsOn)
            {
                for (int i = 0; i < xS_Unplanned_RawDataGridView.Items.Count; i++)
                {
                    xSigma_DisplayEvent tmpEvent = (xSigma_DisplayEvent)xS_Unplanned_RawDataGridView.Items[i];
                    if (tmpEvent.Name == intermediate.xSigma_Daily_Names[Bubbleclicked_number])
                    {
                        xSigma_Unplanned_EventsOn = false;
                        xS_Unplanned_RawDataGridView.SelectedItem = tmpEvent;
                        xS_Unplanned_RawDataGridView.Focus();
                        xS_Unplanned_RawDataGridView.ScrollIndexIntoViewAsync(i, null);
                        xSigma_Unplanned_EventsOn = true;
                        i = xS_Unplanned_RawDataGridView.Items.Count; //exit for loop
                    }
                }
            }
            //populate telerik control chart
            SigmaControl_Unplanned_UpdateChartFromIntermediateSheet();
        }


        public void CS_Bubbles_Move(object sender, MouseEventArgs e)
        {
            Ellipse tempsender = (Ellipse)sender;
            tempsender.Opacity = 0.8;
        }

        public void CS_Bubbles_Leave(object sender, MouseEventArgs e)
        {
            Ellipse tempsender = (Ellipse)sender;
            tempsender.Opacity = 1.0;
        }
        #endregion

        #region OnTargetspecificbubblechartfunctions

        public void OnTarget_Bubbles_Clicked(object sender, MouseButtonEventArgs e)
        {
            /*
            int j = 0;
            Ellipse el;
            DependencyObject dep = OntargetGraphicsArea;

            //Removing all ellipses' strokes
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Ellipse") > -1)
                {
                    el = (Ellipse) VisualTreeHelper.GetChild(dep, j);
                    el.StrokeThickness = 0.0;
                }
            }
            //
            Ellipse tempsender = (Ellipse) sender;


            //Selected ellipse is given a stroke
            tempsender.StrokeThickness = 1.5;

            //Update values for failuremode name, stops, mttr and pr
            int Bubbleclicked_number = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString())) - 1;
            intermediate.xS_OnTarget_FailureModeSelected(Bubbleclicked_number);


            //select corresponding gridview item
            if (xSigma_Planned_EventsOn)
            {
                for (int i = 0; i < xS_Planned_RawDataGridView.Items.Count; i++)
                {
                    xSigma_DisplayEvent tmpEvent = (xSigma_DisplayEvent) xS_Planned_RawDataGridView.Items[i];
                    if (tmpEvent.Name == intermediate.xSigma_Daily_Names[Bubbleclicked_number])
                    {
                        xSigma_Planned_EventsOn = false;
                        xS_Planned_RawDataGridView.SelectedItem = tmpEvent;
                        xS_Planned_RawDataGridView.Focus();
                        xS_Planned_RawDataGridView.ScrollIndexIntoViewAsync(i, null);
                        xSigma_Planned_EventsOn = true;
                        i = xS_Planned_RawDataGridView.Items.Count; //exit for loop
                    }
                }
            }

            OnTarget_CreateBellCurveDots();
            */
        }

        public void OnTarget_Bubbles_Move(object sender, MouseEventArgs e)
        {
            Ellipse tempsender = (Ellipse)sender;

            tempsender.Opacity = 0.8;

        }

        public void OnTarget_Bubbles_Leave(object sender, MouseEventArgs e)
        {
            Ellipse tempsender = (Ellipse)sender;

            tempsender.Opacity = 1.0;

        }

        public void OnTarget_CreateBellCurveDots(Canvas dep = null, bool isonlydots = false, bool barseparate = false)
        {

            double mindrn = intermediate.xSigma_OnTarget_Selected_Distribution_DurationMin;
            double maxdrn = intermediate.xSigma_OnTarget_Selected_Distribution_DurationMax;
            double targetdrn = intermediate.xSigma_OnTarget_Selected_Distribution_DurationTarget;
            double tempPOS = 0.0;
            double totalnoofevents = intermediate.xSigma_OnTarget_Selected_Distribution_NetEvents;
            double actdrn = 0;
            List<double> listofeventsduration = new List<double>();
            List<double> listofhistogramvalues = new List<double>();
            List<string> listofhistogrambucketrange = new List<string>();


            //remove existing elipses and rectangles
            int m;
            Ellipse elp1;
            Rectangle rect1;
            Label lbl;
            if (dep == null) { dep = Ontargetbellcurve_dotscanvas; }

            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Ellipse") > -1)
                {
                    elp1 = (Ellipse)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(elp1);


                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect1 = (Rectangle)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(rect1);


                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(lbl);


                }

            }
            int k;


            // generate dots for every selected planned event type

            for (k = 0; k < totalnoofevents; k++)
            {

                actdrn = intermediate.xSigma_OnTarget_Selected_Distribution_EventDurations[k];


                tempPOS = ((actdrn - mindrn) / (maxdrn - mindrn)) * dep.Width;
                if (isonlydots == true)
                {
                    GenerateEllipseUI(dep, "dot" + k, 15, 15, tempPOS, dep.Height / 2 - 7.5, Brushes.White, BrushColors.mybrushSelectedCriteria, 1, null, null, null, 0, -1, 0.5, Math.Round(actdrn, 1).ToString() + " minutes");
                    //              AnimateZoomUIElement(0.2, 1.0, 0.05, OpacityProperty, getMenuItem_Ellipse_fromitemindex(Ontargetbellcurve_dotscanvas, -1, "dot" + k));
                    // System.Windows.Forms.Application.DoEvents();
                    //   Thread.Sleep(1);
                }

                listofeventsduration.Add(actdrn);

            }

            //create target line
            // tempPOS = ((targetdrn - mindrn) / (maxdrn - mindrn)) * dep.Width;
            // GenerateRectangleUI(dep, "targetduration", 35, 1, tempPOS, 0, Brushes.OrangeRed, null, 0, null, null, null);



            //creation of histogram for bell curve
            listofeventsduration.Sort();

            int j;
            int tempeventcount = 0;
            double offsetactualduration = 0;
            double currentbucket_upperlimit = 0;
            double maxhistogramvalue = 0;
            double histogramgraphicwidth = 0;
            double histogram_valuewidth = 0.1 * (maxdrn - mindrn);

            Rectangle temprect;

            for (j = Convert.ToInt32(mindrn); j < Convert.ToInt32(maxdrn); j++)

            {
                tempeventcount = 0;
                currentbucket_upperlimit = currentbucket_upperlimit + histogram_valuewidth;
                for (k = 0; k <= listofeventsduration.Count - 1; k++)
                {
                    offsetactualduration = listofeventsduration[k] - mindrn;
                    if (offsetactualduration >= currentbucket_upperlimit - histogram_valuewidth && offsetactualduration < currentbucket_upperlimit)
                    {
                        tempeventcount = tempeventcount + 1;
                    }
                }
                if (tempeventcount >= maxhistogramvalue)
                {
                    maxhistogramvalue = tempeventcount;
                }
                if (currentbucket_upperlimit + mindrn > maxdrn) { break; }
                listofhistogramvalues.Add(tempeventcount);
                listofhistogrambucketrange.Add((Math.Round(mindrn + currentbucket_upperlimit - histogram_valuewidth)).ToString() + "-" + (Math.Round(mindrn + currentbucket_upperlimit).ToString()));

            }
            tempPOS = 0;
            histogramgraphicwidth = dep.Width / (listofhistogramvalues.Count);



            for (j = 0; j <= listofhistogramvalues.Count - 1; j++)
            {
                tempPOS = tempPOS + histogramgraphicwidth + 0.5;

                if (barseparate == true)
                {
                    GenerateRectangleUI(dep, "histogram" + j, (dep.Height - 40) * (listofhistogramvalues[j] / maxhistogramvalue), histogramgraphicwidth, tempPOS, dep.Height - 20, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 180, -1, 1, "No. of events: " + Math.Round(listofhistogramvalues[j]).ToString() + " in " + listofhistogrambucketrange[j] + " min.");
                    temprect = (Rectangle)getMenuItem_Rectangle_fromitemindex(dep, -1, "histogram" + j);
                    if (histogramgraphicwidth >= 10)
                    {
                        GenerateLabelUI(dep, "histogramdatalabel" + j, 20, 20, (double)temprect.GetValue(Canvas.LeftProperty) - temprect.Width / 2 - 10, (dep.Height - 20 - temprect.Height - 20), null, BrushColors.mybrushfontgray, 8, null, null, null, -1, Math.Round(listofhistogramvalues[j]).ToString());
                    }
                    if (histogramgraphicwidth >= 20)
                    {
                        GenerateLabelUI(dep, "histogramrangelabel" + j, 10, 30, (double)temprect.GetValue(Canvas.LeftProperty) - temprect.Width / 2 - 15, dep.Height - 20, null, BrushColors.mybrushSelectedCriteria, 7, null, null, null, -1, listofhistogrambucketrange[j]);


                    }
                }
                //   AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, temprect);
                //   System.Windows.Forms.Application.DoEvents();
                //Thread.Sleep(10);

            }



        }

        public void xSigma_Planned_VarianceCharts_Generate()
        {
            xSigma_Planned_Variance_ClearChart();

            Canvas dep = xSigma_Planned_Variance_GraphicsCanvas;
            Canvas tempcanvas_main;
            Canvas tempcanvas_chart = null;
            Canvas tempcanvas_variancebar = null;
            Canvas tempcanvas_distribution = null;

            SolidColorBrush headercolor = new SolidColorBrush(Color.FromRgb(170, 170, 170));
            Rectangle temprect = null;
            Label templabel;
            double itemheight = 50;
            double itemverticalgap = 2;
            double gapbetweenFMlabel_chartcanvas = 10;
            double gapbetweenchartcanvas_variancebarcanvas = 50;
            double gapbetweenvariancebarcanvas_OEE = 50;
            double gapbetweenOEE_Mean = 30;
            double widthofchartcanvas;

            double variancedatalabelwidth = 20;
            double OEElosslabelwidth = 30;
            double MeanDurnlabelwidth = 30;
            double minmaxellipsewidth = 15;
            double minmaxconnnectorwidth = 1;
            double lossnamelabelwidth = 150;
            double itemwidth = dep.Width;
            double actuallossvalue = 0;
            double chartoverallmindrn = intermediate.xSigma_Planned_getOverallMin();
            double chartoverallmaxdrn = intermediate.xSigma_Planned_getOverallMax();
            double chartinstamin = 0;
            double chartinstamax = 0;
            double minmaxchartwidth = 600;
            double minBubbleLeftPos = 0;
            double maxBubbleLeftPos = 0;
            double connectorlength = 0;
            double variancebarcanvaswidth = 100;
            double variancebarwidth = 0;
            double variancebarmaxwidth = 0;
            double deltaiconheight = 0.8 * itemheight;
            double deltaiconwidth = deltaiconheight;
            double labelleftposoffset = 10;
            double firstitemoffset = 40;
            double actualbarwidth = 0;
            int i;
            int j;
            string deltaimagefilename = "";


            for (i = 0; i < intermediate.xSigma_Planned_RawStopValues.Count; i++)
            {
                //Main Canvas Item
                GenerateCanvasUI(dep, "xSigma_Planned_VarianceItem" + i, itemheight, itemwidth, 0, firstitemoffset + itemverticalgap + (i * itemheight));
                tempcanvas_main = getMenuItem_Canvas_fromitemindex(dep, -1, "", "xSigma_Planned_VarianceItem" + i);
                tempcanvas_main.Background = Brushes.White;
                tempcanvas_main.MouseDown += xSigma_Planned_Variance_PlannedItemClicked;
                tempcanvas_main.Cursor = Cursors.Hand;

                GenerateRectangleUI(tempcanvas_main, "xSigma_Planned_ItemTopBorder" + i, 0.3, tempcanvas_main.Width, 0, 0, BrushColors.mybrushNOTSelectedCriteria, null, 0, null, null, null);
                if (i == intermediate.xSigma_Planned_RawStopValues.Count - 1)
                {
                    GenerateRectangleUI(tempcanvas_main, "xSigma_Planned_ItemBottomBorder" + i, 0.3, tempcanvas_main.Width, 0, itemheight, BrushColors.mybrushNOTSelectedCriteria, null, 0, null, null, null);
                }

                //Main Failure Mode name label
                GenerateLabelUI(tempcanvas_main, "xSigma_Planned_Variance_FMname" + i, itemheight, lossnamelabelwidth, labelleftposoffset, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, intermediate.xSigma_Planned_AnalysisPeriodReport.DT_Report.MappedDirectory_Planned[i].Name, true);
                templabel = getMenuItem_Label_fromitemindex(tempcanvas_main, -1, "", "xSigma_Planned_Variance_FMname" + i);
                templabel.ToolTip = templabel.Content.ToString();


                //ChartCanvas 
                GenerateCanvasUI(tempcanvas_main, "xSigma_Planned_MinMaxChart" + i, itemheight, minmaxchartwidth, (double)templabel.GetValue(Canvas.LeftProperty) + lossnamelabelwidth + gapbetweenFMlabel_chartcanvas, 0);
                tempcanvas_chart = getMenuItem_Canvas_fromitemindex(tempcanvas_main, -1, "", "xSigma_Planned_MinMaxChart" + i);
                GenerateRectangleUI(tempcanvas_chart, "xSigma_Planned_MinMaxChart_minBar" + i, itemheight, 1, 0, 0, Brushes.LightGray, null, 0, null, null, null);
                GenerateRectangleUI(tempcanvas_chart, "xSigma_Planned_MinMaxChart_maxBar" + i, itemheight, 1, tempcanvas_chart.Width, 0, Brushes.LightGray, null, 0, null, null, null);


                minBubbleLeftPos = ((intermediate.xSigma_Planned_RawStopValues[i].Min() - chartoverallmindrn) / (chartoverallmaxdrn - chartoverallmindrn)) * tempcanvas_chart.Width;
                maxBubbleLeftPos = ((intermediate.xSigma_Planned_RawStopValues[i].Max() - chartoverallmindrn) / (chartoverallmaxdrn - chartoverallmindrn)) * tempcanvas_chart.Width;
                connectorlength = (maxBubbleLeftPos - minBubbleLeftPos);

                //min bubble in chart canvas
                GenerateEllipseUI(tempcanvas_chart, "xSigma_Planned_MinBubble" + i, minmaxellipsewidth, minmaxellipsewidth, minBubbleLeftPos - minmaxellipsewidth / 2, tempcanvas_chart.Height / 2 - minmaxellipsewidth / 2, BrushColors.mybrushbrightorange, null, 0, null, null, null, 0, 2, 1, Math.Round(intermediate.xSigma_Planned_RawStopValues[i].Min(), 1).ToString() + " minutes");
                //max bubble in chart canvas
                GenerateEllipseUI(tempcanvas_chart, "xSigma_Planned_MaxBubble" + i, minmaxellipsewidth, minmaxellipsewidth, maxBubbleLeftPos - minmaxellipsewidth / 2, tempcanvas_chart.Height / 2 - minmaxellipsewidth / 2, Brushes.DarkBlue, null, 0, null, null, null, 0, 2, 1, Math.Round(intermediate.xSigma_Planned_RawStopValues[i].Max(), 1).ToString() + " minutes");
                //connector line in chart canvas
                GenerateRectangleUI(tempcanvas_chart, "xSigma_Planned_connectorline" + i, 1.3, connectorlength, minBubbleLeftPos + minmaxellipsewidth / 2, tempcanvas_chart.Height / 2, Brushes.Gray, null, 0, null, null, null, 0, -2);


                //all events dots and distribution bars
                intermediate.xS_OnTarget_FailureModeSelected(i);
                GenerateCanvasUI(tempcanvas_chart, "xSigma_Planned_DistributionCanvas" + i, itemheight, Math.Max(0, connectorlength - minmaxellipsewidth), minBubbleLeftPos + minmaxellipsewidth / 2, 0, 1);
                tempcanvas_distribution = getMenuItem_Canvas_fromitemindex(tempcanvas_chart, -1, "", "xSigma_Planned_DistributionCanvas" + i);

                OnTarget_CreateBellCurveDots(tempcanvas_distribution, true);

                GenerateCanvasUI(tempcanvas_chart, "xSigma_Planned_DistributionCanvas_Aux" + i, itemheight * 3, Math.Max(0, connectorlength - minmaxellipsewidth), minBubbleLeftPos + minmaxellipsewidth / 2, itemheight, 1);
                tempcanvas_distribution = getMenuItem_Canvas_fromitemindex(tempcanvas_chart, -1, "", "xSigma_Planned_DistributionCanvas_Aux" + i);
                OnTarget_CreateBellCurveDots(tempcanvas_distribution, false, true);
                tempcanvas_distribution.Visibility = Visibility.Hidden;


                //Variance bar canvas
                GenerateCanvasUI(tempcanvas_main, "xSigma_Planned_VarianceBarCanvas" + i, itemheight, variancebarcanvaswidth, (double)tempcanvas_chart.GetValue(Canvas.LeftProperty) + tempcanvas_chart.Width + gapbetweenchartcanvas_variancebarcanvas, 0);
                tempcanvas_variancebar = getMenuItem_Canvas_fromitemindex(tempcanvas_main, -1, "", "xSigma_Planned_VarianceBarCanvas" + i);
                GenerateRectangleUI(tempcanvas_variancebar, "xSigma_Planned_VarianceBarVerticalBase" + i, 0.8 * itemheight, 1.5, 0, 0.1 * itemheight, Brushes.DarkGray, null, 0, null, null, null);

                //horizontal bar in variance bar canvas
                variancebarwidth = (intermediate.xSigma_Planned_Variations[i] / intermediate.xSigma_Planned_Variations.Max()) * tempcanvas_variancebar.Width;
                GenerateRectangleUI(tempcanvas_variancebar, "xSigma_Planned_VarianceBar" + i, 0.6 * tempcanvas_variancebar.Height, variancebarwidth, 0, 0.2 * tempcanvas_variancebar.Height, Brushes.MediumPurple, null, 0, null, null, null);
                temprect = getMenuItem_Rectangle_fromitemindex(tempcanvas_variancebar, -1, "", "xSigma_Planned_VarianceBar" + i);
                GenerateLabelUI(tempcanvas_variancebar, "xSigma_Planned_VarianceBar_DataLabel" + i, itemheight, variancedatalabelwidth, (double)temprect.GetValue(Canvas.LeftProperty) + temprect.Width + 5, 0, null, BrushColors.mybrushfontgray, 9, null, null, null, -1, Math.Round(intermediate.xSigma_Planned_Variations[i]).ToString(), true);

                //OEE loss
                GenerateLabelUI(tempcanvas_main, "xSigma_Planned_OEEloss" + i, itemheight, OEElosslabelwidth, (double)tempcanvas_variancebar.GetValue(Canvas.LeftProperty) + tempcanvas_variancebar.Width + gapbetweenvariancebarcanvas_OEE, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, Math.Round(intermediate.xSigma_Planned_AnalysisPeriodReport.MappedDirector_Planned_DTpct(i) * 100, 1) + "%", true);

                //Average Duration
                GenerateLabelUI(tempcanvas_main, "xSigma_Planned_MeanDuration" + i, itemheight, MeanDurnlabelwidth, (double)tempcanvas_variancebar.GetValue(Canvas.LeftProperty) + tempcanvas_variancebar.Width + gapbetweenvariancebarcanvas_OEE + OEElosslabelwidth + gapbetweenOEE_Mean, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, Math.Round(intermediate.xSigma_Planned_RawStopValues[i].Mean(), 1).ToString(), true);

                //Setting the actual height of canvas based on the number of contents, so that scrolling can work properly
                dep.Height = firstitemoffset + (itemheight - itemverticalgap) + (i * (itemheight + itemverticalgap));





            }
            GenerateLabelUI(dep, "xSigma_Planned_MinDur", 30, 50, (double)tempcanvas_chart.GetValue(Canvas.LeftProperty) - 20, 0, null, headercolor, 11, null, null, null, 2, Math.Round(chartoverallmindrn).ToString() + " min", true);
            GenerateLabelUI(dep, "xSigma_Planned_MaxDur", 30, 50, (double)tempcanvas_chart.GetValue(Canvas.LeftProperty) + tempcanvas_chart.Width - 20, 0, null, headercolor, 11, null, null, null, 2, Math.Round(chartoverallmaxdrn).ToString() + " min", true);
            GenerateLabelUI(dep, "xSigma_Planned_FMLabelheader", 30, 150, labelleftposoffset, 0, null, headercolor, 11, null, null, null, -1, "Planned Event", true);
            GenerateLabelUI(dep, "xSigma_Planned_Varianceheader", 30, 150, (double)tempcanvas_variancebar.GetValue(Canvas.LeftProperty), 0, null, headercolor, 11, null, null, null, -1, "Variance", true);
            GenerateLabelUI(dep, "xSigma_Planned_OEElossheader", 30, 50, (double)tempcanvas_variancebar.GetValue(Canvas.LeftProperty) + tempcanvas_variancebar.Width + gapbetweenvariancebarcanvas_OEE, 0, null, headercolor, 11, null, null, null, -1, "Jobs Loss", true);
            GenerateLabelUI(dep, "xSigma_Planned_MeanDuration", 30, 80, (double)tempcanvas_variancebar.GetValue(Canvas.LeftProperty) + tempcanvas_variancebar.Width + gapbetweenvariancebarcanvas_OEE + OEElosslabelwidth + gapbetweenOEE_Mean, 0, null, headercolor, 11, null, null, null, -1, "Average (min)", true);

        }
        public void xSigma_Planned_Variance_ClearChart()
        {
            Canvas dep = TrendsStepChange_FMGraphicsCanvas;
            Canvas cvs;
            Label lbl;
            Rectangle rect;
            Image img;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cvs = (Canvas)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(cvs);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(rect);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(lbl);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Image") > -1)
                {
                    img = (Image)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(img);

                }
            }
        }

        public void xSigma_Planned_Variance_PlannedItemClicked(object sender, MouseButtonEventArgs e)
        {
            Canvas tempsender = (Canvas)sender;
            Canvas dep = xSigma_Planned_Variance_GraphicsCanvas;
            Canvas tempcanvasmain = null;
            Canvas tempcanvas = null;
            Canvas tempcanvas_chart = null;
            Canvas tempcanvas_distribution = null;

            int itemno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            int i;
            double itemheight = 50;
            double verticaloffset = 3 * itemheight;

            tempcanvasmain = getMenuItem_Canvas_fromitemindex(dep, -1, "", "xSigma_Planned_VarianceItem" + itemno);


            tempcanvas_chart = getMenuItem_Canvas_fromitemindex(tempcanvasmain, -1, "", "xSigma_Planned_MinMaxChart" + itemno);
            tempcanvas_distribution = getMenuItem_Canvas_fromitemindex(tempcanvas_chart, -1, "", "xSigma_Planned_DistributionCanvas_Aux" + itemno);
            tempcanvas_distribution.Visibility = Visibility.Visible;

            for (i = 0; i < intermediate.xSigma_Planned_AnalysisPeriodReport.DT_Report.MappedDirectory_Planned.Count; i++)
            {
                if (i > itemno)
                {
                    tempcanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "xSigma_Planned_VarianceItem" + i);
                    if (tempcanvasmain.Height == itemheight)
                    {
                        AnimateZoomUIElement((double)tempcanvas.GetValue(Canvas.TopProperty), (double)tempcanvas.GetValue(Canvas.TopProperty) + verticaloffset, 0.15, Canvas.TopProperty, tempcanvas);
                        // tempcanvas.SetValue(Canvas.TopProperty, (double)tempcanvas.GetValue(Canvas.TopProperty) + verticaloffset);
                    }
                    else
                    {
                        AnimateZoomUIElement((double)tempcanvas.GetValue(Canvas.TopProperty), (double)tempcanvas.GetValue(Canvas.TopProperty) - verticaloffset, 0.1, Canvas.TopProperty, tempcanvas);
                        //tempcanvas.SetValue(Canvas.TopProperty, (double)tempcanvas.GetValue(Canvas.TopProperty) - verticaloffset);
                    }
                }
            }
            if (tempcanvasmain.Height == itemheight)
            {
                tempcanvasmain.Height = 4 * itemheight;
                tempcanvas_distribution.Visibility = Visibility.Visible;
                dep.Height = dep.Height + verticaloffset;
            }
            else
            {
                tempcanvasmain.Height = itemheight;
                tempcanvas_distribution.Visibility = Visibility.Hidden;
                dep.Height = dep.Height - verticaloffset;
            }



        }
        #endregion


        #endregion


        public void SetupBubbleCharts(Canvas dep, Double Canvaswidth = 587, int barnumberclicked = -1)
        {
            //Hide first
            List<double> Xvalues = new List<double>();
            List<double> Yvalues = new List<double>();
            List<String> lossnames = new List<string>();
            List<double> Sizevalues = new List<double>();
            List<SolidColorBrush> Colorvalues = new List<SolidColorBrush>();

            int numberoflosses = intermediate.xSigma_Daily_Names.Count;//intermediate.xSigma_Daily_NumberOfLosses;
            double maxXvalue = intermediate.xSigma_Daily_Xaxis_Max;
            double maxYvalue = intermediate.xSigma_Daily_Yaxis_Max;
            double maxsizevalue = intermediate.xSigma_Daily_Size_Max;
            int maxbubblesize = 65;

            //Color for stability
            Colorvalues.Add(Brushes.YellowGreen);
            Colorvalues.Add(Brushes.Orange);
            Colorvalues.Add(Brushes.OrangeRed);
            Colorvalues.Add(Brushes.DarkRed);


            //Legend creation
            Canvas tempcanvas;
            int i;
            if (dep == DailyincontrolGraphicsArea)
            {
                if (dep.Children.Count == 0)  // checking if already the legend is created.
                {
                    for (i = 0; i <= 3; i++)   // generating 4 rectangles, and assigning a color from Colorvalues list
                    {
                        GenerateRectangleUI(Dailyincontrol_StabilityCanvas, "Legend" + i, Dailyincontrol_StabilityCanvas.Height, Dailyincontrol_StabilityCanvas.Width / 4, i * Dailyincontrol_StabilityCanvas.Width / 4, 0, Colorvalues[i], null, 0, null, null, null);
                    }
                }
            }
            else
            {
                if (dep.Children.Count == 0)  // checking if already the legend is created.
                {
                    for (i = 0; i <= 3; i++)   // generating 4 rectangles, and assigning a color from Colorvalues list
                    {
                        GenerateRectangleUI(Ontarget_gapCanvas, "Legend" + i, Dailyincontrol_StabilityCanvas.Height, Dailyincontrol_StabilityCanvas.Width / 4, i * Dailyincontrol_StabilityCanvas.Width / 4, 0, Colorvalues[i], null, 0, null, null, null);
                    }
                }
            }



            //Downloading intermediate sheet values to local lists.

            int j = 0;
            int m;
            Ellipse elp;
            numberoflosses = intermediate.xSigma_Daily_Xaxis_Values.Count; //SRO - added for CS Integration - 1/28/16
            for (j = 0; j < numberoflosses; j++)
            {

                Xvalues.Add(intermediate.xSigma_Daily_Xaxis_Values[j]);
                Yvalues.Add(intermediate.xSigma_Daily_Yaxis_Values[j]);
                Sizevalues.Add(intermediate.xSigma_Daily_Size_Values[j]);

                lossnames.Add("Lossbubble");

            }
            //CS Score is XAxis
            //Stops values is Y asis
            //PRvalues is bubble size
            if (dep == DailyincontrolGraphicsArea)  // checks if Planned or Unplanned canvas was clicked
            {
                //Actual creation of bubbles
                createbubblegraphs(true, dep, numberoflosses, Xvalues, Sizevalues, Yvalues, lossnames, maxbubblesize, maxXvalue, maxsizevalue, maxYvalue, Colorvalues, CS_Bubbles_Clicked, CS_Bubbles_Move, CS_Bubbles_Leave, "CSbubble", Canvaswidth);

                //Animation stuff for bubbles to pop up gradually
                for (m = 0; m <= VisualTreeHelper.GetChildrenCount(dep) - 1; m++)
                {
                    if (VisualTreeHelper.GetChild(dep, m).GetType().ToString().IndexOf("Ellipse") > -1)
                    {
                        elp = (Ellipse)VisualTreeHelper.GetChild(dep, m);
                        AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, elp);
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(2);
                    }

                }

                //Setting up the header for bubble chart
                if (barnumberclicked != -1)
                {
                    DateTime tempdate;
                    tempdate = intermediate.xSigma_Trend_Dates[barnumberclicked];
                    string tempselecteddate;

                    tempselecteddate = tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture) + " " + tempdate.ToString("yyyy", CultureInfo.InvariantCulture);

                    Dailyincontrol_Header.Content = "Chronic and sporadic job activity on " + tempselecteddate;
                }

                //Programtically clicking on the first bubble in the freshly generated chart.
                CS_Bubbles_Clicked(getMenuItem_Ellipse_fromitemindex(DailyincontrolGraphicsArea, -1, "", "CSbubble1"), Publics.f);

            }
            else if (false)   //for Planned (OnTarget)
            {
                //Actual creation of bubbles
                createbubblegraphs(true, dep, numberoflosses, Xvalues, Sizevalues, Yvalues, lossnames, maxbubblesize, maxXvalue, maxsizevalue, maxYvalue, Colorvalues, OnTarget_Bubbles_Clicked, OnTarget_Bubbles_Move, OnTarget_Bubbles_Leave, "OnTargetbubble", Canvaswidth);

                //Animation stuff for bubbles to pop up gradually
                for (m = 0; m <= VisualTreeHelper.GetChildrenCount(dep) - 1; m++)
                {
                    if (VisualTreeHelper.GetChild(dep, m).GetType().ToString().IndexOf("Ellipse") > -1)
                    {
                        elp = (Ellipse)VisualTreeHelper.GetChild(dep, m);
                        AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, elp);
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(2);
                    }

                }

            }




        }

        #endregion


        #region Trends
        public void ToggleShowHide_Trends(object sender, MouseButtonEventArgs e)
        {
            if (TrendsCanvas.Visibility != Visibility.Visible)
            {
                HideAllDashboards();
                TrendsCanvas.Visibility = Visibility.Visible;
                Trends_onload();

            }
        }
        public void TrendsGoBack(object sender, MouseButtonEventArgs e)
        {
            HideAllDashboards();
            LossCompassCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.2, 1.0, 0.5, OpacityProperty, LossCompassCanvas);
            TrendsBackBtn.Visibility = Visibility.Hidden;
            TrendsBackLabel.Visibility = Visibility.Hidden;
        }
        public void LaunchTrendsfromLossCompass(object sender, MouseButtonEventArgs e)
        {
            CardTier tempcardname = CardTier.NA;
            if (ActiveToolTip_Cardname == "A") { tempcardname = CardTier.A; }
            if (ActiveToolTip_Cardname == "B") { tempcardname = CardTier.B; }
            if (ActiveToolTip_Cardname == "C") { tempcardname = CardTier.C; }
            ActiveToolTip_Card = tempcardname;
            populateLossTrendsfromLossCompass(intermediate.LossCompass_GetMapping_A(tempcardname), Tooltip_failuremodenamelabel.Content.ToString());
            HideAllDashboards();
            TrendsCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.2, 1.0, 0.7, OpacityProperty, TrendsCanvas);
        }
        public void Trends_onload(Canvas defaultcanvasview = null, bool initiateLists = true)
        {
            LossTrendsClicked(LossTrendsHeaderLabel, Publics.f);
            CloseKPI_Grid(KPI_grid_button, Publics.f);
            KPI_grid_Canvas.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, TrendsCanvas);
            TrendsBackBtn.Visibility = Visibility.Hidden;
            TrendsBackLabel.Visibility = Visibility.Hidden;
            List<DowntimeMetrics> listofkpi = new List<DowntimeMetrics>();

            //OEE is the default trend line shown for Line Trends
            listofkpi.Add(DowntimeMetrics.OEE);
            Trends_CreateLegends(LineTrendsLegendCanvas, 100, listofkpi); // Sam to replace this with KPI click

            LineTrends_CharttypetoggleON.Visibility = Visibility.Hidden;
            LineTrends_CharttypetoggleOFF.Visibility = Visibility.Visible;
            LossTrends_CharttypetoggleON.Visibility = Visibility.Hidden;
            LossTrends_CharttypetoggleOFF.Visibility = Visibility.Visible;

            if (initiateLists == true)
            {

                //Loss Trends combobox and failure list initialization
                PopulateTrendsMappingCombo();
                Trends_Mode_unplannedClicked(Trends_Mode_unplannedLossArea_Btn, Publics.f);

                //STEP CHANGE combobox and failure list initialization
                PopulateTrendsSTEPCHANGEMappingCombo();
                PopulateTrends_STEPCHANGE_Failuremodelistbox();

                TrendsFailuremodeListbox_unplanned.SelectedIndex = 0;

                if (getMenuItem_Canvas_fromitemindex(TrendsStepChange_FMGraphicsCanvas, -1, "", "TrendsSTEPCHANGE_FMitem0") != null)
                {
                    TrendsSTEPCHANGE_FMList_MouseDown(getMenuItem_Canvas_fromitemindex(TrendsStepChange_FMGraphicsCanvas, -1, "", "TrendsSTEPCHANGE_FMitem0"), Publics.f);
                }
            }


            //Auto Complete Lists initialization for LOSS Trends and STEPCHANGE
            Trends_Mode_PopulateAutoCompleteList();
            TrendsSTEPCHANGE_PopulateAutoCompleteList();


            if (ListofSelectedKPI_LineTrends.Count < 1)
            {
                ListofSelectedKPI_LineTrends.Add(DowntimeMetrics.OEE);
                LineTrends_UpdateChartFromIntermediateSheet();
                Griditem1.Opacity = 1.0;
            }
            if (ListofSelectedFailureModeTrends_unplanned.Count < 1)
            {
                ListofSelectedFailureModeTrends_unplanned.Add(TrendsFailuremodeListbox_unplanned.SelectedItem.ToString());
            }
            if (ListofSelectedFailureModeTrends_planned.Count < 1 & TrendsFailuremodeListbox_planned.SelectedItem != null)
            {
                ListofSelectedFailureModeTrends_planned.Add(TrendsFailuremodeListbox_planned.SelectedItem.ToString());
            }
            if (ListofSelectedKPI_ModeTrends.Count < 1)
            {
                ListofSelectedKPI_ModeTrends.Add(DowntimeMetrics.DTpct);

                ModeTrends_UpdateChartFromIntermediateSheet();
                Mode_Griditem1.Opacity = 1.0;
            }
            if (ListofSelectedKPI_StepTrends.Count < 1)
            {
                ListofSelectedKPI_StepTrends.Add(DowntimeMetrics.DTpct);
                Trends_Step_SelectedFailureModes_Unplanned.Add(intermediate.Trends_Mode_Names_Unplanned[0]);
                StepChange_UpdateChartFromIntermediateSheet();
                Mode_Griditem1.Opacity = 1.0;
            }



            if (LineTrendsTimeframe_Combo.Items.Count < 1)
            {
                LineTrendsTimeframe_Combo.Items.Add("Daily");
                LineTrendsTimeframe_Combo.Items.Add("Weekly");
                LineTrendsTimeframe_Combo.Items.Add("Monthly");
                LineTrendsTimeframe_Combo.SelectedValue = "Weekly";
            }
            if (LineTrends_Mode_Timeframe_Combo.Items.Count < 1)
            {
                LineTrends_Mode_Timeframe_Combo.Items.Add("Daily");
                LineTrends_Mode_Timeframe_Combo.Items.Add("Weekly");
                LineTrends_Mode_Timeframe_Combo.Items.Add("Monthly");
                LineTrends_Mode_Timeframe_Combo.SelectedValue = "Weekly";
            }
            if (LineTrends_Step_Timeframe_Combo.Items.Count < 1)
            {
                LineTrends_Step_Timeframe_Combo.Items.Add("Daily");
                LineTrends_Step_Timeframe_Combo.Items.Add("Weekly");
                LineTrends_Step_Timeframe_Combo.SelectedValue = "Daily";
            }







            //DT% is the default trend line shown for Failure Mode Loss Trends
            TrendsMode_PrepareLegendsList();

        }

        #region LineTrends

        public void LossTrendsClicked(object sender, MouseButtonEventArgs e)
        {
            LossTrendsCanvas.Visibility = Visibility.Visible;
            LossTrendsModeCanvas.Visibility = Visibility.Hidden;
            StepChangeCanvas.Visibility = Visibility.Hidden;
            TrendsSelectionBar1.Visibility = Visibility.Visible;
            TrendsSelectionBar2.Visibility = Visibility.Hidden;
            TrendsSelectionBar3.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 95, 0.2, WidthProperty, TrendsSelectionBar1);

        }
        public void LossTrendsModeClicked(object sender, MouseButtonEventArgs e)
        {
            LossTrendsModeCanvas.Visibility = Visibility.Visible;
            LossTrendsCanvas.Visibility = Visibility.Hidden;
            StepChangeCanvas.Visibility = Visibility.Hidden;
            TrendsSelectionBar2.Visibility = Visibility.Visible;
            TrendsSelectionBar1.Visibility = Visibility.Hidden;
            TrendsSelectionBar3.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 95, 0.2, WidthProperty, TrendsSelectionBar2);

        }
        public void StepChangeClicked(object sender, MouseButtonEventArgs e)
        {
            StepChangeCanvas.Visibility = Visibility.Visible;
            LossTrendsModeCanvas.Visibility = Visibility.Hidden;
            LossTrendsCanvas.Visibility = Visibility.Hidden;
            TrendsSelectionBar3.Visibility = Visibility.Visible;
            TrendsSelectionBar1.Visibility = Visibility.Hidden;
            TrendsSelectionBar2.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 100, 0.2, WidthProperty, TrendsSelectionBar3);
        }

        public void LaunchKPIgrid(object sender, MouseButtonEventArgs e)
        {
            KPI_grid_Canvas.Visibility = Visibility.Visible;
            KPI_grid_button.Visibility = Visibility.Hidden;
            KPI_grid_label.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 504, 0.2, WidthProperty, KPI_grid_Canvas);
            AnimateZoomUIElement(0, 504, 0.2, HeightProperty, KPI_grid_Canvas);
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, KPI_grid_Canvas);

        }
        public void CloseKPI_Grid(object sender, MouseButtonEventArgs e)
        {
            AnimateZoomUIElement(504, 0, 0.2, WidthProperty, KPI_grid_Canvas);
            AnimateZoomUIElement(504, 0, 0.2, HeightProperty, KPI_grid_Canvas);
            KPI_grid_Canvas.Visibility = Visibility.Hidden;
            KPI_grid_button.Visibility = Visibility.Visible;
            KPI_grid_label.Visibility = Visibility.Visible;
        }
        public void KPIMousemove(object sender, MouseEventArgs e)
        {
            Image tempsender = (Image)sender;

        }


        public void KPIselected(object sender, MouseButtonEventArgs e)
        {
            DowntimeMetrics kpinumber;
            var tempsender = (Image)sender;
            kpinumber = Trends_Line_getMetricFromInt_new(Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString())));

            if (tempsender.Opacity < 1.0)
            {
                tempsender.Opacity = 1.0;
                ListofSelectedKPI_LineTrends.Add(kpinumber);
            }
            else
            {
                tempsender.Opacity = 0.3;
                ListofSelectedKPI_LineTrends.Remove(kpinumber);
            }
            LineTrends_UpdateChartFromIntermediateSheet();
            TrendsLineTrends_PrepareLegendsList();

        }
        public void TrendsLineTrends_PrepareLegendsList()
        {
            int i;
            int j;
            int k;

            // first clear the list
            if (ListofLineTrendsLegends.Count > 0)
            {
                ListofLineTrendsLegends.Clear();
            }



            for (j = 0; j <= ListofSelectedKPI_LineTrends.Count - 1; j++)  //loop through kpi list
            {
                if (intermediate.Multi_CurrentLineNames.Count > 1)  // multi line, hence we append line name to the legend
                {
                    ListofLineTrendsLegends.Add(ListofSelectedKPI_LineTrends[j].ToString() + "-" + "Multiline Roll-up");

                    for (k = 0; k <= intermediate.Multi_CurrentLineNames.Count - 1; k++)
                    {
                        ListofLineTrendsLegends.Add(ListofSelectedKPI_LineTrends[j].ToString() + "-" + intermediate.Multi_CurrentLineNames[k].ToString());

                    }

                }
                else   // just one line, hence no need to put line name in the legend
                {
                    ListofLineTrendsLegends.Add(ListofSelectedKPI_LineTrends[j].ToString());
                }

            }

            Trends_CreateLegends(LineTrendsLegendCanvas, 100, null, ListofLineTrendsLegends);



        }
        private DowntimeMetrics Trends_Line_getMetricFromInt_new(int Metric)
        {
            switch (Metric)
            {
                case 1:
                    return DowntimeMetrics.OEE;
                case 2:
                    return DowntimeMetrics.Stops;
                case 3:
                    return DowntimeMetrics.UPDTpct;
                case 4:
                    return DowntimeMetrics.PDTpct;
                case 5:
                    return DowntimeMetrics.UnitsProduced;
                case 6:
                    return DowntimeMetrics.SPD;
                case 7:
                    return DowntimeMetrics.SKUs;
                case 8:
                    return DowntimeMetrics.MTBF;
                case 9:
                    return DowntimeMetrics.NumChangeovers;
                default:
                    return DowntimeMetrics.NA;
            }
        }
        private DowntimeMetrics Trends_Line_getMetricFromInt(int Metric)
        {
            switch (Metric)
            {
                case 1:
                    return DowntimeMetrics.OEE;
                case 2:
                    return DowntimeMetrics.Stops;
                case 3:
                    return DowntimeMetrics.UPDTpct;
                case 4:
                    return DowntimeMetrics.PDTpct;
                case 5:
                    return DowntimeMetrics.UnitsProduced;
                case 6:
                    return DowntimeMetrics.SPD;
                case 7:
                    return DowntimeMetrics.SKUs;
                case 8:
                    return DowntimeMetrics.MTBF;
                case 9:
                    return DowntimeMetrics.NumChangeovers;
                default:
                    return DowntimeMetrics.NA;
            }
        }
        private DowntimeMetrics Trends_Mode_getMetricFromInt(int Metric)
        {
            switch (Metric)
            {
                case 1:
                    return DowntimeMetrics.DTpct;
                case 2:
                    return DowntimeMetrics.SPD;
                case 3:
                    return DowntimeMetrics.MTBF;
                case 4:
                    return DowntimeMetrics.Stops;
                case 5:
                    return DowntimeMetrics.MTTR;
                case 6:
                    return DowntimeMetrics.DT;
                case 7:
                    return DowntimeMetrics.DT;
                default:
                    return DowntimeMetrics.NA;
            }
        }


        public void HideGrid(object sender, MouseEventArgs e)
        {
            AnimateZoomUIElement(504, 0, 0.2, WidthProperty, KPI_grid_Canvas);
            AnimateZoomUIElement(504, 0, 0.2, HeightProperty, KPI_grid_Canvas);
            KPI_grid_Canvas.Visibility = Visibility.Hidden;
        }
        public void LineTrends_TimeFrameSelected(object sender, RoutedEventArgs e)
        {

            LineTrendsTimeframe_Combo.Visibility = Visibility.Hidden;
            LineTrendsTimeframe_Label.Content = LineTrendsTimeframe_Combo.SelectedItem.ToString();
            if (LineTrendsTimeframe_Label.Content.ToString() == "Daily")
            {
                LineTrends_analysistimeperiod = 1;
            }
            else if (LineTrendsTimeframe_Label.Content.ToString() == "Weekly")
            {
                LineTrends_analysistimeperiod = 7;
            }
            else if (LineTrendsTimeframe_Label.Content.ToString() == "Monthly")
            {
                LineTrends_analysistimeperiod = 30;
            }
            LineTrends_UpdateChartFromIntermediateSheet();
        }



        public void LineTrends_Timeframepicker_Clicked(object sender, MouseButtonEventArgs e)
        {
            LineTrendsTimeframe_Combo.Visibility = Visibility.Visible;
            LineTrendsTimeframe_Combo.IsDropDownOpen = true;
        }

        public void GlidepathONOFFToggleClicked(object sender, MouseButtonEventArgs e)
        {

            int temptogglepos = ToggleNow(GlidepathONOFFToggleframe, GlidepathONOFFToggleball);  // this is the function that does the toggle and returns the final position of the ball after the toggle
            if (temptogglepos == 0) // Zero means Toggle Ball is on the Left 
            {
                GlidepathOFFlabel.Foreground = BrushColors.mybrushSelectedCriteria;
                GlidepathONlabel.Foreground = BrushColors.mybrushLIGHTGRAY;
                Glidepath_Turnoff();

            }
            else if (temptogglepos == 1)  // One means Toggle Ball is on the right.
            {
                GlidepathOFFlabel.Foreground = BrushColors.mybrushLIGHTGRAY;
                GlidepathONlabel.Foreground = BrushColors.mybrushSelectedCriteria;
                Glidepath_Turnon();
            }

        }
        public void GlidepathNotesONOFFToggleClicked(object sender, MouseButtonEventArgs e)
        {

            int temptogglepos = ToggleNow(GlidepathNotesONOFFToggleframe, GlidepathNotesONOFFToggleball);  // this is the function that does the toggle and returns the final position of the ball after the toggle
            if (temptogglepos == 0) // Zero means Toggle Ball is on the Left 
            {
                GlidepathNotesOFFlabel.Foreground = BrushColors.mybrushSelectedCriteria;
                GlidepathNotesONlabel.Foreground = BrushColors.mybrushLIGHTGRAY;
                GlidepathNotes_Turnoff();

            }
            else if (temptogglepos == 1)  // One means Toggle Ball is on the right.
            {
                GlidepathNotesOFFlabel.Foreground = BrushColors.mybrushLIGHTGRAY;
                GlidepathNotesONlabel.Foreground = BrushColors.mybrushSelectedCriteria;
                GlidepathNotes_Turnon();
            }

        }
        public void Glidepath_Turnon()
        {
            ISGlidepathOn = true;
            AnimateZoomUIElement(60, 201, 0.3, HeightProperty, GlidepathCanvasBorder);
            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(30);
            GlidepathSecondaryCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0, 1.0, 1.5, OpacityProperty, GlidepathSecondaryCanvas);
            /*Glidepathfileselectionlabel.Visibility = Visibility.Visible;
            GlidepathFileSelectionCombobox.Visibility = Visibility.Visible;
            GlidepathSavebutton.Visibility = Visibility.Visible;
            GlidepathSaveLabel.Visibility = Visibility.Visible;
            GlidepathDeletebutton.Visibility = Visibility.Visible;
            GlidepathDeleteLabel.Visibility = Visibility.Visible;
            GlidepathCanvasSeparator1.Visibility = Visibility.Visible;
            HideGlidepathButton.Visibility = Visibility.Visible;
            GlidepathNotesLabel.Visibility = Visibility.Visible;
            GlidepathNotesONOFFToggleCanvas.Visibility = Visibility.Visible;*/

            Glidepath_UpdateFileList();

            intermediate.Trends_Glidepath_TurnOn();
        }
        private const string ACTIVE_GLIDEPATH_NAME = "Active Sim";
        private void Glidepath_UpdateFileList()
        {
            List<string> FileNames = getAllFileNamesinFolder(Globals.HTML.PATH_FORK_GLIDEPATH);
            FileNames.Insert(0, ACTIVE_GLIDEPATH_NAME);
            GlidepathFileSelectionCombobox.ItemsSource = FileNames;
            GlidepathFileSelectionCombobox.SelectedIndex = 0;
            LineTrends_AddGlidePath();
        }
        public void Glidepath_LoadSelectedFile(object sender, RoutedEventArgs e)
        {
            string SelectedName = GlidepathFileSelectionCombobox.Items[GlidepathFileSelectionCombobox.SelectedIndex].ToString();
            if (SelectedName.Length > 1 && !SelectedName.Contains(" "))
            {
                intermediate.Trends_Glidepath_SetCurrent(SelectedName);
                LineTrends_RemoveGlidePath();
                LineTrends_AddGlidePath();
            }
            else if (GlidepathFileSelectionCombobox.SelectedIndex == 0)
            {
                intermediate.Trends_Glidepath_SetCurrentSimAsActive();
                LineTrends_RemoveGlidePath();
                LineTrends_AddGlidePath();
            }
        }
        public void Glidepath_Turnoff()
        {
            ISGlidepathOn = false;
            AnimateZoomUIElement(201, 60, 0.3, HeightProperty, GlidepathCanvasBorder);
            System.Windows.Forms.Application.DoEvents();
            AnimateZoomUIElement(1.0, 0, 0.2, OpacityProperty, GlidepathSecondaryCanvas);
            /*Glidepathfileselectionlabel.Visibility = Visibility.Hidden;
            GlidepathFileSelectionCombobox.Visibility = Visibility.Hidden;
            GlidepathSavebutton.Visibility = Visibility.Hidden;
            GlidepathSaveLabel.Visibility = Visibility.Hidden;
            GlidepathDeletebutton.Visibility = Visibility.Hidden;
            GlidepathDeleteLabel.Visibility = Visibility.Hidden;
            GlidepathCanvasSeparator1.Visibility = Visibility.Hidden;
            HideGlidepathButton.Visibility = Visibility.Hidden;
            GlidepathNotesLabel.Visibility = Visibility.Hidden;
            GlidepathNotesONOFFToggleCanvas.Visibility = Visibility.Hidden;*/


            intermediate.Trends_Glidepath_TurnOff();
            LineTrends_RemoveGlidePath();
        }

        public void HideGlidepath(object sender, MouseButtonEventArgs e)
        {
            AnimateZoomUIElement(201, 60, 0.3, HeightProperty, GlidepathCanvasBorder);

            AnimateZoomUIElement(1.0, 0, 0.2, OpacityProperty, GlidepathSecondaryCanvas);
        }
        public void ShowGlidepath(object sender, MouseButtonEventArgs e)
        {
            ShowGlidepathButton.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(60, 201, 0.3, HeightProperty, GlidepathCanvasBorder);
            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(30);
            GlidepathSecondaryCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0, 1.0, 1.5, OpacityProperty, GlidepathSecondaryCanvas);

        }
        public void Glidepathmousemove(object sender, MouseEventArgs e)
        {
            if (ISGlidepathOn == true)
            {
                if (GlidepathSecondaryCanvas.Opacity == 0 || GlidepathSecondaryCanvas.Visibility == Visibility.Hidden)
                {
                    ShowGlidepathButton.Visibility = Visibility.Visible;
                }
            }
        }

        public void Glidepathmouseleave(object sender, MouseEventArgs e)
        {
            ShowGlidepathButton.Visibility = Visibility.Hidden;

        }
        public void GlidepathNotes_Turnon()
        { }
        public void GlidepathNotes_Turnoff()
        { }
        public void Glidepath_DeleteSelectedFile(object sender, MouseButtonEventArgs e)
        {
            if (GlidepathFileSelectionCombobox.SelectedIndex > 0)
            {
                string FileName = Globals.HTML.PATH_FORK_GLIDEPATH + GlidepathFileSelectionCombobox.SelectedItem;
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                else
                {
                    MessageBox.Show("Error Deleting Glidepath");
                }
                Glidepath_UpdateFileList();
            }
        }
        public void Glidepath_SaveAsSelectedFile(object sender, MouseButtonEventArgs e)
        {
            string message; string title; string defaultValue;
            string myValue;
            //' Set prompt.
            message = "Enter file name to save";
            //' Set title.
            title = "Tmp File Name";
            defaultValue = "";  //' Set default value.

            //' Display message, title, and default value.

            myValue = Interaction.InputBox(message, title, defaultValue);
            //' If user has clicked Cancel, set myValue to defaultValue
            if (myValue != "")
            {
                intermediate.Trends_Glidepath_SaveAsCurrent(myValue);
                Glidepath_UpdateFileList();
                for (int i = 0; i < GlidepathFileSelectionCombobox.Items.Count; i++)
                {
                    if (GlidepathFileSelectionCombobox.Items[i].ToString().Contains(myValue)) { GlidepathFileSelectionCombobox.SelectedIndex = i; }
                }
            }
            else
            {
                MessageBox.Show("Invalid File Name");
            }
        }

        public void Trends_CreateLegends(Canvas dep, double labelwidth = 100, List<ForkAnalyticsSettings.GlobalConstants.DowntimeMetrics> ListofLegends_dtmetrics = null, List<string> ListofLegends_string = null)
        {
            Trends_ClearLegends(dep);
            double legendbubblewidth = 18;
            double bubbleandlabelgap = 2;

            double labelheight = 18;
            double rowgap = 10;
            Ellipse Tempbubble;
            int numberoflegends = 0;
            if (ListofLegends_dtmetrics == null)
            {
                numberoflegends = ListofLegends_string.Count;
            }
            else
            {
                numberoflegends = ListofLegends_dtmetrics.Count;
            }

            int i = 0;
            int j = 0; //row number
            int k = 0; //col number
            int maxlegendsinarow = 8;

            for (i = 0; i < numberoflegends; i++)
            {
                if (ListofLegends_dtmetrics == null)
                {
                    // for stepchange and Losstrends because it returns string list 
                    GenerateEllipseUI(dep, "LegendBubble" + i, legendbubblewidth, legendbubblewidth, k * (legendbubblewidth + bubbleandlabelgap + labelwidth), j * (labelheight + rowgap), Trends_GetColors(i), null, 0, null, null, null);
                    Tempbubble = getMenuItem_Ellipse_fromitemindex(dep, -1, "", "LegendBubble" + i);
                    GenerateLabelUI(dep, "LegendLabel" + i, labelheight, labelwidth, (double)Tempbubble.GetValue(Canvas.LeftProperty) + bubbleandlabelgap + (legendbubblewidth), j * (labelheight + rowgap), null, BrushColors.mybrushdarkgray, 10, null, null, null, -1, ListofLegends_string[i], true);

                }
                else
                {
                    // for line trends because it takes KPI
                    GenerateEllipseUI(dep, "LegendBubble" + i, legendbubblewidth, legendbubblewidth, k * (legendbubblewidth + bubbleandlabelgap + labelwidth), j * (labelheight + rowgap), Trends_GetColors(i), null, 0, null, null, null);
                    Tempbubble = getMenuItem_Ellipse_fromitemindex(dep, -1, "", "LegendBubble" + i);
                    GenerateLabelUI(dep, "LegendLabel" + i, labelheight, labelwidth, (double)Tempbubble.GetValue(Canvas.LeftProperty) + bubbleandlabelgap + (legendbubblewidth), j * (labelheight + rowgap), null, BrushColors.mybrushdarkgray, 10, null, null, null, -1, getStringForEnum_Metric(ListofLegends_dtmetrics[i]), true);
                }
                if (((k + 1) * (legendbubblewidth + bubbleandlabelgap + labelwidth + 30) >= dep.Width)) { j++; k = 0; }
                else { k++; }

            }



        }

        public void Trends_ClearLegends(Canvas dep)
        {
            Label lbl;
            Ellipse elp;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(lbl);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Ellipse") > -1)
                {
                    elp = (Ellipse)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(elp);

                }

            }
        }

        public SolidColorBrush Trends_GetKPILegendColors(ForkAnalyticsSettings.GlobalConstants.DowntimeMetrics KPIindex)
        {
            switch (KPIindex)
            {
                case (DowntimeMetrics.OEE):
                    return new SolidColorBrush(Color.FromRgb(98, 210, 143));
                case (DowntimeMetrics.PDTpct):
                    return new SolidColorBrush(Color.FromRgb(55, 217, 185));
                case (DowntimeMetrics.MTBF):
                    return new SolidColorBrush(Color.FromRgb(249, 92, 154));
                case (DowntimeMetrics.SPD):
                    return new SolidColorBrush(Color.FromRgb(182, 136, 253));
                case (DowntimeMetrics.Stops):
                    return new SolidColorBrush(Color.FromRgb(219, 187, 157));
                case (DowntimeMetrics.UnitsProduced):
                    return new SolidColorBrush(Color.FromRgb(221, 137, 170));
                case (DowntimeMetrics.NumChangeovers):
                    return new SolidColorBrush(Color.FromRgb(213, 108, 108));

            }
            return BrushColors.mybrushSelectedCriteria;
        }

        public SolidColorBrush Trends_GetColors(int colorindex)
        {
            colorindex = colorindex + 1;
            if (colorindex % 10 == 1)
            {
                return new SolidColorBrush(Color.FromRgb(50, 205, 240));
            }
            if (colorindex % 10 == 2)
            {
                return new SolidColorBrush(Color.FromRgb(254, 118, 58));
            }
            if (colorindex % 10 == 3)
            {
                return new SolidColorBrush(Color.FromRgb(153, 192, 73));
            }
            if (colorindex % 10 == 4)
            {
                return new SolidColorBrush(Color.FromRgb(1, 149, 159));
            }
            if (colorindex % 10 == 5)
            {
                return new SolidColorBrush(Color.FromRgb(115, 127, 65));

            }
            if (colorindex % 10 == 6)
            {
                return new SolidColorBrush(Color.FromRgb(119, 199, 198));
            }
            if (colorindex % 10 == 7)
            {
                return new SolidColorBrush(Color.FromRgb(189, 171, 210));
            }
            if (colorindex % 10 == 8)
            {
                return new SolidColorBrush(Color.FromRgb(255, 175, 2));
            }
            if (colorindex % 10 == 9)
            {
                return new SolidColorBrush(Color.FromRgb(150, 76, 143));
            }
            if (colorindex % 10 == 0)
            {
                return new SolidColorBrush(Color.FromRgb(18, 135, 170));
            }



            return BrushColors.mybrushSelectedCriteria;
        }

        public void LineTrends_ChangeChartType(object sender, MouseButtonEventArgs e)
        {
            if (LineTrends_CharttypetoggleON.Visibility == Visibility.Visible)
            {
                LineTrends_CharttypetoggleOFF.Visibility = Visibility.Visible;
                LineTrends_CharttypetoggleON.Visibility = Visibility.Hidden;
                LineTrends_isLineGraph = true;
                LineTrends_UpdateChartFromIntermediateSheet();

            }
            else
            {
                LineTrends_CharttypetoggleOFF.Visibility = Visibility.Hidden;
                LineTrends_CharttypetoggleON.Visibility = Visibility.Visible;
                LineTrends_isLineGraph = false;
                LineTrends_UpdateChartFromIntermediateSheet();
            }

        }


        #endregion

        #region ModeTrends

        public void LaunchmodeKPIgrid(object sender, MouseButtonEventArgs e)
        {
            Mode_KPI_grid_Canvas.Visibility = Visibility.Visible;
            Mode_KPI_grid_button.Visibility = Visibility.Hidden;
            Mode_KPI_grid_label.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 504, 0.2, WidthProperty, Mode_KPI_grid_Canvas);
            AnimateZoomUIElement(0, 504, 0.2, HeightProperty, Mode_KPI_grid_Canvas);
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, Mode_KPI_grid_Canvas);
        }
        public void CloseMode_KPI_grid(object sender, MouseButtonEventArgs e)
        {
            AnimateZoomUIElement(504, 0, 0.2, WidthProperty, Mode_KPI_grid_Canvas);
            AnimateZoomUIElement(504, 0, 0.2, HeightProperty, Mode_KPI_grid_Canvas);
            Mode_KPI_grid_Canvas.Visibility = Visibility.Hidden;
            Mode_KPI_grid_button.Visibility = Visibility.Visible;
            Mode_KPI_grid_label.Visibility = Visibility.Visible;
        }

        public void ModeKPISelected(object sender, MouseButtonEventArgs e)
        {
            DowntimeMetrics kpinumber;
            var tempsender = (Image)sender;
            kpinumber = Trends_Mode_getMetricFromInt(Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString())));
            if (tempsender.Opacity < 1.0)
            {
                tempsender.Opacity = 1.0;
                ListofSelectedKPI_ModeTrends.Add(kpinumber);
            }
            else
            {
                tempsender.Opacity = 0.3;
                ListofSelectedKPI_ModeTrends.Remove(kpinumber);
            }
            //   intermediate.Trends_Mode_generateKPIchart(LineTrends_Mode_analysistimeperiod, ListofSelectedKPI_ModeTrends, ListofSelectedFailureModeTrends_unplanned, ListofSelectedFailureModeTrends_planned);//NEEDS TO BE CHANGED FOR PLANNED LIST
            ModeTrends_UpdateChartFromIntermediateSheet();
            TrendsMode_PrepareLegendsList();
        }

        public void ModeHideGrid(object sender, MouseEventArgs e)
        {
            AnimateZoomUIElement(504, 0, 0.2, WidthProperty, Mode_KPI_grid_Canvas);
            AnimateZoomUIElement(504, 0, 0.2, HeightProperty, Mode_KPI_grid_Canvas);
            Mode_KPI_grid_Canvas.Visibility = Visibility.Hidden;
        }


        public void TrendsMode_PrepareLegendsList()
        {
            int i;
            int j;
            int k;

            // first clear the list
            if (ListofLossTrendsLegends.Count > 0)
            {
                ListofLossTrendsLegends.Clear();
            }


            //populate legend list
            for (i = 0; i <= ListofSelectedKPI_ModeTrends.Count - 1; i++) //loop through all KPIs
            {
                //unplanned first
                if (ListofSelectedFailureModeTrends_unplanned != null) // check if there any items in unplanned list
                {
                    for (j = 0; j <= ListofSelectedFailureModeTrends_unplanned.Count - 1; j++)  //loop through unplanned fm list
                    {
                        if (intermediate.Multi_CurrentLineNames.Count > 1)  // multi line, hence we append line name to the legend
                        {
                            ListofLossTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[i]) + "-" + ListofSelectedFailureModeTrends_unplanned[j].ToString() + "-" + "Multiline Roll-up");

                            for (k = 0; k <= intermediate.Multi_CurrentLineNames.Count - 1; k++)
                            {
                                ListofLossTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[i]) + "-" + ListofSelectedFailureModeTrends_unplanned[j].ToString() + "-" + intermediate.Multi_CurrentLineNames[k].ToString());

                            }

                        }
                        else   // just one line, hence no need to put line name in the legend
                        {
                            ListofLossTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[i]) + "-" + ListofSelectedFailureModeTrends_unplanned[j].ToString());
                        }
                    }
                }

                // planned second
                if (ListofSelectedFailureModeTrends_planned != null)  // check if there any items in unplanned list
                {
                    for (j = 0; j <= ListofSelectedFailureModeTrends_planned.Count - 1; j++) // loop through all planned events in the list
                    {
                        if (intermediate.Multi_CurrentLineNames.Count > 1)  // multi line, hence we append line name to the legend
                        {
                            ListofLossTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[i]) + "-" + ListofSelectedFailureModeTrends_planned[j].ToString() + "-" + "Multiline Roll-up");

                            for (k = 0; k <= intermediate.Multi_CurrentLineNames.Count - 1; k++)
                            {
                                ListofLossTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[i]) + "-" + ListofSelectedFailureModeTrends_planned[j].ToString() + "-" + intermediate.Multi_CurrentLineNames[k].ToString());

                            }

                        }
                        else   // just one line, hence no need to put line name in the legend
                        {
                            ListofLossTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[i]) + "-" + ListofSelectedFailureModeTrends_planned[j].ToString());
                        }
                    }
                }


            }


            //
            if (intermediate.Multi_CurrentLineNames.Count > 1)
            {
                Trends_CreateLegends(TrendsFailureMode_LegendCanvas, 220, null, ListofLossTrendsLegends);
            }
            else
            {
                Trends_CreateLegends(TrendsFailureMode_LegendCanvas, 150, null, ListofLossTrendsLegends);

            }
        }

        public void populateLossTrendsfromLossCompass(string originmappingfield, string failuremodename)
        {
            Trends_onload();

            //  intermediate.Trends_Mode_Remap(getEnumForString(originmappingfield), DowntimeField.NA);
            PopulateTrends_Failuremodelistbox_Unplanned();
            PopulateTrends_Failuremodelistbox_planned();
            LossTrendsModeClicked(LossTrendsModeHeaderLabel, Publics.f);
            TrendsBackBtn.Visibility = Visibility.Visible;
            TrendsBackLabel.Visibility = Visibility.Visible;
            try
            {
                TrendsFailuremodeListbox_unplanned.SelectedItem = failuremodename;
            }
            catch (Exception ex)
            { }

            try
            {
                TrendsFailuremodeListbox_planned.SelectedItem = failuremodename;
            }
            catch (Exception ex)
            { }
        }


        public void PopulateTrendsMappingCombo()
        {
            Trendsmappingcombobox.ItemsSource = getStringListForEnumList(intermediate.LossCompass_getMappingFieldList_Helper(CardTier.A, true));
            Trendsmappingcombobox.SelectedItem = getStringForEnum(intermediate.Trends_Mode_MappingA);
        }
        public void TrendsMappingComboSelected(object sender, RoutedEventArgs e)
        {
            intermediate.Trends_Mode_Remap(getEnumForString(Trendsmappingcombobox.SelectedItem.ToString()), DowntimeField.NA);
            PopulateTrends_Failuremodelistbox_Unplanned();
            PopulateTrends_Failuremodelistbox_planned();

        }
        public void PopulateTrends_Failuremodelistbox_Unplanned()
        {
            TrendsFailuremodeListbox_unplanned.ItemsSource = null;
            TrendsFailuremodeListbox_unplanned.ItemsSource = intermediate.Trends_Mode_Names_Unplanned;
        }
        public void PopulateTrends_Failuremodelistbox_planned()
        {
            TrendsFailuremodeListbox_planned.ItemsSource = null;
            TrendsFailuremodeListbox_planned.ItemsSource = intermediate.Trends_Mode_Names_Planned;
        }
        public void TrendsFailureModeSelected(object sender, RoutedEventArgs e)
        {
            //First clear the list
            ListofSelectedFailureModeTrends_unplanned.Clear();
            ListofSelectedFailureModeTrends_planned.Clear();

            //populate the unplanned list from listbox selecteditems
            for (int i = 0; i < TrendsFailuremodeListbox_unplanned.SelectedItems.Count; i++)
            {
                ListofSelectedFailureModeTrends_unplanned.Add(TrendsFailuremodeListbox_unplanned.SelectedItems[i].ToString());
            }

            //populate the planned list from listbox selecteditems
            for (int i = 0; i < TrendsFailuremodeListbox_planned.SelectedItems.Count; i++)
            {
                ListofSelectedFailureModeTrends_planned.Add(TrendsFailuremodeListbox_planned.SelectedItems[i].ToString());
            }

            //Generate charts
            //  intermediate.Trends_Mode_generateKPIchart(LineTrends_Mode_analysistimeperiod, ListofSelectedKPI_ModeTrends, ListofSelectedFailureModeTrends_unplanned, ListofSelectedFailureModeTrends_planned);//needs to be swapped out for planned
            ModeTrends_UpdateChartFromIntermediateSheet();
            TrendsMode_PrepareLegendsList();
        }
        public void Trends_Mode_unplannedClicked(object sender, MouseButtonEventArgs e)
        {
            Trends_Mode_unplannedLossArea_Btn.Background = BrushColors.mybrushSelectedCriteria;
            Trends_Mode_plannedLossArea_Btn.Background = BrushColors.mybrushLIGHTGRAY;
            TrendsFailuremodeListbox_unplanned.Visibility = Visibility.Visible;
            TrendsFailuremodeListbox_planned.Visibility = Visibility.Hidden;
            Trends_Mode_PopulateAutoCompleteList();
        }
        public void Trends_Mode_plannedClicked(object sender, MouseButtonEventArgs e)
        {
            Trends_Mode_unplannedLossArea_Btn.Background = BrushColors.mybrushLIGHTGRAY;
            Trends_Mode_plannedLossArea_Btn.Background = BrushColors.mybrushSelectedCriteria;
            TrendsFailuremodeListbox_unplanned.Visibility = Visibility.Hidden;
            TrendsFailuremodeListbox_planned.Visibility = Visibility.Visible;
            Trends_Mode_PopulateAutoCompleteList_planned();
        }
        public void LineTrends_Mode_TimeFrameSelected(object sender, RoutedEventArgs e)
        {

            LineTrends_Mode_Timeframe_Combo.Visibility = Visibility.Hidden;
            LineTrends_Mode_Timeframe_Label.Content = LineTrends_Mode_Timeframe_Combo.SelectedItem.ToString();
            if (LineTrends_Mode_Timeframe_Label.Content.ToString() == "Daily")
            {
                LineTrends_Mode_analysistimeperiod = 1;
            }
            else if (LineTrends_Mode_Timeframe_Label.Content.ToString() == "Weekly")
            {
                LineTrends_Mode_analysistimeperiod = 7;
            }
            else if (LineTrends_Mode_Timeframe_Label.Content.ToString() == "Monthly")
            {
                LineTrends_Mode_analysistimeperiod = 30;
            }

            //   intermediate.Trends_Mode_generateKPIchart(LineTrends_Mode_analysistimeperiod, ListofSelectedKPI_ModeTrends, ListofSelectedFailureModeTrends_unplanned, ListofSelectedFailureModeTrends_planned);//needs to be swapped for planned
            ModeTrends_UpdateChartFromIntermediateSheet();
        }
        public void LineTrends_Mode_Timeframepicker_Clicked(object sender, MouseButtonEventArgs e)
        {
            LineTrends_Mode_Timeframe_Combo.Visibility = Visibility.Visible;
            LineTrends_Mode_Timeframe_Combo.IsDropDownOpen = true;



        }

        public void Trends_MODE_AutoCompleteBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            List<int> TempselecteditemsinListBox = new List<int>();
            int i;
            if (Trends_Mode_AutoCompleteBox.SelectedItem != null)
            {
                if (Trends_Mode_unplannedLossArea_Btn.Background == BrushColors.mybrushSelectedCriteria)
                {
                    if (intermediate.Trends_Mode_Names_Unplanned.IndexOf(Trends_Mode_AutoCompleteBox.SelectedItem.ToString()) != -1)
                    {

                        TrendsFailuremodeListbox_unplanned.SelectedIndex = intermediate.Trends_Mode_Names_Unplanned.IndexOf(Trends_Mode_AutoCompleteBox.SelectedItem.ToString());
                    }
                }

                if (Trends_Mode_plannedLossArea_Btn.Background == BrushColors.mybrushSelectedCriteria)
                {
                    if (intermediate.Trends_Mode_Names_Planned.IndexOf(Trends_Mode_AutoCompleteBox.SelectedItem.ToString()) != -1)
                    {
                        TrendsFailuremodeListbox_planned.SelectedIndex = intermediate.Trends_Mode_Names_Planned.IndexOf(Trends_Mode_AutoCompleteBox.SelectedItem.ToString());
                    }
                }
            }

        }
        private ObservableCollection<String> _Trends_Mode_failureslist = new ObservableCollection<string>();
        public void Trends_Mode_PopulateAutoCompleteList()
        {
            Trends_Mode_AutoCompleteBox.SearchText = "";
            _Trends_Mode_failureslist.Clear();
            //TrendsSTEPCHANGEfailuresList
            List<string> _FMlist = new List<string>();
            int j;
            _FMlist = intermediate.Trends_Mode_Names_Unplanned;
            for (j = 0; j <= _FMlist.Count - 1; j++)
            {

                _Trends_Mode_failureslist.Add(_FMlist[j]);
            }
            Trends_Mode_AutoCompleteBox.ItemsSource = _FMlist;
        }
        public void Trends_Mode_PopulateAutoCompleteList_planned()
        {
            Trends_Mode_AutoCompleteBox.SearchText = "";
            _Trends_Mode_failureslist.Clear();
            //TrendsSTEPCHANGEfailuresList
            List<string> _FMlist = new List<string>();
            int j;
            _FMlist = intermediate.Trends_Mode_Names_Planned;
            for (j = 0; j <= _FMlist.Count - 1; j++)
            {

                _Trends_Mode_failureslist.Add(_FMlist[j]);
            }
            Trends_Mode_AutoCompleteBox.ItemsSource = _FMlist;
        }

        public void LossTrends_ChangeChartType(object sender, MouseButtonEventArgs e)
        {
            if (LossTrends_CharttypetoggleON.Visibility == Visibility.Visible)
            {
                LossTrends_CharttypetoggleOFF.Visibility = Visibility.Visible;
                LossTrends_CharttypetoggleON.Visibility = Visibility.Hidden;
                LossTrends_isLineGraph = true;

                ModeTrends_UpdateChartFromIntermediateSheet();

            }
            else
            {
                LossTrends_CharttypetoggleOFF.Visibility = Visibility.Hidden;
                LossTrends_CharttypetoggleON.Visibility = Visibility.Visible;
                LossTrends_isLineGraph = false;
                ModeTrends_UpdateChartFromIntermediateSheet();
            }

        }

        #endregion

        #region StepChange


        public void LaunchstepKPIgrid(object sender, MouseButtonEventArgs e)
        {
            Step_KPI_grid_Canvas.Visibility = Visibility.Visible;
            Step_KPI_grid_button.Visibility = Visibility.Hidden;
            Step_KPI_grid_label.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 504, 0.2, WidthProperty, Step_KPI_grid_Canvas);
            AnimateZoomUIElement(0, 504, 0.2, HeightProperty, Step_KPI_grid_Canvas);
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, Step_KPI_grid_Canvas);
        }
        public void CloseStep_KPI_grid(object sender, MouseButtonEventArgs e)
        {
            AnimateZoomUIElement(504, 0, 0.2, WidthProperty, Step_KPI_grid_Canvas);
            AnimateZoomUIElement(504, 0, 0.2, HeightProperty, Step_KPI_grid_Canvas);
            Step_KPI_grid_Canvas.Visibility = Visibility.Hidden;
            Step_KPI_grid_button.Visibility = Visibility.Visible;
            Step_KPI_grid_label.Visibility = Visibility.Visible;
        }

        public void StepKPISelected(object sender, MouseButtonEventArgs e)
        {
            DowntimeMetrics kpinumber;
            var tempsender = (Image)sender;
            kpinumber = Trends_Mode_getMetricFromInt(Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString())));

            //set selected KPI
            if (tempsender.Opacity < 1.0)
            {
                //reset everything
                Step_Griditem1.Opacity = 0.3;
                Step_Griditem2.Opacity = 0.3;
                Step_Griditem3.Opacity = 0.3;
                Step_Griditem4.Opacity = 0.3;
                Step_Griditem5.Opacity = 0.3;
                Step_Griditem6.Opacity = 0.3;
                ListofSelectedKPI_StepTrends.Clear(); //only one at a time

                //set current
                tempsender.Opacity = 1.0;
                ListofSelectedKPI_StepTrends.Add(kpinumber);
            }
            PopulateTrends_STEPCHANGE_Failuremodelistbox();
            StepChange_UpdateChartFromIntermediateSheet();
            //this still needs to be ported TrendsMode_PrepareLegendsList();
        }

        public void Trends_Step_unplannedClicked(object sender, MouseButtonEventArgs e)
        {
            Trends_Step_unplannedLossArea_Btn.Background = BrushColors.mybrushSelectedCriteria;
            Trends_Step_plannedLossArea_Btn.Background = BrushColors.mybrushLIGHTGRAY;
            TrendsStepChange_FailureModeSelection_ScrollView.Visibility = Visibility.Visible;
            TrendsStepChange_FailureModeSelection_ScrollView_Planned.Visibility = Visibility.Hidden;
        }
        public void Trends_Step_plannedClicked(object sender, MouseButtonEventArgs e)
        {
            Trends_Step_unplannedLossArea_Btn.Background = BrushColors.mybrushLIGHTGRAY;
            Trends_Step_plannedLossArea_Btn.Background = BrushColors.mybrushSelectedCriteria;
            TrendsStepChange_FailureModeSelection_ScrollView.Visibility = Visibility.Hidden;
            TrendsStepChange_FailureModeSelection_ScrollView_Planned.Visibility = Visibility.Visible;
        }
        public void LineTrends_Step_TimeFrameSelected(object sender, RoutedEventArgs e)
        {

            LineTrends_Step_Timeframe_Combo.Visibility = Visibility.Hidden;
            LineTrends_Step_Timeframe_Label.Content = LineTrends_Step_Timeframe_Combo.SelectedItem.ToString();
            if (LineTrends_Step_Timeframe_Label.Content.ToString() == "Daily")
            {
                LineTrends_Step_analysistimeperiod = 1;
            }
            else if (LineTrends_Mode_Timeframe_Label.Content.ToString() == "Weekly")
            {
                LineTrends_Step_analysistimeperiod = 7;
            }
            StepChange_UpdateChartFromIntermediateSheet();
        }
        public void LineTrends_Step_Timeframepicker_Clicked(object sender, MouseButtonEventArgs e)
        {
            LineTrends_Step_Timeframe_Combo.Visibility = Visibility.Visible;
            LineTrends_Step_Timeframe_Combo.IsDropDownOpen = true;
        }


        public void StepTrends_ChangeChartType(object sender, MouseButtonEventArgs e)
        {
            if (StepTrends_CharttypetoggleON.Visibility == Visibility.Visible)
            {
                StepTrends_CharttypetoggleOFF.Visibility = Visibility.Visible;
                StepTrends_CharttypetoggleON.Visibility = Visibility.Hidden;
                StepTrends_isLineGraph = true;
            }
            else
            {
                StepTrends_CharttypetoggleOFF.Visibility = Visibility.Hidden;
                StepTrends_CharttypetoggleON.Visibility = Visibility.Visible;
                StepTrends_isLineGraph = false;
            }
            StepChange_UpdateChartFromIntermediateSheet();
        }

















        public void TrendsSTEPCHANGEMappingComboSelected(object sender, RoutedEventArgs e)
        {
            Trendsmappingcombobox.SelectedIndex = TrendsSTEPCHANGEmappingcombobox.SelectedIndex;

            intermediate.Trends_Step_Remap(getEnumForString(TrendsSTEPCHANGEmappingcombobox.SelectedItem.ToString()), DowntimeField.NA);
            PopulateTrends_STEPCHANGE_Failuremodelistbox();
        }

        public void PopulateTrends_STEPCHANGE_Failuremodelistbox()
        {
            if (ListofSelectedKPI_StepTrends.Count == 0) { ListofSelectedKPI_StepTrends.Add(DowntimeMetrics.DTpct); }

            TrendsSTEPCHANGE_CreateFMListonCanvas();
            TrendsSTEPCHANGE_CreateFMListonCanvas_Planned();

        }


        public void TrendsStepChange_PrepareLegendsList()
        {
            int i;
            int j;
            int k;

            // first clear the list
            if (ListofStepChangeTrendsLegends.Count > 0)
            {
                ListofStepChangeTrendsLegends.Clear();
            }


            //populate legend list
            for (i = 0; i <= ListofSelectedKPI_StepTrends.Count - 1; i++) //loop through all KPIs
            {
                //unplanned first
                if (Trends_Step_SelectedFailureModes_Unplanned != null) // check if there any items in unplanned list
                {
                    for (j = 0; j <= Trends_Step_SelectedFailureModes_Unplanned.Count - 1; j++)  //loop through unplanned fm list
                    {
                        if (intermediate.Multi_CurrentLineNames.Count > 1)  // multi line, hence we append line name to the legend
                        {
                            ListofStepChangeTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_StepTrends[i]) + "-" + Trends_Step_SelectedFailureModes_Unplanned[j].ToString() + "-" + "Multiline Roll-up");

                            for (k = 0; k <= intermediate.Multi_CurrentLineNames.Count - 1; k++)
                            {
                                ListofStepChangeTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_StepTrends[i]) + "-" + Trends_Step_SelectedFailureModes_Unplanned[j].ToString() + "-" + intermediate.Multi_CurrentLineNames[k].ToString());

                            }

                        }
                        else   // just one line, hence no need to put line name in the legend
                        {
                            ListofStepChangeTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_StepTrends[i]) + "-" + Trends_Step_SelectedFailureModes_Unplanned[j].ToString());
                        }
                    }
                }

                // planned second
                if (Trends_Step_SelectedFailureModes_Planned != null)  // check if there any items in unplanned list
                {
                    for (j = 0; j <= Trends_Step_SelectedFailureModes_Planned.Count - 1; j++) // loop through all planned events in the list
                    {
                        if (intermediate.Multi_CurrentLineNames.Count > 1)  // multi line, hence we append line name to the legend
                        {
                            ListofStepChangeTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_StepTrends[i]) + "-" + Trends_Step_SelectedFailureModes_Planned[j].ToString() + "-" + "Multiline Roll-up");

                            for (k = 0; k <= intermediate.Multi_CurrentLineNames.Count - 1; k++)
                            {
                                ListofStepChangeTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_StepTrends[i]) + "-" + Trends_Step_SelectedFailureModes_Planned[j].ToString() + "-" + intermediate.Multi_CurrentLineNames[k].ToString());

                            }

                        }
                        else   // just one line, hence no need to put line name in the legend
                        {
                            ListofStepChangeTrendsLegends.Add(getStringForEnum_Metric(ListofSelectedKPI_StepTrends[i]) + "-" + Trends_Step_SelectedFailureModes_Planned[j].ToString());
                        }
                    }
                }


            }


            //
            if (intermediate.Multi_CurrentLineNames.Count > 1)
            {
                Trends_CreateLegends(TrendsSTEPCHANGE_LegendCanvas, 220, null, ListofStepChangeTrendsLegends);
            }
            else
            {
                Trends_CreateLegends(TrendsSTEPCHANGE_LegendCanvas, 150, null, ListofStepChangeTrendsLegends);

            }
        }


        public void PopulateTrendsSTEPCHANGEMappingCombo()
        {

            TrendsSTEPCHANGEmappingcombobox.ItemsSource = getStringListForEnumList(intermediate.LossCompass_getMappingFieldList_Helper(CardTier.A, true));
            TrendsSTEPCHANGEmappingcombobox.SelectedItem = getStringForEnum(intermediate.Trends_Mode_MappingA);

        }
        public void TrendsSTEPCHANGEAutoCompleteBoxSelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        public void TrendsSTEPCHANGE_PopulateAutoCompleteList()
        {

            //TrendsSTEPCHANGEfailuresList
            /*  List<string> _FMlist = new List<string>();
            int j;
            _FMlist = intermediate.Trends_StepChange_getAllFailureModes();
            for (j = 0; j <= _FMlist.Count - 1; j++)
            {

                _TrendsSTEPCHANGEfailureslist.Add(_FMlist[j]);
            }
              TrendsSTEPCHANGEAutoCompleteBox.ItemsSource = _FMlist; */
        }


        //unplanned FM list
        public void TrendsSTEPCHANGE_CreateFMListonCanvas()
        {
            TrendsSTEPCHANGE_ClearFMLISTcanvas();

            Canvas dep = TrendsStepChange_FMGraphicsCanvas;
            Canvas tempcanvas;
            Rectangle temprect;
            Label templabel;
            double itemheight = 30;
            double itemverticalgap = 5;
            double gapbetweenlabelandDT = 10;
            double gapbetweenDTandmtbf = 30;
            double datalabelwidth = 55;
            double mtbflabelwidth = 50;
            double lossnamelabelwidth = 150;
            double itemwidth = 360;
            double actuallossvalue = 0;

            double deltaiconheight = 0.8 * itemheight;
            double deltaiconwidth = deltaiconheight;
            double labelleftposoffset = 10;

            double actualbarwidth = 0;

            string deltaimagefilename = "";

            //step 1: create a list of tuples - delta & name - for current kpi & sort by delta
            var rawDeltaList = new List<Tuple<double, string, double>>();
            int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_StepTrends[0]);
            int lineIndex = intermediate.Multi_AllSystemReports_Names.IndexOf(intermediate.Multi_CurrentLineNames[0]);
            for (int i = 0; i < intermediate.Trends_Mode_Names_Unplanned.Count; i++)
            {
                double tmpDelta = 0;

                //only do anything if we're looking at one line
                if (intermediate.Multi_CurrentLineNames.Count == 1)
                {
                    if (LineTrends_Step_analysistimeperiod == 1)
                    {
                        tmpDelta = intermediate.Trends_Step_MasterDataList_Daily_Unplanned[lineIndex][i][metricIndex][(intermediate.Trends_Step_MasterDataList_Daily_Unplanned[lineIndex][i][metricIndex].Count - 1)] - intermediate.Trends_Step_MasterDataList_Daily_Unplanned[lineIndex][i][metricIndex][0];
                    }
                    else
                    {
                        tmpDelta = intermediate.Trends_Step_MasterDataList_Weekly_Unplanned[lineIndex][i][metricIndex][(intermediate.Trends_Step_MasterDataList_Weekly_Unplanned[lineIndex][i][metricIndex].Count - 1)] - intermediate.Trends_Step_MasterDataList_Weekly_Unplanned[lineIndex][i][metricIndex][0];
                    }
                }
                rawDeltaList.Add(new Tuple<double, string, double>(Math.Abs(tmpDelta), intermediate.Trends_Mode_Names_Unplanned[i], tmpDelta));
            }
            List<Tuple<double, string, double>> result = rawDeltaList.OrderBy(x => x.Item1).ToList();
            result.Reverse();

            //step 2: add system to beginning of list
            /*
            int ix = 0;
            GenerateCanvasUI(dep, "TrendsSTEPCHANGE_FMitem" + ix, itemheight, itemwidth, 0, itemverticalgap + (ix * itemheight));

            tempcanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "TrendsSTEPCHANGE_FMitem" + ix);
            tempcanvas.MouseMove += TrendsSTEPCHANGE_FMList_Move;
            tempcanvas.MouseLeave += TrendsSTEPCHANGE_FMList_Leave;
            tempcanvas.MouseDown += TrendsSTEPCHANGE_FMList_MouseDown;
            tempcanvas.Background = Brushes.White; 
            GenerateLabelUI(tempcanvas, "TrendsSTEPCHANGE_FMlabel" + ix, itemheight, lossnamelabelwidth, labelleftposoffset, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, "System", true);
            templabel = getMenuItem_Label_fromitemindex(tempcanvas, -1, "", "TrendsSTEPCHANGE_FMlabel" + ix);
            templabel.ToolTip = templabel.Content.ToString();
            

            //DTpct
            if (LineTrends_Step_analysistimeperiod == 1)
            {
                actuallossvalue = intermediate.Trends_Step_MasterDataList_Daily[lineIndex][metricIndex][(intermediate.Trends_Step_MasterDataList_Daily[lineIndex][metricIndex].Count - 1)] - intermediate.Trends_Step_MasterDataList_Daily[lineIndex][metricIndex][0];
            }
            else
            {
                actuallossvalue = intermediate.Trends_Step_MasterDataList_Weekly[lineIndex][metricIndex][(intermediate.Trends_Step_MasterDataList_Weekly[lineIndex][metricIndex].Count - 1)] - intermediate.Trends_Step_MasterDataList_Weekly[lineIndex][metricIndex][0];
            }
           // actuallossvalue = Math.Round(result[ix].Item3, 1);

            if (actuallossvalue > 0)
            {
                deltaimagefilename = "UpStep";
            }
            else if (actuallossvalue < 0)
            {
                deltaimagefilename = "DownStep";
            }
            else
            {
                deltaimagefilename = "noStep";

            }
            GenerateImageUI(tempcanvas, "TrendsSTEPCHANGE_FM_DTpctDeltaicon" + ix, deltaiconheight, deltaiconwidth, lossnamelabelwidth + gapbetweenlabelandDT, itemheight / 2 - deltaiconheight / 2, AppDomain.CurrentDomain.BaseDirectory + @"\" + deltaimagefilename + ".png", null, null, null);
            GenerateLabelUI(tempcanvas, "TrendsSTEPCHANGE_FM_DTpctlabel" + ix, itemheight, datalabelwidth, lossnamelabelwidth + gapbetweenlabelandDT + deltaiconwidth + 2, 0, null, BrushColors.mybrushfontgray, 9, null, null, null, -1, actuallossvalue + " " + StepChange_selectedmode, true);

            dep.Height = (itemheight - itemverticalgap) + (ix * (itemheight + itemverticalgap));
            */
            //step 3: add items to canvas based on sorted list
            int j = 0; //system offset
            for (int i = 0; i < result.Count; i++)
            {
                GenerateCanvasUI(dep, "TrendsSTEPCHANGE_FMitem" + (i + j), itemheight, itemwidth, 0, itemverticalgap + ((i + j) * itemheight));

                tempcanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "TrendsSTEPCHANGE_FMitem" + (i + j));
                tempcanvas.MouseMove += TrendsSTEPCHANGE_FMList_Move;
                tempcanvas.MouseLeave += TrendsSTEPCHANGE_FMList_Leave;
                tempcanvas.MouseDown += TrendsSTEPCHANGE_FMList_MouseDown;
                tempcanvas.Background = Brushes.White; //light grey for selected?
                GenerateLabelUI(tempcanvas, "TrendsSTEPCHANGE_FMlabel" + (i + j), itemheight, lossnamelabelwidth, labelleftposoffset, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, result[i].Item2, true);
                templabel = getMenuItem_Label_fromitemindex(tempcanvas, -1, "", "TrendsSTEPCHANGE_FMlabel" + (i + j));
                templabel.ToolTip = templabel.Content.ToString();


                //DTpct
                actuallossvalue = Math.Round(result[i].Item3, 1);

                if (actuallossvalue > 0)
                {
                    deltaimagefilename = "UpDelta";
                }
                else if (actuallossvalue < 0)
                {
                    deltaimagefilename = "DownDelta";
                }
                else
                {
                    deltaimagefilename = "DownDelta";

                }
                GenerateImageUI(tempcanvas, "TrendsSTEPCHANGE_FM_DTpctDeltaicon" + (i + j), deltaiconheight, deltaiconwidth, lossnamelabelwidth + gapbetweenlabelandDT, itemheight / 2 - deltaiconheight / 2, AppDomain.CurrentDomain.BaseDirectory + @"\" + deltaimagefilename + ".png", null, null, null);
                GenerateLabelUI(tempcanvas, "TrendsSTEPCHANGE_FM_DTpctlabel" + (i + j), itemheight, datalabelwidth, lossnamelabelwidth + gapbetweenlabelandDT + deltaiconwidth + 2, 0, null, BrushColors.mybrushfontgray, 9, null, null, null, -1, actuallossvalue + " " + StepChange_selectedmode, true);

                dep.Height = (itemheight - itemverticalgap) + ((i + j) * (itemheight + itemverticalgap));
            }
        }
        public void TrendsSTEPCHANGE_ClearFMLISTcanvas()
        {
            Canvas dep = TrendsStepChange_FMGraphicsCanvas;
            Canvas cvs;
            Label lbl;
            Rectangle rect;
            Image img;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cvs = (Canvas)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(cvs);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(rect);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(lbl);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Image") > -1)
                {
                    img = (Image)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(img);

                }
            }
        }

        public void TrendsSTEPCHANGE_FMList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas tempsender = (Canvas)sender;
            int toplossno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            string lossnamesearch = intermediate.Trends_Mode_Names_Unplanned[toplossno];

            if (tempsender.Background == Brushes.White || tempsender.Background == BrushColors.mybrushLIGHTGRAY)
            {
                tempsender.Background = BrushColors.mybrushgray;
            }
            else
            {
                tempsender.Background = Brushes.White;
            }

            TrendsSTEPCHANGEFailureModeSelected(null, Publics.f);
        }
        public void TrendsSTEPCHANGE_FMList_Move(object sender, MouseEventArgs e)
        {
            Canvas tempsender = (Canvas)sender;
            tempsender.Opacity = 0.8;
            int toplossno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            string lossnamesearch = intermediate.Trends_Mode_Names_Unplanned[toplossno];
            if (tempsender.Background != BrushColors.mybrushgray)
            {
                tempsender.Background = BrushColors.mybrushLIGHTGRAY;
            }

        }
        public void TrendsSTEPCHANGE_FMList_Leave(object sender, MouseEventArgs e)
        {
            Canvas tempsender = (Canvas)sender;
            tempsender.Opacity = 1.0;
            int toplossno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            string lossnamesearch = intermediate.Trends_Mode_Names_Unplanned[toplossno];
            if (tempsender.Background != BrushColors.mybrushgray)
            {
                tempsender.Background = Brushes.White;
            }
        }

        public void TrendsSTEPCHANGEFailureModeSelected(object sender, RoutedEventArgs e)
        {
            Trends_Step_SelectedFailureModes_Unplanned.Clear();
            Canvas dep = TrendsStepChange_FMGraphicsCanvas;
            Canvas tempcanvas;
            Label templabel;
            for (int i = 0; i < intermediate.Trends_Mode_Names_Unplanned.Count; i++)
            {
                tempcanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "TrendsSTEPCHANGE_FMitem" + i);

                if (tempcanvas.Background == BrushColors.mybrushgray)
                {
                    templabel = getMenuItem_Label_fromitemindex(tempcanvas, -1, "", "TrendsSTEPCHANGE_FMlabel" + i);
                    Trends_Step_SelectedFailureModes_Unplanned.Add(templabel.Content.ToString());
                }
            }
            if (ListofSelectedKPI_StepTrends.Count > 0) //prevents errors
            {
                StepChange_UpdateChartFromIntermediateSheet();
                TrendsStepChange_PrepareLegendsList();
            }
        }

        //planned FM list
        public void TrendsSTEPCHANGE_CreateFMListonCanvas_Planned()
        {
            TrendsSTEPCHANGE_ClearFMLISTcanvas_Planned();

            Canvas dep = TrendsStepChange_FMGraphicsCanvas_Planned;
            Canvas tempcanvas;
            Rectangle temprect;
            Label templabel;
            double itemheight = 30;
            double itemverticalgap = 5;
            double gapbetweenlabelandDT = 10;
            double gapbetweenDTandmtbf = 30;
            double datalabelwidth = 55;
            double mtbflabelwidth = 50;
            double lossnamelabelwidth = 150;
            double itemwidth = 360;
            double actuallossvalue = 0;

            double deltaiconheight = 0.8 * itemheight;
            double deltaiconwidth = deltaiconheight;
            double labelleftposoffset = 10;

            double actualbarwidth = 0;

            string deltaimagefilename = "";

            //step 1: create a list of tuples - delta & name - for current kpi & sort by delta
            var rawDeltaList = new List<Tuple<double, string, double>>();
            int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_StepTrends[0]);
            int lineIndex = intermediate.Multi_AllSystemReports_Names.IndexOf(intermediate.Multi_CurrentLineNames[0]);
            for (int i = 0; i < intermediate.Trends_Mode_Names_Planned.Count; i++)
            {
                double tmpDelta = 0;

                //only do anything if we're looking at one line
                if (intermediate.Multi_CurrentLineNames.Count == 1)
                {
                    if (LineTrends_Step_analysistimeperiod == 1)
                    {
                        tmpDelta = intermediate.Trends_Step_MasterDataList_Daily_Planned[lineIndex][i][metricIndex][(intermediate.Trends_Step_MasterDataList_Daily_Planned[lineIndex][i][metricIndex].Count - 1)] - intermediate.Trends_Step_MasterDataList_Daily_Planned[lineIndex][i][metricIndex][0];
                    }
                    else
                    {
                        tmpDelta = intermediate.Trends_Step_MasterDataList_Weekly_Planned[lineIndex][i][metricIndex][(intermediate.Trends_Step_MasterDataList_Weekly_Planned[lineIndex][i][metricIndex].Count - 1)] - intermediate.Trends_Step_MasterDataList_Weekly_Planned[lineIndex][i][metricIndex][0];
                    }
                }
                rawDeltaList.Add(new Tuple<double, string, double>(Math.Abs(tmpDelta), intermediate.Trends_Mode_Names_Planned[i], tmpDelta));
            }
            List<Tuple<double, string, double>> result = rawDeltaList.OrderBy(x => x.Item1).ToList();
            result.Reverse();

            //step 2: add system to beginning of list

            //step 3: add items to canvas based on sorted list
            for (int i = 0; i < result.Count; i++)
            {
                GenerateCanvasUI(dep, "TrendsSTEPCHANGE_FMitem" + i, itemheight, itemwidth, 0, itemverticalgap + (i * itemheight));

                tempcanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "TrendsSTEPCHANGE_FMitem" + i);
                tempcanvas.MouseMove += TrendsSTEPCHANGE_FMList_Move_Planned;
                tempcanvas.MouseLeave += TrendsSTEPCHANGE_FMList_Leave_Planned;
                tempcanvas.MouseDown += TrendsSTEPCHANGE_FMList_MouseDown_Planned;
                tempcanvas.Background = Brushes.White; //light grey for selected?
                GenerateLabelUI(tempcanvas, "TrendsSTEPCHANGE_FMlabel" + i, itemheight, lossnamelabelwidth, labelleftposoffset, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, result[i].Item2, true);
                templabel = getMenuItem_Label_fromitemindex(tempcanvas, -1, "", "TrendsSTEPCHANGE_FMlabel" + i);
                templabel.ToolTip = templabel.Content.ToString();


                //DTpct
                actuallossvalue = Math.Round(result[i].Item3, 1);

                if (actuallossvalue > 0)
                {
                    deltaimagefilename = "UpDelta";
                }
                else if (actuallossvalue < 0)
                {
                    deltaimagefilename = "DownDelta";
                }
                else
                {
                    deltaimagefilename = "DownDelta";

                }
                GenerateImageUI(tempcanvas, "TrendsSTEPCHANGE_FM_DTpctDeltaicon" + i, deltaiconheight, deltaiconwidth, lossnamelabelwidth + gapbetweenlabelandDT, itemheight / 2 - deltaiconheight / 2, AppDomain.CurrentDomain.BaseDirectory + @"\" + deltaimagefilename + ".png", null, null, null);
                GenerateLabelUI(tempcanvas, "TrendsSTEPCHANGE_FM_DTpctlabel" + i, itemheight, datalabelwidth, lossnamelabelwidth + gapbetweenlabelandDT + deltaiconwidth + 2, 0, null, BrushColors.mybrushfontgray, 9, null, null, null, -1, actuallossvalue + " " + StepChange_selectedmode, true);

                dep.Height = (itemheight - itemverticalgap) + (i * (itemheight + itemverticalgap));
            }
        }
        public void TrendsSTEPCHANGE_ClearFMLISTcanvas_Planned()
        {
            Canvas dep = TrendsStepChange_FMGraphicsCanvas_Planned;
            Canvas cvs;
            Label lbl;
            Rectangle rect;
            Image img;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cvs = (Canvas)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(cvs);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(rect);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(lbl);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Image") > -1)
                {
                    img = (Image)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(img);

                }
            }
        }

        public void TrendsSTEPCHANGE_FMList_MouseDown_Planned(object sender, MouseButtonEventArgs e)
        {

            Canvas tempsender = (Canvas)sender;
            int toplossno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            string lossnamesearch = intermediate.Trends_Mode_Names_Unplanned[toplossno];

            if (tempsender.Background == Brushes.White || tempsender.Background == BrushColors.mybrushLIGHTGRAY)
            {
                tempsender.Background = BrushColors.mybrushgray;
            }
            else
            {
                tempsender.Background = Brushes.White;
            }

            TrendsSTEPCHANGEFailureModeSelected_Planned(null, Publics.f);
        }
        public void TrendsSTEPCHANGE_FMList_Move_Planned(object sender, MouseEventArgs e)
        {
            Canvas tempsender = (Canvas)sender;
            tempsender.Opacity = 0.8;
            int toplossno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            string lossnamesearch = intermediate.Trends_Mode_Names_Planned[toplossno];
            if (tempsender.Background != BrushColors.mybrushgray)
            {
                tempsender.Background = BrushColors.mybrushLIGHTGRAY;
            }

        }
        public void TrendsSTEPCHANGE_FMList_Leave_Planned(object sender, MouseEventArgs e)
        {
            Canvas tempsender = (Canvas)sender;
            tempsender.Opacity = 1.0;
            int toplossno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            string lossnamesearch = intermediate.Trends_Mode_Names_Planned[toplossno];
            if (tempsender.Background != BrushColors.mybrushgray)
            {
                tempsender.Background = Brushes.White;
            }
        }

        public void TrendsSTEPCHANGEFailureModeSelected_Planned(object sender, RoutedEventArgs e)
        {
            Trends_Step_SelectedFailureModes_Planned.Clear();
            Canvas dep = TrendsStepChange_FMGraphicsCanvas_Planned;
            Canvas tempcanvas;
            Label templabel;
            for (int i = 0; i < intermediate.Trends_Mode_Names_Planned.Count; i++)
            {
                tempcanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "TrendsSTEPCHANGE_FMitem" + i);

                if (tempcanvas.Background == BrushColors.mybrushgray)
                {
                    templabel = getMenuItem_Label_fromitemindex(tempcanvas, -1, "", "TrendsSTEPCHANGE_FMlabel" + i);
                    Trends_Step_SelectedFailureModes_Planned.Add(templabel.Content.ToString());
                }
            }
            if (ListofSelectedKPI_StepTrends.Count > 0) //prevents errors
            {
                StepChange_UpdateChartFromIntermediateSheet();
                TrendsStepChange_PrepareLegendsList();
            }
        }


        #endregion

        #region PitStop

        public void ToggleShowHide_PitStop(object sender, MouseButtonEventArgs e)
        {
            HideAllDashboards();
            PitStopCanvas.Visibility = Visibility.Visible;

            PitStop_Onload();
        }
        public void PitStop_StartUpClicked(object sender, MouseButtonEventArgs e)
        {
            PitStopSelectionBar1.Visibility = Visibility.Visible;
            PitStopSelectionBar2.Visibility = Visibility.Hidden;
            PitStopSelectionBar3.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 70, 0.2, WidthProperty, PitStopSelectionBar1);
            PitStop_StartupCanvas.Visibility = Visibility.Visible;
            PitStop_RuntimeCanvas.Visibility = Visibility.Hidden;
            PitStop_WearoutCanvas.Visibility = Visibility.Hidden;



        }
        public void PitStop_RunTimeClicked(object sender, MouseButtonEventArgs e)
        {
            PitStopSelectionBar2.Visibility = Visibility.Visible;
            PitStopSelectionBar1.Visibility = Visibility.Hidden;
            PitStopSelectionBar3.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 78, 0.2, WidthProperty, PitStopSelectionBar2);
            PitStop_StartupCanvas.Visibility = Visibility.Hidden;
            PitStop_RuntimeCanvas.Visibility = Visibility.Visible;
            PitStop_WearoutCanvas.Visibility = Visibility.Hidden;

        }
        public void PitStop_WearOutClicked(object sender, MouseButtonEventArgs e)
        {
            //PitStopSelectionBar3.Visibility = Visibility.Visible;
            PitStopSelectionBar1.Visibility = Visibility.Hidden;
            PitStopSelectionBar2.Visibility = Visibility.Hidden;
            // AnimateZoomUIElement(0, 83, 0.2, WidthProperty, PitStopSelectionBar3);
            PitStop_StartupCanvas.Visibility = Visibility.Hidden;
            PitStop_RuntimeCanvas.Visibility = Visibility.Hidden;
            PitStop_WearoutCanvas.Visibility = Visibility.Visible;

        }
        public void PitStop_Onload()
        {
            //PitStopStartup_GenerateStartupVisuals();
            AnimateZoomUIElement(0.2, 1.0, 0.3, OpacityProperty, PitStopCanvas);
            intermediate.PitStop_initialize();
            //   PitStop_RunTime_UpdateChartFromIntermediateSheet();
            PitStopStartUpFailuremodeListbox.ItemsSource = intermediate.PitStop_SU_LossNames;
            PitStopRuntimeFailuremodeListbox.ItemsSource = intermediate.PitStop_RT_ModeNames;
            PitStopRuntimeFailuremodeListbox.SelectedItem = intermediate.PitStop_RT_SYSTEMNAME;
            PitStop_RuntimeCanvas.Visibility = Visibility.Hidden;
            PitStop_StartupCanvas.Visibility = Visibility.Visible;
            CarinfoCanvas.Visibility = Visibility.Hidden;
            YellowFlagInfoCanvas.Visibility = Visibility.Hidden;
            HideCarInfoCanvas_RawData(null, Publics.f);
            PitStop_SU_SelectFirstNFailures();
        }


        #region Startup
        public void PitStopStartUpAutoCompleteBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
        }

        public void PitStop_SU_SelectFirstNFailures()
        {
            int n = 4;
            int i = 0;

            foreach (var boundObject in PitStopStartUpFailuremodeListbox.Items)
            {
                if (i == n) { break; }
                PitStopStartUpFailuremodeListbox.SelectedItems.Add(boundObject);
                i++;
            }
        }
        public void PitStopStartUpFailureModeSelected(object sender, RoutedEventArgs e) { PitStopStartUpFailureModeSelected(); }
        public void PitStopStartUpFailureModeSelected()
        {
            List<string> Selectedfailuremodes = new List<string>();
            List<int> Selectedfailuremodeindex = new List<int>();
            List<double> Selectedfailuremode_chequeredpos = new List<double>();
            List<double> Selectedfailuremode_yellowflagpos = new List<double>();
            List<List<double>> Selectedfailuremode_listofscores = new List<List<double>>();

            int i;
            for (i = 0; i <= PitStopStartUpFailuremodeListbox.SelectedItems.Count - 1; i++) // getting the names of failure modes selected
            {
                Selectedfailuremodes.Add(PitStopStartUpFailuremodeListbox.SelectedItems[i].ToString());
            }
            for (i = 0; i <= Selectedfailuremodes.Count - 1; i++)  // getting the index of failure modes selected
            {
                Selectedfailuremodeindex.Add(intermediate.PitStop_SU_LossNames.IndexOf(Selectedfailuremodes[i]));
            }
            for (i = 0; i <= Selectedfailuremodes.Count - 1; i++)  // compiling the list of chequred flag positions and list of list of actual scores
            {
                Selectedfailuremode_chequeredpos.Add(intermediate.PitStop_SU_CheckeredFlagPositions[Selectedfailuremodeindex[i]]);
                Selectedfailuremode_listofscores.Add(intermediate.PitStop_SU_LossScores[Selectedfailuremodeindex[i]]);
                Selectedfailuremode_yellowflagpos.Add(intermediate.PitStop_SU_YellowFlagPositions[Selectedfailuremodeindex[i]]);
            }
            PitStopStartup_GenerateStartupVisuals(Selectedfailuremodes, Selectedfailuremode_chequeredpos, Selectedfailuremode_yellowflagpos, Selectedfailuremode_listofscores);
        }



        public void PitStopStartupRemapLaunched(object sender, MouseButtonEventArgs e)
        {
            MappingSplashCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, MappingSplashCanvas);
            MappingDestinationLabel.Content = "PitStop - Planned Loss Mapping";
            Remap_PitStop_PopulateMappingFields();
        }
        public void Remap_PitStop_PopulateMappingFields()
        {
            Mapping1_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.A);
            Mapping2_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.B);
            Mapping1_Combobox.SelectedItem = intermediate.PitStop_SU_Mapping_A_string;
            Mapping2_Combobox.SelectedItem = intermediate.PitStop_SU_Mapping_B_string;

        }

        public void PitStopStartup_PopulateFailureModeList()
        {
            //PitStopStartUpFailuremodeListbox.Items.Clear();
            PitStopStartUpFailuremodeListbox.ItemsSource = null;

            PitStopStartUpFailuremodeListbox.ItemsSource = intermediate.PitStop_SU_LossNames;
        }
        public void PitStopStartup_GenerateStartupVisuals(List<string> Failuremodenames, List<double> ChequeredPos, List<double> YellowFlagPos, List<List<double>> ActualScoresList)
        {
            PitStopStartup_removeallcanvas();

            double maxpossiblescore = intermediate.PitStop_SU_OverallMaxScore;
            List<double> avgscore = new List<double>();
            List<double> yellowflagscore = new List<double>();
            List<string> lossareaname = new List<string>();
            List<List<int>> actualscore = new List<List<int>>();
            int numberofStartupVisuals = Failuremodenames.Count;
            double offsetPosLeftofscale = 50;
            double offsetPosTopofscale = 70;
            double widthofscale = 750;
            double widthofcanvas = 850;
            Canvas tempcanvas;
            Image tempimage;
            double sizeofcar = 20;
            double tempactualscore = 0;
            int i;
            int j;

            for (i = 0; i < numberofStartupVisuals; i++)
            {
                GenerateCanvasUI(PitStopStartup_GraphicsAreaCanvas, "StartupVisualCanvas" + i, 100, widthofcanvas, 10, 5 + (i * 105));
                tempcanvas = getMenuItem_Canvas_fromitemindex(PitStopStartup_GraphicsAreaCanvas, -1, "StartupVisualCanvas" + i);
                //GenerateRectangleUI(tempcanvas, "startupvisualcanvasborder" + i, tempcanvas.Height, tempcanvas.Width, (double) tempcanvas.GetValue(Canvas.LeftProperty), 5 + (i * 105), null, Brushes.LightGreen, 0.5, null, null, null);
                avgscore.Add(ChequeredPos[i]);
                yellowflagscore.Add(YellowFlagPos[i]);


                //setting random actual scores for the nth visual
                for (j = 0; j <= ActualScoresList[i].Count - 1; j++)
                {
                    tempactualscore = ActualScoresList[i][j];
                    GenerateImageUI(tempcanvas, "Car_" + i + "_" + j, sizeofcar, sizeofcar, offsetPosLeftofscale + ((tempactualscore / maxpossiblescore) * widthofscale) - sizeofcar, offsetPosTopofscale - (sizeofcar - 5), AppDomain.CurrentDomain.BaseDirectory + @"\caricon.png", LaunchCarInfoCanvas, Generalmousemove, Generalmouseleave, "", -2);
                    tempimage = getMenuItem_Image_fromitemindex(tempcanvas, -1, "Car_" + i + "_" + j);
                    tempimage.ToolTip = "Event Startup Score: " + Math.Round(tempactualscore, 1).ToString();
                    AnimateZoomUIElement(offsetPosLeftofscale, offsetPosLeftofscale + ((tempactualscore / maxpossiblescore) * widthofscale), 0.2, Canvas.LeftProperty, tempimage);
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(5);
                }

                GenerateRectangleUI(tempcanvas, "ScaleLine" + i, 7, widthofscale, offsetPosLeftofscale, offsetPosTopofscale, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null);
                GenerateLabelUI(tempcanvas, "StartupLabelheader" + i, 20, 400, offsetPosLeftofscale, 10, null, BrushColors.mybrushfontgray, 11, null, null, null, -1, Failuremodenames[i], true);
                GenerateImageUI(tempcanvas, "YellowFlag" + i, 20, 20, offsetPosLeftofscale + (yellowflagscore[i] / maxpossiblescore) * widthofscale, offsetPosTopofscale - 18, AppDomain.CurrentDomain.BaseDirectory + @"\YellowFlagicon.png", LaunchYellowFlagInfoCanvas, Generalmousemove, Generalmouseleave, "Average Startup Score: " + Math.Round(avgscore[i], 1).ToString(), -3);
                GenerateImageUI(tempcanvas, "ChequeredFlag" + i, 20, 20, offsetPosLeftofscale + (avgscore[i] / maxpossiblescore) * widthofscale, offsetPosTopofscale - 18, AppDomain.CurrentDomain.BaseDirectory + @"\ChequeredFlag_Simple.png", null, null, null, "Target Startup Score: " + Math.Round(avgscore[i], 1).ToString(), -3);

                PitStopStartup_GraphicsAreaCanvas.Height = (double)tempcanvas.GetValue(Canvas.TopProperty) + 110;
            }



        }

        public void PitStopStartup_removeallcanvas()
        {
            Canvas dep = PitStopStartup_GraphicsAreaCanvas;
            Canvas cvs;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cvs = (Canvas)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(cvs);

                }
            }
        }
        public void LaunchCarInfoCanvas(object sender, MouseButtonEventArgs e)
        {
            HideCarInfoCanvas_RawData(null, Publics.f);
            int fmnumber = -1;
            int eventnumber = -1;
            Image tempsender = (Image)sender;
            string[] splits;
            splits = tempsender.Name.Split('_');
            fmnumber = Convert.ToInt32(splits[1]);
            eventnumber = Convert.ToInt32(splits[2]);

            PitStopStartup_GraphicsAreaCanvas.Opacity = 0.3;
            CarinfoCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.2, 1.0, 0.3, OpacityProperty, CarinfoCanvas);

            LoadvaluesintoCarInfoCanvas(fmnumber, eventnumber);
        }
        public void LoadvaluesintoCarInfoCanvas(int fmnumber, int eventnumber)
        {
            CarInfoCanvas_PDTname.Content = intermediate.PitStop_SU_LossNames[fmnumber];
            CarInfoCanvas_Startupscorevalue.Content = Math.Round(intermediate.PitStop_SU_LossScores[fmnumber][eventnumber], 1);
            CarInfoCanvas_DateofEvent.Content = intermediate.PitStop_SU_CarInfo[fmnumber][eventnumber].Item1;
            CarInfoCanvas_TargetStartupscorevalue.Content = Math.Round(intermediate.PitStop_SU_CheckeredFlagPositions[fmnumber]);
            CarInfoCanvas_AvgStartupscorevalue.Content = Math.Round(intermediate.PitStop_SU_YellowFlagPositions[fmnumber]);
            CarInfoCanvas_EventDurartionValue.Content = "Event duration: " + Math.Round(intermediate.PitStop_SU_CarInfo[fmnumber][eventnumber].Item5) + " min";
            //CarInfoCanvas_EventKPI1.Content = Math.Round(intermediate.pits, 0) + " Stops";
            CarInfoCanvas_EventKPI2.Content = Math.Round(intermediate.PitStop_SU_CarInfo[fmnumber][eventnumber].Item3, 0) + " Stops";

            GeneratePitStop_microdurationchart(fmnumber, eventnumber);
        }

        public void GeneratePitStop_microdurationchart(int fmnumber, int eventno)
        {
            PitStopStartup_removeallmicrobars();
            Canvas dep = CarInfoCanvas_DurationMicroChartGraphiCanvas;
            int i;
            double actualbarheight;
            double maxbarheight = dep.Height;
            double actualduration = 0;
            double actualbarwidth = 0;
            double maxduration = 0;
            SolidColorBrush rectcolor = new SolidColorBrush();
            int j;
            double tempval = 0;
            for (j = 0; j <= intermediate.PitStop_SU_LossScores[fmnumber].Count - 1; j++)
            {
                if (intermediate.PitStop_SU_CarInfo[fmnumber][j].Item5 > tempval)
                {
                    tempval = intermediate.PitStop_SU_CarInfo[fmnumber][j].Item5;
                }

            }

            maxduration = tempval;
            actualbarwidth = dep.Width / intermediate.PitStop_SU_LossScores[fmnumber].Count;

            for (i = 0; i <= intermediate.PitStop_SU_LossScores[fmnumber].Count - 1; i++)
            {
                actualduration = intermediate.PitStop_SU_CarInfo[fmnumber][i].Item5;
                actualbarheight = (actualduration / maxduration) * maxbarheight;
                if (i == eventno)
                { rectcolor = Brushes.LightGreen; }
                else
                { rectcolor = BrushColors.mybrushSelectedCriteria; }
                GenerateRectangleUI(dep, "PitStopMicroBar" + i, actualbarheight, actualbarwidth, (i + 1) * actualbarwidth, dep.Height, rectcolor, null, 0, null, null, null, 180);
            }
        }

        public void PitStopStartup_removeallmicrobars()
        {
            Canvas dep = CarInfoCanvas_DurationMicroChartGraphiCanvas;
            Rectangle rect;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(rect);

                }
            }
        }
        public void CloseCarInfoCanvas(object sender, MouseButtonEventArgs e)
        {
            CarinfoCanvas.Visibility = Visibility.Hidden;
            PitStopStartup_GraphicsAreaCanvas.Opacity = 1.0;
        }




        public void HideCarInfoCanvas_RawData(object sender, MouseButtonEventArgs e)
        {
            AnimateZoomUIElement(607, 234, 0.2, WidthProperty, CarInfoCanvasRectangle);

            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(200);
            CarInfoCanvas_RawDataHeader.Visibility = Visibility.Hidden;
            CarInfoCanvas_RawDataWindowClose.Visibility = Visibility.Hidden;
        }

        public void LaunchCarInfoRawData(object sender, MouseButtonEventArgs e)
        {
            if (CarInfoCanvasRectangle.Width == 607)
            {
                HideCarInfoCanvas_RawData(null, Publics.f);
                return;
            }

            AnimateZoomUIElement(234, 607, 0.2, WidthProperty, CarInfoCanvasRectangle);

            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(200);
            CarInfoCanvas_RawDataHeader.Visibility = Visibility.Visible;
            CarInfoCanvas_RawDataWindowClose.Visibility = Visibility.Visible;
        }



        /// /////YelowFlag

        public void LaunchYellowFlagInfoCanvas(object sender, MouseButtonEventArgs e)
        {
            int fmnumber = -1;
            int eventnumber = -1;
            Image tempsender = (Image)sender;

            fmnumber = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));


            PitStopStartup_GraphicsAreaCanvas.Opacity = 0.3;
            YellowFlagInfoCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.2, 1.0, 0.3, OpacityProperty, YellowFlagInfoCanvas);

            LoadvaluesintoYellowCardInfoCanvas(fmnumber);
        }


        public void LoadvaluesintoYellowCardInfoCanvas(int fmnumber)
        {
            YellowFlagInfoCanvas_PDTname.Content = intermediate.PitStop_SU_LossNames[fmnumber];
            YellowFlaginfoCanvas_Startupscorevalue.Content = Math.Round(intermediate.PitStop_SU_YellowFlagPositions[fmnumber]);
            //YellowFlaginfoCanvas_EventKPI1.Content = Math.Round(intermediate.PitStop_SU_CarInfo[fmnumber][eventnumber].Item3, 0) + " Stops";


        }
        public void CloseYellowFlaginfoCanvas(object sender, MouseButtonEventArgs e)
        {
            YellowFlagInfoCanvas.Visibility = Visibility.Hidden;
            PitStopStartup_GraphicsAreaCanvas.Opacity = 1.0;
        }

        #endregion


        #region Runtime

        public void PitStopRuntimeAutoCompleteBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
        }

        public void PitStopRuntimeFailureModeSelected(object sender, RoutedEventArgs e)
        {
            PitStop_RunTime_UpdateChartFromIntermediateSheet();
        }
        public void PitStopRuntimeRemapLaunched(object sender, MouseButtonEventArgs e)
        {
            MappingSplashCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, MappingSplashCanvas);
            MappingDestinationLabel.Content = "PitStop - Planned Loss Mapping";
            Remap_PitStop_PopulateMappingFields_Runtime();
        }
        public void Remap_PitStop_PopulateMappingFields_Runtime()
        {
            Mapping1_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.A);
            Mapping2_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.B);
            Mapping1_Combobox.SelectedItem = intermediate.PitStop_SU_Mapping_A_string;
            Mapping2_Combobox.SelectedItem = intermediate.PitStop_SU_Mapping_B_string;

        }

        public void PitStopRuntime_PopulateFailureModeList()
        {
            //PitStopRuntimeFailuremodeListbox.Items.Clear();
            PitStopRuntimeFailuremodeListbox.ItemsSource = null;

            PitStopRuntimeFailuremodeListbox.ItemsSource = intermediate.PitStop_RT_ModeNames;
        }

        #endregion

        #endregion

        #region StopsWatch

        public void ToggleShowHide_stopwatch(object sender, MouseButtonEventArgs e)
        {
            HideAllDashboards();
            StopsWatchCanvas.Visibility = Visibility.Visible;
            intermediate.StopsWatch_initialize();
            StopsWatchOnload(StopsWatchHeadericon, Publics.f);
        }

        public void StopsWatchOnload(object sender, MouseButtonEventArgs e)
        {
            DateTime tempdate;
            tempdate = intermediate.StopsWatch_DailyDates[0];
            StopsWatchClockDialHeader.Content = intermediate.StopsWatch_DailyFailureModeNames[0] + " Stops on " + tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture) + " " + tempdate.ToString("yyyy", CultureInfo.InvariantCulture) + " - " + intermediate.StopsWatch_DailyStops[0][0].ToString() + " Stops";


            StopsWatch_Loadvalues();
            GenerateStopsheatmap();



        }
        public void GenerateStopsheatmap()
        {
            DeleteStopswatchHeatmap();
            Canvas dep = stopsheatmapgraphicsareacanvas;
            double numberoffailuremodes = intermediate.StopsWatch_DailyFailureModeNames.Count;
            double numberofvisibledays = 25;
            double failurelabelwidth = 110;
            double failurelabelheight = 17;
            double maxsizeofheatsquare = 13;
            double failurelabegap = 10;
            double heatsquaregap = 3;
            double failurefont = 12;
            double heatsqaureoriginoffset = 10;
            double actualstopsval = 0;
            SolidColorBrush rectcolor = BrushColors.mybrushSelectedCriteria;
            double maxstopsval = intermediate.StopsWatch_DailyMax;
            double actualsizeofsquare = 0;
            Rectangle temprect;
            Label templabel;
            int i;
            int j;
            int verticaloffsetfirstrow = 0;

            for (i = 0; i < numberoffailuremodes; i++)
            {
                if (i == 0) { verticaloffsetfirstrow = 0; maxstopsval = intermediate.StopsWatch_DailyMax; rectcolor = Brushes.LawnGreen; } else { verticaloffsetfirstrow = 10; maxstopsval = intermediate.StopsWatch_DailyMax_FailureModes; rectcolor = BrushColors.mybrushSelectedCriteria; }
                if (i == 0)
                { GenerateLabelUI(dep, "failureheat" + i, failurelabelheight, failurelabelwidth, 0, verticaloffsetfirstrow + (i * (failurelabelheight + failurelabegap)), null, Brushes.DarkBlue, failurefont + 1, null, null, null, -1, intermediate.StopsWatch_DailyFailureModeNames[i], true); }
                else
                { GenerateLabelUI(dep, "failureheat" + i, failurelabelheight, failurelabelwidth, 0, verticaloffsetfirstrow + (i * (failurelabelheight + failurelabegap)), null, BrushColors.mybrushfontgray, failurefont, null, null, null, -1, intermediate.StopsWatch_DailyFailureModeNames[i], true); }
                templabel = getMenuItem_Label_fromitemindex(dep, -1, "", "failureheat" + i);
                templabel.ToolTip = templabel.Content.ToString();

                for (j = 0; j < numberofvisibledays; j++)
                {
                    if (i == 0) { verticaloffsetfirstrow = 0; } else { verticaloffsetfirstrow = 10; }

                    actualstopsval = intermediate.StopsWatch_DailyStops[i][j];
                    actualsizeofsquare = (actualstopsval / maxstopsval) * maxsizeofheatsquare;
                    GenerateRectangleUI(dep, "heatsquare_" + i + "_" + j, actualsizeofsquare, actualsizeofsquare, failurelabelwidth + heatsqaureoriginoffset + j * (heatsquaregap + maxsizeofheatsquare), verticaloffsetfirstrow + (i * (failurelabelheight + failurelabegap)), rectcolor, null, 0, Stopsheatmap_squareclicked, Stopsheatmap_Showselecteddatelabel, Stopsheatmap_Hideselecteddatelabel);
                    temprect = getMenuItem_Rectangle_fromitemindex(dep, -1, "heatsquare_" + i + "_" + j);
                    AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, temprect);
                    System.Windows.Forms.Application.DoEvents();

                }
                Thread.Sleep(2);
            }
            dep.Height = (i + 1) * (failurelabegap + failurelabelheight);
        }
        public void GenerateStopsWatchFrame_UNUSED()
        {
            DeleteStopsWatchFrameandBar("clockframebar");

            int i;
            Canvas tempcanvas;
            double radius = 175;

            double Framebarwidth = 20;
            double Framebarheight = 25;
            double heightoffset = radius;
            double widthoffset = radius;
            //These formulas are used
            // sin 30 = 0.5 (p / h)
            // cos 30 = 0.866 (b / h)
            // 1 - 0.866 = 0.134


            GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 0, Framebarheight, Framebarwidth, widthoffset, 0, BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, 180);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 1, Framebarheight, Framebarwidth, widthoffset + 0.5 * radius, 0.134 * radius, BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, 210);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 2, Framebarheight, Framebarwidth, widthoffset + 0.866 * radius, 0.5 * radius, BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, 240);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 3, Framebarheight, Framebarwidth, widthoffset + radius, radius, BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, 270);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 4, Framebarheight, Framebarwidth, widthoffset + 0.866 * radius, heightoffset + 0.5 * radius, BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, 300);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 5, Framebarheight, Framebarwidth, widthoffset + 0.5 * radius, heightoffset + radius * 0.866, BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, 330);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 6, Framebarheight, Framebarwidth, widthoffset, heightoffset + radius, BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, 360);

            GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 7, Framebarheight, Framebarwidth, widthoffset - 0.5 * radius, heightoffset + (radius * 0.866), BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, -330, -1, 1, "", new ScaleTransform(1, -1));
            GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 8, Framebarheight, Framebarwidth, widthoffset - (0.866 * radius), (heightoffset + (radius * 0.5)), BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, -300, -1, 1, "", new ScaleTransform(1, -1));
            //GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 9, Framebarheight, Framebarwidth, widthoffset -  radius, radius, BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, -270, -1, 1, "", new ScaleTransform(-1, 1));
            //GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 10, Framebarheight, Framebarwidth, widthoffset - (0.866 * radius), (0.5 * radius), BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null, -240, -1, 1, "", new ScaleTransform(-1, 1));
            //GenerateRectangleUI(ClockDialGraphicsCanvas, "clockframebar" + 11, Framebarheight, Framebarwidth, widthoffset - 0.5 * radius, (0.134 * radius), BrushColors.mybrushLIGHTGRAY, null, 0, null, null, null,-210, -1, 1, "", new ScaleTransform(-1, 1));


        }
        public void GenerateStopsWatchBars_UNUSED()
        {
            DeleteStopsWatchFrameandBar("Stopsbar");

            int i;
            Canvas tempcanvas;
            double radius = 210;
            double heightoffset = radius;
            double widthoffset = radius;
            double barwidth = 40;
            double barheight = 25;

            //These formulas are used
            // sin 30 = 0.5 (p / h)
            // cos 30 = 0.866 (b / h)
            // 1 - 0.866 = 0.134


            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 0, barheight, barwidth, widthoffset, 0, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 180);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 1, barheight, barwidth, widthoffset + 0.5 * radius, 0.134 * radius, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 210);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 2, barheight, barwidth, widthoffset + 0.866 * radius, 0.5 * radius, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 240);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 3, barheight, barwidth, widthoffset + radius, radius, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 270);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 4, barheight, barwidth, widthoffset + 0.866 * radius, heightoffset + 0.5 * radius, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 300);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 5, barheight, barwidth, widthoffset + 0.5 * radius, heightoffset + radius * 0.866, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 330);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 6, barheight, barwidth, widthoffset, heightoffset + radius, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 360);

            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 7, barheight, barwidth, widthoffset - 0.5 * radius, heightoffset + radius * 0.866, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 390);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 8, barheight, barwidth, widthoffset - 0.866 * radius, heightoffset + radius * 0.5, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 420);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 9, barheight, barwidth, widthoffset - radius, radius, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 450);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 10, barheight, barwidth, widthoffset - 0.866 * radius, 0.5 * radius, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 480);
            GenerateRectangleUI(ClockDialGraphicsCanvas, "Stopsbar" + 11, barheight, barwidth, widthoffset - 0.5 * radius, 0.134 * radius, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 510);


        }
        public void DeleteStopsWatchFrameandBar(string searchstring)
        {
            Canvas dep = ClockDialGraphicsCanvas;
            Rectangle rect;
            while (getMenuItem_Rectangle_fromitemindex(dep, -1, searchstring) != null)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);
                    if (rect.Name.Contains(searchstring))
                    {
                        dep.Children.Remove(rect);
                    }

                }

            }
        }
        public void DeleteStopswatchHeatmap()
        {
            Canvas dep = stopsheatmapgraphicsareacanvas;
            Rectangle rect;
            Label lbl;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(rect);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(lbl);

                }

            }
            StopsWatch_ClearStopsBarSelection();

        }
        public void Stopsheatmap_Hideselecteddatelabel(object sender, MouseEventArgs e)
        {
            string[] splits;
            int fmnumber = 0;
            int sqrnumber = 0;
            Canvas dep = stopsheatmapgraphicsareacanvas;

            Floatingdatelabelstopsheatmap.Visibility = Visibility.Hidden;
            Cursor = Cursors.Arrow;
            Rectangle tempsender = (Rectangle)sender;
            tempsender.Opacity = 1.0;
            tempsender.StrokeThickness = 0;
            splits = tempsender.Name.Split('_');
            fmnumber = Convert.ToInt32(splits[1]);
            sqrnumber = Convert.ToInt32(splits[2]);

            getMenuItem_Label_fromitemindex(dep, -1, "failureheat" + fmnumber, "failureheat" + fmnumber).Foreground = BrushColors.mybrushfontgray;
            getMenuItem_Label_fromitemindex(dep, -1, "failureheat" + fmnumber, "failureheat" + fmnumber).Background = Brushes.White;

        }
        public void Stopsheatmap_Showselecteddatelabel(object sender, MouseEventArgs e)
        {
            DateTime tempdate;
            string[] splits;
            int fmnumber = 0;
            int sqrnumber = 0;
            Rectangle tempsender = (Rectangle)sender;
            tempsender.Opacity = 0.8;
            tempsender.Stroke = Brushes.Black;
            tempsender.StrokeThickness = 0.5;
            Cursor = Cursors.Hand;
            splits = tempsender.Name.Split('_');
            fmnumber = Convert.ToInt32(splits[1]);
            sqrnumber = Convert.ToInt32(splits[2]);
            Canvas dep = stopsheatmapgraphicsareacanvas;
            double temppreviouscanvasleft = (double)Floatingdatelabelstopsheatmap.GetValue(Canvas.LeftProperty);
            tempdate = intermediate.StopsWatch_DailyDates[sqrnumber];
            Floatingdatelabelstopsheatmap.Content = tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture) + " " + tempdate.ToString("yyyy", CultureInfo.InvariantCulture);
            Canvas.SetLeft(Floatingdatelabelstopsheatmap, (double)tempsender.GetValue(Canvas.LeftProperty) - 50);
            double tempcurrentcanvasleft = (double)Floatingdatelabelstopsheatmap.GetValue(Canvas.LeftProperty);
            Floatingdatelabelstopsheatmap.Visibility = Visibility.Visible;
            AnimateZoomUIElement(temppreviouscanvasleft, (double)tempsender.GetValue(Canvas.LeftProperty) - 25, 0.1, Canvas.LeftProperty, Floatingdatelabelstopsheatmap);
            getMenuItem_Label_fromitemindex(dep, -1, "failureheat" + fmnumber, "failureheat" + fmnumber).Foreground = Brushes.Black;
            getMenuItem_Label_fromitemindex(dep, -1, "failureheat" + fmnumber, "failureheat" + fmnumber).Background = Brushes.MediumSpringGreen;
        }
        public void Stopsheatmap_squareclicked(object sender, MouseButtonEventArgs e)
        {
            Stopsheatmap_clearselection();
            string[] splits;
            DateTime tempdate;
            int fmnumber = 0;
            int sqrnumber = 0;
            Rectangle tempsender = (Rectangle)sender;
            tempsender.Fill = Brushes.OrangeRed;

            splits = tempsender.Name.Split('_');
            fmnumber = Convert.ToInt32(splits[1]);
            sqrnumber = Convert.ToInt32(splits[2]);
            Canvas dep = stopsheatmapgraphicsareacanvas;
            if (fmnumber == 0)
            {
                tempsender.Fill = Brushes.MediumVioletRed;
            }
            getMenuItem_Label_fromitemindex(dep, -1, "failureheat" + fmnumber, "failureheat" + fmnumber).FontWeight = FontWeights.Bold;

            intermediate.StopsWatch_DailySelectionChange(sqrnumber, fmnumber);  //Informing intermediate sheet which failuremode and day was clicked 
            StopsWatch_Loadvalues(); // UI function to load values from intermediate sheet to clock dial
            StopsWatch_ClearStopsBarSelection();
            StopsWatch_Linear_Generate();

            tempdate = intermediate.StopsWatch_DailyDates[sqrnumber];
            StopsWatchClockDialHeader.Content = intermediate.StopsWatch_DailyFailureModeNames[fmnumber] + " Stops on " + tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture) + " " + tempdate.ToString("yyyy", CultureInfo.InvariantCulture) + " - " + intermediate.StopsWatch_DailyStops[fmnumber][sqrnumber].ToString() + " Stops";

        }
        public void Stopsheatmap_clearselection()
        {
            int m;
            Canvas dep = stopsheatmapgraphicsareacanvas;
            Rectangle rect;
            Label lbl;

            for (m = 0; m <= VisualTreeHelper.GetChildrenCount(dep) - 1; m++)
            {
                if (VisualTreeHelper.GetChild(dep, m).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, m);
                    if (rect.Fill == Brushes.OrangeRed)
                    {
                        rect.Fill = BrushColors.mybrushSelectedCriteria;
                    }
                    else if (rect.Fill == Brushes.MediumVioletRed)
                    {
                        rect.Fill = Brushes.LawnGreen;
                    }

                }
                else if (VisualTreeHelper.GetChild(dep, m).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, m);
                    if (lbl.FontWeight == FontWeights.Bold)
                    {
                        lbl.FontWeight = FontWeights.Normal;

                    }

                }


            }

        }
        public void StopsWatch_Loadvalues()
        {
            Canvas dep = ClockDialGraphicsCanvas;
            double maxheightofstopsbar = 90;
            double maxvalueofhourlystops = intermediate.StopsWatch_HourlyMax_12;
            double actualvalofhourlystops = 0;
            int i;
            Random rnd = new Random();
            Rectangle temprect;
            double actualstopsbarheight = 0;



            for (i = 1; i <= 12; i++)
            {
                actualvalofhourlystops = (intermediate.StopsWatch_HourlyStops_12[i - 1]);
                temprect = getMenuItem_Rectangle_fromitemindex(dep, -1, "", "Stopsbar" + i);
                if (maxvalueofhourlystops == 0) { maxvalueofhourlystops = 1; }
                actualstopsbarheight = (actualvalofhourlystops / maxvalueofhourlystops) * maxheightofstopsbar;

                temprect.Height = actualstopsbarheight;

                AnimateZoomUIElement(0, actualstopsbarheight, 0.2, HeightProperty, temprect);
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(10);

                temprect.MouseDown += StopsWatch_ShowHourlyinfo;
                temprect.MouseMove += Generalmousemove;
                temprect.MouseLeave += Generalmouseleave;
            }

            if (AMlabel.Foreground == BrushColors.mybrushSelectedCriteria)
            {
                StopsWatchHourInfo_HeaderLabel2.Content = "AM";
            }
            else
            {
                StopsWatchHourInfo_HeaderLabel2.Content = "PM";
            }

        }
        public void StopsWatch_ShowHourlyinfo(object sender, MouseButtonEventArgs e)
        {
            StopsWatch_ClearStopsBarSelection();
            Rectangle tempsender = (Rectangle)sender;
            tempsender.Stroke = Brushes.DarkGray;
            tempsender.StrokeThickness = 1;
            int hourvalue = 0;
            hourvalue = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));



            StopsWatchHourInfo_ValueLabel2.Content = hourvalue.ToString();
            StopsWatchHourInfo_ValueLabel1.Content = intermediate.StopsWatch_HourlyStops_12[hourvalue - 1].ToString();
            StopsWatchHourInfo_ValueLabel3.Content = Math.Round(100 * intermediate.StopsWatch_HourlyAvailability_12[hourvalue - 1], 0) + "%";

            StopsWatchHourInfo_Canvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.2, 1.0, 0.3, OpacityProperty, StopsWatchHourInfoCircle1);
            AnimateZoomUIElement(0.2, 1.0, 0.3, OpacityProperty, StopsWatchHourInfoCircle2);
            AnimateZoomUIElement(0.2, 1.0, 0.3, OpacityProperty, StopsWatchHourInfoCircle3);

        }
        public void StopsWatch_ClearStopsBarSelection()
        {
            int m;
            Canvas dep = ClockDialGraphicsCanvas;
            Rectangle rect;


            for (m = 0; m <= VisualTreeHelper.GetChildrenCount(dep) - 1; m++)
            {
                if (VisualTreeHelper.GetChild(dep, m).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, m);
                    rect.StrokeThickness = 0;

                }
            }

            StopsWatchHourInfo_Canvas.Visibility = Visibility.Hidden;
        }
        public void AMPMToggleClicked(object sender, MouseButtonEventArgs e)
        {
            int temptogglepos = ToggleNow(AMPMToggleframe, AMPMToggleball);  // this is the function that does the toggle and returns the final position of the ball after the toggle
            if (temptogglepos == 0) // Zero means Toggle Ball is on the Left 
            {
                AMlabel.Foreground = BrushColors.mybrushSelectedCriteria;
                PMlabel.Foreground = BrushColors.mybrushLIGHTGRAY;
                intermediate.StopsWatch_SetAM();
                StopsWatch_Loadvalues();
            }
            else if (temptogglepos == 1)  // One means Toggle Ball is on the right.
            {
                AMlabel.Foreground = BrushColors.mybrushLIGHTGRAY;
                PMlabel.Foreground = BrushColors.mybrushSelectedCriteria;
                intermediate.StopsWatch_SetPM();
                StopsWatch_Loadvalues();
            }

        }
        public void Launch_Remap_StopsWatch(object sender, MouseButtonEventArgs e)
        {
            MappingSplashCanvas.Visibility = Visibility.Visible;
            Remap_StopsWatch_PopulateMappingFields();
            MappingDestinationLabel.Content = "StopsWatch Mapping";
        }

        public void Remap_StopsWatch_PopulateMappingFields()
        {
            Mapping1_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.A);
            Mapping2_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.B);
            Mapping1_Combobox.SelectedItem = getStringForEnum(intermediate.StopsWatch_Mapping_A);
            Mapping2_Combobox.SelectedItem = getStringForEnum(intermediate.StopsWatch_Mapping_B);

        }

        public void StopsWatch_Linear_Generate()
        {
            StopsWatch_Linear_Clear();

            int numberofbars = 24;
            int hourvalue = 0;
            double maxstopsvalue = intermediate.StopsWatch_HourlyMax_24;
            double actualstopsvalue = 0;
            double linearactualbarheight = 0;
            double linearmaxbarheight = 350;
            double linearbarwidth = 24;
            double gapbetweenlinearbars = 5;
            Random rnd = new Random();
            Rectangle temprect;
            int i;
            Canvas dep = LinearDialGraphicsCanvas;

            for (i = 0; i < 24; i++)
            {
                //determine whether its AM or PM

                int actualhourvalue = i + 1;
                string AMPM = "AM";
                if (i + 1 > 12)
                {
                    AMPM = "PM";
                    actualhourvalue = i + 1 - 12;
                }

                //generate rectangle and labels for linear view                
                actualstopsvalue = intermediate.StopsWatch_HourlyStops_24[i];
                linearactualbarheight = (actualstopsvalue / maxstopsvalue) * linearmaxbarheight;
                GenerateRectangleUI(dep, "LinearHourBar" + i, linearactualbarheight, linearbarwidth, (i * linearbarwidth) + gapbetweenlinearbars, dep.Height, BrushColors.mybrushSelectedCriteria, null, 0, StopsWatch_Linear_ShowHourInfo, Generalmousemove, Generalmouseleave, 180);
                temprect = getMenuItem_Rectangle_fromitemindex(dep, -1, "", "LinearHourBar" + i);
                GenerateLabelUI(dep, "LinearHourDataLabel" + i, 15, linearbarwidth, (double)temprect.GetValue(Canvas.LeftProperty) - (temprect.Width + linearbarwidth) / 2, dep.Height - (temprect.Height + 15), null, BrushColors.mybrushfontgray, 9, null, null, null, -1, actualstopsvalue.ToString());
                GenerateLabelUI(dep, "LinearHourLabel" + i, 15, linearbarwidth, (double)temprect.GetValue(Canvas.LeftProperty) - (temprect.Width + linearbarwidth) / 2, dep.Height + 2, null, Brushes.DarkGray, 8, null, null, null, -1, actualhourvalue.ToString() + " " + AMPM);




            }

            //Default click the first hour of day to show hourly info
            temprect = getMenuItem_Rectangle_fromitemindex(dep, -1, "", "LinearHourBar" + 0);
            StopsWatch_Linear_ShowHourInfo(temprect, Publics.f);
        }

        public void StopsWatch_Linear_Clear()
        {
            Canvas dep = LinearDialGraphicsCanvas;
            Rectangle rect;
            Label lbl;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(rect);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(lbl);

                }

            }
        }

        public void StopsWatch_Linear_ShowHourInfo(object sender, MouseButtonEventArgs e)
        {
            StopsWatch_Linear_ClearSelection();
            Rectangle tempsender = (Rectangle)sender;
            tempsender.Stroke = Brushes.Gray;
            tempsender.StrokeThickness = 1;
            int hourclicked = -1;
            int actualhourvalue = 0;
            string AMPM = "";


            hourclicked = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            actualhourvalue = hourclicked + 1;
            AMPM = "AM";
            if (hourclicked + 1 > 12)
            {
                AMPM = "PM";
                actualhourvalue = hourclicked + 1 - 12;
            }

            LinearDial_AvInfoLabel.Content = Math.Round(intermediate.StopsWatch_HourlyAvailability_24[hourclicked] * 100, 1) + "%";
            LinearDial_HourInfoLabel.Content = actualhourvalue.ToString();
            LinearDial_HourInfoLabelHeader.Content = AMPM;
            LinearDial_StopsInfoLabel.Content = intermediate.StopsWatch_HourlyStops_24[hourclicked];
        }

        public void StopsWatch_Linear_ClearSelection()
        {
            Canvas dep = LinearDialGraphicsCanvas;
            Rectangle rect;

            int m;
            for (m = 0; m <= VisualTreeHelper.GetChildrenCount(dep) - 1; m++)
            {
                if (VisualTreeHelper.GetChild(dep, m).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, m);
                    rect.StrokeThickness = 0;

                }
            }
        }

        public void ShowClockDialStopsWatch(object sender, MouseButtonEventArgs e)
        {
            ClockDialCanvas.Visibility = Visibility.Visible;
            LinearDialCanvas.Visibility = Visibility.Hidden;

            Ellipse tempsender = (Ellipse)sender;
            tempsender.Fill = BrushColors.mybrushLIGHTGRAY;
            LinearViewCircularbutton.Fill = Brushes.White;

        }
        public void ShowLinearDialStopsWatch(object sender, MouseButtonEventArgs e)
        {
            LinearDialCanvas.Visibility = Visibility.Visible;
            ClockDialCanvas.Visibility = Visibility.Hidden;
            Ellipse tempsender = (Ellipse)sender;
            tempsender.Fill = BrushColors.mybrushLIGHTGRAY;
            ClassicViewCircularbutton.Fill = Brushes.White;
            StopsWatch_Linear_Generate();

        }
        #endregion

        #region Loss Network

        public void ToggleShowHide_lossnetwork(object sender, MouseButtonEventArgs e)
        {
            HideAllDashboards();
            LossNetworkCanvas.Visibility = Visibility.Visible;
            intermediate.LossNetwork_initialize();
            LossNetwork_onload(null, Publics.f);
        }
        public void LossNetwork_onload(object sender, MouseButtonEventArgs e)
        {
            LossNetwork_CreateBubbles();
        }

        public void LossNetwork_CreateBubbles()
        {
            LossNetwork_DeleteNetworks();
            double maxbubblesize = 90;
            double actualbubblesize = 0;
            Random rnd = new Random();
            double numberofbubbles = 0;
            Canvas dep = LossNetworkGraphicsCanvas;
            int numberoflines = intermediate.LossNetwork_LossOEE.Count - 1;

            double maxbubbleval = 0;
            double actualbubbleval = 0;
            double TopPos_so_far = 0;
            double LeftOffset = 230;
            double gapbetweenlines = 100;
            List<SolidColorBrush> Colorvalues = new List<SolidColorBrush>();
            Ellipse tempbubble;
            Colorvalues.Add(Brushes.MediumSpringGreen);
            Colorvalues.Add(BrushColors.mybrushSelectedCriteria);
            Colorvalues.Add(Brushes.MediumOrchid);
            Colorvalues.Add(Brushes.DarkCyan);


            //maxbubblesize = dep.Height / numberofbubbles;
            int lossindex;
            int lineindex;

            for (lineindex = 0; lineindex <= numberoflines; lineindex++)
            {
                TopPos_so_far = 30;
                numberofbubbles = intermediate.LossNetwork_LossNames[lineindex].Count;
                for (lossindex = 0; lossindex < numberofbubbles; lossindex++)
                {
                    maxbubbleval = intermediate.LossNetwork_MaxOEELoss(lineindex);
                    actualbubbleval = intermediate.LossNetwork_LossOEE[lineindex][lossindex];
                    actualbubblesize = (actualbubbleval / maxbubbleval) * maxbubblesize;
                    GenerateEllipseUI(dep, "Lossnetworkbubble" + lineindex + "_" + lossindex, actualbubblesize, actualbubblesize, gapbetweenlines + ((lineindex + 1) * LeftOffset) - actualbubblesize / 2, TopPos_so_far, Brushes.LightGreen, null, 0, LossNetwork_bubbleclicked, Generalmousemove, Generalmouseleave, 0, -1, 1, intermediate.LossNetwork_LossNames[lineindex][lossindex].ToString() + " DT%: " + Math.Round(intermediate.LossNetwork_LossOEE[lineindex][lossindex] * 100, 1) + ", Stops: " + Math.Round(intermediate.LossNetwork_LossStops[lineindex][lossindex]));
                    tempbubble = getMenuItem_Ellipse_fromitemindex(dep, -1, "", "Lossnetworkbubble" + lineindex + "_" + lossindex);

                    if (actualbubblesize > 10)
                    {
                        GenerateLabelUI(dep, "Lossnetwork_FMnamelabel" + lineindex + "_" + lossindex, actualbubblesize, 150, gapbetweenlines + ((lineindex + 1) * LeftOffset) - 190, TopPos_so_far, null, BrushColors.mybrushlightgray, 8, null, null, null, -1, intermediate.LossNetwork_LossNames[lineindex][lossindex].ToString(), true);
                    }
                    AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, tempbubble);
                    System.Windows.Forms.Application.DoEvents();
                    Thread.Sleep(5);
                    TopPos_so_far = TopPos_so_far + actualbubblesize;
                }

                GenerateLabelUI(dep, "LossNetwork_LineName" + lineindex, 25, 200, gapbetweenlines + ((lineindex + 1) * LeftOffset) - 190, 5, null, BrushColors.mybrushSelectedCriteria, 15, null, null, null, -1, intermediate.Multi_CurrentLineNames[lineindex], true);
                LossNetwork_CreateRelationshipsBetweenBubbles(lineindex);
                gapbetweenlines = LossNetwork_MaxRadius + 100;
            }
            if (dep.Height < TopPos_so_far)
            {
                dep.Height = TopPos_so_far;
            }
            dep.Width = gapbetweenlines + ((lineindex + 1) * LeftOffset);
        }



        public void LossNetwork_CreateRelationshipsBetweenBubbles(int k = 0)
        {

            int destinationbubblenumber = 0;
            int originbubblenumber = 0;
            int numberofrelations = 0;
            Canvas dep = LossNetworkGraphicsCanvas;
            Ellipse tempbubble_dest;
            Ellipse tempbubble_origin;
            double actualdependency = 0;
            double maxdependency = 0;
            double maxlinethickness = 3;
            double actuallinethickness = 0;
            Random rnd = new Random();
            double numberofbubbles = 0;
            int numberofpacklines = intermediate.LossNetwork_LossOEE.Count - 1;
            int i;
            int j;

            double CenterX;
            double CenterY;
            double Radius;
            Ellipse tempelp;
            Rectangle rect;
            LossNetwork_MaxRadius = 0;
            numberofrelations = rnd.Next(5, 15);
            originbubblenumber = rnd.Next(0, 10);

            numberofbubbles = intermediate.LossNetwork_LossOEE[k].Count - 1;
            for (i = 0; i <= numberofbubbles; i++)
            {
                tempbubble_origin = getMenuItem_Ellipse_fromitemindex(dep, -1, "", "Lossnetworkbubble" + k + "_" + i);
                if (intermediate.LossNetwork_LossOEE[k][i] != 0)
                {
                    numberofrelations = intermediate.LossNetwork_Dependencies[k][i].Count;
                    for (j = 0; j < numberofrelations; j++)
                    {
                        destinationbubblenumber = intermediate.LossNetwork_Dependencies[k][i][j].Item1;
                        actualdependency = intermediate.LossNetwork_Dependencies[k][i][j].Item2;
                        maxdependency = intermediate.LossNetwork_MaxDependency[k];
                        actuallinethickness = (actualdependency / maxdependency) * maxlinethickness;

                        if (destinationbubblenumber != -1 && intermediate.LossNetwork_LossOEE[k][destinationbubblenumber] != 0)
                        {
                            tempbubble_dest = getMenuItem_Ellipse_fromitemindex(dep, -1, "", "Lossnetworkbubble" + k + "_" + destinationbubblenumber);
                            tempbubble_origin.Fill = BrushColors.mybrushSelectedCriteria;
                            tempbubble_dest.Fill = BrushColors.mybrushSelectedCriteria;


                            //GenerateLineUI(dep, "relationline" + i, (double)tempbubble_origin.GetValue(Canvas.LeftProperty) + tempbubble_origin.Width / 2, (double)tempbubble_origin.GetValue(Canvas.TopProperty) + tempbubble_origin.Height / 2, (double)tempbubble_dest.GetValue(Canvas.LeftProperty) + tempbubble_dest.Width / 2, (double)tempbubble_dest.GetValue(Canvas.TopProperty) + tempbubble_dest.Height / 2, (SolidColorBrush)tempbubble_origin.Fill, actuallinethickness, null, null, null);
                            CenterX = (double)tempbubble_origin.GetValue(Canvas.LeftProperty) + tempbubble_origin.Width / 2;
                            CenterY = ((double)tempbubble_origin.GetValue(Canvas.TopProperty) + tempbubble_origin.Height / 2 + (double)tempbubble_dest.GetValue(Canvas.TopProperty) + tempbubble_dest.Height / 2) / 2;
                            Radius = ((double)tempbubble_dest.GetValue(Canvas.TopProperty) + tempbubble_dest.Height / 2) - CenterY;
                            if (Radius < 0) { Radius = -1 * Radius; }
                            // GenerateEllipseUI(dep, "DependencyEllipse" + i, 2 * Radius, 2 * Radius, CenterX - Radius, CenterY - Radius, null, (SolidColorBrush)tempbubble_origin.Fill, actuallinethickness, null, null, null);
                            // tempelp = getMenuItem_Ellipse_fromitemindex(dep, -1, "", "DependencyEllipse" + i);
                            GenerateImageUI(dep, "RelationArc" + i, 2 * Radius, Radius, CenterX, CenterY - Radius, AppDomain.CurrentDomain.BaseDirectory + @"\Arc.png", null, null, null, "");
                            if (Radius > LossNetwork_MaxRadius)
                            {
                                LossNetwork_MaxRadius = Radius;
                            }


                        }
                    }
                }
            }

        }
        public void LossNetwork_DeleteNetworks()
        {
            Canvas dep = LossNetworkGraphicsCanvas;

            Ellipse elp;
            Line lne;
            Label lbl;
            Image img;

            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Ellipse") > -1)
                {
                    elp = (Ellipse)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(elp);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Line") > -1)
                {
                    lne = (Line)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(lne);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(lbl);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Image") > -1)
                {
                    img = (Image)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(img);

                }
            }
        }
        public void LossNetwork_bubbleclicked(object sender, MouseButtonEventArgs e)
        {
            int fmindex;
            Ellipse tempsender = (Ellipse)sender;
            fmindex = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name));
            //  LossNetwork_ListBox.SelectedIndex = fmindex;
        }
        public void Launch_LossNetwork_Remap(object sender, MouseButtonEventArgs e)
        {
            MappingSplashCanvas.Visibility = Visibility.Visible;

            Mapping1_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.A);
            Mapping2_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(CardTier.B);
            Mapping1_Combobox.SelectedItem = getStringForEnum(intermediate.LossNetwork_Mapping_A);
            Mapping2_Combobox.SelectedItem = getStringForEnum(intermediate.LossNetwork_Mapping_B);

            MappingDestinationLabel.Content = "Loss Network Mapping";
        }


        public void LossNetwork_RefreshBubbles(object sender, MouseButtonEventArgs e)
        {
            int numberofbubbles = intermediate.LossNetwork_LossOEE.Count;
            int i;
            Ellipse tempbubble;
            for (i = 0; i < numberofbubbles; i++)
            {

                tempbubble = getMenuItem_Ellipse_fromitemindex(LossNetworkGraphicsCanvas, -1, "", "Lossnetworkbubble" + i);

                tempbubble.Stroke = Brushes.Black;
                tempbubble.StrokeThickness = 0;
                tempbubble.Opacity = 1.0;

            }

        }


        #endregion

        #region LiveLine

        public void ToggleShowHide_LiveLine(object sender, MouseButtonEventArgs e)
        {
            HideAllDashboards();
            LiveLineCanvas.Visibility = Visibility.Visible;

            LiveLine_onload(null, Publics.f);
        }
        public void LiveLine_onload(object sender, MouseButtonEventArgs e)
        {
            intermediate.LiveLine_initialize();
            LiveLine_GenerateDTViewer();
            LiveLine_TopLoss_GenerateItems();
            LiveLine_TopPlanned_GenerateItems();
            LiveLine_TopDelta_GenerateItems();
            LiveLine_Trend_GenerateChart(DowntimeMetrics.OEE, 24);
            LiveLine_UpdateDTViewerLabels();
        }

        private void LiveLine_UpdateDTViewerLabels()
        {
            LiveLine_label_uptime.Content = "Uptime - " + Math.Round(intermediate.LiveLine_AnalysisPeriodData.OEE * 100, 0) + "%";
            LiveLine_label_planned.Content = "Planned - " + Math.Round(intermediate.LiveLine_AnalysisPeriodData.PDTpct * 100, 0) + "%";
            LiveLine_label_unplanned.Content = "Unplanned - " + Math.Round(intermediate.LiveLine_AnalysisPeriodData.UPDTpct * 100, 0) + "%";
        }

        public void LiveLine_GenerateDTViewer()
        {
            LiveLine_ClearDTViewer();
            Rectangle temprect;
            Canvas dep = LiveLineDTViewerGraphicsCanvas;
            Label templabel;
            int noofevents = intermediate.LiveLine_NumberOfEvents;


            double rectheight = dep.Height;
            double rectwidth = 0;
            double sumofalldur = 0;
            double currentLeftPos = 0;

            List<double> actualeventdur = new List<double>();
            List<SolidColorBrush> Colorvalues = new List<SolidColorBrush>();
            Colorvalues.Add(BrushColors.mybrushLIGHTBLUEGREEN);
            Colorvalues.Add(BrushColors.bubblecolorRed);
            Colorvalues.Add(BrushColors.mybrushdarkgray);
            Colorvalues.Add(BrushColors.mybrushbrightblue);
            SolidColorBrush Actualcolor = new SolidColorBrush(); ;

            int j;

            for (j = 0; j < noofevents; j++)
            {
                actualeventdur.Add(intermediate.LiveLine_ActualDurationOfEachEvent[j]);
                sumofalldur = sumofalldur + actualeventdur[j];
            }


            int i;
            for (i = 0; i < noofevents; i++)
            {

                rectwidth = (actualeventdur[i] / sumofalldur) * dep.Width;
                if (intermediate.LiveLine_EventTypes[i] == EventType.Unplanned) { Actualcolor = Colorvalues[1]; }
                if (intermediate.LiveLine_EventTypes[i] == EventType.Planned) { Actualcolor = Colorvalues[3]; }
                if (intermediate.LiveLine_EventTypes[i] == EventType.Running) { Actualcolor = Colorvalues[0]; }
                if (intermediate.LiveLine_EventTypes[i] == EventType.Excluded) { Actualcolor = Colorvalues[2]; }


                GenerateRectangleUI(dep, "DTviewrect" + i, rectheight, rectwidth, currentLeftPos, 0, Actualcolor, null, 0, LiveLine_DTviewer_EventSeleced, LiveLine_DTviewer_Eventmousemove, LiveLine_DTviewer_Eventmouseleave, 0, -1, 1, intermediate.LiveLine_DTviewer_EventNames[i]);
                currentLeftPos = currentLeftPos + rectwidth;

            }

            //TimeLabel
            GenerateLabelUI(dep, "DTviewer_TimeLabel", 18, 120, 0, -19, Brushes.DarkSlateGray, Brushes.White, 8, null, null, null, -1, "");

            templabel = getMenuItem_Label_fromitemindex(LiveLineDTViewerGraphicsCanvas, -1, "", "DTviewer_TimeLabel");
            templabel.Visibility = Visibility.Hidden;


            // Time Highlight rectangle
            GenerateRectangleUI(dep, "DTviewer_TimeHighlight", dep.Height + 10, 10, 0, -5, null, BrushColors.mybrushdarkgray, 1, null, null, null);
            temprect = getMenuItem_Rectangle_fromitemindex(dep, -1, "", "DTviewer_TimeHighlight");
            temprect.Visibility = Visibility.Hidden;

            //Time Frame Header
            DateTime starttime = intermediate.LiveLine_SelectedStartTime;
            DateTime endtime = intermediate.LiveLine_SelectedEndTime;
            LiveLineDtViewer_TimeFrameHeader.Content = starttime.ToString("MMM", CultureInfo.InvariantCulture) + " " + starttime.ToString("dd", CultureInfo.InvariantCulture) + ", " + starttime.ToString("hh", CultureInfo.InvariantCulture) + ":" + starttime.ToString("mm", CultureInfo.InvariantCulture) + " to " + endtime.ToString("MMM", CultureInfo.InvariantCulture) + " " + endtime.ToString("dd", CultureInfo.InvariantCulture) + ", " + endtime.ToString("hh", CultureInfo.InvariantCulture) + ":" + endtime.ToString("mm", CultureInfo.InvariantCulture);
            LiveLineTrends_TimeFrameHeader.Content = LiveLineDtViewer_TimeFrameHeader.Content;

        }

        public void LiveLine_LocateTimeHighlightRectangle(double LeftPos = 0, double width = 0)
        {
            Rectangle temprect;
            double currentLeftPos = 0;
            Canvas dep = LiveLineDTViewerGraphicsCanvas;
            temprect = getMenuItem_Rectangle_fromitemindex(dep, -1, "", "DTviewer_TimeHighlight");
            temprect.Width = width + 4;
            temprect.Visibility = Visibility.Visible;
            currentLeftPos = (double)temprect.GetValue(Canvas.LeftProperty);
            System.Windows.Forms.Application.DoEvents();
            AnimateZoomUIElement(currentLeftPos, LeftPos - 2 - width, 0.2, Canvas.LeftProperty, temprect);
        }




        public void LiveLine_ClearDTViewer()
        {
            Canvas dep = LiveLineDTViewerGraphicsCanvas;
            Rectangle rect;
            Label lbl;

            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(rect);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(lbl);

                }
            }
        }

        public void LiveLine_DTviewer_Eventmousemove(object sender, MouseEventArgs e)
        {
            Rectangle tempsender = (Rectangle)sender;
            tempsender.Opacity = 0.7;
            Cursor = Cursors.Hand;
            Label templabel;
            DateTime tempdate;
            double DTduration = 0;
            int eventno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            templabel = getMenuItem_Label_fromitemindex(LiveLineDTViewerGraphicsCanvas, -1, "", "DTviewer_TimeLabel");
            tempdate = intermediate.LiveLine_EventStartTimes[eventno];
            DTduration = intermediate.LiveLine_ActualDurationOfEachEvent[eventno];
            templabel.Content = tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture) + " " + tempdate.ToString("hh: mm tt", CultureInfo.InvariantCulture) + "  [" + Math.Round(DTduration, 1) + " min]";
            templabel.Visibility = Visibility.Visible;
            AnimateZoomUIElement((double)templabel.GetValue(Canvas.LeftProperty), (double)tempsender.GetValue(Canvas.LeftProperty) + tempsender.Width / 2 - templabel.Width / 2, 0.1, Canvas.LeftProperty, templabel);

        }
        public void LiveLine_DTviewer_Eventmouseleave(object sender, MouseEventArgs e)
        {
            Rectangle tempsender = (Rectangle)sender;
            Label templabel;
            Cursor = Cursors.Arrow;
            tempsender.Opacity = 1.0;
            templabel = getMenuItem_Label_fromitemindex(LiveLineDTViewerGraphicsCanvas, -1, "", "DTviewer_TimeLabel");
            templabel.Visibility = Visibility.Hidden;

        }
        public void LiveLine_DTviewer_EventSeleced(object sender, MouseButtonEventArgs e)
        {
            LiveLine_DTviewer_Eventselectionclear();
            LiveLine_TopLoss_CanvasClearSelection();

            Rectangle tempsender = (Rectangle)sender;

            int eventno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            tempsender.StrokeThickness = 1.0;
            tempsender.Stroke = Brushes.Black;
            LiveLine_DTview_selectedlossname.Content = intermediate.LiveLine_DTviewer_EventNames[eventno].ToString() + ":  " + Math.Round(intermediate.LiveLine_ActualDurationOfEachEvent[eventno], 1) + " min";

            int i;
            int canvasno = 0;
            for (i = 0; i < intermediate.LiveLine_TopLosses.Count; i++)
            {
                if (intermediate.LiveLine_DTviewer_EventNames[eventno].ToString() == intermediate.LiveLine_TopLosses[i].Item1.ToString())
                {
                    canvasno = i;
                }
            }

            getMenuItem_Canvas_fromitemindex(LiveLine_TopLossGraphicsCanvas, -1, "", "LiveLine_TopLossItem" + canvasno).Background = BrushColors.mybrushgray;
        }

        public void LiveLine_DTviewer_Eventselectionclear()
        {
            int i;
            for (i = 0; i < intermediate.LiveLine_NumberOfEvents; i++)
            {
                getMenuItem_Rectangle_fromitemindex(LiveLineDTViewerGraphicsCanvas, -1, "", "DTviewrect" + i).StrokeThickness = 0;
            }

        }

        public void LiveLine_TimeFrameChanged(object sender, MouseButtonEventArgs e)
        {
            LiveLine_RefreshTimeframeSelection();
            Label tempsender = (Label)sender;
            tempsender.Background = BrushColors.mybrushSelectedCriteria;

            if (tempsender.Content.ToString().Contains("7 days")) { intermediate.LiveLine_setDTviewerTimeFrame(7); LiveLineTrends_TimeFrame = 7; LiveLine_Trend_GenerateChart(DowntimeMetrics.OEE, 7); }
            if (tempsender.Content.ToString().Contains("30 days")) { intermediate.LiveLine_setDTviewerTimeFrame(30); LiveLineTrends_TimeFrame = 30; LiveLine_Trend_GenerateChart(DowntimeMetrics.OEE, 30); }
            if (tempsender.Content.ToString().Contains("24 hours")) { intermediate.LiveLine_setDTviewerTimeFrame(1); LiveLineTrends_TimeFrame = 1; LiveLine_Trend_GenerateChart(DowntimeMetrics.OEE, 24); }

            LiveLine_UpdateDTViewerLabels();

            LiveLine_GenerateDTViewer();
            LiveLine_TopLoss_GenerateItems();
            LiveLine_TopPlanned_GenerateItems();
            LiveLine_TopDelta_GenerateItems();


        }
        public void LiveLine_RefreshTimeframeSelection()
        {
            LiveLine_Last24hours.Background = BrushColors.mybrushdarkgray;
            LiveLine_Last7days.Background = BrushColors.mybrushdarkgray;
            LiveLine_Last30days.Background = BrushColors.mybrushdarkgray;

        }

        public void LiveLine_Trend_SetCharttoOEE(object sender, MouseButtonEventArgs e)
        {
            LiveLine_Trend_GenerateChart(DowntimeMetrics.OEE, LiveLineTrends_TimeFrame);
            LiveLine_Trends_OEE.Background = BrushColors.mybrushSelectedCriteria;
            LiveLine_Trends_Stops.Background = BrushColors.mybrushdarkgray;
        }
        public void LiveLine_Trend_SetCharttoStops(object sender, MouseButtonEventArgs e)
        {
            LiveLine_Trend_GenerateChart(DowntimeMetrics.Stops, LiveLineTrends_TimeFrame);
            LiveLine_Trends_Stops.Background = BrushColors.mybrushSelectedCriteria;
            LiveLine_Trends_OEE.Background = BrushColors.mybrushdarkgray;
        }


        public void LiveLine_Trend_GenerateChart(DowntimeMetrics losstype, int noofbars)
        {
            if (noofbars == 1) { noofbars = 24; }

            LiveLine_Trends_ClearChart();
            Canvas dep = LiveLineTrendGraphicCanvas;
            Rectangle temprect;
            Label templabel;
            double maxbarheight = dep.Height;
            double actualbarheight = 0;
            double actuallossvalue = 0;
            double maxlossvalue = 0;
            double gapbetweenbars = 0;
            string actualfieldlabel = "";
            DateTime tempdate;
            double widthofbar = (dep.Width - (gapbetweenbars * noofbars)) / noofbars;
            int i;

            if (losstype == DowntimeMetrics.Stops)
            {
                maxlossvalue = intermediate.LiveLine_Trends_MaxStops;
            }
            else if (losstype == DowntimeMetrics.OEE)
            {
                maxlossvalue = Math.Round(100 * intermediate.LiveLine_Trends_MaxOEE, 1);
            }

            for (i = 0; i < noofbars; i++)
            {
                if (losstype == DowntimeMetrics.Stops)
                {
                    actuallossvalue = intermediate.LiveLine_TrendsData[i].Item3;

                }
                else if (losstype == DowntimeMetrics.OEE)
                {
                    actuallossvalue = Math.Round(100 * intermediate.LiveLine_TrendsData[i].Item2, 1);
                }

                tempdate = intermediate.LiveLine_TrendsData[i].Item1;
                switch (LiveLineTrends_TimeFrame)
                {
                    case 7:
                        actualfieldlabel = tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture);
                        break;
                    case 1:
                        actualfieldlabel = tempdate.ToString("HH", CultureInfo.InvariantCulture) + ":00";
                        break;
                    case 30:
                        actualfieldlabel = tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture);
                        break;
                    default:

                        actualfieldlabel = tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture);
                        break;

                }


                actualbarheight = (actuallossvalue / maxlossvalue) * maxbarheight;

                GenerateRectangleUI(dep, "LiveLine_TrendBar" + i, 0.8 * actualbarheight, widthofbar, widthofbar + (i * (widthofbar + gapbetweenbars)), 0.9 * dep.Height, BrushColors.mybrushSelectedCriteria, Brushes.White, 0.5, LiveLine_Trends_Clicked, Generalmousemove, Generalmouseleave, 180, -1, 1, "");
                GenerateLabelUI(dep, "LiveLine_TrendDateLabel" + i, 10, widthofbar, widthofbar + ((i - 1) * (widthofbar + gapbetweenbars)), 0.9 * dep.Height, null, BrushColors.mybrushfontgray, 7, null, null, null, -1, actualfieldlabel);
                temprect = getMenuItem_Rectangle_fromitemindex(dep, -1, "", "LiveLine_TrendBar" + i);
                GenerateLabelUI(dep, "LiveLine_DataLabel" + i, 10, widthofbar, widthofbar + ((i - 1) * (widthofbar + gapbetweenbars)), dep.Height - temprect.Height - 20, null, BrushColors.mybrushfontgray, 7, null, null, null, -1, Math.Round(actuallossvalue, 1).ToString());

            }
        }

        public void LiveLine_Trends_ClearChart()
        {
            Canvas dep = LiveLineTrendGraphicCanvas;
            Rectangle rect;
            Label lbl;

            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(rect);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(lbl);

                }
            }
        }

        public void LiveLine_Trends_Clicked(object sender, MouseButtonEventArgs e)
        {
            Rectangle temprect = (Rectangle)sender;
            Canvas dep = LiveLineTrendGraphicCanvas;
            LiveLine_LocateTimeHighlightRectangle((double)temprect.GetValue(Canvas.LeftProperty), temprect.Width);
        }


        ////TopLoss


        public void LiveLine_TopLoss_GenerateItems()
        {
            LiveLine_TopCardsGraphicCanvas_Clear(LiveLine_TopLossGraphicsCanvas);
            Random rnd = new Random();
            Canvas dep = LiveLine_TopLossGraphicsCanvas;
            Canvas tempcanvas;
            Rectangle temprect;
            Label templabel;
            double itemheight = 30;
            double itemverticalgap = 5;
            double gapbetweenlabelandbar = 5;
            double datalabelwidth = 30;
            double stopslabelwidth = 50;
            double lossnamelabelwidth = 150;
            double itemwidth = 360;
            double actuallossvalue = 0;
            double maxlossvalue = Math.Round(100 * intermediate.LiveLine_TopLoss_MaxLossValue, 1);
            double maxlossbarwidth = 100;
            double actualbarwidth = 0;
            int i;
            int j;

            if (maxlossvalue == 0) { maxlossvalue = 1; }

            for (i = 0; i < intermediate.LiveLine_TopLosses.Count; i++)
            {
                GenerateCanvasUI(dep, "LiveLine_TopLossItem" + i, itemheight, itemwidth, 0, itemverticalgap + (i * itemheight));
                tempcanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "LiveLine_TopLossItem" + i);
                tempcanvas.MouseDown += LiveLine_TopLoss_CanvasMouseDown;
                tempcanvas.MouseMove += Generalmousemove;
                tempcanvas.MouseLeave += Generalmouseleave;
                GenerateLabelUI(tempcanvas, "LiveLine_TopLoss_lossnameLabel" + i, itemheight, lossnamelabelwidth, 0, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, intermediate.LiveLine_TopLosses[i].Item1, true);
                templabel = getMenuItem_Label_fromitemindex(tempcanvas, -1, "", "LiveLine_TopLoss_lossnameLabel" + i);
                templabel.ToolTip = templabel.Content.ToString();

                actuallossvalue = Math.Round(100 * intermediate.LiveLine_TopLosses[i].Item2, 1);
                actualbarwidth = (actuallossvalue / maxlossvalue) * maxlossbarwidth;
                GenerateRectangleUI(tempcanvas, "LiveLine_TopLoss_bar" + i, 0.5 * itemheight, actualbarwidth, lossnamelabelwidth + gapbetweenlabelandbar, 0.25 * itemheight, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null);
                GenerateLabelUI(tempcanvas, "LiveLine_TopLoss_datalabel" + i, itemheight, datalabelwidth, lossnamelabelwidth + gapbetweenlabelandbar + actualbarwidth + 2, 0, null, BrushColors.mybrushfontgray, 9, null, null, null, -1, actuallossvalue + "%", true);
                GenerateLabelUI(tempcanvas, "LiveLine_TopLoss_stopslabel" + i, itemheight, stopslabelwidth, lossnamelabelwidth + gapbetweenlabelandbar + maxlossbarwidth + datalabelwidth + 15, 0, null, BrushColors.mybrushfontgray, 11, null, null, null, -1, intermediate.LiveLine_TopLosses[i].Item3 + " stops", true);

                dep.Height = (itemheight - itemverticalgap) + (i * (itemheight + itemverticalgap));
                AnimateZoomUIElement(0.5 * actualbarwidth, actualbarwidth, 0.1, WidthProperty, getMenuItem_Rectangle_fromitemindex(tempcanvas, -1, "", "LiveLine_TopLoss_bar" + i));
                AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, tempcanvas);
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(10);

            }


        }

        public void LiveLine_TopLoss_CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            LiveLine_TopLoss_CanvasClearSelection();
            Canvas tempsender = (Canvas)sender;
            int toplossno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            string lossnamesearch = intermediate.LiveLine_TopLosses[toplossno].Item1;
            tempsender.Background = BrushColors.mybrushgray;
            Rectangle temprect;
            int i;
            LiveLine_DTviewer_Eventselectionclear();
            for (i = 0; i < intermediate.LiveLine_NumberOfEvents; i++)
            {
                if (intermediate.LiveLine_DTviewer_EventNames[i] == lossnamesearch)
                {
                    temprect = getMenuItem_Rectangle_fromitemindex(LiveLineDTViewerGraphicsCanvas, -1, "", "DTviewrect" + i);
                    temprect.StrokeThickness = 1.0;
                    temprect.Stroke = Brushes.Black;
                    LiveLine_DTview_selectedlossname.Content = lossnamesearch;
                }
            }

        }

        public void LiveLine_TopLoss_CanvasClearSelection()
        {
            Canvas tempcanvas;
            int i;

            for (i = 0; i < intermediate.LiveLine_TopLosses.Count; i++)
            {
                tempcanvas = getMenuItem_Canvas_fromitemindex(LiveLine_TopLossGraphicsCanvas, -1, "", "LiveLine_TopLossItem" + i);
                tempcanvas.Background = null;
            }
        }

        public void LiveLine_TopCardsGraphicCanvas_Clear(Canvas dep)
        {

            Canvas cvs;


            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cvs = (Canvas)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(cvs);

                }
            }

        }

        //Top Planned

        public void LiveLine_TopPlanned_GenerateItems()
        {
            LiveLine_TopCardsGraphicCanvas_Clear(LiveLine_TopLosChangeoverraphicsCanvas);
            Random rnd = new Random();
            Canvas dep = LiveLine_TopLosChangeoverraphicsCanvas;
            Canvas tempcanvas;
            Rectangle temprect;
            Label templabel;
            double itemheight = 30;
            double itemverticalgap = 5;
            double gapbetweenlabelandbar = 5;
            double datalabelwidth = 40;
            double stopslabelwidth = 50;
            double lossnamelabelwidth = 150;
            double itemwidth = 360;
            double actuallossvalue = 0;
            double maxlossvalue = intermediate.LiveLine_TopLoss_MaxValue_Planned;
            double maxlossbarwidth = 100;
            double actualbarwidth = 0;
            int i;
            int j;

            if (maxlossvalue == 0) { maxlossvalue = 1; }

            for (i = 0; i < intermediate.LiveLine_Planned.Count; i++)
            {
                GenerateCanvasUI(dep, "LiveLine_TopPlannedItem" + i, itemheight, itemwidth, 0, itemverticalgap + (i * itemheight));
                tempcanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "LiveLine_TopPlannedItem" + i);
                //tempcanvas.MouseDown += LiveLine_TopPlanned_CanvasMouseDown;
                tempcanvas.MouseMove += Generalmousemove;
                tempcanvas.MouseLeave += Generalmouseleave;
                GenerateLabelUI(tempcanvas, "LiveLine_TopPlanned_lossnameLabel" + i, itemheight, lossnamelabelwidth, 0, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, intermediate.LiveLine_Planned[i].Item1, true);
                templabel = getMenuItem_Label_fromitemindex(tempcanvas, -1, "", "LiveLine_TopPlanned_lossnameLabel" + i);
                templabel.ToolTip = templabel.Content.ToString();

                actuallossvalue = Math.Round(intermediate.LiveLine_Planned[i].Item2);
                actualbarwidth = (actuallossvalue / maxlossvalue) * maxlossbarwidth;
                GenerateRectangleUI(tempcanvas, "LiveLine_TopPlanned_bar" + i, 0.5 * itemheight, actualbarwidth, lossnamelabelwidth + gapbetweenlabelandbar, 0.25 * itemheight, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null);
                GenerateLabelUI(tempcanvas, "LiveLine_TopPlanned_datalabel" + i, itemheight, datalabelwidth, lossnamelabelwidth + gapbetweenlabelandbar + actualbarwidth + 2, 0, null, BrushColors.mybrushfontgray, 9, null, null, null, -1, actuallossvalue + "min", true);
                GenerateLabelUI(tempcanvas, "LiveLine_TopPlanned_stopslabel" + i, itemheight, stopslabelwidth, lossnamelabelwidth + gapbetweenlabelandbar + maxlossbarwidth + datalabelwidth + 10, 0, null, BrushColors.mybrushfontgray, 11, null, null, null, -1, Math.Round(intermediate.LiveLine_Planned[i].Item3 * 100, 1) + "% DT", true);

                dep.Height = (itemheight - itemverticalgap) + (i * (itemheight + itemverticalgap));
                AnimateZoomUIElement(0.5 * actualbarwidth, actualbarwidth, 0.1, WidthProperty, getMenuItem_Rectangle_fromitemindex(tempcanvas, -1, "", "LiveLine_TopPlanned_bar" + i));
                AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, tempcanvas);
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(20);

            }


        }


        //Top Delta 
        public void LiveLine_TopDelta_GenerateItems()
        {
            LiveLine_TopCardsGraphicCanvas_Clear(LiveLine_TopDeltaGraphicsCanvas);
            Random rnd = new Random();
            Canvas dep = LiveLine_TopDeltaGraphicsCanvas;
            Canvas tempcanvas;
            Rectangle temprect;
            Label templabel;
            double itemheight = 30;
            double itemverticalgap = 5;
            double gapbetweenlabelandDT = 10;
            double gapbetweenDTandstops = 30;
            double datalabelwidth = 55;
            double stopslabelwidth = 50;
            double lossnamelabelwidth = 150;
            double itemwidth = 360;
            double actuallossvalue = 0;
            double deltaiconwidth = 10;
            double deltaiconheight = 0.5 * itemheight;


            double actualbarwidth = 0;
            int i;
            int j;
            string deltaimagefilename = "";


            for (i = 0; i < intermediate.LiveLine_BiggestChanges.Count; i++)
            {
                GenerateCanvasUI(dep, "LiveLine_TopDeltaItem" + i, itemheight, itemwidth, 0, itemverticalgap + (i * itemheight));
                tempcanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "LiveLine_TopDeltaItem" + i);
                // tempcanvas.MouseDown += LiveLine_TopDelta_CanvasMouseDown;
                tempcanvas.MouseMove += Generalmousemove;
                tempcanvas.MouseLeave += Generalmouseleave;
                GenerateLabelUI(tempcanvas, "LiveLine_TopDelta_lossnameLabel" + i, itemheight, lossnamelabelwidth, 0, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, intermediate.LiveLine_BiggestChanges[i].Item1, true);
                templabel = getMenuItem_Label_fromitemindex(tempcanvas, -1, "", "LiveLine_TopDelta_lossnameLabel" + i);
                templabel.ToolTip = templabel.Content.ToString();


                //DT
                actuallossvalue = Math.Round(100 * intermediate.LiveLine_BiggestChanges[i].Item2, 1);

                if (actuallossvalue >= 0)
                {
                    deltaimagefilename = "UpDelta";
                }
                else
                {
                    deltaimagefilename = "DownDelta";
                }
                GenerateImageUI(tempcanvas, "LiveLine_TopDelta_DTdeltaicon" + i, deltaiconheight, deltaiconwidth, lossnamelabelwidth + gapbetweenlabelandDT, itemheight / 2 - deltaiconheight / 2, AppDomain.CurrentDomain.BaseDirectory + @"\" + deltaimagefilename + ".png", null, null, null);
                GenerateLabelUI(tempcanvas, "LiveLine_TopDelta_DTlabel" + i, itemheight, datalabelwidth, lossnamelabelwidth + gapbetweenlabelandDT + deltaiconwidth + 2, 0, null, BrushColors.mybrushfontgray, 9, null, null, null, -1, actuallossvalue + "% DT", true);


                //Stops
                actuallossvalue = intermediate.LiveLine_BiggestChanges[i].Item3;

                if (actuallossvalue >= 0)
                {
                    deltaimagefilename = "UpDelta";
                }
                else
                {
                    deltaimagefilename = "DownDelta";
                }
                GenerateImageUI(tempcanvas, "LiveLine_TopDelta_Stopsdeltaicon" + i, deltaiconheight, deltaiconwidth, lossnamelabelwidth + gapbetweenlabelandDT + deltaiconwidth + 2 + datalabelwidth + gapbetweenDTandstops, itemheight / 2 - deltaiconheight / 2, AppDomain.CurrentDomain.BaseDirectory + @"\" + deltaimagefilename + ".png", null, null, null);
                GenerateLabelUI(tempcanvas, "LiveLine_TopDelta_stopslabel" + i, itemheight, stopslabelwidth, lossnamelabelwidth + gapbetweenlabelandDT + deltaiconwidth + 2 + datalabelwidth + gapbetweenDTandstops + deltaiconwidth + 2, 0, null, BrushColors.mybrushfontgray, 11, null, null, null, -1, intermediate.LiveLine_BiggestChanges[i].Item3 + " stops", true);

                dep.Height = (itemheight - itemverticalgap) + (i * (itemheight + itemverticalgap));
                AnimateZoomUIElement(0.2, 1.0, 0.1, OpacityProperty, tempcanvas);
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(10);




            }


        }



        #endregion


        #region MindtheGap
        public void ToggleShowHide_MindtheGap(object sender, MouseButtonEventArgs e)
        {
            HideAllDashboards();
            GapAnalysisCanvas.Visibility = Visibility.Visible;
            intermediate.initializeGapAnalysis();
            GapAnalysisTargetInputCanvas.Visibility = Visibility.Hidden;
            GapAnalysis_Onload();

        }

        public void GapAnalysis_Onload()
        {
            intermediate.Gap_reMap_Level1(DowntimeField.Tier1, DowntimeField.NA);
            intermediate.Gap_reMap_Level2(DowntimeField.Tier2, DowntimeField.NA);
            gapanalysis_activeDTmetric = DowntimeMetrics.DT;
            intermediate.Gap_SetNewKPI(gapanalysis_activeDTmetric);
            GapAnalysis_ClearChart();
            ListofLevel2ContainerHeight.Clear();
            GapAnalysis_Charts_Generate(GapAnalysisGraphicsCanvas, 1, intermediate.Gap_Level1_LossNames_Unplanned, intermediate.Gap_Level1_LossKPIs_Unplanned);
        }
        public void GapAnalysis_Charts_Generate(Canvas dep, int LevelNo, List<string> ListofLossNames, List<double> ListofLossValues, double lossvaluemax = 1)
        {
            if (LevelNo == 1)
            {
                GapAnalysis_ClearChart();
                ListofLevel2ContainerHeight.Clear();
            }


            Canvas tempcanvas_main = null;

            Canvas tempcanvas_lossbar = null;
            Canvas tempcanvas_level2container = null;

            SolidColorBrush headercolor = new SolidColorBrush(Color.FromRgb(170, 170, 170));
            Rectangle temprect = null;
            Label templabel = null;
            double itemheight = 50;
            double itemverticalgap = 2;
            double gapbetweenFMlabel_lossbarcanvas = 50;

            double gapbetweenlossbarcanvas_OEE = 50;
            double gapbetweenOEE_Mean = 30;
            double widthofchartcanvas;

            double lossdatalabelwidth = 40;
            double OEElosslabelwidth = 30;
            double MeanDurnlabelwidth = 30;
            double lossnamelabelwidth = 150;
            double itemwidth = dep.Width;
            double actuallossvalue = 0;


            double minmaxchartwidth = 600;
            double minBubbleLeftPos = 0;
            double maxBubbleLeftPos = 0;
            double lossbarcanvaswidth = 100;
            double lossbarwidth = 0;
            double lossbarmaxwidth = 0;
            double deltaiconheight = 0.8 * itemheight;
            double deltaiconwidth = deltaiconheight;
            double labelleftposoffset = 10;
            double firstitemoffset = 40;
            double canvashorizontaloffset = 40;
            double actualbarwidth = 0;
            int i;
            int j;
            string deltaimagefilename = "";

            double level2canvasheight = 0;
            double level2canvaswidth = itemwidth;
            string s_levelno = "";
            double targetvalue = 0;
            double gapbarwidth = 0;
            double gapbarorientation = 0;
            SolidColorBrush gapbarcolor = new SolidColorBrush();
            double gapheightfactor = .2;


            if (LevelNo == 1)
            { s_levelno = "one"; }
            else if (LevelNo == 2)
            { s_levelno = "two"; }

            if (LevelNo == 2)
            {
                firstitemoffset = 0;
                canvashorizontaloffset = 55;
                itemheight = 30;
            }


            for (i = 0; i < ListofLossNames.Count; i++)
            {
                //Main Canvas Item
                GenerateCanvasUI(dep, "GapAnalysis" + "Level" + s_levelno + "Item" + i, itemheight, itemwidth, canvashorizontaloffset, firstitemoffset + itemverticalgap + (i * itemheight));
                tempcanvas_main = getMenuItem_Canvas_fromitemindex(dep, -1, "", "GapAnalysis" + "Level" + s_levelno + "Item" + i);
                tempcanvas_main.Background = Brushes.White;
                tempcanvas_main.Cursor = Cursors.Hand;

                if (LevelNo == 1)
                {
                    GenerateRectangleUI(tempcanvas_main, "GapAnalysisItemTopBorder" + i, 0.3, tempcanvas_main.Width, 0, 0, BrushColors.mybrushNOTSelectedCriteria, null, 0, null, null, null);
                    if (i == intermediate.Gap_Level1_LossNames_Unplanned.Count - 1)
                    {
                        GenerateRectangleUI(tempcanvas_main, "GapAnalysisItemBottomBorder" + i, 0.3, tempcanvas_main.Width, 0, itemheight, BrushColors.mybrushNOTSelectedCriteria, null, 0, null, null, null);
                    }

                }

                //Main Failure Mode name label
                GenerateLabelUI(tempcanvas_main, "GapAnalysis" + "Level" + s_levelno + "_FMname" + i, itemheight, lossnamelabelwidth, labelleftposoffset, 0, null, BrushColors.mybrushfontgray, 12, null, null, null, -1, ListofLossNames[i], true);
                templabel = getMenuItem_Label_fromitemindex(tempcanvas_main, -1, "", "GapAnalysis" + "Level" + s_levelno + "_FMname" + i);
                templabel.ToolTip = templabel.Content.ToString();


                if (LevelNo == 1)
                {
                    lossvaluemax = ListofLossValues.Max();
                    if (lossvaluemax == 0)
                    { lossvaluemax = 1; }
                    //Level2Canvas
                    GenerateCanvasUI(tempcanvas_main, "GapAnalysisLeveltwoContainer" + i, level2canvasheight, level2canvaswidth, (double)tempcanvas_main.GetValue(Canvas.LeftProperty), tempcanvas_main.Height);
                    tempcanvas_level2container = getMenuItem_Canvas_fromitemindex(tempcanvas_main, -1, "", "GapAnalysisLeveltwoContainer" + i);
                    GapAnalysis_Charts_Generate(tempcanvas_level2container, 2, intermediate.Gap_Level2_LossNames_Unplanned[i], intermediate.Gap_Level2_LossKPIs_Unplanned[i], lossvaluemax);
                    //tempcanvas_level2container.Opacity = 0;

                    // Expansion and Contraction
                    GenerateImageUI(tempcanvas_main, "GapAnalysisExpansionIcon" + i, itemheight, 15, -30, 0, AppDomain.CurrentDomain.BaseDirectory + @"\ResizePlus.png", GapAnalysis_ItemClicked, Generalmousemove, Generalmouseleave);
                    GenerateImageUI(tempcanvas_main, "GapAnalysisContractionIcon" + i, itemheight, 15, -30, 0, AppDomain.CurrentDomain.BaseDirectory + @"\ResizeMinus.png", GapAnalysis_ItemClicked, Generalmousemove, Generalmouseleave);
                    //hide the minus icon 
                    getMenuItem_Image_fromitemindex(tempcanvas_main, -1, "", "GapAnalysisContractionIcon" + i).Visibility = Visibility.Hidden;
                }


                //Loss bar canvas
                GenerateCanvasUI(tempcanvas_main, "GapAnalysis_lossbarCanvas" + i, itemheight, lossbarcanvaswidth, (double)tempcanvas_main.GetValue(Canvas.LeftProperty) + templabel.Width + gapbetweenFMlabel_lossbarcanvas, 0);
                tempcanvas_lossbar = getMenuItem_Canvas_fromitemindex(tempcanvas_main, -1, "", "GapAnalysis_lossbarCanvas" + i);
                GenerateRectangleUI(tempcanvas_lossbar, "GapAnalysis_lossbarVerticalBase" + i, 0.8 * itemheight, 1.5, 0, 0.1 * itemheight, Brushes.DarkGray, null, 0, null, null, null);

                //horizontal bar in Loss bar canvas

                lossbarwidth = (ListofLossValues[i] / lossvaluemax) * tempcanvas_lossbar.Width;
                GenerateRectangleUI(tempcanvas_lossbar, "GapAnalysis_lossbar" + i, 0.6 * tempcanvas_lossbar.Height, lossbarwidth, 0, 0.2 * tempcanvas_lossbar.Height, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null);
                temprect = getMenuItem_Rectangle_fromitemindex(tempcanvas_lossbar, -1, "", "GapAnalysis_lossbar" + i);
                GenerateLabelUI(tempcanvas_lossbar, "GapAnalysis_lossbar_DataLabel" + i, itemheight, lossdatalabelwidth, (double)temprect.GetValue(Canvas.LeftProperty) + temprect.Width + 5, 0, null, BrushColors.mybrushfontgray, 9, null, null, null, -1, Math.Round(ListofLossValues[i], 1).ToString(), true);

                //target line
                targetvalue = (intermediate.Gap_FindTargetForLoss(ListofLossNames[i], DowntimeMetrics.DT, DowntimeField.Tier1, DowntimeField.NA) / lossvaluemax) * tempcanvas_lossbar.Width;
                GenerateRectangleUI(tempcanvas_lossbar, "GapAnalysis_targetline" + i, itemheight * 1.2, 1, targetvalue, -0.1 * itemheight, Brushes.Black, null, 0, null, null, null);

                //target upload button
                GenerateLabelUI(tempcanvas_main, "GapAnalysis_targetbuttonLevel" + s_levelno + i, 0.5 * itemheight, 20, (double)tempcanvas_main.GetValue(Canvas.LeftProperty) + templabel.Width + gapbetweenFMlabel_lossbarcanvas + tempcanvas_lossbar.Width + 100, 0.25 * itemheight, Brushes.LightGray, Brushes.Black, 9, GapAnalysis_ShowTargetInputBox, Generalmousemove, Generalmouseleave, 2, "T");
                dep.Height = firstitemoffset + (itemheight - itemverticalgap) + (i * (itemheight + itemverticalgap));
                getMenuItem_Label_fromitemindex(tempcanvas_main, -1, "", "GapAnalysis_targetbuttonLevel" + s_levelno + i).ToolTip = "Add/Change Target";


                //Gap bar
                gapbarcolor = Brushes.Gray;
                if (intermediate.Gap_FindTargetForLoss(ListofLossNames[i], DowntimeMetrics.DT, DowntimeField.Tier1, DowntimeField.NA) != -1)
                {
                    gapbarwidth = lossbarwidth - targetvalue;
                    if (gapbarwidth > 0) { gapbarcolor = Brushes.Red; gapbarorientation = 0; gapheightfactor = .2; }
                    else if (gapbarwidth < 0) { gapbarcolor = Brushes.LightGreen; gapbarwidth = -1 * gapbarwidth; gapbarorientation = 180; gapheightfactor = 0.8; }
                }
                else
                {
                    gapbarwidth = 0;
                    gapbarorientation = 0;
                    gapheightfactor = .2;
                }
                GenerateRectangleUI(tempcanvas_main, "GapAnalysis_gapbar" + s_levelno + i, 0.6 * tempcanvas_main.Height, gapbarwidth, (double)tempcanvas_main.GetValue(Canvas.LeftProperty) + templabel.Width + gapbetweenFMlabel_lossbarcanvas + tempcanvas_lossbar.Width + 200 + tempcanvas_lossbar.Width, gapheightfactor * tempcanvas_main.Height, gapbarcolor, null, 0, null, null, null, gapbarorientation, -1, 1, "Gap (Difference between actual loss and target loss. Red bar indicates loss is above target");
                //defaultgapbaseline
                GenerateRectangleUI(tempcanvas_main, "GapAnalysis_gapbaseline" + s_levelno + i, tempcanvas_main.Height, 1, (double)tempcanvas_main.GetValue(Canvas.LeftProperty) + templabel.Width + gapbetweenFMlabel_lossbarcanvas + tempcanvas_lossbar.Width + 200 + tempcanvas_lossbar.Width, 0, Brushes.Gray, null, 0, null, null, null);


            }
            if (LevelNo == 2)
            {
                ListofLevel2ContainerHeight.Add(dep.Height);
                dep.Height = 0;
                dep.Visibility = Visibility.Hidden;

            }
            if (LevelNo == 1)
            {
                GenerateLabelUI(dep, "GapAnalysis_FMLabelheader", 30, 150, canvashorizontaloffset + labelleftposoffset, 0, null, headercolor, 11, null, null, null, -1, "Competency area", true);
                GenerateLabelUI(dep, "GapAnalysis_Lossheader", 30, 150, canvashorizontaloffset + (double)tempcanvas_lossbar.GetValue(Canvas.LeftProperty), 0, null, headercolor, 11, null, null, null, -1, "Competency rating", true);
                GenerateLabelUI(dep, "GapAnalysis_GapLabel", 30, 150, canvashorizontaloffset + (double)tempcanvas_main.GetValue(Canvas.LeftProperty) + templabel.Width + gapbetweenFMlabel_lossbarcanvas + tempcanvas_lossbar.Width + 190 + tempcanvas_lossbar.Width, 0, null, headercolor, 11, null, null, null, -1, "Gap", true);
            }
            // GenerateLabelUI(dep, "GapAnalysis_OEElossheader", 30, 50, (double)tempcanvas_lossbar.GetValue(Canvas.LeftProperty) + tempcanvas_lossbar.Width + gapbetweenlossbarcanvas_OEE, 0, null, headercolor, 11, null, null, null, -1, "Jobs Loss", true);
            // GenerateLabelUI(dep, "GapAnalysis_MeanDuration", 30, 80, (double)tempcanvas_lossbar.GetValue(Canvas.LeftProperty) + tempcanvas_lossbar.Width + gapbetweenlossbarcanvas_OEE + OEElosslabelwidth + gapbetweenOEE_Mean, 0, null, headercolor, 11, null, null, null, -1, "Average (min)", true);

        }
        public void GapAnalysis_ClearChart()
        {
            Canvas dep = GapAnalysisGraphicsCanvas;
            Canvas cvs;
            Label lbl;
            Rectangle rect;
            Image img;
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cvs = (Canvas)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(cvs);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(rect);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(lbl);

                }
                else if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Image") > -1)
                {
                    img = (Image)VisualTreeHelper.GetChild(dep, 0);

                    dep.Children.Remove(img);

                }
            }
        }

        public void GapAnalysis_ItemClicked(object sender, MouseButtonEventArgs e)
        {
            if (ListofLevel2ContainerHeight.Count > 0)
            {
                Image tempsender = (Image)sender;
                Canvas dep = GapAnalysisGraphicsCanvas;
                Canvas tempcanvasmain = null;
                Canvas tempcanvas_others = null;
                Canvas tempcanvas_level2container = null;
                double verticaloffset = 0;
                int itemno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
                int i;




                tempcanvasmain = getMenuItem_Canvas_fromitemindex(dep, -1, "", "GapAnalysis" + "Level" + "one" + "Item" + itemno);


                tempcanvas_level2container = getMenuItem_Canvas_fromitemindex(tempcanvasmain, -1, "", "GapAnalysisLeveltwoContainer" + itemno);

                if (tempcanvas_level2container.Height == 0)
                {
                    tempcanvas_level2container.Visibility = Visibility.Visible;
                    AnimateZoomUIElement(0, ListofLevel2ContainerHeight[itemno], 0.15, HeightProperty, tempcanvas_level2container);
                    verticaloffset = ListofLevel2ContainerHeight[itemno] - tempcanvas_level2container.Height;
                    getMenuItem_Image_fromitemindex(getMenuItem_Canvas_fromitemindex(dep, -1, "", "GapAnalysis" + "Level" + "one" + "Item" + itemno), -1, "", "GapAnalysisExpansionIcon" + itemno).Visibility = Visibility.Hidden;
                    getMenuItem_Image_fromitemindex(getMenuItem_Canvas_fromitemindex(dep, -1, "", "GapAnalysis" + "Level" + "one" + "Item" + itemno), -1, "", "GapAnalysisContractionIcon" + itemno).Visibility = Visibility.Visible;

                }
                else if (tempcanvas_level2container.Height != 0)
                {
                    tempcanvas_level2container.Visibility = Visibility.Hidden;
                    AnimateZoomUIElement(tempcanvas_level2container.Height, 0, 0.1, HeightProperty, tempcanvas_level2container);
                    verticaloffset = -1 * (ListofLevel2ContainerHeight[itemno] - 0);
                    getMenuItem_Image_fromitemindex(getMenuItem_Canvas_fromitemindex(dep, -1, "", "GapAnalysis" + "Level" + "one" + "Item" + itemno), -1, "", "GapAnalysisExpansionIcon" + itemno).Visibility = Visibility.Visible;
                    getMenuItem_Image_fromitemindex(getMenuItem_Canvas_fromitemindex(dep, -1, "", "GapAnalysis" + "Level" + "one" + "Item" + itemno), -1, "", "GapAnalysisContractionIcon" + itemno).Visibility = Visibility.Hidden;
                }



                if (itemno != ListofLevel2ContainerHeight.Count - 1)
                {
                    for (i = itemno + 1; i < intermediate.Gap_Level1_LossKPIs_Unplanned.Count; i++)
                    {
                        tempcanvas_others = getMenuItem_Canvas_fromitemindex(GapAnalysisGraphicsCanvas, -1, "", "GapAnalysisLeveloneItem" + i);
                        AnimateZoomUIElement((double)tempcanvas_others.GetValue(Canvas.TopProperty), (double)tempcanvas_others.GetValue(Canvas.TopProperty) + verticaloffset, 0.15, Canvas.TopProperty, tempcanvas_others);
                    }
                }


                dep.Height = (double)getMenuItem_Canvas_fromitemindex(dep, -1, "", "GapAnalysis" + "LeveloneItem" + (intermediate.Gap_Level1_LossKPIs_Unplanned.Count - 1)).GetValue(Canvas.TopProperty) + 200;


            }
            else
            {
                MessageBox.Show("No 2nd level");
            }


        }

        public void GapAnalysis_ShowTargetInputBox(object sender, MouseButtonEventArgs e)
        {
            GapAnalysisGraphicsCanvas.Opacity = 0.3;
            GapAnalysisGraphicsCanvas.IsEnabled = false;
            GapAnalysisTargetInputCanvas.Visibility = Visibility.Visible;
            Label templabel = (Label)sender;
            double gettargetval = -1;

            int Level1CanvasNo = -1;

            gapanalysis_activefmno = Convert.ToInt32(GlobalFcns.onlyDigits(templabel.Name.ToString()));

            if (templabel.Name.ToString().Contains("one"))
            {
                gapanalysis_clickedlevel = "one";
                GapAnalysis_TargetFailureModeName.Content = intermediate.Gap_Level1_LossNames_Unplanned[gapanalysis_activefmno];
                gettargetval = intermediate.Gap_FindTargetForLoss(intermediate.Gap_Level1_LossNames_Unplanned[gapanalysis_activefmno].ToString(), DowntimeMetrics.DT, DowntimeField.Tier1, DowntimeField.NA);

            }
            else if (templabel.Name.ToString().Contains("two"))
            {
                gapanalysis_clickedlevel = "two";

                Canvas parentcanvas = (Canvas)templabel.Parent;
                parentcanvas = (Canvas)parentcanvas.Parent;
                Level1CanvasNo = Convert.ToInt32(GlobalFcns.onlyDigits(parentcanvas.Name.ToString()));

                GapAnalysis_TargetFailureModeName.Content = intermediate.Gap_Level2_LossNames_Unplanned[Level1CanvasNo][gapanalysis_activefmno];
                gettargetval = intermediate.Gap_FindTargetForLoss(intermediate.Gap_Level2_LossNames_Unplanned[Level1CanvasNo][gapanalysis_activefmno].ToString(), DowntimeMetrics.DT, DowntimeField.Tier1, DowntimeField.NA);


            }
            if (gettargetval == -1)
            {
                GapAnalysis_TargetInputTextBox.Text = "None";
            }
            else
            {
                GapAnalysis_TargetInputTextBox.Text = gettargetval.ToString();
            }

            GapAnalysis_TargetInput_Metricname.Content = "Set targets for " + getStringForEnum_Metric(gapanalysis_activeDTmetric);

        }
        public void GapAnalysis_HideTargetInputBox(object sender, MouseButtonEventArgs e)
        {
            GapAnalysisTargetInputCanvas.Visibility = Visibility.Hidden;
            GapAnalysisGraphicsCanvas.Opacity = 1.0;
            GapAnalysisGraphicsCanvas.IsEnabled = true;
        }

        public void GapAnalysis_UploadTargets(object sender, MouseButtonEventArgs e)
        {

            if (GapAnalysis_TargetInputTextBox.Text.ToString() == "None")
            {
                GapAnalysisTargetInputCanvas.Visibility = Visibility.Hidden;
                GapAnalysisGraphicsCanvas.Opacity = 1.0;
                GapAnalysisGraphicsCanvas.IsEnabled = true;

            }

            double n;
            bool isNumeric = double.TryParse(GapAnalysis_TargetInputTextBox.Text.ToString(), out n);


            if (isNumeric == true && Convert.ToDouble(GapAnalysis_TargetInputTextBox.Text.ToString()) >= 0)

            {

                if (gapanalysis_clickedlevel == "one")
                {
                    intermediate.Gap_AddTarget(GapAnalysis_TargetFailureModeName.Content.ToString(), Convert.ToDouble(GapAnalysis_TargetInputTextBox.Text.ToString()), gapanalysis_activeDTmetric, DowntimeField.Tier1, DowntimeField.NA);

                }

                else if (gapanalysis_clickedlevel == "two")
                {
                    intermediate.Gap_AddTarget(GapAnalysis_TargetFailureModeName.Content.ToString(), Convert.ToDouble(GapAnalysis_TargetInputTextBox.Text.ToString()), gapanalysis_activeDTmetric, DowntimeField.Tier2, DowntimeField.NA);

                }

                GapAnalysis_Charts_Generate(GapAnalysisGraphicsCanvas, 1, intermediate.Gap_Level1_LossNames_Unplanned, intermediate.Gap_Level1_LossKPIs_Unplanned);
                GapAnalysisTargetInputCanvas.Visibility = Visibility.Hidden;
                GapAnalysisGraphicsCanvas.Opacity = 1.0;
                GapAnalysisGraphicsCanvas.IsEnabled = true;
                GapAnalysis_TargetInput_NumericEntryError.Visibility = Visibility.Hidden;

            }
            else
            {
                GapAnalysis_TargetInput_NumericEntryError.Visibility = Visibility.Visible;
            }
        }
        private void GapAnalysis_TargetInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tempsender = (TextBox)sender;
            if (tempsender.Text == "Enter Target")
            {
                tempsender.Text = "";
            }
        }
        #endregion



        #region MultiLine
        //#MULTILINE
        public void LaunchMultiLineFailureMode(object sender, MouseButtonEventArgs e)
        {
            var tmpList = new List<DTeventSummary>();

            FloatingToolTipCanvas.Visibility = Visibility.Hidden;
            MultiLineSplashCanvas.Visibility = Visibility.Visible;
            ActivateMultiLine_DeepDiveCanvas(MultiLineDeepDiveButton, Publics.f);
            MultiLineChartHeaderLabel.Content = ActiveToolTip_FailureModename;

            tmpList = intermediate.Multi_UpdateLossModeGraphs(ActiveToolTip_Card, ActiveToolTip_FailureModename);
            MultiLine_UpdateModeChart(tmpList);
            MultiLine_UpdateSummaryChart();
            //MultiLine_UpdateAllCharts(); //this needs to be changed w/ name of failure mode if it comes from that tooltip
        }

        public void ActivateMultiLine_SummaryCanvas(object sender, MouseButtonEventArgs e)
        {
            MultiLine_DeepDiveChartCanvas.Visibility = Visibility.Hidden;
            MultiLine_SummaryChartCanvas.Visibility = Visibility.Visible;
            MultiLineSummaryButton.Background = BrushColors.mybrushSelectedCriteria;
            MultiLineSummaryButton.Foreground = Brushes.White;
            MultiLineDeepDiveButton.Background = Brushes.White;
            MultiLineDeepDiveButton.Foreground = BrushColors.mybrushfontgray;
        }

        public void ActivateMultiLine_DeepDiveCanvas(object sender, MouseButtonEventArgs e)
        {
            //need to hide/unhide canvases
            MultiLine_DeepDiveChartCanvas.Visibility = Visibility.Visible;
            MultiLine_SummaryChartCanvas.Visibility = Visibility.Hidden;
            MultiLineDeepDiveButton.Background = BrushColors.mybrushSelectedCriteria;
            MultiLineDeepDiveButton.Foreground = Brushes.White;
            MultiLineSummaryButton.Background = Brushes.White;
            MultiLineSummaryButton.Foreground = BrushColors.mybrushfontgray;
        }

        public void CloseMultiLineSplashCanvas(object sender, MouseButtonEventArgs e)
        {
            MultiLineSplashCanvas.Visibility = Visibility.Hidden;
        }

        #endregion



        public void ToggleShowHide_RateTrainer(object sender, MouseButtonEventArgs e)
        {
            HideAllDashboards();
            RateOMeterCanvas.Visibility = Visibility.Visible;
            fork_onload();
        }

        #region fork


        public void fork_onload()
        {
            fork_generatecards();

        }


        public int fork_getcurrentCARDScount()
        {
            Canvas cvs;
            int j;
            DependencyObject dep = forkfeedcanvas;
            int count = 0;


            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cvs = (Canvas)VisualTreeHelper.GetChild(dep, j);
                    if (cvs.Name.Contains("Card"))   //Each card has the canvas name in the format Card#P  (# is number, P is alignment position - L(left), R, C)
                    {
                        count = count + 1;
                    }
                }

            }

            return count;
        }

        public void fork_generatecards()
        {
            int i;
            int currentnumberofcards = fork_getcurrentCARDScount();

            Canvas tempcanvas;
            fork_deleteallcards();
            for (i = 0; i <= currentnumberofcards - 1; i++)
            {
                GenerateCanvasUI(forkfeedcanvas, "Card" + i + "C", heightofcard, widthofcard, 0, i * (verticalgapbetweencards + heightofcard));
                tempcanvas = getMenuItem_Canvas_fromitemindex(forkfeedcanvas, -1, "Card" + i + "C");
                GenerateRectangleUI(tempcanvas, "Cardrect" + i + "C", heightofcard, widthofcard, 0, 0, Brushes.White, Brushes.LightSlateGray, 0.3, null, null, null);
                GenerateLabelUI(tempcanvas, "CardHeader" + i + "C", cardnamelabelheight, cardnamelabelwidth, LeftPoscardnamelabel, TopPoscardnamelabel, null, BrushColors.mybrushfontgray, 20, null, null, null, -1, "Card " + (i + 1), true);
                AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, tempcanvas);
                System.Windows.Forms.Application.DoEvents();
                Thread.Sleep(50);

            }
            if (i == currentnumberofcards - 1 && currentnumberofcards != 0) { i += 1; }
            if (currentnumberofcards == 0) { i = 0; }
            //Last dummy card
            GenerateCanvasUI(forkfeedcanvas, "Card" + i + "C", heightofcard, widthofcard, 0, i * (verticalgapbetweencards + heightofcard));
            tempcanvas = getMenuItem_Canvas_fromitemindex(forkfeedcanvas, -1, "Card" + i + "C");
            GenerateRectangleUI(tempcanvas, "Cardrect" + i + "C", heightofcard, widthofcard, 0, 0, Brushes.Black, Brushes.LightSlateGray, 0.3, null, null, null, 0, -1, 0.2);
            GenerateLabelUI(tempcanvas, "AddCard" + i + "C", 30, 100, tempcanvas.Width / 2 - 50, tempcanvas.Height / 2 - 15, BrushColors.mybrushSelectedCriteria, Brushes.White, 15, fork_AddCard, Generalmousemove, Generalmouseleave, -1, "Add Card");
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, tempcanvas);
            forkfeedcanvas.Height = (i + 1) * (heightofcard + verticalgapbetweencards + 50);

        }

        public void fork_AddCard(object sender, MouseButtonEventArgs e)
        {
            fork_generatecards();

        }


        public void fork_deleteallcards()
        {
            Canvas cvs;
            int j;
            Canvas dep = forkfeedcanvas;
            int count = 0;


            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cvs = (Canvas)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(cvs);


                }

            }


        }

        #endregion

        #region Raw Data Window


        public void CloseRawLossDataSplash(object sender, MouseButtonEventArgs e)
        {
            RawDataSplashCanvas.Visibility = Visibility.Hidden;
        }
        public void LaunchRawData(object sender, MouseButtonEventArgs e)
        {
            populateRawDataWindow(Tooltip_failuremodenamelabel.Content.ToString(), ActiveToolTip_Cardname);


            RawDataSplashCanvas.Visibility = Visibility.Visible;
            FloatingToolTipCanvas.Visibility = Visibility.Hidden;
            rawlossdata_failuremodename_label.Content = Tooltip_failuremodenamelabel.Content;
            AnimateZoomUIElement(0.1, 1.0, 0.3, OpacityProperty, RawDataSplashCanvas);
        }
        public void LossCompass_ExportRawData_Launch(object sender, MouseButtonEventArgs e)
        {
            RawDataExportOptionsCanvas.Visibility = Visibility.Visible;

        }

        public void LossCompass_ExportRawData_Close(object sender, MouseButtonEventArgs e)
        {


            RawDataExport(rawlossdata_listview);

            //hide the canvas
            RawDataExportOptionsCanvas.Visibility = Visibility.Hidden;
        }


        public void RawDataExport(object parameter)
        {
            var grid = parameter as Telerik.Windows.Controls.RadGridView;
            if (grid != null)
            {
                grid.ElementExporting -= this.ElementExporting;
                grid.ElementExporting += this.ElementExporting;

                string extension = "";
                var format = Telerik.Windows.Controls.ExportFormat.Html;

                string SelectedExportFormat = "Excel";

                if ((bool)Export_Excelradio.IsChecked)
                {
                    SelectedExportFormat = "Excel";
                    extension = "xls";
                    format = Telerik.Windows.Controls.ExportFormat.Html;
                }
                else if ((bool)Export_Wordradio.IsChecked)
                {
                    SelectedExportFormat = "Word";
                    extension = "doc";
                    format = Telerik.Windows.Controls.ExportFormat.Html;
                }
                else if ((bool)Export_CSVradio.IsChecked)
                {
                    SelectedExportFormat = "Csv";
                    extension = "csv";
                    format = Telerik.Windows.Controls.ExportFormat.Csv;
                }
                else
                {
                    SelectedExportFormat = "ExcelML";
                    extension = "xml";
                    format = Telerik.Windows.Controls.ExportFormat.ExcelML;
                }


                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.DefaultExt = extension;
                dialog.Filter = String.Format("{1} files (*.{0})|*.{0}|All files (*.*)|*.*", extension, SelectedExportFormat);
                dialog.FilterIndex = 1;

                if (dialog.ShowDialog() == true)
                {
                    using (var stream = dialog.OpenFile())
                    {
                        var exportOptions = new Telerik.Windows.Controls.GridViewExportOptions();
                        exportOptions.Format = format;
                        exportOptions.ShowColumnFooters = true;
                        exportOptions.ShowColumnHeaders = true;
                        exportOptions.ShowGroupFooters = true;
                        exportOptions.Encoding = System.Text.Encoding.Unicode;

                        grid.Export(stream, exportOptions);
                    }
                }
            }
        }

        private void ElementExporting(object sender, Telerik.Windows.Controls.GridViewElementExportingEventArgs e)
        {
            var htmlVisualExportParameters = e.VisualParameters as Telerik.Windows.Controls.GridViewHtmlVisualExportParameters;
            if (htmlVisualExportParameters != null)
            {
                if (e.Element == Telerik.Windows.Controls.ExportElement.HeaderRow || e.Element == Telerik.Windows.Controls.ExportElement.FooterRow
                    || e.Element == Telerik.Windows.Controls.ExportElement.GroupFooterRow)
                {
                    htmlVisualExportParameters.Background = Colors.LightGray;
                    htmlVisualExportParameters.Foreground = Colors.Black;
                    htmlVisualExportParameters.FontSize = 20;
                    htmlVisualExportParameters.FontWeight = FontWeights.Bold;
                }
                else if (e.Element == Telerik.Windows.Controls.ExportElement.Row)
                {
                    htmlVisualExportParameters.Background = Colors.White;
                    htmlVisualExportParameters.Foreground = Colors.Black;
                }
                else if (e.Element == Telerik.Windows.Controls.ExportElement.Cell &&
                    e.Value != null && e.Value.Equals("Chocolade"))
                {
                    htmlVisualExportParameters.FontFamily = new FontFamily("Verdana");
                    htmlVisualExportParameters.Background = Colors.LightGray;
                    htmlVisualExportParameters.Foreground = Colors.Blue;
                }
                else if (e.Element == Telerik.Windows.Controls.ExportElement.GroupHeaderRow)
                {
                    htmlVisualExportParameters.FontFamily = new FontFamily("Verdana");
                    htmlVisualExportParameters.Background = Colors.LightGray;
                    htmlVisualExportParameters.Height = 30;
                }
                else if (e.Element == Telerik.Windows.Controls.ExportElement.GroupHeaderCell &&
                    e.Value != null && e.Value.Equals("Chocolade"))
                {
                    e.Value = "MyNewValue";
                }
            }
        }




        #endregion

        #region Mapping
        public void CloseMappingSplash(object sender, MouseButtonEventArgs e)
        {
            MappingSplashCanvas.Visibility = Visibility.Hidden;

        }
        public void LaunchMappingSplash(object sender, MouseButtonEventArgs e)
        {
            MappingSplashCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.1, 1.0, 0.3, OpacityProperty, MappingSplashCanvas);
            Image tempsender = (Image)sender;

            if (tempsender.Name.Contains("TierA"))
            {
                MappingDestinationLabel.Content = "Loss Compass mapping fields for " + getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxA").Child, -1, "TierAHeader").Content;
                PopulateMappingCombo1(CardTier.A);
                PopulateMappingCombo2(CardTier.A);
                MappingOriginCard = CardTier.A;
            }
            if (tempsender.Name.Contains("TierB"))
            {
                MappingDestinationLabel.Content = "Loss Compass mapping fields for " + getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child, -1, "TierBHeader").Content;
                PopulateMappingCombo1(CardTier.B);
                PopulateMappingCombo2(CardTier.B);
                MappingOriginCard = CardTier.B;
            }
            if (tempsender.Name.Contains("TierC"))
            {
                MappingDestinationLabel.Content = "Loss Compass mapping fields for " + getMenuItem_Label_fromitemindex(getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child, -1, "TierCHeader").Content;
                PopulateMappingCombo1(CardTier.C);
                PopulateMappingCombo2(CardTier.C);
                MappingOriginCard = CardTier.C;
            }
        }

        public void PopulateMappingCombo1(CardTier cardname)
        {
            Mapping1_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(cardname);
            Mapping1_Combobox.SelectedItem = intermediate.LossCompass_GetMapping_A(cardname);
        }
        public void PopulateMappingCombo2(CardTier cardname)
        {
            Mapping2_Combobox.ItemsSource = intermediate.LossCompass_getMappingFieldList(cardname);
            Mapping2_Combobox.SelectedItem = intermediate.LossCompass_GetMapping_B(cardname);

        }
        public void Remapclicked(object sender, MouseButtonEventArgs e) //Generic Remapping function from any product
        {
            string tmpString;
            CloseMappingSplash(Mapping_Close_button, Publics.f);
            if (Mapping2_Combobox.SelectedItem == null) { tmpString = ""; } else { tmpString = Mapping2_Combobox.SelectedItem.ToString(); }

            if (MappingDestinationLabel.Content.ToString().Contains("Loss Compass"))
            {
                // if origin is Loss Compass
                intermediate.LossCompass_CardRemap(MappingOriginCard, Mapping1_Combobox.SelectedItem.ToString(), tmpString);
                InitiateChartsvalues();
            }
            else if (MappingDestinationLabel.Content.ToString().Contains("StopsWatch"))
            {
                intermediate.StopsWatch_ReMap(Mapping1_Combobox.SelectedItem.ToString(), tmpString);
                StopsWatchOnload(StopsWatchHeadericon, Publics.f);
            }
            else if (MappingDestinationLabel.Content.ToString().Contains("xSigma"))
            {
                intermediate.xSigma_CardRemap(Mapping1_Combobox.SelectedItem.ToString(), tmpString);

            }
            else if (MappingDestinationLabel.Content.ToString().Contains("PitStop"))
            {
                intermediate.PitStop_SU_ReMap(Mapping1_Combobox.SelectedItem.ToString(), tmpString);
                PitStopStartup_PopulateFailureModeList();
                PitStopRuntime_PopulateFailureModeList();
            }
            else if (MappingDestinationLabel.Content.ToString().Contains("Loss Network"))
            {
                intermediate.LossNetwork_ReMap(Mapping1_Combobox.SelectedItem.ToString(), tmpString);
                LossNetwork_onload(LossNetworkHeaderIcon, Publics.f);

            }
            else if (MappingDestinationLabel.Content.ToString().Contains("Gap Analysis"))
            {
                //intermediate.Gap_reMap_Level1(Mapping1_Combobox.SelectedItem.ToString(), tmpString);

            }
            AnimateZoomUIElement(0.8, 1.0, 0.1, OpacityProperty, LossCompass_MainChartsarea);
        }


        public void DrillDown_RemapClicked(object sender, MouseButtonEventArgs e)
        {
            Label tempsender = (Label)sender;
            string cardname = "";
            Canvas tempcanvas;
            ComboBox tempcombo;
            if (tempsender.Name.Contains("TierB"))
            {
                cardname = "B";
                tempcanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxB").Child;
                tempcanvas = getMenuItem_Canvas_fromitemindex(tempcanvas, -1, "TierB");
                tempcombo = getMenuItem_ComboBox_fromitemindex(tempcanvas, -1, "Combo");
                intermediate.LossCompass_CardRemap(CardTier.B, tempcombo.SelectedItem.ToString(), null);
                InitiateChartsvalues();
                tempcanvas.Visibility = Visibility.Hidden;
            }
            else if (tempsender.Name.Contains("TierC"))
            {
                cardname = "C";
                tempcanvas = (Canvas)getMenuItem_ViewBox_fromitemindex(LossCompass_MainChartsarea, -1, "ViewBoxC").Child;
                tempcanvas = getMenuItem_Canvas_fromitemindex(tempcanvas, -1, "TierC");
                tempcombo = getMenuItem_ComboBox_fromitemindex(tempcanvas, -1, "Combo");
                intermediate.LossCompass_CardRemap(CardTier.C, tempcombo.SelectedItem.ToString(), null);
                InitiateChartsvalues();
                tempcanvas.Visibility = Visibility.Hidden;
            }

        }
        public void Mapping1ComboBoxChanged(object sender, RoutedEventArgs e)
        {
            //PopulateMappingCombo2(MappingOriginCard);
        }

        #endregion

        #region Funnel Analysis

        public void LaunchFilter(object sender, MouseButtonEventArgs e)
        {
            FilterSplashCanvas.Visibility = Visibility.Visible;
            AnimateZoomUIElement(0.1, 1.0, 0.3, OpacityProperty, FilterSplashCanvas);
            Funnel_UpdateBarGraphs();
        }

        public void CloseFilterSplash(object sender, MouseButtonEventArgs e)
        {
            FilterSplashCanvas.Visibility = Visibility.Hidden;
        }
        public void ApplyFunnelClicked(object sender, MouseButtonEventArgs e)
        {
            //do filtering
            int i;
            List<string> funnelselecteditems = new List<string>();
            for (i = 0; i <= FunnellistBox.SelectedItems.Count - 1; i++)
            {
                funnelselecteditems.Add(FunnellistBox.SelectedItems[i].ToString());
            }


            intermediate.LossCompass_Funnel_ApplyFunnel(Funnel_Combobox.SelectedItem.ToString(), funnelselecteditems);
            FunnellistBox.SelectedIndex = -1;
            FunnelCentralCanvas.Visibility = Visibility.Hidden;
            AddFunnelButton.IsEnabled = true;
            AddFunnelButton.Background = BrushColors.mybrushFunnelBlue;
            GenerateFunnelSteps_Labels();
            InitiateChartsvalues();
            Generatetoplineresults_charts();
            InitiateHeadervalues();
            AnimateZoomUIElement(0.8, 1.0, 0.1, OpacityProperty, LossCompass_MainChartsarea);
            Funnel_UpdateBarGraphs();
        }

        public void Funnel_Stepremoved(object sender, MouseButtonEventArgs e)
        {
            int tempstepnumber = -1;
            if (sender.GetType().ToString().Contains("Image"))
            {
                Image tempsender = (Image)sender;
                tempstepnumber = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            }
            else
            {
                Label tempsender = (Label)sender;
                tempstepnumber = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            }

            intermediate.LossCompass_Funnel_ClearFilter(tempstepnumber - 1);  // zero based 

            //Animate the removal of a step canvas, before generating the canvases
            Canvas tempcanvas;

            int k;
            tempcanvas = getMenuItem_Canvas_fromitemindex(FunnelStepsCanvas, -1, "stepcanvas_" + tempstepnumber);
            Thickness tempmargin = tempcanvas.Margin;


            tempcanvas.Opacity = 0.9;
            FunnelStepRemovalAnimation(tempcanvas);
            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(300);

            FunnelStepsRemovalRefresh();

            Funnel_UpdateBarGraphs();
        }
        public void FunnelStepRemovalAnimation(Canvas tempcanvas)
        {
            //FunnelStepsCanvas.Children.Remove(tempcanvas);
            AnimateZoomUIElement((double)tempcanvas.GetValue(Canvas.LeftProperty), (double)tempcanvas.GetValue(Canvas.LeftProperty) + 400, 0.2, Canvas.LeftProperty, tempcanvas);
            AnimateZoomUIElement(0.9, 0.01, 0.2, OpacityProperty, tempcanvas);
            System.Windows.Forms.Application.DoEvents();
        }
        public void FunnelStepsRemovalRefresh()
        {

            GenerateFunnelSteps_Labels();
            InitiateChartsvalues();
            Generatetoplineresults_charts();
            InitiateHeadervalues();

        }
        public void PopulateFunnelCriteriaCombobox()
        {

            Funnel_Combobox.ItemsSource = intermediate.LossCompass_Funnel_GetListForFieldsThatCanBeFiltered();

        }
        public void FilterComboBoxChanged(object sender, RoutedEventArgs e)
        {
            FunnellistBox.ItemsSource = intermediate.LossCompass_Funnel_GetListOfAllItmesForGivenField(Funnel_Combobox.SelectedItem.ToString());
        }

        //more funnel stuff

        public void GenerateFunnelSteps_Labels()
        {


            Canvas dep = FunnelStepsCanvas;
            Canvas cvs;

            //first delete all canvases inside funnelstepscanvas
            while (VisualTreeHelper.GetChildrenCount(dep) != 0)
            {
                if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cvs = (Canvas)VisualTreeHelper.GetChild(dep, 0);
                    dep.Children.Remove(cvs);


                }

            }


            //then add canvases
            int i;
            int k;
            string funnelstepname = "stepcanvas_";
            string funnelstepimagename = "stepimage_";
            string funnelstepLabelHeadername = "stepLabelHeader_";
            string funnelstepLabelDesc = "stepLabelDesc_";
            string funnel_detailstepdesccontent = "";


            double funnelstepCanvasheight = 100;
            Canvas tempcanvas;
            for (i = 0; i <= intermediate.LossCompass_Funnel_NumberOfActiveFilters - 1; i++)
            {
                funnel_detailstepdesccontent = "";

                for (k = 0; k <= intermediate.LossCompass_Funnel_ActiveFilters[i].Count - 1; k++)
                {
                    funnel_detailstepdesccontent = funnel_detailstepdesccontent + " " + intermediate.LossCompass_Funnel_ActiveFilters[i][k].ToString();
                }


                //Canvas for each step
                GenerateCanvasUI(FunnelStepsCanvas, funnelstepname + (i + 1), funnelstepCanvasheight, 400, 0, 10 + (i * funnelstepCanvasheight));
                tempcanvas = getMenuItem_Canvas_fromitemindex(FunnelStepsCanvas, -1, funnelstepname + (i + 1));
                // Funnel image for each step
                GenerateImageUI(tempcanvas, funnelstepimagename + (i + 1), 60, 60, 0, 0, AppDomain.CurrentDomain.BaseDirectory + @"\FunnelBlue.png", null, null, null);

                //Header label for each step - STEP "N"
                GenerateLabelUI(tempcanvas, funnelstepLabelHeadername + (i + 1), 20, 300, 80, 10, null, Brushes.White, 14, null, null, null, -1, "Step " + (i + 1), true);
                //description of every filter items selected in each step
                GenerateTextBlockUI(tempcanvas, funnelstepLabelDesc + (i + 1), 65, 300, 80, 35, null, Brushes.White, 10, null, null, null, -1, funnel_detailstepdesccontent, true);

                //remove button and label for each step
                GenerateImageUI(tempcanvas, funnelstepimagename + "remove" + (i + 1), 11, 11, 150, 14, AppDomain.CurrentDomain.BaseDirectory + @"\RemoveIcon.png", Funnel_Stepremoved, null, null);
                GenerateLabelUI(tempcanvas, "RemoveLabel" + (i + 1), 20, 80, 168, 10, null, BrushColors.mybrushFunnelBlue, 10, Funnel_Stepremoved, null, null, -1, "Remove Step", true);

                //AnimateZoomUIElement((double)tempcanvas.GetValue(Canvas.LeftProperty) -100, (double)tempcanvas.GetValue(Canvas.LeftProperty), 0.2, Canvas.LeftProperty, tempcanvas);
                AnimateZoomUIElement(0.01, 1, 0.2, OpacityProperty, tempcanvas);


            }
            FunnelStepsCanvas.Height = (i) * 110;

        }
        public void AddFunnelClicked(object sender, MouseButtonEventArgs e)
        {
            FunnelCentralCanvas.Visibility = Visibility.Visible;
            AddFunnelButton.IsEnabled = false;
            AddFunnelButton.Background = BrushColors.mybrushFunnelGray;
            PopulateFunnelCriteriaCombobox();
        }
        public void CancelFunnelClicked(object sender, MouseButtonEventArgs e)
        {
            FunnelCentralCanvas.Visibility = Visibility.Hidden;
            AddFunnelButton.IsEnabled = true;
            AddFunnelButton.Background = BrushColors.mybrushFunnelBlue;
        }
        #endregion

        #region FloatingTooltip
        public void CloseFloaterToolTip(object sender, MouseButtonEventArgs e)
        {
            FloatingToolTipCanvas.Visibility = Visibility.Hidden;
        }
        public void FloatingToolTipMouseMove(object sender, MouseEventArgs e)
        {
            FloatingToolTipCanvas.Visibility = Visibility.Visible;
        }
        public void FloatingToolTipMouseLeave(object sender, MouseEventArgs e)
        {
            FloatingToolTipCanvas.Visibility = Visibility.Hidden;
        }

        public void ManageFloatingToolTip_forMultiline()
        {
            if (IsMultiLineActive == false)
            {
                TooltipTrendsbtnBorder.SetValue(Canvas.TopProperty, (double)104);
                TooltipRawDatabtnBorder.SetValue(Canvas.TopProperty, (double)104);
                TooltipMultilinebtnBorder.Visibility = Visibility.Hidden;
                TooltipsecondaryBorder.SetValue(Canvas.TopProperty, (double)100);
                TooltipsecondaryBorder.Height = 32;
                LossCompass_SparkChart.Height = 60;
            }
            else
            {
                TooltipTrendsbtnBorder.SetValue(Canvas.TopProperty, (double)78);
                TooltipRawDatabtnBorder.SetValue(Canvas.TopProperty, (double)78);
                TooltipMultilinebtnBorder.Visibility = Visibility.Visible;
                TooltipsecondaryBorder.SetValue(Canvas.TopProperty, (double)75);
                LossCompass_SparkChart.Height = 45;
                TooltipsecondaryBorder.Height = 58;

            }


        }

        #endregion

        #region Launch Canvas

        #region PRE-DOANALYZE


        Thread importTargetsThread;
        Array tmpProdArray;


        #region "Sector/Site Selection"
        BusinessUnit tmpSector;
        productionSite tmpSite;
        List<int> activeLineIndeces = new List<int>();
        bool PROF_connectionError;

        #endregion

        #region Initialization
        private void CheckScreenResolution()
        {

            Double screenWidth = SystemParameters.PrimaryScreenWidth;
            Double screenHeight = SystemParameters.PrimaryScreenHeight;

            if (screenWidth < 1200 | screenHeight < 700)

                this.WindowState = System.Windows.WindowState.Maximized;
        }

        private void MainCanvasClicked_EventDetected(object sender, MouseButtonEventArgs e) { }
        private void BGImageClicked_EventDetected(object sender, EventArgs e)
        {
            HideCalendars(true, true);
            HideQuickPickMenu(MainCanvasDummyRectangle, Publics.g);
        }
        private void BGImageMouseMove_EventDetected(object sender, EventArgs e)
        {
            HideQuickPickMenu(MainCanvasDummyRectangle, Publics.g);
        }
        private void LaunchStartCalendar(object sender, MouseButtonEventArgs e)
        {
            SetStartandEndTime();
            if (StartDateCanvas.Visibility == Visibility.Visible)
            {
                StartDateCanvas.Visibility = Visibility.Hidden;
            }
            else
            {
                StartDateCanvas.Visibility = Visibility.Visible;
            }

            HideCalendars(false, true);


        }
        private void LaunchEndCalendar(object sender, MouseButtonEventArgs e)
        {
            SetStartandEndTime();
            if (EndDateCanvas.Visibility == Visibility.Visible)
            {
                EndDateCanvas.Visibility = Visibility.Hidden;
            }
            else
            {
                EndDateCanvas.Visibility = Visibility.Visible;
            }
            HideCalendars(true, false);
        }
        private void StartCalendarClicked(object sender, SelectionChangedEventArgs e)
        {
            StartDateCanvas.Visibility = Visibility.Hidden;
            StartDateNewLabel.Content = ((DateTime)(StartDateNewCalendar.SelectedDate)).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
            fork_datepicker_startdate.SelectedDate = ((DateTime)(StartDateNewCalendar.SelectedDate)).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
        }
        private void EndCalendarClicked(object sender, SelectionChangedEventArgs e)
        {
            EndDateCanvas.Visibility = Visibility.Hidden;
            EndDateNewLabel.Content = ((DateTime)(EndDateNewCalendar.SelectedDate)).AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60)));
            fork_datepicker_enddate.SelectedDate = ((DateTime)(EndDateNewCalendar.SelectedDate)).AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60)));
        }

        private void HideCalendars(bool StartC = false, bool EndC = false)
        {
            if (StartC == true)
            {
                StartDateCanvas.Visibility = Visibility.Hidden;

            }
            if (EndC == true)
            {
                EndDateCanvas.Visibility = Visibility.Hidden;
            }
        }

        public void ShowQuickPickMenu(object sender, EventArgs e)
        {
            QuickDatePickCanvas.Visibility = Visibility.Visible;
        }

        public void HideQuickPickMenu(object sender, EventArgs e)
        {
            QuickDatePickCanvas.Visibility = Visibility.Hidden;
            QuickPickDropDownIcon.Visibility = Visibility.Visible;
        }
        public void ShowLaunchSplashCanvas()
        {
            LaunchSplashCanvas.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();
            Thread.Sleep(3000);
            LaunchSplashCanvas.Visibility = Visibility.Hidden;
        }

        public void MakeLaunchReady()
        {
            int indexOffset = 0;

            CheckScreenResolution();


            HideCalendars(true, true);
            HideQuickPickMenu(MainCanvasDummyRectangle, Publics.g);

            string tempString = null;
            importTargetsThread = new Thread(Import_CSV.CSV_readTargetsFile);

            Settings.Default.Reload();

            Initialization_Support.verifyFolderStructure();
            tempString = "";

            hideforksettings();


            HideDateSelectionAlert();
            initializeMenuTextFromLanguage();

            importTargetsThread.Start();

            fork_linedropdown.SelectedIndex = 0;

            if (Settings.Default.DefaultLineIndex > -1)
            {
                fork_linedropdown.SelectedIndex = Settings.Default.DefaultLineIndex - indexOffset;
            }

            HideLineDefaultQuery(LineDefaultNoButton, Publics.f);

            populatestartandendtimehourandmin();
            Settings.Default.AdvancedSettings_isAvailabilityMode = false;
            dateSelectionShortcut();
            HideAllDashboards();
            //  JSON.TXT.importAllRawData();
        }


        public void populatestartandendtimehourandmin()
        {
            int k = 0;

            for (k = 0; k <= 23; k++)
            {
                if (k > 9)
                {
                    starthour.Items.Add(Convert.ToString(k));
                    endhour.Items.Add(Convert.ToString(k));
                }
                else
                {
                    starthour.Items.Add("0" + Convert.ToString(k));
                    endhour.Items.Add("0" + Convert.ToString(k));
                }
            }


            for (k = 0; k <= 59; k++)
            {
                if (k > 9)
                {
                    startmin.Items.Add(Convert.ToString(k));
                    endmin.Items.Add(Convert.ToString(k));
                }
                else
                {
                    startmin.Items.Add("0" + Convert.ToString(k));
                    endmin.Items.Add("0" + Convert.ToString(k));
                }
            }


            starthour.SelectedValue = "06";
            endhour.SelectedValue = "06";
            startmin.SelectedValue = "00";
            endmin.SelectedValue = "00";

        }


        private ObservableCollection<ProductionUnit> activelines;

        public ObservableCollection<ProductionUnit> ActiveLines
        {
            get
            {
                if (activelines == null)
                {
                    activelines = new ObservableCollection<ProductionUnit>();
                    activelines.Add(new ProductionUnit("Data Center"));
                    activelines.Add(new ProductionUnit("Customer Success"));
                    activelines.Add(new ProductionUnit("Product Owner"));
                    activelines.Add(new ProductionUnit("Developer - C++"));
                    activelines.Add(new ProductionUnit("Developer - Low Code"));
                }
                return activelines;
            }
        }

        #endregion

        #region Settings
        public void hideforksettings()
        {

            forkSettingsForm.Visibility = Visibility.Hidden;

            fork_datepicker_startdate.Visibility = Visibility.Hidden;
            fork_datepicker_enddate.Visibility = Visibility.Hidden;

            forkstartdate_label.Visibility = Visibility.Hidden;
            forkenddate_label.Visibility = Visibility.Hidden;

        }

        public void launchforkdaterange()
        {
            SetStartandEndTime();
        }
        public void SetStartandEndTime()
        {
            if (fork_linedropdown.SelectedIndex == 0)
            {
                starthour.SelectedValue = "06";
                endhour.SelectedValue = "06";
                startmin.SelectedValue = "00";
                endmin.SelectedValue = "00";

                return;
            }
        }

        private Boolean settingsDONE_dates()
        {
            if ((fork_datepicker_enddate.SelectedDate == null) & (fork_datepicker_startdate.SelectedDate == null))
            {
                return false;

            }
            else if ((fork_datepicker_enddate.SelectedDate == null) | (fork_datepicker_startdate.SelectedDate == null))
            {

                MessageBox.Show("Date field cannot be left blank");
                return false;
            }


            if (fork_datepicker_startdate.SelectedDate == fork_datepicker_enddate.SelectedDate)
            {
                MessageBox.Show("Start date/time cannot be same as end date/time.");

                fork_datepicker_startdate.SelectedDate = null;
                fork_datepicker_enddate.SelectedDate = null;
                EndDateNewLabel.Content = "";
                return false;

            }



            ///Very Important Code - Temporarily Commented out // #LG Code
            //  fork_datepicker_startdate.SelectedDate = ((DateTime)(fork_datepicker_startdate.SelectedDate)).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
            // fork_datepicker_enddate.SelectedDate = ((DateTime)(fork_datepicker_enddate.SelectedDate)).AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60)));

            //fork_datepicker_startdate.SelectedDate = ((DateTime)(fork_datepicker_startdate.SelectedDate));//.AddHours((double) ((double) starthour.SelectedValue + (double) startmin.SelectedValue / 60) );
            //fork_datepicker_enddate.SelectedDate = ((DateTime)(fork_datepicker_enddate.SelectedDate));//.AddHours((double)((double)endhour.SelectedValue + (double)endmin.SelectedValue / 60));


            if (fork_datepicker_startdate.SelectedDate > fork_datepicker_enddate.SelectedDate)
            {
                MessageBox.Show("Start date/time cannot be later than end date/time.");

                return false;
            }


            if (fork_datepicker_enddate.SelectedDate > DateTime.Now)
            {
                MessageBox.Show("Future dates cannot be selected");

                return false;
            }
            if ((int)((TimeSpan)(fork_datepicker_startdate.SelectedDate - fork_datepicker_enddate.SelectedDate)).Days > 89)


            {
                MessageBox.Show("Sorry, we are not there yet. We are still working on getting fork work for date ranges greater than 90 days.");
                return false;
            }

            //fork_dateselectionLabel.Content = Format(fork_datepicker_startdate.SelectedDate.ToString("MMMM dd, yyyy HH:mm") & vbNewLine & Format(fork_datepicker_enddate.SelectedDate.ToString("MMMM dd, yyyy HH:mm") & vbNewLine
            Publics.starttimeselected = (DateTime)fork_datepicker_startdate.SelectedDate;
            Publics.endtimeselected = (DateTime)fork_datepicker_enddate.SelectedDate;

            //save dates and line selection
            //   Settings.Default.DefaultLineIndex = fork_linedropdown.SelectedIndex
            Settings.Default.LastStartDate = Publics.starttimeselected;
            Settings.Default.LastEndDate = Publics.endtimeselected;
            Settings.Default.areDatesSaved = true;
            return true;
        }
        private void settingsDONE_settings()
        {
            //if we need to make a change to how we are showing names then:

            Settings.Default.Save();
        }


        public void settingsDONE(object sender, MouseButtonEventArgs e)
        {
            HideDateSelectionAlert();
            settingsDONE_dates();
            settingsDONE_settings();

            hideforksettings();
        }


        #endregion

        #region "Mouse Move/Leave/Down"
        private void Generalmousemove(object sender, EventArgs e)
        {
            if (sender.GetType().ToString().EndsWith("Image"))
            {
                Image tempsender = (Image)sender;
                tempsender.Opacity = 0.8;
            }
            else
            {
                Control tempsender = (Control)sender;
                tempsender.Opacity = 0.8;
            }

        }

        private void Generalmouseleave(object sender, EventArgs e)
        {
            if (sender.GetType().ToString().EndsWith("Image"))
            {
                Image tempsender = (Image)sender;
                tempsender.Opacity = 1.0;
            }
            else
            {
                Control tempsender = (Control)sender;
                tempsender.Opacity = 1.0;
            }
        }


        private void IconMouseMove(object sender, EventArgs e)
        {
            Cursor = System.Windows.Input.Cursors.Hand;





            if (object.ReferenceEquals(sender, fork_linedropdown))
            {
                fork_linedropdown.Opacity = 0.9;
            }

        }


        private void IconMouseLeave(object sender, EventArgs e)
        {
            Cursor = System.Windows.Input.Cursors.Arrow;



            if (object.ReferenceEquals(sender, fork_linedropdown))
            {
                fork_linedropdown.Opacity = 0.9;
            }

        }

        private void forkmtdoption_MouseDown()
        {
            fork_datepicker_startdate.SelectedDate = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
            fork_datepicker_enddate.SelectedDate = DateTime.Today.AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60))); ;
            StartDateNewLabel.Content = DateTime.Today.AddDays(-DateTime.Today.Day + 1).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
            EndDateNewLabel.Content = DateTime.Today.AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60)));

        }

        private void forkmtdoption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideQuickPickMenu(QuickDates_MtD, Publics.g);
            //fork_datepicker_startdate.SelectedDate = DateTime.Today.ToString("MM") & "/01/" & DateTime.Today.ToString("yyyy")
            forkmtdoption_MouseDown();
        }
        private void forklast7daysoption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideQuickPickMenu(QuickDates_Last7Days, Publics.g);
            fork_datepicker_startdate.SelectedDate = DateTime.Today.AddDays(-7).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
            fork_datepicker_enddate.SelectedDate = DateTime.Today.AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60))); ;
            StartDateNewLabel.Content = DateTime.Today.AddDays(-7).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
            EndDateNewLabel.Content = DateTime.Today.AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60))); ;
        }
        private void forkyesterdayoption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideQuickPickMenu(QuickDates_Yesterday, Publics.g);
            if (DateTime.Now.Hour > 6)
            {
                fork_datepicker_startdate.SelectedDate = DateTime.Today.AddDays(-1).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
                fork_datepicker_enddate.SelectedDate = DateTime.Today.AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60))); ;
                StartDateNewLabel.Content = DateTime.Today.AddDays(-1).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
                EndDateNewLabel.Content = DateTime.Today.AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60))); ;
            }
            else
            {
                fork_datepicker_startdate.SelectedDate = DateTime.Today.AddDays(-2).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60)));
                fork_datepicker_enddate.SelectedDate = DateTime.Today.AddDays(-1).AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60))); ;
                StartDateNewLabel.Content = DateTime.Today.AddDays(-2).AddHours(((double)Convert.ToInt32(starthour.SelectedValue)) + (((double)Convert.ToInt32(startmin.SelectedValue) / 60))); ;
                EndDateNewLabel.Content = DateTime.Today.AddDays(-1).AddHours(((double)Convert.ToInt32(endhour.SelectedValue)) + (((double)Convert.ToInt32(endmin.SelectedValue) / 60))); ;
            }

        }




        #endregion

        #region "Progress Bar"
        private delegate void DelegateUpdateProgressBar();
        private void updateProgressBar_TimeDriven()
        {
            MainProgressBar.Visibility = Visibility.Visible;

            var _with5 = MainProgressBar;
            _with5.Value = 0;
            while (_with5.Value < _with5.Maximum - 2 & !PROF_connectionError)
            {
                _with5.Value += 1;
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }
            _with5.Visibility = Visibility.Hidden;
            //System.Windows.Forms.Application.DoEvents()
        }

        private delegate void DelegateUpdateProgressBar_Fast();
        private void updateProgressBar_TimeDriven_Fast()
        {
            MainProgressBar.Visibility = Visibility.Visible;

            var _with6 = MainProgressBar;
            _with6.Value = 25;
            System.Threading.Thread.Sleep(70);
            _with6.Value = 60;
            while (_with6.Value < _with6.Maximum - 2 & !PROF_connectionError)
            {
                _with6.Value += 10;
                System.Windows.Forms.Application.DoEvents();
                System.Threading.Thread.Sleep(150);
            }
            _with6.Visibility = Visibility.Hidden;
            //System.Windows.Forms.Application.DoEvents()
        }
        #endregion
        #endregion

        #region Do Analyze
        public void Do_Analyze(object sender, MouseButtonEventArgs e)
        {
            HideCalendars(true, true);
            if (!settingsDONE_dates())
            {
                return;
            }
            if (prepareForDoAnalyze())
            {
                //UI elements
                dashboardwindow Dashboard;
                //Dashboard_Intermediate IntermediateSheet;

                //everything else
                DateTime _startTime = default(DateTime);
                DateTime _endTime = default(DateTime);
                productionLine lineToAnalyze = default(productionLine);
                List<DTevent> rawDataList;
                Settings.Default.AdvancedSettings_isAvailabilityMode = true;
                PROF_connectionError = false;

                ReInitializeAllPublicVariables();

                //Initialize The Data Engine
                _endTime = (DateTime)fork_datepicker_enddate.SelectedDate;
                _startTime = _endTime.AddDays(-180);
                string selectedName = fork_linedropdown.Items[fork_linedropdown.SelectedIndex].ToString();

                //Downtime Data
                rawDataList = DemoMode.getDemoData(_endTime, selectedName);

                // #MULTILINE
                //Downtime Interface
                CurrentEndTime = _endTime;

                LineNameLabel.Content = selectedName;

                CurrentLineConfig = new ProductionLines.LineConfig((int) Mappings.DTsched.GENERIC, (int) Mappings.fork.GENERIC, (int) Mappings.Shape.NoMappingAvailable, (int) Mappings.Format.NoMappingAvailables, "server", "password", "username");


                var DTinterface = new downtimeInterface(CurrentLineConfig, rawDataList);
                var DTreport = new SystemDowntimeReport(DTinterface);
                var SystemReport = new SystemSummaryReport(DTreport);

                /* Generate 'Single Production Unit View' Window */
                intermediate = new Dashboard_Intermediate_Single(SystemReport, selectedName, _endTime.AddDays(-2), _endTime);

                //initialize reports w/in intermediate sheet
                intermediate.initializeLossCompass();
                intermediate.initializeSigmaControl();
                intermediate.initializeTrends();
                LaunchCanvas.Visibility = Visibility.Hidden;

                //prepare UI
                MakeReportsReady();

                //Rate-o-Meter UI setup
                RateMeter_InitializeFromRawData();
            }
        }
        DateTime CurrentEndTime;
        ProductionLines.LineConfig CurrentLineConfig;
        #endregion

        #region POST_DOANALYZE
        private bool prepareForDoAnalyze()
        {
            string msgTextTmp = null;
            Publics.selectedindexofLine_temp = fork_linedropdown.SelectedIndex;


            //handles user generated  no-date selection error
            if ((fork_datepicker_enddate.SelectedDate == null) | (fork_datepicker_startdate.SelectedDate == null))
            {

                launchforkdaterange();
                Publics.IsAnalyzeButtonClickSource_Analyze = true;

                DateSelection_Alert.Visibility = Visibility.Visible;
                return false;
            }

            return true;
        }

        #region "LanguageOptions"



        private void initializeMenuTextFromLanguage()
        {
            switch (Settings.Default.LanguageActive)
            {
                case (int)Globals.Lang.English:
                    tmpSector = new BusinessUnit("All Sectors");
                    tmpSite = new productionSite("All Sites", "", "", "", "", "");
                    fork_analyzeLabel.Content = "Analyze";

                    // forkstartdate_label.Content = "Start date & time";
                    // forkenddate_label.Content = "End date & time";


                    Set_Default_Line_Button.Content = "Set Selected Line as Default";
                    LineDefaultQueryLabel.Content = "Do you want fork to remember this line as the default selection?";
                    LineDefaultYesButton.Content = "Yes";
                    LineDefaultNoButton.Content = "No";
                    LineDefaultCancelButton.Content = "I'll decide later";


                    break;

            }

        }

        #endregion

        public void ReInitializeAllPublicVariables()
        {
            //   Publics.AllProductionLines[Publics.selectedindexofLine_temp].isFilterByBrandcode = false;

            PROF_connectionError = false;
        }

        private void HideLineDefaultQuery(object sender, MouseButtonEventArgs e)
        {
            LineDefaultQueryLabel.Visibility = Visibility.Hidden;
            LineDefaultYesButton.Visibility = Visibility.Hidden;
            LineDefaultNoButton.Visibility = Visibility.Hidden;
            LineDefaultCancelButton.Visibility = Visibility.Hidden;
        }
        private void ShowLineDefaultQuery()
        {
            LineDefaultQueryLabel.Visibility = Visibility.Visible;
            LineDefaultYesButton.Visibility = Visibility.Visible;
            LineDefaultNoButton.Visibility = Visibility.Visible;
            LineDefaultCancelButton.Visibility = Visibility.Visible;
        }

        private void dateSelectionShortcut(object sender, MouseButtonEventArgs e)
        {
            //  launchforksettings();
            launchforkdaterange();
            //  forkmtdoption_MouseDown(forkmtdoption, Publics.f);
            // settingsDONE(forkGoButton, Publics.f);
        }
        private void dateSelectionShortcut()
        {
            //  launchforksettings();
            launchforkdaterange();
            // forkmtdoption_MouseDown(forkmtdoption, Publics.f);
            // settingsDONE(forkGoButton, Publics.f);
        }

        #region "Saving / Importing Default Settings"

        private void LineDefaultYesClicked(object sender, MouseButtonEventArgs e)
        {

            if (fork_linedropdown.SelectedIndex > 0)
            {
                Settings.Default.DefaultLineIndex = fork_linedropdown.SelectedIndex;
                Settings.Default.WantstoSetDefaultLine = false;
                if (LineDefaultQueryLabel.Visibility == Visibility.Hidden)

                    Settings.Default.Save();
                HideLineDefaultQuery(LineDefaultNoButton, Publics.f);
            }
            else
            {

                MessageBox.Show("No line selected");

            }
        }

        private void HideDateSelectionAlert()
        {
            DateSelection_Alert.Visibility = Visibility.Hidden;
        }
        #endregion
        #endregion

        #endregion

        #region Settings
        public void CloseSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsCanvas.Visibility = Visibility.Hidden;
        }
        public void LaunchSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsCanvas.Visibility = Visibility.Visible;
            HideAllSettingsPages();

        }
        public void GoBacktoSettingsIconPage(object sender, MouseButtonEventArgs e)
        {
            HideAllSettingsPages();

        }

        public void HideAllSettingsPages()
        {
            SettingsBack.Visibility = Visibility.Hidden;
            GeneralSettingsCanvas.Visibility = Visibility.Hidden;
            LossCompassSettingsCanvas.Visibility = Visibility.Hidden;
            xSigmaSettingsCanvas.Visibility = Visibility.Hidden;
            TrendsSettingsCanvas.Visibility = Visibility.Hidden;
            PitStopSettingsCanvas.Visibility = Visibility.Hidden;
            LossNetworkSettingsCanvas.Visibility = Visibility.Hidden;
            LiveLineSettingsCanvas.Visibility = Visibility.Hidden;
            StopsWatchSettingsCanvas.Visibility = Visibility.Hidden;
            RateTrainerSettingsCanvas.Visibility = Visibility.Hidden;

        }
        public void LaunchGeneralSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsBack.Visibility = Visibility.Visible;
            GeneralSettingsCanvas.Visibility = Visibility.Visible;
        }
        public void LaunchLossCompassSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsBack.Visibility = Visibility.Visible;
            LossCompassSettingsCanvas.Visibility = Visibility.Visible;
        }
        public void LaunchxSigmaSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsBack.Visibility = Visibility.Visible;
            xSigmaSettingsCanvas.Visibility = Visibility.Visible;
        }
        public void LaunchTrendsSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsBack.Visibility = Visibility.Visible;
            TrendsSettingsCanvas.Visibility = Visibility.Visible;
        }
        public void LaunchPitStopSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsBack.Visibility = Visibility.Visible;
            PitStopSettingsCanvas.Visibility = Visibility.Visible;
        }
        public void LaunchLossNetworkSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsBack.Visibility = Visibility.Visible;
            LossNetworkSettingsCanvas.Visibility = Visibility.Visible;
        }
        public void LaunchStopsWatchSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsBack.Visibility = Visibility.Visible;
            StopsWatchSettingsCanvas.Visibility = Visibility.Visible;
        }
        public void LaunchLiveLineSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsBack.Visibility = Visibility.Visible;
            LiveLineSettingsCanvas.Visibility = Visibility.Visible;
        }
        public void LaunchRateTrainerSettings(object sender, MouseButtonEventArgs e)
        {
            SettingsBack.Visibility = Visibility.Visible;
            RateTrainerSettingsCanvas.Visibility = Visibility.Visible;
        }

        public void SettingsIconMouseMove(object sender, MouseEventArgs e)
        {
            Rectangle tempsender = (Rectangle)sender;

        }


        #endregion

        #region HelpCanvas
        public void LaunchHelp(object sender, MouseButtonEventArgs e)
        {
            HelpCanvas.Visibility = Visibility.Visible;
            HideAllHelpCanvas();
            Image tempsender = (Image)sender;
            if (LossCompassCanvas.Visibility == Visibility.Visible)
            {
                LossCompassHelpCanvas.Visibility = Visibility.Visible;
            }
            else if (ChronicSporadicCanvas.Visibility == Visibility.Visible)
            {
                xSigmaHelpCanvas.Visibility = Visibility.Visible;
            }
            else if (TrendsCanvas.Visibility == Visibility.Visible)
            {
                TrendsHelpCanvas.Visibility = Visibility.Visible;
            }
            else if (StopsWatchCanvas.Visibility == Visibility.Visible)
            {
                StopsWatchHelpCanvas.Visibility = Visibility.Visible;
            }
            else if (LiveLineCanvas.Visibility == Visibility.Visible)
            {
                LiveLineHelpCanvas.Visibility = Visibility.Visible;
            }
            else if (LossNetworkCanvas.Visibility == Visibility.Visible)
            {
                LossNetworkHelpCanvas.Visibility = Visibility.Visible;
            }
            else if (RateOMeterCanvas.Visibility == Visibility.Visible)
            {
                RateTrainerHelpCanvas.Visibility = Visibility.Visible;
            }
            else if (PitStopCanvas.Visibility == Visibility.Visible)
            {
                PitStopHelpCanvas.Visibility = Visibility.Visible;
            }



        }
        public void CloseHelpCanvas(object sender, MouseButtonEventArgs e)
        {
            HelpCanvas.Visibility = Visibility.Hidden;
        }
        public void HideAllHelpCanvas()
        {
            LossCompassHelpCanvas.Visibility = Visibility.Hidden;
            xSigmaHelpCanvas.Visibility = Visibility.Hidden;
            TrendsHelpCanvas.Visibility = Visibility.Hidden;
            LiveLineHelpCanvas.Visibility = Visibility.Hidden;
            RateTrainerHelpCanvas.Visibility = Visibility.Hidden;
            StopsWatchHelpCanvas.Visibility = Visibility.Hidden;
            PitStopHelpCanvas.Visibility = Visibility.Hidden;
            LossNetworkHelpCanvas.Visibility = Visibility.Hidden;
        }
        #endregion


        #region CANVASyourresultsyourway

        #region CANVASgeneralfunctions

        public void LaunchCANVASyourresultsyourway(object sender, MouseButtonEventArgs e)
        {
            MenuCanvas.Visibility = Visibility.Hidden;
            HideAllDashboards();
            CANVASyourresultsyourway.Visibility = Visibility.Visible;

            if (IsCanvasOn == false && IsCanvasLaunchedFirstTime == true)
            {
                IsCanvasLaunchedFirstTime = false;
                WelcomeCanvas.Visibility = Visibility.Visible;
                GuidanceCanvas.Visibility = Visibility.Hidden;
                BlurBitmapEffect myBlurEffect = new BlurBitmapEffect();


                // Set the Radius property of the blur. This determines how 
                // blurry the effect will be. The larger the radius, the more
                // blurring. 
                myBlurEffect.Radius = 10;

                // Set the KernelType property of the blur. A KernalType of "Box"
                // creates less blur than the Gaussian kernal type.
                myBlurEffect.KernelType = KernelType.Gaussian;

                ResultsCanvas_GraphicsArea.BitmapEffect = myBlurEffect;
            }
            else
            {
                WelcomeCanvas.Visibility = Visibility.Hidden;
                GuidanceCanvas.Visibility = Visibility.Hidden;
                ResultsCanvas_GraphicsArea.Visibility = Visibility.Visible;
            }
        }
        public void CanvasGetStarted(object sender, MouseButtonEventArgs e)
        {
            WelcomeCanvas.Visibility = Visibility.Hidden;
            GuidanceCanvas.Visibility = Visibility.Visible;
            ResultsCanvas_GraphicsArea.BitmapEffect = null;
        }

        public void CreateNewCanvas(object sender, MouseButtonEventArgs e)
        {
            IsCanvasOn = true;
            LastPickedCanvas = 0;
            ActivateAllCanvasPickers();
        }

        public void LaunchMenu_fromCanvasyourresultsyourway(object sender, MouseButtonEventArgs e)
        {
            LaunchMenu(null, Publics.f);
            CreateNewCanvas(null, Publics.f);
            CANVASPickerONOFFToggleClicked(null, Publics.f);
        }

        public void PickCanvas(object sender, MouseButtonEventArgs e)
        {
            if (LastPickedCanvas < 32)
            {
                double defaultXpos = 50;
                double defaultYpos = 50;
                double XYposoffset = 20;
                Image tempsender = (Image)sender;
                tempsender.Visibility = Visibility.Hidden;
                Canvas dep = (Canvas)tempsender.Parent;

                //Show Captured on Canvas Message
                if (getMenuItem_Label_fromitemindex(dep, -1, "", "CapturedMessage") == null)
                {
                    GenerateLabelUI(dep, "CapturedMessage", 30, 150, dep.Width / 2 - 75, dep.Height / 2 - 15, Brushes.Black, Brushes.White, 11, null, null, null, -1, "Captured on Canvas");
                    AnimateZoomUIElement(1.0, 0, 1.5, OpacityProperty, getMenuItem_Label_fromitemindex(dep, -1, "", "CapturedMessage"));
                }
                else
                {
                    AnimateZoomUIElement(1.0, 0, 1.5, OpacityProperty, getMenuItem_Label_fromitemindex(dep, -1, "", "CapturedMessage"));
                }
                //


                try
                {
                    //Creating Image FIle and Saving with a filename in Public
                    CreateBitmapFromVisual(dep, "CanvasPick" + (LastPickedCanvas + 1));
                    Canvas dep2 = ResultsCanvas_GraphicsArea;

                    System.Drawing.Image imgfile = System.Drawing.Image.FromFile(@Globals.HTML.SERVER_FOLDER_PATH + "CanvasPick" + (LastPickedCanvas + 1) + ".png");

                    Image tmpimg = null;
                    tmpimg = getMenuItem_Image_fromitemindex(dep2, -1, "", "CanvasImage" + (LastPickedCanvas + 1));

                    //tmpimg.Source = new BitmapImage(new Uri(Globals.HTML.SERVER_FOLDER_PATH + "CanvasPick" + (LastPickedCanvas + 1), UriKind.Relative));

                    //Hooking up the just created image file with image control
                    tmpimg.Source = new BitmapImage(new Uri(Globals.HTML.SERVER_FOLDER_PATH + "CanvasPick" + (LastPickedCanvas + 1) + ".png"));
                    tmpimg.SetValue(Canvas.LeftProperty, defaultXpos + (LastPickedCanvas * XYposoffset));
                    tmpimg.SetValue(Canvas.TopProperty, defaultYpos + (LastPickedCanvas * XYposoffset));
                    tmpimg.Effect = null;
                    tmpimg.Height = imgfile.Height;
                    tmpimg.Width = imgfile.Width;
                    tmpimg.MouseMove += CanvasImageMouseMove;
                    tmpimg.MouseLeave += CanvasImageMouseLeave;
                    tmpimg.MouseDown += CanvasImageMouseDown;
                    LastPickedCanvas++;
                    tempsender.Visibility = Visibility.Visible;
                }
                catch (WebException ex)
                {
                }
                AnimateZoomUIElement(0.2, dep.Opacity, 0.5, OpacityProperty, dep);
            }

        }


        public void CanvasImage_ManageScroll()
        {
            Image tempsender;
            Canvas tempcnv;
            int i;
            Canvas dep = ResultsCanvas_GraphicsArea;
            for (i = 1; i <= 33; i++)
            {
                tempsender = getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasImage" + i);

                if ((double)tempsender.GetValue(Canvas.LeftProperty) + tempsender.Width > dep.Width)
                {
                    dep.Width = (double)tempsender.GetValue(Canvas.LeftProperty) + tempsender.Width + 100;

                }

                if ((double)tempsender.GetValue(Canvas.TopProperty) + tempsender.Height > dep.Height)
                {
                    dep.Height = (double)tempsender.GetValue(Canvas.TopProperty) + tempsender.Height + 100;

                }
            }
            /*
            for (i = 1; i <= LastInsertedAnnotation; i++)
            {
                tempcnv = getMenuItem_Canvas_fromitemindex(dep, -1, "", "InputTextCanvas" + i);

                if ((double)tempcnv.GetValue(Canvas.LeftProperty) + tempcnv.Width > dep.Width)
                {
                    dep.Width = (double)tempcnv.GetValue(Canvas.LeftProperty) + tempcnv.Width + 100;

        }

                if ((double)tempcnv.GetValue(Canvas.TopProperty) + tempcnv.Height > dep.Height)
                {
                    dep.Height = (double)tempcnv.GetValue(Canvas.TopProperty) + tempcnv.Height + 100;

                }
            }
            */
        }
        public void ActivateAllCanvasPickers()
        {

            Incontrol_Trends_CanvasPickerButton.Visibility = Visibility.Visible;
            Incontrol_Daily_CanvasPickerButton.Visibility = Visibility.Visible;
            Incontrol_Unplanned_rawData_CanvasPicker.Visibility = Visibility.Visible;
            Incontrol_ControlChart_CanvasPicker.Visibility = Visibility.Visible;
            xSigma_Planned_CanvasPicker.Visibility = Visibility.Visible;

            LossCompass_CanvasPickerButton.Visibility = Visibility.Visible;
            LossCompass_KPIs_CanvasPickerButton.Visibility = Visibility.Visible;
            LossCompass_Main_CanvasPickerButton.Visibility = Visibility.Visible;

            StopsWatch_HeatMap_CanvasPickerButton.Visibility = Visibility.Visible;
            StopsWatch_ClockDial_CanvasPickerButton.Visibility = Visibility.Visible;

            LossNetwork_CanvasPickerButton.Visibility = Visibility.Visible;

            PitStop_Runtime_CanvasPickerButton.Visibility = Visibility.Visible;
            PitStop_StartupMain_CanvasPickerButton.Visibility = Visibility.Visible;
            PitStop_StartupCarInfo_CanvasPickerButton.Visibility = Visibility.Visible;
            PitStop_StartupYellowFlagInfo_CanvasPickerButton.Visibility = Visibility.Visible;

            LiveLine_DTviewer_CanvasPickerButton.Visibility = Visibility.Visible;
            LiveLine_Trends_CanvasPickerButton.Visibility = Visibility.Visible;
            LiveLine_BiggestChanges_CanvasPickerButton.Visibility = Visibility.Visible;
            LiveLine_PlannedActivities_CanvasPickerButton.Visibility = Visibility.Visible;
            LiveLine_TopLosses_CanvasPickerButton.Visibility = Visibility.Visible;

            RateOMeter_Main_CanvasPickerButton.Visibility = Visibility.Visible;

            LineTrends_CanvasPickerButton.Visibility = Visibility.Visible;
            LossTrendsMode_CanvasPickerButton.Visibility = Visibility.Visible;
            StepChange_CanvasPickerButton.Visibility = Visibility.Visible;
        }
        public void DeActivateAllCanvasPickers()
        {

            Incontrol_Trends_CanvasPickerButton.Visibility = Visibility.Hidden;
            Incontrol_Daily_CanvasPickerButton.Visibility = Visibility.Hidden;
            Incontrol_Unplanned_rawData_CanvasPicker.Visibility = Visibility.Hidden;
            Incontrol_ControlChart_CanvasPicker.Visibility = Visibility.Hidden;
            xSigma_Planned_CanvasPicker.Visibility = Visibility.Hidden;

            LossCompass_CanvasPickerButton.Visibility = Visibility.Hidden;
            LossCompass_KPIs_CanvasPickerButton.Visibility = Visibility.Hidden;
            LossCompass_Main_CanvasPickerButton.Visibility = Visibility.Hidden;

            StopsWatch_HeatMap_CanvasPickerButton.Visibility = Visibility.Hidden;
            StopsWatch_ClockDial_CanvasPickerButton.Visibility = Visibility.Hidden;

            LossNetwork_CanvasPickerButton.Visibility = Visibility.Hidden;

            PitStop_Runtime_CanvasPickerButton.Visibility = Visibility.Hidden;
            PitStop_StartupMain_CanvasPickerButton.Visibility = Visibility.Hidden;
            PitStop_StartupCarInfo_CanvasPickerButton.Visibility = Visibility.Hidden;
            PitStop_StartupYellowFlagInfo_CanvasPickerButton.Visibility = Visibility.Hidden;

            LiveLine_DTviewer_CanvasPickerButton.Visibility = Visibility.Hidden;
            LiveLine_Trends_CanvasPickerButton.Visibility = Visibility.Hidden;
            LiveLine_BiggestChanges_CanvasPickerButton.Visibility = Visibility.Hidden;
            LiveLine_PlannedActivities_CanvasPickerButton.Visibility = Visibility.Hidden;
            LiveLine_TopLosses_CanvasPickerButton.Visibility = Visibility.Hidden;

            RateOMeter_Main_CanvasPickerButton.Visibility = Visibility.Hidden;

            LineTrends_CanvasPickerButton.Visibility = Visibility.Hidden;
            LossTrendsMode_CanvasPickerButton.Visibility = Visibility.Hidden;
            StepChange_CanvasPickerButton.Visibility = Visibility.Hidden;
        }
        public void CANVASPickerONOFFToggleClicked(object sender, MouseButtonEventArgs e)
        {
            IsCanvasOn = false;
            int temptogglepos = ToggleNow(CANVASPickerONOFFToggleframe, CANVASPickerONOFFToggleball);  // this is the function that does the toggle and returns the final position of the ball after the toggle
            if (temptogglepos == 0) // Zero means Toggle Ball is on the Left 
            {
                CANVASPickerOFFlabel.Foreground = BrushColors.mybrushSelectedCriteria;
                CANVASPickerONlabel.Foreground = BrushColors.mybrushLIGHTGRAY;
                IsCanvasOn = false;
                DeActivateAllCanvasPickers();

            }
            else if (temptogglepos == 1)  // One means Toggle Ball is on the right.
            {
                CANVASPickerOFFlabel.Foreground = BrushColors.mybrushLIGHTGRAY;
                CANVASPickerONlabel.Foreground = BrushColors.mybrushSelectedCriteria;
                IsCanvasOn = true;
                ActivateAllCanvasPickers();

            }

        }

        public void ExportCanvasClicked(object sender, MouseButtonEventArgs e)
        { }

        #endregion

        #region CanvasImage
        public void CanvasImageMouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.SizeAll;
            Image tempimage = (Image)sender;
            tempimage.Opacity = 0.9;


            int imagefileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempimage.Name.ToString()));
            Canvas dep = ResultsCanvas_GraphicsArea;
            tempimage = getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasImage" + imagefileno);
            if (getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasImageResizePlus" + imagefileno) == null)
            {
                GenerateImageUI(dep, "CanvasImageResizePlus" + imagefileno, 20, 20, (double)tempimage.GetValue(Canvas.LeftProperty), (double)tempimage.GetValue(Canvas.TopProperty), AppDomain.CurrentDomain.BaseDirectory + @"\ResizePlus.png", CanvasImageResize_or_DeleteButtonClicked, CanvasImageMouseMove, CanvasImageMouseLeave);
                GenerateImageUI(dep, "CanvasImageResizeMinus" + imagefileno, 20, 20, (double)tempimage.GetValue(Canvas.LeftProperty) + 30, (double)tempimage.GetValue(Canvas.TopProperty), AppDomain.CurrentDomain.BaseDirectory + @"\ResizeMinus.png", CanvasImageResize_or_DeleteButtonClicked, CanvasImageMouseMove, CanvasImageMouseLeave);
                GenerateImageUI(dep, "CanvasImageDelete" + imagefileno, 20, 20, (double)tempimage.GetValue(Canvas.LeftProperty) + 60, (double)tempimage.GetValue(Canvas.TopProperty), AppDomain.CurrentDomain.BaseDirectory + @"\DeleteButton.png", CanvasImageResize_or_DeleteButtonClicked, CanvasImageMouseMove, CanvasImageMouseLeave);
            }

            CanvasImage_ManageScroll();

            /*Rectangle BorderRect = null;
            DropShadowEffect imageshadow = new DropShadowEffect();
            imageshadow.BlurRadius = 10;
            imageshadow.ShadowDepth = 1;
            imageshadow.Color = Color.FromRgb(200, 200, 200);
            GenerateRectangleUI(dep, "CanvasImageBorder" + imagefileno, tempimage.Height, tempimage.Width, (double)tempimage.GetValue(Canvas.LeftProperty), (double)tempimage.GetValue(Canvas.TopProperty), Brushes.White, null, 0, null, null, null);
            BorderRect = getMenuItem_Rectangle_fromitemindex(dep, -1, "", "CanvasImageBorder" + imagefileno);
            BorderRect.Effect = imageshadow;
           BorderRect.SetValue(Canvas.ZIndexProperty, (int) tempimage.GetValue(Canvas.ZIndexProperty) -1);
           */
        }

        public void CanvasImageMouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            Image tempimage = (Image)sender;

            tempimage.Opacity = 1.0;
            Canvas dep = ResultsCanvas_GraphicsArea;
            int imagefileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempimage.Name.ToString()));
            dep.Children.Remove(getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasImageResizePlus" + imagefileno));
            dep.Children.Remove(getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasImageResizeMinus" + imagefileno));
            dep.Children.Remove(getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasImageDelete" + imagefileno));
            CanvasImage_ManageScroll();

            /*
             int imagefileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempimage.Name.ToString()));
             Canvas dep = ResultsCanvas_GraphicsArea;
             dep.Children.Remove(getMenuItem_Rectangle_fromitemindex(dep, -1, "", "CanvasImageBorder" + imagefileno));
         */
        }
        public void CanvasImageMouseDown(object sender, MouseEventArgs e)
        {

            CanvasImageMouseLeave(sender, e);
            CanvasImage_ManageScroll();
        }

        public void CanvasImageResize_or_DeleteButtonClicked(object sender, MouseButtonEventArgs e)
        {
            Image tempsender = (Image)sender;
            Image tempimage;
            double aspectratio = 0;
            int imagefileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            Canvas dep = ResultsCanvas_GraphicsArea;
            tempimage = getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasImage" + imagefileno);
            aspectratio = tempimage.Height / tempimage.Width;
            //tempsender is plus minus or delete buttom
            //tempimage is the screenshot of the chart itself


            if (tempsender.Name.Contains("Plus"))
            {
                if (tempimage.Height < 0.95 * dep.Height)
                {
                    tempimage.Height = tempimage.Height + 10;
                    tempimage.Width = tempimage.Height / aspectratio;
                }
            }
            else if (tempsender.Name.Contains("Minus"))
            {
                if (tempimage.Height > 10)
                {
                    tempimage.Height = tempimage.Height - 10;
                    tempimage.Width = tempimage.Height / aspectratio;
                }

            }
            else if (tempsender.Name.Contains("Delete"))
            {
                tempimage.Source = null;
                CanvasImageMouseLeave(tempimage, Publics.f);
            }

        }



        #endregion

        #region CanvasAnnotation
        public void CanvasInsertAnnotationClicked(object sender, MouseButtonEventArgs e)
        {
            Canvas dep = ResultsCanvas_GraphicsArea;
            Canvas Anno_dep = null;
            TextBox Anno_tb = null;
            int numberofmaxannotationtb = 3;
            int i;


            //cycle through all elements in ResultsCanvas
            foreach (UIElement child in dep.Children)
            {

                //if find a canvas
                if (child.GetType().ToString().Contains("Canvas"))
                {
                    Anno_dep = (Canvas)child;
                    //check if the canvas found is an annotation canvas
                    if (Anno_dep.Name.Contains("InputTextCanvas0"))
                    {
                        //temporarily change the name of canvas so that index is consistent
                        Anno_dep.Name = "InputTextCanvas" + (LastInsertedAnnotation + 1);
                        //then copy the xaml 
                        var xaml = System.Windows.Markup.XamlWriter.Save(child);
                        var deepCopy = System.Windows.Markup.XamlReader.Parse(xaml) as UIElement;
                        dep.Children.Add(deepCopy);
                        //change the name back
                        Anno_dep.Name = "InputTextCanvas0";
                        break;
                    }
                }
            }

            //Make the newly inserted canvas visible
            Anno_dep = getMenuItem_Canvas_fromitemindex(dep, -1, "", "InputTextCanvas" + (LastInsertedAnnotation + 1));
            Anno_dep.Visibility = Visibility.Visible;
            //AnimateZoomUIElement(80, (double) Anno_dep.GetValue(Canvas.TopProperty) + (LastInsertedAnnotation *5), 0.3, Canvas.TopProperty, Anno_dep);
            Anno_dep.SetValue(Canvas.TopProperty, (double)Anno_dep.GetValue(Canvas.TopProperty) + (LastInsertedAnnotation * 5));
            AnimateZoomUIElement(0.2, 1.0, 0.2, OpacityProperty, Anno_dep);
            TextBox tb = getMenuItem_TextBox_fromitemindex(Anno_dep, -1, "CanvasInputText");


            Anno_dep.MouseMove += CanvasAnnotationMouseMove;
            Anno_dep.MouseLeave += CanvasAnnotationMouseLeave;
            Anno_dep.MouseLeftButtonDown += CanvasAnnotationMouseDown;
            Anno_dep.MouseRightButtonDown += CanvasAnnotationToggleEnableDisable;
            tb.MouseRightButtonDown += CanvasAnnotationToggleEnableDisable;


            LastInsertedAnnotation++;

        }

        public void CanvasAnnotationMouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.SizeAll;
            Canvas tempCanvas = null;
            Image tempimage = null;
            Label templbl = null;
            int Canvasfileno = -1;
            //because plus, minus icons are images and the textbox is ina  canvas
            if (sender.GetType().ToString().Contains("Canvas"))
            {
                tempCanvas = (Canvas)sender;
                tempCanvas.Opacity = 0.9;
                Canvasfileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempCanvas.Name.ToString()));
            }
            else if (sender.GetType().ToString().Contains("Image"))
            {
                tempimage = (Image)sender;
                Canvasfileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempimage.Name.ToString()));
            }
            else if (sender.GetType().ToString().Contains("Label"))
            {
                templbl = (Label)sender;
                Canvasfileno = Convert.ToInt32(GlobalFcns.onlyDigits(templbl.Name.ToString()));
            }



            Canvas dep = ResultsCanvas_GraphicsArea;
            tempCanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "InputTextCanvas" + Canvasfileno);
            double Positionoffset = 0;
            if (getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasAnnotationResizePlusWidth" + Canvasfileno) == null)
            {

                GenerateImageUI(dep, "CanvasAnnotationResizePlusWidth" + Canvasfileno, 15, 15, (double)tempCanvas.GetValue(Canvas.LeftProperty) + Positionoffset, (double)tempCanvas.GetValue(Canvas.TopProperty) - 10, AppDomain.CurrentDomain.BaseDirectory + @"\ResizePlus.png", CanvasAnnotationResize_or_DeleteButtonClicked, CanvasAnnotationMouseMove, CanvasAnnotationMouseLeave);
                GenerateImageUI(dep, "CanvasAnnotationResizeMinusWidth" + Canvasfileno, 15, 15, (double)tempCanvas.GetValue(Canvas.LeftProperty) + 20 + Positionoffset, (double)tempCanvas.GetValue(Canvas.TopProperty) - 10, AppDomain.CurrentDomain.BaseDirectory + @"\ResizeMinus.png", CanvasAnnotationResize_or_DeleteButtonClicked, CanvasAnnotationMouseMove, CanvasAnnotationMouseLeave);
                GenerateImageUI(dep, "CanvasAnnotationDelete" + Canvasfileno, 15, 15, (double)tempCanvas.GetValue(Canvas.LeftProperty) + 40 + Positionoffset, (double)tempCanvas.GetValue(Canvas.TopProperty) - 10, AppDomain.CurrentDomain.BaseDirectory + @"\DeleteButton.png", CanvasAnnotationResize_or_DeleteButtonClicked, CanvasAnnotationMouseMove, CanvasAnnotationMouseLeave);
                GenerateLabelUI(dep, "CanvasAnnotationMoveHide" + Canvasfileno, 15, 50, (double)tempCanvas.GetValue(Canvas.LeftProperty) + 60 + Positionoffset, (double)tempCanvas.GetValue(Canvas.TopProperty) - 10, Brushes.Black, Brushes.White, 8, CanvasAnnotationMoveHideLabelClicked, CanvasAnnotationMouseMove, CanvasAnnotationMouseLeave, -1, "Edit");
                GenerateImageUI(dep, "CanvasAnnotationResizePlusHeight" + Canvasfileno, 15, 15, (double)tempCanvas.GetValue(Canvas.LeftProperty) - 7, (double)tempCanvas.GetValue(Canvas.TopProperty) + 20, AppDomain.CurrentDomain.BaseDirectory + @"\ResizePlus.png", CanvasAnnotationResize_or_DeleteButtonClicked, CanvasAnnotationMouseMove, CanvasAnnotationMouseLeave);
                GenerateImageUI(dep, "CanvasAnnotationResizeMinusHeight" + Canvasfileno, 15, 15, (double)tempCanvas.GetValue(Canvas.LeftProperty) - 7, (double)tempCanvas.GetValue(Canvas.TopProperty) + 40, AppDomain.CurrentDomain.BaseDirectory + @"\ResizeMinus.png", CanvasAnnotationResize_or_DeleteButtonClicked, CanvasAnnotationMouseMove, CanvasAnnotationMouseLeave);


            }

            //deterime move or hide

            if (getMenuItem_TextBox_fromitemindex(tempCanvas, -1, "CanvasInputText").IsEnabled == true)
            {
                getMenuItem_Label_fromitemindex(dep, -1, "", "CanvasAnnotationMoveHide" + Canvasfileno).Content = "Move";
            }
            else
            {
                getMenuItem_Label_fromitemindex(dep, -1, "", "CanvasAnnotationMoveHide" + Canvasfileno).Content = "Edit";
            }

            CanvasImage_ManageScroll();


        }

        public void CanvasAnnotationMouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;


            Canvas tempcanvas = null;
            Image tempimage = null;
            Label templbl = null;
            int Canvasfileno = -1;
            //because plus, minus icons are images and the textbox is ina  canvas
            if (sender.GetType().ToString().Contains("Canvas"))
            {
                tempcanvas = (Canvas)sender;
                tempcanvas.Opacity = 1.0;
                Canvasfileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempcanvas.Name.ToString()));
            }
            else if (sender.GetType().ToString().Contains("Image"))
            {
                tempimage = (Image)sender;
                Canvasfileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempimage.Name.ToString()));
            }
            else if (sender.GetType().ToString().Contains("Label"))
            {
                templbl = (Label)sender;
                Canvasfileno = Convert.ToInt32(GlobalFcns.onlyDigits(templbl.Name.ToString()));
            }

            Canvas dep = ResultsCanvas_GraphicsArea;

            dep.Children.Remove(getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasAnnotationResizePlusHeight" + Canvasfileno));
            dep.Children.Remove(getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasAnnotationResizeMinusHeight" + Canvasfileno));
            dep.Children.Remove(getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasAnnotationDelete" + Canvasfileno));
            dep.Children.Remove(getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasAnnotationResizePlusWidth" + Canvasfileno));
            dep.Children.Remove(getMenuItem_Image_fromitemindex(dep, -1, "", "CanvasAnnotationResizeMinusWidth" + Canvasfileno));
            dep.Children.Remove(getMenuItem_Label_fromitemindex(dep, -1, "", "CanvasAnnotationMoveHide" + Canvasfileno));
            CanvasImage_ManageScroll();

            /*
             int imagefileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempimage.Name.ToString()));
             Canvas dep = ResultsCanvas_GraphicsArea;
             dep.Children.Remove(getMenuItem_Rectangle_fromitemindex(dep, -1, "", "CanvasImageBorder" + imagefileno));
         */
        }

        public void CanvasAnnotationMouseDown(object sender, MouseButtonEventArgs e)
        {

            CanvasAnnotationMouseLeave(sender, e);
            CanvasImage_ManageScroll();

        }
        public void CanvasAnnotationResize_or_DeleteButtonClicked(object sender, MouseButtonEventArgs e)
        {
            Image tempsender = (Image)sender;
            Canvas tempCanvas;
            TextBox temptb;

            double aspectratio = 0;
            int Canvasfileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            Canvas dep = ResultsCanvas_GraphicsArea;
            tempCanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "InputTextCanvas" + Canvasfileno);

            temptb = getMenuItem_TextBox_fromitemindex(tempCanvas, -1, "CanvasInputText");
            aspectratio = tempCanvas.Height / tempCanvas.Width;
            //tempsender is plus minus or delete buttom
            //tempimage is the screenshot of the chart itself


            if (tempsender.Name.Contains("PlusHeight"))
            {
                if (tempCanvas.Height < 0.95 * dep.Height)
                {
                    tempCanvas.Height = tempCanvas.Height + 10;
                    temptb.Height = temptb.Height + 10;

                }
            }
            else if (tempsender.Name.Contains("MinusHeight"))
            {
                if (temptb.Height > 50)
                {
                    tempCanvas.Height = tempCanvas.Height - 10;
                    temptb.Height = temptb.Height - 10;

                }

            }
            else if (tempsender.Name.Contains("PlusWidth"))
            {
                if (tempCanvas.Width < 0.95 * dep.Width)
                {
                    tempCanvas.Width = tempCanvas.Width + 10;
                    temptb.Width = temptb.Width + 10;

                }
            }
            else if (tempsender.Name.Contains("MinusWidth"))
            {
                if (tempCanvas.Width > 50)
                {
                    tempCanvas.Width = tempCanvas.Width - 10;
                    temptb.Width = temptb.Width - 10;

                }

            }
            else if (tempsender.Name.Contains("Delete"))
            {

                CanvasAnnotationMouseLeave(tempCanvas, Publics.f);
                dep.Children.Remove(tempCanvas);
            }

        }
        public void CanvasAnnotationMoveHideLabelClicked(object sender, MouseButtonEventArgs e)
        {
            Label tempsender = (Label)sender;
            int Canvasfileno = Convert.ToInt32(GlobalFcns.onlyDigits(tempsender.Name.ToString()));
            Canvas dep = ResultsCanvas_GraphicsArea;
            Canvas tempCanvas = getMenuItem_Canvas_fromitemindex(dep, -1, "", "InputTextCanvas" + Canvasfileno);


            if (getMenuItem_TextBox_fromitemindex(tempCanvas, -1, "CanvasInputText").IsEnabled == true)
            {
                getMenuItem_Label_fromitemindex(dep, -1, "", "CanvasAnnotationMoveHide" + Canvasfileno).Content = "Move";
                getMenuItem_TextBox_fromitemindex(tempCanvas, -1, "CanvasInputText").IsEnabled = false;
            }
            else
            {
                getMenuItem_Label_fromitemindex(dep, -1, "", "CanvasAnnotationMoveHide" + Canvasfileno).Content = "Edit";
                getMenuItem_TextBox_fromitemindex(tempCanvas, -1, "CanvasInputText").IsEnabled = true;

            }

            CanvasAnnotationMouseDown(sender, e);
        }
        public void CanvasAnnotationToggleEnableDisable(object sender, MouseButtonEventArgs e)
        {

            TextBox tb = null;
            Canvas cv = null;
            if (sender.GetType().ToString().Contains("Canvas"))
            {
                cv = (Canvas)sender;
                tb = getMenuItem_TextBox_fromitemindex(cv, -1, "CanvasInputText");
            }
            else
            {
                tb = (TextBox)sender;
            }


            if (tb.IsEnabled == true)
            {
                tb.IsEnabled = false;
                Cursor = Cursors.SizeAll;
            }
            else
            {
                tb.IsEnabled = true;
                Cursor = Cursors.IBeam;

            }
        }

        #endregion


        #endregion

        #region General UI Functions

        #region Show/Hide stuff

        public void HideAllDashboards()
        {
            AssetStoryCanvas.Visibility = Visibility.Hidden;
            LossCompassCanvas.Visibility = Visibility.Hidden;
            TrendsCanvas.Visibility = Visibility.Hidden;
            ChronicSporadicCanvas.Visibility = Visibility.Hidden;
            StopsWatchCanvas.Visibility = Visibility.Hidden;
            PitStopCanvas.Visibility = Visibility.Hidden;
            LossNetworkCanvas.Visibility = Visibility.Hidden;
            LiveLineCanvas.Visibility = Visibility.Hidden;
            RateOMeterCanvas.Visibility = Visibility.Hidden;
            SettingsCanvas.Visibility = Visibility.Hidden;
            HelpCanvas.Visibility = Visibility.Hidden;
            CANVASyourresultsyourway.Visibility = Visibility.Hidden;
            GapAnalysisCanvas.Visibility = Visibility.Hidden;
        }
        #endregion

        //incontrol bar chart
        #region "GenericProgrammedbargraphs"

        public void createbargraphs(Boolean clearfirst, Canvas dep, int barcount, List<double> values, double maxvalue, SolidColorBrush rectcolor, MouseButtonEventHandler mousedownact, MouseEventHandler mousemoveact, MouseEventHandler mouseleaveact, string objectname)
        {

            Rectangle r;
            int j = 0;
            int i = 1;
            DateTime tempdate;
            int childrencount = VisualTreeHelper.GetChildrenCount(dep);

            //first clean all rectangles in the canvas if clearfirst = true
            if (clearfirst == true && childrencount != 0)
            {

                childrencount = VisualTreeHelper.GetChildrenCount(dep);


                while (VisualTreeHelper.GetChildrenCount(dep) != 0)
                {
                    if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Rectangle") > -1)
                    {
                        r = (Rectangle)VisualTreeHelper.GetChild(dep, 0);
                        dep.Children.Remove(r);
                    }

                }



            }
            // cleaning finished. deleted all rectangles inside the canvas





            // Now add rectangles//
            for (i = 1; i <= barcount; i++)
            {
                r = new Rectangle();
                dep.Children.Add(r);
                r.RenderTransform = new RotateTransform(180, 0, 0);
                r.Height = values[i - 1] * (dep.Height / maxvalue);
                r.Width = dep.Width / barcount;
                r.Stroke = Brushes.Gray;
                r.StrokeThickness = 0.1;
                r.Fill = rectcolor;
                r.Name = objectname + (i - 1);
                tempdate = intermediate.xSigma_Trend_Dates[i - 1];
                r.ToolTip = tempdate.ToString("MMM", CultureInfo.InvariantCulture) + " " + tempdate.ToString("dd", CultureInfo.InvariantCulture) + " " + tempdate.ToString("yyyy", CultureInfo.InvariantCulture);
                Canvas.SetLeft(r, (i) * (r.Width));
                Canvas.SetTop(r, (dep.Height));
                r.MouseDown += mousedownact;
                r.MouseMove += mousemoveact;
                r.MouseLeave += mouseleaveact;
                r.Cursor = Cursors.Hand;
                r.Opacity = 0;

            }
            //addition finished//
        }


        #endregion

        //Bubble charts
        #region GenericProgrammedBubblegraphs

        public void createbubblegraphs(Boolean clearfirst, Canvas dep, int bubblescount, List<double> CSscore, List<double> PRvalue, List<double> Stopsvalue, List<string> Lossnames, double maxBubbleHeight, double maxXvalue, double maxsizevalue, double maxYvalue, List<SolidColorBrush> bubblecolor, MouseButtonEventHandler mousedownact, MouseEventHandler mousemoveact, MouseEventHandler mouseleaveact, string objectname, double canvaswidth = 0)

        {
            Ellipse e;
            int i = 1;
            int j = 0;
            int childrencount = VisualTreeHelper.GetChildrenCount(dep);



            //first clean all ellipses in the canvas if clearfirst = true
            if (clearfirst == true && childrencount != 0)
            {

                while (VisualTreeHelper.GetChildrenCount(dep) != 0)
                {
                    if (VisualTreeHelper.GetChild(dep, 0).GetType().ToString().IndexOf("Ellipse") > -1)
                    {
                        e = (Ellipse)VisualTreeHelper.GetChild(dep, 0);
                        dep.Children.Remove(e);
                    }

                }




            }
            // cleaning finished. deleted all ellipses inside the canvas


            // Now add ellipses//
            for (i = 1; i <= bubblescount; i++)
            {
                e = new Ellipse();
                dep.Children.Add(e);

                e.Height = PRvalue[i - 1] * (maxBubbleHeight / maxsizevalue);
                e.Width = e.Height;
                e.Stroke = Brushes.Black;
                e.StrokeThickness = 0.0;
                e.Fill = bubblecolor[intermediate.xSigma_Daily_Color_Values[i - 1]]; //Stability
                e.Name = objectname + i;
                e.ToolTip = intermediate.xSigma_Daily_Names[i - 1] + System.Environment.NewLine + "DT%: " + Math.Round(intermediate.xSigma_Daily_Size_Values[i - 1], 1) + "%" + System.Environment.NewLine + "Stops: " + Math.Round(intermediate.xSigma_Daily_Yaxis_Values[i - 1]) + System.Environment.NewLine;
                e.MouseDown += mousedownact;
                e.MouseMove += mousemoveact;
                e.MouseLeave += mouseleaveact;
                e.Cursor = Cursors.Hand;
                Canvas.SetTop(e, (dep.Height - (Stopsvalue[i - 1] * dep.Height / maxYvalue)));
                e.Opacity = 0;  // this is done to enable animated effect of bubble appearing one after the other.

                if (canvaswidth != 0)
                {
                    Canvas.SetLeft(e, (CSscore[i - 1] * canvaswidth / maxXvalue) - e.Width);
                }
                else
                {
                    Canvas.SetLeft(e, (CSscore[i - 1] * dep.Width / maxXvalue) - e.Width);
                }

                //AnimateZoomUIElement(0.3, 1.0, 0.2, OpacityProperty, e);
            }
            //addition finished//
        }


        #endregion


        //Animation functions
        #region Animation
        public void AnimateZoomUIElement(double from, double to, double durn, DependencyProperty depprop, UIElement AnimatedObject)   // Standard animation function
        {
            var da = new DoubleAnimation();                         // da will contain the characteristics of the animation
            da.From = from;                                                     // position, where it starts 
            da.To = to;                                                         // position, where it ends
            da.Duration = new Duration(TimeSpan.FromSeconds(durn));             // how long animation lasts
            AnimatedObject.BeginAnimation(depprop, da);                         // Animate object is the subject we are playing with. And Depprop determines what type of UI element it is (rectangle, label, control.. etc)

        }
        public void AnimateZoomUIElement_Margin(Thickness from, Thickness to, double durn, DependencyProperty depprop, UIElement AnimatedObject)   // Standard animation function
        {
            var da = new ThicknessAnimation();                    // da will contain the characteristics of the animation

            da.From = from;                                                     // position, where it starts 
            da.To = to;                                                         // position, where it ends
            da.Duration = new Duration(TimeSpan.FromSeconds(durn));             // how long animation lasts
            AnimatedObject.BeginAnimation(depprop, da);                         // Animate object is the subject we are playing with. And Depprop determines what type of UI element it is (rectangle, label, control.. etc)

        }


        #endregion


        //Dynamic shapes / controls generation functions
        #region Generate Shapes Dynamically

        public void GenerateCanvasUI(Viewbox dep, string canvasname, double height, double width, double PosLeft, double PosTop, int Zindex = -1)
        {


            Canvas c;
            c = new Canvas();
            dep.Child = c;
            //dep.Children.Add(c);
            c.Height = height;
            c.Width = width;
            c.Name = canvasname;
            Canvas.SetLeft(c, PosLeft);
            Canvas.SetTop(c, PosTop);
            if (Zindex != -1)
            {
                Canvas.SetZIndex(c, Zindex);
            }

            //c.MouseDown += mousedownact;


        }
        public void GenerateCanvasUI(Canvas dep, string canvasname, double height, double width, double PosLeft, double PosTop, int Zindex = -1, SolidColorBrush canvascolor = null)
        {
            Canvas c;
            c = new Canvas();
            dep.Children.Add(c);
            //dep.Children.Add(c);
            c.Height = height;
            c.Width = width;
            c.Name = canvasname;
            Canvas.SetLeft(c, PosLeft);
            Canvas.SetTop(c, PosTop);
            if (Zindex != -1)
            {
                Canvas.SetZIndex(c, Zindex);
            }
            if (canvascolor != null)
            {
                c.Background = canvascolor;

            }
        }
        public void GenerateViewBoxUI(Canvas dep, string viewboxname, double height, double width, double PosLeft, double PosTop)
        {


            Viewbox v;
            v = new Viewbox();
            dep.Children.Add(v);
            v.Height = height;
            v.Width = width;
            v.Name = viewboxname;
            Canvas.SetLeft(v, PosLeft);
            Canvas.SetTop(v, PosTop);
            v.Stretch = Stretch.UniformToFill;
            v.MaxHeight = v.Height;
            v.MaxWidth = v.Width;

            //c.MouseDown += mousedownact;


        }
        public void GenerateRectangleUI(Canvas dep, string rectanglename, double height, double width, double PosLeft, double PosTop, SolidColorBrush rectcolor, SolidColorBrush rectborder, double strokethickness, MouseButtonEventHandler mousedownact, MouseEventHandler mousemoveact, MouseEventHandler mouseleaveact, double transformoriginangle = 0, int Zindex = -1, double opacity = 1.0, string tooltip = "", ScaleTransform transformmyscale = null)
        {

            Rectangle r;
            r = new Rectangle();
            dep.Children.Add(r);
            r.Height = height;
            r.Width = width;
            r.Name = rectanglename;
            Canvas.SetLeft(r, PosLeft);
            Canvas.SetTop(r, PosTop);
            if (rectcolor != null)
            {
                r.Fill = rectcolor;
            }

            r.Stroke = rectborder;
            r.StrokeThickness = strokethickness;
            r.Opacity = opacity;
            var myRotateTransform = new RotateTransform();
            myRotateTransform.Angle = transformoriginangle;


            if (transformmyscale != null)
            {


                TransformGroup trGrp;
                SkewTransform trSkw;
                RotateTransform trRot;
                TranslateTransform trTns;
                ScaleTransform trScl;

                myRotateTransform.CenterX = 0.5;
                myRotateTransform.CenterY = 0.5;
                //trSkw = new SkewTransform(0, 0);


                // trTns = new TranslateTransform(0, 0);
                trScl = transformmyscale;
                trRot = myRotateTransform;

                trGrp = new TransformGroup();
                // trGrp.Children.Add(trSkw);
                trGrp.Children.Add(trRot);
                // trGrp.Children.Add(trTns);
                trGrp.Children.Add(trScl);

                r.RenderTransform = trGrp;
            }
            else
            {
                r.RenderTransform = myRotateTransform;
            }


            if (Zindex != -1)
            {
                Canvas.SetZIndex(r, Zindex);
            }

            if (mousedownact != null)
            {
                r.MouseDown += mousedownact;

            }
            if (mousemoveact != null)
            {
                r.MouseMove += mousemoveact;

            }
            if (mouseleaveact != null)
            {
                r.MouseLeave += mouseleaveact;

            }
            if (tooltip != "")
            {
                r.ToolTip = tooltip;
            }


        }



        public void GenerateLineUI(Canvas dep, string linename, double X1, double Y1, double X2, double Y2, SolidColorBrush linecolor, double strokethickness, MouseButtonEventHandler mousedownact, MouseEventHandler mousemoveact, MouseEventHandler mouseleaveact, double transformoriginangle = 0, int Zindex = -1, double opacity = 1.0, string tooltip = "", ScaleTransform transformmyscale = null)
        {
            Line l;
            l = new Line();

            dep.Children.Add(l);
            l.X1 = X1;
            l.X2 = X2;
            l.Y1 = Y1;
            l.Y2 = Y2;
            l.Name = linename;
            if (linecolor != null)
            {
                l.Stroke = linecolor;
            }


            l.StrokeThickness = strokethickness;
            l.Opacity = opacity;
            var myRotateTransform = new RotateTransform();
            myRotateTransform.Angle = transformoriginangle;


            if (transformmyscale != null)
            {


                TransformGroup trGrp;
                SkewTransform trSkw;
                RotateTransform trRot;
                TranslateTransform trTns;
                ScaleTransform trScl;

                myRotateTransform.CenterX = 0.5;
                myRotateTransform.CenterY = 0.5;
                //trSkw = new SkewTransform(0, 0);


                // trTns = new TranslateTransform(0, 0);
                trScl = transformmyscale;
                trRot = myRotateTransform;

                trGrp = new TransformGroup();
                // trGrp.Children.Add(trSkw);
                trGrp.Children.Add(trRot);
                // trGrp.Children.Add(trTns);
                trGrp.Children.Add(trScl);

                l.RenderTransform = trGrp;
            }
            else
            {
                l.RenderTransform = myRotateTransform;
            }


            if (Zindex != -1)
            {
                Canvas.SetZIndex(l, Zindex);
            }

            if (mousedownact != null)
            {
                l.MouseDown += mousedownact;

            }
            if (mousemoveact != null)
            {
                l.MouseMove += mousemoveact;

            }
            if (mouseleaveact != null)
            {
                l.MouseLeave += mouseleaveact;

            }
            if (tooltip != "")
            {
                l.ToolTip = tooltip;
            }
        }

        public void GenerateEllipseUI(Canvas dep, string Ellipsename, double height, double width, double PosLeft, double PosTop, SolidColorBrush rectcolor, SolidColorBrush rectborder, double strokethickness, MouseButtonEventHandler mousedownact, MouseEventHandler mousemoveact, MouseEventHandler mouseleaveact, double transformoriginangle = 0, int Zindex = -1, double opacity = 1.0, string tooltip = "")
        {

            Ellipse elp;
            elp = new Ellipse();
            dep.Children.Add(elp);
            elp.Height = height;
            elp.Width = width;
            elp.Name = Ellipsename;
            Canvas.SetLeft(elp, PosLeft);
            Canvas.SetTop(elp, PosTop);
            if (rectcolor != null)
            {
                elp.Fill = rectcolor;
            }

            elp.Stroke = rectborder;
            elp.StrokeThickness = strokethickness;
            elp.Opacity = opacity;
            RotateTransform myRotateTransform = new RotateTransform();
            myRotateTransform.Angle = transformoriginangle;
            elp.RenderTransform = myRotateTransform;
            if (Zindex != -1)
            {
                Canvas.SetZIndex(elp, Zindex);
            }

            if (mousedownact != null)
            {
                elp.MouseDown += mousedownact;

            }
            if (mousemoveact != null)
            {
                elp.MouseMove += mousemoveact;

            }
            if (mouseleaveact != null)
            {
                elp.MouseLeave += mouseleaveact;

            }
            if (tooltip != "")
            {
                elp.ToolTip = tooltip;

            }

        }

        public void GenerateLabelUI(Canvas dep, string labelname, double height, double width, double PosLeft, double PosTop, SolidColorBrush labelfillcolor, SolidColorBrush labelfontcolor, double fontsize, MouseButtonEventHandler mousedownact, MouseEventHandler mousemoveact, MouseEventHandler mouseleaveact, int Zindex, string content = "", bool isleftaligned = false)
        {

            Label l;
            l = new Label();
            dep.Children.Add(l);
            l.Height = height;
            l.Width = width;
            l.Name = labelname;
            Canvas.SetLeft(l, PosLeft);
            Canvas.SetTop(l, PosTop);
            l.Background = labelfillcolor;
            l.Foreground = labelfontcolor;
            l.FontSize = fontsize;
            l.Cursor = Cursors.Hand;
            l.Padding = new Thickness(0.5, 0.5, 0.5, 0.5);
            l.VerticalContentAlignment = VerticalAlignment.Center;
            if (isleftaligned == false)
            {
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
            }
            else
            {
                l.HorizontalContentAlignment = HorizontalAlignment.Left;

            }
            if (Zindex != -1)
            {
                Canvas.SetZIndex(l, Zindex);
            }

            l.Content = content;
            //Canvas.SetZIndex(l, templabelZindex + i);
            //c.MouseDown += mousedownact;

            if (mousedownact != null)
            {
                l.MouseDown += mousedownact;

            }
            if (mousemoveact != null)
            {
                l.MouseMove += mousemoveact;

            }
            if (mouseleaveact != null)
            {
                l.MouseLeave += mouseleaveact;

            }


        }

        public void GenerateTextBlockUI(Canvas dep, string textblockname, double height, double width, double PosLeft, double PosTop, SolidColorBrush textblockfillcolor, SolidColorBrush textblockfontcolor, double fontsize, MouseButtonEventHandler mousedownact, MouseEventHandler mousemoveact, MouseEventHandler mouseleaveact, int Zindex, string content = "", bool isleftaligned = false)
        {
            TextBlock tb;
            tb = new TextBlock();
            dep.Children.Add(tb);
            tb.Height = height;
            tb.Width = width;
            tb.Name = textblockname;
            Canvas.SetLeft(tb, PosLeft);
            Canvas.SetTop(tb, PosTop);
            tb.Background = textblockfillcolor;
            tb.Foreground = textblockfontcolor;
            tb.FontSize = fontsize;
            tb.Cursor = Cursors.Hand;
            tb.Padding = new Thickness(0.5, 0.5, 0.5, 0.5);
            tb.TextWrapping = TextWrapping.Wrap;
            if (Zindex != -1)
            {
                Canvas.SetZIndex(tb, Zindex);
            }

            tb.Text = content;
            //Canvas.SetZIndex(l, templabelZindex + i);
            //c.MouseDown += mousedownact;

            if (mousedownact != null)
            {
                tb.MouseDown += mousedownact;

            }
            if (mousemoveact != null)
            {
                tb.MouseMove += mousemoveact;

            }
            if (mouseleaveact != null)
            {
                tb.MouseLeave += mouseleaveact;

            }


        }

        public void GenerateImageUI(Canvas dep, string Imagename, double height, double width, double PosLeft, double PosTop, string source, MouseButtonEventHandler mousedownact, MouseEventHandler mousemoveact, MouseEventHandler mouseleaveact, string tooltip = "", int Zindex = -1)
        {
            Image I;
            I = new Image();
            dep.Children.Add(I);
            I.Height = height;
            I.Width = width;
            I.Name = Imagename;
            Canvas.SetLeft(I, PosLeft);
            Canvas.SetTop(I, PosTop);
            try
            {
                I.Source = new BitmapImage(new Uri(source));
            }
            catch
            {
                int ixyz = 0;
            }

            I.Cursor = Cursors.Hand;
            if (mousedownact != null)
            {
                I.MouseDown += mousedownact;

            }
            if (mousemoveact != null)
            {
                I.MouseMove += mousemoveact;

            }
            if (mouseleaveact != null)
            {
                I.MouseLeave += mouseleaveact;

            }

            if (tooltip != "")
            {
                I.ToolTip = tooltip;
            }

            if (Zindex != -1)
            {
                Canvas.SetZIndex(I, Zindex);
            }


        }
        public void GenerateComboBoxUI(Canvas dep, string comboboxname, double height, double width, double PosLeft, double PosTop, List<string> source, RoutedEventHandler selectionchangedact)
        {
            ComboBox CB;
            CB = new ComboBox();
            dep.Children.Add(CB);
            CB.Height = height;
            CB.Width = width;
            CB.Name = comboboxname;
            Canvas.SetLeft(CB, PosLeft);
            Canvas.SetTop(CB, PosTop);
            CB.ItemsSource = source;
            CB.Cursor = Cursors.Hand;
            CB.FontSize = 12;
            CB.Foreground = BrushColors.mybrushfontgray;
            CB.Background = Brushes.White;
            CB.BorderBrush = null;
            CB.BorderThickness = new Thickness(0, 0, 0, 0);
            CB.HorizontalContentAlignment = HorizontalAlignment.Center;
        }


        #endregion


        //Finding needle in haystack functions - (get a control from within a canvas or a viewbox)
        #region Find UI Element in Canvas
        public Image getMenuItem_Image_fromitemindex_UNUSED(DependencyObject dep, int menuitemindex)
        {
            Image Img;
            Image sender = null;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Image") > -1)
                {
                    Img = (Image)VisualTreeHelper.GetChild(dep, j);

                    if (Img.Name.IndexOf(menuitemindex.ToString()) > -1)
                    {
                        sender = Img;
                    }
                }

            }


            return sender;
        }
        public Label getMenuItem_Label_fromitemindex_UNUSED(DependencyObject dep, int menuitemindex)
        {
            Label sender = null;
            Label lbl;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, j);

                    if (lbl.Name.IndexOf(menuitemindex.ToString()) > -1)
                    {
                        sender = lbl;
                    }
                }

            }

            return sender;
        }
        public Label getMenuItem_Label_fromitemindex(DependencyObject dep, int menuitemindex = -1, string stringitemindex = "", string exactstring = "")
        {
            Label sender = null;
            Label lbl;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Label") > -1)
                {
                    lbl = (Label)VisualTreeHelper.GetChild(dep, j);

                    if (stringitemindex == "" && menuitemindex != -1)
                    {
                        if (lbl.Name.IndexOf(menuitemindex.ToString()) > -1)
                        {
                            sender = lbl;
                        }
                    }
                    else
                    {
                        if (exactstring == "")
                        {
                            if (lbl.Name.IndexOf(stringitemindex) > -1)
                            {
                                sender = lbl;
                            }
                        }
                        else
                        {
                            if (lbl.Name.ToString() == exactstring)
                            {
                                sender = lbl;
                            }

                        }

                    }
                }

            }

            return sender;
        }
        public Image getMenuItem_Image_fromitemindex(DependencyObject dep, int menuitemindex = -1, string stringitemindex = "", string exactstring = "")
        {
            Image sender = null;
            Image img;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Image") > -1)
                {
                    img = (Image)VisualTreeHelper.GetChild(dep, j);

                    if (stringitemindex == "" && menuitemindex != -1)
                    {
                        if (img.Name.IndexOf(menuitemindex.ToString()) > -1)
                        {
                            sender = img;
                        }
                    }
                    else
                    {
                        if (exactstring == "")
                        {
                            if (img.Name.IndexOf(stringitemindex) > -1)
                            {
                                sender = img;
                            }
                        }
                        else
                        {
                            if (img.Name.ToString() == exactstring)
                            {
                                sender = img;
                            }

                        }
                    }
                }

            }

            return sender;
        }
        public Canvas getMenuItem_Canvas_fromitemindex(DependencyObject dep, int menuitemindex = -1, string stringitemindex = "", string exactstring = "")
        {
            Canvas sender = null;
            Canvas cnv;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Canvas") > -1)
                {
                    cnv = (Canvas)VisualTreeHelper.GetChild(dep, j);

                    if (stringitemindex == "" && menuitemindex != -1)
                    {
                        if (cnv.Name.IndexOf(menuitemindex.ToString()) > -1)
                        {
                            sender = cnv;
                        }
                    }
                    else
                    {
                        if (exactstring == "")
                        {
                            if (cnv.Name.IndexOf(stringitemindex) > -1)
                            {
                                sender = cnv;
                            }
                        }
                        else
                        {
                            if (cnv.Name.ToString() == exactstring)
                            {
                                sender = cnv;
                            }

                        }

                    }
                }

            }

            return sender;
        }
        public Viewbox getMenuItem_ViewBox_fromitemindex(DependencyObject dep, int menuitemindex = -1, string stringitemindex = "")
        {
            Viewbox sender = null;
            Viewbox vbx;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Viewbox") > -1)
                {
                    vbx = (Viewbox)VisualTreeHelper.GetChild(dep, j);

                    if (stringitemindex == "" && menuitemindex != -1)
                    {
                        if (vbx.Name.IndexOf(menuitemindex.ToString()) > -1)
                        {
                            sender = vbx;
                        }
                    }
                    else
                    {
                        if (vbx.Name.IndexOf(stringitemindex) > -1)
                        {
                            sender = vbx;
                        }

                    }
                }

            }

            return sender;
        }
        public Rectangle getMenuItem_Rectangle_fromitemindex(DependencyObject dep, int menuitemindex = -1, string stringitemindex = "", string exactstring = "")
        {
            Rectangle sender = null;
            Rectangle rect;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(dep, j);

                    if (stringitemindex == "" && menuitemindex != -1)
                    {
                        if (rect.Name.IndexOf(menuitemindex.ToString()) > -1)
                        {
                            sender = rect;
                        }
                    }
                    else
                    {
                        if (exactstring == "")
                        {
                            if (rect.Name.IndexOf(stringitemindex) > -1)
                            {
                                sender = rect;
                            }
                        }
                        else
                        {
                            if (rect.Name.ToString() == exactstring)
                            {
                                sender = rect;
                            }

                        }

                    }
                }

            }

            return sender;
        }
        public ComboBox getMenuItem_ComboBox_fromitemindex(DependencyObject dep, int menuitemindex = -1, string stringitemindex = "")
        {
            ComboBox sender = null;
            ComboBox combo;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("ComboBox") > -1)
                {
                    combo = (ComboBox)VisualTreeHelper.GetChild(dep, j);

                    if (stringitemindex == "" && menuitemindex != -1)
                    {
                        if (combo.Name.IndexOf(menuitemindex.ToString()) > -1)
                        {
                            sender = combo;
                        }
                    }
                    else
                    {
                        if (combo.Name.IndexOf(stringitemindex) > -1)
                        {
                            sender = combo;
                        }

                    }
                }

            }

            return sender;
        }

        public Ellipse getMenuItem_Ellipse_fromitemindex(DependencyObject dep, int menuitemindex = -1, string stringitemindex = "", string exactstring = "")
        {
            Ellipse sender = null;
            Ellipse elp;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("Ellipse") > -1)
                {
                    elp = (Ellipse)VisualTreeHelper.GetChild(dep, j);

                    if (stringitemindex == "" && menuitemindex != -1)
                    {
                        if (elp.Name.IndexOf(menuitemindex.ToString()) > -1)
                        {
                            sender = elp;
                        }
                    }
                    else
                    {
                        if (exactstring == "")
                        {
                            if (elp.Name.IndexOf(stringitemindex) > -1)
                            {
                                sender = elp;
                            }
                        }
                        else
                        {
                            if (elp.Name.ToString() == exactstring)
                            {
                                sender = elp;
                            }

                        }

                    }
                }

            }

            return sender;
        }

        public TextBox getMenuItem_TextBox_fromitemindex(DependencyObject dep, int menuitemindex = -1, string stringitemindex = "", string exactstring = "")
        {
            TextBox sender = null;
            TextBox elp;
            int j;
            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(dep) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(dep, j).GetType().ToString().IndexOf("TextBox") > -1)
                {
                    elp = (TextBox)VisualTreeHelper.GetChild(dep, j);

                    if (stringitemindex == "" && menuitemindex != -1)
                    {
                        if (elp.Name.IndexOf(menuitemindex.ToString()) > -1)
                        {
                            sender = elp;
                        }
                    }
                    else
                    {
                        if (exactstring == "")
                        {
                            if (elp.Name.IndexOf(stringitemindex) > -1)
                            {
                                sender = elp;
                            }
                        }
                        else
                        {
                            if (elp.Name.ToString() == exactstring)
                            {
                                sender = elp;
                            }

                        }

                    }
                }

            }

            return sender;
        }



        #endregion


        //Taking dynamic screenshots
        #region Screenshot
        public void TakeScreenshot(Canvas dep)
        {
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            Thickness depPos;
            depPos = dep.Margin;
            using (var bitmap = new System.Drawing.Bitmap((int)dep.Width, (int)dep.Height))
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
            {

                graphics.CopyFromScreen((int)depPos.Left, (int)depPos.Top, 0, 0, bitmap.Size);
                bitmap.Save(Globals.HTML.SERVER_FOLDER_PATH + "test.png", System.Drawing.Imaging.ImageFormat.Png);
            }


        }

        public void CreateBitmapFromVisual(Visual target, string filename)
        {
            if (target == null)
                return;

            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

            RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(target);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            rtb.Render(dv);

            PngBitmapEncoder png = new PngBitmapEncoder();

            png.Frames.Add(BitmapFrame.Create(rtb));

            using (Stream stm = File.Create(Globals.HTML.SERVER_FOLDER_PATH + filename + ".png"))
            {
                png.Save(stm);
            }

        }

        public void DeleteImagefromHarddrive(string filename)
        {
            try
            {

                // File.Delete(filename);
            }
            catch (WebException we)
            {
            }
        }
        #endregion


        //Toggle Ball
        #region ToggleBall



        public int ToggleNow(Rectangle ToggleFrame, Ellipse ToggleBall)
        {
            int togglePOS = 0;  // Left is zero
            double toggleDistance = 0;
            toggleDistance = ToggleFrame.Width / 2;


            if ((double)ToggleBall.GetValue(Canvas.LeftProperty) - (double)ToggleFrame.GetValue(Canvas.LeftProperty) >= toggleDistance)
            {
                toggleDistance = toggleDistance * -1;
                togglePOS = 0;
            }
            else { togglePOS = 1; }
            AnimateZoomUIElement((double)ToggleBall.GetValue(Canvas.LeftProperty), (double)ToggleBall.GetValue(Canvas.LeftProperty) + ((toggleDistance)), 0.15, Canvas.LeftProperty, ToggleBall);

            return togglePOS;
        }
        #endregion




        //other General UI functions
        public void RecolorAllChartColumns(SolidColorBrush desiredcolor, Canvas TempCanvas)
        {
            int j;
            Rectangle rect;

            for (j = 0; j <= VisualTreeHelper.GetChildrenCount(TempCanvas) - 1; j++)
            {
                if (VisualTreeHelper.GetChild(TempCanvas, j).GetType().ToString().IndexOf("Rectangle") > -1)
                {
                    rect = (Rectangle)VisualTreeHelper.GetChild(TempCanvas, j);

                    if (rect.Name.Contains("Bar_Rect"))
                    {
                        rect.Fill = desiredcolor;
                    }
                }
            }


        }
        public void Generate_CrystalBall_Bars(Canvas TempCanvas, int numberofrect)
        {

            int i;
            int j;
            int rectnum = -1;
            double rectleft = -1;
            double recttop = -1;


            Rectangle rect;
            Label lbl;
            for (j = 1; j <= numberofrect; j++)
            {
                rect = getMenuItem_Rectangle_fromitemindex(TempCanvas, j);


                rectnum = j;
                recttop = (double)rect.GetValue(Canvas.TopProperty);
                rectleft = (double)rect.GetValue(Canvas.LeftProperty) + 2 + rect.Width / 6;

                GenerateRectangleUI(TempCanvas, "Sim_Rect_" + rectnum, rect.Height, rect.Width / 6, rectleft, recttop, BrushColors.mybrushSelectedCriteria, null, 0, null, null, null, 180);
                //GenerateLabelUI(TempCanvas, "Sim_Label_" + rectnum)

                GenerateLabelUI(TempCanvas, "Sim_Label_" + rectnum, Tiers_datalabelheight, Tiers_datalabelwidth, rectleft, recttop - rect.Height, null, BrushColors.mybrushSelectedCriteria, 9, null, null, null, -1, "", true);
            }


        }
        public void Remove_CrystalBall_Bars(Canvas TempCanvas, int numberofrect)
        {

            int i;
            int j;


            Rectangle rect;
            Label lbl;
            for (j = 1; j <= numberofrect; j++)
            {
                rect = getMenuItem_Rectangle_fromitemindex(TempCanvas, -1, "Sim_Rect_" + j);
                lbl = getMenuItem_Label_fromitemindex(TempCanvas, -1, "Sim_Label_" + j);
                TempCanvas.Children.Remove(rect);
                TempCanvas.Children.Remove(lbl);

            }

        }



        #endregion

        #region Telerik
        #region Chart

        #region Loss Compass
        //#MULTILINE
        private void MultiLine_UpdateModeChart(List<DTeventSummary> dataList)
        {
            int numDecimals = 1;
            /* Chart #1 */
            MultiLineSplashChart2.Series.Clear();
            CategoricalSeries series = new BarSeries();
            CategoricalSeries series1 = new BarSeries();
            for (int i = 0; i < dataList.Count; i++)
            {
                series.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(dataList[i].DTpct * 100, 1), Category = dataList[i].Name });
                series1.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(dataList[i].SPD, 1), Category = dataList[i].Name });

            }
            // series1.Foreground = new SolidColorBrush(Colors.White);
            series1.ShowLabels = true;
            series.ShowLabels = true;
            MultiLineSplashChart2.Series.Add(series);
            MultiLineSplashChart2.Series.Add(series1);
            //   MultiLineSplashChart2.Palette = FunnelChart_getChartColors();
            //   MultiLineSplashChart1.VerticalAxis.Title = "Jobs %";
            //  MultiLineSplashChart2.VerticalAxis.Foreground = new SolidColorBrush(Colors.White);

            // FunnelChart1.VerticalAxis.Background = new SolidColorBrush(Colors.LightBlue);
            //  MultiLineSplashChart2.Foreground = new SolidColorBrush(Colors.White);
            //  MultiLineSplashChart2.VerticalAxis.LabelInterval = 1;
        }

        private void MultiLine_UpdateSummaryChart()
        {
            int numDecimals = 1;
            /* Chart #1 */
            MultiLineSplashChart1.Series.Clear();
            CategoricalSeries series = new BarSeries();
            CategoricalSeries series1 = new BarSeries();
            CategoricalSeries series2 = new BarSeries();
            for (int i = 0; i < intermediate.Multi_CurrentSystemReports_Names.Count; i++)
            {
                series.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(intermediate.Multi_CurrentSystemReports[i].OEE * 100, 1), Category = (intermediate.Multi_CurrentSystemReports_Names[i]) });
                series1.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(intermediate.Multi_CurrentSystemReports[i].UPDTpct * 100, 1), Category = (intermediate.Multi_CurrentSystemReports_Names[i]) });
                series2.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(intermediate.Multi_CurrentSystemReports[i].PDTpct * 100, 1), Category = (intermediate.Multi_CurrentSystemReports_Names[i]) });

            }
            // series1.Foreground = new SolidColorBrush(Colors.White);
            series.ShowLabels = true;
            series1.ShowLabels = true;
            series2.ShowLabels = true;
            MultiLineSplashChart1.Series.Add(series);
            MultiLineSplashChart1.Series.Add(series1);
            MultiLineSplashChart1.Series.Add(series2);
            //  MultiLineSplashChart1.Palette = FunnelChart_getChartColors();
            //   MultiLineSplashChart1.VerticalAxis.Title = "Jobs %";
            //  MultiLineSplashChart1.VerticalAxis.Foreground = new SolidColorBrush(Colors.White);

            // FunnelChart1.VerticalAxis.Background = new SolidColorBrush(Colors.LightBlue);
            //  MultiLineSplashChart1.Foreground = new SolidColorBrush(Colors.White);
            //  MultiLineSplashChart1.VerticalAxis.LabelInterval = 1;
        }

        private void Funnel_UpdateBarGraphs() //updating graphs from intermediate sheet
        {
            int numDecimals = 1;
            /* Chart #1 */
            FunnelChart1.Series.Clear();
            CategoricalSeries series1 = new BarSeries();
            for (int i = 0; i < intermediate.LossCompass_Funnel_KPI1_Values.Count; i++)
            {
                if (i == 0)
                {
                    series1.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(intermediate.LossCompass_Funnel_KPI1_Values[i], numDecimals), Category = ("Original") });
                }
                else
                {
                    series1.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(intermediate.LossCompass_Funnel_KPI1_Values[i], numDecimals), Category = ("Step " + i) });
                }
            }
            // series1.Foreground = new SolidColorBrush(Colors.White);
            series1.ShowLabels = true;
            FunnelChart1.Series.Add(series1);
            FunnelChart1.Palette = FunnelChart_getChartColors();
            FunnelChart1.VerticalAxis.Title = "Jobs %";
            FunnelChart1.VerticalAxis.Foreground = new SolidColorBrush(Colors.White);

            // FunnelChart1.VerticalAxis.Background = new SolidColorBrush(Colors.LightBlue);
            FunnelChart1.Foreground = new SolidColorBrush(Colors.White);
            FunnelChart1.VerticalAxis.LabelInterval = 1;
            //FunnelChart1.HorizontalAxis.LabelFitMode = AxisLabelFitMode.Rotate;

            /* Chart #2 */
            FunnelChart2.Series.Clear();
            CategoricalSeries series2 = new BarSeries();
            for (int i = 0; i < intermediate.LossCompass_Funnel_KPI2_Values.Count; i++)
            {
                if (i == 0)
                {
                    series2.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(intermediate.LossCompass_Funnel_KPI2_Values[i], numDecimals), Category = ("Original") });
                }
                else
                {
                    series2.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(intermediate.LossCompass_Funnel_KPI2_Values[i], numDecimals), Category = ("Step " + i) });
                }
            }
            // series2.Foreground = new SolidColorBrush(Colors.White);
            series2.ShowLabels = true;
            FunnelChart2.Series.Add(series2);
            FunnelChart2.Palette = FunnelChart_getChartColors();
            FunnelChart2.VerticalAxis.Title = "MTBF (min)";
            FunnelChart2.VerticalAxis.Foreground = new SolidColorBrush(Colors.White);

            // FunnelChart2.VerticalAxis.Background = new SolidColorBrush(Colors.LightBlue);
            FunnelChart2.Foreground = new SolidColorBrush(Colors.White);
            FunnelChart2.VerticalAxis.LabelInterval = 2;
            //FunnelChart2.HorizontalAxis.LabelFitMode = AxisLabelFitMode.Rotate;

            /* Chart #3 */
            FunnelChart3.Series.Clear();
            CategoricalSeries series3 = new BarSeries();
            for (int i = 0; i < intermediate.LossCompass_Funnel_KPI3_Values.Count; i++)
            {
                if (i == 0)
                {
                    series3.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(intermediate.LossCompass_Funnel_KPI3_Values[i], numDecimals), Category = ("Original") });
                }
                else
                {
                    series3.DataPoints.Add(new CategoricalDataPoint { Value = Math.Round(intermediate.LossCompass_Funnel_KPI3_Values[i], numDecimals), Category = ("Step " + i) });
                }
            }
            series3.ShowLabels = true;
            // series3.Foreground = new SolidColorBrush(Colors.White);
            FunnelChart3.Foreground = new SolidColorBrush(Colors.White);
            FunnelChart3.Series.Add(series3);
            FunnelChart3.Palette = FunnelChart_getChartColors();
            FunnelChart3.VerticalAxis.Title = "UPDT %";
            FunnelChart3.VerticalAxis.Foreground = new SolidColorBrush(Colors.White);
            //FunnelChart3.HorizontalAxis.LabelInterval = 5;
            //FunnelChart3.HorizontalAxis.LabelFitMode = AxisLabelFitMode.Rotate;

        }

        //event handlers for funnel charts

        private void FunnelChartSelectionBehavior_SelectionChanged_1(object sender, ChartSelectionChangedEventArgs e)
        {
            //responsive to telerik chart


            //  var barSeries = (BarSeries) this.FunnelChart1.Series[0];
            //  MessageBox.Show("here!");
        }

        #endregion

        #region Rate Trainer
        // COMMENTS USED FOR CURVES
        private void RateTrainer_UpdateChartFromIntermediateSheet()
        {
            /*  RateTrainerChart.Series.Clear();
              RateTrainer_UpdateChart_Throughput();
              RateTrainerChart.Palette = RateTrainer_getChartColors(); */
        }


        private void RateTrainer_UpdateChart_Throughput()
        {
            CategoricalSeries series2 = new LineSeries();
            CategoricalSeries series4 = new LineSeries();
            CategoricalSeries series8 = new LineSeries();
            CategoricalSeries series12 = new LineSeries();
            CategoricalSeries series16 = new LineSeries();

            for (int i = 0; i < 61; i++)
            {
                series2.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_2_Tput[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
                series4.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_4_Tput[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
                series8.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_8_Tput[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
                series12.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_12_Tput[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
                series16.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_16_Tput[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
            }

            RateTrainerChart.Series.Add(series2);
            RateTrainerChart.Series.Add(series4);
            RateTrainerChart.Series.Add(series8);
            RateTrainerChart.Series.Add(series12);
            RateTrainerChart.Series.Add(series16);
        }
        private void RateTrainer_UpdateChart_OEE()
        {
            CategoricalSeries series2 = new LineSeries();
            CategoricalSeries series4 = new LineSeries();
            CategoricalSeries series8 = new LineSeries();
            CategoricalSeries series12 = new LineSeries();
            CategoricalSeries series16 = new LineSeries();

            for (int i = 0; i < 61; i++)
            {
                series2.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_2_OEE[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
                series4.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_4_OEE[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
                series8.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_8_OEE[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
                series12.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_12_OEE[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
                series16.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.RateTrainer_RawAnalysis.Values_16_OEE[i], Category = intermediate.RateTrainer_RawAnalysis.Values_X_Axis[i] });
            }

            RateTrainerChart.Series.Add(series2);
            RateTrainerChart.Series.Add(series4);
            RateTrainerChart.Series.Add(series8);
            RateTrainerChart.Series.Add(series12);
            RateTrainerChart.Series.Add(series16);
        }
        //*/
        #endregion

        #region Trends

        #region Line
        // e.Context.
        private void Trends_Line_ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            var tmpString = "";
            foreach (DataPointInfo info in e.Context.DataPointInfos)
            {
                // info.DisplayHeader = "Custom data point header";
                tmpString += info.DataPoint.Label + Environment.NewLine;
            }

            e.Header = tmpString;
        }

        private void LineTrends_AddGlidePath()
        {
            //series
            CategoricalSeries series1 = new LineSeries();
            for (int i = 0; i < intermediate.Trends_GlidePath_CurrentGlidePath.Count; i++)
            {
                //  series1.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.Trends_GlidePath_CurrentGlidePath[i], Category = intermediate.Trends_Mode_ChartCategories[i].ToString("MM/dd") });
            }
            LineTrendChart.Series.Add(series1);
        }
        private void LineTrends_RemoveGlidePath()
        {
            LineTrends_UpdateChartFromIntermediateSheet();
        }

        private List<DowntimeMetrics> LineTrends_Axis1_Metrics = new List<DowntimeMetrics> { DowntimeMetrics.OEE, DowntimeMetrics.UPDTpct, DowntimeMetrics.PDTpct };
        private List<DowntimeMetrics> LineTrends_Axis2_Metrics = new List<DowntimeMetrics> { DowntimeMetrics.MTBF, DowntimeMetrics.UnitsProduced, DowntimeMetrics.Stops };
        private List<DowntimeMetrics> LineTrends_Axis3_Metrics = new List<DowntimeMetrics> { DowntimeMetrics.SKUs, DowntimeMetrics.SPD, DowntimeMetrics.NumChangeovers };

        private List<DowntimeMetrics> ModeTrends_Axis1_Metrics = new List<DowntimeMetrics> { DowntimeMetrics.DTpct, DowntimeMetrics.DT };
        private List<DowntimeMetrics> ModeTrends_Axis2_Metrics = new List<DowntimeMetrics> { DowntimeMetrics.MTBF, DowntimeMetrics.MTTR };
        private List<DowntimeMetrics> ModeTrends_Axis3_Metrics = new List<DowntimeMetrics> { DowntimeMetrics.Stops, DowntimeMetrics.SPD };

        public static DataTemplate Telerik_getLinePoint(string colorAsString = "#FF8EC441", string WidthAsString = "10", string HeightAsString = "10")
        {
            string bgcolor = "#FFFFFFFF";
            StringReader stringReader = new StringReader(
            @"<DataTemplate 
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""> 
                    <Ellipse Width = """ + WidthAsString + @""" Height = """ + HeightAsString + @""" Fill=""" + bgcolor + @""" Stroke =""" + colorAsString + @""" StrokeThickness =""" + 1 + @""" /> 
                    </DataTemplate>");
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            return System.Windows.Markup.XamlReader.Load(xmlReader) as DataTemplate;
        }

        private void LineTrends_UpdateChartFromIntermediateSheet()
        {
            var blankDataTemplate = new DataTemplate("");
            LineTrendChart.Series.Clear();
            LineTrendChart.Palette = Trends_defaultChartColors();

            //axis stuff
            LineTrendChart.VerticalAxis = new LinearAxis();
            var secondaryVAxis = new LinearAxis();
            secondaryVAxis.HorizontalLocation = AxisHorizontalLocation.Right;

            var thirdVAxis = new LinearAxis();
            thirdVAxis.HorizontalLocation = AxisHorizontalLocation.Right;

            //find axis titles
            string AxisTitle1 = "";
            string AxisTitle2 = "";
            string AxisTitle3 = "";

            for (int metricInc = 0; metricInc < ListofSelectedKPI_LineTrends.Count; metricInc++)
            {
                //find the appropriate axis
                string newString = getStringForEnum_Metric(ListofSelectedKPI_LineTrends[metricInc]);
                if (LineTrends_Axis1_Metrics.IndexOf(ListofSelectedKPI_LineTrends[metricInc]) > -1)
                {
                    if (AxisTitle1 == "")
                    {
                        AxisTitle1 = newString;
                    }
                    else
                    {
                        AxisTitle1 += ", " + newString;
                    }
                }
                else if (LineTrends_Axis2_Metrics.IndexOf(ListofSelectedKPI_LineTrends[metricInc]) > -1)
                {
                    if (AxisTitle2 == "")
                    {
                        AxisTitle2 = newString;
                    }
                    else
                    {
                        AxisTitle2 += ", " + newString;
                    }
                }
                else
                {
                    if (AxisTitle3 == "")
                    {
                        AxisTitle3 = newString;
                    }
                    else
                    {
                        AxisTitle3 += ", " + newString;
                    }
                }
            }

            LineTrendChart.VerticalAxis.Title = AxisTitle1;
            secondaryVAxis.Title = AxisTitle2;
            thirdVAxis.Title = AxisTitle3;

            /* ROLL UP SERIES */
            if (intermediate.Multi_CurrentLineNames.Count > 1 && LineTrends_showRollUp)
            {
                for (int metricInc = 0; metricInc < ListofSelectedKPI_LineTrends.Count; metricInc++)
                {
                    int metricIndex = intermediate.Trends_Line_MasterMetricList.IndexOf(ListofSelectedKPI_LineTrends[metricInc]);
                    //get series type right
                    CategoricalSeries newSeries;
                    if (LineTrends_isLineGraph)
                    {
                        newSeries = new LineSeries();
                    }
                    else
                    {
                        newSeries = new BarSeries();
                    }

                    //add the data for right time period
                    string labelIntroString = "MultiLine " + getStringForEnum_Metric(ListofSelectedKPI_LineTrends[metricInc]) + ": ";

                    if (LineTrends_analysistimeperiod == 1) //daily
                    {
                        for (int i = 0; i < intermediate.Trends_Line_MasterDataList_Daily_RollUp[metricIndex].Count; i++)
                        {
                            double value = intermediate.Trends_Line_MasterDataList_Daily_RollUp[metricIndex][i];
                            newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[0][i].startTime.ToString("MM/dd"), Label = labelIntroString + Math.Round(value, 1) });
                        }
                    }
                    else if (LineTrends_analysistimeperiod == 7) //weekly
                    {
                        for (int i = 0; i < intermediate.Trends_Line_MasterDataList_Weekly_RollUp[metricIndex].Count; i++)
                        {
                            double value = intermediate.Trends_Line_MasterDataList_Weekly_RollUp[metricIndex][i];
                            newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[0][i].startTime.ToString("MMM dd"), Label = labelIntroString + Math.Round(value, 1) });
                        }
                        //add the correct point template
                        if (LineTrends_isLineGraph)
                        {
                            string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), LineTrendChart.Series.Count);
                            newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                        }
                    }
                    else //monthly
                    {
                        for (int i = 0; i < intermediate.Trends_Line_MasterDataList_Monthly_RollUp[metricIndex].Count; i++)
                        {
                            double value = intermediate.Trends_Line_MasterDataList_Monthly_RollUp[metricIndex][i];
                            newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Monthly[0][i].startTime.ToString("MMM yy"), Label = labelIntroString + Math.Round(value, 1) });
                        }
                        //add the correct point template
                        if (LineTrends_isLineGraph)
                        {
                            string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), LineTrendChart.Series.Count);
                            newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                        }
                    }

                    //find the appropriate axis
                    if (LineTrends_Axis1_Metrics.IndexOf(ListofSelectedKPI_LineTrends[metricInc]) > -1)
                    {
                        //we're good!
                    }
                    else if (LineTrends_Axis2_Metrics.IndexOf(ListofSelectedKPI_LineTrends[metricInc]) > -1)
                    {
                        newSeries.VerticalAxis = secondaryVAxis;
                    }
                    else
                    {
                        newSeries.VerticalAxis = thirdVAxis;
                    }

                    //wrap it up
                    newSeries.TrackBallInfoTemplate = blankDataTemplate;
                    LineTrendChart.Series.Add(newSeries);
                }
            }
            /* END ROLLUP SERIES */


            if (intermediate.Multi_CurrentLineNames.Count == 1 || !LineTrends_showRollUpOnly)
            {

                //for each metrics...
                for (int metricInc = 0; metricInc < ListofSelectedKPI_LineTrends.Count; metricInc++)
                {
                    //for each line...
                    int metricIndex = intermediate.Trends_Line_MasterMetricList.IndexOf(ListofSelectedKPI_LineTrends[metricInc]);
                    for (int lineInc = 0; lineInc < intermediate.Multi_CurrentLineNames.Count; lineInc++)
                    {
                        //get series type right
                        CategoricalSeries newSeries;
                        if (LineTrends_isLineGraph)
                        {
                            newSeries = new LineSeries();
                        }
                        else
                        {
                            newSeries = new BarSeries();
                        }

                        //add the data for right time period
                        string labelIntroString;
                        if (intermediate.Multi_CurrentLineNames.Count > 1)
                        {
                            labelIntroString = intermediate.Multi_CurrentLineNames[lineInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_LineTrends[metricInc]) + ": ";
                        }
                        else
                        {
                            labelIntroString = getStringForEnum_Metric(ListofSelectedKPI_LineTrends[metricInc]) + ": ";
                        }

                        if (LineTrends_analysistimeperiod == 1) //daily
                        {
                            for (int i = 0; i < intermediate.Trends_Line_MasterDataList_Daily[lineInc][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Line_MasterDataList_Daily[lineInc][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[lineInc][i].startTime.ToString("MM/dd"), Label = labelIntroString + Math.Round(value, 1) });
                            }
                        }
                        else if (LineTrends_analysistimeperiod == 7) //weekly
                        {
                            for (int i = 0; i < intermediate.Trends_Line_MasterDataList_Weekly[lineInc][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Line_MasterDataList_Weekly[lineInc][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[lineInc][i].startTime.ToString("MMM dd"), Label = labelIntroString + Math.Round(value, 1) });
                            }
                            //add the correct point template
                            if (LineTrends_isLineGraph)
                            {
                                string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), LineTrendChart.Series.Count);
                                newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                            }
                        }
                        else //monthly
                        {
                            for (int i = 0; i < intermediate.Trends_Line_MasterDataList_Monthly[lineInc][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Line_MasterDataList_Monthly[lineInc][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Monthly[lineInc][i].startTime.ToString("MMM yy"), Label = labelIntroString + Math.Round(value, 1) });
                            }
                            //add the correct point template
                            if (LineTrends_isLineGraph)
                            {
                                string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), LineTrendChart.Series.Count);
                                newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                            }
                        }

                        //find the appropriate axis
                        if (LineTrends_Axis1_Metrics.IndexOf(ListofSelectedKPI_LineTrends[metricInc]) > -1)
                        {
                            //we're good!
                        }
                        else if (LineTrends_Axis2_Metrics.IndexOf(ListofSelectedKPI_LineTrends[metricInc]) > -1)
                        {
                            newSeries.VerticalAxis = secondaryVAxis;
                        }
                        else
                        {
                            newSeries.VerticalAxis = thirdVAxis;
                        }

                        //wrap it up
                        newSeries.TrackBallInfoTemplate = blankDataTemplate;
                        LineTrendChart.Series.Add(newSeries);
                    }
                }
            }

            //format chart accordingly
            if (LineTrends_analysistimeperiod == 1)
            {
                LineTrendChart.HorizontalAxis.LabelInterval = 10;
            }
            else if (LineTrends_analysistimeperiod == 7)
            {
                LineTrendChart.HorizontalAxis.LabelInterval = 2;
            }
            else
            {
                LineTrendChart.HorizontalAxis.LabelInterval = 1;
            }
            LineTrendChart.HorizontalAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.None;

        }

        private void ModeTrends_UpdateChartFromIntermediateSheet()
        {
            var blankDataTemplate = new DataTemplate("");
            RDC_LossTrendChart.Series.Clear();
            RDC_LossTrendChart.Palette = Trends_defaultChartColors();

            //axis stuff
            RDC_LossTrendChart.VerticalAxis = new LinearAxis();
            var secondaryVAxis = new LinearAxis();
            secondaryVAxis.HorizontalLocation = AxisHorizontalLocation.Right;

            var thirdVAxis = new LinearAxis();
            thirdVAxis.HorizontalLocation = AxisHorizontalLocation.Right;

            //find axis titles
            string AxisTitle1 = "";
            string AxisTitle2 = "";
            string AxisTitle3 = "";

            //find selected items & indices
            var selectedUnplannedNames = new List<string>();
            var selectedPlannedNames = new List<string>();
            var selectedUnplannedIndices = new List<int>();
            var selectedPlannedIndices = new List<int>();

            for (int i = 0; i < TrendsFailuremodeListbox_unplanned.SelectedItems.Count; i++)
            {
                selectedUnplannedNames.Add(TrendsFailuremodeListbox_unplanned.SelectedItems[i].ToString());
                selectedUnplannedIndices.Add(intermediate.Trends_Mode_Names_Unplanned.IndexOf(TrendsFailuremodeListbox_unplanned.SelectedItems[i].ToString()));
            }
            for (int i = 0; i < TrendsFailuremodeListbox_planned.SelectedItems.Count; i++)
            {
                selectedPlannedNames.Add(TrendsFailuremodeListbox_planned.SelectedItems[i].ToString());
                selectedPlannedIndices.Add(intermediate.Trends_Mode_Names_Planned.IndexOf(TrendsFailuremodeListbox_planned.SelectedItems[i].ToString()));
            }

            //make the chart
            for (int metricInc = 0; metricInc < ListofSelectedKPI_ModeTrends.Count; metricInc++)
            {
                //find the appropriate axis
                string newString = getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[metricInc]);
                if (ModeTrends_Axis1_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                {
                    if (AxisTitle1 == "")
                    {
                        AxisTitle1 = newString;
                    }
                    else
                    {
                        AxisTitle1 += ", " + newString;
                    }
                }
                else if (ModeTrends_Axis2_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                {
                    if (AxisTitle2 == "")
                    {
                        AxisTitle2 = newString;
                    }
                    else
                    {
                        AxisTitle2 += ", " + newString;
                    }
                }
                else
                {
                    if (AxisTitle3 == "")
                    {
                        AxisTitle3 = newString;
                    }
                    else
                    {
                        AxisTitle3 += ", " + newString;
                    }
                }
            }

            RDC_LossTrendChart.VerticalAxis.Title = AxisTitle1;
            secondaryVAxis.Title = AxisTitle2;
            thirdVAxis.Title = AxisTitle3;

            /* ROLL UP SERIES */
            if (intermediate.Multi_CurrentLineNames.Count > 1 && LossTrends_showRollUp)
            {

                //UNPLANNED

                for (int modeInc = 0; modeInc < selectedUnplannedIndices.Count; modeInc++)
                {
                    int modeIndex = selectedUnplannedIndices[modeInc];
                    for (int metricInc = 0; metricInc < ListofSelectedKPI_ModeTrends.Count; metricInc++)
                    {
                        int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]);
                        //get series type right
                        CategoricalSeries newSeries;
                        if (LossTrends_isLineGraph)
                        {
                            newSeries = new LineSeries();
                        }
                        else
                        {
                            newSeries = new BarSeries();
                        }

                        //add the data for right time period
                        string labelIntroString = "MultiLine " + selectedUnplannedNames[modeInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[metricInc]) + ": ";

                        if (LineTrends_Mode_analysistimeperiod == 1) //daily
                        {
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Daily_RollUp_Unplanned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Daily_RollUp_Unplanned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[0][i].startTime.ToString("MM/dd"), Label = labelIntroString + Math.Round(value, 1) });
                            }
                        }
                        else if (LineTrends_Mode_analysistimeperiod == 7) //weekly
                        {
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Weekly_RollUp_Unplanned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Weekly_RollUp_Unplanned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[0][i].startTime.ToString("MMM dd"), Label = labelIntroString + Math.Round(value, 1) });
                            }
                            //add the correct point template
                            if (LossTrends_isLineGraph)
                            {
                                string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), RDC_LossTrendChart.Series.Count);
                                newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                            }
                        }
                        else //monthly
                        {
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Monthly_RollUp_Unplanned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Monthly_RollUp_Unplanned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Monthly[0][i].startTime.ToString("MMM yy"), Label = labelIntroString + Math.Round(value, 1) });
                            }
                            //add the correct point template
                            if (LossTrends_isLineGraph)
                            {
                                string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), RDC_LossTrendChart.Series.Count);
                                newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                            }
                        }

                        //find the appropriate axis
                        if (ModeTrends_Axis1_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                        {
                            //we're good!
                        }
                        else if (ModeTrends_Axis2_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                        {
                            newSeries.VerticalAxis = secondaryVAxis;
                        }
                        else
                        {
                            newSeries.VerticalAxis = thirdVAxis;
                        }

                        //wrap it up
                        newSeries.TrackBallInfoTemplate = blankDataTemplate;
                        RDC_LossTrendChart.Series.Add(newSeries);
                    }
                }

                //PLANNED

                for (int modeInc = 0; modeInc < selectedPlannedIndices.Count; modeInc++)
                {
                    int modeIndex = selectedPlannedIndices[modeInc];
                    for (int metricInc = 0; metricInc < ListofSelectedKPI_ModeTrends.Count; metricInc++)
                    {
                        int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]);
                        //get series type right
                        CategoricalSeries newSeries;
                        if (LossTrends_isLineGraph)
                        {
                            newSeries = new LineSeries();
                        }
                        else
                        {
                            newSeries = new BarSeries();
                        }

                        //add the data for right time period
                        string labelIntroString = "MultiLine " + selectedPlannedNames[modeInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[metricInc]) + ": ";

                        if (LineTrends_Mode_analysistimeperiod == 1) //daily
                        {
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Daily_RollUp_Planned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Daily_RollUp_Planned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[0][i].startTime.ToString("MM/dd"), Label = labelIntroString + Math.Round(value, 1) });
                            }
                        }
                        else if (LineTrends_Mode_analysistimeperiod == 7) //weekly
                        {
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Weekly_RollUp_Planned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Weekly_RollUp_Planned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[0][i].startTime.ToString("MMM dd"), Label = labelIntroString + Math.Round(value, 1) });
                            }
                            //add the correct point template
                            if (LossTrends_isLineGraph)
                            {
                                string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), RDC_LossTrendChart.Series.Count);
                                newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                            }
                        }
                        else //monthly
                        {
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Monthly_RollUp_Planned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Monthly_RollUp_Planned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Monthly[0][i].startTime.ToString("MMM yy"), Label = labelIntroString + Math.Round(value, 1) });
                            }
                            //add the correct point template
                            if (LossTrends_isLineGraph)
                            {
                                string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), RDC_LossTrendChart.Series.Count);
                                newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                            }
                        }

                        //find the appropriate axis
                        if (ModeTrends_Axis1_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                        {
                            //we're good!
                        }
                        else if (ModeTrends_Axis2_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                        {
                            newSeries.VerticalAxis = secondaryVAxis;
                        }
                        else
                        {
                            newSeries.VerticalAxis = thirdVAxis;
                        }

                        //wrap it up
                        newSeries.TrackBallInfoTemplate = blankDataTemplate;
                        RDC_LossTrendChart.Series.Add(newSeries);
                    }
                }

            }
            /* END ROLLUP SERIES */



            if (intermediate.Multi_CurrentLineNames.Count == 1 || !LossTrends_showRollUpOnly)
            {
                //UNPLANNED
                //for each mode...
                for (int modeInc = 0; modeInc < selectedUnplannedIndices.Count; modeInc++)
                {
                    int modeIndex = selectedUnplannedIndices[modeInc];
                    //for each metrics...
                    for (int metricInc = 0; metricInc < ListofSelectedKPI_ModeTrends.Count; metricInc++)
                    {
                        //for each line...
                        int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]);
                        for (int lineInc = 0; lineInc < intermediate.Multi_CurrentLineNames.Count; lineInc++)
                        {
                            //get series type right
                            CategoricalSeries newSeries;
                            if (LossTrends_isLineGraph)
                            {
                                newSeries = new LineSeries();
                            }
                            else
                            {
                                newSeries = new BarSeries();
                            }

                            //add the data for right time period
                            string labelIntroString;
                            if (intermediate.Multi_CurrentLineNames.Count > 1)
                            {
                                labelIntroString = selectedUnplannedNames[modeInc] + " " + intermediate.Multi_CurrentLineNames[lineInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[metricInc]) + ": ";
                            }
                            else
                            {
                                labelIntroString = selectedUnplannedNames[modeInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[metricInc]) + ": ";
                            }

                            if (LineTrends_Mode_analysistimeperiod == 1) //daily
                            {
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Daily_Unplanned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Daily_Unplanned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[lineInc][i].startTime.ToString("MM/dd"), Label = labelIntroString + Math.Round(value, 1) });
                                }
                            }
                            else if (LineTrends_Mode_analysistimeperiod == 7) //weekly
                            {
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Weekly_Unplanned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Weekly_Unplanned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[lineInc][i].startTime.ToString("MMM dd"), Label = labelIntroString + Math.Round(value, 1) });
                                }
                                //add the correct point template
                                if (LossTrends_isLineGraph)
                                {
                                    string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), RDC_LossTrendChart.Series.Count);
                                    newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                                }
                            }
                            else //monthly
                            {
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Monthly_Unplanned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Monthly_Unplanned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Monthly[lineInc][i].startTime.ToString("MMM yy"), Label = labelIntroString + Math.Round(value, 1) });
                                }
                                //add the correct point template
                                if (LossTrends_isLineGraph)
                                {
                                    string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), RDC_LossTrendChart.Series.Count);
                                    newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                                }
                            }

                            //find the appropriate axis
                            if (ModeTrends_Axis1_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                            {
                                //we're good!
                            }
                            else if (ModeTrends_Axis2_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                            {
                                newSeries.VerticalAxis = secondaryVAxis;
                            }
                            else
                            {
                                newSeries.VerticalAxis = thirdVAxis;
                            }

                            //wrap it up
                            newSeries.TrackBallInfoTemplate = blankDataTemplate;
                            RDC_LossTrendChart.Series.Add(newSeries);
                        }
                    }
                }

                //PLANNED
                //for each mode...
                for (int modeInc = 0; modeInc < selectedPlannedIndices.Count; modeInc++)
                {
                    int modeIndex = selectedPlannedIndices[modeInc];
                    //for each metrics...
                    for (int metricInc = 0; metricInc < ListofSelectedKPI_ModeTrends.Count; metricInc++)
                    {
                        //for each line...
                        int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]);
                        for (int lineInc = 0; lineInc < intermediate.Multi_CurrentLineNames.Count; lineInc++)
                        {
                            //get series type right
                            CategoricalSeries newSeries;
                            if (LossTrends_isLineGraph)
                            {
                                newSeries = new LineSeries();
                            }
                            else
                            {
                                newSeries = new BarSeries();
                            }

                            //add the data for right time period
                            string labelIntroString;
                            if (intermediate.Multi_CurrentLineNames.Count > 1)
                            {
                                labelIntroString = selectedPlannedNames[modeInc] + " " + intermediate.Multi_CurrentLineNames[lineInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[metricInc]) + ": ";
                            }
                            else
                            {
                                labelIntroString = selectedPlannedNames[modeInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[metricInc]) + ": ";
                            }

                            if (LineTrends_Mode_analysistimeperiod == 1) //daily
                            {
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Daily_Planned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Daily_Planned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[lineInc][i].startTime.ToString("MM/dd"), Label = labelIntroString + Math.Round(value, 1) });
                                }
                            }
                            else if (LineTrends_Mode_analysistimeperiod == 7) //weekly
                            {
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Weekly_Planned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Weekly_Planned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[lineInc][i].startTime.ToString("MMM dd"), Label = labelIntroString + Math.Round(value, 1) });
                                }
                                if (LossTrends_isLineGraph)
                                {
                                    string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), RDC_LossTrendChart.Series.Count);
                                    newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                                }
                            }
                            else //monthly
                            {
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Monthly_Planned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Monthly_Planned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Monthly[lineInc][i].startTime.ToString("MMM yy"), Label = labelIntroString + Math.Round(value, 1) });
                                }
                                if (LossTrends_isLineGraph)
                                {
                                    string hexColor = Color_HexFromPaletteEntry(Trends_defaultChartColors(), RDC_LossTrendChart.Series.Count);
                                    newSeries.PointTemplate = Telerik_getLinePoint("#" + hexColor);
                                }
                            }

                            //find the appropriate axis
                            if (ModeTrends_Axis1_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                            {
                                //we're good!
                            }
                            else if (ModeTrends_Axis2_Metrics.IndexOf(ListofSelectedKPI_ModeTrends[metricInc]) > -1)
                            {
                                newSeries.VerticalAxis = secondaryVAxis;
                            }
                            else
                            {
                                newSeries.VerticalAxis = thirdVAxis;
                            }

                            //wrap it up
                            newSeries.TrackBallInfoTemplate = blankDataTemplate;
                            RDC_LossTrendChart.Series.Add(newSeries);
                        }
                    }
                }
            }

            //format chart accordingly
            if (LineTrends_Mode_analysistimeperiod == 1)
            {
                RDC_LossTrendChart.HorizontalAxis.LabelInterval = 10;
            }
            else if (LineTrends_Mode_analysistimeperiod == 7)
            {
                RDC_LossTrendChart.HorizontalAxis.LabelInterval = 2;
            }
            else
            {
                RDC_LossTrendChart.HorizontalAxis.LabelInterval = 1;
            }
            RDC_LossTrendChart.HorizontalAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.None;

        }

        #endregion

        private void Trends_Mode_ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            var tmpString = "";
            foreach (DataPointInfo info in e.Context.DataPointInfos)
            {
                // info.DisplayHeader = "Custom data point header";
                tmpString += info.DataPoint.Label + Environment.NewLine;
            }

            e.Header = tmpString;
        }

        private void Trends_Step_ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            var tmpString = "";
            foreach (DataPointInfo info in e.Context.DataPointInfos)
            {
                // info.DisplayHeader = "Custom data point header";
                tmpString += info.DataPoint.Label + Environment.NewLine;
            }

            e.Header = tmpString;
        }




        private void StepChange_UpdateChartFromIntermediateSheet()
        {
            var blankDataTemplate = new DataTemplate("");
            RDC_StepChange.Series.Clear();
            RDC_StepChange.Palette = StepChange_getChartColors();
            RDC_StepChange.VerticalAxis = new LinearAxis();

            //find selected items & indices
            var selectedUnplannedNames = Trends_Step_SelectedFailureModes_Unplanned; //new List<string>();
            var selectedPlannedNames = Trends_Step_SelectedFailureModes_Planned; // new List<string>();
            var selectedUnplannedIndices = new List<int>();
            var selectedPlannedIndices = new List<int>();

            for (int i = 0; i < selectedUnplannedNames.Count; i++)
            {
                selectedUnplannedIndices.Add(intermediate.Trends_Mode_Names_Unplanned.IndexOf(selectedUnplannedNames[i]));
            }
            for (int i = 0; i < selectedPlannedNames.Count; i++)
            {
                selectedPlannedIndices.Add(intermediate.Trends_Mode_Names_Planned.IndexOf(selectedPlannedNames[i]));
            }

            RDC_StepChange.VerticalAxis.Title = getStringForEnum_Metric(ListofSelectedKPI_StepTrends[0]);


            /* ROLL UP SERIES */
            if (intermediate.Multi_CurrentLineNames.Count > 1 && StepTrends_showRollUp)
            {
                //UNPLANNED
                for (int modeInc = 0; modeInc < selectedUnplannedIndices.Count; modeInc++)
                {
                    int modeIndex = selectedUnplannedIndices[modeInc];
                    for (int metricInc = 0; metricInc < ListofSelectedKPI_StepTrends.Count; metricInc++)
                    {
                        int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_StepTrends[metricInc]);
                        //get series type right
                        CategoricalSeries newSeries;
                        CategoricalSeries newSeries2;
                        if (StepTrends_isLineGraph)
                        {
                            newSeries = new LineSeries();
                            newSeries2 = new LineSeries();
                        }
                        else
                        {
                            newSeries = new BarSeries();
                            newSeries2 = new BarSeries();
                        }

                        //add the data for right time period
                        string labelIntroString = "MultiLine " + selectedUnplannedNames[modeInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_StepTrends[metricInc]) + ": ";

                        if (LineTrends_Step_analysistimeperiod == 1) //daily
                        {
                            //actual
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Daily_RollUp_Unplanned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Daily_RollUp_Unplanned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[0][i].startTime.ToString("MM/dd"), Label = "Act. " + labelIntroString + Math.Round(value, 1) });
                            }
                            //step
                            for (int i = 0; i < intermediate.Trends_Step_MasterDataList_Daily_RollUp_Unplanned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Step_MasterDataList_Daily_RollUp_Unplanned[modeIndex][metricIndex][i];
                                newSeries2.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[0][i].startTime.ToString("MM/dd"), Label = "Step " + labelIntroString + Math.Round(value, 1) });
                            }
                        }
                        else if (LineTrends_Step_analysistimeperiod == 7) //weekly
                        {
                            //actual
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Weekly_RollUp_Unplanned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Weekly_RollUp_Unplanned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[0][i].startTime.ToString("MMM dd"), Label = "Act. " + labelIntroString + Math.Round(value, 1) });
                            }
                            //step
                            for (int i = 0; i < intermediate.Trends_Step_MasterDataList_Weekly_RollUp_Unplanned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Step_MasterDataList_Weekly_RollUp_Unplanned[modeIndex][metricIndex][i];
                                newSeries2.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[0][i].startTime.ToString("MMM dd"), Label = "Step " + labelIntroString + Math.Round(value, 1) });
                            }
                        }


                        //wrap it up
                        newSeries.TrackBallInfoTemplate = blankDataTemplate;
                        newSeries2.TrackBallInfoTemplate = blankDataTemplate;
                        RDC_StepChange.Series.Add(newSeries);
                        RDC_StepChange.Series.Add(newSeries2);
                    }
                }

                //PLANNED

                for (int modeInc = 0; modeInc < selectedPlannedIndices.Count; modeInc++)
                {
                    int modeIndex = selectedPlannedIndices[modeInc];
                    for (int metricInc = 0; metricInc < ListofSelectedKPI_StepTrends.Count; metricInc++)
                    {
                        int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_StepTrends[metricInc]);
                        //get series type right
                        CategoricalSeries newSeries;
                        CategoricalSeries newSeries2;
                        if (StepTrends_isLineGraph)
                        {
                            newSeries = new LineSeries();
                            newSeries2 = new LineSeries();
                        }
                        else
                        {
                            newSeries = new BarSeries();
                            newSeries2 = new BarSeries();
                        }

                        //add the data for right time period
                        string labelIntroString = "MultiLine " + selectedPlannedNames[modeInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_StepTrends[metricInc]) + ": ";
                        if (LineTrends_Step_analysistimeperiod == 1) //daily
                        {
                            //actual
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Daily_RollUp_Planned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Daily_RollUp_Planned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[0][i].startTime.ToString("MM/dd"), Label = "Act. " + labelIntroString + Math.Round(value, 1) });
                            }
                            //step
                            for (int i = 0; i < intermediate.Trends_Step_MasterDataList_Daily_RollUp_Planned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Step_MasterDataList_Daily_RollUp_Planned[modeIndex][metricIndex][i];
                                newSeries2.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[0][i].startTime.ToString("MM/dd"), Label = "Step " + labelIntroString + Math.Round(value, 1) });
                            }
                        }
                        else if (LineTrends_Step_analysistimeperiod == 7) //weekly
                        {
                            //actual
                            for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Weekly_RollUp_Planned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Mode_MasterDataList_Weekly_RollUp_Planned[modeIndex][metricIndex][i];
                                newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[0][i].startTime.ToString("MMM dd"), Label = "Act. " + labelIntroString + Math.Round(value, 1) });
                            }
                            //step
                            for (int i = 0; i < intermediate.Trends_Step_MasterDataList_Weekly_RollUp_Unplanned[modeIndex][metricIndex].Count; i++)
                            {
                                double value = intermediate.Trends_Step_MasterDataList_Weekly_RollUp_Unplanned[modeIndex][metricIndex][i];
                                newSeries2.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[0][i].startTime.ToString("MMM dd"), Label = "Step " + labelIntroString + Math.Round(value, 1) });
                            }
                        }

                        //wrap it up
                        newSeries.TrackBallInfoTemplate = blankDataTemplate;
                        newSeries2.TrackBallInfoTemplate = blankDataTemplate;
                        RDC_StepChange.Series.Add(newSeries);
                        RDC_StepChange.Series.Add(newSeries2);
                    }
                }

            }
            /* END ROLLUP SERIES */



            if (intermediate.Multi_CurrentLineNames.Count == 1 || !StepTrends_showRollUpOnly)
            {
                //UNPLANNED
                //for each mode...
                for (int modeInc = 0; modeInc < selectedUnplannedIndices.Count; modeInc++)
                {
                    int modeIndex = selectedUnplannedIndices[modeInc];
                    //for each metrics...
                    for (int metricInc = 0; metricInc < ListofSelectedKPI_StepTrends.Count; metricInc++)
                    {
                        //for each line...
                        int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_StepTrends[metricInc]);
                        for (int lineInc = 0; lineInc < intermediate.Multi_CurrentLineNames.Count; lineInc++)
                        {
                            //get series type right
                            CategoricalSeries newSeries;
                            CategoricalSeries newSeries2;
                            if (StepTrends_isLineGraph)
                            {
                                newSeries = new LineSeries();
                                newSeries2 = new LineSeries();
                            }
                            else
                            {
                                newSeries = new BarSeries();
                                newSeries2 = new BarSeries();
                            }

                            //add the data for right time period
                            string labelIntroString;
                            if (intermediate.Multi_CurrentLineNames.Count > 1)
                            {
                                labelIntroString = selectedUnplannedNames[modeInc] + " " + intermediate.Multi_CurrentLineNames[lineInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_StepTrends[metricInc]) + ": ";
                            }
                            else
                            {
                                labelIntroString = selectedUnplannedNames[modeInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_StepTrends[metricInc]) + ": ";
                            }

                            if (LineTrends_Step_analysistimeperiod == 1) //daily
                            {
                                //actual
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Daily_Unplanned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Daily_Unplanned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[lineInc][i].startTime.ToString("MM/dd"), Label = "Act. " + labelIntroString + Math.Round(value, 1) });
                                }
                                //step
                                for (int i = 0; i < intermediate.Trends_Step_MasterDataList_Daily_Unplanned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Step_MasterDataList_Daily_Unplanned[lineInc][modeIndex][metricIndex][i];
                                    newSeries2.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[lineInc][i].startTime.ToString("MM/dd"), Label = "Step " + labelIntroString + Math.Round(value, 1) });
                                }
                            }
                            else if (LineTrends_Step_analysistimeperiod == 7) //weekly
                            {
                                //actual
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Weekly_Unplanned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Weekly_Unplanned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[lineInc][i].startTime.ToString("MMM dd"), Label = "Act. " + labelIntroString + Math.Round(value, 1) });
                                }
                                //step
                                for (int i = 0; i < intermediate.Trends_Step_MasterDataList_Weekly_Unplanned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Step_MasterDataList_Weekly_Unplanned[lineInc][modeIndex][metricIndex][i];
                                    newSeries2.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[lineInc][i].startTime.ToString("MMM dd"), Label = "Step " + labelIntroString + Math.Round(value, 1) });
                                }
                            }

                            //wrap it up
                            newSeries.TrackBallInfoTemplate = blankDataTemplate;
                            RDC_StepChange.Series.Add(newSeries);
                            newSeries2.TrackBallInfoTemplate = blankDataTemplate;
                            RDC_StepChange.Series.Add(newSeries2);
                        }
                    }
                }


                //PLANNED
                //for each mode...
                for (int modeInc = 0; modeInc < selectedPlannedIndices.Count; modeInc++)
                {
                    int modeIndex = selectedPlannedIndices[modeInc];
                    //for each metrics...
                    for (int metricInc = 0; metricInc < ListofSelectedKPI_StepTrends.Count; metricInc++)
                    {
                        //for each line...
                        int metricIndex = intermediate.Trends_Mode_MasterMetricList.IndexOf(ListofSelectedKPI_StepTrends[metricInc]);
                        for (int lineInc = 0; lineInc < intermediate.Multi_CurrentLineNames.Count; lineInc++)
                        {
                            //get series type right
                            CategoricalSeries newSeries;
                            CategoricalSeries newSeries2;
                            if (StepTrends_isLineGraph)
                            {
                                newSeries = new LineSeries();
                                newSeries2 = new LineSeries();
                            }
                            else
                            {
                                newSeries = new BarSeries();
                                newSeries2 = new BarSeries();
                            }

                            //add the data for right time period
                            string labelIntroString;
                            if (intermediate.Multi_CurrentLineNames.Count > 1)
                            {
                                labelIntroString = selectedPlannedNames[modeInc] + " " + intermediate.Multi_CurrentLineNames[lineInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_ModeTrends[metricInc]) + ": ";
                            }
                            else
                            {
                                labelIntroString = selectedPlannedNames[modeInc] + " " + getStringForEnum_Metric(ListofSelectedKPI_StepTrends[metricInc]) + ": ";
                            }

                            if (LineTrends_Step_analysistimeperiod == 1) //daily
                            {
                                //actual
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Daily_Planned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Daily_Planned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[lineInc][i].startTime.ToString("MM/dd"), Label = "Act. " + labelIntroString + Math.Round(value, 1) });
                                }
                                //step
                                for (int i = 0; i < intermediate.Trends_Step_MasterDataList_Daily_Planned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Step_MasterDataList_Daily_Planned[lineInc][modeIndex][metricIndex][i];
                                    newSeries2.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Daily[lineInc][i].startTime.ToString("MM/dd"), Label = "Step " + labelIntroString + Math.Round(value, 1) });
                                }
                            }
                            else if (LineTrends_Step_analysistimeperiod == 7) //weekly
                            {
                                //actual
                                for (int i = 0; i < intermediate.Trends_Mode_MasterDataList_Weekly_Planned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Mode_MasterDataList_Weekly_Planned[lineInc][modeIndex][metricIndex][i];
                                    newSeries.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[lineInc][i].startTime.ToString("MMM dd"), Label = "Act. " + labelIntroString + Math.Round(value, 1) });
                                }
                                //step
                                for (int i = 0; i < intermediate.Trends_Step_MasterDataList_Weekly_Planned[lineInc][modeIndex][metricIndex].Count; i++)
                                {
                                    double value = intermediate.Trends_Step_MasterDataList_Weekly_Planned[lineInc][modeIndex][metricIndex][i];
                                    newSeries2.DataPoints.Add(new CategoricalDataPoint { Value = value, Category = intermediate.Multi_AllSystemReports_Weekly[lineInc][i].startTime.ToString("MMM dd"), Label = "Step " + labelIntroString + Math.Round(value, 1) });
                                }
                            }

                            //wrap it up
                            newSeries.TrackBallInfoTemplate = blankDataTemplate;
                            newSeries2.TrackBallInfoTemplate = blankDataTemplate;
                            RDC_StepChange.Series.Add(newSeries);
                            RDC_StepChange.Series.Add(newSeries2);
                        }
                    }
                }
            }

            //format chart accordingly
            if (LineTrends_Step_analysistimeperiod == 1)
            {
                RDC_StepChange.HorizontalAxis.LabelInterval = 10;
            }
            else if (LineTrends_Step_analysistimeperiod == 7)
            {
                RDC_StepChange.HorizontalAxis.LabelInterval = 2;
            }

            RDC_StepChange.HorizontalAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.None;



        }

        #endregion

        #region PitStop
        private void PitStop_RT_ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            var tmpString = "";
            var tmpString2 = "";
            foreach (DataPointInfo info in e.Context.DataPointInfos)
            {
                // info.DisplayHeader = "Custom data point header";
                tmpString += info.DataPoint.Label + Environment.NewLine;
                tmpString2 = (info.DataPoint.Index / 2).ToString();
            }

            e.Header = "Time - " + tmpString2 + " minutes" + Environment.NewLine + Environment.NewLine + tmpString;
        }
        private void PitStop_RunTime_UpdateChartFromIntermediateSheet()
        {
            var blankDataTemplate = new DataTemplate("");
            RunTimeRadChart.Series.Clear();
            //   IList<string> tmpList = (IList<string>)PitStopRuntimeFailuremodeListbox.SelectedItems;
            List<string> selectedModes = new List<string>();// tmpList.to;
            for (int i = 0; i < PitStopRuntimeFailuremodeListbox.SelectedItems.Count; i++)
            {
                selectedModes.Add(PitStopRuntimeFailuremodeListbox.SelectedItems[i].ToString());
            }

            //first add overall line CDF
            if (selectedModes.Contains(intermediate.PitStop_RT_SYSTEMNAME))
            {
                CategoricalSeries series1 = new LineSeries();
                for (int i = 0; i < intermediate.PitStop_RT_LineCDF.GetLength(0); i++)
                {
                    series1.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.PitStop_RT_LineCDF[i], Category = intermediate.PitStop_RT_Xaxis[i], Label = intermediate.PitStop_RT_SYSTEMNAME + " " + Math.Round((100 * intermediate.PitStop_RT_LineCDF[i]), 0) + "%" });
                }
                series1.TrackBallInfoTemplate = blankDataTemplate;
                RunTimeRadChart.Series.Add(series1);
            }


            //now add failure modes
            for (int i = 0; i < selectedModes.Count; i++)// intermediate.PitStop_RT_ModeNames.Count; i++)
            {
                if (selectedModes[i] != intermediate.PitStop_RT_SYSTEMNAME)
                {
                    CategoricalSeries series2 = new LineSeries();
                    for (int j = 0; j < intermediate.PitStop_RT_ModeCDF[0].Count; j++)
                    {
                        series2.DataPoints.Add(new CategoricalDataPoint { Value = intermediate.PitStop_RT_ModeCDF[i - 1][j], Category = intermediate.PitStop_RT_Xaxis[j], Label = selectedModes[i] + " " + Math.Round((100 * intermediate.PitStop_RT_ModeCDF[i - 1][j]), 0) + "%" });
                    }
                    series2.DisplayName = intermediate.PitStop_RT_ModeNames[i];
                    series2.TrackBallInfoTemplate = blankDataTemplate;
                    RunTimeRadChart.Series.Add(series2);
                }
            }

            //finish up
            RunTimeRadChart.Palette = RuntimeChart_getChartColors();
            RunTimeRadChart.HorizontalAxis.LabelInterval = 10;
            RunTimeRadChart.VerticalAxis.Title = "Survival Probability";
            RunTimeRadChart.HorizontalAxis.Title = "Time (min)";
            RunTimeRadChart.HorizontalAxis.LabelFitMode = AxisLabelFitMode.None;
        }
        #endregion

        #region xSigma
        private void xSigma_Planned_UpdateChartFromIntermediateSheet()
        {
            /*
            xSigma_Planned_Chart.Series.Clear();
            this.xSigma_Planned_Chart.Palette = Trends_defaultChartColors();

            CandlestickSeries series = new CandlestickSeries();
            CategoricalSeries series2 = new LineSeries();
            CategoricalSeries series3 = new LineSeries();
            CategoricalSeries series4 = new LineSeries();
            CategoricalSeries series5 = new LineSeries();

            double xHigh; double xLow; double xOpen; double xClose;

            for (int tmpIndex = 0; tmpIndex < intermediate.xSigma_Planned_AnalysisPeriodReport.DT_Report.MappedDirectory_Planned.Count; tmpIndex++)
            {
   
                double Mu = intermediate.xSigma_Planned_RawStopValues[tmpIndex].Mean();
                double Sigma = intermediate.xSigma_Planned_Variations[tmpIndex];

                
                xHigh =  Math.Round(intermediate.xSigma_Planned_RawStopValues[tmpIndex].Max(),2);
                 xLow =  Math.Round(intermediate.xSigma_Planned_RawStopValues[tmpIndex].Min(),2);
                xOpen =  Math.Round(Mu + Sigma,2);
                xClose = Math.Round(Mu - Sigma,2);
                
                
                
                double xHigh = 20.1;// Math.Round(intermediate.xSigma_Planned_RawStopValues[tmpIndex].Max(),0);
                double xLow = 5.2; // Math.Round(intermediate.xSigma_Planned_RawStopValues[tmpIndex].Min(),0);
                double xOpen = 10.3;// Math.Round(Mu + Sigma,0);
                double xClose = 15.5; // Math.Round(Mu - Sigma,0);
                

                string tmpName = intermediate.xSigma_Planned_AnalysisPeriodReport.DT_Report.MappedDirectory_Planned[tmpIndex].Name;
                 series.DataPoints.Add(new OhlcDataPoint { Category = tmpName, High = xHigh, Low = xLow, Open = xOpen, Close = xClose  });

                series2.DataPoints.Add(new CategoricalDataPoint {Value = xHigh, Category = tmpName });
                series3.DataPoints.Add(new CategoricalDataPoint { Value = xLow, Category = tmpName });
                series4.DataPoints.Add(new CategoricalDataPoint { Value = xOpen, Category = tmpName });
                series5.DataPoints.Add(new CategoricalDataPoint { Value = xClose, Category = tmpName }); */
            //  }

            // xSigma_Planned_Chart.Series.Add(series);
            /*  xSigma_Planned_Chart.Series.Add(series2);
              xSigma_Planned_Chart.Series.Add(series3);
              xSigma_Planned_Chart.Series.Add(series4);
              xSigma_Planned_Chart.Series.Add(series5); */
            //  xSigma_Planned_Chart.HorizontalAxis.LabelFitMode = AxisLabelFitMode.Rotate;
        }

        private void xSigma_ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            var tmpString = "";
            foreach (DataPointInfo info in e.Context.DataPointInfos)
            {
                // info.DisplayHeader = "Custom data point header";
                tmpString += info.DataPoint.Label;
            }

            e.Header = tmpString;
        }

        private void SigmaControl_Unplanned_UpdateChartFromIntermediateSheet()
        {
            const double ControlChartStrokeThickness = 1;
            var blankDataTemplate = new DataTemplate("");
            double Sigma = intermediate.xSigma_Selected_ControlChart_StdDev[0];
            double Mu = intermediate.xSigma_Selected_ControlChart_Mean[0];
            CS_SPC_RAD.Series.Clear();
            this.CS_SPC_RAD.Palette = SigmaControl_getChartColors();

            CategoricalSeries series = new LineSeries();

            for (int j = 0; j < intermediate.xSigma_Selected_ControlChart_Value.Count; j++)
            {
                string tmplabelString = intermediate.xSigma_Selected_ControlChart_Dates[j].ToString("MMM dd yy") + ", " + intermediate.xSigma_Selected_ControlChart_Dates[j].ToString("yy") + Environment.NewLine + intermediate.xSigma_Selected_ControlChart_Value[j] + " Stops";
                series.DataPoints.Add(new CategoricalDataPoint { Label = tmplabelString, Value = intermediate.xSigma_Selected_ControlChart_Value[j], Category = intermediate.xSigma_Selected_ControlChart_Dates[j] });
            }
            series.TrackBallInfoTemplate = blankDataTemplate;
            CS_SPC_RAD.Series.Add(series);


            #region Data Series


            CategoricalSeries series1 = new LineSeries();
            for (int j = 0; j < intermediate.xSigma_Selected_ControlChart_Value.Count; j++)
            {
                series1.DataPoints.Add(new CategoricalDataPoint { Label = "", Value = Mu, Category = intermediate.xSigma_Selected_ControlChart_Dates[j] });
            }
            series1.TrackBallInfoTemplate = blankDataTemplate;
            ((LineSeries)series1).StrokeThickness = ControlChartStrokeThickness;
            CS_SPC_RAD.Series.Add(series1);

            CategoricalSeries series2 = new LineSeries();
            for (int j = 0; j < intermediate.xSigma_Selected_ControlChart_Value.Count; j++)
            {
                series2.DataPoints.Add(new CategoricalDataPoint { Label = "", Value = Mu + Sigma, Category = intermediate.xSigma_Selected_ControlChart_Dates[j] });
            }
            series2.TrackBallInfoTemplate = blankDataTemplate;
            ((LineSeries)series2).StrokeThickness = ControlChartStrokeThickness;
            CS_SPC_RAD.Series.Add(series2);

            CategoricalSeries series3 = new LineSeries();
            for (int j = 0; j < intermediate.xSigma_Selected_ControlChart_Value.Count; j++)
            {
                series3.DataPoints.Add(new CategoricalDataPoint { Label = "", Value = Mu + 2 * Sigma, Category = intermediate.xSigma_Selected_ControlChart_Dates[j] });
            }
            series3.TrackBallInfoTemplate = blankDataTemplate;
            ((LineSeries)series3).StrokeThickness = ControlChartStrokeThickness;
            CS_SPC_RAD.Series.Add(series3);

            CategoricalSeries series4 = new LineSeries();
            for (int j = 0; j < intermediate.xSigma_Selected_ControlChart_Value.Count; j++)
            {
                series4.DataPoints.Add(new CategoricalDataPoint { Label = "", Value = Mu + 3 * Sigma, Category = intermediate.xSigma_Selected_ControlChart_Dates[j] });
            }
            series4.TrackBallInfoTemplate = blankDataTemplate;
            ((LineSeries)series4).StrokeThickness = ControlChartStrokeThickness;
            CS_SPC_RAD.Series.Add(series4);

            if (Mu - Sigma > 0)
            {
                CategoricalSeries series5 = new LineSeries();
                for (int j = 0; j < intermediate.xSigma_Selected_ControlChart_Value.Count; j++)
                {
                    series5.DataPoints.Add(new CategoricalDataPoint { Label = "", Value = Mu - Sigma, Category = intermediate.xSigma_Selected_ControlChart_Dates[j] });
                }
                series5.TrackBallInfoTemplate = blankDataTemplate;
                ((LineSeries)series5).StrokeThickness = ControlChartStrokeThickness;
                CS_SPC_RAD.Series.Add(series5);

                if (Mu - 2 * Sigma > 0)
                {
                    CategoricalSeries series6 = new LineSeries();
                    for (int j = 0; j < intermediate.xSigma_Selected_ControlChart_Value.Count; j++)
                    {
                        series6.DataPoints.Add(new CategoricalDataPoint { Label = "", Value = Mu - 2 * Sigma, Category = intermediate.xSigma_Selected_ControlChart_Dates[j] });
                    }
                    series6.TrackBallInfoTemplate = blankDataTemplate;
                    ((LineSeries)series6).StrokeThickness = ControlChartStrokeThickness;
                    CS_SPC_RAD.Series.Add(series6);

                    if (Mu - 3 * Sigma > 0)
                    {
                        CategoricalSeries series7 = new LineSeries();
                        for (int j = 0; j < intermediate.xSigma_Selected_ControlChart_Value.Count; j++)
                        {
                            series7.DataPoints.Add(new CategoricalDataPoint { Label = "", Value = Mu - 3 * Sigma, Category = intermediate.xSigma_Selected_ControlChart_Dates[j] });
                        }
                        series7.TrackBallInfoTemplate = blankDataTemplate;
                        ((LineSeries)series7).StrokeThickness = ControlChartStrokeThickness;
                        CS_SPC_RAD.Series.Add(series7);
                    }
                }
            }
            #endregion
        }
        #endregion 
        #endregion

        #region Rate-o-Meter

        private void RateMeter_InitializeFromRawData()
        {

            Tuple<double, double> RateMeterData = intermediate.initializeRateTrainer();
            Gauge_Planned.Value = RateMeterData.Item1;
            PlannedDowntimecurrentvaluelabel.Content = "Current Planned Downtime: " + Math.Round(RateMeterData.Item1, 1) + "%";
            Gauge_PDTSetValue_Value.Content = Math.Round(RateMeterData.Item1, 1) + "%";
            Gauge_tickValue_Planned.Value = RateMeterData.Item1;
            Gauge_Rate_Scale.Min = intermediate.RateTrainer_RawAnalysis.Baseline_Rate * 0.6;
            Gauge_Rate_Scale.Max = intermediate.RateTrainer_RawAnalysis.Baseline_Rate * 1.2;
            Gauge_Rate.Value = intermediate.RateTrainer_RawAnalysis.Baseline_Rate;
            RateOMeterSetRate_Value.Content = intermediate.RateTrainer_RawAnalysis.Baseline_Rate;
            Gauge_Rate_marker.Value = intermediate.RateTrainer_RawAnalysis.Baseline_Rate;
            Gauge_tickValue_Rate_1.Value = intermediate.RateTrainer_RawAnalysis.Baseline_Rate;
            Gauge_Rate_CurrentRateLabel.Text = "Current Rate " + intermediate.RateTrainer_RawAnalysis.Baseline_Rate;
            RateTrainer_UpdateChartFromIntermediateSheet();
            double actOEE = intermediate.RateTrainer_RawAnalysis.Baseline_OEE * 100;
            double actUPDT = Math.Round(intermediate.AnalysisPeriodData.UPDTpct * 100, 0);
            Gauge_PDT_Scale1.Max = 100 - actOEE;
            Gauge_PDT_Scale2.Max = 100 - actOEE;

            Gauge_Range_Scale1.Max = intermediate.RateTrainer_RawAnalysis.Baseline_Rate * 1.2;
            Gauge_Range_Scale1.Min = intermediate.RateTrainer_RawAnalysis.Baseline_Rate * 0.6;

            //OUTPUT KPI 1 - OEE
            double minOEE = Math.Round(intermediate.RateTrainer_RawAnalysis.getMinOEE() * 100, 0);
            double maxOEE = Math.Round(intermediate.RateTrainer_RawAnalysis.getMaxOEE() * 100, 0);
            Gauge_KPI_One_Scale.Max = maxOEE + Math.Round(RateMeterData.Item1, 0);
            Gauge_KPI_One_Scale.Min = minOEE;

            Gauge_KPI_One_Range1.Min = minOEE;
            Gauge_KPI_One_Range1.Max = actOEE;
            Gauge_tickValue_KPI_1.Value = actOEE;
            Gauge_KPI_One_Range2.Min = actOEE;
            Gauge_KPI_One_Range2.Max = maxOEE + Math.Round(RateMeterData.Item1, 0);

            //OUTPUT KPI 2- THROUGHPUT
            double maxTput = Math.Round(intermediate.RateTrainer_RawAnalysis.getMaxTput() * 100, 0);
            double minTput = Math.Round(intermediate.RateTrainer_RawAnalysis.getMinTput() * 100, 0);
            Gauge_KPI_Two_Scale.Max = maxTput + Math.Round(RateMeterData.Item1 / 2, 0);
            Gauge_KPI_Two_Scale.Min = minTput;
            Gauge_tickValue_KPI_2.Value = 100;
            Gauge_KPI_Two_Range1.Min = minTput;
            Gauge_KPI_Two_Range2.Max = maxTput + Math.Round(RateMeterData.Item1 / 2, 0);

            //OUTPUT KPI 3 - UPDT
            Gauge_KPI_Three_Scale.Max = 100 - minOEE;

            // Gauge_KPI_Three_Range1.Min = 0;
            Gauge_KPI_Three_Range1.Max = actUPDT;
            Gauge_KPI_Three_Range2.Min = actUPDT;
            Gauge_KPI_Three_Range2.Max = 100 - minOEE;
            Gauge_tickValue_KPI_3.Value = actUPDT;

            //update card labels
            RateOMeter_CurrentUPDTValue.Content = Math.Round(intermediate.AnalysisPeriodData.UPDTpct * 100, 1) + "%";
            RateOMeter_NewUPDTValue.Content = Math.Round(intermediate.AnalysisPeriodData.UPDTpct * 100, 1) + "%";
            RateOMeter_NewOEEValue.Content = Math.Round(actOEE, 1) + "%";
            RateOMeter_CurrentOEEValue.Content = Math.Round(actOEE, 1) + "%";
        }




        public void RateMeter_PDTChanged(object sender, RoutedEventArgs e)
        {
            if (intermediate != null)
            {
                Tuple<double, double, double> outputData = intermediate.RateTrainer_RawAnalysis.UpdateInsensitiveLoss(Gauge_Planned.Value, Gauge_Rate.Value);

                Gauge_KPI_One.Value = outputData.Item1;
                Gauge_KPI_Two.Value = outputData.Item2;
                Gauge_KPI_Three.Value = Math.Max(100 - outputData.Item1 - Gauge_Planned.Value, 0);//outputData.Item3;

                RateOMeter_NewOEEValue.Content = Math.Round(outputData.Item1, 1) + "%";
                RateOMeter_NewUPDTValue.Content = Math.Round(Math.Max(100 - outputData.Item1 - Gauge_Planned.Value, 0), 1) + "%";
                RateOMeterSetRate_Value.Content = Math.Round(Gauge_Rate.Value, 1);

                Gauge_PDTSetValue_Value.Content = Math.Round(Gauge_Planned.Value, 1) + "%";

                RateTrainer_UpdateChartFromIntermediateSheet();
            }

        }

        public void RateMeter_SensitivityChanged(object sender, RoutedEventArgs e)
        {
            if (intermediate != null)
            {
                Tuple<double, double, double> outputData = intermediate.RateTrainer_RawAnalysis.UpdateSensitivity(Gauge_Sensitivity.Value, Gauge_Rate.Value);

                Gauge_KPI_One.Value = outputData.Item1;
                Gauge_KPI_Two.Value = outputData.Item2;
                Gauge_KPI_Three.Value = Math.Max(100 - outputData.Item1 - Gauge_Planned.Value, 0);//outputData.Item3;

                RateOMeter_NewOEEValue.Content = Math.Round(outputData.Item1, 1) + "%";
                RateOMeter_NewUPDTValue.Content = Math.Round(Math.Max(100 - outputData.Item1 - Gauge_Planned.Value, 0), 1) + "%";
                RateOMeterSetRate_Value.Content = Math.Round(Gauge_Rate.Value, 1);

                SensitivityFactorSetValue_Value.Content = Math.Round(Gauge_Sensitivity.Value, 1);
            }

            //  RateTrainer_UpdateChartFromIntermediateSheet();
        }

        public double RateMeter_MaxOEE_Rate { get { return RateMeter_OEE_Pin_Value; } }
        private double RateMeter_OEE_Pin_Value = 200;

        public void RateMeter_ValueChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (intermediate != null)
                {
                    //  Gauge_Rate_OEELabel.
                    RateMeter_OEE_Pin_Value = Gauge_Rate.Value;

                    Tuple<double, double, double> outputData = intermediate.RateTrainer_RawAnalysis.getOutputForSelectedValues(Gauge_Rate.Value);

                    Gauge_KPI_One.Value = outputData.Item1;
                    Gauge_KPI_Two.Value = outputData.Item2;
                    Gauge_KPI_Three.Value = Math.Max(100 - outputData.Item1 - Gauge_Planned.Value, 0);//outputData.Item3;

                    RateOMeter_NewOEEValue.Content = Math.Round(outputData.Item1, 1) + "%";
                    RateOMeter_NewUPDTValue.Content = Math.Round(Math.Max(100 - outputData.Item1 - Gauge_Planned.Value, 0), 1) + "%";
                    RateOMeterSetRate_Value.Content = Math.Round(Gauge_Rate.Value, 1);
                }
            }
            catch
            {
                Debugger.Break(); //THIS IS JUST HERE FOR TESTING! - sro 3/2/16
            }
        }

        #endregion

        #region Telerik Chart Palettes
        private string Color_HexFromPaletteEntry(ChartPalette palette, int I)
        {
            CategoricalSeries x = new LineSeries();
            PaletteEntry targetEntry = (PaletteEntry)palette.GetEntry(x, I);
            Brush targetBrush = targetEntry.Fill;

            int r = ((Color)targetBrush.GetValue(SolidColorBrush.ColorProperty)).R;
            int g = ((Color)targetBrush.GetValue(SolidColorBrush.ColorProperty)).G;
            int b = ((Color)targetBrush.GetValue(SolidColorBrush.ColorProperty)).B;

            return Color_HexFromRGB(r, g, b);
        }

        private static string Color_HexFromRGB(int r, int g, int b)
        {
            return System.Drawing.ColorTranslator.FromHtml(String.Format("#{0:X2}{1:X2}{2:X2}", r, g, b)).Name.Remove(0, 2);
        }

        public ChartPalette RateTrainer_getChartColors()
        {
            var tmp = new ChartPalette();
            addPaletteEntry(ref tmp, 0, 0, 0);
            addPaletteEntry(ref tmp, 255, 255, 0);
            addPaletteEntry(ref tmp, 255, 0, 0);
            addPaletteEntry(ref tmp, 0, 255, 255);
            addPaletteEntry(ref tmp, 255, 0, 255);
            return tmp;
        }

        public ChartPalette Trends_defaultChartColors()
        {
            var tmp = new ChartPalette();
            addPaletteEntry(ref tmp, 50, 205, 240);
            addPaletteEntry(ref tmp, 254, 118, 58);
            addPaletteEntry(ref tmp, 153, 192, 73);
            addPaletteEntry(ref tmp, 1, 149, 159);
            addPaletteEntry(ref tmp, 115, 127, 65);
            addPaletteEntry(ref tmp, 119, 199, 198);
            addPaletteEntry(ref tmp, 189, 171, 210);
            addPaletteEntry(ref tmp, 76, 74, 75);
            addPaletteEntry(ref tmp, 255, 175, 2);
            addPaletteEntry(ref tmp, 150, 76, 143);
            addPaletteEntry(ref tmp, 18, 135, 170);
            return tmp;
        }

        public ChartPalette StepChange_getChartColors()
        {
            double tintFactor = 0.8;
            byte R; byte G; byte B;
            var tmp = new ChartPalette();
            //line pair 
            R = 50; G = 205; B = 240;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 254; G = 118; B = 58;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 153; G = 192; B = 73;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 1; G = 149; B = 159;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 115; G = 127; B = 65;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 119; G = 199; B = 198;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 189; G = 171; B = 210;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 76; G = 74; B = 75;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 255; G = 175; B = 2;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 150; G = 76; B = 143;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);
            //line pair 
            R = 18; G = 135; B = 170;
            addPaletteEntry(ref tmp, (byte)(R + (255 - R) * tintFactor), (byte)(G + (255 - G) * tintFactor), (byte)(B + (255 - B) * tintFactor));
            addPaletteEntry(ref tmp, R, G, B);

            return tmp;
        }
        public ChartPalette SigmaControl_getChartColors()
        {
            var tmp = new ChartPalette();
            addPaletteEntry(ref tmp, 100, 100, 100);
            addPaletteEntry(ref tmp, 153, 255, 51);
            addPaletteEntry(ref tmp, 100, 100, 100);
            addPaletteEntry(ref tmp, 100, 100, 100);
            addPaletteEntry(ref tmp, 255, 124, 128);
            addPaletteEntry(ref tmp, 100, 100, 100);
            addPaletteEntry(ref tmp, 100, 100, 100);
            addPaletteEntry(ref tmp, 255, 124, 128);
            return tmp;
        }

        public ChartPalette RuntimeChart_getChartColors()
        {
            /*  var tmp = new ChartPalette();
              addPaletteEntry(ref tmp, 0, 0, 0);
              addPaletteEntry(ref tmp, 255, 124, 128);
              addPaletteEntry(ref tmp, 124, 255, 128);
              addPaletteEntry(ref tmp, 124, 100, 128);
              addPaletteEntry(ref tmp, 124, 255, 255);
              return tmp; */

            return Trends_defaultChartColors();
        }
        public ChartPalette FunnelChart_getChartColors()
        {
            var tmp = new ChartPalette();
            addPaletteEntry(ref tmp, 50, 205, 240);
            return tmp;
        }
        private void addPaletteEntry(ref ChartPalette palette, byte R, byte G, byte B)
        {
            var tmp = new PaletteEntry();
            tmp.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
            tmp.Stroke = new SolidColorBrush(Color.FromRgb(R, G, B));
            palette.GlobalEntries.Add(tmp);
        }
        #endregion

        #region Telerik List View 
        #region xSigma
        public ObservableCollection<xSigma_DisplayEvent> CS_ActiveDataCollection { get; set; } = new ObservableCollection<xSigma_DisplayEvent>();
        private void CS_PopulateUnplannedDataWindow()
        {
            CS_ActiveDataCollection.Clear();
            for (int i = 0; i < intermediate.xSigma_Unplanned_DataList.Count; i++)
            {
                CS_ActiveDataCollection.Add(intermediate.xSigma_Unplanned_DataList[i]);
            }
        }

        public ObservableCollection<xSigma_DisplayEvent> CS_Planned_ActiveDataCollection { get; set; } = new ObservableCollection<xSigma_DisplayEvent>();
        private void CS_Planned_PopulateUnplannedDataWindow()
        {
            CS_Planned_ActiveDataCollection.Clear();
            for (int i = 0; i < intermediate.xSigma_Planned_DataList.Count; i++)
            {
                CS_Planned_ActiveDataCollection.Add(intermediate.xSigma_Planned_DataList[i]);
            }
        }
        #endregion
        #region Loss Compass
        private ObservableCollection<DTevent> _ActiveDataCollection = new ObservableCollection<DTevent>();
        List<DTevent> _ActiveDataSortList = new List<DTevent>();
        public ObservableCollection<DTevent> ActiveDataCollection { get { return _ActiveDataCollection; } }

        public void populateRawDataWindow(string failuremodename, string cardname)
        {
            _ActiveDataCollection.Clear();
            List<DTevent> tmpList = intermediate.getRawData(failuremodename, cardname); //
            for (int i = 0; i < tmpList.Count; i++) { _ActiveDataCollection.Add(tmpList[i]); }
        }



        #endregion

        #endregion

        #endregion


    }
}

#endregion
