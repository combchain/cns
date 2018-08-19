using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace DApp
{
    public class cns_domaincenter : SmartContract
    {
        const int blockday = 4096
        const string rootDomain = "test";
        static readonly byte[] InitSuperAdmin = Helper.ToScriptHash("Ajdf2hdinsmndekif993ndke230n3");;
        public static byte[] rootNameHash()
        {
            return nameHash(rootDomain);
        }
        public static string rootName()
        {
            return rootDomain;
        }
        public static object[] getInfo(byte[] cnshash)
        {
            object[] ret = new object[4];
            ret[0] = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x00 }));
            ret[1] = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x01 }));
            ret[2] = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x02 }));
            ret[3] = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x03 }));
            return ret;
        }
        delegate object deleDyncall(string method, object[] arr);
        
        static byte[] resolveFull(string protocol, string[] domainarray)
        {
            byte[] hash = nameHash(domainarray[0]);
            var height = Blockchain.GetHeight();
            for (var i = 1; i < domainarray.Length - 1; i++)
            {
        
                byte[] data = (byte[])regcall("getSubOwner", new object[] { hash, subhash });
                if (data.Length == 0)
                {
                    return new byte[] { 0x00 };

                }
                
                hash = subhash;
                var ttl = Storage.Get(Storage.CurrentContext, hash.Concat(new byte[] { 0x03 })).AsBigInteger();
                if (ttl < height)
                {
                    return new byte[] { 0x00 };
                }
            }
            string lastname = domainarray[domainarray.Length - 1];
            return resolve(protocol, hash, lastname);
        }
        
        static byte[] resolve(string protocol, byte[] cnshash, string subdomain)
        {
            var fullhash = nameHashSub(cnshash, subdomain);

            var resolver = Storage.Get(Storage.CurrentContext, fullhash.Concat(new byte[] { 0x02 }));
            if (resolver.Length != 0)
            {
                var ttl = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x03 })).AsBigInteger();
                if (ttl < Blockchain.GetHeight())
                {
                    return new byte[] { 0x00 };
                }
                
                byte[] register = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x01 }));
                if (register.Length == 0)
                {
                    return new byte[] { 0x00 };
                }
                var regcall = (deleDyncall)register.ToDelegate();

                var subhash = nameHashSub(cnshash, subdomain);
                byte[] data = (byte[])regcall("getSubOwner", new object[] { cnshash, subhash });
                if (data.Length == 0)
                {
                    return new byte[] { 0x00 };
                }

                var resolveCall = (deleDyncall)resolver.ToDelegate();
                return resolveCall("resolve", new object[] { protocol, fullhash });
            }
            resolver = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x02 }));
            if (resolver.Length != 0)
            {
                var ttl = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x03 })).AsBigInteger();
                if (ttl < Blockchain.GetHeight())
                {
                    return new byte[] { 0x00 };
                }
                var resolveCall = (deleDyncall)resolver.ToDelegate();
                return resolveCall("resolve", new object[] { protocol, cnshash });
            }
            return new byte[] { 0x00 };
        }

        static byte[] owner_SetOwner(byte[] owner, byte[] cnshash, byte[] newowner)
        {
            var callhash = ExecutionEngine.CallingScriptHash;
            var o = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x00 }));
            if (o.Length == 0 && 
                InitSuperAdmin.AsBigInteger() == owner.AsBigInteger() && 
                rootNameHash().AsBigInteger() == cnshash.AsBigInteger() 
                )
            {
                Storage.Put(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x00 }), newowner);
                return new byte[] { 0x01 };
            }
            if (
                callhash.AsBigInteger() == o.AsBigInteger()
                ||
                (Runtime.CheckWitness(owner) && o.AsBigInteger() == owner.AsBigInteger())
                )
            {
                Storage.Put(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x00 }), newowner);
                return new byte[] { 0x01 };
            }
            return new byte[] { 0x00 };
        }
        static object owner_SetRegister(byte[] owner, byte[] cnshash, byte[] controller)
        {
            var callhash = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;
            var o = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x00 }));

            if (
                callhash.AsBigInteger() == o.AsBigInteger()
                ||
                (Runtime.CheckWitness(owner) && o.AsBigInteger() == owner.AsBigInteger())
                )
            {
                Storage.Put(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x01 }), controller);
                return new byte[] { 0x01 };

            }
            return new byte[] { 0x00 };
        }
        static byte[] owner_SetResolver(byte[] owner, byte[] cnshash, byte[] resolver)
        {
            var callhash = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;
            var o = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x00 }));
            if (
                callhash.AsBigInteger() == o.AsBigInteger()
                ||
                (Runtime.CheckWitness(owner) && o.AsBigInteger() == owner.AsBigInteger())
                )
            {
               Storage.Put(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x02 }), resolver);
                return new byte[] { 0x01 };
            }
            return new byte[] { 0x00 };
        }

        static byte[] register_SetSubdomainOwner(byte[] cnshash, string subdomain, byte[] owner, BigInteger ttl)
        {
            var ttlself = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x03 })).AsBigInteger();
            if (
                (cnshash.AsBigInteger() != rootNameHash().AsBigInteger())
                &&
                ttl > ttlself
                )
            {
                return new byte[] { 0x00 };
            }
            var register = Storage.Get(Storage.CurrentContext, cnshash.Concat(new byte[] { 0x01 }));
            if (Helper.AsBigInteger(register) == Helper.AsBigInteger(ExecutionEngine.CallingScriptHash))
            {
                byte[] namehashsub = nameHashSub(cnshash, subdomain);
                Storage.Put(Storage.CurrentContext, namehashsub.Concat(new byte[] { 0x00 }), owner);
                Storage.Put(Storage.CurrentContext, namehashsub.Concat(new byte[] { 0x03 }), ttl);
                return new byte[] { 0x01 };
            }
            return new byte[] { 0x00 };
        }

        static byte[] nameHash(string domain)
        {
            return SmartContract.Sha256(domain.AsByteArray());
        }
        static byte[] nameHashSub(byte[] roothash, string subdomain)
        {
            var domain = SmartContract.Sha256(subdomain.AsByteArray()).Concat(roothash);
            return SmartContract.Sha256(domain);
        }
        static byte[] nameHashArray(string[] domainarray)
        {
            byte[] hash = nameHash(domainarray[0]);
            for (var i = 1; i < domainarray.Length; i)
            {
                hash = nameHashSub(hash, domainarray[i]);
            }
            return hash;
        }




        public static object Main(string method, object[] args)
        {
            if (method == "rootName")
                return rootName();
            if (method == "rootNameHash")
                return rootNameHash();
            if (method == "getInfo")
                return getInfo(args[0] as byte[]);
            if (method == "nameHash")
                return nameHash(args[0] as string);
            if (method == "nameHashSub")
                return nameHashSub(args[0] as byte[], args[1] as string);
            if (method == "nameHashArray")
                return nameHashArray(args[0] as string[]);
            if (method == "resolve")
                return resolve(args[0] as string, args[1] as byte[], args[2] as string);
            if (method == "resolveFull")
                return resolveFull(args[0] as string, args[1] as string[]);
            if (method == "owner_SetOwner")
                return owner_SetOwner(args[0] as byte[], args[1] as byte[], args[2] as byte[]);
            if (method == "owner_SetRegister")
                return owner_SetRegister(args[0] as byte[], args[1] as byte[], args[2] as byte[]);
            if (method == "owner_SetResolver")
                return owner_SetResolver(args[0] as byte[], args[1] as byte[], args[2] as byte[]);
            if (method == "register_SetSubdomainOwner")
                return register_SetSubdomainOwner(args[0] as byte[], args[1] as string, args[2] as byte[], (args[3] as byte[]).AsBigInteger());
            return new byte[] { 0 };

        }
    }


}
