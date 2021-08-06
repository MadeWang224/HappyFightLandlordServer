using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Constant
{
    public class Identity
    {
        public const int FARMER = 0;
        public const int LANDLORD = 1;

        public static string GetString(int identity)
        {
            if(identity==0)
            {
                return "农名";
            }
            else
            {
                return "地主";
            }
        }
    }
}
