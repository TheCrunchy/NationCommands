
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
using Torch.API.Plugins;
using Torch.Commands;
using Torch.Commands.Permissions;
using Torch.Managers;
using Torch.Mod;
using Torch.Mod.Messages;
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
        [Command("nation fuckfuck", "debug stuff")]
        [Permission(MyPromoteLevel.None)]
        public void fuckfuckfuck()
        {
               if (Context.Player.IsAdmin || Context.Player.SteamUserId == 76561198045390854)
            {
                NationsPlugin.FUCKINGFUCKFUCK = !NationsPlugin.FUCKINGFUCKFUCK;
               Context.Respond("Now to spam Crunch with messages!");
            }

         }

        [Command("repforce", "debug stuff")]
        [Permission(MyPromoteLevel.None)]
        public void fuckfuckfuck(string tag)
        {
            if (Context.Player.IsAdmin || Context.Player.SteamUserId == 76561198045390854)
            {
                IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(tag);
                if (fac2 == null)
                {
                    Context.Respond("Cant find that faction.", Color.Red, "The Government");
                    return;
                }
                IMyFaction fac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
                if (fac.IsLeader(Context.Player.IdentityId) || fac.IsFounder(Context.Player.IdentityId))
                {
                    MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, fac2.FactionId, 1500);
                }
            }
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


        [Command("nation add", "send a reputation request")]
        [Permission(MyPromoteLevel.None)]
        public void addToDescription(string nation, string tag)
        {
            if (Context.Player != null && !Context.Player.IsAdmin && Context.Player.SteamUserId != 76561198045390854)
            {
                return;
            } 
                MyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(nation);
            if (fac2 == null)
            {
                Context.Respond("Cant find that nation.", Color.Red, "The Government");
                return;
            }
            IMyFaction fac3 = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac3 == null)
            {
                Context.Respond("Cant find that faction.", Color.Red, "The Government");
                return;
            }
            String description = fac2.Description;
           fac2.Description = description + "\n[" + fac3.Tag.ToUpper() + "] # " + fac3.Name;
            Context.Respond("Worked, though keen needs a relog to see changes :(");
        }
        [Command("nation remove", "send a reputation request")]
        [Permission(MyPromoteLevel.None)]
        public void removeFromDescription(string nation, string tag, string name = "")
        {
            if (Context.Player != null && !Context.Player.IsAdmin && Context.Player.SteamUserId != 76561198045390854)
            {
                return;
            }
            MyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(nation);
            if (fac2 == null)
            {
                Context.Respond("Cant find that nation.", Color.Red, "The Government");
                return;
            }
            IMyFaction fac3 = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac3 == null)
            {
                Context.Respond("Cant find that faction.", Color.Red, "The Government");
                return;
            }
            String description = fac2.Description;
  
            if (name != "")
            {
                fac2.Description = fac2.Description.Replace("[" + tag.ToUpper() + "] # " + name, "");
            } 
            else
            {
                if (fac2.Description.Contains("[" + fac3.Tag.ToUpper() + "]")){
                    fac2.Description = fac2.Description.Replace("[" + fac3.Tag + "] # " + fac3.Name, "");
                }
                else
                {
                    Context.Respond("Cant seem to find that, try !nation remove TAG NAME");
                }
            }
          

            Context.Respond("Worked, though keen needs a relog to see changes :(");
        }
        [Command("nation info", "display a nations members")]
        [Permission(MyPromoteLevel.None)]
        public void DisplayFactionInfo(string tag)
        {
          
                bool console = false;
        
                if (Context.Player == null)
                {
                    console = true;
                }
                IMyFaction fac = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac == null)
            {
                Context.Respond("Cant find that, try FEDR, CONS or UNIN");
                return;
            }
            
  
            
                    if (!console)
                    {
                        DialogMessage m = new DialogMessage("Faction Info", fac.Name, "\nTag: " + fac.Tag + "\nDescription: " + fac.Description);
                        ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);
                    }
                    else
                    {
                        Context.Respond("Name: " + fac.Name + "\nTag: " + fac.Tag + "\nDescription: " + fac.Description);
                    }
                    return;
                

            
        }

        public  Dictionary<String, RepRequest> RepRequests = new Dictionary<string, RepRequest>();
        [Command("rep request", "send a reputation request")]
        [Permission(MyPromoteLevel.None)]
        public void repsend(string tag)
        {
            if (FacUtils.GetPlayersFaction(Context.Player.IdentityId) == null)
            {
                Context.Respond("You arent in a faction.", Color.Red, "The Government");
                return;
            }
            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac2 == null)
            {
                Context.Respond("Cant find that faction.", Color.Red, "The Government");
                return;
            }

            IMyFaction fac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
            if (fac.IsLeader(Context.Player.IdentityId) || fac.IsFounder(Context.Player.IdentityId))
            {


                if (RepRequests.ContainsKey(fac.Tag))
                {
                    RepRequests.TryGetValue(fac.Tag, out RepRequest requests);
                    requests.addRequest(tag, 1500);
                    RepRequests.Remove(fac.Tag);
                    RepRequests.Add(fac.Tag, requests);
                    Context.Respond("Request sent");
                    foreach (MyFactionMember mb in fac2.Members.Values)
                    {


                        ulong steamid = MySession.Static.Players.TryGetSteamId(mb.PlayerId);
                
                        if (steamid != 0)
                        {
                       
                        SendMessage("[Reputation Requests]", fac.Name + " has requested reputation, to accept use !rep accept " + fac.Tag, Color.DarkGreen, (long)steamid);

                        }

                    }
                }
                else
                {
                    RepRequest requests = new RepRequest();
                    requests.addRequest(tag, 1500);
                    Context.Respond("Request sent");
                    RepRequests.Add(fac.Tag, requests);
                }
            }
            else
            {
                Context.Respond("You are not a leader or a founder.", Color.Red, "The Government");
            }
         }
        [Command("rep remove", "remove a reputation modification")]
        [Permission(MyPromoteLevel.None)]
        public void repremove(string tag)
        {
            if (FacUtils.GetPlayersFaction(Context.Player.IdentityId) == null)
            {
                Context.Respond("You arent in a faction.", Color.Red, "The Government");
                return;
            }
            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac2 == null)
            {
                Context.Respond("Cant find that faction.", Color.Red, "The Government");
                return;
            }
            IMyFaction fac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
            if (fac.IsLeader(Context.Player.IdentityId) || fac.IsFounder(Context.Player.IdentityId))
            {
                        MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, fac2.FactionId, 0);
                        Context.Respond("Rep changed - You may need to disconnect to see the changes.", Color.Green, "The Government");

              
            }
            else
            {
                Context.Respond("You are not a leader or a founder.", Color.Red, "The Government");
            }
        }

        [Command("rep accept", "accept a reputation modification")]
        [Permission(MyPromoteLevel.None)]
        public void repaccept(string tag)
        {
            if (FacUtils.GetPlayersFaction(Context.Player.IdentityId) == null)
            {
                Context.Respond("You arent in a faction.", Color.Red, "The Government");
                return;
            }
            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (fac2 == null)
            {
                Context.Respond("Cant find that faction.", Color.Red, "The Government");
                return;
            }
            IMyFaction fac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
            if (fac.IsLeader(Context.Player.IdentityId) || fac.IsFounder(Context.Player.IdentityId))
            {

            
            
          
            if (RepRequests.ContainsKey(tag))
            {
                RepRequests.TryGetValue(tag, out RepRequest requests);
                if (requests.getRequestAmount(fac.Tag) > 0)
                {
                    int amount = requests.getRequestAmount(fac.Tag);
                    MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, fac2.FactionId, 1500);
                    Context.Respond("Rep changed - You may need to disconnect to see the changes.", Color.Green, "The Government");
                 
                }
            }
            else
            {
                Context.Respond("No request from that faction.", Color.Red, "The Government");
            }
            }
            else
            {
                Context.Respond("You are not a leader or a founder.", Color.Red, "The Government");
            }
        }

        public string getOnline(String nation)
        {
      
           

            List<object[]> ReturnPlayers = new List<object[]>();
            object[] MethodInput = new object[] { ReturnPlayers };

          NationsPlugin.GetOnlinePlayers?.Invoke(null, MethodInput);

            //After inputing the object[] you can simply call this to get your return variable (since this method is by ref)
            ReturnPlayers = (List<object[]>)MethodInput[0];

            //Here you can call either my function, or create your own to convert it into more 'usable' data
            List<Player> Players = new List<Player>();
            
           foreach (object[] obj in ReturnPlayers)
            {
            
                Players.Add(new Player((string)obj[0], (ulong)obj[1], (long) obj[2], (int)obj[3]));
           }
            Dictionary<int, String> online = new Dictionary<int, string>();
            StringBuilder sb = new StringBuilder();
            foreach (Player player in Players)
            {
                IMyFaction fac = FacUtils.GetPlayersFaction(player.IdentityID);
                if (fac != null && fac.Description != null && fac.Description.Contains(nation)) {
                   // sb.Append(player.OnServer + " # " + player.PlayerName + "\n");
                   if (online.ContainsKey(player.OnServer))
                    {
                        online.TryGetValue(player.OnServer, out string temp);
                            temp += ", " + player.PlayerName ;
                        online.Remove(player.OnServer);
                        online.Add(player.OnServer, temp);
                    }
                   else
                    {
                        online.Add(player.OnServer, player.PlayerName);
                    }
                }


            }
           
            foreach (KeyValuePair<int, String> pairs in online)
            {
                sb.Append("Instance " + pairs.Key + " - " + pairs.Value + "\n");
            }
            return sb.ToString();
        }
        [Command("nation online", "online members")]
        [Permission(MyPromoteLevel.None)]
        public void getonlinenation(string nation = "")
        {

         //   Context.Respond("Command is a thing");
         //   if (Context.Player.SteamUserId == 76561198045390854 || Context.Player.IsAdmin)
        //    {




                //  Context.Respond("Setting up the method broke, will try again");


              



             if (Context.Player != null)
          {

                    IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId);
                    if (playerFac == null)
                    {
                        Context.Respond("You dont have a faction.");
                        return;
                    }



                    if (playerFac.Description.Contains("UNIN"))
                    {
                        nation = "UNIN";


                    }
                    if (playerFac.Description.Contains("CONS"))
                    {
                        nation = "CONS";


                    }
                    if (playerFac.Description.Contains("FEDR"))
                    {
                        nation = "FEDR";

                    }
                MyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(nation);
                if (fac2 == null)
                {
                    Context.Respond("Cant find that nation.", Color.Red, "The Government");
                    return;
                }
                if (NationsPlugin.file.doWhitelist)
                {
                    if (fac2.Description.Contains("[" + playerFac.Tag.ToUpper() + "]"))
                    {
                        DialogMessage m = new DialogMessage("Online members", nation, getOnline(nation));
                        ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);
                    }

                    else
                    {
                        Context.Respond("You havent been added to the whitelist so you cannot use this command.", Color.Red, "The Government");
                        return;
                    }
                }
                else
                {
                    DialogMessage m = new DialogMessage("Online members", nation, getOnline(nation));
                    ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);
                }
                }
                else
                {
                    Context.Respond(getOnline(nation));
            //    }
            }
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
        [Command("enablewhitelist", "reload config")]
        [Permission(MyPromoteLevel.Admin)]
        public void reloadjoin()
        {
            NationsPlugin.LoadConfig();
            NationsPlugin.file.doWhitelist = true;
            NationsPlugin.SaveConfig();
            Context.Respond("Reloaded!", Color.Orange, NationsPlugin.file.Name);
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
            MyGps gps = CreateGps(Position, new Color(NationsPlugin.file.red, NationsPlugin.file.green, NationsPlugin.file.blue), 300, nation, reason);

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
              private MyGps CreateGps(Vector3D Position, Color gpsColor, int seconds, String Nation, String Reason)
        {

            MyGps gps = new MyGps
            {
                Coords = Position,
                Name = Nation + " - Distress Signal",
                DisplayName = Nation + " - Distress Signal",
                GPSColor = gpsColor,
                IsContainerGPS = true,
                ShowOnHud = true,
                DiscardAt = new TimeSpan(0, 0, seconds, 0),
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
        [Command("nation list", "scan descriptions for nation")]
        [Permission(MyPromoteLevel.Admin)]
        public void scan(string nation)
        {
            int count = 0;
            StringBuilder response = new StringBuilder();
            foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
            {

                if (f.Value.Description != null)
                {
                    if (NationsPlugin.file.doWhitelist) {
                        IMyFaction nationfac = MySession.Static.Factions.TryGetFactionByTag(nation);
                     
                    if (nationfac != null && f.Value.Description.Contains(nation.ToUpper()) && nationfac.Description != null && nationfac.Description.Contains("["+f.Value.Tag.ToUpper()+"]"))
                    {
                        count += f.Value.Members.Count;
                        response.Append(f.Value.Name + " [" + f.Value.Tag + "] MEMBERS : " + f.Value.Members.Count + "\n");
                    }
                }
                    else
                    {
                        if (f.Value.Description.Contains(nation.ToUpper()))
                        {
                            count += f.Value.Members.Count;
                            response.Append(f.Value.Name + " [" + f.Value.Tag + "] MEMBERS : " + f.Value.Members.Count + "\n");
                        }
                    }
                }
            }
            Context.Respond(response.ToString() + "\n" + count);

        }

        [Command("nation scan", "scan descriptions for nation")]
        [Permission(MyPromoteLevel.Admin)]
        public void scan()
        {
            String response = "";
            foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
            {

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
                        response +=  f.Value.Name + " " + f.Value.Tag + ", ";
                        NationsPlugin.Log.Info("NATION - This guys trying to do !nationjoin with multiple tags " + Context.Player + " " + f.Value.Name + " " + f.Value.Tag);
                    }
                }
            }
            Context.Respond(response);

        }

        [Command("nationjoin", "Join a nation")]
        [Permission(MyPromoteLevel.None)]
        public void old(string tag)
        {
           // Context.Respond("No no, do !nation join");
            massjoin(tag);
        }
        [Command("nation join", "Join a nation")]
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
            MyFaction nation = MySession.Static.Factions.TryGetFactionByTag(tag);
            if (nation == null)
            {
                Context.Respond("Cant find that nation.", Color.Red, "The Government");
                return;
            }
            if (NationsPlugin.file.doWhitelist)
            {
                if (!nation.Description.Contains("[" + playerFac.Tag.ToUpper() + "]"))
                {
                    Context.Respond("You havent been added to the whitelist so you cannot use the !nationjoin.", Color.Red, "The Government");
                    return;
                }
            }
            int tagsInDescription = 0;
            if (playerFac.Description.Contains("UNIN"))
            {
                tagsInDescription++;
            }
            if (playerFac.Description.Contains("FEDR"))
            {
                tagsInDescription++;
            }
            if (playerFac.Description.Contains("CONS"))
            {
                tagsInDescription++;
            }
            if (tagsInDescription > 1)
            {
                Context.Respond("You cannot be a member of multiple nations.");
                NationsPlugin.Log.Info("NATION - This guys trying to do !nationjoin with multiple tags " + Context.Player + " " + playerFac.Name + " " + playerFac.Tag);

                return;
            }

            StringBuilder logThis = new StringBuilder();
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
                        if (f.Value != null && f.Value != playerFac)
                        {
                            if (f.Value.Description != null && f.Value.Description.Contains(tag))
                            {
                         
                                    int tagsInDescription2 = 0;
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
                                if (tagsInDescription2 > 1)
                                {

                                    NationsPlugin.Log.Info("NATION - This guys trying to do !nationjoin with multiple tags " + Context.Player + " " + f.Value.Name + " " + f.Value.Tag);

                                }
                                else
                                {

                                    MyFactionPeaceRequestState state = MySession.Static.Factions.GetRequestState(playerFac.FactionId, f.Value.FactionId);

                                    if (excluding)
                                    {
                                        if (!exclusions.Contains(f.Value.Tag.ToLower()))
                                        {
                                            if (state != MyFactionPeaceRequestState.Sent)
                                            {
                                                Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, f.Value.FactionId);
                                              logThis.Append("NATION REQUESTS - Sending peace reqest between " + playerFac.Name + " " + playerFac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);
                                                MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);
                                            }
                                            if (state == MyFactionPeaceRequestState.Pending)
                                            {
                                                Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, f.Value.FactionId);
                                                logThis.Append("NATION REQUESTS - Accepting peace reqest between " + playerFac.Name + " " + playerFac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);
                                                MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);
                                            }
                                            if (MySession.Static.Factions.AreFactionsNeutrals(playerFac.FactionId, f.Value.FactionId))
                                            {
                                                MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);
                                            }
                                        }
                                    }

                                    else
                                    {
                                        if (state != MyFactionPeaceRequestState.Sent)
                                        {
                                            Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, f.Value.FactionId);
                                             logThis.Append("NATION REQUESTS - Sending peace reqest between " + playerFac.Name + " " + playerFac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);
                                            MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);
                                        }
                                        if (state == MyFactionPeaceRequestState.Pending)
                                        {
                                            Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, f.Value.FactionId);
                                            logThis.Append("NATION REQUESTS - Accepting peace reqest between " + playerFac.Name + " " + playerFac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);
                                            MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);
                                        }
                                        if (MySession.Static.Factions.AreFactionsNeutrals(playerFac.FactionId, f.Value.FactionId))
                                        {
                                            MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);
                                        }
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
                    Context.Respond("Sent peace requests to all factions!");
                    NationsPlugin.Log.Info(logThis.ToString());
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
