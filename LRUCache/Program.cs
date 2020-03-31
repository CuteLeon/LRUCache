using System;
using System.Threading.Tasks;

namespace LRUCache
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            LRUCacheSet<int, string> cacheSet = new LRUCacheSet<int, string>(5);
            cacheSet.Log += (s, e) => Console.WriteLine(e);

            Parallel.For(0, 100, (index) =>
            {
                cacheSet.Add(index, index.ToString());
            });
            _ = cacheSet.GetKeyList();

            for (int index = -5; index <= 5; index++)
            {
                cacheSet.Add(index, index.ToString());
                _ = cacheSet.GetKeyList();
            }

            cacheSet.Remove(-1);

            cacheSet.Use(1);
            _ = cacheSet.GetKeyList();

            cacheSet.Add(6, "6");
            _ = cacheSet.GetKeyList();

            cacheSet.Use(5);
            _ = cacheSet.GetKeyList();

            cacheSet.Use(2);
            _ = cacheSet.GetKeyList();

            cacheSet.Remove(3);
            cacheSet.Remove(4);
            _ = cacheSet.GetKeyList();

            cacheSet.RemoveHead();
            _ = cacheSet.GetKeyList();

            cacheSet.Add(1, "0");
            cacheSet.Use(1);
            cacheSet.Add(1, "1");
            cacheSet.Use(1);
            _ = cacheSet.GetKeyList();

            Console.Read();
        }
    }
}
