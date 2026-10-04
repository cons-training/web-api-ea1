using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace GdbWebApi.Application.Dtos
{
    public class CloseAccountRequestDto
    {
        [Required]
        public string AccountNumber { get; set; }
    }
}
