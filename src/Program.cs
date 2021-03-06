namespace Ch.Masora.MxpegRepair
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// DESCRIPTION:    Repair an MxPEG video recorded by Synology Surveillance Station
    ///                 and converting it to MP4/H264/AC3
    ///                 
    /// AUTHOR:         Nico Raschle
    /// COPYRIGHT:      (C) 2021 Masora AG
    /// </summary>
    class Program
    {
        private static string ffmpegFileName = "ffmpeg.exe";
        private static string resultMxgFileName = "result.mxg";
        private static string resultAc3FileName = "result.ac3";
        private static string resultMp4FileName = "result.mp4";
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please provide a file as parameter.");
                Console.Read();
                return;
            }

            if (!File.Exists(ffmpegFileName))
            {
                Console.WriteLine($"{ffmpegFileName} not found.");
                Console.Read();
                return;
            }

            var sourceFile = Path.GetFileName(args[0]);
            var videoBytes = File.ReadAllBytes(sourceFile);
            bool mxpegFrameActive = false;
            var frameCount = 0;
            using (var targetStream = File.Create(resultMxgFileName))
            {
                for (var i = 0; i < videoBytes.Length; i++)
                {
                    if (i + 4 < videoBytes.Length)
                    {
                        // JPEG SOI and APP0 marker
                        if (videoBytes[i] == 0xFF
                            && videoBytes[i + 1] == 0xD8
                            && videoBytes[i + 2] == 0xFF
                            && videoBytes[i + 3] == 0xE0)
                        {
                            mxpegFrameActive = true;
                            frameCount++;
                        }
                    }

                    if (mxpegFrameActive)
                    {
                        targetStream.WriteByte(videoBytes[i]);
                    }

                    if (i > 2)
                    {
                        // JPEG EOI marker
                        if (videoBytes[i - 1] == 0xFF
                            && videoBytes[i] == 0xD9)
                        {
                            mxpegFrameActive = false;
                        }
                    }
                }
            }
            Console.WriteLine($"MxPEG frames read: {frameCount}");

            // Convert repaired MxPEG video to MP4/H264
            var process = new Process
            {
                StartInfo =
                {
                    FileName = ffmpegFileName,
                    Arguments = $"-y -f mxg -i {resultMxgFileName} -max_muxing_queue_size 999999 {resultMp4FileName}",
                    WindowStyle = ProcessWindowStyle.Maximized
                }
            };
            process.Start();
            process.WaitForExit();

            // Convert source MxPEG audio to AC3
            process = new Process
            {
                StartInfo =
                {
                    FileName = ffmpegFileName,
                    Arguments = $"-y -i \"{sourceFile}\" -vn {resultAc3FileName}",
                    WindowStyle = ProcessWindowStyle.Maximized
                }
            };
            process.Start();
            process.WaitForExit();

            var fixedFileName =
                $"fixed.{Path.GetFileNameWithoutExtension(sourceFile)}.mp4";

            // Merge MP4 video and AC3 audio
            process = new Process
            {
                StartInfo =
                {
                    FileName = ffmpegFileName,
                    Arguments = $"-y -i {resultMp4FileName} -i {resultAc3FileName} -c:v copy -c:a copy \"{fixedFileName}\"",
                    WindowStyle = ProcessWindowStyle.Maximized
                }
            };
            process.Start();
            process.WaitForExit();

            File.Delete(resultAc3FileName);
            File.Delete(resultMp4FileName);
            File.Delete(resultMxgFileName);
        }
    }
}
