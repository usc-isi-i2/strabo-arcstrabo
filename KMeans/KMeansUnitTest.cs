using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace KdKeys.DataMining.Clustering.KMeans
{
	class UnitTest
	{
		[STAThread]
		static void Main(string[] args)
		{			
			KMeansUnitTest.Run();
			KMeansParallelUnitTest.Run();
						
			Console.ReadLine();
		}
	}

	
	class KMeansUnitTest
	{		
		public static void Run()
		{				
			KMeansUnitTest.EuclideanDistanceTest();
			KMeansUnitTest.ManhattanDistanceTest();
			KMeansUnitTest.ClusterMean();
			KMeansUnitTest.ClusterDataSetTestCase();				
		}			

		/// <summary>
		/// Test the Euclidean Distance calculation between two data points
		/// </summary>
		private static void EuclideanDistanceTest()
		{
			double [] John = new double[] {20, 170, 80};
			double [] Henry = new double[] {30, 160, 120};

			double distance = new KdKMeans().EuclideanDistance(John, Henry);
			System.Diagnostics.Debug.WriteLine(distance);
		}
		
		/// <summary>
		/// Test the Manhattan Distance calculation between two data points
		/// </summary>
		private static void ManhattanDistanceTest()
		{
			double [] John = new double[] {20, 170, 80};
			double [] Henry = new double[] {30, 160, 120};

			double distance = new KdKMeans().ManhattanDistance(John, Henry);
			System.Diagnostics.Debug.WriteLine(distance);
		}
		
		/// <summary>
		/// Test the Cluster Mean calculation between two data points
		/// </summary>
		private static void ClusterMean()
		{
			double [] John = new double[] {20, 170, 80};
			double [] Henry = new double[] {30, 160, 120};
		
			double[][] cluster = new double[2][];		
			cluster[0] = John;
			cluster[1] = Henry;
			
			double [] centroid = new KdKMeans().ClusterMean(cluster);
			
			//((20+30)/2), ((170+160)/2), ((80+120)/2)
			System.Diagnostics.Debug.WriteLine(centroid.ToString());
		}
		
		/// <summary>
		/// Test the clustering of data in an Array
		/// </summary>
		private static void ClusterDataSetTestCase()
		{			
			double[][] data = DataSample.GetDataSampleFromFile(10000, 25);

			ClusterCollection clusters;
			clusters = new KdKMeans().ClusterDataSet(5, data);
			
			//This line has been commented out. Uncomment it to serialize your object(s)
			KdKMeans.Serialize(clusters, @"kmeansclusters.xml");
		}		
	}

	

	class KMeansParallelUnitTest
	{		
		public static void Run()
		{							
			KMeansParallelUnitTest.ClusterDataSetTestCase();					
		}	
		
		/// <summary>
		/// Test the clustering of data in an Array
		/// </summary>
		private static void ClusterDataSetTestCase()
		{
			double[][] data = DataSample.GetDataSampleFromFile(10000, 25);

			ClusterCollection clusters;
			KMeansParallel kMeans = new KMeansParallel();
			clusters = kMeans.ClusterDataSet(5, data);
			
			//This line has been commented out. Uncomment it to serialize your object(s)
			KMeansParallel.Serialize(clusters, @"kmeansparallelclusters.xml");
		}		
	}

}
