## About

A curious case of why Akka fails to deserialize Quartz's `CronExpression` type. Encountered during the recovery of a persisted event stream.

Most likely workaround is to use a surrogate, though existing serialized objects appear no longer recoverable.

## Steps to reproduce:

1. Execute NetFxTests

2. Execute NetCoreTests

3. Observe `NullReferenceException` in ReadNetFxOutput:


### Message:

```
System.Runtime.Serialization.SerializationException : Failed to deserialize instance of type Quartz.CronExpression. Failed to deserialize object of type [Quartz.CronExpression] from the stream. Cause: Object reference not set to an instance of an object.
---- System.Runtime.Serialization.SerializationException : Failed to deserialize object of type [Quartz.CronExpression] from the stream. Cause: Object reference not set to an instance of an object.
-------- System.NullReferenceException : Object reference not set to an instance of an object.
```

### Stack Trace:

```
HyperionSerializer.FromBinary(Byte[] bytes, Type type)
Serializer.FromBinary[T](Byte[] bytes)
NetCoreTests.ReadNetFxOutput() line 79
----- Inner Stack Trace -----
ObjectSerializer.ReadValue(Stream stream, DeserializerSession session)
Serializer.Deserialize[T](Stream stream)
HyperionSerializer.FromBinary(Byte[] bytes, Type type)
----- Inner Stack Trace -----
lambda_method101(Closure , Stream , DeserializerSession )
ObjectSerializer.ReadValue(Stream stream, DeserializerSession session)
```

## Reference serialized formats

See also [`akkanetfx.bin`](./akkanetfx.bin) and [`akkanetcore.bin`](./akkanetcore.bin) for the wire representations produced by the .NET Framework based test and the .NET Core based test.