using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supdate
{
    public class Version
    
    {
        /// <summary>
        /// Stores all the subversions in decending order. Null means always update so operators will return null which means update
        /// </summary>
        private List<int>? subversions;

        /// <summary>
        /// Valid verison numbers must be all ints and can be point seperated. E.g: 1.5.15.
        /// When comparing versions, the further left part is always more significant so E.g: 1.6.2 would be greater than 1.3.15
        /// </summary>
        /// <param name="version">The input version in the described format</param>
        /// <exception cref="Exception"></exception>
        public Version(string version)
        {
            string[] parts = version.Split('.');
            subversions = new List<int>();
            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int parsed))
                {
                    throw new Exception("Version number is not valid. Please look into the doks for further info.");
                }
                subversions.Add(parsed);
            }
        }
        /// <summary>
        /// Use this constructor to enable always update mode (Mostly used for installers)
        /// </summary>
        public Version()
        {
            subversions = null;
        }


        public bool AlwaysUpdate
        {
            get { return subversions == null; }
        }

        public static bool? operator <(Version a, Version b)
        {
            if (a.subversions == null || b.subversions == null)
                return null;

            for (int i = 0; i < Math.Max(a.subversions.Count, b.subversions.Count); i++)
            {
                int aSubversion;
                int bSubversion;
                if (i > a.subversions.Count)
                    aSubversion = 0;
                else
                    aSubversion = a.subversions[i];

                if (i > b.subversions.Count)
                    bSubversion = 0;
                else
                    bSubversion = b.subversions[i];
                if (aSubversion < bSubversion)
                    return true;
            }
            return false;
        }
        public static bool? operator >(Version a, Version b)
        {
            if (a.subversions == null || b.subversions == null)
                return null;

            for (int i = 0; i < Math.Max(a.subversions.Count, b.subversions.Count); i++)
            {
                int aSubversion;
                int bSubversion;
                if (i > a.subversions.Count)
                    aSubversion = 0;
                else
                    aSubversion = a.subversions[i];

                if (i > b.subversions.Count)
                    bSubversion = 0;
                else
                    bSubversion = b.subversions[i];
                if (aSubversion > bSubversion)
                    return true;
            }
            return false;
        }
        public override string ToString()
        {
            StringBuilder result = new();
            for (int i = 0; i < subversions.Count; i++)
            {
                if (i != 0)
                    result.Append(".");
                result.Append(subversions[i].ToString());
            }
            return result.ToString();
        }
    }
}
