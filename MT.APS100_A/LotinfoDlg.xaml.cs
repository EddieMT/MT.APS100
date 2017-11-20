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
    /// Interaction logic for LotinfoDlg.xaml
    /// </summary>
    public partial class LotinfoDlg : Window
    {
        public event OKCallback OKCallback;
        private LotInfo lotInfo;

        public LotinfoDlg(LotInfo LotInfo, BasicInfo BasicInfo)
        {
            InitializeComponent();

            lotInfo = LotInfo;

            txtTesterID.Text = string.IsNullOrEmpty(BasicInfo.TesterID) ? "" : BasicInfo.TesterID;

            txtProgramName.IsReadOnly = true;
            txtProgramName.Text = LotInfo.ProgramName.ToString();

            txtOperatorID.Text = string.IsNullOrEmpty(BasicInfo.OperatorID) ? "" : BasicInfo.OperatorID;

            txtCustomerID.Text = string.IsNullOrEmpty(BasicInfo.CustomerID) ? "" : BasicInfo.CustomerID;

            txtDeviceName.Text = string.IsNullOrEmpty(BasicInfo.DeviceName) ? "" : BasicInfo.DeviceName;

            txtCustomerLotNo.Text = string.IsNullOrEmpty(BasicInfo.CustomerLotNo) ? "" : BasicInfo.CustomerLotNo;

            txtSubLotNo.Text = string.IsNullOrEmpty(BasicInfo.SubLotNo) ? "" : BasicInfo.SubLotNo;

            //txtTestCode.Text
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = this.DialogResult.HasValue ? this.DialogResult : false;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if(!validateControls())
            {
                return;
            }

            if (btnOK.Content.ToString() == "OK")
            {
                lblConfirmMessage.Visibility = Visibility.Visible;
                btnOK.Content = "Continue";
                btnCancel.Content = "Back";

                foreach(var control in grdLotInfo.Children)
                {
                    if (control is TextBox)
                    {
                        TextBox tb = control as TextBox;
                        tb.IsReadOnly = true;
                    }
                }
            }
            else
            {
                lotInfo.TesterID.Value = txtTesterID.Text;
                
                lotInfo.ProgramName.Value = txtProgramName.Text;

                lotInfo.OperatorID.Value = txtOperatorID.Text;
                
                lotInfo.CustomerID.Value = txtCustomerID.Text;

                lotInfo.DeviceName.Value = txtDeviceName.Text.Replace('/', '_');
                
                lotInfo.CustomerLotNo.Value = txtCustomerLotNo.Text;

                lotInfo.SubLotNo.Value = txtSubLotNo.Text;

                lotInfo.ModeCode.Value = string.Empty;

                lotInfo.TestCode.Value = txtTestCode.Text;

                lotInfo.TestBinNo.Value = string.Empty;

                lotInfo.WaferVersion.Value = string.Empty;

                lotInfo.MotherFab.Value = string.Empty;

                //BasicInfo

                if (OKCallback())
                {
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (btnCancel.Content.ToString() == "Back")
            {
                lblConfirmMessage.Visibility = Visibility.Collapsed;
                btnOK.Content = "OK";
                btnCancel.Content = "Cancel";

                foreach (var control in grdLotInfo.Children)
                {
                    if (control is TextBox)
                    {
                        TextBox tb = control as TextBox;
                        tb.IsReadOnly = false;
                    }
                }
            }
            else
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void ApplyPattern(Recipe recipe)
        {
            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Tester_ID"))
                lotInfo.TesterID.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Tester_ID").Pattern;
            else
                lotInfo.TesterID.Pattern = @"^[A-Z]|[0-9]+\S*$";

            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Operator_ID"))
                lotInfo.OperatorID.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Operator_ID").Pattern;
            else
                lotInfo.OperatorID.Pattern = @"^[A-Z]|[0-9]+\S*$";

            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Device_Name"))
                lotInfo.DeviceName.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Device_Name").Pattern;
            else
                lotInfo.DeviceName.Pattern = @"^[A-Z]|[0-9]+\S*$";

            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Customer_LotNo"))
                lotInfo.CustomerLotNo.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Customer_LotNo").Pattern;
            else
                lotInfo.CustomerLotNo.Pattern = @"^[A-Z]|[0-9]+\S*$";

            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Sub_LotNo"))
                lotInfo.SubLotNo.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Sub_LotNo").Pattern;
            else
                lotInfo.SubLotNo.Pattern = @"^[A-Z]+[0-9]+\S*$";

            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Mode_Code"))
                lotInfo.ModeCode.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Mode_Code").Pattern;
            else
                lotInfo.ModeCode.Pattern = @"^(FT1)|(QA1)$";

            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Test_Code"))
                lotInfo.TestCode.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Test_Code").Pattern;
            else
                lotInfo.TestCode.Pattern = @"^R|Q\d+(\.\d+)?$";

            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Test_BinNo"))
                lotInfo.TestBinNo.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Test_BinNo").Pattern;
            else
                lotInfo.TestBinNo.Pattern = @"^ALL$";

            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Wafer_Version"))
                lotInfo.WaferVersion.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Wafer_Version").Pattern;
            else
                lotInfo.WaferVersion.Pattern = @"^(AC)|(AA)|(BB)|(B2)|(AB)$";

            if (recipe.UIInputItemConfiguration.listUIInputItems.Any(x => x.Name == "Mother_Fab"))
                lotInfo.MotherFab.Pattern = recipe.UIInputItemConfiguration.listUIInputItems.First(x => x.Name == "Mother_Fab").Pattern;
            else
                lotInfo.MotherFab.Pattern = @"^(TSMC)|(SMIC)|(HJ)|(UMC)$";
        }

        private bool validateControls()
        {
            foreach (var control in grdLotInfo.Children)
            {
                if (control is TextBox)
                {
                    TextBox tb = control as TextBox;
                    if (string.IsNullOrEmpty(tb.Text))
                    {
                        MessageBox.Show("批次信息均为必填项，请核对后重新输入 ！");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
