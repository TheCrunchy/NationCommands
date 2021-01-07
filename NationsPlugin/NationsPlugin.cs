using NLog;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Torch;
using Torch.API;

namespace NationsPlugin
{
    public class NationsPlugin : TorchPluginBase
    {
        public static ConfigFile file;
        public static Logger Log = LogManager.GetCurrentClassLogger();
        private static string path;
        public static Dictionary<long, CurrentCooldown> CurrentCooldownMap { get; } = new Dictionary<long, CurrentCooldown>();
        public static Dictionary<MyGps, DateTime> signalsToClear = new Dictionary<MyGps, DateTime>();
        private static Timer aTimer = new Timer();
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            Log.Info("Dirk smells");
            path = StoragePath;
            SetupConfig();
            aTimer.Enabled = false;
            aTimer.Interval = 30000;
            aTimer.Elapsed += OnTimedEventA;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        public static ConfigFile LoadConfig()
        {
            FileUtils utils = new FileUtils();
            file = utils.ReadFromXmlFile<ConfigFile>(path + "\\NationsConfig.xml");
     
            return file;
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
