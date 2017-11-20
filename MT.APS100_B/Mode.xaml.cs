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
using System.IO;
using MT.APS100.Model;

namespace MT.APS100_B
{
    /// <summary>
    /// Interaction logic for Mode.xaml
    /// </summary>
    public partial class Mode : Window
    {
        public WorkMode workMode;
        public Dictionary<string, List<string>> listPassword;

        public Mode(Dictionary<string, List<string>> ListPassword)
        {
            InitializeComponent();

            listPassword = ListPassword;
        }

        private void rdbOperator_Checked(object sender, RoutedEventArgs e)
        {
            grdPassword.Visibility = Visibility.Visible;
        }

        private void rdbMES_Checked(object sender, RoutedEventArgs e)
        {
            grdPassword.Visibility = Visibility.Collapsed;
        }

        //private void chcPassword_Checked(object sender, RoutedEventArgs e)
        //{
        //    txtPassword.PasswordChar = new char();
        //}

        //private void chcPassword_UnChecked(object sender, RoutedEventArgs e)
        //{
        //    txtPassword.PasswordChar = '*';
        //}

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (rdbOperator.IsChecked.Value)
            {
                var list = listPassword.FirstOrDefault(x => x.Key == "operator_mode_password");
                if (list.Value == null || !list.Value.Contains(txtPassword.Password))
                {
                    MessageBox.Show("alarm O01:password fail !");
                    return;
                }

                workMode = WorkMode.OPERATOR;
            }
            else
                workMode = WorkMode.MES;

            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rdbOperator.IsChecked = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = this.DialogResult.HasValue ? this.DialogResult : false;
        }
    }
}
