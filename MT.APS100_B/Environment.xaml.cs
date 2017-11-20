using System.Windows;
using MT.APS100.Model;
using System.IO;
using MT.APS100.Service;
using System.Windows.Forms;

namespace MT.APS100_B
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Environment : Window
    {
        public Environment()
        {
            InitializeComponent();

            DataContext = this;
        }

        public TransferMode selectedTransferMode
        {
            get { return (TransferMode)GetValue(SelectedWeekProperty); }
            set { SetValue(SelectedWeekProperty, value); }
        }

        public static readonly DependencyProperty SelectedWeekProperty =
            DependencyProperty.Register("selectedTransferMode", typeof(TransferMode), typeof(Environment), new PropertyMetadata(default(TransferMode)));

        private void cboTransferMode_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (selectedTransferMode == TransferMode.FTP)
            {
                txtServerDataIP.Text = @"192.1.1.172";
                txtServerDataUser.Text = @"icupload";
                txtServerDataPassword.Text = @"icuploadxccdkj";
                txtServerProgIP.Text = @"192.1.1.172";
                txtServerProgUser.Text = @"prog";
                txtServerProgPassword.Text = @"prog";
                txtServerDataIP.SelectAll();
            }
            else if (selectedTransferMode == TransferMode.Server)
            {
                txtServerDataIP.Text = @"C:\MerlinTest\Server";
                txtServerDataUser.Text = string.Empty;
                txtServerDataPassword.Text = string.Empty;
                txtServerProgIP.Text = @"C:\MerlinTest\Server";
                txtServerProgUser.Text = string.Empty;
                txtServerProgPassword.Text = string.Empty;
                txtServerDataIP.SelectAll();
            }
            else
            {
                txtServerDataIP.Text = string.Empty;
                txtServerDataUser.Text = string.Empty;
                txtServerDataPassword.Text = string.Empty;
                txtServerProgIP.Text = string.Empty;
                txtServerProgUser.Text = string.Empty;
                txtServerProgPassword.Text = string.Empty;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = new StreamReader(FileStructure.ENVIRONMENT_CONFIG_FILE_PATH))
            {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;

                    if (line.StartsWith(@"TransferMode ="))
                    {
                        if (line.Split('=')[1].Trim() == TransferMode.FTP.ToString())
                            selectedTransferMode = TransferMode.FTP;
                        else if (line.Split('=')[1].Trim() == TransferMode.Server.ToString())
                            selectedTransferMode = TransferMode.Server;
                        else if (line.Split('=')[1].Trim() == TransferMode.Default.ToString())
                            selectedTransferMode = TransferMode.Default;
                    }

                    if (line.StartsWith(@"ProductionWorkspace ="))
                    {
                        txtProductionWorkspace.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"server_data_IP ="))
                    {
                        txtServerDataIP.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"server_data_user ="))
                    {
                        txtServerDataUser.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"server_data_password ="))
                    {
                        txtServerDataPassword.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"server_pgm_IP ="))
                    {
                        txtServerProgIP.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"server_pgm_user ="))
                    {
                        txtServerProgUser.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"server_pgm_password ="))
                    {
                        txtServerProgPassword.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"tester_type ="))
                    {
                        txtTesterType.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"Local_dlog_dir ="))
                    {
                        txtLocalDlogDir.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"local_prog_dir ="))
                    {
                        txtLocalProgDir.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"server_dlog_dir ="))
                    {
                        txtServerDlogDir.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"server_prog_dir ="))
                    {
                        txtServerProgDir.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"server_mesfile_dir ="))
                    {
                        txtServerMESFileDir.Text = line.Split('=')[1].Trim();
                    }

                    if (line.StartsWith(@"ui_clear_pgm_flag ="))
                    {
                        chkUIClearProg.IsChecked = (line.Split('=')[1].Trim() == "1") ? true : false;
                    }

                    if (line.StartsWith(@"start_clear_pgm_flag ="))
                    {
                        chkStartClearProg.IsChecked = (line.Split('=')[1].Trim() == "1") ? true : false;
                    }

                    if (line.StartsWith(@"exit_clear_pgm_flag ="))
                    {
                        chkExitClearProg.IsChecked = (line.Split('=')[1].Trim() == "1") ? true : false;
                    }

                    if (line.StartsWith(@"alarm_M01_flag ="))
                    {
                        chkAlarmM01.IsChecked = (line.Split('=')[1].Trim() == "1") ? true : false;
                    }

                    if (line.StartsWith(@"alarm_M02_flag ="))
                    {
                        chkAlarmM02.IsChecked = (line.Split('=')[1].Trim() == "1") ? true : false;
                    }

                    if (line.StartsWith(@"alarm_M03_flag ="))
                    {
                        chkAlarmM03.IsChecked = (line.Split('=')[1].Trim() == "1") ? true : false;
                    }

                    if (line.StartsWith(@"alarm_M04_flag ="))
                    {
                        chkAlarmM04.IsChecked = (line.Split('=')[1].Trim() == "1") ? true : false;
                    }
                }
                sr.Close();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string text = string.Empty;
            using (FileStream fs = new FileStream(FileStructure.ENVIRONMENT_CONFIG_FILE_PATH, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    text = @"TransferMode = {TransferMode}
ProductionWorkspace = {ProductionWorkspace}

[server_data_IP]
server_data_IP = {server_data_IP}
server_data_user = {server_data_user}
server_data_password = {server_data_password}

[server_program_IP]
server_pgm_IP = {server_pgm_IP}
server_pgm_user = {server_pgm_user}
server_pgm_password = {server_pgm_password}

[main path]
tester_type = {tester_type}
Local_dlog_dir = {Local_dlog_dir}
local_prog_dir = {local_prog_dir}
server_dlog_dir = {server_dlog_dir}
server_prog_dir = {server_prog_dir}
server_mesfile_dir = {server_mesfile_dir}

[ui_control_flag]
ui_clear_pgm_flag = {ui_clear_pgm_flag}
start_clear_pgm_flag = {start_clear_pgm_flag}
exit_clear_pgm_flag = {exit_clear_pgm_flag}

[alarm_control_flag]
alarm_M01_flag = {alarm_M01_flag}
alarm_M02_flag = {alarm_M02_flag}
alarm_M03_flag = {alarm_M03_flag}
alarm_M04_flag = {alarm_M04_flag}";

                    text = text.Replace("{TransferMode}", selectedTransferMode.ToString());
                    text = text.Replace("{ProductionWorkspace}", txtProductionWorkspace.Text);
                    text = text.Replace("{server_data_IP}", txtServerDataIP.Text);
                    text = text.Replace("{server_data_user}", txtServerDataUser.Text);
                    text = text.Replace("{server_data_password}", txtServerDataPassword.Text);
                    text = text.Replace("{server_pgm_IP}", txtServerProgIP.Text);
                    text = text.Replace("{server_pgm_user}", txtServerProgUser.Text);
                    text = text.Replace("{server_pgm_password}", txtServerProgPassword.Text);
                    text = text.Replace("{tester_type}", txtTesterType.Text);
                    text = text.Replace("{Local_dlog_dir}", txtLocalDlogDir.Text);
                    text = text.Replace("{local_prog_dir}", txtLocalProgDir.Text);
                    text = text.Replace("{server_dlog_dir}", txtServerDlogDir.Text);
                    text = text.Replace("{server_prog_dir}", txtServerProgDir.Text);
                    text = text.Replace("{server_mesfile_dir}", txtServerMESFileDir.Text);
                    text = text.Replace("{ui_clear_pgm_flag}", (chkUIClearProg.IsChecked.Value) ? "1" : "0");
                    text = text.Replace("{start_clear_pgm_flag}", (chkStartClearProg.IsChecked.Value) ? "1" : "0");
                    text = text.Replace("{exit_clear_pgm_flag}", (chkExitClearProg.IsChecked.Value) ? "1" : "0");
                    text = text.Replace("{alarm_M01_flag}", (chkAlarmM01.IsChecked.Value) ? "1" : "0");
                    text = text.Replace("{alarm_M02_flag}", (chkAlarmM02.IsChecked.Value) ? "1" : "0");
                    text = text.Replace("{alarm_M03_flag}", (chkAlarmM03.IsChecked.Value) ? "1" : "0");
                    text = text.Replace("{alarm_M04_flag}", (chkAlarmM04.IsChecked.Value) ? "1" : "0");

                    sw.Write(text);
                    sw.WriteLine();
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnProductionWorkspace_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtProductionWorkspace.Text = dlg.SelectedPath;
            }
        }
    }
}
