namespace Financial_Calculator
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Windows;
    using Financial_Calculator.Common;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.RateTb.Text = "";
        }

        private void ConfirmBTN_Click(object sender, RoutedEventArgs e)
        {
            double rate = 0;

            if (this.BuyDateTimeTp.SelectedDate == null
                || this.StartDateTimeTp.SelectedDate == null
                || this.EndTimeTp.SelectedDate == null
                || this.RateTb.Text.Trim().Length == 0
                || !double.TryParse(this.RateTb.Text, out rate))
            {
                MessageBox.Show("参数错误!");
            }

            Debug.Assert(this.StartDateTimeTp.SelectedDate != null, "StartDateTimeTp.SelectedDate != null");
            Debug.Assert(this.BuyDateTimeTp.SelectedDate != null, "BuyDateTimeTp.SelectedDate != null");
            Debug.Assert(this.EndTimeTp.SelectedDate != null, "EndTimeTp.SelectedDate != null");

            var buyDate = this.BuyDateTimeTp.SelectedDate.Value;
            var startDate = this.StartDateTimeTp.SelectedDate.Value;
            var endDate = this.EndTimeTp.SelectedDate.Value;

            var possibleHolidays = HolidayCalculator.PossibleHolidays(endDate, out var workDate);

            var holidayInfo = new StringBuilder();

            holidayInfo.Append("{");
            foreach (var possibleHoliday in possibleHolidays)
            {
                if (holidayInfo.Length == 1)
                {
                    holidayInfo.Append(possibleHoliday.Name);
                }
                else
                {
                    holidayInfo.Append("," + possibleHoliday.Name);
                }
            }

            holidayInfo.Append("}");

            var result = (rate * (endDate - startDate).Days) / (workDate - buyDate).Days;

            var message = "===================\n"
                          + "宣传:" + (endDate - startDate).TotalDays + "天, 实际:" + (workDate - buyDate).TotalDays + "天\n"
                          + ToDateString(startDate) + "~" + ToDateString(endDate) + ":" + ToPercent(rate)
                          + " => " + ToDateString(buyDate) + "~" + ToDateString(workDate) + ":" + ToPercent(result)
                          + (possibleHolidays.Count > 0 ? "\n可能节假日:" + holidayInfo : "");

            MessageBox.Show(message);

            this.HistoryLB.Content = message + "\n" + this.HistoryLB.Content;
        }

        private static string ToDateString(DateTime dateTime)
        {
            return dateTime.Year + "." + dateTime.Month + "." + dateTime.Day;
        }

        private void ResetBTN_Click(object sender, RoutedEventArgs e)
        {
            this.RateTb.Text = "";
            this.HistoryLB.Content = "";
        }

        private static string ToPercent(double value)
        {
            return value.ToString("F") + "%";
        }
    }
}
