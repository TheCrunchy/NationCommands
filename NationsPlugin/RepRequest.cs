using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NationsPlugin
{
   public class RepRequest
    {
        public Dictionary<String, int> requests = new Dictionary<string, int>();

        public void addRequest(String tag, int amount)
        {
            if (requests.ContainsKey(tag))
            {
                requests.Remove(tag);
            }
            requests.Add(tag, amount);
        }

        public int getRequestAmount(String tag)
        {
            if (requests.ContainsKey(tag))
            {
                requests.TryGetValue(tag, out int amount);
                return amount;
            }
            return 0;
        }

        public void removeRequest(String tag)
        {
            if (requests.ContainsKey(tag))
            {
                requests.Remove(tag);
            }
        }
    }
}
