using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;
using System.Text;

namespace NeoContractTest1
{
    public class Contract1 : SmartContract
    {
        public static byte[] Main()
        {
            byte[] trueByte = new byte[] { 1,2,3 };

            return trueByte;
        }


    }

  
}
