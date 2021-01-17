using NUnit.Framework;
using Kurisu.External.VirusTotal;
using Kurisu.External.HybridAnalysis;
using System.Threading.Tasks;
using System;
using System.IO;
using Kurisu.Scan.PE;
using System.Linq;
using Newtonsoft.Json;
using Kurisu.Models;
using Kurisu.Configuration;
using System.Reflection;
using System.Collections.Generic;

namespace Kurisu.Test
{
    public class Tests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            VirusTotal.Key = Environment.GetEnvironmentVariable("VIRUSTOTAL_KEY");
            HybridAnalysis.Key = Environment.GetEnvironmentVariable("HYBRIDANALYSIS_KEY");

            var convars = typeof(Tests).GetProperties().Where(property => property.GetCustomAttributes(typeof(ConVar), false).Any());
            foreach (var property in convars)
            {
                var convar = property.GetCustomAttribute<ConVarAttribute>(true);
                ConVar.Convars.Add(convar.Name, new KeyValuePair<ConVarAttribute, PropertyInfo>(convar, property));
            }
        }

        [Test]
        public async Task TestVirusTotal()
        {
            var scan = new VirusTotal();
            // EICAR test file
            var result = await scan.ScanAsync(null, "275a021bbfb6489e54d471899f7db9d1663fc695ec2fe2a2c4538aabf651fd0f");

            Assert.Greater(result.Score, 50);
        }

        [Test]
        public async Task TestHybridAnalysis()
        {
            var scan = new HybridAnalysis();
            var result = await scan.ScanAsync(null, "275a021bbfb6489e54d471899f7db9d1663fc695ec2fe2a2c4538aabf651fd0f");

            Assert.Greater(result.Score, 50);
        }

        [Test]
        public void TestImports()
        {
            var reader = new PEReader(File.OpenRead("Sample.exe"));
            reader.Parse();

            Assert.IsTrue(reader.Imports.Any(x => x.Name == "ntdll.dll"));
            Assert.IsTrue(reader.Imports.Any(x => x.Imports.Any(x => x.Name == "NtRaiseHardError")));
        }

        [Test]
        public void TestSections()
        {
            var reader = new PEReader(File.OpenRead("Sample.exe"));
            reader.Parse();

            var test = new string(reader.Sections.First().Name);

            Assert.IsTrue(reader.Sections.Any(x => new string(x.Name).Trim('\0') == ".rdata"));
            Assert.IsTrue(reader.Sections.Any(x => new string(x.Name).Trim('\0') == ".data"));
            Assert.IsTrue(reader.Sections.Any(x => new string(x.Name).Trim('\0') == ".text"));
        }

        [Test]
        public void TestEscapeMentions()
        {
            var reminder = JsonConvert.DeserializeObject<Reminder>("{\"message\": \"@everyone\"}");

            Assert.IsTrue(reminder.Message == "@\u200beveryone");
        }

        public enum Food { Pizza, Spaghetti }

        [ConVar("convar_string")]
        public static string ConvarString { get; set; }

        [ConVar("convar_enum")]
        public static Food CovnarEnum { get; set; }

        [ConVar("convar_bool")]
        public static bool ConvarBool { get; set; }

        [Test]
        public void TestConvarString()
        {
            ConVar.Set("convar_string", "test");
            Assert.AreEqual("test", ConVar.Get("convar_string"));
        }

        [Test]
        public void TestConvarEnum()
        {
            ConVar.Set("convar_enum", "Pizza");
            Assert.AreEqual(Food.Pizza, ConVar.Get<Food>("convar_enum"));

            ConVar.Set("convar_enum", Food.Spaghetti);
            Assert.AreEqual(Food.Spaghetti, ConVar.Get<Food>("convar_enum"));
        }

        [Test]
        public void TestConvarBool()
        {
            ConVar.Set("convar_bool", "true");
            Assert.AreEqual(true, ConVar.Get("convar_bool"));

            ConVar.Set("convar_bool", false);
            Assert.AreEqual(false, ConVar.Get("convar_bool"));
        }
    }
}