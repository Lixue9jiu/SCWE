using System;

namespace SCWE
{
    public interface IMeshGenerationManager
    {
        void GenerateMeshes(int chunkx, int chunkz, int radius, int maxVertexCount, Action progress, Action<Mesh> callback);
        bool PollEvents();
    }
}
