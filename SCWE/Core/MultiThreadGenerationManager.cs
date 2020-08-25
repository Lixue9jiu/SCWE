using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCWE
{
    public class MultiThreadGenerationManager : IMeshGenerationManager
    {
        int threadCount;
        TerrainPuller puller;

        public MultiThreadGenerationManager(int threadCount)
        {
            this.threadCount = threadCount;
        }

        public void GenerateMeshes(int chunkx, int chunkz, int radius, int maxVertexCount, Action progress, Action<Mesh> callback)
        {
            puller = new TerrainPuller(threadCount, WorldManager.World.Terrain, chunkx, chunkz, radius, maxVertexCount, progress, callback);
        }

        public bool PollEvents()
        {
            if (puller != null)
            {
                return puller.Update();
            }
            return false;
        }

        private class TerrainPuller
        {
            TaskManager<(Vector2Int, MeshGenerator)> taskManager;
            Queue<Vector2Int> remainingChunks = new Queue<Vector2Int>();

            Dictionary<Vector2Int, ushort> masks = new Dictionary<Vector2Int, ushort>();
            Terrain terrain;

            Queue<MeshGenerator> freeGenerators = new Queue<MeshGenerator>();

            int maxVertexCount;
            Action progress;
            Action<Mesh> callback;

            public TerrainPuller(int threadCount,
                                 Terrain terrain,
                                 int chunkx,
                                 int chunkz,
                                 int radius,
                                 int maxVertexCount,
                                 Action progress,
                                 Action<Mesh> callback)
            {
                taskManager = new TaskManager<(Vector2Int, MeshGenerator)>(threadCount);
                taskManager.OnTaskComplete += OnChunkGenerated;

                this.terrain = terrain;

                var center = new Vector2Int(chunkx, chunkz);
                HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
                foreach (var pos in MeshGenerator.SpiralIter(center, radius))
                {
                    if (terrain.ChunkExists(pos.x, pos.y) && Vector2.Distance(pos, center) < radius)
                    {
                        masks.Add(pos, 0);
                        positions.Add(pos);
                        remainingChunks.Enqueue(pos);
                    }
                }

                foreach (var pos in positions)
                {
                    for (int i = 0; i < MeshGenerator.neighbors.Length; i++)
                    {
                        var pos2 = pos + MeshGenerator.neighbors[i];
                        if (!positions.Contains(pos2))
                        {
                            masks[pos] |= (byte)(1 << i);
                        }
                    }
                }

                for (int i = 0; i < threadCount; i++)
                {
                    freeGenerators.Enqueue(new MeshGenerator());
                }

                this.maxVertexCount = maxVertexCount;
                this.progress = progress;
                this.callback = callback;
            }

            public bool Update()
            {
                if (remainingChunks.Count > 0)
                {
                    if (freeGenerators.Count < 1)
                    {
                        if (taskManager.RunningJobCount == 0)
                        {
                            throw new Exception("Illegal state");
                        }
                        taskManager.PollEvents();
                    }
                    else
                    {
                        var pos = remainingChunks.Dequeue();
                        var g = freeGenerators.Dequeue();
                        LoadChunkWithNeighbors(pos);
                        var c = new ChunkCluster(pos, terrain);
                        taskManager.QueueJob(() => TaskJob(pos, g, c));
                    }
                    return true;
                }
                taskManager.WaitAll();
                taskManager.PollEvents();

                int vertexCount = 0;
                List<Mesh> meshes = new List<Mesh>();
                while (freeGenerators.Count > 0)
                {
                    var g = freeGenerators.Dequeue();
                    if (g.TerrainMesh.VertexCount > 0)
                    {
                        meshes.Add(g.TerrainMesh.ToMesh());
                        vertexCount += g.TerrainMesh.VertexCount;
                        g.TerrainMesh.Clear();

                        if (vertexCount > maxVertexCount)
                        {
                            callback(Mesh.Combine(meshes));
                            meshes.Clear();
                            vertexCount = 0;
                        }
                    }
                }
                if (vertexCount > 0)
                {
                    callback(Mesh.Combine(meshes));
                }
                return false;
            }

            private void LoadChunkWithNeighbors(Vector2Int pos)
            {
                foreach (var offset in MeshGenerator.neighbors)
                {
                    var pos2 = pos + offset;
                    terrain.LoadChunk(pos2.x, pos2.y);
                }
                terrain.LoadChunk(pos.x, pos.y);
            }

            private void OnChunkGenerated(object sender, (Vector2Int, MeshGenerator) pair)
            {
                Vector2Int pos = pair.Item1;
                for (int i = 0; i < MeshGenerator.neighbors.Length; i++)
                {
                    var pos2 = pos + MeshGenerator.neighbors[i];
                    if (masks.ContainsKey(pos2))
                    {
                        masks[pos2] |= (byte)(1 << MeshGenerator.opposite[i]);

                        if (masks[pos2] == 0x1ff)
                        {
                            terrain.DisposeChunk(pos2.x, pos2.y);
                        }
                    }
                }
                masks[pos] |= 0x100;
                if (masks[pos] == 0x1ff)
                {
                    terrain.DisposeChunk(pos.x, pos.y);
                }
                progress();
                if (pair.Item2.TerrainMesh.VertexCount > maxVertexCount)
                {
                    callback(pair.Item2.TerrainMesh.ToMesh());
                    pair.Item2.TerrainMesh.Clear();
                }
                if (freeGenerators.Contains(pair.Item2))
                {
                    throw new Exception("Illegal state");
                }
                freeGenerators.Enqueue(pair.Item2);
            }

            private (Vector2Int, MeshGenerator) TaskJob(Vector2Int pos, MeshGenerator g, ChunkCluster c)
            {
                g.GenerateChunkMesh(pos.x, pos.y, c);
                return (pos, g);
            }
        }
    }
}
