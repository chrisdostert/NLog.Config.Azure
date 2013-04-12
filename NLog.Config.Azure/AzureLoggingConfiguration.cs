using Microsoft.WindowsAzure;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace NLog.Config.Azure
{
    public class AzureLoggingConfiguration
    {
        public AzureLoggingConfiguration()
        {
            var settingString = CloudConfigurationManager.GetSetting(SettingName);
            SettingValue = XDocument.Parse(settingString);
        }

        #region Properties

        protected LoggingConfiguration LoggingConfiguration = new LoggingConfiguration();
        protected virtual string SettingName
        {
            get { return "NLog.Config.Azure"; }
        }

        protected XDocument SettingValue { get; set; }
        protected virtual IEnumerable<XElement> XExtensions { get; set; }
        protected virtual IEnumerable<XElement> XTargets { get; set; }
        protected virtual IEnumerable<XElement> XRules { get; set; }

        #endregion

        public virtual void Init()
        {
            ParseTargets();
            ParseRules();
            LoadTargets();
            LoadRules();
            LogManager.Configuration = LoggingConfiguration;
        }

        protected virtual void ParseExtensions()
        {
            var extensions = SettingValue.Descendants("extensions");
            XExtensions = extensions.Elements("add");
        }

        protected virtual void ParseTargets()
        {
            var targets = SettingValue.Descendants("targets");
            XTargets = targets.Elements("target");
        }

        protected virtual void ParseRules()
        {
            var rules = SettingValue.Descendants("rules");
            XRules = rules.Elements("logger");
        }

        protected virtual void LoadTargets()
        {
            foreach (var xTarget in XTargets)
            {
                // locate target type
                var typeName = xTarget.Attribute("type").Value;
                var type = (from a in AppDomain.CurrentDomain.GetAssemblies()
                            from t in a.GetTypes().Where(foundType => foundType.Name == typeName)
                            select t).First();
                // create an instance of the target type
                var target = Activator.CreateInstance(type);

                var attributes = xTarget.Attributes();
                foreach (var attribute in attributes)
                {
                    if (attribute.Name.LocalName != "type" && attribute.Name.LocalName != "name")
                    {
                        var member = type
                            .GetProperty(attribute.Name.LocalName,
                                         BindingFlags.IgnoreCase
                                         | BindingFlags.Instance
                                         | BindingFlags.GetProperty
                                         | BindingFlags.Public);

                        // if enum type
                        if(member.PropertyType.IsEnum)
                        {
                            member.SetValue(target, Enum.Parse(member.PropertyType, attribute.Value));
                        }

                        // if convertible type
                        else if (member.PropertyType.GetInterfaces().Contains(typeof(IConvertible)))
                        {
                            member.SetValue(target, Convert.ChangeType(attribute.Value, member.PropertyType));
                        }
                        else
                        {
                            var msg = String.Format("Unable to populate property of Target {0}", target.GetType());

                            throw new Exception(msg);
                        }
                    }
                }

                LoggingConfiguration.AddTarget(xTarget.Attribute("name").Value, (Target)target);
            }
        }

        protected virtual void LoadRules()
        {
            foreach (var xRule in XRules)
            {
                const string namePattern = "*";
                var minLevel = LogLevel.FromString(xRule.Attribute("minLevel").Value);
                var target = LoggingConfiguration.FindTargetByName(xRule.Attribute("appendTo").Value);

                var rule = new LoggingRule(namePattern, minLevel, target);

                LoggingConfiguration.LoggingRules.Add(rule);
            }
        }
    }
}
