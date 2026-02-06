using NightScout.Models;
using NightScout.Services;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NightScout
{
    public partial class NightScout : Form
    {
        private readonly NightscoutService _nightscoutService;
        private readonly System.Windows.Forms.Timer _timer;
        private GlucoseReading? _lastReading;
        private Icon? _currentIcon;

        public NightScout()
        {
            InitializeComponent();

            _nightscoutService = new NightscoutService();

            // Set up timer to fetch data every minute
            _timer = new System.Windows.Forms.Timer
            {
                Interval = 60000 // 60 seconds
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Initial fetch
            _ = FetchGlucoseDataAsync();
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            await FetchGlucoseDataAsync();
        }

        private async Task FetchGlucoseDataAsync()
        {
            try
            {
                var reading = await _nightscoutService.GetLatestGlucoseReadingAsync();
                if (reading != null)
                {
                    _lastReading = reading;
                    UpdateTaskbarIcon();
                    UpdateTooltip();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching data: {ex.Message}");
            }
        }

        private void UpdateTaskbarIcon()
        {
            if (_lastReading == null)
                return;

            // Dispose previous icon
            _currentIcon?.Dispose();

            // Create custom icon with glucose value and trend
            _currentIcon = CreateGlucoseIcon(_lastReading.BloodGlucoseMmol, _lastReading.DirectionArrow);
            
            // Force icon update by setting to null first, then to new icon
            this.Icon = null;
            this.Icon = _currentIcon;
            
            // Force refresh of the taskbar icon
            this.Refresh();
            Application.DoEvents();
        }

        private void UpdateTooltip()
        {
            if (_lastReading == null)
                return;

            this.Text = $"{_lastReading.DateTime:HH:mm} - {_lastReading.BloodGlucoseMmol} mmol/L {_lastReading.DirectionArrow}";
        }

        private static Icon CreateGlucoseIcon(double glucoseValue, string arrow)
        {
            const int iconSize = 32;
            using (var bitmap = new Bitmap(iconSize, iconSize))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Clear background
                graphics.Clear(Color.Transparent);

                // Determine background color based on glucose range
                Color backgroundColor = GetGlucoseRangeColor(glucoseValue);
                using (var backgroundBrush = new SolidBrush(backgroundColor))
                {
                    graphics.FillEllipse(backgroundBrush, 0, 0, iconSize - 1, iconSize - 1);
                }

                // Draw border
                using (var borderPen = new Pen(Color.White, 1))
                {
                    graphics.DrawEllipse(borderPen, 1, 1, iconSize - 3, iconSize - 3);
                }

                // Draw glucose value - increased font size and adjusted positioning
                string glucoseText = glucoseValue.ToString("F1");
                using (var font = new Font("Arial", 12f, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.Black))
                {
                    var textSize = graphics.MeasureString(glucoseText, font);
                    var textX = (iconSize - textSize.Width) / 2;
                    var textY = 5; // Move closer to top
                    graphics.DrawString(glucoseText, font, textBrush, textX, textY);
                }

                // Draw trend arrow - increased font size and moved closer to number
                using (var arrowFont = new Font("Arial", 7f, FontStyle.Bold))
                using (var arrowBrush = new SolidBrush(Color.Black))
                {
                    var arrowSize = graphics.MeasureString(arrow, arrowFont);
                    var arrowX = (iconSize - arrowSize.Width) / 2;
                    var arrowY = iconSize - arrowSize.Height - 2; // Move closer to bottom
                    graphics.DrawString(arrow, arrowFont, arrowBrush, arrowX, arrowY);
                }

                // Convert bitmap to icon with proper cleanup
                IntPtr hIcon = bitmap.GetHicon();
                return Icon.FromHandle(hIcon);
            }
        }

        private static Color GetGlucoseRangeColor(double glucoseValue)
        {
            return glucoseValue switch
            {
                < 3.4 => Color.Red,                     // Very Low
                >= 3.4 and <= 4.0 => Color.LightPink,   // Low
                >= 4.0 and <= 7.8 => Color.Green,       // In range
                > 7.8 and <= 10.0 => Color.Yellow,      // High
                _ => Color.Orange                       // Very high
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _nightscoutService?.Dispose();
                _currentIcon?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}