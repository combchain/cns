using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace NCcnsRegistry
{
    public class cnsRegistry : SmartContract
    {
        private static byte[] GetZeroByte32()
        {
            byte[] zeroByte32 = new byte[32];

            Runtime.Notify(new object[] { "return", zeroByte32 });
            return zeroByte32;
        }

        private static byte[] GetFalseByte()
        {
            byte[] falseByte = new byte[1];

            Runtime.Notify(new object[] { "return", falseByte });
            return falseByte;
        }

        private static byte[] GetTrueByte()
        {
            byte[] trueByte = new byte[2];

            Runtime.Notify(new object[] { "return", trueByte });
            return trueByte;
        }



        public static byte[] Main(byte[] signature, string operation, object[] args)
        {
            switch (operation)
            {
                case "namehash"://string domain, string name, string subname
                    return NameHash((string)args[0], (string)args[1], (string)args[2]);
                case "query"://string domain, string name, string subname
                    return Query((string)args[0], (string)args[1], (string)args[2]);
                case "register"://string domain, string name, byte[] publickey, byte[] signature
                    return Register((string)args[0], (string)args[1], (byte[])args[2], signature);
                case "subregister"://string domain, string name, string subname, byte[] publickey,byte[] signature
                    return SubRegister((string)args[0], (string)args[1], (string)args[2], (byte[])args[3], signature);
                case "delete"://string domain, string name, string subname, byte[] signature
                    return Delete((string)args[0], (string)args[1], (string)args[2], (byte[])args[3], signature);
                //case "transfer":
                //    return Transfer((string)args[0], (byte[])args[1]);
                default:
                    return GetFalseByte();
            }
        }

        private static byte[] NameHash(string domain, string name, string subname)
        {
            byte[] namehash = Hash256(domain.AsByteArray().Concat(name.AsByteArray()).Concat(subname.AsByteArray()));

            Runtime.Notify(new object[] { "namehash", namehash });
            return namehash;
        }

        private static byte[] Query(string domain, string name, string subname)
        {
            byte[] owner = Storage.Get(Storage.CurrentContext, NameHash(domain, name, subname));
            if (owner == null) { return GetZeroByte32(); }

            Runtime.Notify(new object[] { "owner", owner });
            return owner;
        }

        private static byte[] Register(string domain, string name, byte[] publickey, byte[] signature)
        {
            byte[] namehash = NameHash(domain, name,"");
            byte[] value = Storage.Get(Storage.CurrentContext, namehash);
            if (value != null) return GetFalseByte();

            Storage.Put(Storage.CurrentContext, namehash, publickey);

            return GetTrueByte();
        }

        private static byte[] SubRegister(string domain, string name, string subname, byte[] publickey,byte[] signature)
        {

            byte[] namehash = NameHash(domain, name,"");
            byte[] namevalue = Storage.Get(Storage.CurrentContext, namehash);
            if (namevalue == null) return GetFalseByte();
            if (namevalue != publickey) return GetFalseByte();

            byte[] subnamehash = NameHash(domain, name, subname);
            byte[] subnamevalue = Storage.Get(Storage.CurrentContext, subnamehash);
            if (subnamevalue != null) return GetFalseByte();

            Storage.Put(Storage.CurrentContext, subnamehash, publickey);

            return GetTrueByte();
        }

        private static byte[] Delete(string domain, string name, string subname, byte[] publickey, byte[] signature)
        {
            byte[] subnamehash = NameHash(domain, name, subname);

            byte[] subnamevalue = Storage.Get(Storage.CurrentContext, subnamehash);
            if (subnamevalue == null) return GetFalseByte();
            if (subnamevalue != publickey) return GetFalseByte();

            Storage.Delete(Storage.CurrentContext, subnamehash);

            return GetTrueByte();
        }


    }
}
