using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.External
{
 

    public class PersonnelApiOptions
    {
        public const string SectionName = "ExternalApis:Personnel";
        public string BaseUrl { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
    }

}
