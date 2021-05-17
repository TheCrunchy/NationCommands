using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Gui;
using Sandbox.Game.Entities;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Managers;
using Torch.Managers.ChatManager;
using Torch.Managers.PatchManager;
using Torch.Session;

using VRage.Network;
using VRageMath;
using static Sandbox.Game.Multiplayer.MyFactionCollection;
using VRage.Game.ModAPI;
using Nexus.DataStructures;
using ServerNetwork.Sync;
using VRage.Game;
using System.Text.RegularExpressions;
using System.Globalization;

namespace NationsPlugin
{
    public class NationsPlugin : TorchPluginBase
    {


        public static MyIdentity GetIdentityByNameOrId(string playerNameOrSteamId)
        {
            foreach (var identity in MySession.Static.Players.GetAllIdentities())
            {
                if (identity.DisplayName == playerNameOrSteamId)
                    return identity;
                if (ulong.TryParse(playerNameOrSteamId, out ulong steamId))
                {
                    ulong id = MySession.Static.Players.TryGetSteamId(identity.IdentityId);
                    if (id == steamId)
                        return identity;
                    if (identity.IdentityId == (long)steamId)
                        return identity;
                }
            }
            return null;
        }
        public static Whitelist CONS;
        public static Whitelist FEDR;
        public static Whitelist UNIN;
        public static void SaveWhitelist(string file, Whitelist json)
        {
            if (!File.Exists(path + "//NationsWhitelists//" + file + ".json"))
            {
                //File.Create(path + "//NationsWhitelists//" + file + ".json");
            }
            FileUtils xml = new FileUtils();
            xml.WriteToJsonFile(path + "//NationsWhitelists//" + file + ".json", json);

        }
        public static Whitelist LoadWhitelist(String file)
        {

            if (!File.Exists(path + "//NationsWhitelists//" + file + ".json"))
            {
                // File.Create(path + "//NationsWhitelists//" + file + ".json");
                Log.Info("FUCK FUCK FUCK FUCK");
                Whitelist list2 = new Whitelist();
                list2.factions.Add(242354235235, "PLACEHOLDER");
                SaveWhitelist(file, list2);
                return list2;

            }
            FileUtils xml = new FileUtils();
            Whitelist list = xml.ReadFromJsonFile<Whitelist>(path + "//NationsWhitelists//" + file + ".json");
            return list;

        }
        public static List<long> playersInNationChat = new List<long>();

        public static MethodInfo GetOnlinePlayers;

        public static MethodInfo FriendRequests;
        public static Boolean friendReqs;

        public static bool SetupMethod()
        {
            var pluginManager = torchbase.CurrentSession.Managers.GetManager<PluginManager>();
            var pluginId = new Guid("28a12184-0422-43ba-a6e6-2e228611cca5");

            if (pluginManager.Plugins.TryGetValue(pluginId, out ITorchPlugin nexus))
            {

                try
                {
                    // MyPlayer player = MySession.Static.Players.GetPlayerByName("Crunch");
                    Type ReflectedServerSideAPI = nexus.GetType().Assembly.GetType("Nexus.API.NexusServerSideAPI");
                    GetOnlinePlayers = ReflectedServerSideAPI?.GetMethod("GetAllOnlinePlayersObject", BindingFlags.Public | BindingFlags.Static);



                    //SendMessage("CRUNCH", "1", Color.Blue, (long)player.Id.SteamId);
                    //List<object[]> ReturnPlayers = new List<object[]>();
                    //object[] MethodInput = new object[] { ReturnPlayers };
                    //SendMessage("CRUNCH", "2", Color.Blue, (long)player.Id.SteamId);
                    //GetOnlinePlayers?.Invoke(null, MethodInput);
                    //SendMessage("CRUNCH", "3", Color.Blue, (long)player.Id.SteamId);
                    ////After inputing the object[] you can simply call this to get your return variable (since this method is by ref)
                    //ReturnPlayers = (List<object[]>)MethodInput[0];
                    //SendMessage("CRUNCH", "4", Color.Blue, (long)player.Id.SteamId);
                    ////Here you can call either my function, or create your own to convert it into more 'usable' data
                    //List<Player> Players = new List<Player>();
                    //SendMessage("CRUNCH", "5", Color.Blue, (long)player.Id.SteamId);
                    //foreach (object[] obj in ReturnPlayers)
                    //{

                    //    Players.Add(new Player((string)obj[0], (ulong)obj[1], (long)obj[2], (int)obj[3]));
                    //}
                    //SendMessage("CRUNCH", "6", Color.Blue, (long)player.Id.SteamId);
                    //if (MySession.Static.Players.GetPlayerByName("Crunch") != null)
                    //{

                    //    SendMessage("CRUNCH", Players.Count.ToString(), Color.Blue, (long)player.Id.SteamId);
                    //    SendMessage("CRUNCH", ReturnPlayers.Count.ToString(), Color.Blue, (long)player.Id.SteamId);
                    //}
                    return true;
                }
                catch (Exception ex)
                {
                    if (MySession.Static.Players.GetPlayerByName("Crunch") != null)
                    {
                        MyPlayer player = MySession.Static.Players.GetPlayerByName("Crunch");
                        SendMessage("CRUNCH", ex.ToString(), Color.Blue, (long)player.Id.SteamId);
                    }
                    NationsPlugin.Log.Error(ex, "");
                    return false;
                }
            }
            else
            {
                if (MySession.Static.Players.GetPlayerByName("Crunch") != null)
                {
                    MyPlayer player = MySession.Static.Players.GetPlayerByName("Crunch");
                    SendMessage("CRUNCH", "SHIT NO WORK", Color.Blue, (long)player.Id.SteamId);
                }
            }
            return false;

        }

