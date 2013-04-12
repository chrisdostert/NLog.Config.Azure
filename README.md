NLog.Config.Azure 
=================
**Description:**
Allows the configuration for [NLog](https://github.com/NLog/NLog) to be stored as a Windows Azure Configuration settings
rather than a local XML file

**Installation:**
The easiest way to install is by using [Nuget](http://nuget.org/packages/NLog.Config.Azure/)

**How to use:**
1. Add a new setting to your azure settings with the name set to NLog.Config.Azure

2. Enter a valid [NLog XML document](https://github.com/nlog/NLog/wiki/Configuration-file) into the Value field of the setting

3. Wherever you bootstrap your application, just call: 
``` C#
new AzureLoggingConfiguration().Init();
```

From then on, anywhere you request (as shown below) a logger from NLog, it will use the XML file you provided in 
your Azure Configuration Settings. Happy NLogging. -CD

``` C#
Logger logger = LogManager.GetLogger("Example");
```

