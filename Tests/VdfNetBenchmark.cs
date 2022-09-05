using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Gameloop.Vdf;
using Newtonsoft.Json;
using SteamKit2;

namespace VdfNetBenchmark
{
    internal class VdfNetBenchmark
    {
        static HttpClient clt = new();

        private static void Main()
        {
            const int numIterations = 10;
            Console.WriteLine($"Running {numIterations} iterations of deserializing the TF2 Schema...");

            Task<string> vdfStrTask = clt.GetStringAsync("https://api.steampowered.com/IEconItems_440/GetSchemaOverview/v0001/?key=0FD14BFEBAC1DA36DDBF34B4FF87732E&format=vdf"), 
                jsonStrTask = clt.GetStringAsync("https://api.steampowered.com/IEconItems_440/GetSchemaOverview/v0001/?key=0FD14BFEBAC1DA36DDBF34B4FF87732E&format=json");

            Task.WaitAll(vdfStrTask, jsonStrTask);

            string vdfStr = vdfStrTask.Result;
            string jsonStr = jsonStrTask.Result;

            Stopwatch sw = Stopwatch.StartNew();
            VdfNetDeserializeIterations(vdfStr, numIterations);
            sw.Stop();
            Console.WriteLine($"Vdf.NET (VDF)       : {sw.ElapsedMilliseconds/numIterations}ms, {sw.ElapsedTicks/numIterations}ticks average");

            sw = Stopwatch.StartNew();
            JsonNetDeserializeIterations(jsonStr, numIterations);
            sw.Stop();
            Console.WriteLine($"Json.NET (JSON)     : {sw.ElapsedMilliseconds/numIterations}ms, {sw.ElapsedTicks/numIterations}ticks average");

            sw = Stopwatch.StartNew();
            Sk2KeyvalueDeserializeIterations(vdfStr, numIterations);
            sw.Stop();
            Console.WriteLine($"SK2 KeyValue (VDF)  : {sw.ElapsedMilliseconds/numIterations}ms, {sw.ElapsedTicks/numIterations}ticks average");
        }

        public static void VdfNetDeserializeIterations(string vdf, int numIterations)
        {
            for (int index = 0; index < numIterations; index++)
                VdfConvert.Deserialize(vdf);
        }

        public static void JsonNetDeserializeIterations(string json, int numIterations)
        {
            for (int index = 0; index < numIterations; index++)
                JsonConvert.DeserializeObject(json);
        }

        public static void Sk2KeyvalueDeserializeIterations(string vdf, int numIterations)
        {
            for (int index = 0; index < numIterations; index++)
                KeyValue.LoadFromString(vdf);
        }
    }
}