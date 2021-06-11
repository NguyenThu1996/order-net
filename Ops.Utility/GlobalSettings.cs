using System;
using Microsoft.Extensions.Configuration;

namespace OPS.Utility
{
    public static class GlobalSettings
    {
        public static IConfiguration Configuration { get; set; }

        public static string GetStiKey()
        {
            return Configuration.GetSection("AppSettings").GetValue<string>("StiKey");
        }
    }
}
