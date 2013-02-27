using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemporalNetworks
{
    /// <summary>
    /// Used to specify directed vs. undirected edges
    /// </summary>
    public enum EdgeType { Directed = 0, Undirected = 1 }

    /// <summary>
    /// Instances of this class represent a weighted, time-aggregated perspective on a temporal network in which the weight
    /// represents the number of times an edge was active.
    /// </summary>
    public class WeightedNetwork : Dictionary<Tuple<string, string>, double>
    {
        private List<string> _vertices;
        private Dictionary<string, List<string>> _successors;
        private Dictionary<string, List<string>> _predecessors;
        private Dictionary<string, double> _cumulativeOutWeight;
        private Dictionary<string, double> _cumulativeInWeight;
        Random r;

        /// <summary>
        /// Creates an empty weighted network
        /// </summary>
        public WeightedNetwork()
        {
            _vertices = new List<string>();
            _successors = new Dictionary<string, List<string>>();
            _predecessors = new Dictionary<string, List<string>>();
            _cumulativeInWeight = new Dictionary<string, double>();
            _cumulativeOutWeight = new Dictionary<string, double>();
            r = new Random();
        }

        /// <summary>
        /// Creates a weighted network representation of a temporal network by aggregating edge occurences 
        /// (and thus discarding information on the temporal ordering of edges)
        /// </summary>
        /// <param name="temp_net">he temporal network that shall be aggregated</param>
        /// <returns>An instance of a weighted aggregate network</returns>
        public static WeightedNetwork FromTemporalNetwork(TemporalNetwork temp_net)
        {
            WeightedNetwork weighted_net = new WeightedNetwork();

            foreach(var t in temp_net.Keys)
                foreach (Tuple<string, string> edge in temp_net[t])
                    weighted_net.AddEdge(edge.Item1, edge.Item2);
            return weighted_net;
        }

        /// <summary>
        /// Returns all vertices present in this network
        /// </summary>
        public IEnumerable<string> Vertices
        {
            get
            {
                return _vertices;
            }
        }

        /// <summary>
        /// Returns all edges present in this network
        /// </summary>
        public IEnumerable<Tuple<string, string>> Edges
        {
            get 
            {
                return this.Keys;
            }
        }

        /// <summary>
        /// Returns the number of vertices
        /// </summary>
        public int VertexCount 
        {
            get
            {
                return _vertices.Count;
            }
        }

        /// <summary>
        /// Returns the number of weighted edges (not considering weights)
        /// </summary>
        public int EdgeCount
        {
            get
            {
                return this.Count;
            }
        }
        
        /// <summary>
        /// Returns the cumulative weight of the whole network (i.e. the sum over all edge weights)
        /// </summary>
        public double CumulativeWeight
        {
            get
            {
                return this.Values.Sum();
            }
        }

        /// <summary>
        /// Returns a random predecessor of a given node (equal to performing an unbiased random walk over the incoming edges)
        /// </summary>
        /// <param name="v">The node for which a random predecessor will be returned</param>
        /// <returns>A random node name</returns>
        public string GetRandomPredecessor(string v)
        {
            return _predecessors[v][r.Next(_predecessors[v].Count)];
        }

        /// <summary>
        /// Returns a random successor of a given node (equal to performing an unbiased random walk over the outgoing edges)
        /// </summary>
        /// <param name="v">The node for which a random predecessor will be returned</param>
        /// <returns>A random node name</returns>
        public string GetRandomSuccessor(string v, bool weighted = false)
        {
            if (GetOutdeg(v) == 0)
                return null;

            if (!weighted)
                return _successors[v][r.Next(_successors[v].Count)];
            else
            {
                Dictionary<double, string> cumulative = new Dictionary<double, string>();
                double sum = 0d;
                foreach (string n in _successors[v])
                {
                    cumulative[sum + GetWeight(v, n)/GetCumulativeOutWeight(v)] = n;
                    sum += GetWeight(v, n) / GetCumulativeOutWeight(v);
                }
                double dice = r.NextDouble();
                for (int i = 0; i < _successors[v].Count; i++)
                {
                    if(cumulative.Keys.ElementAt(i)>dice)
                        return cumulative.Values.ElementAt(i);
                }
                throw new Exception("This should never happen!");
            }
        }

        /// <summary>
        /// Returns the weight of an edge
        /// </summary>
        /// <param name="source">The source node of the edge</param>
        /// <param name="target">The target node of the edge</param>
        /// <returns>The (integer) weight of the edge</returns>
        public double GetWeight(string source, string target)
        {
            Tuple<string, string> t = new Tuple<string, string>(source, target);
            return GetWeight(t);
        }

        /// <summary>
        /// Returns the weight of an edge
        /// </summary>
        /// <param name="t">A tupel representing an edge</param>
        /// <returns>The (integer) weight of the edge</returns>
        public double GetWeight(Tuple<string, string> t)
        {
            if (!this.ContainsKey(t))
                return 0;
            else
                return this[t];
        }

        /// <summary>
        /// Adds either a directed (default) or undirected edge between two nodes to the weighted network
        /// </summary>
        /// <param name="source">The source of this edge</param>
        /// <param name="target">The target of this edge</param>
        /// <param name="edgetype">The type of the edge to be added (directed by default).</param>
        public void AddEdge(string source, string target, EdgeType  edgetype = EdgeType.Directed, double weight = 1d)
        {
            if (!_vertices.Contains(source))
                _vertices.Add(source);
            if (!_vertices.Contains(target))
                _vertices.Add(target);

            Tuple<string, string> edge = new Tuple<string, string>(source, target);

            // Create edge or increase weight
            if (!this.ContainsKey(edge))
                this[edge] = weight;
            else
                this[edge] = this[edge] + weight;

            RegisterEdge(edge);

            if (edgetype == EdgeType.Undirected)
            {
                edge = new Tuple<string, string>(target, source);

                // Create edge or increase weight
                if (!this.ContainsKey(edge))
                    this[edge] = 1;
                else
                    this[edge] = this[edge] + 1;

                RegisterEdge(edge);
            }
        }

        /// <summary>
        /// Registers a directed edge with the predecessor and successor dictionaries (to speed up later access)
        /// </summary>
        /// <param name="edge">The edge to register</param>
        private void RegisterEdge(Tuple<string, string> edge)
        {
            if (!_successors.ContainsKey(edge.Item1))
                _successors[edge.Item1] = new List<string>();

            if (!_successors[edge.Item1].Contains(edge.Item2))
                _successors[edge.Item1].Add(edge.Item2);

            if (!_predecessors.ContainsKey(edge.Item2))
                _predecessors[edge.Item2] = new List<string>();

            if (!_predecessors[edge.Item2].Contains(edge.Item1))
                _predecessors[edge.Item2].Add(edge.Item1);
        }

        /// <summary>
        /// Unregisters a directed edge with the predecessor and successor dictionaries
        /// </summary>
        /// <param name="edge">The edge to unregister</param>
        private void UnregisterEdge(Tuple<string, string> edge)
        {
            _successors[edge.Item1].Remove(edge.Item2);
            _predecessors[edge.Item2].Remove(edge.Item1);
        }

        /// <summary>
        /// Increases or decreases the weight of an edge in the weighted network. If no added weight is specified, the weight will be increased by 1
        /// </summary>
        /// <param name="source">The source of the edge whose weight shall be changed</param>
        /// <param name="target">The target of the edge whose weight shall be changed</param>
        /// <param name="added_weight">The value to add to the weight. This can be negative if the weight shall be decreased. A default value of 1 will be used if this parameter is omitted</param>
        public void AddToWeight(string source, string target, double added_weight = 1d)
        {
            Tuple<string, string> edge = new Tuple<string, string>(source, target);
            if (!this.ContainsKey(edge))
                this[edge] = added_weight;
            else 
                this[edge] = this[edge] + added_weight;

            // If the weight is zero, destroy the edge
            if (this[edge] == 0d)
            {
                this.Remove(edge);
                UnregisterEdge(edge);
            }
        }

        /// <summary>
        /// Returns the list of successors of a node
        /// </summary>
        /// <param name="node">The node for which successors will be listed</param>
        /// <returns>A list of successor nodes</returns>
        public IEnumerable<string> GetSuccessors(string node)
        {
            if (!_successors.ContainsKey(node))
                return new string[0];
            return _successors[node];
        }

        /// <summary>
        /// Returns the list of predecessors of a node
        /// </summary>
        /// <param name="node">The node for which predecessors will be listed</param>
        /// <returns>A list of predecessor nodes</returns>
        public IEnumerable<string> GetPredecessors(string node)
        {
            if (!_predecessors.ContainsKey(node))
                return new string[0];
            return _predecessors[node];
        }

        /// <summary>
        /// Returns the in-degree of a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetIndeg(string node)
        {
            if (!_predecessors.ContainsKey(node))
                return 0;
            return _predecessors[node].Count;
        }

        /// <summary>
        /// Returns the cumulative weight of all incoming edges of a node
        /// </summary>
        /// <param name="node">The node to compute the cumulative weight</param>
        /// <returns></returns>
        public double GetCumulativeInWeight(string node)
        {
            if(!_cumulativeInWeight.ContainsKey(node))
            {
                if (!_predecessors.ContainsKey(node))
                    return _cumulativeInWeight[node] = 0;
                else
                {
                    double sum = 0;
                    foreach (string x in _predecessors[node])
                        sum += this[new Tuple<string, string>(x, node)];
                    _cumulativeInWeight[node] = sum;
                }
            }
            return _cumulativeInWeight[node];
        }

        /// <summary>
        /// Returns the cumulative weight of all incoming edges of a node
        /// </summary>
        /// <param name="node">The node to compute the cumulative weight</param>
        /// <returns></returns>
        public double GetCumulativeOutWeight(string node)
        {
            if (!_cumulativeOutWeight.ContainsKey(node))
            {
                if (!_successors.ContainsKey(node))
                    _cumulativeOutWeight[node] = 0d;
                else 
                {
                    double sum = 0;
                    foreach (string x in _successors[node])
                        sum += this[new Tuple<string, string>(node, x)];
                    _cumulativeOutWeight[node] = sum;
                }
            }
            return _cumulativeOutWeight[node];
        }

        /// <summary>
        /// Returns a random node of the network
        /// </summary>
        public string RandomNode
        {
            get {
                return this.Vertices.ElementAt(r.Next(this.Vertices.Count()));
            }
        }

        /// <summary>
        /// Returns a random edge of the network
        /// </summary>
        public Tuple<string,string> RandomEdge
        {
            get
            {
                return this.Keys.ElementAt(r.Next(this.Keys.Count()));
            }
        }

        /// <summary>
        /// Returns the out-degree of a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetOutdeg(string node)
        {
            if (!_successors.ContainsKey(node))
                return 0;
            return _successors[node].Count;
        }

        /// <summary>
        /// Checks whether this weighted network is identical to a given one (in terms of node names, edges and weights)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
                return false;

            WeightedNetwork temp = obj as WeightedNetwork;

            foreach (var edge in temp.Edges)
            {
                if (!this.ContainsKey(edge))
                    return false;
                if (this[edge] != temp[edge])
                    return false;
            }

            foreach (var edge in this.Edges)
            {
                if (!temp.ContainsKey(edge))
                    return false;
                if (temp[edge] != this[edge])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Return the hase code of the underlying dictionary
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        /// <summary>
        /// Saves a weighted aggregate network to a file in the edge format
        /// </summary>
        /// <param name="path"></param>
        /// <param name="net"></param>
        public static void SaveToFile(string path, WeightedNetwork net)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var edge in net.Edges)
                sb.AppendLine(string.Format("{0} {1} {2}", edge.Item1, edge.Item2, string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat, "{0}", net[edge])));

            System.IO.File.WriteAllText(path, sb.ToString());
        }

        /// <summary>
        /// Returns the maximum in-degree (without considering weights) of nodes in the network
        /// </summary>
        public int MaxIndeg
        {
            get 
            {
                int max = 0;
                foreach (string v in Vertices)
                    max = Math.Max(GetIndeg(v), max);
                return max;
            }
        }

        /// <summary>
        /// Returns the maximum out-degree (without considering weights) of nodes in the network
        /// </summary>
        public int MaxOutdeg
        {
            get
            {
                int max = 0;
                foreach (string v in Vertices)
                    max = Math.Max(GetOutdeg(v), max);
                return max;
            }
        }


        /// <summary>
        /// Returns the maximum weight of edges in the network
        /// </summary>
        public double MaxWeight
        {
            get
            {
                if (this.Count == 0)
                    return 0;
                return this.Values.Max();
            }
        }

        /// <summary>
        /// Returns the maximum weight of edges in the network
        /// </summary>
        public double MinWeight
        {
            get
            {
                if (this.Count == 0)
                    return 0;
                return this.Values.Min();
            }
        }
    }
}
