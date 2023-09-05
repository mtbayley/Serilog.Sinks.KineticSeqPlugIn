# Setup
1. Build project with Serilog.Sinks.KineticSeqPlugIn.Store/build.ps1
2. Copy the contents of Serilog.Sinks.KineticSeqPlugIn.Store/deployment to a location on your Kinetic application server
3. Set the following enviroment variables on the machine

| Environment Variable | Value |
| :---------------- | :------: |
| DOTNET_SHARED_STORE    | <Location from step #2>\store (i.e. C:\dotnet\store) |
| DOTNET_ADDITIONAL_DEPS | <Location from step #2>\additionalDeps (i.e. C:\dotnet\additionalDeps) |
| ASPNETCORE_HOSTINGSTARTUPASSEMBLIES | Serilog.Sinks.KineticSeqPlugIn |

4. Open the AppServer.config in the Kinetic server application files
5. Add the following section to `<appSettings>`
```xml
<appSettings>
  <!-- KineticSeqPlugIn -->
  <add key="serilog:using:KineticSeq" value="Serilog.Sinks.KineticSeqPlugIn" />
  <add key="serilog:write-to:KineticSeq.serverUrl" value="<your-seq-server-url>" />
  <add key="serilog:write-to:KineticSeq.apiKey" value="<your-api-key>" />
</appSettings>
```
6. Turn on any logging from the Epicor Administration Console that you wish to log
7. Recycle Kinetic Application Pool
8. Enjoy your logs being fed directly to Seq!

![image](https://github.com/mtbayley/Serilog.Sinks.KineticSeqPlugIn/assets/34929316/b5a27e16-b3d4-4c25-bbf1-dcd5ec17a080)
