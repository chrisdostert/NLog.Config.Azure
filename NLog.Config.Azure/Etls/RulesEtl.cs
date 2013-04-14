using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NLog.Config.Azure.Etls
{
    public class RulesEtl : IEtl
    {
        #region Fields

        private IEnumerable<XElement> _xRules;

        #endregion

        #region Methods

        public void Extract(XDocument xDocument)
        {
            var rulesNode = xDocument.Descendants("rules");
            _xRules = rulesNode.Elements("logger");
        }

        public void Transform()
        {
        }

        public void Load(LoggingConfiguration loggingConfiguration)
        {
            foreach (var xRule in _xRules)
            {
                const string namePattern = "*";
                var minLevel = LogLevel.FromString(xRule.Attribute("minLevel").Value);
                var target = loggingConfiguration.FindTargetByName(xRule.Attribute("appendTo").Value);

                var rule = new LoggingRule(namePattern, minLevel, target);

                loggingConfiguration.LoggingRules.Add(rule);
            }
        }

        #endregion
    }
}
