using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PasswordsDTO
    {
     
        public string Id { get; set; }

      
        public string UserId { get; set; }

        
  
        public string SiteId { get; set; }

        
        public string DateReg { get; set; }

       
        public string LastDateUse { get; set; }
        
        public string Password { get; set; }
        
        public string VP { get; set; }

    }
}
