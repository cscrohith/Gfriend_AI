using HP.GFriend.Client;
using NDesk.Options;
using System;
using System.Collections.Generic;

namespace HP.GFriend.Repeater
{
    class Program
    {
        static void Main(string[] args)
        {
            string executionID = null;
            string serverEndpoint = null;
            string scriptRoot = null;
            string outputRoot = null;
            GFServerConnector gfServerConnector = null;

            var options = new OptionSet()
            {
                {"i|executionId=","{ExecutionID} to run",v => executionID = v},
                {"s|server=","{Server Endpoint Address} to connect",v => serverEndpoint = v},
                {"t|testscriptRoot=","{Test Script root} where script will be saved",v => scriptRoot = v},
                {"o|outputRoot=","{Output root} where output files will be saved",v => outputRoot = v},
            };
            List<string> extra;
            try
            {
                extra = options.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine("Usage:");
                options.WriteOptionDescriptions(Console.Out);
                Environment.Exit(1);
            }

            if(string.IsNullOrEmpty(executionID) || string.IsNullOrEmpty(serverEndpoint))
            {
                Console.WriteLine("Execution ID and server address should be given.");
                Console.WriteLine("Usage:");
                options.WriteOptionDescriptions(Console.Out);
                Environment.Exit(1);
            }
            
            
            gfServerConnector = new GFServerConnector(serverEndpoint);
            
            
            RepeatEngine engine = new RepeatEngine(executionID, gfServerConnector, scriptRoot, outputRoot);
            engine.Run();
        }
    }
}
