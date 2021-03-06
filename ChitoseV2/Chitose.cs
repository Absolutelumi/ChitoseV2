﻿using ChitoseV2.Commands;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ChitoseV2
{
    internal class Chitose
    {
        public static readonly string GoogleApiKey = Properties.Settings.Default.GoogleApiKey;
        public static readonly string APIKey = Properties.Settings.Default.APIKey;
        public static readonly string ConfigDirectory = Properties.Settings.Default.ConfigDirectory;
        public static readonly string FfmpegPath = Properties.Settings.Default.FfmpegPath;
        public static readonly ServiceAccountCredential GoogleServiceAccount;
        public static readonly string ImgurKey = Properties.Settings.Default.ImgurKey;
        public static readonly string ImgurSecret = Properties.Settings.Default.ImgurSecret;
        public static readonly string MALPassword = Properties.Settings.Default.MALPassword;
        public static readonly string MALUsername = Properties.Settings.Default.MALUsername;
        public static readonly string NSFWPath = Properties.Settings.Default.NSFWPath;
        public static readonly string TempDirectory = Properties.Settings.Default.TempDirectory;
        private char prefix = '!';

        static Chitose()
        {
            var serviceAccount = JsonConvert.DeserializeObject<JsonCredentialParameters>(File.OpenRead(Properties.Settings.Default.ConfigDirectory + Properties.Settings.Default.GoogleServiceAccount).ReadString());
            GoogleServiceAccount = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(serviceAccount.ClientEmail)
            {
                Scopes =
                {
                }
            }.FromPrivateKey(serviceAccount.PrivateKey));
        }

        public Chitose()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            DiscordClient client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = (_, e) => Console.WriteLine(e.Message);
            });

            client.UsingCommands(input =>
            {
                input.PrefixChar = prefix;
                input.AllowMentionPrefix = true;
                input.HelpMode = HelpMode.Public;
            });
            client.UsingAudio(x => x.Mode = AudioMode.Outgoing);
            MusicModule music = new MusicModule(client.GetService<AudioService>());
            music.OnSongChanged += async (title) => await client.FindServers("Too Too Roo").FirstOrDefault().TextChannels.Where(channel => channel.Name == "music").FirstOrDefault().SendMessage("Now playing " + (title ?? "nothing"));
            List<ICommandSet> commandSets = new List<ICommandSet>()
            {
                new MusicCommands(music),
                new ChitosePictureResponse(),
                new GeneralCommands(),
                new Japanese(),
                new ServerUpdates(music),
                new MalCommands(),
                new Pictures(),
                new Osu_(),
                new Help(),
                new OsuScoreUpload()
            };

            string[] playing = { "with lolis～", "with hvick225", "csgo with snax", "life", "osu!", "killing myself", "circle smash", "kancolle" };

            client.ExecuteAndWait(async () =>
            {
                await client.Connect(new StreamReader(File.OpenRead(ConfigDirectory + "token.txt")).ReadToEnd(), TokenType.Bot);
                client.SetGame(playing.Random());
                commandSets.ForEach(set => set.AddCommands(client, client.GetService<CommandService>()));
            });
        }
    }
}