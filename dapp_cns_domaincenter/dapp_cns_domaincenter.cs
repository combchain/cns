using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace DApp
{
    public class cns_domaincenter : SmartContract
    {

        const string rootDomain = "test";
        static readonly byte[] InitSuperAdmin = Helper.ToScriptHash("");
        public static byte[] rootNameHash()
        {
            return NameHash(rootDomain);
        }
        public static string rootName()
        {
            return rootDomain;
        }

        delegate object deleResolve(string method, object[] arr);
        
        static object resolveFull(string protocol, string[] domainarray)
        {
            byte[] hash = NameHash(domainarray[0]);
            byte[] resolver = Storage.Get(Storage.CurrentContext, "mapresolver".AsByteArray().Concat(hash));
       
            for (var i = 1; i < domainarray.Length; i)
            {
                hash = NameHashSub(hash, domainarray[i]);
                var ttl = Storage.Get(Storage.CurrentContext, "mapttl".AsByteArray().Concat(hash)).AsBigInteger();
                if (ttl < Blockchain.GetHeight()) 
                {
                    return null;
                }

                if (i == domainarray.Length - 1)
                {
                    var resolveCall = (deleResolve)resolver.ToDelegate();
                    return resolveCall("resolve", new object[] {protocol, hash });
                }
                else
                {
                    var resolveCall = (deleResolve)resolver.ToDelegate();
                    if ((int)resolveCall("active", new object[] { hash }) == 1)
                    {
                        resolver = Storage.Get(Storage.CurrentContext, "mapresolver".AsByteArray().Concat(hash));//得到子解析器
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            return null;
        }
        
        static object resolveQuick(string protocol, byte[] cnshash)
        {
            var ttl = Storage.Get(Storage.CurrentContext, "mapttl".AsByteArray().Concat(cnshash)).AsBigInteger();
            if (ttl < Blockchain.GetHeight()) 
            {
                return null;
            }

            var o = Storage.Get(Storage.CurrentContext, "mapresolvedata".AsByteArray().Concat(cnshash).Concat(protocol.AsByteArray()));
            return o;
        }

        static object owner_SetOwner(byte[] owner, byte[] cnshash, byte[] newowner)
        {
            var callhash = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;
            var o = Storage.Get(Storage.CurrentContext, "mapowner".AsByteArray().Concat(cnshash));
            if (o.Length == 0 && 
                InitSuperAdmin.AsBigInteger() == owner.AsBigInteger() && 
                rootNameHash().AsBigInteger() == cnshash.AsBigInteger() 
                )
            {
                Storage.Put(Storage.CurrentContext, "mapowner".AsByteArray().Concat(cnshash), newowner);
            }
            if (
                callhash.AsBigInteger() == o.AsBigInteger()
                ||
                (Runtime.CheckWitness(owner) && o.AsBigInteger() == owner.AsBigInteger())
                )
            {
                Storage.Put(Storage.CurrentContext, "mapowner".AsByteArray().Concat(cnshash), newowner);
                return true;
            }
            return false;
        }
        static object owner_SetController(byte[] owner, byte[] cnshash, byte[] controller)
        {
            var callhash = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;
            var o = Storage.Get(Storage.CurrentContext, "mapowner".AsByteArray().Concat(cnshash));
            if (
                callhash.AsBigInteger() == o.AsBigInteger()
                ||
                (Runtime.CheckWitness(owner) && o.AsBigInteger() == owner.AsBigInteger())
                )
            {
                Storage.Put(Storage.CurrentContext, "mapcontroller".AsByteArray().Concat(cnshash), controller);
                return true;
            }
            return false;
        }

        static object controller_SetSubdomainOwner(byte[] cnshash, string subdomain, byte[] owner)
        {
            var c = Storage.Get(Storage.CurrentContext, "mapcontroller".AsByteArray().Concat(cnshash));
            if (Helper.AsBigInteger(c) == Helper.AsBigInteger(Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash))
            {

                byte[] namehashsub = NameHashSub(cnshash, subdomain);
                Storage.Put(Storage.CurrentContext, "mapowner".AsByteArray().Concat(namehashsub), owner);
                return true;
            }
            return false;
        }

        static object controller_SetResolver(byte[] cnshash, byte[] resolver)
        {
            var c = Storage.Get(Storage.CurrentContext, "mapcontroller".AsByteArray().Concat(cnshash));
            if (Helper.AsBigInteger(c) == Helper.AsBigInteger(Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash))
            {

                Storage.Put(Storage.CurrentContext, "mapresolver".AsByteArray().Concat(cnshash), resolver);
                return true;
            }
            return false;
        }
        static object controller_SetSubdomainResolver(byte[] cnshash, string subdomain, byte[] resolver)
        {
            var c = Storage.Get(Storage.CurrentContext, "mapcontroller".AsByteArray().Concat(cnshash));
            if (Helper.AsBigInteger(c) == Helper.AsBigInteger(Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash))
            {
                byte[] namehashsub = NameHashSub(cnshash, subdomain);
                Storage.Put(Storage.CurrentContext, "mapresolver".AsByteArray().Concat(namehashsub), resolver);
                return true;
            }
            return false;
        }

        static object controller_SetResolveData(byte[] cnshash, string protocol, byte[] data)
        {
            var c = Storage.Get(Storage.CurrentContext, "mapcontroller".AsByteArray().Concat(cnshash));
            if (Helper.AsBigInteger(c) == Helper.AsBigInteger(Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash))
            {

                Storage.Put(Storage.CurrentContext, "mapresolvedata".AsByteArray().Concat(cnshash).Concat(protocol.AsByteArray()), data);
                return true;
            }
            return false;
        }
        static object controller_SetSubdomainResolveData(byte[] cnshash, string subdomain, string protocol, byte[] data)
        {
            var c = Storage.Get(Storage.CurrentContext, "mapcontroller".AsByteArray().Concat(cnshash));
            if (Helper.AsBigInteger(c) == Helper.AsBigInteger(Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash))
            {

                byte[] namehashsub = NameHashSub(cnshash, subdomain);
                Storage.Put(Storage.CurrentContext, "mapresolvedata".AsByteArray().Concat(namehashsub).Concat(protocol.AsByteArray()), data);
                return true;
            }
            return false;
        }

        static byte[] NameHash(string domain)
        {
            return SmartContract.Sha256(domain.AsByteArray());
        }
        static byte[] NameHashSub(byte[] roothash, string subdomain)
        {
            var domain = SmartContract.Sha256(subdomain.AsByteArray()).Concat(roothash);
            return SmartContract.Sha256(domain);
        }
        static byte[] NameHashArray(string[] domainarray)
        {
            byte[] hash = NameHash(domainarray[0]);
            for (var i = 1; i < domainarray.Length; i)
            {
                hash = NameHashSub(hash, domainarray[i]);
            }
            return hash;
        }




        public static object Main(string method, object[] args)
        {
            if (method == "rootName")
                return rootName();
            if (method == "rootNameHash")
                return rootNameHash();


            return false;
        }
    }


}
