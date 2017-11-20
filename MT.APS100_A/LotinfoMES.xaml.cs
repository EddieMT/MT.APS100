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
    /// Interaction logic for LotinfoMES.xaml
    /// </summary>
    public partial class LotinfoMES : Window
    {
        public event OKCallback OKCallback;
        private LotInfo lotInfo;

        public LotinfoMES(LotInfo LotInfo)
        {
            InitializeComponent();

            lotInfo = LotInfo;
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            lotInfo.ProgramName.Value = txtProgramName.Text;

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
