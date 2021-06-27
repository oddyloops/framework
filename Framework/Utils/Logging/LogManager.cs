using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Utils.Logging
{
    public static class LogManager
    {
        public static ILogger GetLogger(Type classType)
        {
            ILogger logger = Util.Container.CreateInstance<ILogger>();
            logger.ClassType = classType;
            return logger;
        }
    }
}
