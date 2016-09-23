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

		public int Width { get { return colorMap.GetLength ( 0 ); } }
		public int Height { get { return colorMap.GetLength ( 1 ); } }

		public OutOfFrameColor OutOfFrameColor { get; set; } = OutOfFrameColor.Border;

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
						case OutOfFrameColor.NoColor: return new SuperColor ();
						case OutOfFrameColor.Border:
							if ( x < 0 ) x = 0;
							if ( x >= Width ) x = Width - 1;
							if ( y < 0 ) y = 0;
							if ( y >= Height ) y = Height - 1;
							return colorMap [ x, y ];
						case OutOfFrameColor.Mirror:
							x = Math.Abs ( x );
							y = Math.Abs ( y );
							x %= Width;
							y %= Height;
							return colorMap [ x, y ];
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

		public ProcessImage ( Bitmap bitmap, bool copyData = true )
		{
			colorMap = new SuperColor [ bitmap.Width, bitmap.Height ];

			if ( copyData )
			{
				var bitmapData = bitmap.LockBits ( new Rectangle ( 0, 0, bitmap.Width, bitmap.Height ),
					System.Drawing.Imaging.ImageLockMode.ReadOnly,
					System.Drawing.Imaging.PixelFormat.Format32bppArgb );

				for ( int y = 0; y < bitmap.Height; ++y )
				{
					for ( int x = 0; x < bitmap.Width; ++x )
					{
						int currentPosition = ( ( bitmap.Width * y ) + x ) * 4;
						colorMap [ x, y ] = new SuperColor ( Marshal.ReadInt32 ( bitmapData.Scan0 + currentPosition ) );
					}
				}

				bitmap.UnlockBits ( bitmapData );
			}
		}

		public Bitmap ToBitmap ()
		{
			Bitmap bitmap = new Bitmap ( Width, Height );

			var bitmapData = bitmap.LockBits ( new Rectangle ( 0, 0, bitmap.Width, bitmap.Height ),
				System.Drawing.Imaging.ImageLockMode.WriteOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			for ( int y = 0; y < bitmap.Height; ++y )
			{
				for ( int x = 0; x < bitmap.Width; ++x )
				{
					int currentPosition = ( ( bitmap.Width * y ) + x ) * 4;
					Marshal.WriteInt32 ( bitmapData.Scan0 + currentPosition, colorMap [ x, y ].ToArgb () );
				}
			}

			bitmap.UnlockBits ( bitmapData );

			return bitmap;
		}

		public SuperColor FilterProcess ( int x, int y, SuperColor [,] filter )
		{
			int filterWidth = filter.GetLength ( 0 );
			int filterHeight = filter.GetLength ( 1 );
			if ( filterWidth % 2 == 0 || filterHeight % 2 == 0 )
				throw new ArgumentException ( "Filter's length must have odd number." );
			int filterWidthCenter = filterWidth / 2;
			int filterHeightCenter = filterHeight / 2;

			SuperColor total = new SuperColor ();
			for ( int ty = -filterHeightCenter; ty < filterHeightCenter; ++ty )
			{
				for ( int tx = -filterWidthCenter; tx < filterWidthCenter; ++tx )
				{
					SuperColor c = this [ x + tx, y + ty ] * filter [ tx + filterWidthCenter, ty + filterHeightCenter ];
					total += c;
				}
			}

			return total;
		}
	}
}
