** NLog.Config.Azure** Allows the NLog configuration to be stored in Azure Configuration settings rather than a local XML file

** How to use:
1. Add a new setting to your azure settings with the name set to NLog.Config.Azure 
2. Enter a valid XML document into the Value field of the setting
3. Wherever you bootstrap your application, just call
'''C#
new AzureLoggingConfiguration().Init();
'''


