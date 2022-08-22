using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading.Channels;
using DSharpPlus.VoiceNext.EventArgs;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.IO;
using System.Net;
using DSharpPlus.CommandsNext.Converters;

namespace VocieBot
{
    public class VoiceChat
    {
        private bool _connected;
        private System.Timers.Timer aTimer;
        private VoiceNextConnection _connection;

        private DiscordClient _discord;
        private readonly PieceManager _pieceManager;
        private DiscordGuild _kanela;
        private DiscordChannel _channel;

        private static List<VoicePiece> _pieceList;


        public VoiceChat(DiscordClient discord, PieceManager pieceManager)
        {
            _discord = discord;
            _pieceManager = pieceManager;
            _connected = false;
            _pieceList = new List<VoicePiece>();

            _discord.MessageCreated += _discord_MessageCreated;
        }



        private async Task _discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            _discord.MessageCreated -= _discord_MessageCreated;
            _connection.VoiceReceived -= VoiceReceiveHandler;
            if (e.Message.Content.ToLower().StartsWith("download"))
            {
                var mentions = e.Message.MentionedUsers.ToList();

                var userFilteredpieceLists = _pieceList.Where(piece => piece.User is not null)
                    .GroupBy(x => x.User);
                var streams = new List<UserStream>();
                if (mentions.Count == 0)
                {
                    streams.Add(new UserStream()
                    {
                        Userame = "Everyone",
                        Stream = await _pieceManager.WriteFile(_pieceList.ToList(), allUsers: true)
                    });
                }
                else
                {
                    foreach (var userFilteredpieceList in userFilteredpieceLists.Where(list => e.MentionedUsers.ToList().Contains(list.Key)))
                    {
                        streams.Add(new UserStream()
                        {
                            Userame = userFilteredpieceList.Key.Username,
                            Stream = await _pieceManager.WriteFile(userFilteredpieceList.ToList())
                        });
                    }
                    streams.Add(new UserStream()
                    {
                        Userame = "Everyone",
                        Stream = await _pieceManager.WriteFile(_pieceList.ToList(), allUsers: true)
                    });
                }






                //var msg = await new DiscordMessageBuilder()
                //    .WithFiles(new Dictionary<string, Stream>() { { "nejakynormalnijmeno.wav", stream } })
                //    .SendAsync(e.Channel);
            }
            _discord.MessageCreated += _discord_MessageCreated;
            _connection.VoiceReceived += VoiceReceiveHandler;
        }

        public async Task StartVoiceChatCheck()
        {
            _kanela = await _discord.GetGuildAsync(288667412549730304);
            _channel = await _discord.GetChannelAsync(288683278435614720);

            _discord.UseVoiceNext(new VoiceNextConfiguration()
            {
                EnableIncoming = true
            });

            SetTimer();
        }

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (_channel.Users.Count > 0 && !_connected)
            {
                _connection = await _channel.ConnectAsync();
                _connected = true;

                if (!Directory.Exists("Output"))
                {
                    Directory.CreateDirectory("Output");
                }

                _connection.VoiceReceived += VoiceReceiveHandler;
            }
            else if (_channel.Users.Count == 1 && _connected)
            {
                _connection.VoiceReceived -= VoiceReceiveHandler;
                _connection.Disconnect();
                _connected = false;
            }
        }

        private async Task VoiceReceiveHandler(VoiceNextConnection connection, VoiceReceiveEventArgs args)
        {
            _pieceList.Add(new VoicePiece(DateTime.Now, args.PcmData.ToArray(), args.User));
            //Console.WriteLine($"[ {DateTime.Now} ] {args.User}: Time {args.AudioDuration}");

            _pieceList.RemoveAll(x => DateTime.Now.Subtract(x.Time) > TimeSpan.FromMinutes(30));
        }
    }

    
}
