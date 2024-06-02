# Trigger Bar Detector

## Overview

The **Trigger Bar Detector** is a custom indicator for the Quantower trading platform. It identifies various types of momentum candles and significant swing points (highs and lows) on a price chart. This tool can help traders recognize key trading signals and potential reversal points.

## Features

- **Elephant Bars**: Detects large momentum candles (Elephant Bars) and marks them on the chart.
- **Tail Bars**: Identifies Tail Bars, which are candles with long wicks, indicating potential reversals.
- **Engulfing Bars**: Recognizes Bullish and Bearish Engulfing Bars, signaling strong reversal patterns.
- **Swing High/Low Detection**: Marks significant swing highs and lows on the chart, helping to identify key support and resistance levels.

## Installation

1. Clone the repository to your local machine:
    ```sh
    git clone https://github.com/yourusername/trigger-bar-detector.git
    ```

2. Open the solution in Visual Studio.

3. Build the solution to compile the indicator.

4. Load the compiled indicator into Quantower.

## Usage

1. In Quantower, open the chart where you want to apply the indicator.

2. Add the `TriggerBarDetector` indicator from your custom indicators list.

3. Configure the indicator settings as desired:
    - **ATR Period**: Period for the Average True Range calculation.
    - **Elephant Bars**: Enable/disable detection, set minimum size (ATR multiple), and body size percentage.
    - **Tail Bars**: Enable/disable detection, set minimum size (ATR multiple), and tail size percentage.
    - **Engulfing Bars**: Enable/disable detection, set minimum size (ATR multiple), and configure wick engulfing.
    - **Swing High/Low**: Enable/disable detection, set lookback period, and confirmation bars.

4. The indicator will mark detected bars and swing points on the chart with color-coded arrows.

## Settings

- **Elephant Bars**:
  - `detectElephantBars`: Enable/disable Elephant Bars detection.
  - `elephantMinSize`: Minimum size of Elephant Bar (ATR multiple).
  - `elephantBodySizePercent`: Minimum body size percentage of the total candle range.
  - `elephantBearishColor`: Color for Bearish Elephant Bars.
  - `elephantBullishColor`: Color for Bullish Elephant Bars.

- **Tail Bars**:
  - `detectTailBars`: Enable/disable Tail Bars detection.
  - `tailBarMinSize`: Minimum size of Tail Bar (ATR multiple).
  - `tailMinPercent`: Minimum tail size percentage of the total candle range.
  - `tailBearishColor`: Color for Bearish Tail Bars.
  - `tailBullishColor`: Color for Bullish Tail Bars.

- **Engulfing Bars**:
  - `detectEngulfingBars`: Enable/disable Engulfing Bars detection.
  - `floatAllowance`: Allowable float for gap openings.
  - `engulfingMinSize`: Minimum size of Engulfing Bar (ATR multiple).
  - `engulfWick`: Whether the candle needs to engulf the wick as well.
  - `engulfingBearishColor`: Color for Bearish Engulfing Bars.
  - `engulfingBullishColor`: Color for Bullish Engulfing Bars.

- **Swing High/Low**:
  - `detectSwingHighLow`: Enable/disable Swing High/Low detection.
  - `swingLookback`: Lookback period for detecting swings.
  - `swingConfirmationBars`: Number of bars needed to confirm a swing point.
  - `swingHighColor`: Color for Swing High markers.
  - `swingLowColor`: Color for Swing Low markers.

## Contribution

Feel free to fork the repository, make changes, and submit pull requests. Your contributions are welcome!

## License

This project is licensed under the GNU General Public License v3.0. See the [LICENSE](LICENSE) file for details.

