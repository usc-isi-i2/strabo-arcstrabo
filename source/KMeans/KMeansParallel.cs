using System;
using System.Collections;
using System.Data;
using System.Threading;
using System.IO;

namespace KdKeys.DataMining.Clustering.KMeans
{
	public delegate int[] ClusterPartialDataSetDelegate(ClusterCollection clusters, double[][] data, int start, int count);

	/// <summary>
	/// This class implement a KMeans clustering algorithm
	/// </summary>
	public class KMeansParallel
	{
		private const int threads = 2;
		private ClusterPartialDataSetDelegate clusterDelegate = null;	

		public KMeansParallel()
		{
			clusterDelegate = new ClusterPartialDataSetDelegate(ClusterPartialDataSet);			
		}
		
		/// <summary>
		/// Calculates the Euclidean Distance Measure between two data points
		/// </summary>
		/// <param name="X">An array with the values of an object or datapoint</param>
		/// <param name="Y">An array with the values of an object or datapoint</param>
		/// <returns>Returns the Euclidean Distance Measure Between Points X and Points Y</returns>
		public double EuclideanDistance(double [] X, double []Y)
		{
			int count = 0;
			double distance = 0.0;
			double sum = 0.0;

			if(X.GetUpperBound(0) != Y.GetUpperBound(0))
				throw new System.ArgumentException("the number of elements in X must match the number of elements in Y");
			else
				count = X.Length;

			for (int i = 0; i < count; i++)
				sum = sum + Math.Pow(Math.Abs(X[i] - Y[i]),2);

			distance = Math.Sqrt(sum);
			return distance;
		}

		
		/// <summary>
		/// Calculates the Manhattan Distance Measure between two data points
		/// </summary>
		/// <param name="X">An array with the values of an object or datapoint</param>
		/// <param name="Y">An array with the values of an object or datapoint</param>
		/// <returns>Returns the Manhattan Distance Measure Between Points X and Points Y</returns>
		public double ManhattanDistance(double [] X, double []Y)
		{
			int count = 0;
			double distance = 0.0;
			double sum = 0.0;

			if(X.GetUpperBound(0) != Y.GetUpperBound(0))
				throw new System.ArgumentException("the number of elements in X must match the number of elements in Y");
			else
				count = X.Length;

			for (int i = 0; i < count; i++)
				sum = sum + Math.Abs(X[i] - Y[i]);

			distance = sum;
			return distance;
		}

		/// <summary>
		/// Calculates The Mean Of A Cluster OR The Cluster Center
		/// </summary>
		/// <param name="cluster">
		/// A two-dimensional array containing a dataset of numeric values
		/// </param>
		/// <returns>
		/// Returns an Array Defining A Data Point Representing The Cluster Mean or Centroid
		/// </returns>
		public double [] ClusterMean(double [][] cluster)
		{			
			double [] dataSum;
			double [] centroid;
			
			int rowCount = cluster.Length; 
			int fieldCount = cluster[0].Length;

			dataSum = new double[fieldCount];
			centroid = new double[fieldCount];

			//((20+30)/2), ((170+160)/2), ((80+120)/2)
			for( int j = 0; j < fieldCount; j++)
			{
				for (int i = 0; i < rowCount; i++)
					dataSum[j] = dataSum[j] + cluster[i][j];	

				centroid[j] = (dataSum[j] / rowCount);
			}
			
			return centroid;
		}

