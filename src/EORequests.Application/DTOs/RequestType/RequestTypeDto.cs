using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.DTOs.RequestType
{
    public sealed class RequestTypeDto { public Guid Id { get; init; } public string Code { get; init; } = ""; public string Name { get; init; } = ""; }

}
