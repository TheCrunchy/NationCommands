using NLog;
using Sandbox.Engine.Multiplayer;
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
using Torch.Session;
using VRageMath;
using static Sandbox.Game.Multiplayer.MyFactionCollection;

namespace NationsPlugin
{
    public class NationsPlugin : TorchPluginBase
    {

        public static MethodInfo GetOnlinePlayers;
        public static bool SetupMethod()
        {
            var pluginManager = torchbase.CurrentSession.Managers.GetManager<PluginManager>();
            var pluginId = new Guid("28a12184-0422-43ba-a6e6-2e228611cca5");
            pluginManager.Plugins.TryGetValue(pluginId, out ITorchPlugin nexus);
            if (nexus == null)
            {
                return false;
            }
            try
            {
                Type ReflectedServerSideAPI = nexus.GetType().Assembly.GetType("Nexus.API.NexusServerSideAPI");
                MethodInfo GetOnlinePlayers = ReflectedServerSideAPI?.GetMethod("GetAllOnlinePlayersObject", BindingFlags.Public | BindingFlags.Static);
                return true;
            }
            catch (Exception ex)
            {
                NationsPlugin.Log.Error(ex, "");
                return false;
            }

        }

        public static ConfigFile file;
        public static Logger Log = LogManager.GetCurrentClassLogger();
        private static string path;
        public static Dictionary<long, CurrentCooldown> CurrentCooldownMap { get; } = new Dictionary<long, CurrentCooldown>();
        public static Dictionary<MyGps, DateTime> signalsToClear = new Dictionary<MyGps, DateTime>();
        private static Timer aTimer = new Timer();
        private static Timer bTimer = new Timer();
        private TorchSessionManager sessionManager;
        private ChatManagerServer _chatmanager;
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
            aTimer.Enabled = true;
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
        //private void MessageRecieved(TorchChatMessage msg, ref bool consumed)
        //{
        //    Log.Info("HERE BE A MESSAGE");
        //    consumed = true;
        //    msg.Message
        //    return;
        //}

        private void SessionChanged(ITorchSession session, TorchSessionState state)
        {

            if (state == TorchSessionState.Loaded)
            {
               
               TorchState = TorchSessionState.Loaded;
              //  SetupMethod();
            }
        }
          //  _chatmanager = Torch.CurrentSession.Managers.GetManager<ChatManagerServer>();
        //        if (_chatmanager == null)
        //        {
        //            Log.Warn("No chat manager loaded!");
        //        }
        //        else
        //        {
        //            _chatmanager.MessageRecieved += MessageRecieved;
        //        }
        //    }
        //}
        

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
                            if (f.Value.Description.ToLower().Contains("unin"))
                            {
                                tagsInDescription++;
                            }
                            if (f.Value.Description.ToLower().Contains("fedr"))
                            {
                                tagsInDescription++;
                            }
                            if (f.Value.Description.ToLower().Contains("cons"))
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
                                    doStuff(f.Value, "UNIN");


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
