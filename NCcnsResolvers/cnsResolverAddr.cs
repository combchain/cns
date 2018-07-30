using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.ComponentModel;
using System.Numerics;

namespace NCcnsResolverAddr
{
    public class cnsResolverAddr : SmartContract
    {
        string magic = "comb";//magic

        private static byte[] GetZeroByte34(string label)
        {
            byte[] zeroByte34 = new byte[34];

            Runtime.Notify(new object[] { label, zeroByte34 });
            return zeroByte34;
        }

        private static byte[] GetFalseByte(string label)
        {
            byte[] falseByte = new byte[] { 0 };

            Runtime.Notify(new object[] { label, falseByte });
            return falseByte;
        }

        private static byte[] GetTrueByte(string label)
        {
            byte[] trueByte = new byte[] { 1 };

            Runtime.Notify(new object[] { label, trueByte });
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
                case "alter"://string domain, string name, string subname, string addr
                    return Altert((string)args[0], (string)args[1], (string)args[2], (string)args[3]);
                case "delete"://string domain, string name, string subname
                    return Delete((string)args[0], (string)args[1], (string)args[2]);
                default:
                    return GetFalseByte("main方法");
            }
        }

        [Appcall("c132nksdjf9a82390dfjow829sbao20dkfl29dj1bd83sn")]
        public static extern byte[] CnsRegistry(byte[] signature, string operation, object[] args);

        private static byte[] NameHash(string domain, string name, string subname)
        {
            byte[] namehash = CnsRegistry(new byte[32], "namehash", new object[]{ domain, name, subname });

            Runtime.Notify(new object[] { "namehash", namehash });
            return namehash;
        }

        private static bool CheckCnsOwner(string domain, string name, string subname)
        {
            byte[] owner = CnsRegistry(new byte[32], "query", new object[] { domain, name, subname });
            Runtime.Notify(new object[] { "owner", owner });
            if (Runtime.CheckWitness(owner)){
                byte[] trueByte = new byte[] { 1 };
                Runtime.Notify(new object[] { "CheckWitness", trueByte });
                return true;
            }
            else{
                Runtime.Notify(new object[] { "CheckWitness", new byte[] { 0 } });
                return false;
            }
        }

        private static byte[] Query(string domain, string name, string subname)
        {
            byte[] addr = Storage.Get(Storage.CurrentContext, NameHash(domain, name, subname));
            if (addr == null) { return GetZeroByte34("query"); }

            Runtime.Notify(new object[] { "addr", addr });
            return addr;
        }

        public delegate void deleAlertResolver(byte[] namehash, string addr);
        [DisplayName("alertResolver")]
        public static event deleAlertResolver AlertResolverNotify;

        private static byte[] Altert(string domain, string name, string subname, string addr)
        {
            if (CheckCnsOwner(domain, name, subname))
            {
                byte[] namehash = NameHash(domain, name, subname);

                byte[] oldAddr = Storage.Get(Storage.CurrentContext, namehash);
                if (oldAddr.Length>0)
                {
                    Storage.Delete(Storage.CurrentContext, namehash);
                }

                Storage.Put(Storage.CurrentContext, namehash, addr);

                AlertResolverNotify(namehash, addr);

                return GetTrueByte("altert");
            }
            else
            {
                return GetFalseByte("altert");
            }
        }

        private static byte[] Delete(string domain, string name, string subname)
        {
            if (CheckCnsOwner(domain, name, subname))
            {
                byte[] namehash = NameHash(domain, name, subname);

                byte[] oldAddr = Storage.Get(Storage.CurrentContext, namehash);

                if (oldAddr.Length>0){
                    Storage.Delete(Storage.CurrentContext, namehash);

                    AlertResolverNotify(namehash, "");

                    return GetTrueByte("delete");
                }
                else
                {
                    return GetFalseByte("delete");
                }
            }
            else
            {
                return GetFalseByte("delete");
            }
        }
    }
}