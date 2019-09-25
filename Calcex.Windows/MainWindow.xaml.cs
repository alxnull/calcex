using System;
using System.Windows;
using System.Windows.Controls;
using Calcex;
using Calcex.Parsing;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading;

namespace Calcex.Windows
{
    public partial class MainWindow : Window
    {
        private CalcMode mode = CalcMode.Double;
        private Parser parser = new Parser();
        private ObservableCollection<Calculation> SavedItems { get; set; }

        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            InitializeComponent();
            mode = Properties.Settings.Default.CalcMode;
            SavedItems = Properties.Settings.Default.SavedItems 
                ?? new ObservableCollection<Calculation>();
            lstSaved.ItemsSource = SavedItems;
            foreach (var item in SavedItems)
                parser.SetVariable($"r{item.ID}", item.Result);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Version ver = Assembly.GetAssembly(typeof(Parser)).GetName().Version;
            txtVersion.Text = $"using Calcex.Core v.{ver}";
            comFromBase.ItemsSource = Enum.GetNames(typeof(NumberBase));
            comToBase.ItemsSource = Enum.GetNames(typeof(NumberBase));
            lstHelp.ItemsSource = ParserSymbols.GetParserSymbols();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.CalcMode = mode;
            Properties.Settings.Default.SavedItems = SavedItems;
            Properties.Settings.Default.Save();
        }

        #region Calculator
        private object calculate(string expr)
        {
            return mode == CalcMode.Double ? parser.Parse(expr).Evaluate()
                : mode == CalcMode.Decimal ? parser.Parse(expr).EvaluateDecimal()
                : (object)parser.Parse(expr).EvaluateBool();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtInput.Text))
            {
                lblMessage.Content = String.Empty;
                lblResult.Content = null;
                return;
            }
            try
            {
                var result = calculate(txtInput.Text);
                lblResult.Content = result;
                lblMessage.Content = String.Empty;
            }
            catch (ParserException ex)
            {
                lblResult.Content = null;
                lblMessage.Content = ex.Message;
            }
        }

        private void saveCalculation()
        {
            if (lblResult.Content != null)
            {
                var calcItem = new Calculation()
                {
                    ID = SavedItems.Count,
                    Expression = txtInput.Text,
                    Result = lblResult.Content
                };
                SavedItems.Add(calcItem);
                parser.SetVariable($"r{calcItem.ID}", calcItem.Result);
            }
        }

        private void txtInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                saveCalculation();
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Owner = this;
            settings.CalculationMode = mode;
            if (settings.ShowDialog() == true)
            {
                mode = settings.CalculationMode;
                textBox_TextChanged(null, null);
            }
        }

        private void MenuClear_Click(object sender, RoutedEventArgs e)
        {
            SavedItems.Clear();
        }

        private void MenuConsole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process p = Process.Start("calcex.exe");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show(ex.Message, "Calcex App - Error");
            }
        }

        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            if (lblResult.Content != null)
                Clipboard.SetText(lblResult.Content.ToString());
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e) => saveCalculation();

        private void ContextCopyCalculation_Click(object sender, RoutedEventArgs e)
        {
            Calculation selected = (Calculation)lstSaved.SelectedItem;
            if (selected != null)
                Clipboard.SetText(selected.ToString());
        }

        private void ContextCopyResult_Click(object sender, RoutedEventArgs e)
        {
            Calculation selected = (Calculation)lstSaved.SelectedItem;
            if (selected != null)
                Clipboard.SetText(selected.Result.ToString());
        }
        #endregion

        #region Converter
        private string convertBase(string value, int from, int to)
        {
            var decValue = Utils.ConvertToBase10(value, from);
            return Utils.ConvertFromBase10(decValue, to);
        }

        private void txtFrom_TextChanged(object sender, EventArgs e)
        {
            if (txtFrom.Text != "")
            {
                try
                {
                    txtTo.Text = convertBase(txtFrom.Text,
                        (int)Enum.Parse(typeof(NumberBase), comFromBase.SelectedItem.ToString()),
                        (int)Enum.Parse(typeof(NumberBase), comToBase.SelectedItem.ToString()));
                }
                catch { txtTo.Text = "error"; }
            }
            else txtTo.Text = String.Empty;
        }
        #endregion

        private void MenuHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(this, "Calcex GUI App\n" + txtVersion.Text 
                + "\n\nhttps://github.com/alxnull/calcex\n\n\u00a9 2018-2019, alxnull.", "Calcex App");
        }
    }
}
