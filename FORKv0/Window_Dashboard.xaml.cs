using Analytics;
using DataPersistancy;
using Helper;
using Microsoft.VisualBasic;
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
using System.Reflection;
using System.ServiceModel.Channels;
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
using Telerik.Windows.Controls.Map;
using Windows_Desktop.Properties;
using static Analytics.Constants;
using static DataPersistancy.GeneralIO;
using Excel = Microsoft.Office.Interop.Excel;


namespace Windows_Desktop
{
    public partial class dashboardwindow : Window
    {
        public static System.Windows.Input.MouseButtonEventArgs f { get; set; }
        public static EventArgs g { get; set; }
        public int Menuitemclicked_number = -1;
        public ONETReport ForkReport { get; set; }
        public List<string> OccupationNames { get; set; }
        public List<string> CanvasB1FileNames { get; set; }
        public List<string> CanvasB1FileNames_Net { get; set; } = new List<string>();
        public List<string> CanvasB1FileNames_Skill { get; set; } = new List<string>();
        public List<string> CanvasB1FileNames_Other { get; set; } = new List<string>();

        public ActiveLocations SelectedLocation { get; set; }


        private bool initComplete = false;
        private bool firstInitComplete = false;

        public string USER_NAME { get; set; } = "";
        public string ROLE_NAME { get; set; } = "";
        public string LOCATION_NAME { get; set; } = "";


        public void fork_onload(object sender, RoutedEventArgs e)
        {
            InitializeComponent();

            this.map.Provider = new BingRestMapProvider(MapMode.Aerial, true, Analytics.Constants.BING_MAPS_API_KEY);
            this.map2.Provider = new BingRestMapProvider(MapMode.Aerial, true, Analytics.Constants.BING_MAPS_API_KEY);
            this.miniJobMap.Provider = new BingRestMapProvider(MapMode.Aerial, true, Analytics.Constants.BING_MAPS_API_KEY);
            this.D_miniJobMap.Provider = new BingRestMapProvider(MapMode.Aerial, true, Analytics.Constants.BING_MAPS_API_KEY);

            LaunchCanvas.Visibility = Visibility.Visible;
            BallSummaryCanvas.Visibility = Visibility.Hidden;
            B2Canvas.Visibility = Visibility.Hidden;
            B1Canvas.Visibility = Visibility.Visible;
            Landing_Name_AutoCompleteBox.ItemsSource = demoNameList;
            Landing_Role_AutoCompleteBox.ItemsSource = demoRoleList;
            Landing_Location_AutoCompleteBox.ItemsSource = demoLocationList;

            MakeLaunchReady();
            ManageScreenResolution();

            Thread onetThread = new Thread(setONETReport);
            onetThread.Start();

        }

        private void setONETReport()
        {
            //import data
            ForkReport = new ONETReport();
            ForkReport.MasterOccupationList = JSON_IO.Import_OccupationList(Helper.Publics.FILENAMES.OCCUPATIONS + ".txt");
            ForkReport.MasterSkillList = JSON_IO.Import_AttributeList(Helper.Publics.FILENAMES.SKILLS + ".txt");
            ForkReport.MasterAbilityList = JSON_IO.Import_AttributeList(Helper.Publics.FILENAMES.ABILITIES + ".txt");
            ForkReport.MasterKnowledgeList = JSON_IO.Import_AttributeList(Helper.Publics.FILENAMES.KNOWLEDGE + ".txt");

            DemoIO.Demo_ImportGraph();

            //set listbox components
            OccupationNames = ForkReport.MasterOccupationList.Select(c => c.Name).ToList(); //sets CanvasA1 Listbox

            initComplete = true;
        }

        public void do_analyze(object sender, RoutedEventArgs e)
        {
            if (loadingInfoComplete)
            {
                HeaderTitleLabel.Content = USER_NAME + ".  " + ROLE_NAME + ".  " + LOCATION_NAME;
                LaunchCanvas.Visibility = Visibility.Hidden;

                if (LOCATION_NAME.Contains("AR"))
                {
                    SelectedLocation = ActiveLocations.AR;
                }
                else
                {
                    SelectedLocation = ActiveLocations.TN;
                }

                if (firstInitComplete)
                {
                    ToggleShowHide_CanvasD(sender, f);
                }
                else
                {
                    while (!initComplete) { Thread.Sleep(500); }
                    CanvasA_init();
                    CanvasB_init();
                    CanvasC_init();
                    CanvasD_init();

                    ToggleShowHide_CanvasD(sender, f);
                    firstInitComplete = true;
                }
            }
            else
            {
                MessageBox.Show("Please fill out your Name, Role, and Location and try again.");
            }
        }

        public bool loadingInfoComplete = false;
        public List<string> demoNameList = new List<string>(){ "Lisa Parmiter", "Nick Dalton", "Alan Jope", "Patty Hull", "Sam Odle", "Nataliya Wright", "Nick Psyhogeos" };
        public List<string> demoRoleList = new List<string>() { "Hand Packer", "Factory Technician" };
        public List<string> demoLocationList = new List<string>() { "Jonesboro, AR",  "Covington, TN"};
        private void Landing_Name_AutoCompleteBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Landing_Name_AutoCompleteBox.SelectedItem != null)
            {
                    USER_NAME = Landing_Name_AutoCompleteBox.SelectedItem.ToString();
            }

