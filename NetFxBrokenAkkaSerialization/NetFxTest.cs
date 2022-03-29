using Akka.Serialization;
using Akka.TestKit.VsTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Quartz;

namespace NetFxBrokenAkkaSerialization
{
    using System.IO;
    using Akka.Actor;
    using Akka.Configuration;
    using Akka.Event;

    [TestClass]
    public class NetFxTest : TestKit
    {
        static NetFxTest()
        {
            DiagnosticsConfig = ConfigurationFactory.ParseString(@"       
akka { 
  stdout-logger-class = ""NetCoreBrokenAkkaSerialization.UnitTest1+AkkaTestDiagnosticsLogger, NetCoreBrokenAkkaSerialization""
  log-config-on-start = true
  actor {
    serializers {
      hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
    }
    serialization-bindings {
      ""System.Object"" = hyperion
      # ""Quartz.CronExpression"" = hyperion
    }
  }
}")
                .WithFallback(TestKit.FullDebugConfig)
                .WithFallback(TestKit.DefaultConfig);
        }

        public static Config DiagnosticsConfig { get; }

        public NetFxTest() : base(DiagnosticsConfig)
        {
        }

        [TestMethod]
        public void SerializationTest()
        {
            var example = new CronExpression("0 0 7 ? * MON-SUN");
            var serializer = this.Sys.Serialization.FindSerializerFor(example);
            var result = serializer.FromBinary<CronExpression>(serializer.ToBinary(example));

            Assert.AreEqual(example, result);
        }

        [TestMethod]
        public void WriteNetFxOutput()
        {
            var system = this.Sys;
            Serialization serialization = system.Serialization;

            var example = new CronExpression("0 0 7 ? * MON-SUN");
            Serializer serializer = serialization.FindSerializerFor(example);

            var file = new FileInfo(Path.Combine(Path.GetTempPath(), "fromnetfx.bin"));
            File.WriteAllBytes(file.FullName, serializer.ToBinary(example));
            Assert.AreEqual(true, file.Exists);
        }

        public class AkkaTestDiagnosticsLogger : StandardOutLogger
        {
            protected override void TellInternal(object message, IActorRef sender)
            {
                System.Diagnostics.Debug.WriteLine(message.ToString());
                base.TellInternal(message, sender);
            }
        }
    }
}
