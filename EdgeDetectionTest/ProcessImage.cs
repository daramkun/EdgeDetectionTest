using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdgeDetectionTest
{
	public enum OutOfFrameColor
	{
		NoColor,
		Border,
		Mirror,
	}

	public enum ScaleMethod
	{
		Nearest,
		Bilinear,
	}

	public class ProcessImage
	{
		SuperColor [,] colorMap;

		public int Width { get; private set; }
		public int Height { get; private set; }

		public OutOfFrameColor OutOfFrameColor = OutOfFrameColor.Mirror;

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

		public ProcessImage ( ProcessImage original, int width, int height, ScaleMethod scaleMethod = ScaleMethod.Nearest )
			: this ( width, height )
		{
			switch ( scaleMethod )
			{
				case ScaleMethod.Nearest:
					{
						float widthRatio = ( float ) original.Width / Width;
						float heightRatio = ( float ) original.Height / Height;
						Parallel.For ( 0, Height, ( y ) =>
						{
							for ( int x = 0; x < Width; ++x )
								colorMap [ x, y ] = original [ ( int ) ( x * widthRatio ), ( int ) ( y * heightRatio ) ];
						} );
					}
					break;
				case ScaleMethod.Bilinear:
					throw new NotImplementedException ();
			}
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

		public SuperColor FilterProcess ( int x, int y, Filter filter )
		{
			int filterWidthCenter = x - filter.FilterHalfWidth;
			int filterHeightCenter = y - filter.FilterHalfHeight;

			SuperColor total = new SuperColor ();
			for ( int ty = 0; ty < filter.FilterHeight; ++ty )
			{
				for ( int tx = 0; tx < filter.FilterWidth; ++tx )
				{
					var c = this [ filterWidthCenter + tx, filterHeightCenter + ty ];
					c.Multiply ( filter.FilterData [ tx, ty ] );
					total.Add ( ref c );
				}
			}

			total.Multiply ( filter.Factor );
			return total + filter.Bias;
		}

		public void MakeGrayscale ()
		{
			Parallel.For ( 0, Height, ( y ) =>
			{
				for ( int x = 0; x < Width; ++x )
					colorMap [ x, y ] = colorMap [ x, y ].ToGrayscale ();
			} );
		}

		public void MakeHSVFromRGB ()
		{
			Parallel.For ( 0, Height, ( y ) =>
			{
				for ( int x = 0; x < Width; ++x )
					colorMap [ x, y ] = colorMap [ x, y ].ToHSV ();
			} );
		}

		public void MakeRGBFromHSV ()
		{
			Parallel.For ( 0, Height, ( y ) =>
			{
				for ( int x = 0; x < Width; ++x )
					colorMap [ x, y ] = SuperColor.FromHSV ( colorMap [ x, y ] );
			} );
		}

		#region Private methods for Histogram Equalization
		private long [] GetColorCount ()
		{
			long [] counts = new long [ 256 ];
			Parallel.For ( 0, Height, ( y ) =>
			{
				for ( int x = 0; x < Width; ++x )
					Interlocked.Increment ( ref counts [ ( int ) ( colorMap [ x, y ].B * 255 ) ] );
			} );
			return counts;
		}

		private void GetColorCountRounded ( long [] c )
		{
			for ( int i = 1; i < 256; ++i )
				c [ i ] += c [ i - 1 ];
			double unit = 1 / ( double ) c [ 255 ];
			for ( int i = 0; i < 256; ++i )
				c [ i ] = ( uint ) Math.Ceiling ( ( c [ i ] * unit ) * 255 );
		}
		#endregion

		public void ApplyHistogramEqualization ()
		{
			MakeHSVFromRGB ();
			long [] colorCount = GetColorCount ();
			GetColorCountRounded ( colorCount );
			Parallel.For ( 0, Height, ( y ) =>
			{
				for ( int x = 0; x < Width; ++x )
					colorMap [ x, y ].B = colorCount [ ( int ) ( colorMap [ x, y ].B * 255 ) ] / 255f;
			} );
			MakeRGBFromHSV ();
		}

		public Func<float, bool> GetSingleThreshold ()
		{
			long [] colorCount = GetColorCount ();
			long largestDiscount = 0;
			int largestDiscountIndex = -1;
			for ( int i = 1; i < 256; ++i )
			{
				long discount = Math.Abs ( colorCount [ i ] - colorCount [ i - 1 ] );
				if ( largestDiscount <= discount )
				{
					largestDiscount = discount;
					largestDiscountIndex = i;
				}
			}
			float t = largestDiscountIndex / 255f;
			return ( c ) => { return ( c >= t ); };
		}

		public Func<float, bool> GetDoubleThreshold ()
		{
			long [] colorCount = GetColorCount ();
			long largestDiscount = 0, largestDiscount2 = 0;
			int largestDiscountIndex = -9999999, largestDiscountIndex2 = -9999999;
			for ( int i = 1; i < 256; ++i )
			{
				long discount = Math.Abs ( colorCount [ i ] - colorCount [ i - 1 ] );
				if ( largestDiscount <= discount )
				{
					largestDiscount2 = largestDiscount;
					largestDiscountIndex2 = largestDiscountIndex;
					largestDiscount = discount;
					largestDiscountIndex = i;
				}
			}
			float t = Math.Min ( largestDiscountIndex, largestDiscountIndex2 ) / 255f, t2 = Math.Max ( largestDiscountIndex, largestDiscountIndex2 ) / 255f;
			return ( c ) => { return ( c >= t && c <= t2 ); };
		}

		public void ApplyThreshold ( Func<float, bool> threshold = null )
		{
			MakeGrayscale ();
			if ( threshold == null )
				threshold = ( c ) => { return c >= 0.5f; };
			Parallel.For ( 0, Height, ( y ) =>
			{
				for ( int x = 0; x < Width; ++x )
					colorMap [ x, y ].R = colorMap [ x, y ].G = colorMap [ x, y ].B = threshold ( colorMap [ x, y ].R ) ? 1 : 0;
			} );
		}
	}
}
