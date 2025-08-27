using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.DTOs.RequestType
{
    public sealed class RequestTypeUpsertDto 
    { 
        public Guid? Id { get; set; } 
        [Required, MaxLength(64)] 
        public string Code { get; set; } = ""; 
        [Required, MaxLength(256)] 
        public string Name { get; set; } = ""; 
    }

}
