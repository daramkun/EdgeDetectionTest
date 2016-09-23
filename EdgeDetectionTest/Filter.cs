using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDetectionTest
{
	public class Filter
	{
		public float [,] FilterData;
		public int FilterWidth;
		public int FilterHeight;

		public float Factor = 1;
		public float Bias = 0;

		public Filter ( int width, int height, float factor = 1, float bias = 0 ) : this ( new float [ width, height ], factor, bias ) { }

		public Filter ( float [,] filter, float factor = 1, float bias = 0 )
		{
			FilterData = filter;
			FilterWidth = filter.GetLength ( 0 );
			FilterHeight = filter.GetLength ( 1 );
			if ( FilterWidth % 2 == 0 || FilterHeight % 2 == 0 )
				throw new ArgumentException ( "Filter's length must have odd number." );

			Factor = factor;
			Bias = bias;
		}
	}
}
