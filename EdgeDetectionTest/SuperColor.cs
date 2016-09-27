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
		public static SuperColor FromHSV ( SuperColor hsv )
		{
			float Hd = 6 * hsv.R;
			int Hdi = ( int ) Math.Floor ( 6 * hsv.R );
			float F = Hd - Hdi;
			float P = hsv.B * ( 1 - hsv.G );
			float Q = hsv.B * ( 1 - hsv.G * F );
			float T = hsv.B * ( 1 - hsv.G * ( 1 - F ) );

			switch ( Hdi )
			{
				case 0: return new SuperColor ( hsv.B, T, P );
				case 1: return new SuperColor ( Q, hsv.B, P );
				case 2: return new SuperColor ( P, hsv.B, T );
				case 3: return new SuperColor ( P, Q, hsv.B );
				case 4: return new SuperColor ( T, P, hsv.B );
				case 5: return new SuperColor ( hsv.B, P, Q );
				case 6: return new SuperColor ( hsv.B, T, P );
				case -1: return new SuperColor ( hsv.B, P, Q );
				default: throw new ArgumentException ();
			}
		}

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

		public SuperColor ToCorrection ()
		{
			float r = ( ( R > 1 ) ? 1 : ( R < 0 ? 0 : R ) ),
				g = ( ( G > 1 ) ? 1 : ( G < 0 ? 0 : G ) ),
				b = ( ( B > 1 ) ? 1 : ( B < 0 ? 0 : B ) );
			return new SuperColor ( r, g, b );
		}

		public SuperColor ToGrayscale ()
		{
			float c = ( ( ( 299 * ( ( int ) R * 255 ) ) + ( 587 * ( ( int ) G * 255 ) ) + ( 114 * ( ( int ) B * 255 ) ) ) / 1000 ) / 255.0f;
			return new SuperColor ( c, c, c );
		}

		public SuperColor ToHSV ()
		{
			float rgb_min = Math.Min ( R, Math.Min ( G, B ) ),
				rgb_max = Math.Max ( R, Math.Max ( G, B ) );

			if ( rgb_min == rgb_max ) return new SuperColor ( 0, 0, rgb_min );

			float d = ( R == rgb_min ) ? G - B : ( ( B == rgb_min ) ? R - G : B - R ),
				h = ( R == rgb_min ) ? 3 : ( ( B == rgb_min ) ? 1 : 5 );

			float V = rgb_max;
			float delta = V - rgb_min;
			float S = delta / V;
			float H = 0;
			if ( R == V ) H = ( 1 / 6f ) * ( ( G - B ) / delta );
			else if ( G == V ) H = ( 1 / 6f ) * ( 2 + ( ( B - R ) / delta ) );
			else if ( B == V ) H = ( 1 / 6f ) * ( 4 + ( ( R - G ) / delta ) );
			if ( H < 0 ) H += 1;

			return new SuperColor ( H, S, V );
		}

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
