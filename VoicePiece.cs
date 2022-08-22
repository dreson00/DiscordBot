﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace VocieBot
{
    public class VoicePiece
    {
        public DateTime Time { get; }
        public List<byte> Data { get; }
        public DSharpPlus.Entities.DiscordUser? User { get; }

        public VoicePiece(DateTime time, byte[] data, DSharpPlus.Entities.DiscordUser? user)
        {
            Time = time;
            Data = data.ToList();
            User = user;
        }
    }
}