            CheckLoadInfoComplete();
        }

        private void Landing_Role_AutoCompleteBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Landing_Role_AutoCompleteBox.SelectedItem != null)
            {
                ROLE_NAME = Landing_Role_AutoCompleteBox.SelectedItem.ToString();
            }

            CheckLoadInfoComplete();
        }

        private void Landing_Location_AutoCompleteBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Landing_Location_AutoCompleteBox.SelectedItem != null)
            {
                LOCATION_NAME = Landing_Location_AutoCompleteBox.SelectedItem.ToString();
            }

            CheckLoadInfoComplete();
        }

        private void CheckLoadInfoComplete()
        {
            if(USER_NAME.Length > 1 && ROLE_NAME.Length > 1 && LOCATION_NAME.Length > 1)
            {
                loadingInfoComplete = true;
                GoButton.Cursor = Cursors.Hand;
                GoButton.Opacity = 1;
            }
            else if(loadingInfoComplete)
            {
                loadingInfoComplete = false;
                GoButton.Cursor = Cursors.Arrow;
                GoButton.Opacity = 0.5;

            }
        }

        public void ManageScreenResolution()     // To make it fit for use on any screen - maximize the program if screen resolution of device is less than a threshold, to make the UI legible
        {
            //Height = "706" Width = "1250"
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            if (screenWidth < 1250 | screenHeight < 706)
                this.WindowState = System.Windows.WindowState.Maximized;
        }

        #region Menu
        public void LaunchMenu(object sender, MouseButtonEventArgs e)
        {
            MenuCanvas.Visibility = Visibility.Visible;
            AnimateMenuOpening();
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

        public void CloseMenu(object sender, MouseButtonEventArgs e)
        {
            MenuSplashRectangle.Visibility = Visibility.Hidden;
            AnimateMenuClosing();
            System.Windows.Forms.Application.DoEvents();
            MenuCanvas.Visibility = Visibility.Hidden;


        }
        public void Ballmousemove(object sender, MouseEventArgs e)
        {
            if (sender.GetType().ToString().IndexOf("Ellipse") > -1)
            {
                Ellipse tempsender = (Ellipse)sender;
                tempsender.Opacity = 0.8;
            }
        }

        public void Ballmouseleave(object sender, MouseEventArgs e)
        {
            if (sender.GetType().ToString().IndexOf("Ellipse") > -1)
            {
                Ellipse tempsender = (Ellipse)sender;
                tempsender.Opacity = 1.0;
            }
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
                tempsender.Foreground = Brushes.Orange;
                getMenuItem_Canvas_fromitemindex(Menu_InternalInfiniteCanvas, menuitem).Background = BrushColors.mybrushverylightgray_forcardbackground;

            }
            Menuitemclicked_number = menuitem;
            CloseMenu(MenuCanvas, f);
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

        #region Launch Canvas

        private void CheckScreenResolution()
        {

            Double screenWidth = SystemParameters.PrimaryScreenWidth;
            Double screenHeight = SystemParameters.PrimaryScreenHeight;

            if (screenWidth < 1200 | screenHeight < 700)

                this.WindowState = System.Windows.WindowState.Maximized;
        }

        public void MakeLaunchReady()
        {
            int indexOffset = 0;

            CheckScreenResolution();

            string tempString = null;

            Settings.Default.Reload();

            Initialization_Support.verifyFolderStructure();
            tempString = "";


            HideAllDashboards();
        }
        #endregion

        #region Canvas A - Skill Chart
        public void CanvasA_init()
        {
            CanvasA1PopulateComboBox();
            CanvasA1_PopulateAutoCompleteList();

            CanvasA1ListBox.ItemsSource = null;
            CanvasA1ListBox.ItemsSource = OccupationNames;

            CanvasA1ListBox.SelectedIndex = 0;
        }

        public List<string> A1ListofSelectedOccupations = new List<string>();
        private Analytics.Constants.AttributeType A1_selctedAttribute = Analytics.Constants.AttributeType.Skill;

        private void CanvasA1PopulateComboBox()
        {
            CanvasA1ComboBox.ItemsSource = new List<string>(new string[] { Analytics.Constants.getStringForAttributeType(Analytics.Constants.AttributeType.Skill), Analytics.Constants.getStringForAttributeType(Analytics.Constants.AttributeType.Knowledge), Analytics.Constants.getStringForAttributeType(Analytics.Constants.AttributeType.Ability) });
            CanvasA1ComboBox.SelectedItem = Analytics.Constants.getStringForAttributeType(Analytics.Constants.AttributeType.Skill);
        }


        public void CanvasA1ComboBoxSelected(object sender, RoutedEventArgs e)
        {
            string selectedString = CanvasA1ComboBox.SelectedItem.ToString();
            AHeaderTitle.Content = selectedString + " Breakdown By Occupation";
            A1_selctedAttribute = Analytics.Constants.GetAttributeTypeFromString(selectedString);
            A1_RefreshChart();
        }


        private void CanvasA_ChartTrackBallBehavior_TrackInfoUpdated(object sender, TrackBallInfoEventArgs e)
        {
            var tmpString = "";
            foreach (DataPointInfo info in e.Context.DataPointInfos)
            {
                // info.DisplayHeader = "Custom data point header";
                tmpString += info.DataPoint.Label + Environment.NewLine;
            }

            e.Header = tmpString;
        }

        public void CanvasA1ListBoxSelected(object sender, RoutedEventArgs e)
        {
            //First clear the list
            A1ListofSelectedOccupations.Clear();

            //populate the list from listbox selecteditems
            for (int i = 0; i < CanvasA1ListBox.SelectedItems.Count; i++)
            {
                A1ListofSelectedOccupations.Add(CanvasA1ListBox.SelectedItems[i].ToString());
            }

            //Generate charts
            A1_RefreshChart();
        }

        private void A1_RefreshChart()
        {
            var blankDataTemplate = new DataTemplate("");
            CanvasA1Chart.Series.Clear();
            CanvasA1Chart.Palette = Trends_defaultChartColors_Legacy();

            //axis stuff
            CanvasA1Chart.VerticalAxis = new LinearAxis();
            var secondaryVAxis = new LinearAxis();
            secondaryVAxis.HorizontalLocation = AxisHorizontalLocation.Right;

            //find axis titles
            string AxisTitle1 = "Importance";
            string AxisTitle2 = "Level";

            //find selected items & indices
            var selectedOccupationName = new List<string>();
            var selectedOccupationIndeces = new List<int>();

            for (int i = 0; i < CanvasA1ListBox.SelectedItems.Count; i++)
            {
                selectedOccupationName.Add(CanvasA1ListBox.SelectedItems[i].ToString());
                selectedOccupationIndeces.Add(OccupationNames.IndexOf(CanvasA1ListBox.SelectedItems[i].ToString()));
            }

            CanvasA1Chart.VerticalAxis.Title = AxisTitle1;
            secondaryVAxis.Title = AxisTitle2;

            //for each occupation...
            for (int occupationInc = 0; occupationInc < selectedOccupationIndeces.Count; occupationInc++)
            {
                int occupationIndex = selectedOccupationIndeces[occupationInc];
                var tmpAttributeList = ForkReport.MasterOccupationList[occupationIndex].getAttributesByType(A1_selctedAttribute);

                //get series type right
                CategoricalSeries newSeriesA = new BarSeries();
                CategoricalSeries newSeriesB = new BarSeries();
                //for each attribute
                for (int attributeInc = 0; attributeInc < tmpAttributeList.Count; attributeInc++)
                {
                    string labelIntroStringA = selectedOccupationName[occupationInc] + " " + tmpAttributeList[attributeInc].Name + " " + Analytics.Constants.getStringForAttributeType(A1_selctedAttribute) + " Importance: ";
                    double valueA = tmpAttributeList[attributeInc].Importance.Value;
                    newSeriesA.DataPoints.Add(new CategoricalDataPoint { Value = valueA, Category = tmpAttributeList[attributeInc].Name, Label = labelIntroStringA + Math.Round(valueA, 1) });

                    string labelIntroStringB = selectedOccupationName[occupationInc] + " " + tmpAttributeList[attributeInc].Name + " " + Analytics.Constants.getStringForAttributeType(A1_selctedAttribute) + " Level: ";
                    double valueB = tmpAttributeList[attributeInc].Level.Value;
                    newSeriesB.DataPoints.Add(new CategoricalDataPoint { Value = valueB, Category = tmpAttributeList[attributeInc].Name, Label = labelIntroStringB + Math.Round(valueB, 1) });
                }
                newSeriesB.VerticalAxis = secondaryVAxis;
                //wrap it up
                newSeriesA.TrackBallInfoTemplate = blankDataTemplate;
                CanvasA1Chart.Series.Add(newSeriesA);
                newSeriesB.TrackBallInfoTemplate = blankDataTemplate;
             //   CanvasA1Chart.Series.Add(newSeriesB);
            }
            CanvasA1Chart.HorizontalAxis.LabelInterval = 1;
            CanvasA1Chart.HorizontalAxis.LabelFitMode = Telerik.Charting.AxisLabelFitMode.Rotate;
        }


        public void CanvasA1_AutoCompleteBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            List<int> TempselecteditemsinListBox = new List<int>();
            if (CanvasA1_AutoCompleteBox.SelectedItem != null)
            {
                if (OccupationNames.IndexOf(CanvasA1_AutoCompleteBox.SelectedItem.ToString()) != -1)
                {
                    CanvasA1ListBox.SelectedIndex = OccupationNames.IndexOf(CanvasA1_AutoCompleteBox.SelectedItem.ToString());
                }
            }
        }
        private ObservableCollection<String> CanvasA1_OccupationList = new ObservableCollection<string>();
        public void CanvasA1_PopulateAutoCompleteList()
        {
            CanvasA1_AutoCompleteBox.SearchText = "";
            CanvasA1_OccupationList.Clear();

            for (int j = 0; j < OccupationNames.Count; j++)
            {
                CanvasA1_OccupationList.Add(OccupationNames[j]);
            }
            CanvasA1_AutoCompleteBox.ItemsSource = OccupationNames;
        }

        #endregion

        #region Canvas B - Jobs



        private void CanvasB_init()
        {
            //CanvasB1ListBox.ItemsSource = null;
           // B1Canvas.Visibility = Visibility.Visible;
           // B2Canvas.Visibility = Visibility.Hidden;
           // B3Canvas.Visibility = Visibility.Hidden;
           
        }

        #region B - Tab Navigation

        public void CanvasBHeader1Clicked(object sender, MouseButtonEventArgs e)
        {
            // B1_CurrentState = B1_State.Net;
            // B1_SetListboxSource();

            CanvasBHeaderLabel1.FontWeight = FontWeights.DemiBold;
            CanvasBHeaderLabel2.FontWeight = FontWeights.Regular;

            CanvasBSelectionBar1.Visibility = Visibility.Visible;
            CanvasBSelectionBar2.Visibility = Visibility.Hidden;
            ///CanvasBSelectionBar3.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 95, 0.2, WidthProperty, CanvasBSelectionBar1);

            B1Canvas.Visibility = Visibility.Visible;
            B2Canvas.Visibility = Visibility.Hidden;
            B3Canvas.Visibility = Visibility.Hidden;

        }
        public void CanvasBHeader2Clicked(object sender, MouseButtonEventArgs e)
        {

            CanvasBHeaderLabel2.FontWeight = FontWeights.DemiBold;
            CanvasBHeaderLabel1.FontWeight = FontWeights.Regular;

            //   B1_CurrentState = B1_State.Skill;
            //  B1_SetListboxSource();
            CanvasBSelectionBar2.Visibility = Visibility.Visible;
            CanvasBSelectionBar1.Visibility = Visibility.Hidden;
            //CanvasBSelectionBar3.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 95, 0.2, WidthProperty, CanvasBSelectionBar2);

            B1Canvas.Visibility = Visibility.Hidden;
            B2Canvas.Visibility = Visibility.Visible;
            B3Canvas.Visibility = Visibility.Hidden;
        }

        #endregion


        private void B1_Location_Toggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (B1_Location_Unselected.Visibility == Visibility.Visible)
            {
                B1_Location_Unselected.Visibility = Visibility.Hidden;
                B1_Location_Selected.Visibility = Visibility.Visible;

                miniJobMap.Visibility = Visibility.Visible;
                B_JobLongDescription.Visibility = Visibility.Hidden;

                B_LocationButton.Background = BrushColors.fork_blue;
            }
            else
            {
                B1_Location_Unselected.Visibility = Visibility.Visible;
                B1_Location_Selected.Visibility = Visibility.Hidden;

                miniJobMap.Visibility = Visibility.Hidden;
                B_JobLongDescription.Visibility = Visibility.Visible;

                B_LocationButton.Background = BrushColors.mybrushlanguagewhite;
            }


        }


        private void B_Favorite_Toggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (B_Favorite_Unselected.Visibility == Visibility.Hidden)
            {
                B_Favorite_Unselected.Visibility = Visibility.Visible;
                B_Favorite_Selected.Visibility = Visibility.Hidden;
            }
            else
            {
                B_Favorite_Unselected.Visibility = Visibility.Hidden;
                B_Favorite_Selected.Visibility = Visibility.Visible;
            }

        }

        #endregion

        #region Canvas C - Training
        public void CanvasC_init()
        {

        }



        public void CanvasCHeader1Clicked(object sender, MouseButtonEventArgs e)
        {

            CanvasCHeaderLabel1.FontWeight = FontWeights.DemiBold;
           // CanvasCHeaderLabel2.FontWeight = FontWeights.Regular;

            CanvasCSelectionBar1.Visibility = Visibility.Visible;
            CanvasCSelectionBar2.Visibility = Visibility.Hidden;
            ///CanvasBSelectionBar3.Visibility = Visibility.Hidden;
            AnimateZoomUIElement(0, 95, 0.2, WidthProperty, CanvasCSelectionBar1);

            C1Canvas.Visibility = Visibility.Visible;
        }

        private void C1_Location_Toggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (C1_Location_Unselected.Visibility == Visibility.Visible)
            {
                C1_Location_Unselected.Visibility = Visibility.Hidden;
                C1_Location_Selected.Visibility = Visibility.Visible;

                C_miniJobMap.Visibility = Visibility.Visible;
                C_JobLongDescription.Visibility = Visibility.Hidden;

                C_LocationButton.Background = BrushColors.fork_blue;
            }
            else
            {
                C1_Location_Unselected.Visibility = Visibility.Visible;
                C1_Location_Selected.Visibility = Visibility.Hidden;

                C_miniJobMap.Visibility = Visibility.Hidden;
                C_JobLongDescription.Visibility = Visibility.Visible;

                C_LocationButton.Background = BrushColors.mybrushlanguagewhite;
            }


        }


        private void C_Favorite_Toggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (C_Favorite_Unselected.Visibility == Visibility.Hidden)
            {
                C_Favorite_Unselected.Visibility = Visibility.Visible;
                C_Favorite_Selected.Visibility = Visibility.Hidden;
            }
            else
            {
                C_Favorite_Unselected.Visibility = Visibility.Hidden;
                C_Favorite_Selected.Visibility = Visibility.Visible;
            }

        }


        #endregion

        #region Canvas D - Career Pathfinder

        private CPM_Graph D_Graph;
        private CPM_Node D_ActiveNode;
        private string D_LinkOne;
        private string D_LinkTwo;

        private void CanvasD_init()
        {

            D_StrengthCanvas.Visibility = Visibility.Visible;
            D_ListCanvas.Visibility = Visibility.Hidden;
            D_MapCanvas.Visibility = Visibility.Hidden;

            D_Graph = new CPM_Graph(DemoIO.nodes.First(n => n.Name == ROLE_NAME), SelectedLocation);

            D_UpdateUIFromGraph();
        }



        private void D_UpdateSideCardFromSelection(CPM_Node n)
        {
            D_CardHeader.Content = n.Name;
            D_CardSalary.Content = n.Salary;

            if (n.Growth > 0)
            {
                D_CardGrowth.Content = Math.Round(n.Growth, 1) + "% Growth";
                D_GrowthUp.Visibility = Visibility.Visible;
                D_GrowthDown.Visibility = Visibility.Hidden;
                D_GrowthSide.Visibility = Visibility.Hidden;
            }
            else if (n.Growth < 0)
            {
                D_CardGrowth.Content = Math.Round(n.Growth, 1) + "% Decline";
                D_GrowthUp.Visibility = Visibility.Hidden;
                D_GrowthDown.Visibility = Visibility.Visible;
                D_GrowthSide.Visibility = Visibility.Hidden;
            }
            else
            {
                D_CardGrowth.Content = "Stable Outlook";
                D_GrowthUp.Visibility = Visibility.Hidden;
                D_GrowthDown.Visibility = Visibility.Hidden;
                D_GrowthSide.Visibility = Visibility.Visible;
            }

            D_CardHighlight.Text = n.Summary;

            if(n.Actions.Count > 0)
            {
                D_OppA_Image.Visibility = Visibility.Visible;
                D_OppA_Text.Visibility = Visibility.Visible;
                D_OppA_ImageB.Visibility = Visibility.Visible;

                D_OppA_Text.Text = n.Actions[0].Item1;
                D_LinkOne = n.Actions[0].Item2;

                if(n.Actions.Count > 1)
                {
                    D_OppB_Image.Visibility = Visibility.Visible;
                    D_OppB_Text.Visibility = Visibility.Visible;
                    D_OppB_ImageB.Visibility = Visibility.Visible;

                    D_OppB_Text.Text = n.Actions[1].Item1;
                    D_LinkTwo = n.Actions[1].Item2;
                }
                else
                {
                    D_OppB_Image.Visibility = Visibility.Hidden;
                    D_OppB_Text.Visibility = Visibility.Hidden;
                    D_OppB_ImageB.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                D_OppA_Image.Visibility = Visibility.Hidden;
                D_OppA_Text.Visibility = Visibility.Hidden;
                D_OppB_Image.Visibility = Visibility.Hidden;
                D_OppB_Text.Visibility = Visibility.Hidden;
                D_OppA_ImageB.Visibility = Visibility.Hidden;
                D_OppB_ImageB.Visibility = Visibility.Hidden;
            }


            if(n.Strengths.Count > 0)
            {
                D_StrengthA_Image.Visibility = Visibility.Visible;
                D_StrengthA_Text.Visibility = Visibility.Visible;

                D_StrengthA_Text.Text = n.Strengths[0];

                if(n.Strengths.Count > 1)
                {
                    D_StrengthB_Image.Visibility = Visibility.Visible;
                    D_StrengthB_Text.Visibility = Visibility.Visible;

                    D_StrengthB_Text.Text = n.Strengths[1];
                }
            }
            else
            {
                D_StrengthA_Image.Visibility = Visibility.Hidden;
                D_StrengthA_Text.Visibility = Visibility.Hidden;
                D_StrengthB_Image.Visibility = Visibility.Hidden;
                D_StrengthB_Text.Visibility = Visibility.Hidden;
            }

        }

        private void D_UpdateUIFromGraph()
        {
            Label_1A_Role.Content = TruncateLongString(D_Graph.OneA.Name);
            Label_1A_Pay.Content = "  " + D_Graph.OneA.Salary;

            Label_2A_Role.Content = TruncateLongString(D_Graph.TwoA.Name);
            Label_2A_Pay.Content = "  " + D_Graph.TwoA.Salary;

            Label_2B_Role.Content = TruncateLongString(D_Graph.TwoB.Name);
            Label_2B_Pay.Content = "  " + D_Graph.TwoB.Salary;

            Label_3A_Role.Content = TruncateLongString(D_Graph.ThreeA.Name);
            Label_3A_Pay.Content = "  " + D_Graph.ThreeA.Salary;

            Label_3B_Role.Content = TruncateLongString(D_Graph.ThreeB.Name);
            Label_3B_Pay.Content = "  " + D_Graph.ThreeB.Salary;

            Label_3C_Role.Content = TruncateLongString(D_Graph.ThreeC.Name);
            Label_3C_Pay.Content = "  " + D_Graph.ThreeC.Salary;

            Label_3D_Role.Content = TruncateLongString(D_Graph.ThreeD.Name);
            Label_3D_Pay.Content = "  " + D_Graph.ThreeD.Salary;
        }

        public string TruncateLongString(string str, int maxLength = 25)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }


        private void D_DeleteActiveNode(object sender, MouseButtonEventArgs e)
        {
            D_Graph.Delete_Node(D_ActiveNode);
            BallSummaryCanvas.Visibility = Visibility.Hidden;
            D_UpdateUIFromGraph();
            D_SetAllBallColors();
        }


        private void Ball_Generic_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BallSummaryCanvas.Visibility = Visibility.Visible;

            if (sender.GetType().ToString().IndexOf("Ellipse") > -1)
            {
                Ellipse tempsender = (Ellipse)sender;
                D_SetAllBallColors();
                tempsender.Fill = BrushColors.fork_blue;

                if (tempsender.Name.Contains("2A"))
                {
                    D_UpdateSideCardFromSelection(D_Graph.TwoA);
                    D_ActiveNode = D_Graph.TwoA;
                } 
                else if (tempsender.Name.Contains("2B"))
                {
                    D_UpdateSideCardFromSelection(D_Graph.TwoB);
                    D_ActiveNode = D_Graph.TwoB;
                }
                else if (tempsender.Name.Contains("3A"))
                {
                    D_UpdateSideCardFromSelection(D_Graph.ThreeA);
                    D_ActiveNode = D_Graph.ThreeA;
                }
                else if (tempsender.Name.Contains("3B"))
                {
                    D_UpdateSideCardFromSelection(D_Graph.ThreeB);
                    D_ActiveNode = D_Graph.ThreeB;
                }
                else if (tempsender.Name.Contains("3C"))
                {
                    D_UpdateSideCardFromSelection(D_Graph.ThreeC);
                    D_ActiveNode = D_Graph.ThreeC;
                }
                else if (tempsender.Name.Contains("3D"))
                {
                    D_UpdateSideCardFromSelection(D_Graph.ThreeD);
                    D_ActiveNode = D_Graph.ThreeD;
                }

            }

        }

        private void D_SetAllBallColors()
        {
            Ball_2A.Fill = BrushColors.aliceblue;
            Ball_2B.Fill = BrushColors.aliceblue;
            Ball_3A.Fill = BrushColors.aliceblue;
            Ball_3B.Fill = BrushColors.aliceblue;
            Ball_3C.Fill = BrushColors.aliceblue;
            Ball_3D.Fill = BrushColors.aliceblue;
        }

        private void Ball_1A_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(Ball_1A.Fill == BrushColors.ball_full)
            {
                Ball_1A.Fill = BrushColors.ball_full;
            }
            else
            {
                Ball_1A.Fill = BrushColors.aliceblue;
            }
        }


        private void D_Location_Toggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (D_Location_Unselected.Visibility == Visibility.Visible)
            {
                D_Location_Unselected.Visibility = Visibility.Hidden;
                D_Location_Selected.Visibility = Visibility.Visible;

                D_List_Unselected.Visibility = Visibility.Visible;
                D_List_Selected.Visibility = Visibility.Hidden;
                D_ListButton.Background = BrushColors.mybrushlanguagewhite;

                D_StrengthCanvas.Visibility = Visibility.Hidden;
                D_ListCanvas.Visibility = Visibility.Hidden;
                D_MapCanvas.Visibility = Visibility.Visible;

                D_LocationButton.Background = BrushColors.fork_blue;
            }
            else
            {
                D_Location_Unselected.Visibility = Visibility.Visible;
                D_Location_Selected.Visibility = Visibility.Hidden;

                D_StrengthCanvas.Visibility = Visibility.Visible;
                D_ListCanvas.Visibility = Visibility.Hidden;
                D_MapCanvas.Visibility = Visibility.Hidden;

                D_LocationButton.Background = BrushColors.mybrushlanguagewhite;
            }


        }

        private void D_List_Toggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (D_List_Unselected.Visibility == Visibility.Visible)
            {
                D_List_Unselected.Visibility = Visibility.Hidden;
                D_List_Selected.Visibility = Visibility.Visible;

                D_Location_Unselected.Visibility = Visibility.Visible;
                D_Location_Selected.Visibility = Visibility.Hidden;
                D_LocationButton.Background = BrushColors.mybrushlanguagewhite;

                D_StrengthCanvas.Visibility = Visibility.Hidden;
                D_ListCanvas.Visibility = Visibility.Visible;
                D_MapCanvas.Visibility = Visibility.Hidden;

                D_ListButton.Background = BrushColors.fork_blue;
            }
            else
            {
                D_List_Unselected.Visibility = Visibility.Visible;
                D_List_Selected.Visibility = Visibility.Hidden;

                D_StrengthCanvas.Visibility = Visibility.Visible;
                D_ListCanvas.Visibility = Visibility.Hidden;
                D_MapCanvas.Visibility = Visibility.Hidden;

                D_ListButton.Background = BrushColors.mybrushlanguagewhite;
            }


        }


        private void D_Favorite_Toggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (D_Favorite_Unselected.Visibility == Visibility.Hidden)
            {
                D_Favorite_Unselected.Visibility = Visibility.Visible;
                D_Favorite_Selected.Visibility = Visibility.Hidden;
            }
            else
            {
                D_Favorite_Unselected.Visibility = Visibility.Hidden;
                D_Favorite_Selected.Visibility = Visibility.Visible;
            }

        }


        private void D_OppA_ImageB_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(D_LinkOne);
        }

        private void D_OppB_ImageB_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(D_LinkTwo);
        }

        #endregion

        #region Mouse Move/Leave/Down
        public void Generalmousemove(object sender, EventArgs e)
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

        public void Generalmouseleave(object sender, EventArgs e)
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





        #endregion

        #region General UI Functions

        #region Show/Hide
        public void ToggleShowHide_CanvasA(object sender, MouseButtonEventArgs e)
        {
            if (ContentCanvasA.Visibility != Visibility.Visible)
            {
                HideAllDashboards();
              //  HeaderTitleLabel.Content = "Skill Explorer";
                ContentCanvasA.Visibility = Visibility.Visible;
              //  CanvasA1ListBox.SelectedIndex = 0;
            }
        }

        public void ToggleShowHide_CanvasB(object sender, MouseButtonEventArgs e)
        {
            if (ContentCanvasB.Visibility != Visibility.Visible)
            {
                HideAllDashboards();
             //   HeaderTitleLabel.Content = "Next Steps";
                ContentCanvasB.Visibility = Visibility.Visible;
               //CanvasB1ListBox.SelectedIndex = 0;
            }
        }

        public void ToggleShowHide_CanvasC(object sender, MouseButtonEventArgs e)
        {
            if (ContentCanvasC.Visibility != Visibility.Visible)
            {
                HideAllDashboards();
              //  HeaderTitleLabel.Content = "Job Maps";
                ContentCanvasC.Visibility = Visibility.Visible;
            }
        }

        public void ToggleShowHide_CanvasD(object sender, MouseButtonEventArgs e)
        {
            if (ContentCanvasD.Visibility != Visibility.Visible)
            {
                HideAllDashboards();
              //  HeaderTitleLabel.Content = "Discover Career Paths";
                ContentCanvasD.Visibility = Visibility.Visible;
            }
        }

        public void ToggleShowHide_CanvasE(object sender, MouseButtonEventArgs e)
        {
            if (ContentCanvasE.Visibility != Visibility.Visible)
            {
                HideAllDashboards();
              //  HeaderTitleLabel.Content = "Profile";
                ContentCanvasE.Visibility = Visibility.Visible;
            }
        }

        public void ToggleShowHide_CanvasF(object sender, MouseButtonEventArgs e)
        {
            if (ContentCanvasF.Visibility != Visibility.Visible)
            {
                HideAllDashboards();
                ContentCanvasF.Visibility = Visibility.Visible;
            }
        }


        public void ToggleShowHide_Exit(object sender, MouseButtonEventArgs e)
        {
            HideAllDashboards();
            LaunchCanvas.Visibility = Visibility.Visible;
        }


        public void HideAllDashboards()
        {
            ContentCanvasA.Visibility = Visibility.Hidden;
            ContentCanvasB.Visibility = Visibility.Hidden;
            ContentCanvasC.Visibility = Visibility.Hidden;
            ContentCanvasD.Visibility = Visibility.Hidden;
            ContentCanvasE.Visibility = Visibility.Hidden;
            ContentCanvasF.Visibility = Visibility.Hidden;
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
        #endregion

        #region Telerik

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
            addPaletteEntry(ref tmp, 50, 205, 240);
            addPaletteEntry(ref tmp, 254, 118, 58);
            addPaletteEntry(ref tmp, 254, 118, 58);
            addPaletteEntry(ref tmp, 153, 192, 73);
            addPaletteEntry(ref tmp, 153, 192, 73);
            addPaletteEntry(ref tmp, 1, 149, 159);
            addPaletteEntry(ref tmp, 1, 149, 159);
            addPaletteEntry(ref tmp, 115, 127, 65);
            addPaletteEntry(ref tmp, 115, 127, 65);
            addPaletteEntry(ref tmp, 119, 199, 198);
            addPaletteEntry(ref tmp, 119, 199, 198);
            addPaletteEntry(ref tmp, 189, 171, 210);
            addPaletteEntry(ref tmp, 189, 171, 210);
            addPaletteEntry(ref tmp, 76, 74, 75);
            addPaletteEntry(ref tmp, 76, 74, 75);
            addPaletteEntry(ref tmp, 255, 175, 2);
            addPaletteEntry(ref tmp, 255, 175, 2);
            addPaletteEntry(ref tmp, 150, 76, 143);
            addPaletteEntry(ref tmp, 150, 76, 143);
            addPaletteEntry(ref tmp, 18, 135, 170);
            addPaletteEntry(ref tmp, 18, 135, 170);
            return tmp;
        }

        public ChartPalette Trends_defaultChartColors_Legacy()
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

        #endregion

        //Finding needle in haystack functions - (get a control from within a canvas or a viewbox)
        #region Find UI Element in Canvas
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


        private void CanvasA1_AutoCompleteBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


    }

    static class BrushColors
    {
        public static SolidColorBrush fork_blue = new SolidColorBrush(Color.FromRgb(0,32,96));

        public static SolidColorBrush aliceblue = new SolidColorBrush(Color.FromRgb(240, 248, 255));
        public static SolidColorBrush ball_full = new SolidColorBrush(Color.FromRgb(176, 196, 222));

        public static SolidColorBrush bubblecolorGreen = new SolidColorBrush(Color.FromRgb(153, 255, 51));
        public static SolidColorBrush bubblecolorYellow = new SolidColorBrush(Color.FromRgb(255, 255, 102));
        public static SolidColorBrush bubblecolorOrange = new SolidColorBrush(Color.FromRgb(255, 178, 102));

        public static SolidColorBrush bubblecolorRed = new SolidColorBrush(Color.FromRgb(255, 124, 128));
        public static SolidColorBrush mybrushgray = new SolidColorBrush(Color.FromRgb(235, 235, 235));
        public static SolidColorBrush mybrushcolorlesswhite = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static SolidColorBrush LabelDefaultColor = new SolidColorBrush(Color.FromRgb(89, 89, 89));
        public static SolidColorBrush LabelSelectedColor = new SolidColorBrush(Color.FromRgb(50, 125, 168));
        public static SolidColorBrush CardHeaderdefaultColor = new SolidColorBrush(Color.FromRgb(101, 222, 200));
        public static SolidColorBrush mybrushshadowblack = new SolidColorBrush(Color.FromRgb(143, 143, 143));
        public static SolidColorBrush mybrushbrightblue = new SolidColorBrush(Color.FromRgb(44, 153, 195));
        public static SolidColorBrush mybrushbrightorange = new SolidColorBrush(Color.FromRgb(255, 181, 44));
        public static SolidColorBrush mybrushdarkgray = new SolidColorBrush(Color.FromRgb(143, 143, 143));
        public static SolidColorBrush mybrushlightgray = new SolidColorBrush(Color.FromRgb(170, 170, 170));
        public static SolidColorBrush mybrushlightgreen = new SolidColorBrush(Color.FromRgb(154, 216, 67));
        public static SolidColorBrush mybrushfontgray = new SolidColorBrush(Color.FromRgb(71, 71, 71));
        public static SolidColorBrush mybrushmodifiedGreen = new SolidColorBrush(Color.FromRgb(0, 200, 0));
        public static SolidColorBrush mybrushlanguagegreen = new SolidColorBrush(Color.FromRgb(69, 255, 69));
        public static SolidColorBrush mybrushlanguagewhite = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        //50, 205, 240
        //44,153,195
        public static SolidColorBrush mybrushnormalbargreencolor = new SolidColorBrush(Color.FromRgb(50, 205, 240));        // default color of charts
        public static SolidColorBrush mybrushhighlightedbargreencolor = new SolidColorBrush(Color.FromRgb(80, 215, 250));   // on mouse over color of charts 

        public static SolidColorBrush mybrushNOTSelectedCriteria = new SolidColorBrush(Color.FromRgb(230, 230, 230));


        public static SolidColorBrush mybrushverylightgray_forcardbackground = new SolidColorBrush(Color.FromRgb(248, 248, 248));

        public static SolidColorBrush mybrushLIGHTBLUEGREEN = new SolidColorBrush(Color.FromRgb(6, 197, 180));
        public static SolidColorBrush mybrushBLACK = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        public static SolidColorBrush mybrushLIGHTGRAY = new SolidColorBrush(Color.FromRgb(200, 200, 200));

        public static SolidColorBrush mybrushFunnelBlue = new SolidColorBrush(Color.FromRgb(33, 191, 207));
        public static SolidColorBrush mybrushFunnelGray = new SolidColorBrush(Color.FromRgb(190, 190, 190));

        //Standard Theme Colors
        public static SolidColorBrush mybrushSelectedCriteria = new SolidColorBrush(Color.FromRgb(50, 205, 240));  // Blue




    }

}


