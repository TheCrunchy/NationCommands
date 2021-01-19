using NLog;
using Sandbox.Engine.Multiplayer;
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
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Network;
using VRageMath;
using static Sandbox.Game.Multiplayer.MyFactionCollection;

namespace NationsPlugin
{
    public class NationsPlugin : TorchPluginBase
    {

        [PatchShim]
        public static class MyChatPatch
        {

            internal static readonly MethodInfo update =
                typeof(ChatManagerClient).GetMethod("Multiplayer_ChatMessageReceived", BindingFlags.Instance | BindingFlags.NonPublic) ??
                throw new Exception("Failed to find patch method");

            internal static readonly MethodInfo updatePatch =
                typeof(MyChatPatch).GetMethod(nameof(TestPatchMethod), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");
            public static void Patch(PatchContext ctx)
            {

                ctx.GetPattern(update).Suffixes.Add(updatePatch);

                Log.Info("Patching Successful MyLargeTurretBase!");
            }
            public static bool TestPatchMethod(ulong steamUserId,
      string messageText,
      Sandbox.Game.Gui.ChatChannel channel,
      long targetId,
      string customAuthorName)
            {
                return true;
            }
        }
            //{

            //    if (msg.Text.StartsWith("/n"))
            //    {
            //        String messageText = msg.Text;


            //        if (playersInNationChat.Contains(msg.Author))
            //        {
            //            Commands.SendMessage(MyMultiplayer.Static.GetMemberName(msg.Author), "Toggled off, use /n to toggle", Color.Yellow, (long)msg.Author);
            //            playersInNationChat.Remove(msg.Author);
            //            return false;
            //        }
            //        else
            //        {
            //            Commands.SendMessage(MyMultiplayer.Static.GetMemberName(msg.Author), "Toggled on, use /n to toggle", Color.Yellow, (long)msg.Author);
            //            playersInNationChat.Add(msg.Author);
            //            return false;
            //        }

            //    }
            //    if (msg.Text.StartsWith("/g"))
            //    {
            //        if (playersInNationChat.Contains(msg.Author))
            //        {
            //            playersInNationChat.Remove(msg.Author);
            //            return false;
            //        }
            //    }
            //    if (msg.Text.StartsWith("/f"))
            //    {
            //        if (playersInNationChat.Contains(msg.Author))
            //        {
            //            playersInNationChat.Remove(msg.Author);
            //            return false;
            //        }
            //    }
            //    if (playersInNationChat.Contains(msg.Author))
            //    {
            //        String nation = "";
            //        IMyFaction playerFac = FacUtils.GetPlayersFaction(GetIdentityByNameOrId(msg.Author.ToString()).IdentityId);

            //        msg.Text = "";
            //        msg.TargetId = 24356346457465746;
            //        msg.CustomAuthorName = "";


            //        if (playerFac != null)
            //        {
            //            int tagsInDescription = 0;
            //            if (playerFac.Description.Contains("UNIN"))
            //            {
            //                nation = "UNIN";
            //                tagsInDescription++;
            //            }
            //            if (playerFac.Description.Contains("FEDR"))
            //            {
            //                nation = "FEDR";
            //                tagsInDescription++;
            //            }
            //            if (playerFac.Description.Contains("CONS"))
            //            {
            //                nation = "CONS";
            //                tagsInDescription++;
            //            }
            //            if (tagsInDescription == 1)
            //            {

