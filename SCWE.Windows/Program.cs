using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Globalization;

namespace SCWE.Windows
{
    class Program
    {
        const string DataFolder = "data";
        const string TempFolder = "temp";
        const string Usage = "usage: SCWE.Windows.exe [-c chunkx,chunky] [-r radius] [-v vertex_threshhold] [-j thread_count] [-lang en|zh] scworld_file_name";

        static void Main(string[] args)
        {
            var commandArgs = ReadArgs(args);

            if (commandArgs.options.ContainsKey("lang"))
            {
                Language.Initialize(commandArgs.options["lang"]);
            }
            else
            {
                Language.Initialize(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
            }

            string worldFile;
            if (commandArgs.arguments.Length == 0)
            {
                Console.WriteLine(Usage);
                Console.WriteLine();
                worldFile = ReadInput(Language.GetString("input_file")).Replace("\"", "");
            }
            else
            {
                worldFile = commandArgs.arguments[0];
            }
            if (!File.Exists(worldFile))
            {
                Console.WriteLine(Language.GetString("file_not_found"));
                ReadInput(Language.GetString("enter_exit"));
                return;
            }

            Vector2Int? centerChunk;
            try
            {
                var center = GetOptionOrDefault("c", Language.GetString("input_center_chunk"), commandArgs);
                var c = center.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                centerChunk = new Vector2Int(int.Parse(c[0].Trim()), int.Parse(c[1].Trim()));
            }
            catch(Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                centerChunk = null;
            }

            int radius;
            try
            {
                var r = GetOptionOrDefault("r", Language.GetString("input_radius"), commandArgs);
                radius = int.Parse(r);
                if (radius < 1)
                {
                    throw new Exception("Illegal argument");
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                radius = 64;
            }

            int vertex_thresh;
            try
            {
                var r = GetOptionOrDefault("v", Language.GetString("input_vertex_count"), commandArgs);
                vertex_thresh = int.Parse(r);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                vertex_thresh = 1000000;
            }

            int threadCount;
            try
            {
                var r = GetOptionOrDefault("j", string.Format(Language.GetString("input_thread_count"), Environment.ProcessorCount), commandArgs);
                threadCount = int.Parse(r);
                threadCount = Math.Max(1, threadCount);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                threadCount = Environment.ProcessorCount;
            }

            try
            {
                ProjectManager.Initialize(new ProjectManager.Config { DataPath = DataFolder, VertexCountThreshold = vertex_thresh });
                ProjectManager.LoadWorld(TempFolder, worldFile);

                if (!centerChunk.HasValue)
                {
                    var p = WorldManager.Project.PlayerPosition;
                    centerChunk = new Vector2Int((int)p.x >> 4, (int)p.z >> 4);
                }

                string OutputFolder = Path.Combine(Path.GetDirectoryName(worldFile), Path.GetFileNameWithoutExtension(worldFile));

                int chunkCount = MeshGenerator.CheckChunkCount(centerChunk.Value.x, centerChunk.Value.y, radius);
                Console.WriteLine();
                Console.WriteLine(Language.GetString("finish_config"), centerChunk.Value, radius, threadCount, chunkCount, OutputFolder);

                var f = Directory.Exists(OutputFolder) ? Directory.GetFiles(OutputFolder) : new string[0];
                if (f.Length != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine(Language.GetString("non_empty_folder_warning"));
                    Console.Write(Language.GetString("y_confirm"));
                    if (Console.ReadKey(true).Key != ConsoleKey.Y)
                    {
                        return;
                    }
                    Console.WriteLine();
                }
                Console.Write(Language.GetString("enter_generate"));

                if (Console.ReadKey(true).Key != ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return;
                }

                Console.WriteLine();

                foreach (string file in f)
                {
                    File.Delete(file);
                }

                if (Directory.Exists(Path.Combine(TempFolder, "EmbeddedContent")))
                {
                    foreach (string fname in Directory.EnumerateFiles(Path.Combine(TempFolder, "EmbeddedContent"), "*.scbtex"))
                    {
                        File.Copy(fname, Path.Combine(OutputFolder, Path.GetFileNameWithoutExtension(fname) + ".png"));
                    }
                }

                Stopwatch watch = Stopwatch.StartNew();
                IMeshGenerationManager m;
                if (threadCount == 1)
                {
                    m = new SingleThreadGenerationManager();
                }
                else
                {
                    m = new MultiThreadGenerationManager(threadCount);
                }
                GenerateMesh(m, centerChunk.Value.x, centerChunk.Value.y, radius, OutputFolder);
                Console.WriteLine(Language.GetString("generate_success"),
                                  Directory.GetFiles(OutputFolder).Length,
                                  watch.ElapsedMilliseconds > 1000 ?
                                  string.Format("{0:0.00} " + Language.GetString("s"), watch.Elapsed.TotalSeconds) :
                                  string.Format("{0:0.00} " + Language.GetString("ms"), watch.Elapsed.TotalMilliseconds));

                Console.Write(Language.GetString("enter_exit"));
                BlockUntilEnter();
            }
            catch (Exception e)
            {
                Console.WriteLine(Language.GetString("error"));
                Console.WriteLine(e);
                Console.Write(Language.GetString("enter_exit"));
                BlockUntilEnter();
            }
            finally
            {
                CleanUp();
            }
        }

        public static void GenerateMesh(IMeshGenerationManager manager, int chunkx, int chunkz, int radius, string outputDir)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var terrain = WorldManager.World.Terrain;
            foreach (var p in terrain.LoadedChunks)
                terrain.DisposeChunk(p.x, p.y);

            Console.WriteLine("generating chunk mesh...");
            var cursorPos = new Vector2Int(Console.CursorLeft, Console.CursorTop);

            int count = 0;
            int file_count = 0;

            manager.GenerateMeshes(chunkx, chunkz, radius, ProjectManager.Settings.VertexCountThreshold, () =>
            {
                Console.Write(++count);
                Console.SetCursorPosition(cursorPos.x, cursorPos.y);
            }, (tm) =>
            {
                ExportMesh(chunkx, chunkz, tm, Path.Combine(outputDir, (++file_count) + ".ply"));
            });
            while (manager.PollEvents())
            {
            }
        }

        private static void ExportMesh(int chunkx, int chunkz, Mesh m, string outputPath)
        {
            m.Transform(new Matrix4x4(
                        1, 0, 0, 0,
                        0, 0, 1, 0,
                        0, 1, 0, 0,
                        0, 0, 0, 1) * Matrix4x4.Translate(new Vector3(-(chunkx << TerrainChunk.SizeXShift), 0, -(chunkz << TerrainChunk.SizeZShift))));
            using (Stream s = File.OpenWrite(outputPath))
            {
                ModelExporter.ExportPly(m, s);
            }
        }

        static void BlockUntilEnter()
        {
            while (Console.ReadKey(true).Key != ConsoleKey.Enter)
            {
            }
            Console.WriteLine();
        }

        static void CleanUp()
        {
            ProjectManager.Dispose();
            Directory.Delete(TempFolder, true);
        }

        struct CommandArgs
        {
            public string[] arguments;
            public Dictionary<string, string> options;
        }

        static string GetOptionOrDefault(string name, string prompt, CommandArgs args)
        {
            if (args.options.ContainsKey(name))
            {
                return args.options[name];
            }
            else
            {
                return ReadInput(prompt);
            }
        }

        static CommandArgs ReadArgs(string[] args)
        {
            var arguments = new List<string>();
            var options = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i++)
            {
                string s = args[i];
                if (s[0] == '-')
                {
                    if (i + 1 < args.Length && args[i + 1][0] != '-')
                        options[s.Substring(1)] = args[++i];
                    else
                        options[s.Substring(1)] = string.Empty;
                }
                else
                {
                    arguments.Add(s);
                }
            }
            return new CommandArgs
            {
                arguments = arguments.ToArray(),
                options = options
            };
        }

        static string ReadInput(string note)
        {
            Console.Write(note);
            return Console.ReadLine();
        }
    }
}
