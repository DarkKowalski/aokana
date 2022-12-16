using System;
using System.IO;
using System.Diagnostics;

namespace Aokana
{
    class CommandLine
    {
        struct Options
        {
            public String InputPath;
            public String OutputPath;
            public GameDlcVersion Version;

            public Options(string[] args)
            {
                this.Version = GameDlcVersion.NoDlc;
                this.InputPath = "";
                this.OutputPath = "";

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-i")
                    {
                        this.InputPath = Path.GetFullPath(args[i + 1]);
                    }
                    else if (args[i] == "-o")
                    {
                        this.OutputPath = Path.GetFullPath(args[i + 1]);
                    }
                    else if (args[i] == "--extra1")
                    {
                        if (this.Version != GameDlcVersion.NoDlc)
                        {
                            Console.WriteLine("Cannot specify more than one DLC version");
                            return;
                        }
                        this.Version = GameDlcVersion.Extra1;

                    }
                    else if (args[i] == "--extra2")
                    {
                        if (this.Version != GameDlcVersion.NoDlc)
                        {
                            Console.WriteLine("Cannot specify more than one DLC version");
                            return;
                        }
                        this.Version = GameDlcVersion.Extra2;
                    }
                }
            }

            public bool IsValid()
            {

                if (!File.Exists(this.InputPath) && !Directory.Exists(this.InputPath))
                {
                    Console.WriteLine("Input file {0} does not exist", this.InputPath);
                    return false;
                }

                if (Directory.Exists(this.OutputPath))
                {
                    int fileNum = Directory.GetFiles(this.OutputPath, "*", SearchOption.AllDirectories).Length;
                    if (fileNum > 0)
                    {
                        Console.WriteLine("Output directory {0} already exists and is not empty", this.OutputPath);
                        return false;
                    }
                }

                return true;
            }
        }

        static void Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("Notice: This is a debug build");
#endif

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: aokana.exe -i <inputFile|inputDir> -o <outputDir> [--extra1|--extra2]");
                return;
            }

            Options options = new Options(args);
            if (!options.IsValid())
            {
                return;
            }

            IExtractor extractor = ExtractorFactory.GetExtractor(options.Version);

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            int totalFiles = 0;
            long totalExtractedFiles = 0;

            // If the input is a directory, extract all .dat files in it
            if (Directory.Exists(options.InputPath))
            {
                string[] files = Directory.GetFiles(options.InputPath, "*.dat");
                foreach (string file in files)
                {
                    totalFiles += 1;
                    totalExtractedFiles += extractor.ExtractAll(file, options.OutputPath);
                }
            }
            else
            {
                totalFiles += 1;
                totalExtractedFiles += extractor.ExtractAll(options.InputPath, options.OutputPath);
            }

            stopwatch.Stop();
            Console.WriteLine("Extracted {0} files from {1} dat files, finished in {2} seconds", totalExtractedFiles, totalFiles, stopwatch.Elapsed.TotalSeconds);
        }
    }
}