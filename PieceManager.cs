using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace DiscordBot
{
    public class PieceManager : IPieceManager
    {

        /// <summary>
        /// Writes the file.
        /// </summary>
        /// <param name="pieceList">The piece list.</param>
        /// <returns></returns>
        public async Task<(Stream, string)> WriteFile(List<VoicePiece> pieceList)
        {
            var fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_{pieceList.First().User?.Username}";
            var rawOutputFormat = ".wav";
            var convertedOutputFormat = ".mp3";
            var rawPath = $"Output/{fileName}{rawOutputFormat}";
            var convertedPath = $"Output/{fileName}{convertedOutputFormat}";

            List<byte> total = new List<byte>();
            foreach (var piece in pieceList)
            {
                piece.Data.ForEach(x => total.Add((x)));
            }

            WavConverter converter = new WavConverter();
            var wav = converter.PcmToWav(total.ToArray(), 2, 48000, 16);
            //await File.WriteAllBytesAsync(rawPath, wav);

            await FFMpegArguments.FromPipeInput(new StreamPipeSource(new MemoryStream(wav)))
                .OutputToFile(convertedPath, true,
                    options => options
                        .ForceFormat("mp3")
                        .WithCustomArgument("-ar 48000 -ac 2 -map 0:a:0 -b:a 48k")
                        .WithCustomArgument("-af asetrate=48000*0.5,aresample=48000"))
                .NotifyOnOutput(OnOutput)
                .NotifyOnError(OnOutput)
                .ProcessAsynchronously();

            
            await using var stream = File.OpenRead(convertedPath);
            
            return (stream, convertedPath);
        }


        /// <summary>
        /// Merges the PCM.
        /// </summary>
        /// <param name="userStreams">The user streams.</param>
        /// <returns></returns>
        public UserStream MergePCM(List<UserStream> userStreams)
        {

            userStreams = userStreams.OrderByDescending(x =>
            {
                using var file = File.OpenRead(x.FilePath);
                return file.Length;
            }).ToList();
            var ffmpeg = FFMpegArguments.FromFileInput(userStreams.First().FilePath);
            foreach (var userStream in userStreams.Skip(1))
            {
                ffmpeg.AddFileInput(userStream.FilePath);
            }

            ffmpeg.OutputToFile($"Output/{DateTime.Now:yyyy-MM-dd-HH-mm-ss}_Everyone.mp3", true, options => options
                    .ForceFormat("mp3")
                    .WithAudioBitrate(AudioQuality.Low)
                    .WithCustomArgument(
                    //$"-filter_complex amix=inputs={userStreams.Count}:duration=first:dropout_transition=0"))
                    $"-filter_complex amix=inputs={userStreams.Count}:duration=first"))
                .NotifyOnOutput(OnOutput)
                .NotifyOnError(OnOutput)
                .ProcessSynchronously();


            return null;
        }
        private void OnOutput(string obj)
        {
            Console.WriteLine(obj);
        }
    }
}
