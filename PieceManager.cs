using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using FFmpeg.NET;

namespace VocieBot
{
    public class PieceManager
    {
        public async Task<(MemoryStream, string)> WriteFile(List<VoicePiece> pieceList)
        {
            var fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{pieceList.First().User?.Username}";
            //var arguments = $@"-f s16le -ar 48000 -thread_queue_size 1024 -i pipe:0 -ac 2 -map 0:a:0 -b:a 96k Output/{fileName}.mp3";

            List<byte> total = new List<byte>();
            foreach (var piece in pieceList)
            {
                piece.Data.ForEach(x => total.Add((x)));
            }

            //var stream = new MyStreamPipeSource(new MemoryStream(total.ToArray()));
            //var stream = new StreamPipeSource(new MemoryStream(total.ToArray()));
            var output = new MemoryStream();

            var ff = new FFmpeg.NET.Engine();
            var input = new StreamInput(new MemoryStream(total.ToArray()));
            var convertOptions = new ConversionOptions()
            {

            };
            //ff.ConvertAsync(input, )

            output.Seek(0, SeekOrigin.Begin);

            using (var fs = new FileStream($"Output/{fileName}.mp3", FileMode.OpenOrCreate))
            {
                output.CopyTo(fs);
            }


            //var ffmpeg = FFMpegArguments.FromPipeInput(new StreamPipeSource(stream))
            //    .OutputToFile($"Output", true,
            //        options => options
            //            //.ForceFormat("pcm_s16le")
            //            //.WithAudioCodec(AudioCodec.LibMp3Lame)
            //            .WithAudioBitrate(AudioQuality.Low))
            //            //.WithCustomArgument("map 0:a:0")
            //            //.WithCustomArgument("b:a 96k"))
            //            //.WithCustomArgument("-f s16le -ar 48000 -thread_queue_size 1024 -ac 2 -map 0:a:0 -b:a 96k"))
            //    .NotifyOnOutput(OnOutput)
            //    .NotifyOnError(OnOutput)
            //    .ProcessSynchronously();

            //var ffmpeg = Process.Start(new ProcessStartInfo
            //{
            //    FileName = "ffmpeg",
            //    Arguments = arguments,

            //    RedirectStandardInput = true
            //});

            //await ffmpeg.StandardInput.BaseStream.WriteAsync(total.ToArray());
            //ffmpeg.WaitForExit();

            return (null, fileName);
        }

        private void OnOutput(string obj)
        {
            Console.WriteLine(obj);
        }

        static void WriteToStream(Stream s, Byte[] bytes)
        {
            using (var writer = new BinaryWriter(s))
            {
                writer.Write(bytes);
            }
        }

        public (MemoryStream, string) GetMergedAudio(List<UserStream> streams)
        {
            var inputs = string.Empty;

            foreach (var userStream in streams)
            {
                inputs += inputs + $"-i Output/{userStream.FileName}.mp3 ";
            }

            var fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_Everyone";
            var arguments = $@"{inputs}-filter_complex amix=inputs={streams.Count}:duration=first:dropout_transition=3 {fileName}";

            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,

                RedirectStandardInput = true
            });

            ffmpeg?.Start();

            return (null, fileName);
        }
    }
}
