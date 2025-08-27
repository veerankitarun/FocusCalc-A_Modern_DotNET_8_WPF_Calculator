using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calculator
{
    public partial class MainWindow : Window
    {
        private string _currentInput = "0";
        private double _accumulator = 0;
        private string? _pendingOp = null;     // "+", "−", "×", "÷"
        private bool _resetOnNextDigit = false;

        private string _expression = "";       // e.g., "5+"
        private bool _lastKeyWasOperator = false;

        public ObservableCollection<HistoryEntry> HistoryItems { get; } = new();
        private HistoryWindow? _historyWindow;

        // collapse state
        private bool _isCollapsed = false;
        private double _expandedHeight = 520; // remembered height

        public MainWindow()
        {
            InitializeComponent();
            SetTitleWithVersion();
            UpdateDisplay();
            Loaded += (_, __) => this.Focus();

            this.Closed += (_, __) => { _historyWindow?.Close(); _historyWindow = null; };
            this.LocationChanged += (_, __) => PositionHistoryWindow();
            this.SizeChanged += (_, __) => PositionHistoryWindow();
            this.StateChanged += (_, __) => PositionHistoryWindow();

            // Initialize collapse button visuals if present in XAML
            UpdateCollapseVisual();
        }

        private void SetTitleWithVersion()
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var product = asm.GetCustomAttribute<AssemblyProductAttribute>()?.Product
                          ?? asm.GetName().Name ?? "Calculator";
            var infoVer = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            var ver = infoVer ?? asm.GetName().Version?.ToString() ?? "1.0.0";
            this.Title = "FocusCalc";
        }

        // ===== Title bar handlers =====
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    // double-click toggles collapse
                    BtnCollapse_Click(sender, e);
                }
                else
                {
                    try { DragMove(); } catch { /* ignore */ }
                }
            }
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void BtnMax_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        /// <summary>
        /// Update the optional collapse button's Content/Tooltip if it exists in XAML.
        /// This avoids compile errors when no BtnCollapse is declared.
        /// </summary>
        private void UpdateCollapseVisual()
        {
            var btn = this.FindName("BtnCollapse") as Button; // may be null if not in XAML
            if (btn == null) return;

            btn.Content = _isCollapsed ? "▴" : "▾";
            btn.ToolTip = _isCollapsed ? "Expand" : "Collapse";
        }

        private void BtnCollapse_Click(object? sender, RoutedEventArgs? e)
        {
            if (!_isCollapsed)
            {
                _isCollapsed = true;
                _expandedHeight = this.Height > 0 ? this.Height : _expandedHeight;
                MainContent.Visibility = Visibility.Collapsed;

                // title bar height + margins
                double bar = TitleBar.ActualHeight > 0 ? TitleBar.ActualHeight : 36;
                double topMargins = 14 /*outer grid top*/ + 8 /*title bar bottom*/;
                this.MinHeight = bar + topMargins + 10;
                this.Height = this.MinHeight;
            }
            else
            {
                _isCollapsed = false;
                MainContent.Visibility = Visibility.Visible;
                this.Height = _expandedHeight;
            }

            // Only updates if a BtnCollapse exists
            UpdateCollapseVisual();
        }

        private static readonly CultureInfo CI = CultureInfo.InvariantCulture;

        private void UpdateDisplay()
        {
            if (_currentInput.Length > 1 && _currentInput.StartsWith("0") && !_currentInput.StartsWith("0."))
                _currentInput = _currentInput.TrimStart('0');
            if (_currentInput == "" || _currentInput == "-")
                _currentInput += "0";

            Display.Text = _expression.Length > 0
                ? (_lastKeyWasOperator ? _expression : _expression + _currentInput)
                : _currentInput;
        }

        private void Digit_Click(object sender, RoutedEventArgs e)
        {
            var digit = ((Button)sender).Content.ToString();
            if (_resetOnNextDigit) { _currentInput = "0"; _resetOnNextDigit = false; }
            _currentInput = _currentInput == "0" ? digit! : _currentInput + digit;
            _lastKeyWasOperator = false;
            UpdateDisplay();
        }

        private void Dot_Click(object sender, RoutedEventArgs e)
        {
            if (_resetOnNextDigit) { _currentInput = "0"; _resetOnNextDigit = false; }
            if (!_currentInput.Contains(".")) _currentInput += ".";
            _lastKeyWasOperator = false;
            UpdateDisplay();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _currentInput = "0"; _accumulator = 0; _pendingOp = null;
            _resetOnNextDigit = false; _expression = ""; _lastKeyWasOperator = false;
            History.Text = ""; UpdateDisplay();
        }

        private void Sign_Click(object sender, RoutedEventArgs e)
        {
            if (_currentInput.StartsWith("-")) _currentInput = _currentInput[1..];
            else if (_currentInput != "0") _currentInput = "-" + _currentInput;
            UpdateDisplay();
        }

        private void Percent_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(_currentInput, NumberStyles.Float, CI, out var val))
            {
                val /= 100.0; _currentInput = val.ToString(CI); UpdateDisplay();
            }
        }

        private void Op_Click(object sender, RoutedEventArgs e)
        {
            var op = ((Button)sender).Content.ToString(); // "+", "−", "×", "÷"
            if (!_lastKeyWasOperator)
            {
                ApplyPendingOperation();
                _expression += _currentInput + op;
            }
            else if (_expression.Length > 0)
            {
                _expression = _expression[..^1] + op;
            }
            _pendingOp = op; _resetOnNextDigit = true; _lastKeyWasOperator = true;
            UpdateDisplay();
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (_lastKeyWasOperator) return;
            string fullExpr = _expression + _currentInput;
            ApplyPendingOperation();
            History.Text = fullExpr + " =";
            string resultText = _accumulator.ToString(CI);
            HistoryItems.Insert(0, new HistoryEntry { Expression = fullExpr + " =", Result = resultText });
            _currentInput = resultText; _expression = ""; _pendingOp = null;
            _resetOnNextDigit = true; _lastKeyWasOperator = false;
            UpdateDisplay();
        }

        private void ApplyPendingOperation()
        {
            if (!double.TryParse(_currentInput, NumberStyles.Float, CI, out var value)) value = 0;
            if (_pendingOp == null) _accumulator = value;
            else if (_pendingOp == "+") _accumulator += value;
            else if (_pendingOp == "−") _accumulator -= value;
            else if (_pendingOp == "×") _accumulator *= value;
            else if (_pendingOp == "÷") _accumulator = value == 0 ? double.NaN : _accumulator / value;
            _currentInput = _accumulator.ToString(CI);
        }

        // ===== External history window =====
        private void HistoryButton_Click(object sender, RoutedEventArgs e) => ToggleHistoryWindow();

        private void ToggleHistoryWindow()
        {
            if (_historyWindow == null)
            {
                _historyWindow = new HistoryWindow { Owner = this };
                _historyWindow.Bind(HistoryItems);
                PositionHistoryWindow();
                _historyWindow.Show();
                _historyWindow.Closed += (_, __) => _historyWindow = null;
            }
            else { _historyWindow.Close(); _historyWindow = null; }
        }

        private void PositionHistoryWindow()
        {
            if (_historyWindow == null) return;
            const int gap = 8;
            if (WindowState == WindowState.Minimized) { _historyWindow.Hide(); return; }
            if (!_historyWindow.IsVisible) _historyWindow.Show();
            _historyWindow.Left = Left + Width + gap;
            _historyWindow.Top = Top;
            _historyWindow.Height = ActualHeight > 0 ? ActualHeight : _historyWindow.Height;
        }

        // ===== Keyboard =====
        private void HandleDigit(char d) => Digit_Click(new Button { Content = d.ToString() }, new RoutedEventArgs());
        private void HandleOp(string op) => Op_Click(new Button { Content = op }, new RoutedEventArgs());

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.H)
            { ToggleHistoryWindow(); e.Handled = true; return; }

            if (e.Key >= Key.D0 && e.Key <= Key.D9 && Keyboard.Modifiers == ModifierKeys.None)
            { HandleDigit((char)('0' + (e.Key - Key.D0))); e.Handled = true; return; }

            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            { HandleDigit((char)('0' + (e.Key - Key.NumPad0))); e.Handled = true; return; }

            if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
            { Dot_Click(this, new RoutedEventArgs()); e.Handled = true; return; }

            if (e.Key == Key.Add || (e.Key == Key.OemPlus && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift))
            { HandleOp("+"); e.Handled = true; return; }
            if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            { HandleOp("−"); e.Handled = true; return; }
            if (e.Key == Key.Multiply)
            { HandleOp("×"); e.Handled = true; return; }
            if (e.Key == Key.Divide || e.Key == Key.Oem2)
            { HandleOp("÷"); e.Handled = true; return; }

            if (e.Key == Key.Enter || e.Key == Key.Return || (e.Key == Key.OemPlus && Keyboard.Modifiers == ModifierKeys.None))
            { Equals_Click(this, new RoutedEventArgs()); e.Handled = true; return; }

            if (e.Key == Key.Back)
            {
                if (_resetOnNextDigit) { _currentInput = "0"; _resetOnNextDigit = false; }
                else { _currentInput = _currentInput.Length > 1 ? _currentInput[..^1] : "0"; }
                _lastKeyWasOperator = false; UpdateDisplay();
                e.Handled = true; return;
            }

            // Close with Ctrl+W; Esc clears first, closes if already clear. Alt+F4 is OS-level.
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.W)
            { Close(); e.Handled = true; return; }
            if (e.Key == Key.Escape)
            {
                bool nothingToClear = _expression == "" && (_currentInput == "0" || _currentInput == "-0");
                if (nothingToClear) Close(); else Clear_Click(this, new RoutedEventArgs());
                e.Handled = true; return;
            }
        }
    }

    public class HistoryEntry
    {
        public string Expression { get; set; } = "";
        public string Result { get; set; } = "";
    }
}
