using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;
using System.Text;

namespace NeoContractTest1
{
    public class Contract1 : SmartContract
    {
        private static bool Main(string domain, string name, string subname)
        {
            byte[] namehash = Hash256(domain.AsByteArray().Concat(name.AsByteArray()).Concat(subname.AsByteArray()));

            var owner = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0 };
            byte[] zeroByte32 = new byte[32];
 
            if (owner == zeroByte32)
            {
                return true;
            }
            else
            {
                return false;
            }
                

        }


    }

  
}
