namespace TestOpcUaApp
{
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;
    using Opc.UaFx;
    using Opc.UaFx.Server;

    public class Program
    {
        public static void Main()
        {
            // machine data variable nodes

            Console.WriteLine($"OPC-UA server started at {GetLocalIPAddress()}");

            var temperatureNode = new OpcDataVariableNode<double>("Temperature", 100.0);
            var humidityNode = new OpcDataVariableNode<double>("Humidity", 50.0);
            var pressureNode = new OpcDataVariableNode<double>("Pressure", 10.0);

            var machine = new OpcObjectNode("Machine", temperatureNode, humidityNode, pressureNode);

            using var server = new OpcServer("opc.tcp://localhost:4840/", machine);

            server.ApplicationName = "TestCSharpServer";
            server.ApplicationUri = new Uri("http://test.csharpserver.uri/");
            server.SessionCreated += (sender, e) => Console.Write($"+ {e.Session.Name} ");
            server.SessionClosing += (sender, e) => Console.Write($"~ {e.Session.Name} ");
            server.SessionClosed += (sender, e) => Console.Write($"-");

            Console.WriteLine($"Starting OPC-UA server '{server.Address}'");

            // server security

            var acl = server.Security.UserNameAcl;
            acl.AddEntry("unamecs", "psswrdCS!01");
            acl.IsEnabled = true;

            // machine event node

            var eventNode = new OpcEventNode(machine, "AlarmEvent");
            eventNode.DisplayName = "Alarm Event";
            eventNode.Severity = OpcEventSeverity.Max;

            machine.AddNotifier(server.SystemContext, eventNode);

            // start

            server.Start();

            Console.WriteLine($"Event '{eventNode.Name}:{eventNode.Id}'");

            Console.WriteLine($"Datapoint '{temperatureNode.Name}:{temperatureNode.Id}'");
            Console.WriteLine($"Datapoint '{humidityNode.Name}:{humidityNode.Id}'");
            Console.WriteLine($"Datapoint '{pressureNode.Name}:{pressureNode.Id}'");

            Console.Write($"Started at {DateTime.Now:HH:mm:ss}...");

            var i = 0;

            while (true)
            {
                // temperature

                if (temperatureNode.Value == 110)
                {
                    temperatureNode.Value = 100;
                }
                else
                {
                    temperatureNode.Value++;
                }

                temperatureNode.Timestamp = DateTime.UtcNow;
                temperatureNode.ApplyChanges(server.SystemContext);

                // humidity

                if (humidityNode.Value == 55)
                {
                    humidityNode.Value = 45;
                }
                else
                {
                    humidityNode.Value++;
                }

                humidityNode.Timestamp = DateTime.UtcNow;
                humidityNode.ApplyChanges(server.SystemContext);

                // pressure

                if (pressureNode.Value == 15)
                {
                    pressureNode.Value = 5;
                }
                else
                {
                    pressureNode.Value++;
                }

                pressureNode.Timestamp = DateTime.UtcNow;
                pressureNode.ApplyChanges(server.SystemContext);

                // sleep

                Console.Write(".");
                Thread.Sleep(5000);

                i++;

                if (i % 2 == 0)
                {
                    eventNode.Severity = OpcEventSeverity.Medium;
                    eventNode.Message = $"Urgent situation {i}";
                    eventNode.ReportEvent(server.SystemContext);

                    Console.Write("!");
                }
            }
        }

        private static string GetLocalIPAddress()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();

                return localIP;
            }
        }
    }
}
