using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace VocieBot
{
    public class PieceManager
    {
        public async Task<MemoryStream> WriteFile(List<VoicePiece> pieceList, bool allUsers = false)
        {
            var fileName = $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}_{pieceList.First().User.Username}";
            List<byte> total = new List<byte>();
            foreach (var piece in pieceList)
            {
                piece.Data.ForEach(x => total.Add((x)));
            }

            if (allUsers)
            {
                fileName = $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}_Everyone";
            }

            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-f s16le -ar 48000 -i pipe:0 -ac 2 -map 0:a:0 -b:a 96k Output/{fileName}.mp3",

                RedirectStandardInput = true
            });

            await ffmpeg.StandardInput.BaseStream.WriteAsync(total.ToArray());

            return null;
        }
    }
}
