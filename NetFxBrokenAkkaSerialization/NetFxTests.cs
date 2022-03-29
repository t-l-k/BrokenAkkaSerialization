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
    public class NetFxTests : TestKit
    {
        static NetFxTests()
        {
            DiagnosticsConfig = ConfigurationFactory.ParseString(@"
akka {
  stdout-logger-class = ""NetCoreBrokenAkkaSerialization.NetFxTests+AkkaTestDiagnosticsLogger, NetCoreBrokenAkkaSerialization""
  log-config-on-start = true
  actor {
    serializers {
      hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
    }
    serialization-bindings {
      ""System.Object"" = hyperion
    }
    serialization-settings.hyperion.cross-platform-package-name-overrides = {
      netfx = [
        {
          fingerprint = ""System.Private.CoreLib,%core%"",
          rename-from = ""System.Private.CoreLib,%core%"",
          rename-to = ""mscorlib,%core%""
       }]
      netcore = [
        {
          fingerprint = ""mscorlib,%core%"",
          rename-from = ""mscorlib,%core%"",
          rename-to = ""System.Private.CoreLib,%core%""
        }]
      net = [
        {
          fingerprint = ""mscorlib,%core%"",
          rename-from = ""mscorlib,%core%"",
          rename-to = ""System.Private.CoreLib,%core%""
        }]
    }
  }
}")
                .WithFallback(TestKit.FullDebugConfig)
                .WithFallback(TestKit.DefaultConfig);
        }

        public static Config DiagnosticsConfig { get; }

        public NetFxTests() : base(DiagnosticsConfig)
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

            var file = new FileInfo(Path.Combine(Path.GetTempPath(), "akkanetfx.bin"));
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
