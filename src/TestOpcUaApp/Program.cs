namespace TestOpcUaApp
{
    using System.Threading;
    using Opc.UaFx;
    using Opc.UaFx.Server;

    public class Program
    {
        public static void Main()
        {
            // machine data variable nodes

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

            // machine alarm node

            var alarmNode = new OpcEventNode(machine, "AlarmEvent");
            alarmNode.Severity = OpcEventSeverity.Max;
            machine.AddNotifier(server.SystemContext, alarmNode);

            // start

            server.Start();

            Console.WriteLine($"Event '{alarmNode.Name}:{alarmNode.Id}'");

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
                    alarmNode.Severity = OpcEventSeverity.Medium;
                    alarmNode.Message = $"Urgent situation {i}";
                    alarmNode.ReportEvent(server.SystemContext);

                    Console.Write("!");
                }
            }
        }
    }
}