            //                IMyFaction nationFac = MySession.Static.Factions.TryGetFactionByTag(nation);
            //                if (nationFac != null && nationFac.Description.Contains("[" + playerFac.Tag.ToUpper() + "]"))
            //                {
            //                    foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
            //                    {
            //                        if (p.Id.SteamId != msg.Author)
            //                        {//
            //                            if (FacUtils.GetPlayersFaction(p.Identity.IdentityId) != null && FacUtils.GetPlayersFaction(p.Identity.IdentityId).Description.Contains(nation))
            //                            {
            //                                IMyFaction fac2 = FacUtils.GetPlayersFaction(p.Identity.IdentityId);
            //                                int tagsInDescription2 = 0;
            //                                if (playerFac.Description.Contains("UNIN"))
            //                                {
            //                                    nation = "UNIN";
            //                                    tagsInDescription2++;
            //                                }
            //                                if (playerFac.Description.Contains("FEDR"))
            //                                {
            //                                    nation = "FEDR";
            //                                    tagsInDescription2++;
            //                                }
            //                                if (playerFac.Description.Contains("CONS"))
            //                                {
            //                                    nation = "CONS";
            //                                    tagsInDescription2++;
            //                                }
            //                                if (tagsInDescription2 == 1)
            //                                {
            //                                    if (MySession.Static.Players.TryGetSteamId(p.Identity.IdentityId) > 0)
            //                                    {
            //                                        Commands.SendMessage(MyMultiplayer.Static.GetMemberName(msg.Author), msg.Text, Color.Yellow, (long)p.Id.SteamId);
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }

            //            }
            //        }
            //        return false;
            //    }
            //    return true;
           
          //  }
      //  }


        //[PatchShim]
        //internal static class ChatInterceptPatch
        //{

        //    public static event ChatReceivedDel OnChatRecvAccess;
        //    // private static 
        //    public static ChatManagerServer ChatManager;
        //    internal static void Patch(PatchContext context)
        //    {
        //        var target = typeof(MyMultiplayerBase).GetMethod("OnChatMessageReceived_Server", BindingFlags.Static | BindingFlags.NonPublic);
        //        var patchMethod = typeof(ChatInterceptPatch).GetMethod(nameof(PrefixMessageProcessing), BindingFlags.Static | BindingFlags.NonPublic);
        //        context.GetPattern(target).Prefixes.Add(patchMethod);
        //    }

        //    private static bool PrefixMessageProcessing(ref ChatMsg msg)
        //    {
        //        if (msg.Author == 0)
        //        {
        //            return false;
        //        }
        //        if (msg.Text.StartsWith("/n"))
        //        {
        //            String messageText = msg.Text;

        //            Log.Info("NATION MESSAGE");
        //            if (playersInNationChat.Contains(msg.Author))
        //            {

        //                Commands.SendMessage("Nation Chat: ", "Toggled off", Color.Yellow, (long)msg.Author);
        //                playersInNationChat.Remove(msg.Author);

        //             //   NationsPlugin._chatmanager.UnmuteUser(msg.Author);

        //                return false;
        //            }
        //            else
        //            {
        //                //  ChatManagerServer ChatManager  = torchbase.CurrentSession.Managers.GetManager<ChatManagerServer>();
        //                playersInNationChat.Add(msg.Author);
        //                Commands.SendMessage("Nation Chat: ", "Toggled on", Color.Yellow, (long)msg.Author);
        //             //   NationsPlugin._chatmanager.MuteUser(msg.Author);
        //                return false;
        //            }

        //        }
        //        if (msg.Text.StartsWith("/g"))
        //        {
        //            if (playersInNationChat.Contains(msg.Author))
        //            {
        //                playersInNationChat.Remove(msg.Author);
        //                Commands.SendMessage("Nation Chat ", "Toggled off", Color.Yellow, (long)msg.Author);
        //                return false;
        //            }
        //        }
        //        if (msg.Text.StartsWith("/f"))
        //        {
        //            if (playersInNationChat.Contains(msg.Author))
        //            {
        //                playersInNationChat.Remove(msg.Author);
        //                Commands.SendMessage("Nation Chat ", "Toggled off", Color.Yellow, (long)msg.Author);
        //                return false;
        //            }
        //        }
        //        if (playersInNationChat.Contains(msg.Author))
        //        {
        //            String nation = "";

