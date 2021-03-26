using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch;

namespace NationsPlugin
{
    public class ConfigFile : ViewModel
    {
        public Int64 Price = 1000000;
        public string Name = "The Government";

        public string UNIN = "Union";
        public string FEDR = "Federation";
        public string CONS = "Consortium";
        public long ConsMinister = 0;
        public long FedrMinister = 0;
        public long UninMinister = 0;
        public int red = 252;
        public int green = 236;
        public int blue = 3;
        public int hostilered = 245;
        public int hostilegreen = 66;
        public int hostileblue = 66;
        public int adminDistressRed = 150;
        public int adminDistressGreen = 50;
        public int AdminDistressBlue = 150;
        public string AdminDistressMessage = "Sensors have picked up an unknown distress signal!";
        public string AdminDistressName = "Ship AI";
        public long CooldownMilliseconds = 300000;
        public long MillisecondsTimeItLasts = 300000;
        public int DetectionRangeForNearbyInKM = 10;
        public string Message = "Sensors have picked up a distress signal from a fellow citizen!";
        public string MessageName = "Ship AI";
        public string HostileMessage = "Sensors have picked up a distress signal from a nearby ship!";
        public string HostileMessageName = "Ship AI";
        public Boolean MessageEnabled = true;
        public Boolean OptionalAutomaticPeace = false;
        public ushort socketID = 0;
        public Boolean doWhitelist = false;
       // public Boolean RemoveOldOnNewSignal = true;

        public String toString()
        {
            String temp = "";
            temp += OptionalAutomaticPeace.ToString() + "\n";
            return temp;
        }
    }
}
