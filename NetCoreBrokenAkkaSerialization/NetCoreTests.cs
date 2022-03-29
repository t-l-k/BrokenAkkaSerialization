using Xunit;

namespace NetCoreBrokenAkkaSerialization
{
    using System.IO;
    using Akka.Configuration;
    using Akka.Event;
    using Akka.Serialization;
    using Akka.TestKit.Xunit2;
    using Quartz;
    using Xunit.Abstractions;

    public class NetCoreTests : TestKit
    {

        static NetCoreTests()
        {
            DiagnosticsConfig = ConfigurationFactory.ParseString(@"       
akka { 
  stdout-logger-class = ""NetCoreBrokenAkkaSerialization.NetCoreTests+AkkaTestDiagnosticsLogger, NetCoreBrokenAkkaSerialization""
  log-config-on-start = true
  actor {
    serializers {
      hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
    }
    serialization-bindings {
      ""System.Object"" = hyperion
    }
  }
}")
                .WithFallback(TestKit.FullDebugConfig)
                .WithFallback(TestKit.DefaultConfig);
        }

        public static Config DiagnosticsConfig { get; }

        public NetCoreTests(ITestOutputHelper output) : base(DiagnosticsConfig, null, output)
        {
        }

        [Fact]
        public void SerializationTest()
        {
            var system = this.Sys;
            Serialization serialization = system.Serialization;

            var example = new CronExpression("0 0 7 ? * MON-SUN");
            Serializer serializer = serialization.FindSerializerFor(example);
            var result = serializer.FromBinary<CronExpression>(serializer.ToBinary(example));

            Assert.Equal(example, result);
        }

        [Fact]
        public void ReadNetFxOutput()
        {
            var serializer = this.Sys.Serialization.FindSerializerForType(typeof(CronExpression));
            var file = new FileInfo(Path.Combine(Path.GetTempPath(), "fromnetfx.bin"));
            var result = serializer.FromBinary<CronExpression>(File.ReadAllBytes(file.FullName));

            Assert.Equal("0 0 7 ? * MON-SUN", result.CronExpressionString);
        }

        public class AkkaTestDiagnosticsLogger : StandardOutLogger
        {
            protected override void Log(object message)
            {
                System.Diagnostics.Debug.WriteLine(message.ToString());
            }
        }
    }
}
