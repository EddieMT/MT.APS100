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

namespace MT.APS100_B
{
    /// <summary>
    /// Interaction logic for LotinfoOP.xaml
    /// </summary>
    public partial class LotinfoOP : Window
    {
        public event OKCallback OKCallback;
        private LotInfo lotInfo;

        public LotinfoOP(LotInfo LotInfo)
        {
            InitializeComponent();

            lotInfo = LotInfo;
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            lotInfo.CustomerID.Value = txtCustomerID.Text;
            lotInfo.ProgramName.Value = txtProgramName.Text;
            lotInfo.ModeCode.Value = txtModeCode.Text;

            if (OKCallback())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = this.DialogResult.HasValue ? this.DialogResult : false;
        }
    }
}
