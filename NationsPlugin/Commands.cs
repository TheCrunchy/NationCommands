using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.API.Managers;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Managers;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;
using static Sandbox.Game.Multiplayer.MyFactionCollection;

namespace NationsPlugin
{
    public class Commands : CommandModule
    {
        //method from lord tylus
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
        [Command("factionpurge", "Purge factions if all members havent logged on for x days")]
        [Permission(MyPromoteLevel.Admin)]
        public void purgeFactions(int days, Boolean zero = false)
        {
            if (days == 0 && !zero)
            {
                Context.Respond("Are you sure? this probably isnt a good idea, attach true to end of command.");
                return;
            }
            int purgedFactions;
            var cutoff = DateTime.Now - TimeSpan.FromDays(days);
            List<MyFaction> purging = new List<MyFaction>();
            //rewrite this shit entirely
            foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
            {
                bool purged = true;
                bool npc = false;
                bool isPurging = true;
                //bool notPurging = false;


                //if (f.Value.Members.Count > 0)
                //{

                //}
                //else
                //{
                //    purging.Add(f.Value);
                //    break;
                //}
                if (f.Value.Tag.Length > 3)
                {
                    //breaking here fucks up everything 
                    ///break;
                }
                else
                {
                    foreach (KeyValuePair<long, MyFactionMember> m in f.Value.Members)
                    {
                        //do this shit

                        MyIdentity test = MySession.Static.Players.TryGetIdentity(m.Key);


                        if (test != null && MySession.Static.Players.IdentityIsNpc(test.IdentityId))
                        {
                            purged = false;
                            npc = true;
                            if (purging.Contains(f.Value))
                            {
                                purging.Remove(f.Value);
                            }

                        }
                        if (test != null && test.DisplayName != null && test.LastLoginTime < cutoff && isPurging && !npc)
                        {
                            //debug messages

                            purged = true;

                        }
                        else
                        {
                            purged = false;
                            if (purging.Contains(f.Value))
                            {
                                purging.Remove(f.Value);
                            }
                     
                                isPurging = false;
                          

                        }
                    }
                    if (purged)
                    {

                        purging.Add(f.Value);
                        List<long> kick = new List<long>();
                        foreach (KeyValuePair<long, MyFactionMember> m in f.Value.Members)
                        {
                            kick.Add(m.Key);

                        }
                        foreach (long n in kick)
                        {
                            f.Value.KickMember(n);
                        }
                    }

                }
            }
            Context.Respond("Purged " + purging.Count);
            foreach (MyFaction f in purging)
            {
                //add this to a logger
                NationsPlugin.Log.Info("Purging " + f.Name + "## TAG : " + f.Tag);

                NetworkManager.RaiseStaticEvent(_factionChangeSuccessInfo, MyFactionStateChange.RemoveFaction, f.FactionId, f.FactionId, 0L, 0L);
                if (!MyAPIGateway.Session.Factions.FactionTagExists(f.Tag)) break;
                MyAPIGateway.Session.Factions.RemoveFaction(f.FactionId);
            }

        }
        private static MethodInfo _factionChangeSuccessInfo = typeof(MyFactionCollection).GetMethod("FactionStateChangeSuccess", BindingFlags.NonPublic | BindingFlags.Static);

 