        //            IMyFaction playerFac = FacUtils.GetPlayersFaction(GetIdentityByNameOrId(msg.Author.ToString()).IdentityId);
        //            Log.Info("NATION MESSAGE");
        //            if (playerFac != null)
        //            {
        //                int tagsInDescription = 0;
        //                if (playerFac.Description.Contains("UNIN"))
        //                {
        //                    nation = "UNIN";
        //                    tagsInDescription++;
        //                }
        //                if (playerFac.Description.Contains("FEDR"))
        //                {
        //                    nation = "FEDR";
        //                    tagsInDescription++;
        //                }
        //                if (playerFac.Description.Contains("CONS"))
        //                {
        //                    nation = "CONS";
        //                    tagsInDescription++;
        //                }
        //                if (tagsInDescription == 1)
        //                {

        //                    IMyFaction nationFac = MySession.Static.Factions.TryGetFactionByTag(nation);

        //                    if (nationFac != null && nationFac.Description.Contains("[" + playerFac.Tag.ToUpper() + "]"))
        //                    {
        //                        foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
        //                        {
        //                               if (p.Id.SteamId != msg.Author)
        //                                {
        //                            if (FacUtils.GetPlayersFaction(p.Identity.IdentityId) != null && FacUtils.GetPlayersFaction(p.Identity.IdentityId).Description.Contains(nation))
        //                            {
        //                                IMyFaction fac2 = FacUtils.GetPlayersFaction(p.Identity.IdentityId);

        //                                if (nationFac.Description.Contains("[" + fac2.Tag.ToUpper() + "]"))
        //                                {

        //                                    Commands.SendMessage(MyMultiplayer.Static.GetMemberName(msg.Author), msg.Text, Color.Yellow, (long)p.Id.SteamId);

        //                                }
        //                            }
        //                              }
        //                        }
        //                    }

        //                }
        //            }


        //            return true;

