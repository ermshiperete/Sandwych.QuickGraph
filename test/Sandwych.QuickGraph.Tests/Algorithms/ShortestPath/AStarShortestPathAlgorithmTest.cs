using System;
using System.Collections.Generic;
using QuickGraph.Algorithms.Observers;
using QuickGraph.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace QuickGraph.Algorithms.ShortestPath
{
    public class AStartShortestPathAlgorithmTest
    {
        [Theory, GraphData]
        public void AStartAll(AdjacencyGraph<string, Edge<string>> g)
        {
            foreach (var root in g.Vertices)
                this.AStar(g, root);
        }

        private void AStar<TVertex, TEdge>(IVertexAndEdgeListGraph<TVertex, TEdge> g, TVertex root)
            where TEdge : IEdge<TVertex>
        {
            var distances = new Dictionary<TEdge, double>();
            foreach (var e in g.Edges)
                distances[e] = g.OutDegree(e.Source) + 1;

            var algo = new AStarShortestPathAlgorithm<TVertex, TEdge>(
                g,
                e => distances[e],
                v => 0
                );
            var predecessors = new VertexPredecessorRecorderObserver<TVertex, TEdge>();
            using (predecessors.Attach(algo))
                algo.Compute(root);

            Verify(algo, predecessors);
        }

        private static void Verify<TVertex, TEdge>(
            AStarShortestPathAlgorithm<TVertex, TEdge> algo,
            VertexPredecessorRecorderObserver<TVertex, TEdge> predecessors
            )
            where TEdge : IEdge<TVertex>
        {
            // let's verify the result
            foreach (var v in algo.VisitedGraph.Vertices)
            {
                TEdge predecessor;
                if (!predecessors.VertexPredecessors.TryGetValue(v, out predecessor))
                    continue;
                if (predecessor.Source.Equals(v))
                    continue;
                double vd, vp;
                bool found;
                Assert.Equal(
                    found = algo.TryGetDistance(v, out vd),
                    algo.TryGetDistance(predecessor.Source, out vp)
                    );
            }
        }
    }
}
