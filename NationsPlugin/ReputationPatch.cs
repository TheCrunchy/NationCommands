using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;

namespace NationsPlugin
{
    [PatchShim]
    public static class ReputationPatch
    {

        internal static readonly MethodInfo update =
        typeof(MyFactionCollection).GetMethod("DamageFactionPlayerReputation", BindingFlags.Instance | BindingFlags.Public) ??
        throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo updatePatch =
                typeof(ReputationPatch).GetMethod(nameof(DamageFactionPlayerReputation), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo update2 =
       typeof(MyFactionCollection).GetMethod("AddFactionPlayerReputation", BindingFlags.Instance | BindingFlags.Public) ??
       throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo updatePatch2 =
                typeof(ReputationPatch).GetMethod(nameof(Log1), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo update3 =
typeof(MyFactionCollection).GetMethod("ChangeReputationWithPlayer", BindingFlags.Instance | BindingFlags.NonPublic) ??
throw new Exception("Failed to find patch method");
        internal static readonly MethodInfo updatePatch3 =
                typeof(ReputationPatch).GetMethod(nameof(Log2), BindingFlags.Static | BindingFlags.Public) ??
                throw new Exception("Failed to find patch method");

        public static void Patch(PatchContext ctx)
        {


            ctx.GetPattern(update).Prefixes.Add(updatePatch);
            ctx.GetPattern(update2).Prefixes.Add(updatePatch2);
            ctx.GetPattern(update3).Prefixes.Add(updatePatch3);
        }
        public static void Log1(long playerIdentityId,
      long factionId,
      int delta,
      bool propagate = true,
      bool adminChange = false)
        {
            NationsPlugin.Log.Info("Reputation logging - AddFactionPlayerRep -- Player: " + playerIdentityId + " faction:" + factionId + " amount:" + delta);
        }

        public static void Log2(long fromPlayerId, long toFactionId, int reputation)
        {
            NationsPlugin.Log.Info("Reputation logging - ChangeReputationWithPlayer -- Player: " + fromPlayerId + " faction:" + toFactionId + " amount:" + reputation);
        }
        public static Boolean DamageFactionPlayerReputation(
   long playerIdentityId,
   long attackedIdentityId,
   MyReputationDamageType repDamageType)
        {
            if (NationsPlugin.file != null && NationsPlugin.file.ReputationPatch)
            {

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