        [Command("stripalliances", "declare war on everyone")]
        [Permission(MyPromoteLevel.Admin)]
        public void declarewaronall(string tag)
        {

                bool console = false;

                if (Context.Player == null)
                {
                    console = true;
                }
                IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (fac == null)
                {
                    MyPlayer player = Context.Torch.CurrentSession?.Managers?.GetManager<IMultiplayerManagerBase>()?.GetPlayerByName(tag) as MyPlayer;
                    if (player == null)
                    {
                        IMyIdentity id = GetIdentityByNameOrId(tag);
                        if (id == null)
                        {
                            Context.Respond("Cant find that faction or player.");
                            return;
                        }
                        else
                        {
                            fac = FacUtils.GetPlayersFaction(id.IdentityId);
                            if (fac == null)
                            {
                                Context.Respond("The player that was found does not have a faction.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        fac = FacUtils.GetPlayersFaction(player.Identity.IdentityId);
                        if (fac == null)
                        {
                            Context.Respond("The player that was found does not have a faction.");
                            return;
                        }
                    }



                }
               
               
                foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
                {
                    if (f.Value != fac)
                    {
                        if (f.Value.Description != null && f.Value.Tag.Length == 3)
                        {
                            Sandbox.Game.Multiplayer.MyFactionCollection.DeclareWar(fac.FactionId, f.Value.FactionId);
                        }
                        else
                        {
                        }

                    }
                }
                Context.Respond("That faction has now declared war on all factions");

            

        }
        public static string GetStringBetweenCharacters(string input, char charFrom, char charTo)
        {
            int posFrom = input.IndexOf(charFrom);
            if (posFrom != -1) //if found char
            {
                int posTo = input.IndexOf(charTo, posFrom + 1);
                if (posTo != -1) //if found char
                {
                    return input.Substring(posFrom + 1, posTo - posFrom - 1);
                }
            }

            return string.Empty;
        }
        private static CurrentCooldown CreateNewCooldown(Dictionary<long, CurrentCooldown> cooldownMap, long playerId, long cooldown)
        {

            var currentCooldown = new CurrentCooldown(cooldown);

            if (cooldownMap.ContainsKey(playerId))
                cooldownMap[playerId] = currentCooldown;
            else
                cooldownMap.Add(playerId, currentCooldown);

            return currentCooldown;
        }

       
        
        [Command("distress", "distress signals")]
        [Permission(MyPromoteLevel.None)]
        public void distress(string reason = "")
        {


            if (Context.Player == null)
            {
                Context.Respond("no no console no distress");
                return;
            }

           
            IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
            if (playerFac == null)
            {
                Context.Respond("You dont have a faction.");
                return;
            }
            String nation = "";
            if (reason != "")
            {
             

                reason = Context.RawArgs;
            }
            var currentCooldownMap = NationsPlugin.CurrentCooldownMap;
            if (currentCooldownMap.TryGetValue(Context.Player.IdentityId, out CurrentCooldown currentCooldown))
            {

                long remainingSeconds = currentCooldown.GetRemainingSeconds(null);

                if (remainingSeconds > 0)
                {

                    NationsPlugin.Log.Info("Cooldown for Player " + Context.Player.DisplayName + " still running! " + remainingSeconds + " seconds remaining!");
            Context.Respond("Command is still on cooldown for " + remainingSeconds + " seconds.");
                //    DateTime start = DateTime.Now;
             //    start.AddSeconds(remainingSeconds);
               //    var diff = start.Subtract(DateTime.Now);
                  
               //     string time = String.Format("{0}:{1}:{2}", diff.Minutes, diff.Seconds);
                 //   Context.Respond("Command is still on cooldown for " + remainingSeconds + " seconds.");
                    return;
                }
                currentCooldown = CreateNewCooldown(currentCooldownMap, Context.Player.IdentityId, NationsPlugin.file.CooldownMilliseconds);
                currentCooldown.StartCooldown(null);
            }
            else
            {
                currentCooldown = CreateNewCooldown(currentCooldownMap, Context.Player.IdentityId, NationsPlugin.file.CooldownMilliseconds);
                currentCooldown.StartCooldown(null);
            }
            if (EconUtils.getBalance(Context.Player.IdentityId) >= NationsPlugin.file.Price)
            {
          
                if (playerFac.Description.Contains("UNIN"))
                {
                    doSignal(Context.Player.Character.GetPosition(),"UNIN", NationsPlugin.file.UNIN, reason);
                    EconUtils.takeMoney(Context.Player.IdentityId, NationsPlugin.file.Price);
                    Context.Respond("Signal sent! You were charged " + String.Format("{0:n0}", NationsPlugin.file.Price) + " SC for the convenience.", Color.Orange, NationsPlugin.file.Name);
                   
                        return;
                }
                if (playerFac.Description.Contains("CONS"))
                {
                    doSignal(Context.Player.Character.GetPosition(), "CONS", NationsPlugin.file.CONS, reason);
                    EconUtils.takeMoney(Context.Player.IdentityId, NationsPlugin.file.Price);
                    Context.Respond("Signal sent! You were charged " + String.Format("{0:n0}", NationsPlugin.file.Price) + " SC for the convenience.", Color.Orange, NationsPlugin.file.Name);
                    return;
                }
                if (playerFac.Description.Contains("FEDR"))
                {
                    doSignal(Context.Player.Character.GetPosition(), "FEDR", NationsPlugin.file.FEDR, reason);
                    EconUtils.takeMoney(Context.Player.IdentityId, NationsPlugin.file.Price);
                    Context.Respond("Signal sent! You were charged " + String.Format("{0:n0}", NationsPlugin.file.Price) + " SC for the convenience.", Color.Orange, NationsPlugin.file.Name);
                    return;
                }
            }
            else
            {
                Context.Respond("You cant afford the price of " + String.Format("{0:n0}", NationsPlugin.file.Price) + " SC", Color.Orange, NationsPlugin.file.Name);
            }





        }

        [Command("nation reload", "reload config")]
        [Permission(MyPromoteLevel.Admin)]
        public void reloadjoin()
        {
            NationsPlugin.LoadConfig();
            Context.Respond("Reloaded!", Color.Orange, NationsPlugin.file.Name);
        }


            public void doSignal(Vector3D Position, String tag, String nation, String reason)
        {
            MyGps gps = CreateGps(Position, new Color(NationsPlugin.file.red, NationsPlugin.file.green, NationsPlugin.file.blue), 60, nation, reason);

               NationsPlugin.signalsToClear.Add(gps, DateTime.Now.AddMilliseconds(NationsPlugin.file.MillisecondsTimeItLasts));
         
            MyGpsCollection gpsCollection = (MyGpsCollection)MyAPIGateway.Session?.GPS;
            foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
            {
                float distance = Vector3.Distance(Position, p.GetPosition());
                distance = distance * 1000;
                if (distance <= NationsPlugin.file.DetectionRangeForNearbyInKM)
                {
          

                    IMyFaction playerFac = FacUtils.GetPlayersFaction(p.Identity.IdentityId);
                    if (playerFac != null)
                    {
                        if (NationsPlugin.file.MessageEnabled && !playerFac.Description.Contains(tag))
                 
                        {
                            MyGps gps2 = CreateGps(Position, new Color(NationsPlugin.file.hostilered, NationsPlugin.file.hostilegreen, NationsPlugin.file.hostileblue), 60, nation, reason);
                            MyGps gpsRef = gps2;
                            long entityId = 0L;
                            entityId = gps2.EntityId;

                            gpsCollection.SendAddGps(p.Identity.IdentityId, ref gpsRef, entityId, true);
                            NationsPlugin.signalsToClear.Add(gps2, DateTime.Now.AddMilliseconds(NationsPlugin.file.MillisecondsTimeItLasts));
                            SendMessage(NationsPlugin.file.HostileMessageName, NationsPlugin.file.HostileMessage, Color.Red, (long)p.Id.SteamId);
                        }
                    }
                    else
                    {
                        MyGps gps2 = CreateGps(Position, new Color(NationsPlugin.file.hostilered, NationsPlugin.file.hostilegreen, NationsPlugin.file.hostileblue), 60, nation, reason);
                        MyGps gpsRef = gps2;
                        long entityId = 0L;
                        entityId = gps2.EntityId;

                        gpsCollection.SendAddGps(p.Identity.IdentityId, ref gpsRef, entityId, true);
                        NationsPlugin.signalsToClear.Add(gps2, DateTime.Now.AddMilliseconds(NationsPlugin.file.MillisecondsTimeItLasts));
                        SendMessage(NationsPlugin.file.HostileMessageName, NationsPlugin.file.HostileMessage, Color.Red, (long)p.Id.SteamId);
                    }

                }
            }

            foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
            {
                IMyFaction playerFac = FacUtils.GetPlayersFaction(p.Identity.IdentityId);
                if (playerFac != null)
                {
                    if (playerFac.Description.Contains(tag))
                    {
                        MyGps gpsRef = gps;
                        long entityId = 0L;
                        entityId = gps.EntityId;
                      
                        gpsCollection.SendAddGps(p.Identity.IdentityId, ref gpsRef, entityId, true);
                        // if (NationsPlugin.file.RemoveOldOnNewSignal)
                        //{

                        //   List<IMyGps> playergpsList = MyAPIGateway.Session?.GPS.GetGpsList(p.Identity.IdentityId);
                        //
                        //  if (playergpsList == null)
                        //      break;

                        //  foreach (IMyGps gps2 in playergpsList)
                        //  {

                        //    if (gps2.Description.Contains("Nation Distress Signal"))
                        //    {
                        //       MyAPIGateway.Session?.GPS.RemoveGps(p.Identity.IdentityId, gps2);
                        //    }


                        //   }

                        //  }


                        if (NationsPlugin.file.MessageEnabled)
                        {
                            if (reason == "")
                            {
                                SendMessage(NationsPlugin.file.MessageName, NationsPlugin.file.Message, Color.Orange, (long)p.Id.SteamId);
                            }
                            else
                            {
                                SendMessage(NationsPlugin.file.MessageName, NationsPlugin.file.Message, Color.Orange, (long)p.Id.SteamId);
                                SendMessage(NationsPlugin.file.MessageName, "Signal reads - " + reason, Color.Red, (long)p.Id.SteamId);
                            }

                        }
                    }
                }
            }
        }
              private MyGps CreateGps(Vector3D Position, Color gpsColor, long seconds, String Nation, String Reason)
        {

            MyGps gps = new MyGps
            {
                Coords = Position,
                Name = Nation + " - Distress Signal",
                DisplayName = Nation + " - Distress Signal",
                GPSColor = gpsColor,
                IsContainerGPS = true,
                ShowOnHud = true,
                DiscardAt = new TimeSpan?(),
                Description = "Nation Distress Signal \n" + Reason,
            };
            gps.UpdateHash();
          

            return gps;
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
        [Command("nationjoin", "Join a nation")]
        [Permission(MyPromoteLevel.None)]
        public void massjoin(string tag)
        {


            if (Context.Player == null)
            {
                Context.Respond("Consoles not allowed to join a faction.");
                return;
            }


            IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
            if (playerFac == null)
            {
                Context.Respond("You dont have a faction.");
                return;
            }
          Boolean excluding = false;
            List<String> exclusions = new List<String>();
            if (playerFac.PrivateInfo.ToLower().Contains("exclude["))
            {
                excluding = true;
                
                String exclusionBeforeFormat = GetStringBetweenCharacters(playerFac.PrivateInfo, '[', ']');
                if (exclusionBeforeFormat.Contains(",")){ 
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
            if (playerFac.IsLeader(Context.Player.IdentityId) || playerFac.IsFounder(Context.Player.IdentityId))
            {

                if (playerFac.Description.Contains(tag))
                {
                    foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
                    {
                        if (f.Value != playerFac)
                        {
                            if (f.Value.Description != null && f.Value.Description.Contains(tag))
                            {
                               
                                MyFactionPeaceRequestState state = MySession.Static.Factions.GetRequestState(playerFac.FactionId, f.Value.FactionId);

                                if (excluding)
                                {
                                    if (!exclusions.Contains(f.Value.Tag.ToLower()))
                                    {
                                        if (state != MyFactionPeaceRequestState.Sent)
                                        {
                                            Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, f.Value.FactionId);
                                        }
                                        if (state == MyFactionPeaceRequestState.Pending)
                                        {
                                            Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, f.Value.FactionId);
                                        }
                                    }
                                }
                                else
                                {
                                    if (state != MyFactionPeaceRequestState.Sent)
                                    {
                                        Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, f.Value.FactionId);
                                    }
                                    if (state == MyFactionPeaceRequestState.Pending)
                                    {
                                        Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, f.Value.FactionId);
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
                    Context.Respond("Sent peace requests to all factions with the tag and declared war on all others!");
                }
                else
                {
                    Context.Respond("Your faction description does not contain that tag!");
                }
            }
            else
            {
                Context.Respond("You are not a faction leader or founder!");
                return;
            }

        }
    }
}
