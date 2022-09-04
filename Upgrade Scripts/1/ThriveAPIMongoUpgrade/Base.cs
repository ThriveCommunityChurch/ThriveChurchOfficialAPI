using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThriveAPIMongoUpgrade
{
    internal class Base
    {
        static internal string ConnectionString { get; set; }

        static internal void ReadOptions(string[] args)
        {
            var cmdOptions = new Options();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => cmdOptions = options)
                .WithNotParsed((errors) =>
                {
                    throw new Exception("Invalid arguments.");
                });

            ConnectionString = cmdOptions.ConnectionString;
        }
    }
}
