using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using TradingPlatform.BusinessLayer;

namespace TriggerBar;

public class TriggerBarDetector : Indicator
{
    //What to detect
    public bool    detectElephantBars         = true;
    public bool    detectTailBars             = true;
    public bool    detectEngulfingBars        = true;
    public bool    detectSwingHighLow         = true;

    // Elephant Bars
    public double  elephantMinSize            = 1.3;
    public double  elephantBodySizePercent    = 70.0;
    private Color  elephantBearishColor       = Color.Red;
    private Color  elephantBullishColor       = Color.Green;

    // Tail Bars
    public double  tailBarMinSize             = 1;
    public double  tailMinPercent             = 75.0;
    public bool    tailColorMatters           = false;
    private Color  tailBearishColor           = Color.Orange;
    private Color  tailBullishColor           = Color.PaleGreen;

    // Engulfing Bars
    public double  floatAllowance             = 0;                   // How many ticks to allow the open to be off by (good for gaps)
    public double  engulfingMinSize           = 1;                   // max wick size that disqualifies engulfing. Zero is disabled
    public bool    engulfWick                 = false;               // does the candle need to engulf wick as well?
    private Color  engulfingBearishColor      = Color.Pink;
    private Color  engulfingBullishColor      = Color.PowderBlue;

    // Pivot High/Low
    public int     swingLookback              = 10;
    public int     swingConfirmationBars      = 1;

    private Color  swingHighColor             = Color.DarkSeaGreen;
    private Color  swingLowColor              = Color.DarkSalmon;
    public int     lastSwingLowIndex          = -1;
    public int     lastSwingHighIndex         = -1;
    public int     lowestLowIndex             = -1;
    public int     highestHighIndex           = -1;

    public int     ATRPeriod                  = 14;
    private Indicator atr;

    public TriggerBarDetector()
        : base()
    {
        Name          = "Trigger Bar Detector";
        Description   = "Detects momentum candles and marks them on the chart.";

        AddLineSeries("BullishElephant",  Color.Purple,       0, LineStyle.Points).ShowLineMarker = false;
        AddLineSeries("BearishElephant",  Color.Orange,       0, LineStyle.Points).ShowLineMarker = false;
        AddLineSeries("BullishTail",      Color.PaleGreen,    0, LineStyle.Points).ShowLineMarker = false;
        AddLineSeries("BearishTail",      Color.Orange,       0, LineStyle.Points).ShowLineMarker = false;
        AddLineSeries("BullishEngulfing", Color.PowderBlue,   0, LineStyle.Points).ShowLineMarker = false;
        AddLineSeries("BearishEngulfing", Color.Pink,         0, LineStyle.Points).ShowLineMarker = false;
        AddLineSeries("SwingHigh",        Color.DarkSalmon,   0, LineStyle.Points).ShowLineMarker = false;
        AddLineSeries("SwingLow",         Color.DarkSeaGreen, 0, LineStyle.Points).ShowLineMarker = false;

        SeparateWindow = false;
        UpdateType = IndicatorUpdateType.OnBarClose;
    }

    protected override void OnInit()
    {
        // Initialize ATR indicator
        this.atr = Core.Indicators.BuiltIn.ATR(ATRPeriod, MaMode.SMA, IndicatorCalculationType.AllAvailableData);
        AddIndicator(this.atr);
    }

    protected override void OnUpdate(UpdateArgs args)
    {
        int index = this.Count - 1;
        if (index < ATRPeriod || index < swingLookback) return;

        this.atr.Calculate(this.HistoricalData);

        SetValue(High(), 0);
        SetValue(Low(),  1);
        SetValue(High(), 2); 
        SetValue(Low(),  3);
        SetValue(High(), 4); 
        SetValue(Low(),  5);
        SetValue(High(), 6); 
        SetValue(Low(),  7);

        BarType currentBarType = this.DetectCurrentBarType(0);
        UpdateMarker(currentBarType);
    }

