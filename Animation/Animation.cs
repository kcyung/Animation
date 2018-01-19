using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GDIDrawer;


namespace Animation
{
    public partial class Animation : Form
    {
        CDrawer _canvas = new CDrawer(1000,1000);
        public Animation()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Fungus f1 = new Fungus(new Point(0, 0), _canvas, Fungus.Colors.Yellow);
           Fungus f2 = new Fungus(new Point(999, 0), _canvas, Fungus.Colors.Red); ;
            Fungus f3 = new Fungus(new Point(0, 999), _canvas, Fungus.Colors.Green);
            Fungus f4 = new Fungus(new Point(999, 999), _canvas, Fungus.Colors.Blue);
        }
    }
}
