using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDetectionTest
{
	public struct SuperColor
	{
		public float R, G, B;

		public SuperColor ( float r, float g, float b ) { R = r; G = g; B = b; }
		public SuperColor ( Color color )
			: this ( color.R / 255.0f, color.G / 255.0f, color.B / 255.0f ) { }
		public SuperColor ( int argbColor )
		{
			R = ( ( argbColor >> 16 ) & 0xff ) / 255.0f;
			G = ( ( argbColor >> 8 ) & 0xff ) / 255.0f;
			B = ( argbColor & 0xff ) / 255.0f;
		}
		public SuperColor ( float s ) : this ( s, s, s ) { }

		public int ToArgb ()
		{
			int r = ( int ) ( ( ( R > 1 ) ? 1 : ( R < 0 ? 0 : R ) ) * 255 ),
				g = ( int ) ( ( ( G > 1 ) ? 1 : ( G < 0 ? 0 : G ) ) * 255 ),
				b = ( int ) ( ( ( B > 1 ) ? 1 : ( B < 0 ? 0 : B ) ) * 255 );
			return ( int ) ( 0xff000000 + ( r << 16 ) + ( g << 8 ) + b );
		}

		public override string ToString () { return $"R:{( int ) ( R * 255 )}, G:{( int ) ( G * 255 )}, B:{( int ) ( B * 255 )}"; }

		public SuperColor ToGrayscale () { float c = ( R + G + B ) / 3; return new SuperColor ( c, c, c ); }

		public void Add ( ref SuperColor c )
		{
			R += c.R;
			G += c.G;
			B += c.B;
		}

		public void Multiply ( float s )
		{
			R *= s;
			G *= s;
			B *= s;
		}

		public static SuperColor operator + ( SuperColor c1, SuperColor c2 )
		{
			return new SuperColor ( c1.R + c2.R, c1.G + c2.G, c1.B + c2.B );
		}
		public static SuperColor operator + ( SuperColor c1, float s )
		{
			return new SuperColor ( c1.R + s, c1.G + s, c1.B + s );
		}
		public static SuperColor operator - ( SuperColor c1, SuperColor c2 )
		{
			return new SuperColor ( c1.R - c2.R, c1.G - c2.G, c1.B - c2.B );
		}
		public static SuperColor operator - ( SuperColor c1, float s )
		{
			return new SuperColor ( c1.R - s, c1.G - s, c1.B - s );
		}
		public static SuperColor operator * ( SuperColor c1, SuperColor c2 )
		{
			return new SuperColor ( c1.R * c2.R, c1.G * c2.G, c1.B * c2.B );
		}
		public static SuperColor operator * ( SuperColor c1, float s )
		{
			return new SuperColor ( c1.R * s, c1.G * s, c1.B * s );
		}
		public static SuperColor operator / ( SuperColor c1, SuperColor c2 )
		{
			return new SuperColor ( c1.R / c2.R, c1.G / c2.G, c1.B / c2.B );
		}
		public static SuperColor operator / ( SuperColor c1, float s )
		{
			return new SuperColor ( c1.R / s, c1.G / s, c1.B / s );
		}
	}
}
