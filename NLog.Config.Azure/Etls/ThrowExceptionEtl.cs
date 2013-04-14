using System.Xml.Linq;

namespace NLog.Config.Azure.Etls
{
    public class ThrowExceptionEtl : IEtl
    {
        #region Fields

        private XAttribute _xThrowExceptions;
        private bool _throwExceptions;

        #endregion

        #region Methods

        public void Extract(XDocument xDocument)
        {
            var element = xDocument.Element("nlog");
            if (element != null)
            {
               _xThrowExceptions = element.Attribute("throwExceptions"); 
            }
        }

        public void Transform()
        {
            if (_xThrowExceptions != null)
            {
                _throwExceptions = bool.Parse(_xThrowExceptions.Value);   
            }
        }

        public void Load(LoggingConfiguration loggingConfiguration)
        {
            if (_throwExceptions)
            {
                LogManager.ThrowExceptions = true;
            }
        }

        #endregion
    }
}