    private void UpdateMarker(BarType currentBarType)
    {
        // Clear all markers at the current index
        foreach (var lineSeries in LinesSeries)
        {
            lineSeries.RemoveMarker(0);
        } 

        int markerOffset = 5; // distance between markers

        switch (currentBarType)
        {
            case BarType.BearishElephant:
                LinesSeries[0].SetMarker(0, new IndicatorLineMarker(elephantBearishColor,  upperIcon:  IndicatorLineMarkerIconType.DownArrow));
                break;
            case BarType.BullishElephant:
                LinesSeries[1].SetMarker(0, new IndicatorLineMarker(elephantBullishColor,  bottomIcon: IndicatorLineMarkerIconType.UpArrow));
                break;
            case BarType.BearishTail:
                LinesSeries[2].SetMarker(0, new IndicatorLineMarker(tailBearishColor,      upperIcon:  IndicatorLineMarkerIconType.DownArrow));
                break;
            case BarType.BullishTail:
                LinesSeries[3].SetMarker(0, new IndicatorLineMarker(tailBullishColor,      bottomIcon: IndicatorLineMarkerIconType.UpArrow));
                break;
            case BarType.BearishEngulfing:
                LinesSeries[4].SetMarker(0, new IndicatorLineMarker(engulfingBearishColor, upperIcon:  IndicatorLineMarkerIconType.DownArrow), -1 * markerOffset);
                break;
            case BarType.BullishEngulfing:
                LinesSeries[5].SetMarker(0, new IndicatorLineMarker(engulfingBullishColor, bottomIcon: IndicatorLineMarkerIconType.UpArrow), +1 * markerOffset);
                break;
            case BarType.SwingLow:
                LinesSeries[6].SetMarker(0, new IndicatorLineMarker(swingLowColor,         upperIcon: IndicatorLineMarkerIconType.DownArrow),-2 * markerOffset);
                break;
            case BarType.SwingHigh:
                LinesSeries[7].SetMarker(0, new IndicatorLineMarker(swingHighColor,        bottomIcon: IndicatorLineMarkerIconType.UpArrow),+2 * markerOffset);
                break;

            default:
                LinesSeries[0].RemoveMarker(0);
                LinesSeries[1].RemoveMarker(0);
                break;
        }
    }


    private BarType DetectCurrentBarType(int index)
    {
        BarType currentBarType = BarType.CommonBar;

        double closePrice      = Close();
        double openPrice       = Open();
        double lowPrice        = Low();
        double highPrice       = High();

        double prevClose       = Close(index + 1);
        double prevOpen        = Open( index + 1);
        double prevHigh        = High( index + 1);
        double prevLow         = Low(  index + 1);

        double lowerTail       = closePrice > openPrice ? openPrice - lowPrice   : closePrice - lowPrice;
        double upperTail       = closePrice > openPrice ? highPrice - closePrice : highPrice  - openPrice;
        double bodySize        = Math.Abs(closePrice - openPrice);
        double candleRange     = highPrice - lowPrice;
        double bodyPercent     = (bodySize / candleRange) * 100;
        double atrValue        = this.atr.GetValue(index);
        bool   isBullish       = closePrice > openPrice;
        double tailRatio       = lowerTail > upperTail ? (lowerTail / candleRange) * 100 : (upperTail / candleRange) * 100;

        // Detect Elephant Bar
        if (detectElephantBars && candleRange >= (elephantMinSize * atrValue) && bodyPercent >= elephantBodySizePercent)
            currentBarType = isBullish ? BarType.BullishElephant : BarType.BearishElephant;

        // Detect Tail bar
        if (detectTailBars && candleRange >= (tailBarMinSize * atrValue) && tailRatio >= tailMinPercent)
            currentBarType = lowerTail > upperTail ? BarType.BullishTail : BarType.BearishTail;

        // Detect Engulfing
        if (detectEngulfingBars && candleRange >= (engulfingMinSize * atrValue))
        {
            if (engulfWick)
            {
                if (     closePrice > openPrice && prevClose < prevOpen && openPrice < prevLow  && closePrice > prevHigh) currentBarType = BarType.BullishEngulfing;
                else if (closePrice < openPrice && prevClose > prevOpen && openPrice > prevHigh && closePrice < prevLow)  currentBarType = BarType.BearishEngulfing;
            }
            else
            {
                if (     closePrice > openPrice && prevClose < prevOpen && openPrice < prevClose && closePrice > prevOpen)  currentBarType = BarType.BullishEngulfing;
                else if (closePrice < openPrice && prevClose > prevOpen && openPrice > prevClose && closePrice < prevOpen) currentBarType = BarType.BearishEngulfing;
            }
        }

        // Detect Swing High/Low

        if (detectSwingHighLow)
        {
            if (IsSwingHigh(index))
                currentBarType = BarType.SwingHigh;
            else if (IsSwingLow(index))
                currentBarType = BarType.SwingLow;
        }

        return currentBarType;
    }
    private bool IsSwingLow(int index)
    {
        //if (index < swingLookback) return false;

        double currentHigh = High(index);
        for (int i = 1; i <= swingLookback; i++)
        {
            if (High(index - i) > currentHigh || High(index + i) > currentHigh)
                return false;
        }
        return true;
    }

