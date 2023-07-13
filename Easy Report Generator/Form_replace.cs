using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace Easy_Report_Generator
{
    public partial class Form_replace : KryptonForm
    {
        public string TextOfTextbox { get; set; }
        public Form_replace(string initString)
        {
            InitializeComponent();
            kryptonTextBox1.Text = initString;
        }

        private void Form_replace_Load(object sender, EventArgs e)
        {

        }

        private void kryptonTextBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void kryptonButton1_Click(object sender, EventArgs e)
        {
            TextOfTextbox = kryptonTextBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
