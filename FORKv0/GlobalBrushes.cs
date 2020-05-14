
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Media;

static class BrushColors
{
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

    public static SolidColorBrush mybrushLossLabelDefaultColors = new SolidColorBrush(Color.FromRgb(89, 89, 89));   // Loss label default gray colors

    public static SolidColorBrush mybrushNOTSelectedCriteria = new SolidColorBrush(Color.FromRgb(230, 230, 230));


    public static SolidColorBrush mybrushverylightgray_forcardbackground = new SolidColorBrush(Color.FromRgb(248, 248, 248));

    public static SolidColorBrush mybrushLIGHTBLUEGREEN = new SolidColorBrush(Color.FromRgb(6, 197, 180));
    public static SolidColorBrush mybrushBLACK = new SolidColorBrush(Color.FromRgb(0, 0, 0));
    public static SolidColorBrush mybrushLIGHTGRAY = new SolidColorBrush(Color.FromRgb(200, 200, 200));

    public static SolidColorBrush mybrushCRYSTALLBALLselected = new SolidColorBrush(Color.FromRgb(0, 170, 212));
    public static SolidColorBrush mybrushCRYSTALLBALL_NOT_selected = new SolidColorBrush(Color.FromRgb(240, 240, 240));

    public static SolidColorBrush mybrushFunnelBlue = new SolidColorBrush(Color.FromRgb(33, 191, 207));
    public static SolidColorBrush mybrushFunnelGray = new SolidColorBrush(Color.FromRgb(190, 190, 190));

    //Standard Theme Colors
    public static SolidColorBrush mybrushSelectedCriteria = new SolidColorBrush(Color.FromRgb(50, 205, 240));  // Blue




}


