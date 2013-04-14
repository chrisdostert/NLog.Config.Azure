using System.Collections.Generic;
using Microsoft.WindowsAzure;
using System.Xml.Linq;
using NLog.Config.Azure.Etls;

namespace NLog.Config.Azure
{
    public class AzureLoggingConfiguration
    {
        public AzureLoggingConfiguration()
        {
            LoggingConfiguration = new LoggingConfiguration();
            // init etls
            EnabledEtls = new List<IEtl> { new RulesEtl(), new ThrowExceptionEtl(), new TargetsEtl() };
        }

        #region Properties

        protected LoggingConfiguration LoggingConfiguration { get; set; }

        protected virtual string SettingName
        {
            get { return "NLog.Config.Azure"; }
        }

        protected XDocument NLogConfigXDoc { get; set; }
        protected string NLogConfigXmlString { get; set; }
        protected List<IEtl> EnabledEtls { get; set; }

        #endregion

        public virtual void Init()
        {
            NLogConfigXmlString = CloudConfigurationManager.GetSetting(SettingName);
            NLogConfigXDoc = XDocument.Parse(NLogConfigXmlString);

            // process ETL's
            foreach (var etl in EnabledEtls)
            {
                etl.Extract(NLogConfigXDoc);
                etl.Transform();
                etl.Load(LoggingConfiguration);
            }

            LogManager.Configuration = LoggingConfiguration;
        }
    }
}
