using System.Xml.Linq;

namespace NLog.Config.Azure.Etls
{
    public interface IEtl
    {
        void Extract(XDocument xDocument);
        void Transform();
        void Load(LoggingConfiguration loggingConfiguration);
    }
}