    private bool IsSwingHigh(int index)
    {
        //if (index < swingLookback) return false;

        double currentLow = Low(index);
        for (int i = 1; i <= swingLookback; i++)
        {
            if (Low(index - i) < currentLow || Low(index + i) < currentLow)
                return false;
        }
        return true;
    }
    public enum BarType
    {
        BearishElephant,
        BullishElephant,
        BearishEngulfing,
        BullishEngulfing,
        BearishTail,
        BullishTail,
        SwingHigh,
        SwingLow,
        CommonBar,
    }


    public override IList<SettingItem> Settings
    {
        get
        {
            var settings = base.Settings;

            settings.Add(new SettingItemInteger("ATRPeriod", this.ATRPeriod)
            {
                Text = "ATR Period",
                SortIndex = 1
            });
            // ------------------------------------------------------------------
            // --------------  Elephant Bars ------------------------------------
            // ------------------------------------------------------------------
            settings.Add(new SettingItemBoolean("detectElephantBars", this.detectElephantBars)
            {
                Text = "----- Detect Elephant Bars -----",
                SortIndex = 10,
            });

            settings.Add(new SettingItemDouble("elephantMinSize", this.elephantMinSize)
            {
                Text = "Candle Minimum Size (ATR Multiple)",
                SortIndex = 11,
                Minimum = 0.1,
                Maximum = 5.0,
                DecimalPlaces = 2,
                Increment = 0.1
            });

            settings.Add(new SettingItemDouble("elephantBodySizePercent", this.elephantBodySizePercent)
            {
                Text = "Candle Body Size Percentage",
                SortIndex = 12,
                Minimum = 0.1,
                Maximum = 100.0,
                DecimalPlaces = 1,
                Increment = 0.1
            });

            settings.Add(new SettingItemColor("elephantBearishColor", this.elephantBearishColor)
            {
                Text = "Elephant Bearish Color",
                SortIndex = 13,
            });
            settings.Add(new SettingItemColor("elephantBullishColor", this.elephantBullishColor)
            {
                Text = "Elephant Bullish Color",
                SortIndex = 14,
            });

            // ------------------------------------------------------------
            // ---------------   Tail Bars --------------------------------
            // ------------------------------------------------------------
            settings.Add(new SettingItemBoolean("detectTailBars",     this.detectTailBars)
            {
                Text = "----- Detect Tail Bars ------",
                SortIndex = 20,
            });

            settings.Add(new SettingItemDouble("tailBarMinSize", this.tailBarMinSize)
            {
                Text = "Minimum Size (ATR Multiple)",
                SortIndex = 21,
                Minimum = 0.1,
                Maximum = 10,
                DecimalPlaces = 2,
                Increment = 0.1
            });

            settings.Add(new SettingItemDouble("tailMinPercent", this.tailMinPercent)
            {
                Text = "Min Tail Percentage",
                SortIndex = 22,
                Minimum = 0.1,
                Maximum = 100.0,
                DecimalPlaces = 1,
                Increment = 0.1
            });

            settings.Add(new SettingItemColor("tailBearishColor", this.tailBearishColor)
            {
                Text = "Tail Bar Bearish Color",
                SortIndex = 23,
            });
            settings.Add(new SettingItemColor("tailBullishColor", this.tailBullishColor)
            {
                Text = "Tail Bar Bullish Color",
                SortIndex = 24,
            });

            // -------------------------------------------------------------------
            // ------------- Engulfing Bars --------------------------------------
            // -------------------------------------------------------------------
            settings.Add(new SettingItemBoolean("detectEngulfingBars", this.detectEngulfingBars)
            {
                Text = "----- Detect Engulfing Bars -----",
                SortIndex = 30,
            });

            settings.Add(new SettingItemDouble("floatAllowance", this.floatAllowance)
            {
                Text = "Float Allowance",
                SortIndex = 31,
                Minimum = 0.0,
                Maximum = 10,
                DecimalPlaces = 2,
                Increment = 0.1
            });

            settings.Add(new SettingItemDouble("engulfingMinSizet", this.engulfingMinSize)
            {
                Text = "MaxiumWickSize (%)",
                SortIndex = 32,
                Minimum = 0.1,
                Maximum = 100.0,
                DecimalPlaces = 1,
                Increment = 0.1
            });

            settings.Add(new SettingItemBoolean("engulfWick", this.engulfWick)
            {
                Text = "Wick needs to be engulfed as well",
                SortIndex = 33,
            });

            settings.Add(new SettingItemColor("engulfingBearishColor", this.engulfingBearishColor)
            {
                Text = "Engulfing Bar Bearish Color",
                SortIndex = 34,
            });
            settings.Add(new SettingItemColor("engulfingBullishColor", this.engulfingBullishColor)
            {
                Text = "Engulfing Bullish Color",
                SortIndex = 35,
            });

            // ------------------------------------------------------------------
            // -------------- Swing Bars ----------------------------------------
            // ------------------------------------------------------------------
            settings.Add(new SettingItemBoolean("detectSwingHighLow", this.detectSwingHighLow)
            {
                Text = "----- Detect Swing High/Lows ----",
                SortIndex = 40,
            });

            settings.Add(new SettingItemInteger("swingLookback", this.swingLookback)
            {
                Text = "Swing Lookback",
                SortIndex = 41
            });

            settings.Add(new SettingItemInteger("swingConfirmationBars", this.swingConfirmationBars)
            {
                Text = "Swing Confirmation Bars",
                SortIndex = 42
            });


            settings.Add(new SettingItemColor("swingLowColor", this.swingLowColor)
            {
                Text = "Swing Low Color",
                SortIndex = 43,
            });
            settings.Add(new SettingItemColor("swingHighColor", this.swingHighColor)
            {
                Text = "Swing High Color",
                SortIndex = 44,
            });

            return settings;
        }
        set
        {
            base.Settings = value;

            // Update detection settings
            if (value.TryGetValue("detectElephantBars",      out bool   detectElephantBars))         this.detectElephantBars      = detectElephantBars;
            if (value.TryGetValue("detectTailBars",          out bool   detectTailBars))             this.detectTailBars          = detectTailBars;
            if (value.TryGetValue("detectEngulfingBars",     out bool   detectEngulfingBars))        this.detectEngulfingBars     = detectEngulfingBars;
            if (value.TryGetValue("detectSwingHighLow",      out bool   detectSwingHighLow))         this.detectSwingHighLow      = detectSwingHighLow;

            // Update elephant bar settings
            if (value.TryGetValue("ATRPeriod",               out int    atrPeriod))                  this.ATRPeriod               = atrPeriod;
            if (value.TryGetValue("elephantMinSize",         out double elephantMinSize))            this.elephantMinSize         = elephantMinSize;
            if (value.TryGetValue("elephantBodySizePercent", out double elephantBodySizePercent))    this.elephantBodySizePercent = elephantBodySizePercent;
            if (value.TryGetValue("elephantBearishColor",    out Color  elephantBearishColor))       this.elephantBearishColor    = elephantBearishColor;
            if (value.TryGetValue("elephantBullishColor",    out Color  elephantBullishColor))       this.elephantBullishColor    = elephantBullishColor;

            // Update Tail bar parameters
            if (value.TryGetValue("tailBarMinSize",          out double tailBarMinSize))             this.tailBarMinSize          = tailBarMinSize;
            if (value.TryGetValue("tailMinPercent",          out double tailMinPercent))             this.tailMinPercent          = tailMinPercent;
            if (value.TryGetValue("etailBearishColor",       out Color  tailBearishColor))           this.tailBearishColor        = tailBearishColor;
            if (value.TryGetValue("tailBullishColor",        out Color  tailBullishColor))           this.tailBullishColor        = tailBullishColor;

            // Update Engulfing bar paraeters
            if (value.TryGetValue("floatAllowance",          out double floatAllowance))             this.floatAllowance          = floatAllowance;
            if (value.TryGetValue("engulfingMinSize",        out double engulfingMinSize))           this.engulfingMinSize        = engulfingMinSize;
            if (value.TryGetValue("engulfWick",              out bool   engulfWick))                 this.engulfWick              = engulfWick;
            if (value.TryGetValue("engulfingBearishColor",   out Color  engulfingBearishColor))      this.engulfingBearishColor   = engulfingBearishColor;
            if (value.TryGetValue("engulfingBullishColor",   out Color  engulfingBullishColor))      this.engulfingBullishColor   = engulfingBullishColor;

            // Update Swing High Low parameters
            if (value.TryGetValue("swingLookback",           out int swingLookback))                 this.swingLookback           = swingLookback;
            if (value.TryGetValue("swingConfirmationBars",   out int swingConfirmationBars))         this.swingConfirmationBars   = swingConfirmationBars;

            if (value.TryGetValue("swingHighColor",          out Color  swingHighColor))             this.swingHighColor          = swingHighColor;
            if (value.TryGetValue("swingLowColor",           out Color  swingLowColor))              this.swingLowColor           = swingLowColor;

            this.OnSettingsUpdated();
        }
    }
 
}

