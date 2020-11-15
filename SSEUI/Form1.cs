using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSEUI
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			folderBrowserDialog1.SelectedPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Skyrim Special Edition\\Data";
			label1.Text = folderBrowserDialog1.SelectedPath;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			folderBrowserDialog1.ShowDialog();
			label1.Text = folderBrowserDialog1.SelectedPath;
		}
	}
}
