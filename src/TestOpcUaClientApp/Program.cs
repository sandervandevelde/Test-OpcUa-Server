using Microsoft.AspNetCore.Components;
using Opc.UaFx;
using Opc.UaFx.Client;
using System.Reflection.PortableExecutable;

namespace TestOpcUaClientApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("I am an OPC-UA client!");

            using var client = new OpcClient("opc.tcp://localhost:4840/");

            client.Security.UserIdentity = new OpcClientIdentity("unamecs", "psswrdCS!01");

            client.Connect();

            //AlarmEvent:ns=2;s=Machine/AlarmEvent

            var commands = new OpcSubscribeEvent[] {
                new OpcSubscribeEvent("ns=2;s=Machine", (sender, e) =>
                    {
                        Console.WriteLine($"Command ns=2;s=Machine {e.Event.SourceName} {e.Event.EventType}, {e.Event.Time}, {e.Event.Message}");
                    })
            };

            var subscriptionCommands = client.SubscribeNodes(commands);

            //Temperature:ns=2;s=Machine/Temperature
            //Humidity:ns=2;s=Machine/Humidity
            //Pressure:ns=2;s=Machine/Pressure

            var i = 1;

            while (true)
            {
                OpcValue temperature = client.ReadNode("ns=2;s=Machine/Temperature");
                OpcValue humidity = client.ReadNode("ns=2;s=Machine/Humidity");
                OpcValue pressure = client.ReadNode("ns=2;s=Machine/Pressure");

                Console.WriteLine($"temperature:{temperature}, humidity:{humidity}, pressure:{pressure}");

                Thread.Sleep(1000);

                i++;

                if (i == 25)
                {
                    client.Disconnect();
                    break;
                }
            }
        }
    }
}
