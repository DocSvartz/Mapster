using Mapster.Models;
using System.Linq;

namespace Mapster.Cache
{
    internal class CacheItemTypeAdapterRule
    {
        private int busyCacheItem;
        private TypeAdapterRule[] cacheItems;


        public TypeTuple Types { get; init; }

        public CacheItemTypeAdapterRule(int maxCacheItems = 3)
        {
            busyCacheItem = 0;
            cacheItems = new TypeAdapterRule[maxCacheItems];
        }

        public void AddRule(TypeAdapterRule types) 
        {
            if (cacheItems.Length == busyCacheItem)
            {
                busyCacheItem = 0;
                cacheItems[busyCacheItem] = types;
            }
            else
                cacheItems[busyCacheItem] = types;
                

            busyCacheItem++;
        }

        public TypeAdapterRule GetLastRule => cacheItems.OrderByDescending(x=>x.Settings.Id).First();

        internal TypeAdapterRule GetPreLastOrLastRule => cacheItems.OrderByDescending(x => x.Settings.Id).Take(2).First();

    }
}
