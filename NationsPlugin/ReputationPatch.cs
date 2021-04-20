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


        public static void Patch(PatchContext ctx)
        {


            ctx.GetPattern(update).Prefixes.Add(updatePatch);


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
