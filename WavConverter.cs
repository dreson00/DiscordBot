using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VocieBot
{
    public class WavConverter
    {
        public WavConverter()
        {

        }

        public byte[] pcmToWav(byte[] pcmData, int numChannels, int sampleRate, int bitPerSample)
        {
            byte[] wavData = new byte[pcmData.Length + 44];
            byte[] header = wavHeader(pcmData.Length, numChannels, sampleRate, bitPerSample);
            header.CopyTo(wavData, 0);
            pcmData.CopyTo(wavData, header.Length);
            return wavData;
        }

        /**
         * @param pcmLen pcm
         * @param numChannels, mono = 1, stereo = 2
         * @param sampleRate
         * @param bitPerSample, 8bits
         * @return wav
         */
        public byte[] wavHeader(int pcmLen, int numChannels, int sampleRate, int bitPerSample)
        {
            byte[] header = new byte[44];
            // ChunkID, RIFF, 4bytes
            header[0] = (byte)'R';
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';
            // ChunkSize, pcmLen + 36, 4bytes
            long chunkSize = pcmLen + 36;
            header[4] = (byte)(chunkSize & 0xff);
            header[5] = (byte)((chunkSize >> 8) & 0xff);
            header[6] = (byte)((chunkSize >> 16) & 0xff);
            header[7] = (byte)((chunkSize >> 24) & 0xff);
            // Format, WAVE, 4bytes
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';
            // Subchunk1ID, 'fmt ', 4bytes
            header[12] = (byte)'f';
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';
            // Subchunk1Size, 16, 4bytes
            header[16] = 16;
            header[17] = 0;
            header[18] = 0;
            header[19] = 0;
            // AudioFormat, pcm = 1, 2bytes
            header[20] = 1;
            header[21] = 0;
            // NumChannels, mono = 1, stereo = 2, 2bytes
            header[22] = (byte)numChannels;
            header[23] = 0;
            // SampleRate, 4bytes
            header[24] = (byte)(sampleRate & 0xff);
            header[25] = (byte)((sampleRate >> 8) & 0xff);
            header[26] = (byte)((sampleRate >> 16) & 0xff);
            header[27] = (byte)((sampleRate >> 24) & 0xff);
            // ByteRate = SampleRate * NumChannels * BitsPerSample / 8, 4bytes
            long byteRate = sampleRate * numChannels * bitPerSample / 8;
            header[28] = (byte)(byteRate & 0xff);
            header[29] = (byte)((byteRate >> 8) & 0xff);
            header[30] = (byte)((byteRate >> 16) & 0xff);
            header[31] = (byte)((byteRate >> 24) & 0xff);
            // BlockAlign = NumChannels * BitsPerSample / 8, 2bytes
            header[32] = (byte)(numChannels * bitPerSample / 8);
            header[33] = 0;
            // BitsPerSample, 2bytes
            header[34] = (byte)bitPerSample;
            header[35] = 0;
            // Subhunk2ID, data, 4bytes
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';
            // Subchunk2Size, 4bytes
            header[40] = (byte)(pcmLen & 0xff);
            header[41] = (byte)((pcmLen >> 8) & 0xff);
            header[42] = (byte)((pcmLen >> 16) & 0xff);
            header[43] = (byte)((pcmLen >> 24) & 0xff);

            return header;
        }
    }
}
