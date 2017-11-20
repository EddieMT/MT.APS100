using MT.APS100.Model;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MT.APS100.Service;

namespace MT.APS100_B
{
    /// <summary>
    /// Interaction logic for MTConfig.xaml
    /// </summary>
    public partial class MTConfig : Window
    {
        private int sitecount = 0;

        public MTConfig()
        {
            InitializeComponent();
        }

        private MT.APS100.Model.Configuration configuration = new MT.APS100.Model.Configuration();

        //List<Sites> sites = new List<Sites>();
        private void TxtBox_Sites_LostFocus(object sender, RoutedEventArgs e)
        {
            RB_SiteCount.Children.Clear();
            int siteheight = Int32.Parse(TxtBox_Sites.Text);
            RB_SiteRow.Height = new GridLength(siteheight * 5, GridUnitType.Auto);
            MainGridRowTop.Height = new GridLength((siteheight * 5) + 25, GridUnitType.Auto);
            sitecount = Int32.Parse(TxtBox_Sites.Text);
            for (int x = 1; x <= sitecount; x++)
            {
                CheckBox checkbox = new CheckBox();
                checkbox.Content = "Site " + x;
                checkbox.Name = "Site" + x;
                checkbox.Foreground = new SolidColorBrush(Colors.White);
                checkbox.VerticalAlignment = VerticalAlignment.Center;
                checkbox.Margin = new Thickness(5);
                checkbox.IsChecked = true;
                checkbox.Unchecked += Checkbox_Unchecked;
                checkbox.Checked += Checkbox_Checked;
                RB_SiteCount.Children.Add(checkbox);
                Sites site = new Sites();
                site.SiteName = checkbox.Name;
                site.SiteValue = (bool)checkbox.IsChecked;
                configuration.SiteName.Add(site);
            }
        }

        private void Checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (Sites site in configuration.SiteName)
            {
                CheckBox checkbox = (CheckBox)sender;

                if (checkbox.Name == site.SiteName)
                {
                    site.SiteValue = false;
                }
            }
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (Sites site in configuration.SiteName)
            {
                CheckBox checkbox = (CheckBox)sender;

                if (checkbox.Name == site.SiteName)
                {
                    site.SiteValue = true;
                }
            }
        }

        private void ContinueOnFail_Checked(object sender, RoutedEventArgs e)
        {
            configuration.ContinueOnFail = true;
        }
        private void ContinueOnFail_Unchecked(object sender, RoutedEventArgs e)
        {
            configuration.ContinueOnFail = false;
        }
        private void StopOnFail_Checked(object sender, RoutedEventArgs e)
        {
            configuration.StopOnFail = true;
        }

        private void StopOnFail_Unchecked(object sender, RoutedEventArgs e)
        {
            configuration.StopOnFail = false;
        }

        private void Alarm_True_Checked(object sender, RoutedEventArgs e)
        {
            configuration.StopOnAlarm = true;
        }

        private void Alarm_True_Unchecked(object sender, RoutedEventArgs e)
        {
            configuration.StopOnAlarm = false;
        }

        private void Alarm_False_Checked(object sender, RoutedEventArgs e)
        {
            configuration.ContinueOnAlarm = true;
        }

        private void Alarm_False_Unchecked(object sender, RoutedEventArgs e)
        {
            configuration.ContinueOnAlarm = false;
        }
        private void Chk_GoldUnit_Checked(object sender, RoutedEventArgs e)
        {
            configuration.GoldUnitEnabled = true;

        }
        private void Chk_GoldUnit_Unchecked(object sender, RoutedEventArgs e)
        {
            configuration.GoldUnitEnabled = false;
            
        }

        private void Chk_Calibration_Checked(object sender, RoutedEventArgs e)
        {
            configuration.UserCalibration = true;
            Txt_CalibrationExpiration.IsEnabled = true;
        }

        private void Chk_Calibration_Unchecked(object sender, RoutedEventArgs e)
        {
            configuration.UserCalibration = false;
            Txt_CalibrationExpiration.IsEnabled = false;
        }

        private void Chk_QAOffline_Checked(object sender, RoutedEventArgs e)
        {
            QAInlineNth.IsEnabled = false;
        }

        private void Chk_QAOffline_Unchecked(object sender, RoutedEventArgs e)
        {
            QAInlineNth.IsEnabled = true;
        }

        private void ContinueOnAllFail_Checked(object sender, RoutedEventArgs e)
        {
            configuration.StopOnAllFail = true;
        }

        private void ContinueOnAllFail_Unchecked(object sender, RoutedEventArgs e)
        {
            configuration.StopOnAllFail = false;
        }

        private void BtnConfigSave_Click(object sender, RoutedEventArgs e)
        {
            configuration.ProjectName = LblSelectedProject.Content.ToString();
            configuration.CalibrationExpiration = int.Parse(Txt_CalibrationExpiration.Text);
          configuration.QAInlineEnabled = int.Parse(QAInlineNth.Text);
            configuration.LogNthDevice = int.Parse(Txt_LogNthDevice.Text);
            configuration.NumberOfSites = int.Parse(TxtBox_Sites.Text);

            if ((bool)ContinueOnFail.IsChecked)
            {
                configuration.ContinueOnFail = true;
            }
            else
            {
                configuration.ContinueOnFail = false;
            }

            if ((bool)Alarm_True.IsChecked)
            {
                configuration.StopOnAlarm = true;
            }
            else
            {
                configuration.StopOnAlarm = false;
            }

            if ((bool)Chk_GoldUnit.IsChecked)
            {
                configuration.GoldUnitEnabled = true;
            }
            else
            {
                configuration.GoldUnitEnabled = false;
            }

            if ((bool)Chk_QAOffline.IsChecked)
            {
                configuration.QAOfflineEnabled = true;
            }
            else
            {
                configuration.QAOfflineEnabled = false;
            }

            
            CreateProject.CreateConfigFile(configuration);
            // string config = @"c:\MerlinTest\Project\" + LblSelectedProject.Content + @"\Config.csv";
            //using (System.IO.str = File.AppendText(config))
            //{
            //    writer.Write(",Test");
            //}

            ////////using (System.IO.StreamWriter writer = File.AppendText(config))
            ////////{
            ////////    for (int x = 1; x <= sitecount; x++)
            ////////    {
            ////////        writer.WriteLine();
            ////////        writer.Write("Site " + x);
            ////////    }
            ////////}
        }
    }
}