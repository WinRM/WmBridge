using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;

namespace WmBridge.Editor
{
    public static class CsvFile
    {
        private static readonly HashSet<string> _ValidExecutionPolicy =
            new HashSet<string>(new[] { "", "Restricted", "AllSigned", "RemoteSigned", "Unrestricted", "Bypass" });

        public static List<ConnectionEntry> Load(string path, bool semicolon)
        {
            string header = "";

            using (var textReader = File.OpenText(path))
                header = textReader.ReadLine();

            semicolon = (header.Contains(";"));

            using (var textReader = File.OpenText(path))
            using (var csv = new CsvReader(textReader))
            {
                if (semicolon)
                    csv.Configuration.Delimiter = ";";

                var list = new List<ConnectionEntry>();

                try
                {
                    foreach (var item in csv.GetRecords<ConnectionEntry>())
                    {
                        if (!_ValidExecutionPolicy.Contains(item.ExecutionPolicy))
                            throw new Exception($"Invalid ExecutionPolicy: {item.ExecutionPolicy}");

                        list.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"{ex.Message}\nError on line {csv.Row}");
                }

                return list;
            }
        }

        public static void Save(string path, IEnumerable<ConnectionEntry> list, bool semicolon)
        {
            using (var textWriter = File.CreateText(path))
            using (var csv = new CsvWriter(textWriter))
            {
                if (semicolon)
                    csv.Configuration.Delimiter = ";";
                csv.WriteRecords(list);
            }
        }
    }
}
