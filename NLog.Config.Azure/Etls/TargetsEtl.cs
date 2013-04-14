using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NLog.Targets;

namespace NLog.Config.Azure.Etls
{
    public class TargetsEtl : IEtl
    {
        #region Ctors

        public TargetsEtl()
        {
            // initialize target assemblies using only the NLog core assembly
            _targetAssemblies = (from a in AppDomain.CurrentDomain.GetAssemblies()
                                 where a.GetName().Name == "NLog"
                                 select a).ToList();
        }

        #endregion

        #region Fields

        private IEnumerable<XElement> _xTargets;
        private IEnumerable<XElement> _xExtensions;
        private List<Assembly> _targetAssemblies;

        #endregion

        #region Methods

        public void Extract(XDocument xDocument)
        {
            var targets = xDocument.Descendants("targets");
            _xTargets = targets.Elements("target");

            var extensions = xDocument.Descendants("extensions");
            _xExtensions = extensions.Elements("add");
        }

        public void Transform()
        {
            foreach (var extension in _xExtensions)
            {
                var assemblyAttribute = extension.Attribute("assembly");
                if (assemblyAttribute != null)
                {
                    var assemblyName = assemblyAttribute.Value;

                    Assembly targetAssembly;

                    targetAssembly = (from a in AppDomain.CurrentDomain.GetAssemblies()
                                               where a.GetName().Name == assemblyName
                                               select a).ToList().FirstOrDefault();
                    if (targetAssembly == null)
                    {
                        try
                        {
                            // if assembly not loaded try to load it
                            targetAssembly = Assembly.LoadFrom(String.Format("{0}.dll", assemblyName));
                        }
                        catch (Exception)
                        {
                            var msg = String.Format("Unable to load NLog target with assembly name {0}",
                                assemblyName);

                            throw new Exception(msg);
                        }    
                    }

                    // only add assemblies not already known
                    if (!_targetAssemblies.Contains(targetAssembly))
                    {
                        _targetAssemblies.Add(targetAssembly);
                    }
                }
            }
        }

        public void Load(LoggingConfiguration loggingConfiguration)
        {
            foreach (var xTarget in _xTargets)
            {
                // locate target type
                var targetTypeName = xTarget.Attribute("type").Value;

                // search in target assemblies for the type
                var targetType = (from a in _targetAssemblies
                            from t in a.GetTypes().Where(foundType => foundType.Name == targetTypeName)
                            select t).FirstOrDefault();

                if (targetType == null)
                {
                    var msg = String.Format("Unable to locate NLog Target Type with name {0}", targetTypeName);
                    throw new Exception(msg);
                }

                // create an instance of the target type
                var targetInstance = (Target)Activator.CreateInstance(targetType);
                targetInstance.Name = xTarget.Attribute("name").Value;

                var attributes = xTarget.Attributes();
                foreach (var attribute in attributes)
                {
                    if (attribute.Name.LocalName != "type" && attribute.Name.LocalName != "name")
                    {
                        var member = targetType
                            .GetProperty(attribute.Name.LocalName,
                                         BindingFlags.IgnoreCase
                                         | BindingFlags.Instance
                                         | BindingFlags.GetProperty
                                         | BindingFlags.Public);

                        // if enum type
                        if (member.PropertyType.IsEnum)
                        {
                            member.SetValue(targetInstance, Enum.Parse(member.PropertyType, attribute.Value));
                        }

                        // if convertible type
                        else if (member.PropertyType.GetInterfaces().Contains(typeof(IConvertible)))
                        {
                            member.SetValue(targetInstance, Convert.ChangeType(attribute.Value, member.PropertyType));
                        }
                        else
                        {
                            var msg = String.Format("Unable to populate property of Target {0}",
                                targetInstance.GetType());

                            throw new Exception(msg);
                        }
                    }
                }

                loggingConfiguration.AddTarget(targetInstance.Name,targetInstance);
            }
        }

        #endregion
    }
}
