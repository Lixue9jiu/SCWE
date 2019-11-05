using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using static SCWE.Utils.XMLUtils;

namespace SCWE
{
    public class FurnitureSet
    {
        Dictionary<int, Mesh[]> furnitures = new Dictionary<int, Mesh[]>();

        public bool[] isTransparent;

        public Mesh GetFurniture(int index, int rotation)
        {
            return furnitures[index][rotation];
        }

        public void Load(ProjectData project)
        {
            XElement designs = project.GetSubsystem("FurnitureBlockBehavior").GetValues("FurnitureDesigns");

            List<Furniture> fs = new List<Furniture>();
            List<int> isTrans = new List<int>();
            foreach (XElement elem in designs.Elements("Values"))
            {
                fs.Add(LoadFurniture(elem));
            }
            int count = 0;
            foreach (Furniture f in fs)
            {
                int[] data = f.Data;
                count = Math.Max(count, f.Index);

                for (int i = 0; i < data.Length; i++)
                {
                    if (TerrainChunk.GetContent(data[i]) == 15)
                    {
                        isTrans.Add(f.Index);
                        break;
                    }
                }
            }

            isTransparent = new bool[count + 1];
            foreach (int i in isTrans)
            {
                isTransparent[i] = true;
            }

            foreach (Furniture f in fs)
            {
                if (f.TerrainUseCount > 0)
                    LoadMash(f);
            }
        }

        Furniture LoadFurniture(XElement furniture)
        {
            int resolution;
            furniture.GetValue("Resolution", out resolution);
            int terrainUseCount = furniture.GetValue<int>("TerrainUseCount");
            if (terrainUseCount > 0)
            {
                Furniture f = new Furniture
                {
                    Index = int.Parse(furniture.Attribute("Name").Value),
                    Resolution = resolution,
                    Data = ParseData(FindValueByName(furniture, "Values"), resolution),
                    TerrainUseCount = terrainUseCount
                };
                return f;
            }
            return new Furniture
            {
                Index = int.Parse(furniture.Attribute("Name").Value),
                Resolution = resolution,
                Data = new int[0],
                TerrainUseCount = terrainUseCount
            };
        }

        void LoadMash(Furniture furniture)
        {
            Mesh mesh;
            MeshGenerator.GenerateFurnitureMesh(furniture, out mesh);
            Matrix4x4 t = Matrix4x4.Translate(new Vector3(0.5f, 0f, 0.5f));
            Matrix4x4 inverseT = t.Inverse;
            Mesh[] all = new Mesh[4];
            all[0] = mesh;
            all[1] = mesh.Clone();
            all[1].Transform(t * Matrix4x4.Euler(0, 270, 0) * inverseT);
            all[2] = mesh.Clone();
            all[2].Transform(t * Matrix4x4.Euler(0, 180, 0) * inverseT);
            all[3] = mesh.Clone();
            all[3].Transform(t * Matrix4x4.Euler(0, 90, 0) * inverseT);
            furnitures[furniture.Index] = all;
        }

        int[] ParseData(string str, int resolution)
        {
            List<int> data = new List<int>(resolution * resolution);
            string[] strs = str.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; i++)
            {
                string[] s = strs[i].Split('*');
                int count = int.Parse(s[0]);
                int value = int.Parse(s[1]);
                for (int k = 0; k < count; k++)
                {
                    data.Add(value);
                }
            }
            return data.ToArray();
        }
    }
}
