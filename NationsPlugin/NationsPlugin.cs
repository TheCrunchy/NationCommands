using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;
using Torch.API;

namespace NationsPlugin
{
    public class NationsPlugin : TorchPluginBase
    {
        public static Logger Log = LogManager.GetCurrentClassLogger();
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            Log.Info("Dirk smells");
        }
        
    }
}
