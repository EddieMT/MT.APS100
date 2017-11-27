using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MT.APS100.Model;

namespace MT.APS100_A
{
    /// <summary>
    /// Interaction logic for UserCalibration.xaml
    /// </summary>
    public partial class UserCalibration : Window
    {
        public event OKCallback OKCallback;
        private LotInfo lotInfo;

        public UserCalibration(LotInfo LotInfo)
        {
            InitializeComponent();

            lotInfo = LotInfo;
        }

        private void btnCalibration_Click(object sender, RoutedEventArgs e)
        {
            lotInfo.ProgramName.Value = txtProgramName.Text;

            if (OKCallback())
            {
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
