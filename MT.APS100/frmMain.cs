using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MT.APS100.Model;
using MT.APS100.Service;

namespace MT.APS100
{
    public partial class frmMain : Form
    {
        #region
        ProgramService programService;
        #endregion

        public frmMain()
        {
            InitializeComponent();
        }

        #region Events
        private void frmMain_Load(object sender, EventArgs e)
        {
            setControlStatus(ControlStatus.status_formloaded);
        }

        private void btnNewLot_Click(object sender, EventArgs e)
        {
            if (btnNewLot.Text == "New Lot")
            {
                setControlStatus(ControlStatus.status_newlot);
            }
            else if (btnNewLot.Text == "Done")
            {
                setControlStatus(ControlStatus.status_newlot_done);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            //Todo: Validate folders

            programService = new ProgramService(txtProgramDirectory.Text, txtProgramName.Text);
        }

        private void btnProgramDirectory_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "DLLFile|*.dll";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo di = new DirectoryInfo(dialog.FileName);
                txtProgramName.Text = Path.GetFileNameWithoutExtension(di.FullName);
                txtProgramDirectory.Text = di.Parent.Parent.FullName;
                txtDatalogDirectory.Text = Path.Combine(txtProgramDirectory.Text, "Datalog");
            }
        }
        #endregion

        #region Methods
        private void setControlStatus(ControlStatus controlStatus)
        {
            btnNewLot.Text = (controlStatus == ControlStatus.status_newlot) ? "Done" : "New Lot";

            txtProgramName.ReadOnly = controlStatus != ControlStatus.status_newlot;
            txtDeviceName.ReadOnly = controlStatus != ControlStatus.status_newlot;
            txtLotNo.ReadOnly = controlStatus != ControlStatus.status_newlot;
            txtTestCode.ReadOnly = controlStatus != ControlStatus.status_newlot;
            txtProgramDirectory.ReadOnly = controlStatus != ControlStatus.status_newlot;
            btnProgramDirectory.Enabled = controlStatus == ControlStatus.status_newlot;
            txtDatalogDirectory.ReadOnly = controlStatus != ControlStatus.status_newlot;
            btnDatalogDirectory.Enabled = controlStatus == ControlStatus.status_newlot;
        }

        #endregion
    }

    public enum ControlStatus
    {
        status_formloaded,
        status_newlot,
        status_newlot_done,
    }
}
