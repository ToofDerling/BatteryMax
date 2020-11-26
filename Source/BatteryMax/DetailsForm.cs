using System;
using System.Windows.Forms;

namespace BatteryMax
{
    public partial class DetailsForm : Form
    {
        public BatteryIconManager BatteryIconManager { get; set; }

        public DetailsForm()
        {
            InitializeComponent();
        }

        private void DetailsForm_Load(object sender, EventArgs e)
        {
        }
    }
}
