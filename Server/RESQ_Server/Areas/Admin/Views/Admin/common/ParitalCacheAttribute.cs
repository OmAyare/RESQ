using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace JobTracks.Common
{
    public class ParitalCacheAttribute : OutputCacheAttribute
    {
        public ParitalCacheAttribute(string cacheProfileName) 
        {
            OutputCacheSettingsSection outputCacheSettingsSection =
                (OutputCacheSettingsSection)WebConfigurationManager.GetSection("system.web/caching/outputCacheSettings");
            OutputCacheProfile outputCacheProfile = outputCacheSettingsSection.OutputCacheProfiles[cacheProfileName];
            Duration = outputCacheProfile.Duration;
            VaryByParam = outputCacheProfile.VaryByParam;   
        }
    }
}