        public static ConfigFile file;
        public static Logger Log = LogManager.GetCurrentClassLogger();
        private static string path;
        public static Dictionary<long, CurrentCooldown> CurrentCooldownMap { get; } = new Dictionary<long, CurrentCooldown>();
        public static Dictionary<MyGps, DateTime> signalsToClear = new Dictionary<MyGps, DateTime>();
        private static Timer aTimer = new Timer();
        private static Timer bTimer = new Timer();
        private static Timer cTimer = new Timer();
        private TorchSessionManager sessionManager;
        public static ChatManagerServer _chatmanager;
        public static TorchSessionState TorchState;
        public static Boolean FUCKINGFUCKFUCK = false;
        private static ITorchBase torchbase;


       public static MethodInfo sendChange;

        public void SetupFriendMethod()
        {
            Type FactionCollection = MySession.Static.Factions.GetType().Assembly.GetType("Sandbox.Game.Multiplayer.MyFactionCollection");
           sendChange = FactionCollection?.GetMethod("SendFactionChange", BindingFlags.NonPublic | BindingFlags.Static);
        }
        public override void Init(ITorchBase torch)
        {
            torchbase = torch;
            base.Init(torch);
            Log.Info("Dirk smells");
            path = StoragePath;
            SetupConfig();
            aTimer.Enabled = false;
            aTimer.Interval = 30000;
            aTimer.Elapsed += OnTimedEventA;
            aTimer.AutoReset = true;
            var folder = Path.Combine(path + "//NationsWhitelists//");
            Directory.CreateDirectory(folder);
            //cTimer.Enabled = true;
            //cTimer.Enabled = false;
            //cTimer.Interval = 30000;
            //cTimer.Elapsed += OnTimedEventC;
            //cTimer.AutoReset = true;
            //cTimer.Enabled = true;
            if (file.OptionalAutomaticPeace)
            {
                bTimer.Enabled = false;
                bTimer.Interval = 30000;
                bTimer.Elapsed += OnTimedEventB;
                bTimer.AutoReset = true;
                bTimer.Enabled = true;
            }

            sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
            {
                sessionManager.SessionStateChanged += SessionChanged;
            }
        }
        public void test(IPlayer p)
        {
            if (p == null)
            {
                return;
            }
            MyIdentity id = GetIdentityByNameOrId(p.SteamId.ToString());
            if (id == null)
            {
                return;
            }
            MyFaction arrr = MySession.Static.Factions.TryGetFactionByTag("arrr");
            if (arrr != null)
            {
                if (FacUtils.GetPlayersFaction(id.IdentityId) != null && !MySession.Static.Factions.AreFactionsEnemies(arrr.FactionId, FacUtils.GetPlayersFaction(id.IdentityId).FactionId))
                {
                    Sandbox.Game.Multiplayer.MyFactionCollection.DeclareWar(FacUtils.GetPlayersFaction(id.IdentityId).FactionId, arrr.FactionId);
                }
            }

            MyFaction ACME = MySession.Static.Factions.TryGetFactionByTag("ACME");
            IMyFaction playerFac = MySession.Static.Factions.GetPlayerFaction(id.IdentityId);
            MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, ACME.FactionId, 0);
            MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, ACME.FactionId, 0);
            MyFaction UNIN = MySession.Static.Factions.TryGetFactionByTag("UNIN");
            MyFaction FEDR = MySession.Static.Factions.TryGetFactionByTag("FEDR");
            MyFaction CONS = MySession.Static.Factions.TryGetFactionByTag("CONS");
            if (UNIN != null)
            {
                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, UNIN.FactionId, 0);
                MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, UNIN.FactionId, 0);
            }
            if (CONS != null)
            {
                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, CONS.FactionId, 0);
                MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, CONS.FactionId, 0);
            }
            if (FEDR != null)
            {
                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, FEDR.FactionId, 0);
                MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, FEDR.FactionId, 0);
            }
            if (playerFac != null)
            {

                MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, ACME.FactionId, 0);

                try
                {
                    String nationtag = GetNationTag(playerFac);
                    MyFaction UNINAM = MySession.Static.Factions.TryGetFactionByTag("UNIN-AM");
                    MyFaction FEDRAM = MySession.Static.Factions.TryGetFactionByTag("FEDR-AM");
                    MyFaction CONSAM = MySession.Static.Factions.TryGetFactionByTag("CONS-AM");
                    if (UNINAM != null && FEDRAM != null && CONSAM != null)
                    {
                        if (nationtag != null)
                        {
                            if (nationtag.Equals("UNIN"))
                            {
                               MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                              {
                                  MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, CONSAM.FactionId, -3000);
                                  MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, FEDRAM.FactionId, -3000);
                                  MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, UNINAM.FactionId, 3000);
                                  MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, UNINAM.FactionId, 0);
                                  MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, CONSAM.FactionId, 0);
                                  MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, FEDRAM.FactionId, 0);
                                  //  MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, UNINAM.FactionId, 1500);
                                  //     MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, UNINAM.FactionId, 1500);
                                  //    MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, FEDRAM.FactionId, -1500);
                                  //   MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, CONSAM.FactionId, -1500);
                                  MyFactionCollection.DeclareWar(playerFac.FactionId, FEDRAM.FactionId);
                                    MyFactionCollection.DeclareWar(playerFac.FactionId, CONSAM.FactionId);
                              });
                                //    NationsPlugin.Log.Info("Player is union");
                                return;
                            }
                            if (nationtag.Equals("FEDR"))
                            {

                               MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                               {

                                   
                                   MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, CONSAM.FactionId, -3000);
                                   MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, FEDRAM.FactionId, 3000);
                                   MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, UNINAM.FactionId, -3000);
                                   MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, UNINAM.FactionId, 0);
                                   MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, CONSAM.FactionId, 0);
                                   MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, FEDRAM.FactionId, 0);
                                   MyFactionCollection.DeclareWar(playerFac.FactionId, UNINAM.FactionId);
                                    MyFactionCollection.DeclareWar(playerFac.FactionId, CONSAM.FactionId);
                                });
                                //     NationsPlugin.Log.Info("Player is federation");
                                return;
                            }
                            if (nationtag.Equals("CONS"))
                            {
                                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                               {
                                   MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, CONSAM.FactionId, 3000);
                                   MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, FEDRAM.FactionId, -3000);
                                   MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, UNINAM.FactionId, -3000);
                                   MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, UNINAM.FactionId, 0);
                                   MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, CONSAM.FactionId, 0);
                                   MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, FEDRAM.FactionId, 0);
                                   MyFactionCollection.DeclareWar(playerFac.FactionId, UNINAM.FactionId);
                                    MyFactionCollection.DeclareWar(playerFac.FactionId, FEDRAM.FactionId);
                                });
                                //     NationsPlugin.Log.Info("Player is consortium");
                                return;
                            }
                        }
                    }
                    else
                    {
                  //      NationsPlugin.Log.Info("No nation");
                        MyFactionCollection.DeclareWar(playerFac.FactionId, UNINAM.FactionId);
                        MyFactionCollection.DeclareWar(playerFac.FactionId, FEDRAM.FactionId);
                        MyFactionCollection.DeclareWar(playerFac.FactionId, CONSAM.FactionId);
                        MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, CONSAM.FactionId, -3000);
                        MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, FEDRAM.FactionId, -3000);
                        MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, UNINAM.FactionId, -3000);
                        MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, UNINAM.FactionId, 0);
                        MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, CONSAM.FactionId, 0);
                        MySession.Static.Factions.AddFactionPlayerReputation(id.IdentityId, FEDRAM.FactionId, 0);
                    }
                
                }
                catch (Exception ex)
            {
                NationsPlugin.Log.Error(ex);
                return;
            }
        }


    }

    public static string GetNationTag(IMyFaction fac)
    {
        if (fac.Description.Contains("UNIN"))
            return "UNIN";

        if (fac.Description.Contains("FEDR"))
            return "FEDR";

        if (fac.Description.Contains("CONS"))
            return "CONS";
        return null;

    }

    public static void CRUNCH()
    {
        _chatmanager.SendMessageAsOther("Crunch", "ignore this");
    }
    private void SessionChanged(ITorchSession session, TorchSessionState state)
    {

        if (state == TorchSessionState.Loaded)
        {

            CONS = LoadWhitelist("CONS");
            FEDR = LoadWhitelist("FEDR");
            UNIN = LoadWhitelist("UNIN");
            // SaveWhitelist("CONS", CONS);
            //  SaveWhitelist("FEDR", FEDR);
            // SaveWhitelist("UNIN", UNIN);
            TorchState = TorchSessionState.Loaded;
            SetupMethod();
                SetupFriendMethod();
            /// SetupFriendRequests();
            TorchChatMessage message1 = new TorchChatMessage();
            _chatmanager = Torch.CurrentSession.Managers.GetManager<ChatManagerServer>();

            if (_chatmanager == null)
            {
                Log.Warn("No chat manager loaded!");
            }
            else
            {
                _chatmanager.MessageProcessing += MessageRecieved;
                session.Managers.GetManager<IMultiplayerManagerBase>().PlayerJoined += test;
            }
        }




    }


    public static ConfigFile LoadConfig()
    {
        FileUtils utils = new FileUtils();
        file = utils.ReadFromXmlFile<ConfigFile>(path + "\\NationsConfig.xml");
        if (file.OptionalAutomaticPeace)
        {
            bTimer.Enabled = false;
            bTimer.Interval = 30000;
            bTimer.Elapsed += OnTimedEventB;
            bTimer.AutoReset = true;
            bTimer.Enabled = true;
        }
        else
        {
            bTimer.Enabled = false;
        }
        return file;
    }

    public static void doStuff(MyFaction fac, String nation)
    {
        Boolean excluding = false;
        List<String> exclusions = new List<String>();
        if (fac.PrivateInfo.ToLower().Contains("exclude["))
        {
            excluding = true;

            String exclusionBeforeFormat = Commands.GetStringBetweenCharacters(fac.PrivateInfo, '[', ']');
            if (exclusionBeforeFormat.Contains(","))
            {
                String[] addToExclusions = exclusionBeforeFormat.Split(',');
                foreach (String s in addToExclusions)
                {
                    exclusions.Add(s.ToLower());
                }
            }

            else
            {
                exclusions.Add(exclusionBeforeFormat.ToLower());
            }


        }


        foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
        {
            if (f.Value != fac && f.Value != null)
            {
                if (f.Value.Description != null && f.Value.Description.Contains(nation))
                {

                    MyFactionPeaceRequestState state = MySession.Static.Factions.GetRequestState(fac.FactionId, f.Value.FactionId);

                    if (excluding)
                    {
                        if (!exclusions.Contains(f.Value.Tag.ToLower()))
                        {
                            if (state != MyFactionPeaceRequestState.Sent)
                            {
                                Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(fac.FactionId, f.Value.FactionId);
                                //   Log.Info("NATION REQUESTS - Sending peace reqest between " + fac.Name + " " + fac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);
                            }
                            if (state == MyFactionPeaceRequestState.Pending)
                            {
                                Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(fac.FactionId, f.Value.FactionId);
                                //   Log.Info("NATION REQUESTS - Accepting peace reqest between " + fac.Name + " " + fac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);
                                MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, f.Value.FactionId, 1500);
                            }
                            if (MySession.Static.Factions.AreFactionsNeutrals(fac.FactionId, f.Value.FactionId))
                            {
                                MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, f.Value.FactionId, 1500);
                            }
                        }
                    }
                    else
                    {
                        if (state != MyFactionPeaceRequestState.Sent)
                        {
                            Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(fac.FactionId, f.Value.FactionId);
                            //    Log.Info("NATION REQUESTS - Sending peace reqest between " + fac.Name + " " + fac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);

                        }
                        if (state == MyFactionPeaceRequestState.Pending)
                        {
                            Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(fac.FactionId, f.Value.FactionId);
                            //   Log.Info("NATION REQUESTS - Accepting peace reqest between " + fac.Name + " " + fac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);
                            MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, f.Value.FactionId, 1500);
                        }
                        if (MySession.Static.Factions.AreFactionsNeutrals(fac.FactionId, f.Value.FactionId))
                        {
                            MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, f.Value.FactionId, 1500);
                        }
                    }
                }
                else
                {
                    if (f.Value.Tag.Length == 3)
                    {
                        //if you ever want this
                        //Sandbox.Game.Multiplayer.MyFactionCollection.DeclareWar(playerFac.FactionId, f.Value.FactionId);
                    }
                }

            }
        }
    }



    public static void SendMessage(string author, string message, Color color, long steamID)
    {


        Logger _chatLog = LogManager.GetLogger("Chat");
        ScriptedChatMsg scriptedChatMsg1 = new ScriptedChatMsg();
        scriptedChatMsg1.Author = author;
        scriptedChatMsg1.Text = message;
        scriptedChatMsg1.Font = "White";
        scriptedChatMsg1.Color = color;
        scriptedChatMsg1.Target = Sync.Players.TryGetIdentityId((ulong)steamID);
        ScriptedChatMsg scriptedChatMsg2 = scriptedChatMsg1;
        MyMultiplayerBase.SendScriptedChatMessage(ref scriptedChatMsg2);
    }

    public static bool bob = false;
    private void MessageRecieved(TorchChatMessage msg, ref bool consumed)
    {

        if (playersInNationChat.Contains((long)msg.AuthorSteamId))
        {
            if (!msg.Message.StartsWith("!"))
            {


                String nation = "";
                IMyFaction playerFac = FacUtils.GetPlayersFaction(GetIdentityByNameOrId(msg.Author.ToString()).IdentityId);

                String text = msg.Message;


                Commands.SendMessage(msg.Author, "You are in nation chat", Color.Yellow, (long)msg.AuthorSteamId);

                if (playerFac != null)
                {
                    int tagsInDescription = 0;
                    if (playerFac.Description.Contains("UNIN"))
                    {
                        nation = "UNIN";
                        tagsInDescription++;
                    }
                    if (playerFac.Description.Contains("FEDR"))
                    {
                        nation = "FEDR";
                        tagsInDescription++;
                    }
                    if (playerFac.Description.Contains("CONS"))
                    {
                        nation = "CONS";
                        tagsInDescription++;
                    }
                    if (NationsPlugin.file.doWhitelist)
                    {
                        switch (nation.ToUpper())
                        {
                            case "FEDR":
                                if (!NationsPlugin.FEDR.factions.ContainsKey(playerFac.FactionId))
                                {

                                    return;
                                }
                                break;
                            case "CONS":
                                if (!NationsPlugin.CONS.factions.ContainsKey(playerFac.FactionId))
                                {

                                    return;
                                }
                                break;
                            case "UNIN":
                                if (!NationsPlugin.UNIN.factions.ContainsKey(playerFac.FactionId))
                                {

                                    return;
                                }
                                break;
                        }
                    }
                    if (tagsInDescription == 1)
                    {

                        IMyFaction nationFac = MySession.Static.Factions.TryGetFactionByTag(nation);
                        if (nationFac != null) //&& nationFac.Description.Contains("[" + playerFac.Tag.ToUpper() + "]"))
                        {
                            foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
                            {
                                if (p.Id.SteamId != msg.AuthorSteamId)
                                {
                                    if (FacUtils.GetPlayersFaction(p.Identity.IdentityId) != null && FacUtils.GetPlayersFaction(p.Identity.IdentityId).Description != null && FacUtils.GetPlayersFaction(p.Identity.IdentityId).Description.Contains(nation))
                                    {
                                        IMyFaction fac2 = FacUtils.GetPlayersFaction(p.Identity.IdentityId);
                                        int tagsInDescription2 = 0;
                                        if (playerFac.Description.Contains("UNIN"))
                                        {
                                            nation = "UNIN";
                                            tagsInDescription2++;
                                        }
                                        if (playerFac.Description.Contains("FEDR"))
                                        {
                                            nation = "FEDR";
                                            tagsInDescription2++;
                                        }
                                        if (playerFac.Description.Contains("CONS"))
                                        {
                                            nation = "CONS";
                                            tagsInDescription2++;
                                        }
                                        if (tagsInDescription2 == 1)
                                        {
                                            if (p.Id.SteamId > 0)
                                            {
                                                Commands.SendMessage(msg.Author, text, Color.HotPink, (long)p.Id.SteamId);
                                                MyGpsCollection gpscol = (MyGpsCollection)MyAPIGateway.Session?.GPS;

                                                if (ScanChat(text, null) != null)
                                                {
                                                    MyGps gpsRef = ScanChat(text, null);
                                                    gpsRef.GPSColor = Color.Yellow;
                                                    gpsRef.AlwaysVisible = true;
                                                    gpsRef.ShowOnHud = true;

                                                    gpscol.SendAddGps(p.Identity.IdentityId, ref gpsRef);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }

            consumed = true;
        }
    }




    public static MyGps ScanChat(string input, string desc = null)
    {

        int num = 0;
        bool flag = true;
        MatchCollection matchCollection = Regex.Matches(input, "GPS:([^:]{0,32}):([\\d\\.-]*):([\\d\\.-]*):([\\d\\.-]*):");

        Color color = new Color(117, 201, 241);
        foreach (Match match in matchCollection)
        {
            string str = match.Groups[1].Value;
            double x;
            double y;
            double z;
            try
            {
                x = Math.Round(double.Parse(match.Groups[2].Value, (IFormatProvider)CultureInfo.InvariantCulture), 2);
                y = Math.Round(double.Parse(match.Groups[3].Value, (IFormatProvider)CultureInfo.InvariantCulture), 2);
                z = Math.Round(double.Parse(match.Groups[4].Value, (IFormatProvider)CultureInfo.InvariantCulture), 2);
                if (flag)
                    color = (Color)new ColorDefinitionRGBA(match.Groups[5].Value);
            }
            catch (SystemException ex)
            {
                continue;
            }
            MyGps gps = new MyGps()
            {
                Name = str,
                Description = desc,
                Coords = new Vector3D(x, y, z),
                GPSColor = color,
                ShowOnHud = false
            };
            gps.UpdateHash();

            return gps;
        }
        return null;
    }
    private static MyGps CreateGps(MyCubeGrid grid, Vector3D Position, Color gpsColor, int seconds, String Display)
    {

        MyGps gps = new MyGps
        {
            Coords = Position,
            Name = "Transponder " + Display,
            DisplayName = "Transponder " + Display,
            GPSColor = gpsColor,
            IsContainerGPS = true,
            ShowOnHud = true,
            DiscardAt = new TimeSpan(0, 0, 0, seconds),
            Description = "Transponder",
        };
        gps.UpdateHash();
        gps.SetEntityId(grid.EntityId);

        return gps;
    }

    private static void OnTimedEventB(Object source, System.Timers.ElapsedEventArgs e)
    {

        if (TorchState != TorchSessionState.Loaded)
        {

            return;
        }
        if (MySession.Static.Players.GetPlayerByName("Crunch") != null && FUCKINGFUCKFUCK)
        {
            MyPlayer player = MySession.Static.Players.GetPlayerByName("Crunch");
            SendMessage("CRUNCH", "The fucking task is running?", Color.Blue, (long)player.Id.SteamId);
        }
        MyFaction ACME = MySession.Static.Factions.TryGetFactionByTag("ACME");
        foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
        {
            System.Tuple<MyRelationsBetweenFactions, int> rep = MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(p.Identity.IdentityId, ACME.FactionId);
            if (rep.Item2 < 0)
            {
                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(p.Identity.IdentityId, ACME.FactionId, Math.Abs(rep.Item2));
            }
        }
        int facCount = 0;
        //   Task.Run(() =>
        //   {
        //       List<MyFaction> addThese = new List<MyFaction>();

        //       foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
        //       {

        //           if (f.Value != null)
        //           {
        //               facCount += 1;
        //               if (f.Value.PrivateInfo == null)
        //               {

        //                   addThese.Add(f.Value);

        //               }
        //               else
        //               {
        //                   if (!f.Value.PrivateInfo.ToLower().Contains("autonationpeace=true") && !f.Value.PrivateInfo.ToLower().Contains("autonationpeace=false"))
        //                   {
        //                       addThese.Add(f.Value);


        //                   }
        //               }
        //               if (f.Value.Description != null)
        //               {
        //                   int tagsInDescription = 0;
        //                   if (f.Value.Description.Contains("UNIN"))
        //                   {
        //                       tagsInDescription++;
        //                   }
        //                   if (f.Value.Description.Contains("FEDR"))
        //                   {
        //                       tagsInDescription++;
        //                   }
        //                   if (f.Value.Description.Contains("CONS"))
        //                   {
        //                       tagsInDescription++;
        //                   }
        //                   if (tagsInDescription > 1)
        //                   {

        //                       NationsPlugin.Log.Info("NATION - This guys trying to do !nationjoin with multiple tags " + f.Value.Name + " " + f.Value.Tag);


        //                   }
        //                   else
        //                   {

        //                       if (f.Value.Description != null && f.Value.Description.Contains("UNIN") && f.Value.PrivateInfo != null && f.Value.PrivateInfo.ToLower().Contains("autonationpeace=true"))
        //                       {


        //                           if (file.doWhitelist){

        //                           }
        //                           else
        //                           {
        //                               doStuff(f.Value, "UNIN");
        //                           }
        //                       }

        //                       if (f.Value.Description != null && f.Value.Description.Contains("CONS") && f.Value.PrivateInfo != null && f.Value.PrivateInfo.ToLower().Contains("autonationpeace=true"))
        //                       {

        //                           doStuff(f.Value, "CONS");

        //                       }
        //                       if (f.Value.Description != null && f.Value.Description.Contains("FEDR") && f.Value.PrivateInfo != null && f.Value.PrivateInfo.ToLower().Contains("autonationpeace=true"))
        //                       {

        //                           doStuff(f.Value, "FEDR");

        //                       }
        //                   }
        //               }
        //           }
        //       }
        //foreach (MyFaction f in addThese)
        //       {
        //           String current = f.PrivateInfo;

        //           f.PrivateInfo = "autonationpeace=false\nexclude[]\nTo exclude put tags in the bracket, for example [ITC,ADY]\n" + current;
        //           VRage.ObjectBuilders.SerializableDefinitionId bob = new VRage.ObjectBuilders.SerializableDefinitionId();
        //          // f.FactionIcon.ser
        //         // MySession.Static.Factions.EditFaction(f.FactionId, f.Tag, f.Name, f.Description, f.PrivateInfo, null, f.FactionIcon.Value.Id, f.CustomColor, f.IconColor);
        //       }
        //   });
        //   if (MySession.Static.Players.GetPlayerByName("Crunch") != null && FUCKINGFUCKFUCK)
        //   {
        //       MyPlayer player = MySession.Static.Players.GetPlayerByName("Crunch");
        //       SendMessage("CRUNCH", facCount.ToString(), Color.Blue, (long)player.Id.SteamId);
        //   }

    }
    private static void OnTimedEventA(Object source, System.Timers.ElapsedEventArgs e)
    {
        Task.Run(() =>
        {
            foreach (KeyValuePair<MyGps, DateTime> d in signalsToClear)
            {
                if (d.Value < DateTime.Now)
                {
                    foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
                    {

                        MyAPIGateway.Session?.GPS.RemoveGps(p.Identity.IdentityId, d.Key);

                    }
                }
            }
        });
    }
    public static ConfigFile SaveConfig()
    {
        FileUtils utils = new FileUtils();
        utils.WriteToXmlFile<ConfigFile>(path + "\\NationsConfig.xml", file);

        return file;
    }
    public long Cooldown { get { return file.CooldownMilliseconds; } }
    private void SetupConfig()
    {
        FileUtils utils = new FileUtils();
        path = StoragePath;
        if (File.Exists(StoragePath + "\\NationsConfig.xml"))
        {
            file = utils.ReadFromXmlFile<ConfigFile>(StoragePath + "\\NationsConfig.xml");
            utils.WriteToXmlFile<ConfigFile>(StoragePath + "\\NationsConfig.xml", file, false);
        }
        else
        {
            file = new ConfigFile();
            utils.WriteToXmlFile<ConfigFile>(StoragePath + "\\NationsConfig.xml", file, false);
        }

    }

}
}
