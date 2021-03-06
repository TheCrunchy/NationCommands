﻿
using Nexus.DataStructures;
using NLog;
using Sandbox.Definitions;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.Screens.Models;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using ServerNetwork.Sync;
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
using VRage.Game.Factions.Definitions;
using VRage.Game.ModAPI;
using VRage.Network;
using VRage.ObjectBuilders;
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

        [Command("repforce", "debug stuff")]
        [Permission(MyPromoteLevel.None)]
        public void ReputationTesting(string tag)
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

        public static Dictionary<long, RepRequest> repRequests = new Dictionary<long, RepRequest>();

        [Command("crunchreptest", "debug stuff")]
        [Permission(MyPromoteLevel.None)]
        public void fuk()
        {
            IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
            if (playerFac != null)
            {
                String nationtag = NationsPlugin.GetNationTag(playerFac);
                MyFaction UNINAM = MySession.Static.Factions.TryGetFactionByTag("UNIN-AM");
                MyFaction FEDRAM = MySession.Static.Factions.TryGetFactionByTag("FEDR-AM");
                MyFaction CONSAM = MySession.Static.Factions.TryGetFactionByTag("CONS-AM");
                if (UNINAM != null && FEDRAM != null && CONSAM != null)
                {
                    if (nationtag != null)
                    {

                        //MyFactionPeaceRequestState state;
                        //state = MySession.Static.Factions.GetRequestState(playerFac.FactionId, UNINAM.FactionId);

                        //if (state != MyFactionPeaceRequestState.Sent)
                        //{
                        //    


                        //}
                        //if (state == MyFactionPeaceRequestState.Pending)
                        //{
                        //  
                        //}
                        switch (nationtag)
                        {
                            case "UNIN":
                                //    Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, UNINAM.FactionId);
                                //     Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, UNINAM.FactionId);
                                MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, UNINAM.FactionId, 1500);
                                //   MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, UNINAM.FactionId, 1500);
                                MyFactionCollection.DeclareWar(playerFac.FactionId, FEDRAM.FactionId);
                                MyFactionCollection.DeclareWar(playerFac.FactionId, CONSAM.FactionId);

                                return;
                            case "FEDR":
                                //   Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, FEDRAM.FactionId);
                                //    Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, FEDRAM.FactionId);
                                //  MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, FEDRAM.FactionId, 1500);
                                MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, FEDRAM.FactionId, 1500);
                                MyFactionCollection.DeclareWar(playerFac.FactionId, UNINAM.FactionId);
                                MyFactionCollection.DeclareWar(playerFac.FactionId, CONSAM.FactionId);

                                return;
                            case "CONS":
                                //    Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, CONSAM.FactionId);
                                //    Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, CONSAM.FactionId);
                                //  MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(id.IdentityId, CONSAM.FactionId, 1500);
                                Context.Respond("Doing cons");
                                MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, CONSAM.FactionId, 1500);
                                MyFactionCollection.DeclareWar(playerFac.FactionId, UNINAM.FactionId);
                                MyFactionCollection.DeclareWar(playerFac.FactionId, FEDRAM.FactionId);

                                return;
                        }
                    }
                    else
                    {
                        MyFactionCollection.DeclareWar(playerFac.FactionId, UNINAM.FactionId);
                        MyFactionCollection.DeclareWar(playerFac.FactionId, FEDRAM.FactionId);
                        MyFactionCollection.DeclareWar(playerFac.FactionId, CONSAM.FactionId);

                    }
                }
                else
                {
                    Context.Respond("Null");
                }
            }
        }

        [Command("factionpurge", "Purge factions if all members havent logged on for x days")]
        [Permission(MyPromoteLevel.Admin)]
        public void purgeFactions(int days, Boolean zero = false)
        {
            if (days == 0 && !zero)
            {
                Context.Respond("Are you sure you want to purge everyone from every faction? this probably isnt a good idea, attach true to end of command.");
                return;
            }
            int purgedFactions = 0;
            var cutoff = DateTime.Now - TimeSpan.FromDays(days);
            List<MyFaction> purging = new List<MyFaction>();
            //rewrite this shit entirely
            foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
            {
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

                        MyIdentity test = MySession.Static.Players.TryGetIdentity(m.Value.PlayerId);

                        if (test != null && !MySession.Static.Players.IdentityIsNpc(test.IdentityId) && test.LastLoginTime < cutoff)
                        {
                            f.Value.KickMember(m.Value.PlayerId, true);
                            purgedFactions++;
                        }
                    }

                }

            }
            Context.Respond("Purged " + purgedFactions + " members from factions");

        }
        private static MethodInfo _factionChangeSuccessInfo = typeof(MyFactionCollection).GetMethod("FactionStateChangeSuccess", BindingFlags.NonPublic | BindingFlags.Static);

        [Command("crunchfuckup", "declare war on everyone")]
        [Permission(MyPromoteLevel.Admin)]
        public void makeeveryoneanenemywiththesefacs()
        {
            IMyFaction fac1 = MySession.Static.Factions.TryGetFactionByTag("????");
            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag("WOLF");
            IMyFaction fac3 = MySession.Static.Factions.TryGetFactionByTag("arrr");
            foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
            {
                if (f.Value != fac1 && f.Value != fac2 && f.Value != fac3)
                {
                    Sandbox.Game.Multiplayer.MyFactionCollection.DeclareWar(f.Value.FactionId, fac1.FactionId);
                    Sandbox.Game.Multiplayer.MyFactionCollection.DeclareWar(f.Value.FactionId, fac2.FactionId);
                    Sandbox.Game.Multiplayer.MyFactionCollection.DeclareWar(f.Value.FactionId, fac3.FactionId);
                    foreach (KeyValuePair<long, MyFactionMember> m in f.Value.Members)
                    {
                        MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, fac1.FactionId, -1000);
                        MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, fac2.FactionId, -1000);
                        MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, fac3.FactionId, -1000);
                    }
                }
            }
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
            Dictionary<long, Dictionary<long, int>> reps = new Dictionary<long, Dictionary<long, int>>();
            foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
            {
                if (f.Value != fac)
                {
                    if (f.Value.Tag.Length == 4)
                    {
                        //foreach (KeyValuePair<long, MyFactionMember> m in fac.Members)
                        //{
                        //    System.Tuple<MyRelationsBetweenFactions, int> rep = MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(m.Value.PlayerId, f.Value.FactionId);

                        //    if (reps.TryGetValue(m.Value.PlayerId, out Dictionary<long, int> tempreps))
                        //    {
                        //        tempreps.Add(f.Value.FactionId, rep.Item2);
                        //        reps.Remove(m.Value.PlayerId);
                        //        reps.Add(m.Value.PlayerId, tempreps);
                        //    }
                        //  else
                        //    {
                        //        Dictionary<long, int> tempreps2 = new Dictionary<long, int>();
                        //        tempreps2.Add(f.Value.FactionId, rep.Item2);

                        //        reps.Add(m.Value.PlayerId, tempreps2);
                        //    }
                        //}
                    }
                    else
                    {
                        if (!MySession.Static.Factions.AreFactionsEnemies(fac.FactionId, f.Value.FactionId))
                        {
                            Sandbox.Game.Multiplayer.MyFactionCollection.DeclareWar(fac.FactionId, f.Value.FactionId);
                            MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, f.Value.FactionId, -3000);
                        }
                    }
                }
            }
            Context.Respond("That faction has now declared war on all factions");

            foreach (KeyValuePair<long, Dictionary<long, int>> playerFucks in reps)
            {
                foreach (KeyValuePair<long, int> ff in playerFucks.Value)
                {

                    MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(playerFucks.Key, ff.Key, ff.Value);

                }

            }


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
        [Command("n", "nation chat")]
        [Permission(MyPromoteLevel.None)]
        public void togglechat(bool bob = false)
        {
            if (bob && Context.Player.SteamUserId == 76561198045390854)
            {
                NationsPlugin.bob = true;
            }
            if (NationsPlugin.playersInNationChat.Contains((long)Context.Player.SteamUserId))
            {
                Commands.SendMessage("Nation Chat", "Toggled off, use !n to toggle", Color.Yellow, (long)Context.Player.SteamUserId);
                NationsPlugin.playersInNationChat.Remove((long)Context.Player.SteamUserId);

                return;
            }
            else
            {
                Commands.SendMessage("Nation Chat", "Toggled on, use !n to toggle", Color.Yellow, (long)Context.Player.SteamUserId);
                NationsPlugin.playersInNationChat.Add((long)Context.Player.SteamUserId);

                return;
            }


        }
        [Command("nation setminister", "add nation to whitelist file")]
        [Permission(MyPromoteLevel.Admin)]
        public void setMinister(string nation, string playername)
        {

            MyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(nation);
            if (fac2 == null)
            {
                Context.Respond("Cant find that nation.", Color.Red, "The Government");
                return;
            }
            MyIdentity player = GetIdentityByNameOrId(playername);
            if (player == null)
            {
                Context.Respond("Cant find that player");
                return;
            }
            switch (nation.ToUpper())
            {
                case "FEDR":
                    NationsPlugin.file.FedrMinister = (long)MySession.Static.Players.TryGetSteamId(player.IdentityId);
                    Context.Respond("FEDR Minister set to " + player.DisplayName + ", is this correct?");
                    NationsPlugin.SaveConfig();
                    break;
                case "CONS":
                    NationsPlugin.file.ConsMinister = (long)MySession.Static.Players.TryGetSteamId(player.IdentityId);
                    Context.Respond("CONS Minister set to " + player.DisplayName + ", is this correct?");
                    NationsPlugin.SaveConfig();
                    break;
                case "UNIN":
                    NationsPlugin.file.UninMinister = (long)MySession.Static.Players.TryGetSteamId(player.IdentityId);
                    Context.Respond("UNIN Minister set to " + player.DisplayName + ", is this correct?");
                    NationsPlugin.SaveConfig();
                    break;
            }
            Context.Respond("Added to the whitelist, remember to do it on all servers!");

        }
        [Command("nation balance", "moneys")]
        [Permission(MyPromoteLevel.None)]
        public void nationBalance()
        {
            if (Context.Player == null)
            {

                return;
            }
            IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
            if (playerFac.Description.Contains("UNIN"))
            {
                IMyFaction nation = MySession.Static.Factions.TryGetFactionByTag("UNIN");
                long account = 0;
                foreach (KeyValuePair<long, MyFactionMember> m in nation.Members)
                {
                    if (m.Value.IsFounder)
                    {
                        account = m.Value.PlayerId;
                        break;
                    }
                }
                if (account == 0)
                {
                    Context.Respond("Couldnt find the account, tell Crunch");
                    return;
                }
                if (NationsPlugin.file.UninMinister == (long)Context.Player.SteamUserId)
                {

                    SendMessage("[CrunchEcon]", "Account balance :" + String.Format("{0:n0}", EconUtils.getBalance(account)), Color.Green, (long)Context.Player.SteamUserId);
                    return;

                }
                else
                {
                    SendMessage("[CrunchEcon]", "You are not the trade minister. You cannot see balance.", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }

            }
            if (playerFac.Description.Contains("CONS"))
            {
                IMyFaction nation = MySession.Static.Factions.TryGetFactionByTag("CONS");
                long account = 0;
                foreach (KeyValuePair<long, MyFactionMember> m in nation.Members)
                {
                    if (m.Value.IsFounder)
                    {
                        account = m.Value.PlayerId;
                        break;
                    }
                }
                if (account == 0)
                {
                    Context.Respond("Couldnt find the account, tell Crunch");
                    return;
                }
                if (NationsPlugin.file.ConsMinister == (long)Context.Player.SteamUserId)
                {

                    SendMessage("[CrunchEcon]", "Account balance :" + String.Format("{0:n0}", EconUtils.getBalance(account)), Color.Green, (long)Context.Player.SteamUserId);
                    return;

                }
                else
                {
                    SendMessage("[CrunchEcon]", "You are not the trade minister. You cannot see balance.", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }
            }
            if (playerFac.Description.Contains("FEDR"))
            {
                IMyFaction nation = MySession.Static.Factions.TryGetFactionByTag("FEDR");
                long account = 0;
                foreach (KeyValuePair<long, MyFactionMember> m in nation.Members)
                {
                    if (m.Value.IsFounder)
                    {
                        account = m.Value.PlayerId;
                        break;
                    }
                }
                if (account == 0)
                {
                    Context.Respond("Couldnt find the account, tell Crunch");
                    return;
                }
                if (NationsPlugin.file.FedrMinister == (long)Context.Player.SteamUserId)
                {

                    SendMessage("[CrunchEcon]", "Account balance :" + String.Format("{0:n0}", EconUtils.getBalance(account)), Color.Green, (long)Context.Player.SteamUserId);
                    return;

                }
                else
                {
                    SendMessage("[CrunchEcon]", "You are not the trade minister. You cannot withdraw.", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }
            }
        }
        [Command("nation withdraw", "moneys")]
        [Permission(MyPromoteLevel.None)]
        public void nationWithdraw(string inputAmount)
        {
            if (Context.Player == null)
            {

                return;
            }
            Int64 depositAmount;
            inputAmount = inputAmount.Replace(",", "");
            inputAmount = inputAmount.Replace(".", "");
            inputAmount = inputAmount.Replace(" ", "");
            try
            {
                depositAmount = Int64.Parse(inputAmount);
            }
            catch (Exception)
            {
                SendMessage("[CrunchEcon]", "Error parsing amount", Color.Red, (long)Context.Player.SteamUserId);
                return;
            }
            if (depositAmount < 0 || depositAmount == 0)
            {
                SendMessage("[CrunchEcon]", "Must be a positive number", Color.Red, (long)Context.Player.SteamUserId);
                return;
            }
            if (EconUtils.getBalance(Context.Player.Identity.IdentityId) < depositAmount)
            {
                SendMessage("[CrunchEcon]", "You cant afford to deposit that much!", Color.Red, (long)Context.Player.SteamUserId);
                return;
            }
            IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
            if (playerFac.Description.Contains("UNIN"))
            {
                IMyFaction nation = MySession.Static.Factions.TryGetFactionByTag("UNIN");
                long account = 0;
                foreach (KeyValuePair<long, MyFactionMember> m in nation.Members)
                {
                    if (m.Value.IsFounder)
                    {
                        account = m.Value.PlayerId;
                        break;
                    }
                }
                if (account == 0)
                {
                    Context.Respond("Couldnt find the account, tell Crunch");
                    return;
                }
                if (NationsPlugin.file.UninMinister == (long)Context.Player.SteamUserId)
                {
                    if (EconUtils.getBalance(account) >= depositAmount)
                    {
                        EconUtils.addMoney(Context.Player.Identity.IdentityId, depositAmount);
                        EconUtils.takeMoney(account, depositAmount);
                        SendMessage("[CrunchEcon]", "Withdrawn. New balance :" + String.Format("{0:n0}", EconUtils.getBalance(account)), Color.Green, (long)Context.Player.SteamUserId);
                        return;
                    }
                    else
                    {
                        SendMessage("[CrunchEcon]", "Cannot withdraw more than is in account. Account balance :" + String.Format("{0:n0}", EconUtils.getBalance(account)), Color.Red, (long)Context.Player.SteamUserId);
                        return;
                    }
                }
                else
                {
                    SendMessage("[CrunchEcon]", "You are not the trade minister. You cannot withdraw.", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }

            }
            if (playerFac.Description.Contains("CONS"))
            {
                IMyFaction nation = MySession.Static.Factions.TryGetFactionByTag("CONS");
                long account = 0;
                foreach (KeyValuePair<long, MyFactionMember> m in nation.Members)
                {
                    if (m.Value.IsFounder)
                    {
                        account = m.Value.PlayerId;
                        break;
                    }
                }
                if (account == 0)
                {
                    Context.Respond("Couldnt find the account, tell Crunch");
                    return;
                }
                if (NationsPlugin.file.ConsMinister == (long)Context.Player.SteamUserId)
                {
                    if (EconUtils.getBalance(account) >= depositAmount)
                    {
                        EconUtils.addMoney(Context.Player.Identity.IdentityId, depositAmount);
                        EconUtils.takeMoney(account, depositAmount);
                        SendMessage("[CrunchEcon]", "Withdrawn. New balance :" + String.Format("{0:n0}", EconUtils.getBalance(account)), Color.Green, (long)Context.Player.SteamUserId);
                        return;
                    }
                    else
                    {
                        SendMessage("[CrunchEcon]", "Cannot withdraw more than is in account. Account balance :" + String.Format("{0:n0}", EconUtils.getBalance(account)), Color.Red, (long)Context.Player.SteamUserId);
                        return;
                    }
                }
                else
                {
                    SendMessage("[CrunchEcon]", "You are not the trade minister. You cannot withdraw.", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }
            }
            if (playerFac.Description.Contains("FEDR"))
            {
                IMyFaction nation = MySession.Static.Factions.TryGetFactionByTag("FEDR");
                long account = 0;
                foreach (KeyValuePair<long, MyFactionMember> m in nation.Members)
                {
                    if (m.Value.IsFounder)
                    {
                        account = m.Value.PlayerId;
                        break;
                    }
                }
                if (account == 0)
                {
                    Context.Respond("Couldnt find the account, tell Crunch");
                    return;
                }
                if (NationsPlugin.file.FedrMinister == (long)Context.Player.SteamUserId)
                {
                    if (EconUtils.getBalance(account) >= depositAmount)
                    {
                        EconUtils.addMoney(Context.Player.Identity.IdentityId, depositAmount);
                        EconUtils.takeMoney(account, depositAmount);
                        SendMessage("[CrunchEcon]", "Withdrawn. New balance :" + String.Format("{0:n0}", EconUtils.getBalance(account)), Color.Green, (long)Context.Player.SteamUserId);
                        return;
                    }
                    else
                    {
                        SendMessage("[CrunchEcon]", "Cannot withdraw more than is in account. Account balance :" + String.Format("{0:n0}", EconUtils.getBalance(account)), Color.Red, (long)Context.Player.SteamUserId);
                        return;
                    }
                }
                else
                {
                    SendMessage("[CrunchEcon]", "You are not the trade minister. You cannot withdraw.", Color.Red, (long)Context.Player.SteamUserId);
                    return;
                }
            }
        }
        [Command("nation deposit", "moneys")]
        [Permission(MyPromoteLevel.None)]
        public void nationDeposit(string inputAmount)
        {
            if (Context.Player == null)
            {

                return;
            }
            Int64 depositAmount;
            inputAmount = inputAmount.Replace(",", "");
            inputAmount = inputAmount.Replace(".", "");
            inputAmount = inputAmount.Replace(" ", "");
            try
            {
                depositAmount = Int64.Parse(inputAmount);
            }
            catch (Exception)
            {
                SendMessage("[CrunchEcon]", "Error parsing amount", Color.Red, (long)Context.Player.SteamUserId);
                return;
            }
            if (depositAmount < 0 || depositAmount == 0)
            {
                SendMessage("[CrunchEcon]", "Must be a positive number", Color.Red, (long)Context.Player.SteamUserId);
                return;
            }
            if (EconUtils.getBalance(Context.Player.Identity.IdentityId) < depositAmount)
            {
                SendMessage("[CrunchEcon]", "You cant afford to deposit that much!", Color.Red, (long)Context.Player.SteamUserId);
                return;
            }
            IMyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
            if (playerFac.Description.Contains("UNIN"))
            {
                IMyFaction nation = MySession.Static.Factions.TryGetFactionByTag("UNIN");
                long account = 0;
                foreach (KeyValuePair<long, MyFactionMember> m in nation.Members)
                {
                    if (m.Value.IsFounder)
                    {
                        account = m.Value.PlayerId;
                        break;
                    }
                }
                if (account == 0)
                {
                    Context.Respond("Couldnt find the account, tell Crunch");
                    return;
                }
                EconUtils.takeMoney(Context.Player.Identity.IdentityId, depositAmount);
                EconUtils.addMoney(account, depositAmount);
                SendMessage("[CrunchEcon]", "Deposited", Color.Green, (long)Context.Player.SteamUserId);
                return;
            }
            if (playerFac.Description.Contains("CONS"))
            {
                IMyFaction nation = MySession.Static.Factions.TryGetFactionByTag("CONS");
                long account = 0;
                foreach (KeyValuePair<long, MyFactionMember> m in nation.Members)
                {
                    if (m.Value.IsFounder)
                    {
                        account = m.Value.PlayerId;
                        break;
                    }
                }
                if (account == 0)
                {
                    Context.Respond("Couldnt find the account, tell Crunch");
                    return;
                }
                EconUtils.takeMoney(Context.Player.Identity.IdentityId, depositAmount);
                EconUtils.addMoney(account, depositAmount);
                SendMessage("[CrunchEcon]", "Deposited", Color.Green, (long)Context.Player.SteamUserId);
            }
            if (playerFac.Description.Contains("FEDR"))
            {
                IMyFaction nation = MySession.Static.Factions.TryGetFactionByTag("FEDR");
                long account = 0;
                foreach (KeyValuePair<long, MyFactionMember> m in nation.Members)
                {
                    if (m.Value.IsFounder)
                    {
                        account = m.Value.PlayerId;
                        break;
                    }
                }
                if (account == 0)
                {
                    Context.Respond("Couldnt find the account, tell Crunch");
                    return;
                }
                EconUtils.takeMoney(Context.Player.Identity.IdentityId, depositAmount);
                EconUtils.addMoney(account, depositAmount);
                SendMessage("[CrunchEcon]", "Deposited", Color.Green, (long)Context.Player.SteamUserId);
            }
        }
        [Command("nation add", "add nation to whitelist file")]
        [Permission(MyPromoteLevel.None)]
        public void addToDescriptionFile(string nation, string tag)
        {
            try
            {
                if (Context.Player != null)
                {
                    Context.Respond("Console only");
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
                switch (nation.ToUpper())
                {
                    case "FEDR":
                        if (NationsPlugin.FEDR.factions.ContainsKey(fac2.FactionId))
                        {
                            NationsPlugin.FEDR.factions.Remove(fac2.FactionId);
                        }
                        NationsPlugin.FEDR.factions.Add(fac2.FactionId, fac2.Tag);
                        NationsPlugin.SaveWhitelist("FEDR", NationsPlugin.FEDR);
                        break;
                    case "CONS":
                        if (NationsPlugin.CONS.factions.ContainsKey(fac2.FactionId))
                        {
                            NationsPlugin.CONS.factions.Remove(fac2.FactionId);
                        }
                        NationsPlugin.CONS.factions.Add(fac2.FactionId, fac2.Tag);
                        NationsPlugin.SaveWhitelist("CONS", NationsPlugin.CONS);
                        break;
                    case "UNIN":
                        if (NationsPlugin.UNIN.factions.ContainsKey(fac2.FactionId))
                        {
                            NationsPlugin.UNIN.factions.Remove(fac2.FactionId);
                        }
                        NationsPlugin.CONS.factions.Add(fac2.FactionId, fac2.Tag);
                        NationsPlugin.SaveWhitelist("UNIN", NationsPlugin.UNIN);
                        break;
                }
                Context.Respond("Added to the whitelist file, remember to do it on all servers!");
            }
            catch (Exception e)
            {
                Context.Respond(e.ToString());
                throw;
            }
        }
        [Command("nation remove", "add nation to whitelist file")]
        [Permission(MyPromoteLevel.None)]
        public void removeFromFile(string nation, string tag)
        {
            try
            {
                if (Context.Player != null)
                {
                    Context.Respond("Console only");
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
                switch (nation.ToUpper())
                {
                    case "FEDR":
                        NationsPlugin.FEDR.factions.Remove(fac2.FactionId);
                        NationsPlugin.SaveWhitelist("FEDR", NationsPlugin.FEDR);
                        break;
                    case "CONS":
                        NationsPlugin.CONS.factions.Remove(fac2.FactionId);
                        NationsPlugin.SaveWhitelist("CONS", NationsPlugin.CONS);
                        break;
                    case "UNIN":
                        NationsPlugin.UNIN.factions.Remove(fac2.FactionId);
                        NationsPlugin.SaveWhitelist("UNIN", NationsPlugin.UNIN);
                        break;
                }
                Context.Respond("Removed from the whitelist file, remember to do it on all servers!");
            }
            catch (Exception e)
            {
                Context.Respond(e.ToString());
                throw;
            }
        }
        //[Command("nation add", "send a reputation request")]
        //[Permission(MyPromoteLevel.None)]
        //public void addToDescription(string nation, string tag)
        //{
        //    try
        //    {
        //        if (Context.Player != null && !Context.Player.IsAdmin && Context.Player.SteamUserId != 76561198045390854)
        //        {
        //            return;
        //        }
        //        MyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(nation);
        //        if (fac2 == null)
        //        {
        //            Context.Respond("Cant find that nation.", Color.Red, "The Government");
        //            return;
        //        }
        //        IMyFaction fac3 = MySession.Static.Factions.TryGetFactionByTag(tag);
        //        if (fac3 == null)
        //        {
        //            Context.Respond("Cant find that faction.", Color.Red, "The Government");
        //            return;
        //        }

        //        String description = fac2.Description;
        //        fac2.Description = description + "\n[" + fac3.Tag.ToUpper() + "] # " + fac3.Name;
        //        //  Context.Respond("Worked, though keen needs a relog to see changes :(");
        //        MyFactionCollection.GetDefinitionIdsByIconName(fac2.FactionIcon.Value.String, out SerializableDefinitionId? factionIconGroupId, out int factionIconId);
        //        MySession.Static.Factions.EditFaction(fac2.FactionId, fac2.Tag, fac2.Name, fac2.Description, fac2.PrivateInfo, factionIconGroupId, factionIconId, fac2.CustomColor, fac2.IconColor);
        //    }
        //    catch (Exception e)
        //    {
        //        Context.Respond(e.ToString());
        //        throw;
        //    }
        //}
        //[Command("nation remove", "send a reputation request")]
        //[Permission(MyPromoteLevel.None)]
        //public void removeFromDescription(string nation, string tag, string name = "")
        //{
        //    if (Context.Player != null && !Context.Player.IsAdmin && Context.Player.SteamUserId != 76561198045390854)
        //    {
        //        return;
        //    }
        //    MyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(nation);
        //    if (fac2 == null)
        //    {
        //        Context.Respond("Cant find that nation.", Color.Red, "The Government");
        //        return;
        //    }
        //    IMyFaction fac3 = MySession.Static.Factions.TryGetFactionByTag(tag);
        //    if (fac3 == null)
        //    {
        //        Context.Respond("Cant find that faction.", Color.Red, "The Government");
        //        return;
        //    }
        //    String description = fac2.Description;

        //    if (name != "")
        //    {
        //            fac2.Description = fac2.Description.Replace("[" + tag.ToUpper() + "] # " + name, "");
        //        MyFactionCollection.GetDefinitionIdsByIconName(fac2.FactionIcon.Value.String, out SerializableDefinitionId? factionIconGroupId, out int factionIconId);
        //        MySession.Static.Factions.EditFaction(fac2.FactionId, fac2.Tag, fac2.Name, fac2.Description, fac2.PrivateInfo, factionIconGroupId, factionIconId, fac2.CustomColor, fac2.IconColor);
        //    } 
        //    else
        //    {
        //        if (fac2.Description.Contains("[" + fac3.Tag.ToUpper() + "]")){
        //            fac2.Description = fac2.Description.Replace("[" + tag.ToUpper() + "] # " + name, "");
        //            MyFactionCollection.GetDefinitionIdsByIconName(fac2.FactionIcon.Value.String, out SerializableDefinitionId? factionIconGroupId, out int factionIconId);
        //            MySession.Static.Factions.EditFaction(fac2.FactionId, fac2.Tag, fac2.Name, fac2.Description, fac2.PrivateInfo, factionIconGroupId, factionIconId, fac2.CustomColor, fac2.IconColor);
        //        }
        //        else
        //        {
        //            Context.Respond("Cant seem to find that, try !nation remove TAG NAME");
        //        }
        //    }

        //  //  new SerializableDefinitionId?(fac2.FactionIcon.Value.m_id)
        //    Context.Respond("Worked, though keen needs a relog to see changes :(");
        ////    MyFactionDefinition factionDefinition = MyDefinitionManager.Static.TryGetFactionDefinition(fac2.Tag);

        //    //MySession.Static.Factions.EditFaction(fac2.FactionId, fac2.Tag, fac2.Name, fac2.Description, fac2.PrivateInfo, , fac2.FactionIcon.Value.Id, fac2.CustomColor, fac2.IconColor);

        //}
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

            StringBuilder sb = new StringBuilder();
            switch (tag.ToUpper())
            {
                case "FEDR":
                    foreach (KeyValuePair<long, string> pair in NationsPlugin.FEDR.factions)
                    {
                        if (MySession.Static.Factions.TryGetFactionById(pair.Key) != null)
                        {
                            sb.Append(MySession.Static.Factions.TryGetFactionById(pair.Key).Name + " " + MySession.Static.Factions.TryGetFactionById(pair.Key).Tag + "\n");
                        }
                    }
                    break;
                case "CONS":
                    foreach (KeyValuePair<long, string> pair in NationsPlugin.CONS.factions)
                    {
                        if (MySession.Static.Factions.TryGetFactionById(pair.Key) != null)
                        {
                            sb.Append(MySession.Static.Factions.TryGetFactionById(pair.Key).Name + " " + MySession.Static.Factions.TryGetFactionById(pair.Key).Tag + "\n");
                        }
                    }
                    break;
                case "UNIN":
                    foreach (KeyValuePair<long, string> pair in NationsPlugin.UNIN.factions)
                    {
                        if (MySession.Static.Factions.TryGetFactionById(pair.Key) != null)
                        {
                            sb.Append(MySession.Static.Factions.TryGetFactionById(pair.Key).Name + " " + MySession.Static.Factions.TryGetFactionById(pair.Key).Tag + "\n");
                        }
                    }
                    break;

            }
            if (!console)
            {
                DialogMessage m = new DialogMessage("Faction Info", fac.Name, "\nTag: " + fac.Tag + "\nMembers: " + sb.ToString());
                ModCommunication.SendMessageTo(m, Context.Player.SteamUserId);
            }
            else
            {
                Context.Respond("Name: " + fac.Name + "\nTag: " + fac.Tag + "\nMembers: " + sb.ToString());
            }
            return;



        }


        public void sendNexusChatMessage(string message, long SteamId)
        {
            // Sockets.Publish
            //



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

                Players.Add(new Player((string)obj[0], (ulong)obj[1], (long)obj[2], (int)obj[3]));
            }
            Dictionary<int, String> online = new Dictionary<int, string>();
            StringBuilder sb = new StringBuilder();
            int max = 0;
            foreach (Player player in Players)
            {
                if (player.OnServer > max)
                {
                    max = player.OnServer;
                }
            }
            for (int i = 0; i >= max; i++)
            {
                online.Add(i, "");
            }
            foreach (Player player in Players)
            {
                IMyFaction fac = FacUtils.GetPlayersFaction(player.IdentityID);
                if (fac != null && fac.Description != null && fac.Description.Contains(nation))
                {
                    // sb.Append(player.OnServer + " # " + player.PlayerName + "\n");
                    if (online.ContainsKey(player.OnServer))
                    {
                        online.TryGetValue(player.OnServer, out string temp);
                        temp += ", " + player.PlayerName;
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
                sb.Append("\n" + "Sector " + pairs.Key + " - " + pairs.Value + "\n");
            }
            return sb.ToString();
        }

        [Command("friend", "send a friend request to a faction")]
        [Permission(MyPromoteLevel.None)]
        public void sendRep(string factionTag)
        {


            IMyFaction fac2 = MySession.Static.Factions.TryGetFactionByTag(factionTag);
            if (fac2 == null)
            {
                Context.Respond("Cant find that faction.", Color.Red, "The Government");
                return;
            }
            IMyFaction fac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
            if (fac != null)
            {
                if (fac.IsLeader(Context.Player.IdentityId) || fac.IsFounder(Context.Player.IdentityId))
                {
                    RepRequest reqs;
                    if (repRequests.ContainsKey(fac.FactionId))
                    {
                        repRequests.TryGetValue(fac.FactionId, out RepRequest temp);
                        reqs = temp;
                        if (reqs.hasRequest(fac2.Tag))
                        {
                            reqs.removeRequest(fac2.Tag);
                            Context.Respond("Accepting request, becoming peaceful, and Changing reputation!, relog may be required to see effects.");
                            //do the friend shit


                            // MySession.Static.Factions.DisplayReputationChangeNotification((MySession.Static.Factions.TryGetFactionById(fac.FactionId) as MyFaction).Tag, change.Change);



                            MyFactionPeaceRequestState state = MySession.Static.Factions.GetRequestState(fac.FactionId, fac2.FactionId);


                            if (state == MyFactionPeaceRequestState.Pending || state == MyFactionPeaceRequestState.Sent)
                            {
                                Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(fac.FactionId, fac2.FactionId);
                            }
                            DoFriendlyUpdate(fac.FactionId, fac2.FactionId);
                            MySession.Static.Factions.SetReputationBetweenFactions(fac.FactionId, fac2.FactionId, 1500);
                            foreach (KeyValuePair<long, MyFactionMember> m in fac.Members)
                            {
                                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, fac2.FactionId, 0);
                                MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, fac2.FactionId, 0, true, true);
                            }
                            foreach (KeyValuePair<long, MyFactionMember> m in fac2.Members)
                            {
                                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, fac2.FactionId, 0);

                                MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, fac.FactionId, 0, true, true);

                            }

                            repRequests.Remove(fac.FactionId);
                            repRequests.Add(fac.FactionId, reqs);
                        }
                    }
                    else
                    {
                        reqs = new RepRequest();
                        Context.Respond("Sending a request!");
                        reqs.addRequest(fac.Tag, 0);
                        MyFactionPeaceRequestState state = MySession.Static.Factions.GetRequestState(fac.FactionId, fac2.FactionId);


                        if (state != MyFactionPeaceRequestState.Sent)
                        {
                            Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(fac.FactionId, fac2.FactionId);
                        }
                        repRequests.Remove(fac2.FactionId);
                        repRequests.Add(fac2.FactionId, reqs);
                        foreach (MyFactionMember mb in fac2.Members.Values)
                        {


                            ulong steamid = MySession.Static.Players.TryGetSteamId(mb.PlayerId);

                            if (steamid == 0)
                            {
                                break;
                            }
                            SendMessage("Friend Request", Context.Player.DisplayName + " wishes to be friends with you, if you accept this will set you to at peace and make them untargetable by weaponcore.", Color.DarkGreen, (long)steamid);
                            SendMessage("Friend Request", " To accept use !friend " + fac.Tag, Color.DarkGreen, (long)steamid);



                        }
                    }

                }
                else
                {
                    Context.Respond("Only leaders and founders can send friend requests.");
                    return;
                }
            }
            else
            {
                Context.Respond("You dont have a faction!");
            }

        }

        [Command("nation online", "online members")]
        [Permission(MyPromoteLevel.None)]
        public void getonlinenation(string nation = "")
        {
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

            }
        }


        [Command("admindistress", "admindistress signals")]
        [Permission(MyPromoteLevel.Admin)]
        public void admindistress(string name, string messagename = "", string message = "")
        {


            if (Context.Player == null)
            {
                Context.Respond("no no console no distress");
                return;
            }

            if (messagename.Equals(""))
            {
                messagename = NationsPlugin.file.AdminDistressName;
            }
            if (message.Equals(""))
            {
                message = NationsPlugin.file.AdminDistressMessage;
            }
            MyGps gps = CreateGps(Context.Player.Character.PositionComp.GetPosition(), new Color(NationsPlugin.file.adminDistressRed, NationsPlugin.file.adminDistressGreen, NationsPlugin.file.AdminDistressBlue), 600, name, "");
            MyGps gpsRef = gps;
            long entityId = 0L;
            entityId = gps.EntityId;

            MyGpsCollection gpsCollection = (MyGpsCollection)MyAPIGateway.Session?.GPS;
            foreach (MyPlayer p in MySession.Static.Players.GetOnlinePlayers())
            {
                gpsCollection.SendAddGps(p.Identity.IdentityId, ref gpsRef, entityId, true);
                // NationsPlugin.signalsToClear.Add(gps, DateTime.Now.AddMilliseconds(NationsPlugin.file.MillisecondsTimeItLasts));
                SendMessage(messagename, message, Color.Red, (long)p.Id.SteamId);
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
                    if (NationsPlugin.file.doWhitelist)
                    {

                        if (!NationsPlugin.UNIN.factions.ContainsKey(playerFac.FactionId))
                        {
                            Context.Respond("You havent been added to the whitelist so you cannot use the !nationjoin.", Color.Red, "The Government");
                            return;
                        }


                    }
                    doSignal(Context.Player.Character.GetPosition(), "UNIN", NationsPlugin.file.UNIN, reason);
                    EconUtils.takeMoney(Context.Player.IdentityId, NationsPlugin.file.Price);
                    Context.Respond("Signal sent! You were charged " + String.Format("{0:n0}", NationsPlugin.file.Price) + " SC for the convenience.", Color.Orange, NationsPlugin.file.Name);

                    return;
                }
                if (playerFac.Description.Contains("CONS"))
                {
                    if (NationsPlugin.file.doWhitelist)
                    {


                        if (!NationsPlugin.CONS.factions.ContainsKey(playerFac.FactionId))
                        {
                            Context.Respond("You havent been added to the whitelist so you cannot use the !nationjoin.", Color.Red, "The Government");
                            return;
                        }



                    }
                    doSignal(Context.Player.Character.GetPosition(), "CONS", NationsPlugin.file.CONS, reason);
                    EconUtils.takeMoney(Context.Player.IdentityId, NationsPlugin.file.Price);
                    Context.Respond("Signal sent! You were charged " + String.Format("{0:n0}", NationsPlugin.file.Price) + " SC for the convenience.", Color.Orange, NationsPlugin.file.Name);
                    return;
                }
                if (playerFac.Description.Contains("FEDR"))
                {
                    if (NationsPlugin.file.doWhitelist)
                    {
                        if (!NationsPlugin.FEDR.factions.ContainsKey(playerFac.FactionId))
                        {
                            Context.Respond("You havent been added to the whitelist so you cannot use the !nationjoin.", Color.Red, "The Government");
                            return;
                        }

                    }
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
        public void reloadconfig()
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


        [Command("dividend", "give money to members of a nation")]
        [Permission(MyPromoteLevel.Admin)]
        public void scan(string nation, string inputAmount)
        {
            int count = 0;
            StringBuilder response = new StringBuilder();
            List<long> payTheseFucks = new List<long>();
            Int64 amount;
            inputAmount = inputAmount.Replace(",", "");
            inputAmount = inputAmount.Replace(".", "");
            inputAmount = inputAmount.Replace(" ", "");
            try
            {
                amount = Int64.Parse(inputAmount);
            }
            catch (Exception)
            {
                Context.Respond("Error parsing number");
                return;
            }
            if (amount < 0 || amount == 0)
            {
                Context.Respond("Input must be positive");
                return;
            }
            var cutoff = DateTime.Now - TimeSpan.FromDays(10);
            foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
            {

                if (f.Value.Description != null)
                {
                    //if (NationsPlugin.file.doWhitelist)
                    //{
                    //    IMyFaction nationfac = MySession.Static.Factions.TryGetFactionByTag(nation);

                    //    if (nationfac != null && f.Value.Description.Contains(nation.ToUpper()) && nationfac.Description != null && nationfac.Description.Contains("[" + f.Value.Tag.ToUpper() + "]"))
                    //    {
                    //        count += f.Value.Members.Count;
                    //        response.Append(f.Value.Name + " [" + f.Value.Tag + "] MEMBERS : " + f.Value.Members.Count + "\n");
                    //    }
                    //}
                    //else
                    {
                        if (f.Value.Description.Contains(nation.ToUpper()))
                        {
                            foreach (KeyValuePair<long, MyFactionMember> mem in f.Value.Members)
                            {
                                MyIdentity id = MySession.Static.Players.TryGetIdentity(mem.Value.PlayerId);
                                DateTime referenceTime = id.LastLoginTime;
                                if (id.LastLogoutTime > referenceTime)
                                    referenceTime = id.LastLogoutTime;
                                if (referenceTime >= cutoff)
                                {
                                    payTheseFucks.Add(mem.Value.PlayerId);

                                }

                            }
                        }
                    }
                }
            }
            long amountToPay = amount / payTheseFucks.Count();
            foreach (long id in payTheseFucks)
            {
                MyBankingSystem.ChangeBalance(id, amountToPay);
            }
            Context.Respond("Paid out " + String.Format("{0:n0}", amountToPay) + " SC to each member");

        }
        [Command("tax", "give money to members of a nation")]
        [Permission(MyPromoteLevel.Admin)]
        public void tax(string nation, string inputAmount)
        {
            int count = 0;
            StringBuilder response = new StringBuilder();
            List<long> payTheseFucks = new List<long>();
            Int64 amount;
            inputAmount = inputAmount.Replace(",", "");
            inputAmount = inputAmount.Replace(".", "");
            inputAmount = inputAmount.Replace(" ", "");
            try
            {
                amount = Int64.Parse(inputAmount);
            }
            catch (Exception)
            {
                Context.Respond("Error parsing number");
                return;
            }
            if (amount < 0 || amount == 0)
            {
                Context.Respond("Input must be positive");
                return;
            }
            var cutoff = DateTime.Now - TimeSpan.FromDays(10);
            foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
            {

                if (f.Value.Description != null)
                {
                    //if (NationsPlugin.file.doWhitelist)
                    //{
                    //    IMyFaction nationfac = MySession.Static.Factions.TryGetFactionByTag(nation);

                    //    if (nationfac != null && f.Value.Description.Contains(nation.ToUpper()) && nationfac.Description != null && nationfac.Description.Contains("[" + f.Value.Tag.ToUpper() + "]"))
                    //    {
                    //        count += f.Value.Members.Count;
                    //        response.Append(f.Value.Name + " [" + f.Value.Tag + "] MEMBERS : " + f.Value.Members.Count + "\n");
                    //    }
                    //}
                    //else
                    {
                        if (f.Value.Description.Contains(nation.ToUpper()))
                        {
                            foreach (KeyValuePair<long, MyFactionMember> mem in f.Value.Members)
                            {
                                MyIdentity id = MySession.Static.Players.TryGetIdentity(mem.Value.PlayerId);
                                DateTime referenceTime = id.LastLoginTime;
                                if (id.LastLogoutTime > referenceTime)
                                    referenceTime = id.LastLogoutTime;
                                if (referenceTime >= cutoff)
                                {
                                    payTheseFucks.Add(mem.Value.PlayerId);

                                }

                            }
                        }
                    }
                }
            }
            long amountToPay = amount / payTheseFucks.Count();
            foreach (long id in payTheseFucks)
            {
                EconUtils.takeMoney(id, amount);
            }
            Context.Respond("Paid out " + String.Format("{0:n0}", amountToPay) + " SC to each member");

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
                    if (NationsPlugin.file.doWhitelist)
                    {
                        IMyFaction nationfac = MySession.Static.Factions.TryGetFactionByTag(nation);

                        if (nationfac != null && f.Value.Description.Contains(nation.ToUpper()) && nationfac.Description != null && nationfac.Description.Contains("[" + f.Value.Tag.ToUpper() + "]"))
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
                        response += f.Value.Name + " " + f.Value.Tag + ", ";
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

        [Command("killrequests", "delete all join requests")]
        [Permission(MyPromoteLevel.None)]
        public void killrequests()
        {
            MyFaction playerFac = FacUtils.GetPlayersFaction(Context.Player.Identity.IdentityId) as MyFaction;
            if (playerFac == null)
            {
                Context.Respond("You dont have a faction.");
                return;
            }
            if (playerFac.IsLeader(Context.Player.IdentityId) || playerFac.IsFounder(Context.Player.IdentityId))
            {



                List<long> ids = new List<long>();
                foreach (KeyValuePair<long, MyFactionMember> id in playerFac.JoinRequests)
                {
                    ids.Add(id.Value.PlayerId);

                }
                foreach (long l in ids)
                {
                    playerFac.CancelJoinRequest(l);
                }
                MyPlayer test = Context.Player as MyPlayer;

                MySession.Static.Factions.AddDiscoveredFaction(test.Id, playerFac.FactionId);
                Context.Respond("Yeeted!");
                MyFactionCollection.GetDefinitionIdsByIconName(playerFac.FactionIcon.Value.String, out SerializableDefinitionId? factionIconGroupId, out int factionIconId);
                MySession.Static.Factions.EditFaction(playerFac.FactionId, playerFac.Tag, playerFac.Name, playerFac.Description, playerFac.PrivateInfo, factionIconGroupId, factionIconId, playerFac.CustomColor, playerFac.IconColor);
            }
            else
            {
                Context.Respond("You are not a faction leader or founder!");
                return;

            }
        }
        [Command("fixrep", "fixes negative reputation")]
        [Permission(MyPromoteLevel.None)]
        public void fixrep()
        {
            Context.Respond("No no, relog");
            return;
            foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
            {
                if (f.Value.Tag.Length > 3)
                {
                    if (!f.Value.Tag.Equals("SPRT") && !f.Value.Tag.Equals("MERC") && !f.Value.Tag.Equals("EROR") && !f.Value.Tag.Equals("MEOW"))
                    {
                        System.Tuple<MyRelationsBetweenFactions, int> rep = MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(Context.Player.IdentityId, f.Value.FactionId);
                        if (rep.Item2 < 0)
                        {
                            MySession.Static.Factions.AddFactionPlayerReputation(Context.Player.IdentityId, f.Value.FactionId, Math.Abs(rep.Item2) * 2, true, true);
                        }
                    }
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
        [Command("koth static delete", "delete static grids at target koth")]
        [Permission(MyPromoteLevel.Admin)]
        public void DeleteStatics(int radius, int max, double x, double y, double z)
        {
            Dictionary<string, int> nationStaticGrids = new Dictionary<string, int>();


            BoundingSphereD sphere = new BoundingSphereD(new Vector3D(x, y, z), radius);

            foreach (MyCubeGrid grid in MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere).OfType<MyCubeGrid>())
            {
                if (grid.IsStatic)
                {
            

                    if (FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid)) != null)
                    {
                        IMyFaction fac = FacUtils.GetPlayersFaction(FacUtils.GetOwner(grid));
                        if (fac.Tag.Length > 3)
                        {
                            continue;
                        }
                        if (max == 0)
                        {
                            var b = grid.GetFatBlocks<MyCockpit>();
               
                            foreach (var c in b)
                            {
                                c.RemovePilot();
                            }
                            grid.Close();
                            NationsPlugin.Log.Info("Deleting static grid for going over static limit at koth " + grid.DisplayNameText);
                            SendMessage("KOTH", "Deleting static grid for going over limits " + grid.DisplayNameText, Color.DarkRed, 0L);
                            continue;
                        }
                        if (GetNationTag(fac) != null)
                        {
                            string nation = GetNationTag(fac);
                            if (nationStaticGrids.TryGetValue(nation, out int value))
                            {

                                if (value >= max)
                                {
                                    var b = grid.GetFatBlocks<MyCockpit>();
                                    foreach (var c in b)
                                    {
                                        c.RemovePilot();
                                    }
                                    grid.Close();
                                    NationsPlugin.Log.Info("Deleting static grid for going over static limit at koth " + grid.DisplayNameText);
                                    SendMessage("KOTH", "Deleting static grid for going over limits " + grid.DisplayNameText, Color.DarkRed, 0L);
                                }
                                else
                                {
                                    nationStaticGrids[nation]++;
                                }
                            }
                            else
                            {
                                nationStaticGrids.Add(nation, 1);
                            }
                        }else {
                            if (max == 0)
                            {
                                var b = grid.GetFatBlocks<MyCockpit>();
                                foreach (var c in b)
                                {
                                    c.RemovePilot();
                                }
                                grid.Close();
                                NationsPlugin.Log.Info("Deleting static grid for going over static limit at koth " + grid.DisplayNameText);
                                SendMessage("KOTH", "Deleting static grid for going over limits " + grid.DisplayNameText, Color.DarkRed, 0L);
                                continue;
                            }
                            if (nationStaticGrids.TryGetValue(fac.Tag, out int value))
                            {

                                if (value >= max)
                                {
                                    var b = grid.GetFatBlocks<MyCockpit>();
                                    foreach (var c in b)
                                    {
                                        c.RemovePilot();
                                    }
                                    grid.Close();
                                    NationsPlugin.Log.Info("Deleting static grid for going over static limit at koth " + grid.DisplayNameText);
                                    SendMessage("KOTH", "Deleting static grid for going over limits " + grid.DisplayNameText, Color.DarkRed, 0L);
                                }
                                else
                                {
                                    nationStaticGrids[fac.Tag]++;
                                }
                            }
                            else
                            {
                                nationStaticGrids.Add(fac.Tag, 1);
                            }
                        }
                    }
                    else
                    {
                        var b = grid.GetFatBlocks<MyCockpit>();
                        foreach (var c in b)
                        {
                            c.RemovePilot();
                        }
                        grid.Close();
                        NationsPlugin.Log.Info("Deleting static grid for having no faction at koth " + grid.DisplayNameText);
                        SendMessage("KOTH", "Deleting static grid for having no faction at koth " + grid.DisplayNameText, Color.DarkRed, 0L);
                    }
                }
            }

        }

        [Command("nationtest", "force friendly with target")]
        [Permission(MyPromoteLevel.Admin)]
        public void forceFriendlt(string tag)
        {

            IMyFaction playerfac = FacUtils.GetPlayersFaction(Context.Player.IdentityId);
            IMyFaction target = MySession.Static.Factions.TryGetFactionByTag(tag);
            MySession.Static.Factions.SetReputationBetweenFactions(playerfac.FactionId, target.FactionId, 1500);
            MyFactionStateChange change = MyFactionStateChange.SendFriendRequest;
            MyFactionStateChange change2 = MyFactionStateChange.AcceptFriendRequest;
            Type FactionCollection = MySession.Static.Factions.GetType().Assembly.GetType("Sandbox.Game.Multiplayer.MyFactionCollection");
            MethodInfo sendChange = FactionCollection?.GetMethod("SendFactionChange", BindingFlags.NonPublic | BindingFlags.Static);
            List<object[]> ReturnPlayers = new List<object[]>();
            object[] MethodInput = new object[] { change, target.FactionId, playerfac.FactionId, 0L };

            sendChange?.Invoke(null, MethodInput);
            object[] MethodInput2 = new object[] { change2, playerfac.FactionId, target.FactionId, 0L };
            sendChange?.Invoke(null, MethodInput2);
        }

        public void DoFriendlyUpdate(long firstId, long SecondId)
        {
         MyFactionStateChange change = MyFactionStateChange.SendFriendRequest;
        MyFactionStateChange change2 = MyFactionStateChange.AcceptFriendRequest;
            List<object[]> Input = new List<object[]>();
            object[] MethodInput = new object[] { change, firstId, SecondId, 0L };
            NationsPlugin.sendChange?.Invoke(null, MethodInput);
            object[] MethodInput2 = new object[] { change2, SecondId, firstId, 0L };
            NationsPlugin.sendChange?.Invoke(null, MethodInput2);
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
                switch (nation.Tag.ToUpper())
                {
                    case "FEDR":
                        if (!NationsPlugin.FEDR.factions.ContainsKey(playerFac.FactionId))
                        {
                            Context.Respond("You havent been added to the whitelist so you cannot use the !nationjoin.", Color.Red, "The Government");
                            return;
                        }
                        break;
                    case "CONS":
                        if (!NationsPlugin.CONS.factions.ContainsKey(playerFac.FactionId))
                        {
                            Context.Respond("You havent been added to the whitelist so you cannot use the !nationjoin.", Color.Red, "The Government");
                            return;
                        }
                        break;
                    case "UNIN":
                        if (!NationsPlugin.UNIN.factions.ContainsKey(playerFac.FactionId))
                        {
                            Context.Respond("You havent been added to the whitelist so you cannot use the !nationjoin.", Color.Red, "The Government");
                            return;
                        }
                        break;
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
            if (playerFac.IsLeader(Context.Player.IdentityId) || playerFac.IsFounder(Context.Player.IdentityId))
            {

                if (playerFac.Description.Contains(tag))
                {

                    foreach (KeyValuePair<long, MyFaction> f in MySession.Static.Factions)
                    {
                        if (f.Value != null && f.Value != playerFac)
                        {
                            if (f.Value.Description != null && f.Value.Description.Contains(tag) && f.Value.Tag.Length == 3)
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

                                            }
                                            if (state == MyFactionPeaceRequestState.Pending)
                                            {
                                                Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, f.Value.FactionId);
                                                logThis.Append("NATION REQUESTS - Accepting peace reqest between " + playerFac.Name + " " + playerFac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);
                                                MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);

                                                DoFriendlyUpdate(playerFac.FactionId, f.Value.FactionId);
                                                foreach (KeyValuePair<long, MyFactionMember> m in playerFac.Members)
                                                {

                                                    System.Tuple<MyRelationsBetweenFactions, int> rep = MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(m.Value.PlayerId, f.Value.FactionId);
                                                    if (rep.Item2 < 0)
                                                    {
                                                        MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, f.Value.FactionId, 0);
                                                        MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, f.Value.FactionId, 0, true, true);
                                                    }
                                                }

                                            }
                                            if (MySession.Static.Factions.AreFactionsNeutrals(playerFac.FactionId, f.Value.FactionId))
                                            {
                                                MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);
                                                DoFriendlyUpdate(playerFac.FactionId, f.Value.FactionId);
                                                foreach (KeyValuePair<long, MyFactionMember> m in playerFac.Members)
                                                {

                                                    System.Tuple<MyRelationsBetweenFactions, int> rep = MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(m.Value.PlayerId, f.Value.FactionId);
                                                    if (rep.Item2 < 0)
                                                    {
                                                        MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, f.Value.FactionId, 0);
                                                        MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, f.Value.FactionId, 0, true, true);
                                                    }

                                                }
                                            }
                                        }
                                    }

                                    else
                                    {
                                        if (state != MyFactionPeaceRequestState.Sent)
                                        {
                                            Sandbox.Game.Multiplayer.MyFactionCollection.SendPeaceRequest(playerFac.FactionId, f.Value.FactionId);
                                            logThis.Append("NATION REQUESTS - Sending peace reqest between " + playerFac.Name + " " + playerFac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);

                                        }
                                        if (state == MyFactionPeaceRequestState.Pending)
                                        {
                                            Sandbox.Game.Multiplayer.MyFactionCollection.AcceptPeace(playerFac.FactionId, f.Value.FactionId);
                                            logThis.Append("NATION REQUESTS - Accepting peace reqest between " + playerFac.Name + " " + playerFac.Tag + " and " + f.Value.Name + " " + f.Value.Tag);
                                            MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);
                                            DoFriendlyUpdate(playerFac.FactionId, f.Value.FactionId);
                                            foreach (KeyValuePair<long, MyFactionMember> m in playerFac.Members)
                                            {

                                                System.Tuple<MyRelationsBetweenFactions, int> rep = MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(m.Value.PlayerId, f.Value.FactionId);
                                                if (rep.Item2 < 0)
                                                {
                                                    MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, f.Value.FactionId, 0);
                                                    MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, f.Value.FactionId, 0, true, true);
                                                }
                                            }
                                        }
                                        if (MySession.Static.Factions.AreFactionsNeutrals(playerFac.FactionId, f.Value.FactionId))
                                        {
                                            DoFriendlyUpdate(playerFac.FactionId, f.Value.FactionId);
                                            MySession.Static.Factions.SetReputationBetweenFactions(playerFac.FactionId, f.Value.FactionId, 1500);

                                        }
                                        foreach (KeyValuePair<long, MyFactionMember> m in playerFac.Members)
                                        {
                                            System.Tuple<MyRelationsBetweenFactions, int> rep = MySession.Static.Factions.GetRelationBetweenPlayerAndFaction(m.Value.PlayerId, f.Value.FactionId);
                                            if (rep.Item2 < 0)
                                            {
                                                MySession.Static.Factions.SetReputationBetweenPlayerAndFaction(m.Value.PlayerId, f.Value.FactionId, 0);
                                                MySession.Static.Factions.AddFactionPlayerReputation(m.Value.PlayerId, f.Value.FactionId, 0, true, true);
                                            }

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
