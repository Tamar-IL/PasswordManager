using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DTO
{
    public class UsersDTO
    {
        public string Id { get; set; }

       
        public string UserName { get; set; }
        //להוריד! שדה מאובטח שלא יעבור בין השכבות
  
        public string Password { get; set; }

   
        public string Email { get; set; }

       
        public string Phone { get; set; }
       

        
    }
}
