using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.API.Managers;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using VRageMath;

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
                                Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, f.Value.FactionId);
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
