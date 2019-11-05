using System;
using System.Collections.Generic;
using System.IO;

namespace SCWE
{
    public static class BlockMeshesManager
    {
        static Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

        public static void LoadAllMeshes(string dir)
        {
            foreach (string file in Directory.EnumerateFiles(dir))
            {
                if (Path.GetExtension(file) == ".ply")
                {
                    using (Stream s = File.OpenRead(file))
                    {
                        LoadMesh(Path.GetFileNameWithoutExtension(file), s);
                    }
                }
            }

            foreach (string file in Directory.EnumerateDirectories(dir))
            {
                LoadAllMeshes(file);
            }
        }

        public static void LoadMesh(string name, Stream s)
        {
            //Console.Write("loading block mesh: {0}...", name);
            //Stopwatch watch = Stopwatch.StartNew();
            Mesh m = ModelImporter.ImportPly(s);
            m.Transform(new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, -1, 0,
                0, 0, 0, 1));
            meshes[name] = m;
            //Console.WriteLine(watch.ElapsedMilliseconds);
        }

        public static Mesh FindMesh(string name)
        {
            if (meshes.ContainsKey(name))
            {
                return meshes[name];
            }
            Console.WriteLine("cannot find block mesh: " + name);
            return Mesh.CreateEmpty();
        }
    }
}
