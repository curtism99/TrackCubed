using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Maui.Configs
{
    public static class EntraIdConstants
    {
        public const string TenantId = "53a6fdd4-745f-43df-ae79-79f90df03258";
        public const string ClientId = "300330a5-5589-4ed0-bff1-172e43b30591";

        // This is the scope you defined in the Web API registration
        public static readonly string[] Scopes = { "api://adcdae28-5dca-4715-9d96-5768aa305597/CubedItems.ReadWrite" };
    }
}
