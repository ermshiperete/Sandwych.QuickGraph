using System;
using System.Linq;
using System.Collections.Generic;
using QuickGraph.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace QuickGraph.Algorithms.Search
{
    public class UndirectedBreadthFirstAlgorithmSearchTest
    {
        [Fact]
        public void UndirectedBreadthFirstSearchAll()
        {
            Parallel.ForEach(TestGraphFactory.GetUndirectedGraphs(), g =>
                {
                    foreach (var v in g.Vertices)
                        RunBfs(g, v);
                });
        }

        private void RunBfs<TVertex, TEdge>(IUndirectedGraph<TVertex, TEdge> g, TVertex sourceVertex)
            where TEdge : IEdge<TVertex>
        {
            var parents = new Dictionary<TVertex, TVertex>();
            var distances = new Dictionary<TVertex, int>();
            TVertex currentVertex = default(TVertex);
            int currentDistance = 0;
            var algo = new UndirectedBreadthFirstSearchAlgorithm<TVertex, TEdge>(g);

            algo.InitializeVertex += u =>
            {
                Assert.Equal(algo.VertexColors[u], GraphColor.White);
            };

            algo.DiscoverVertex += u =>
            {
                Assert.Equal(algo.VertexColors[u], GraphColor.Gray);
                if (u.Equals(sourceVertex))
                    currentVertex = sourceVertex;
                else
                {
                    Assert.NotNull(currentVertex);
                    Assert.Equal(parents[u], currentVertex);
                    Assert.Equal(distances[u], currentDistance + 1);
                    Assert.Equal(distances[u], distances[parents[u]] + 1);
                }
            };
            algo.ExamineEdge += args =>
            {
                Assert.True(args.Source.Equals(currentVertex) ||
                              args.Target.Equals(currentVertex));
            };

            algo.ExamineVertex += args =>
            {
                var u = args;
                currentVertex = u;
                // Ensure that the distances monotonically increase.
                Assert.True(
                       distances[u] == currentDistance
                    || distances[u] == currentDistance + 1
                    );

                if (distances[u] == currentDistance + 1) // new level
                    ++currentDistance;
            };
            algo.TreeEdge += (sender, args) =>
            {
                var u = args.Edge.Source;
                var v = args.Edge.Target;
                if (algo.VertexColors[v] == GraphColor.Gray)
                {
                    var temp = u;
                    u = v;
                    v = temp;
                }

                Assert.Equal(algo.VertexColors[v], GraphColor.White);
                Assert.Equal(distances[u], currentDistance);
                parents[v] = u;
                distances[v] = distances[u] + 1;
            };
            algo.NonTreeEdge += (sender, args) =>
            {
                var u = args.Edge.Source;
                var v = args.Edge.Target;
                if (algo.VertexColors[v] != GraphColor.White)
                {
                    var temp = u;
                    u = v;
                    v = temp;
                }

                Assert.False(algo.VertexColors[v] == GraphColor.White);

                if (algo.VisitedGraph.IsDirected)
                {
                    // cross or back edge
                    Assert.True(distances[v] <= distances[u] + 1);
                }
                else
                {
                    // cross edge (or going backwards on a tree edge)
                    Assert.True(
                        distances[v] == distances[u]
                        || distances[v] == distances[u] + 1
                        || distances[v] == distances[u] - 1
                        );
                }
            };

            algo.GrayTarget += (sender, args) =>
            {
                //Assert.AreEqual(algo.VertexColors[args.Edge.Target], GraphColor.Gray);
            };
            algo.BlackTarget += (sender, args) =>
            {
                //Assert.AreEqual(algo.VertexColors[args.Edge.Target], GraphColor.Black);

                //foreach (var e in algo.VisitedGraph.AdjacentEdges(args.Edge.Target))
                //    Assert.IsFalse(algo.VertexColors[e.Target] == GraphColor.White);
            };

            algo.FinishVertex += args =>
            {
                Assert.Equal(algo.VertexColors[args], GraphColor.Black);
            };


            parents.Clear();
            distances.Clear();
            currentDistance = 0;

            foreach (var v in g.Vertices)
            {
                distances[v] = int.MaxValue;
                parents[v] = v;
            }
            distances[sourceVertex] = 0;
            algo.Compute(sourceVertex);

            // All white vertices should be unreachable from the source.
            foreach (var v in g.Vertices)
            {
                if (algo.VertexColors[v] == GraphColor.White)
                {
                    //!IsReachable(start,u,g);
                }
            }

            // The shortest path to a child should be one longer than
            // shortest path to the parent.
            foreach (var v in g.Vertices)
            {
                if (!parents[v].Equals(v)) // *ui not the root of the bfs tree
                    Assert.Equal(distances[v], distances[parents[v]] + 1);
            }
        }
    }
}
