using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureStacker.WinUI
{
    public partial class OverWrapForm : Form
    {
        public OverWrapForm()
        {
            InitializeComponent();
        }

        // マルティディスプレイ用
        public OverWrapForm(int width, int height)
        {
            InitializeComponent();
            this.Size = new Size(width, height);
        }
    }
}
