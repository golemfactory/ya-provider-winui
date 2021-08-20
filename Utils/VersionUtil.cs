using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Utils
{
    class VersionUtil
    {
        /*
         *  Returns true if comparison possible
         *  Returns false if error
         *  Returns result like function .CompareTo
         */
        public static bool CompareVersions(string? v1, string? v2, out int result)
        {
            Version? version1 = null;
            if (v1 != null)
            {
                if (!Version.TryParse(v1, out version1))
                {
                    version1 = null;
                }
            }

            Version? version2 = null;
            if (v2 != null)
            {
                if (!Version.TryParse(v2, out version2))
                {
                    version2 = null;
                }
            }

            if (version1 == null || version2 == null)
            {
                result = 0;
                return false;
            }
            result = version1.CompareTo(version2);
            return true;
        }
    }
}
