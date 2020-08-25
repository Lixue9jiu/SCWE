namespace SCWE
{
    public abstract class Block
    {
        public int Index;

        public int TextureSlot;

        public string Name;

        public string ToString(int value)
        {
            return string.Format("{0}: {1}, {2}", Name, Index, TerrainChunk.GetData(value));
        }

        public virtual void Initialize(string extraData)
        {
        }
    }
}