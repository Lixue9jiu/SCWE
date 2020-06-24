using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace SCWE.Windows
{
    class Program
    {
        const string DataFolder = "data";
        const string TempFolder = "temp";
        const string Usage = "usage: SCWE.Windows.exe [-c chunkx,chunky] [-r radius] scworld_file_name";

        static void Main(string[] args)
        {
            var commandArgs = ReadArgs(args);
            string worldFile;
            if (commandArgs.arguments.Length == 0)
            {
                Console.WriteLine(Usage);
                Console.WriteLine();
                worldFile = ReadInput("请输入.scworld文件路径：").Replace("\"", "");
            }
            else
            {
                worldFile = commandArgs.arguments[0];
            }
            if (!File.Exists(worldFile))
            {
                Console.WriteLine("找不到输入的文件！");
                ReadInput("按Enter键退出");
                return;
            }

            Vector2Int? centerChunk;
            try
            {
                var center = GetOptionOrDefault("c", "输入中心区块（留空使用玩家所在区块）：", commandArgs);
                var c = center.Split(',');
                centerChunk = new Vector2Int(int.Parse(c[0]), int.Parse(c[1]));
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
                var r = GetOptionOrDefault("r", "输入半径（留空使用 1 ）：", commandArgs);
                radius = int.Parse(r);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e);
#endif
                radius = 1;
            }

            try
            {
                ProjectManager.Initialize(new ProjectManager.Config { dataPath = DataFolder });
                ProjectManager.LoadWorld(TempFolder, worldFile);

                if (!centerChunk.HasValue)
                {
                    var p = WorldManager.Project.PlayerPosition;
                    centerChunk = new Vector2Int((int)p.x >> 4, (int)p.z >> 4);
                }

                string OutputFolder = Path.Combine(Path.GetDirectoryName(worldFile), Path.GetFileNameWithoutExtension(worldFile));

                int chunkCount = ProjectManager.CheckChunkCount(centerChunk.Value.x, centerChunk.Value.y, radius);
                Console.WriteLine();
                Console.WriteLine("配置完成");
                Console.WriteLine("中心区块：" + centerChunk.Value);
                Console.WriteLine("半径：" + radius);
                Console.WriteLine("符合条件的区块有：{0} 个", chunkCount);
                Console.WriteLine("模型会被存入同目录下的 {0} 文件夹", OutputFolder);

                var f = Directory.Exists(OutputFolder) ? Directory.GetFiles(OutputFolder) : new string[0];
                if (f.Length != 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("！！！！！！！警告！！！！！！!");
                    Console.WriteLine("输出文件夹不是空的，继续生成会清空输出文件夹");
                    Console.WriteLine();
                }
                Console.Write("按Enter开始生成模型，按其他键退出");

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

                Stopwatch watch = Stopwatch.StartNew();
                ProjectManager.GenerateMesh(centerChunk.Value.x, centerChunk.Value.y, radius, OutputFolder);
                Console.WriteLine("生成成功，总共生成了 {0} 个模型，用时 {1}ms", Directory.GetFiles(OutputFolder).Length, watch.ElapsedMilliseconds);

                Console.Write("按Enter键退出");
                BlockUntilEnter();
            }
            catch (Exception e)
            {
                Console.WriteLine("发生了错误");
                Console.WriteLine(e);
                Console.Write("按Enter键退出");
                BlockUntilEnter();
            }
            finally
            {
                CleanUp();
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
