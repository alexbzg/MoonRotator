using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EncRotator.Properties;

namespace EncRotator
{
    public partial class fConnectionParams : Form
    {

        public fConnectionParams(ConnectionSettings cSettings)
        {
            InitializeComponent();
            lCaption.Text = cSettings.name;
            tbHost.Text = cSettings.jeromeParams.host;
            tbPort.Text = cSettings.jeromeParams.port.ToString();
            tbPassword.Text = cSettings.jeromeParams.password;
        }



    }




}