        public ClusterCollection RandomSeeding(int k, double[][] data)
        {
            int size = data.Length;
            double[][] seeds = new double[k][];
            Random random = new Random();
            Hashtable random_table = new Hashtable();
            Cluster cluster = null;
            ClusterCollection init_clusters = new ClusterCollection();
            for (int i = 0; i < k; )
            {
                int r = random.Next(size - 1);
                if(!random_table.ContainsKey(r))
                {
                    random_table.Add(r,0);
                    seeds[i] = new double[3];
                    seeds[i][0]=data[r][0];seeds[i][1]=data[r][1];seeds[i][2]=data[r][2];
                    cluster = new Cluster();
                    cluster.Add(seeds[i]);
                    init_clusters.Add(cluster);
                    i++;
                }
            }
            return init_clusters;
        }
        /// <summary>
        /// Seperates a dataset into clusters or groups with similar characteristics
        /// </summary>
        /// <param name="clusterCount">The number of clusters or groups to form</param>
        /// <param name="data">An array containing data that will be clustered</param>
        /// <returns>A collection of clusters of data</returns>
        public ClusterCollection ClusterDataSetRandomSeeding(int k,double[][] data)
        {
            ClusterCollection clusters = RandomSeeding(k,data);
            int rowCount = data.Length;
            int stableClustersCount = 0;

            int clusterCount = clusters.Count;

            DateTime start = DateTime.Now;
            Console.WriteLine("Start clustering {0} objects into {1} clusters: {2}", rowCount.ToString(), clusterCount.ToString(), start.ToLongTimeString());

            //do actual clustering
            int iterationCount = 0;
            while (stableClustersCount != clusters.Count)
            {
                iterationCount++;
                stableClustersCount = 0;

                //Do actual clustering
                //Console.WriteLine("Start Cluster for ineration {0}: {1}", iterationCount, DateTime.Now.ToLongTimeString());
                ClusterCollection newClusters = this.ClusterDataSet(clusters, data);
                //Console.WriteLine("  End Cluster for ineration {0}: {1}", iterationCount, DateTime.Now.ToLongTimeString());

                for (int clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
                {
                    double[] originalClusterMean = clusters[clusterIndex].ClusterMean;
                    double[] newClusterMean = newClusters[clusterIndex].ClusterMean;
                    double distance = this.EuclideanDistance(newClusterMean, originalClusterMean);
                    if (distance == 0)
                    {
                        stableClustersCount++;
                        //Console.WriteLine("{0} stable clusters out of {1}", stableClustersCount.ToString(), clusterCount.ToString());
                    }
                }
                bool reset = false;
                for (int clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
                {
                    if (newClusters[clusterIndex].Count == 0)
                    {
                        reset = true; break;
                    }
                }
                if (reset)
                    clusters = RandomSeeding(k,data);
                else
                    clusters = newClusters;
            }

            DateTime end = DateTime.Now;
            TimeSpan span = end - start;
            Console.WriteLine("End clustering {0} objects into {1} clusters with {2} iterations: {3}", rowCount.ToString(), clusterCount.ToString(), iterationCount, end.ToLongTimeString());
            Console.WriteLine("Parallel Clustering {0} objects into {1} clusters took {2} seconds", rowCount.ToString(), clusterCount.ToString(), span.TotalSeconds);
            Console.WriteLine();

            return clusters;
        }


        //My unsucessful code for providing center seed for kmean clustering
        public ClusterCollection ClusterDataSet2(int k,ClusterCollection clusters, double[][] data)
        {
            //ClusterCollection clusters = RandomSeeding(k, data);
            int rowCount = data.Length;
            int stableClustersCount = 0;

            int clusterCount = clusters.Count;

            DateTime start = DateTime.Now;
            Console.WriteLine("Start clustering {0} objects into {1} clusters: {2}", rowCount.ToString(), clusterCount.ToString(), start.ToLongTimeString());

            //do actual clustering
            int iterationCount = 0;
            while (stableClustersCount != clusters.Count)
            {
                iterationCount++;
                stableClustersCount = 0;

                //Do actual clustering
                //Console.WriteLine("Start Cluster for ineration {0}: {1}", iterationCount, DateTime.Now.ToLongTimeString());
                ClusterCollection newClusters = this.ClusterDataSet(clusters, data);
                //Console.WriteLine("  End Cluster for ineration {0}: {1}", iterationCount, DateTime.Now.ToLongTimeString());

                for (int clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
                {
                    double[] originalClusterMean = clusters[clusterIndex].ClusterMean;
                    double[] newClusterMean = newClusters[clusterIndex].ClusterMean;
                    double distance = this.EuclideanDistance(newClusterMean, originalClusterMean);
                    if (distance == 0)
                    {
                        stableClustersCount++;
                        //Console.WriteLine("{0} stable clusters out of {1}", stableClustersCount.ToString(), clusterCount.ToString());
                    }
                }
                bool reset = false;
                for (int clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
                {
                    if (newClusters[clusterIndex].Count == 0)
                    {
                        reset = true; break;
                    }
                }
                if (reset)
                    clusters = RandomSeeding(k, data);
                else
                    clusters = newClusters;
            }

            DateTime end = DateTime.Now;
            TimeSpan span = end - start;
            Console.WriteLine("End clustering {0} objects into {1} clusters with {2} iterations: {3}", rowCount.ToString(), clusterCount.ToString(), iterationCount, end.ToLongTimeString());
            Console.WriteLine("Parallel Clustering {0} objects into {1} clusters took {2} seconds", rowCount.ToString(), clusterCount.ToString(), span.TotalSeconds);
            Console.WriteLine();

            return clusters;
        }



		/// <summary>
		/// Seperates a dataset into clusters or groups with similar characteristics
		/// </summary>
		/// <param name="clusterCount">The number of clusters or groups to form</param>
		/// <param name="data">An array containing data that will be clustered</param>
		/// <returns>A collection of clusters of data</returns>
		public ClusterCollection ClusterDataSet(int clusterCount, double [][] data)
		{			
			int rowCount = data.Length; 			
			int stableClustersCount = 0;									

			Cluster cluster = null;
			ClusterCollection clusters = new ClusterCollection();			
			
			//setup seed clusters
			for (int i = 0; i < clusterCount; i++)			
			{				
				cluster = new Cluster();										
				cluster.Add(data[i]);
				clusters.Add(cluster);				
			}
			
			DateTime start = DateTime.Now;
			Console.WriteLine("Start clustering {0} objects into {1} clusters: {2}", rowCount.ToString(), clusterCount.ToString(), start.ToLongTimeString());

			//do actual clustering
			int iterationCount = 0;
			while (stableClustersCount != clusters.Count)
			{
				iterationCount++;
				stableClustersCount = 0;
				
				//Do actual clustering
				//Console.WriteLine("Start Cluster for ineration {0}: {1}", iterationCount, DateTime.Now.ToLongTimeString());
				ClusterCollection newClusters = this.ClusterDataSet(clusters, data);
				//Console.WriteLine("  End Cluster for ineration {0}: {1}", iterationCount, DateTime.Now.ToLongTimeString());
                bool reset = false;
                for (int clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
                {
                    if (newClusters[clusterIndex].Count == 0)
                    {
                        reset = true; break;
                    }
                }
                if (reset)
                    newClusters = RandomSeeding(clusterCount, data);
				for (int clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
				{
					double[] originalClusterMean = clusters[clusterIndex].ClusterMean;
					double[] newClusterMean = newClusters[clusterIndex].ClusterMean;
					double distance = this.EuclideanDistance(newClusterMean, originalClusterMean);					
					if (distance ==0)
					{
						stableClustersCount++;						
						//Console.WriteLine("{0} stable clusters out of {1}", stableClustersCount.ToString(), clusterCount.ToString());
					}			
				}
                
                clusters = newClusters;
            }

			DateTime end = DateTime.Now;
			TimeSpan span = end - start;
			Console.WriteLine("End clustering {0} objects into {1} clusters with {2} iterations: {3}", rowCount.ToString(), clusterCount.ToString(), iterationCount, end.ToLongTimeString());
			Console.WriteLine("Parallel Clustering {0} objects into {1} clusters took {2} seconds", rowCount.ToString(), clusterCount.ToString(), span.TotalSeconds);
			Console.WriteLine();

			return clusters;
		}
		

		/// <summary>
		/// Seperates a dataset into clusters or groups with similar characteristics
		/// </summary>
		/// <param name="clusters">A collection of data clusters</param>
		/// <param name="data">An array containing data to b eclustered</param>
		/// <returns>A collection of clusters of data</returns>		
		public ClusterCollection ClusterDataSet(ClusterCollection clusters, double[][] data)
		{						
			int rowCount = data.Length;  						

			// create a new collection of clusters
			ClusterCollection newClusters = new ClusterCollection();
			for(int count = 0; count < clusters.Count; count++)
			{
				Cluster newCluster = new Cluster();
				newClusters.Add(newCluster);
			}

			if(clusters.Count <= 0)			
				throw new SystemException("Cluster Count Cannot Be Zero!");
		
			//break data points into n groups			
			int remainder = rowCount % threads;
			int numPerThread = rowCount / threads;
			int start = 0;

			IAsyncResult[] asyncResults = new IAsyncResult[threads];
			WaitHandle[] handles = new WaitHandle[threads];
			for (int i = 0; i < threads; i++)
			{				
				if (i > 0)
					start += numPerThread;
				if (i == threads-1)
					numPerThread += remainder;				
				asyncResults[i] = clusterDelegate.BeginInvoke(clusters, data, start, numPerThread, null, null);
				handles[i] = asyncResults[i].AsyncWaitHandle;
			}

			int index = 0;
			foreach (IAsyncResult asyncResult in asyncResults)
			{
				int[] destinationCluster = clusterDelegate.EndInvoke(asyncResult);				
				for (int i = 0; i < destinationCluster.Length; i++)
					newClusters[ destinationCluster[i] ].Add(data[index++]);
			}

            return newClusters;
		}
		
		public int[] ClusterPartialDataSet(ClusterCollection clusters, double[][] data, int start, int count)
		{
			try
			{
				double [] clusterMean;
				double firstClusterDistance = 0.0;
				double secondClusterDistance = 0.0;
				int[] destinationCluster = new int[count];

				//((20+30)/2), ((170+160)/2), ((80+120)/2)
				for( int row = start; row < start + count; row++)
				{			
					int position = 0;
					for(int cluster = 0; cluster < clusters.Count; cluster++)
					{
						clusterMean = clusters[cluster].ClusterMean;

						if(cluster == 0)
						{
							firstClusterDistance = this.EuclideanDistance(data[row], clusterMean);
	
							position = cluster;
						}
						else
						{
							secondClusterDistance = this.EuclideanDistance(data[row], clusterMean);

							if (firstClusterDistance > secondClusterDistance)
							{
								firstClusterDistance = secondClusterDistance;

								position = cluster;
							}							
						}
					}
				
					destinationCluster[row - start] = position;				
				}

				return destinationCluster;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;
			}
		}

		#region "Remarks" - This section has been commented out. Uncomment it to serialize your objects
		
		/// <summary>
		/// Saves or Serializes a Cluster Collection To an Xml file
		/// </summary>
		/// <param name="myObject">A serializable object to be persisted to an Xml file</param>
		/// <param name="writeToXmlPath">The location of the Xml file tha will contain serialized data</param>
		/// <returns>True if the serialization is successful otherwise false</returns>
		public static bool Serialize(System.Object myObject, string writeToXmlPath)
		{
			bool state = true;
			System.Runtime.Serialization.IFormatter formatter = null;
			System.IO.Stream stream = null;
			try
			{
				formatter = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();
				stream = new System.IO.FileStream(writeToXmlPath, FileMode.Create, FileAccess.Write, FileShare.None);
				formatter.Serialize(stream, myObject);
			}
			catch(System.Exception ex)
			{
				state = false;
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}
			finally
			{
				stream.Close();
				formatter = null;
				stream = null;
			}
			return state;
		}

		#endregion - This section has been commented out. Uncomment it to serialize your objects
	}
	
	
	
	/// <summary>
	/// A class containing a group of data with similar characteristics (cluster)
	/// </summary>
	[Serializable]
	public class ParallelCluster : System.Collections.CollectionBase
	{
		private double [] _clusterSum;		
		/// <summary>
		/// The sum of all the data in the cluster
		/// </summary>
		public double [] ClusterSum
		{
			get
			{
				return this._clusterSum;
			}
		}

		private double [] _clusterMean;
		/// <summary>
		/// The mean of all the data in the cluster
		/// </summary>
		public double [] ClusterMean
		{
			get
			{
				for (int count = 0; count < this[0].Length; count++)
				{
					this._clusterMean[count] = (this._clusterSum[count] / this.List.Count);
				}

				return this._clusterMean;
			}
		}

		/// <summary>
		/// Adds a single dimension array data to the cluster
		/// </summary>
		/// <param name="data">A 1-dimensional array containing data that will be added to the cluster</param>
		public virtual void Add (double [] data)
		{
			this.List.Add(data);
			
			if (this.List.Count == 1)
			{
				this._clusterSum = new double[data.Length];

				this._clusterMean = new double[data.Length];
			}
		
			for (int count = 0; count < data.Length; count++)
			{
				this._clusterSum[count] = this._clusterSum[count] + data[count];
			}
		}
		
		/// <summary>
		/// Returns the one dimensional array data located at the index
		/// </summary>
		public virtual double [] this[int Index] 
		{ 
			get 
			{ 
				//return the Neuron at IList[Index] 
				return (double[])this.List[Index];            
			}        
		} 
	}

	/// <summary>
	/// A collection of Cluster objects or Clusters
	/// </summary>
	[Serializable]
	public class ParallelClusterCollection  : System.Collections.CollectionBase
	{
		/// <summary>
		/// Adds a Cluster to the collection of Clusters
		/// </summary>
		/// <param name="cluster">A Cluster to be added to the collection of clusters</param>
		public virtual void Add (Cluster cluster)
		{
			this.List.Add(cluster);
		}
		
		/// <summary>
		/// Returns the Cluster at this index
		/// </summary>
		public virtual Cluster this[int Index] 
		{ 
			get 
			{ 
				//return the Neuron at IList[Index] 
				return (Cluster)this.List[Index];            
			}        
		} 
	}
}
