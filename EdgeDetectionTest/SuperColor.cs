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
		public float A { get; set; }
		public float R { get; set; }
		public float G { get; set; }
		public float B { get; set; }

		public SuperColor ( float a, float r, float g, float b )
			: this () { A = a; R = r; G = g; B = b; }
		public SuperColor ( Color color )
			: this ( color.A / 255.0f, color.R / 255.0f, color.G / 255.0f, color.B / 255.0f ) { }
		public SuperColor ( int argbColor ) : this ( Color.FromArgb( argbColor ) ) { }
		public SuperColor ( float s ) : this ( 1, s, s, s ) { }

		public int ToArgb ()
		{
			if ( A > 1 ) A = 1; else if ( A < 0 ) A = 0;
			if ( R > 1 ) R = 1; else if ( R < 0 ) R = 0;
			if ( G > 1 ) G = 1; else if ( G < 0 ) G = 0;
			if ( B > 1 ) B = 1; else if ( B < 0 ) B = 0;
			return Color.FromArgb ( ( int ) ( A * 255 ), ( int ) ( R * 255 ), ( int ) ( G * 255 ), ( int ) ( B * 255 ) ).ToArgb ();
		}

		public static SuperColor operator + ( SuperColor c1, SuperColor c2 )
		{
			return new SuperColor ( c1.A + c2.A, c1.R + c2.R, c1.G + c2.G, c1.B + c2.B );
		}
		public static SuperColor operator - ( SuperColor c1, SuperColor c2 )
		{
			return new SuperColor ( c1.A - c2.A, c1.R - c2.R, c1.G - c2.G, c1.B - c2.B );
		}
		public static SuperColor operator * ( SuperColor c1, SuperColor c2 )
		{
			return new SuperColor ( c1.A * c2.A, c1.R * c2.R, c1.G * c2.G, c1.B * c2.B );
		}
		public static SuperColor operator * ( SuperColor c1, float s )
		{
			return new SuperColor ( c1.A * s, c1.R * s, c1.G * s, c1.B * s );
		}
		public static SuperColor operator / ( SuperColor c1, SuperColor c2 )
		{
			return new SuperColor ( c1.A / c2.A, c1.R / c2.R, c1.G / c2.G, c1.B / c2.B );
		}
		public static SuperColor operator / ( SuperColor c1, float s )
		{
			return new SuperColor ( c1.A / s, c1.R / s, c1.G / s, c1.B / s );
		}
	}
}