        //        }
        //        return true;
        //    }
        //}

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
                }
            }
            return null;
        }
        public static List<ulong> playersInNationChat = new List<ulong>();

        public static MethodInfo GetOnlinePlayers;
        public static bool SetupMethod()
        {
            var pluginManager = torchbase.CurrentSession.Managers.GetManager<PluginManager>();
            var pluginId = new Guid("28a12184-0422-43ba-a6e6-2e228611cca5");

            if (pluginManager.Plugins.TryGetValue(pluginId, out ITorchPlugin nexus))
            {

                try
                {
                    MyPlayer player = MySession.Static.Players.GetPlayerByName("Crunch");
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

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {

            if (state == TorchSessionState.Loaded)
            {
               
               TorchState = TorchSessionState.Loaded;
               SetupMethod();
            
        
 
        
        
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
        //private static void OnTimedEventC(Object source, System.Timers.ElapsedEventArgs e)
        //{
        //    if (TorchState == TorchSessionState.Loaded)
        //    {
        //        Task.Run(() =>
        //        {
        //            foreach (MyPlayer player in MySession.Static.Players.GetOnlinePlayers())
        //            {
        //                Log.Info("1");
        //                if (player?.Controller.ControlledEntity is MyCockpit controller)
        //                {
        //                    Log.Info("2");
        //                    MyCubeGrid grid = controller.CubeGrid;
        //                    if (grid != null) { 
        //                    foreach (IMyBeacon block in grid.GetBlocks().OfType<IMyBeacon>())
        //                    {
        //                        Log.Info("3");
        //                        if (block != null && block.BlockDefinition.SubtypeName.Contains("Transponder") && block.IsFunctional && block.IsWorking)
        //                        {
        //                            Log.Info("4");
        //                            List<IMyEntity> l = new List<IMyEntity>();
        //                            IMyFaction fac = FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid));
        //                                if (fac != null)
        //                                {
        //                                    Log.Info("fac wasnt null");
        //                                    BoundingSphereD sphere = new BoundingSphereD(grid.PositionComp.GetPosition(), 15000);
        //                                    l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);

        //                                    foreach (IMyEntity eb in l)
        //                                    {
        //                                        MyCubeGrid gridCheck = (eb as MyCubeGrid);

        //                                        if (gridCheck != null)
        //                                        {
        //                                            foreach (IMyBeacon blocks2 in gridCheck.GetBlocks().OfType<IMyBeacon>())
        //                                            {
        //                                                if (block != null && block.BlockDefinition.SubtypeName.Contains("Transponder") && block.IsFunctional && block.IsWorking)
        //                                                {
        //                                                    IMyFaction fac2 = FacUtils.GetPlayersFaction(FacUtils.GetOwner(gridCheck));
        //                                                    if (fac2 != null && MySession.Static.Factions.AreFactionsFriends(fac.FactionId, fac2.FactionId))
        //                                                    {
        //                                                        MyGps gps = CreateGps(gridCheck, gridCheck.PositionComp.GetPosition(), Color.Green, 15, block.DisplayName);
        //                                                    }
        //                                                    else if (fac2 != null && MySession.Static.Factions.AreFactionsNeutrals(fac.FactionId, fac2.FactionId))
        //                                                    {
        //                                                        MyGps gps = CreateGps(gridCheck, gridCheck.PositionComp.GetPosition(), Color.White, 15, block.DisplayName);
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        });

        //    }
        //}

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
                DiscardAt = new TimeSpan(0, 0,  0, seconds),
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
                SendMessage("CRUNCH", "The fucking task is running?", Color.Blue,(long) player.Id.SteamId);
            }
            int facCount = 0;
            Task.Run(() =>
            {
                List<MyFaction> addThese = new List<MyFaction>();

                foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
                {
                   
                    if (f.Value != null)
                    {
                        facCount += 1;
                        if (f.Value.PrivateInfo == null)
                        {

                            addThese.Add(f.Value);

                        }
                        else
                        {
                            if (!f.Value.PrivateInfo.ToLower().Contains("autonationpeace=true") && !f.Value.PrivateInfo.ToLower().Contains("autonationpeace=false"))
                            {
                                addThese.Add(f.Value);


                            }
                        }
                        if (f.Value.Description != null)
                        {
                            int tagsInDescription = 0;
                            if (f.Value.Description.Contains("UNIN"))
                            {
                                tagsInDescription++;
                            }
                            if (f.Value.Description.Contains("FEDR"))
                            {
                                tagsInDescription++;
                            }
                            if (f.Value.Description.Contains("CONS"))
                            {
                                tagsInDescription++;
                            }
                            if (tagsInDescription > 1)
                            {

                                NationsPlugin.Log.Info("NATION - This guys trying to do !nationjoin with multiple tags " + f.Value.Name + " " + f.Value.Tag);


                            }
                            else
                            {

                                if (f.Value.Description != null && f.Value.Description.Contains("UNIN") && f.Value.PrivateInfo != null && f.Value.PrivateInfo.ToLower().Contains("autonationpeace=true"))
                                {
                                   
                                    
                                    if (file.doWhitelist){

                                    }
                                    else
                                    {
                                        doStuff(f.Value, "UNIN");
                                    }
                                }

                                if (f.Value.Description != null && f.Value.Description.Contains("CONS") && f.Value.PrivateInfo != null && f.Value.PrivateInfo.ToLower().Contains("autonationpeace=true"))
                                {

                                    doStuff(f.Value, "CONS");

                                }
                                if (f.Value.Description != null && f.Value.Description.Contains("FEDR") && f.Value.PrivateInfo != null && f.Value.PrivateInfo.ToLower().Contains("autonationpeace=true"))
                                {

                                    doStuff(f.Value, "FEDR");

                                }
                            }
                        }
                    }
                }
         foreach (MyFaction f in addThese)
                {
                    String current = f.PrivateInfo;
                
                    f.PrivateInfo = "autonationpeace=false\nexclude[]\nTo exclude put tags in the bracket, for example [ITC,ADY]\n" + current;
                }
            });
            if (MySession.Static.Players.GetPlayerByName("Crunch") != null && FUCKINGFUCKFUCK)
            {
                MyPlayer player = MySession.Static.Players.GetPlayerByName("Crunch");
                SendMessage("CRUNCH", facCount.ToString(), Color.Blue, (long)player.Id.SteamId);
            }
  
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
