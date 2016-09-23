using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
			Stopwatch sw = new Stopwatch ();

			sw.Start ();
			ProcessImage image = new ProcessImage ( pictureBox1.Image as Bitmap );
			ProcessImage result = new ProcessImage ( image.Width, image.Height );
			sw.Stop ();
			Text = sw.Elapsed.ToString () + ", ";

			Filter filter = new Filter ( new float [ , ]
			{
				{ 1, 1, 1 },
				{ 1, -7, 1 },
				{ 1, 1, 1 },
			} );
			sw.Restart ();
			Parallel.For ( 0, image.Height, ( y ) =>
			{
				for ( int x = 0; x < image.Width; ++x )
					result [ x, y ] = image.FilterProcess ( x, y, filter );
			} );
			sw.Stop ();
			Text += sw.Elapsed.ToString () + ", ";

			sw.Restart ();
			//pictureBox1.Image = result.ToBitmap ();
			result.ToBitmap ( pictureBox1.Image as Bitmap );
			sw.Stop ();
			Text += sw.Elapsed.ToString ();

			Refresh ();
		}
	}
}
