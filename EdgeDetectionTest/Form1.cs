using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdgeDetectionTest
{
	public partial class Form1 : Form
	{
		public Form1 ()
		{
			InitializeComponent ();
		}

		private void button1_Click ( object sender, EventArgs e )
		{
			OpenFileDialog ofd = new OpenFileDialog ();
			if ( ofd.ShowDialog () == DialogResult.Cancel ) return;

			pictureBox1.Image = new Bitmap ( ofd.FileName );
		}

		private void button2_Click ( object sender, EventArgs e )
		{
			ProcessImage image = new ProcessImage ( pictureBox1.Image as Bitmap );
			ProcessImage result = new ProcessImage ( pictureBox1.Image as Bitmap, false );

			SuperColor [,] filter = new SuperColor [ 3, 3 ]
			{
				{ new SuperColor ( -1.0f ), new SuperColor ( -1.0f ), new SuperColor ( -1.0f ) },
				{ new SuperColor ( -1.0f ), new SuperColor ( 11.0f ), new SuperColor ( -1.0f ) },
				{ new SuperColor ( -1.0f ), new SuperColor ( -1.0f ), new SuperColor ( -1.0f ) }
			};
			Parallel.For ( 0, image.Height, ( y ) =>
			{
				for ( int x = 0; x < image.Width; ++x )
				{
					result [ x, y ] = image.FilterProcess ( x, y, filter ) * ( 1 / 3.0f );
				}
			} );

			pictureBox1.Image = result.ToBitmap ();
		}
	}
}
