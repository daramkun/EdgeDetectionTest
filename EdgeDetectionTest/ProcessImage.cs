using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDetectionTest
{
	public enum OutOfFrameColor
	{
		NoColor,
		Border,
		Mirror,
	}

	public class ProcessImage
	{
		SuperColor [,] colorMap;

		public int Width { get; private set; }
		public int Height { get; private set; }

		public OutOfFrameColor OutOfFrameColor = OutOfFrameColor.NoColor;

		public SuperColor this [ int x, int y ]
		{
			get
			{
				if ( ( x >= 0 && x < Width ) && ( y >= 0 && y < Height ) )
					return colorMap [ x, y ];
				else
				{
					switch ( OutOfFrameColor )
					{
						case OutOfFrameColor.Mirror:
							return colorMap [ ( ( x + ( x >> 31 ) ) ^ ( x >> 31 ) ) % Width, ( ( y + ( y >> 31 ) ) ^ ( y >> 31 ) ) % Height ];
						case OutOfFrameColor.Border:
							if ( x < 0 ) x = 0; else if ( x >= Width ) x = Width - 1;
							if ( y < 0 ) y = 0; else if ( y >= Height ) y = Height - 1;
							return colorMap [ x, y ];
						case OutOfFrameColor.NoColor: return new SuperColor ();
						default: throw new IndexOutOfRangeException ();
					}
				}
			}
			set { colorMap [ x, y ] = value; }
		}
		public SuperColor this [ Point p ]
		{
			get { return this [ p.X, p.Y ]; }
			set { this [ p.X, p.Y ] = value; }
		}

		public ProcessImage ( int width, int height )
		{
			Width = width;
			Height = height;
			colorMap = new SuperColor [ Width, Height ];
		}

		public ProcessImage ( Bitmap bitmap )
			: this ( bitmap.Width, bitmap.Height )
		{
			var bitmapData = bitmap.LockBits ( new Rectangle ( 0, 0, Width, Height ),
				System.Drawing.Imaging.ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			Parallel.For ( 0, Height, ( y ) =>
			{
				int wy = Width * y;
				for ( int x = 0; x < Width; ++x )
					colorMap [ x, y ] = new SuperColor ( Marshal.ReadInt32 ( bitmapData.Scan0 + ( ( wy + x ) * 4 ) ) );
			} );
			bitmap.UnlockBits ( bitmapData );
		}

		public void ToBitmap ( Bitmap bitmap )
		{
			var bitmapData = bitmap.LockBits ( new Rectangle ( 0, 0, bitmap.Width, bitmap.Height ),
				System.Drawing.Imaging.ImageLockMode.WriteOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			Parallel.For ( 0, Height, ( y ) =>
			{
				int wy = Width * y;
				for ( int x = 0; x < Width; ++x )
					Marshal.WriteInt32 ( bitmapData.Scan0 + ( ( wy + x ) * 4 ), colorMap [ x, y ].ToArgb () );
			} );

			bitmap.UnlockBits ( bitmapData );
		}

		public Bitmap ToBitmap ()
		{
			Bitmap bitmap = new Bitmap ( Width, Height );
			ToBitmap ( bitmap );
			return bitmap;
		}

		public ProcessImage CopyProcessImage ()
		{
			ProcessImage pi = new ProcessImage ( Width, Height );

			return pi;
		}

		public SuperColor FilterProcess ( int x, int y, Filter filter )
		{
			int filterWidthCenter = x - filter.FilterWidth / 2;
			int filterHeightCenter = y - filter.FilterHeight / 2;

			SuperColor total = new SuperColor ();
			for ( int ty = 0; ty < filter.FilterHeight; ++ty )
				for ( int tx = 0; tx < filter.FilterWidth; ++tx )
				{
					var c = this [ filterWidthCenter + tx, filterHeightCenter + ty ];
					c.Multiply ( filter.FilterData [ tx, ty ] );
					total.Add ( ref c );
				}
			
			return total * filter.Factor + filter.Bias;
		}

		public void MakeGrayscale ()
		{
			for ( int y = 0; y < Height; ++y )
				for ( int x = 0; x < Width; ++x )
					colorMap [ x, y ] = colorMap [ x, y ].ToGrayscale ();
		}
	}
}
