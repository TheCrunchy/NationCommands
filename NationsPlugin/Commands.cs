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
