using System;
using System.Windows;

namespace Calcex.Windows
{
    public partial class SettingsWindow : Window
    {
        public CalcMode CalculationMode { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (CalculationMode)
            {
                case CalcMode.Decimal:
                    radDecimal.IsChecked = true;
                    break;
                case CalcMode.Boolean:
                    radBoolean.IsChecked = true;
                    break;
                default:
                    radDouble.IsChecked = true;
                    break;
            }
        }

        private void ButtonSubmit_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)radDecimal.IsChecked) CalculationMode = CalcMode.Decimal;
            else if ((bool)radBoolean.IsChecked) CalculationMode = CalcMode.Boolean;
            else CalculationMode = CalcMode.Double;
            this.DialogResult = true;
            this.Close();
        }
    }
}
