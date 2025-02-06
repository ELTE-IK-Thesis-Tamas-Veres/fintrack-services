using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_common.Configuration
{
    public class AuthenticationConfiguration
    {
        public string Secret { get; set; } = "";
        public int Lifetime { get; set; }
        public string Auth0Authority { get; set; } = "";     // e.g., "https://YOUR_AUTH0_DOMAIN/"
        public string Auth0Audience { get; set; } = "";     // e.g., "YOUR_API_AUDIENCE"
    }
}
