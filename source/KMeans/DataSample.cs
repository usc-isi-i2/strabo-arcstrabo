using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;

namespace KdKeys.DataMining.Clustering.KMeans
{
	/// <summary>
	/// Summary description for DataSample.
	/// </summary>
	public class DataSample
	{
		public DataSample()
		{		
		}

		private static void CreateDataSampleToFile(int size, int dimension)
		{			
			double[][] data = GetDataSample(size, dimension);

			StreamWriter sw = new System.IO.StreamWriter( "DataSample_" + size.ToString() + "_" + dimension.ToString() + ".dat",
				false, System.Text.Encoding.UTF8 );
			SoapFormatter soap = new SoapFormatter();
			soap.Serialize(sw.BaseStream, data);
			sw.Flush();
			sw.Close();					
		}

		public static double[][] GetDataSampleFromFile(int size, int dimension)
		{
			string fileName = "DataSample_" + size.ToString() + "_" + dimension.ToString() + ".dat";

			if (! File.Exists(fileName))
				CreateDataSampleToFile(size, dimension);

			double[][] data = null;
			StreamReader sr = new System.IO.StreamReader(fileName, System.Text.Encoding.UTF8 );
			SoapFormatter soap = new SoapFormatter();
			data = (double[][])soap.Deserialize(sr.BaseStream);
			return data;	
		}

		public static double[][] GetDataSample(int size, int dimension)
		{
			Random random = new Random(DateTime.Now.Second);
			double[][] data = new double[size][];			
			for (int x = 0; x < size; x++)
			{	
				data[x] = new double[dimension];
				for (int y = 0; y < dimension; y++)
				{
					data[x][y] = random.Next(100);
				}				
			}
			return data;
		}

	}
}

















