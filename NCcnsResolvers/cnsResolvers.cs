using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace NCcnsResolvers
{
    
    public class cnsResolvers : SmartContract
    {

        private static byte[] GetZeroByte34()
        {
            byte[] zeroByte34 = new byte[34];

            Runtime.Notify(new object[] { "return", zeroByte34 });
            return zeroByte34;
        }

        private static byte[] GetFalseByte()
        {
            byte[] falseByte = new byte[] { 0 };

            Runtime.Notify(new object[] { "return", falseByte });
            return falseByte;
        }

        private static byte[] GetTrueByte()
        {
            byte[] trueByte = new byte[] { 1 };

            Runtime.Notify(new object[] { "return", trueByte });
            return trueByte;
        }

        public static byte[] Main(string operation, object[] args)
        {
            switch (operation)
            {
                case "namehash"://string domain, string name, string subname
                    return NameHash((string)args[0], (string)args[1], (string)args[2]);
                case "query"://string domain, string name, string subname
                    return Query((string)args[0], (string)args[1], (string)args[2]);
                case "alter"://string domain, string name, string subname, byte[] publickey
                    return Altert((string)args[0], (string)args[1], (string)args[2], (string)args[3]);
                case "delete"://string domain, string name, string subname, byte[] publickey
                    return Delete((string)args[0], (string)args[1], (string)args[2], (string)args[3]);
                default:
                    return GetFalseByte();
            }
        }

        [Appcall("c191b3e4030b9105e59c6bb56ec0d1273cd43284")]
        public static extern byte[] CnsRegistry(byte[] signature, string operation, object[] args);

        private static byte[] NameHash(string domain, string name, string subname)
        {
            byte[] namehash = CnsRegistry(new byte[32], "namehash", new object[]{ domain, name, subname });

            Runtime.Notify(new object[] { "namehash", namehash });
            return namehash;
        }

        private static byte[] CheckNnsOwner(string domain, string name, string subname)
        {
            byte[] owner = CnsRegistry(new byte[32], "query", new object[] { domain, name, subname });

            if (Runtime.CheckWitness(owner))
            {
                return GetTrueByte();
            }
            else{
                return GetFalseByte();
            }
        }

        private static byte[] Query(string domain, string name, string subname)
        {
            byte[] addr = Storage.Get(Storage.CurrentContext, NameHash(domain, name, subname));
            if (addr == null) { return GetZeroByte34(); }

            Runtime.Notify(new object[] { "addr", addr });
            return addr;
        }


        private static byte[] Altert(string domain, string name, string subname, string addr)
        {
            if (CheckCnsOwner(domain, name, subname) == new byte[] { 1 })
            {
                byte[] namehash = NameHash(domain, name, subname);

                byte[] oldAddr = Storage.Get(Storage.CurrentContext, namehash);
                if (oldAddr != null) {
                    Storage.Delete(Storage.CurrentContext, namehash);
                }

                Storage.Put(Storage.CurrentContext, namehash, addr);
                return GetTrueByte();
            }
            else
            {
                return GetFalseByte();
            }
        }

        private static byte[] Delete(string domain, string name, string subname, string addr)
        {
            if (CheckCnsOwner(domain, name, subname) == new byte[] { 1 })
            {
                byte[] namehash = NameHash(domain, name, subname);

                byte[] oldAddr = Storage.Get(Storage.CurrentContext, namehash);

                if (oldAddr != null){
                    Storage.Delete(Storage.CurrentContext, namehash);
                    return GetTrueByte();
                }
                else
                {
                    return GetFalseByte();
                }
            }
            else
            {
                return GetFalseByte();
            }
        }
    }
}
