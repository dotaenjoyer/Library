using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Library.ModelsORM
{
    public class Book
    {
        [Key]
        public int Book_Id { get; set; }
        public string? Book_Name { get; set; }
        [JsonIgnore]
        public List<Author>? Authors { get; set; }
        [JsonIgnore]
        public List<User>? Users { get; set; }
        [JsonIgnore]
        public List<Genre>? Genres { get; set; }
    }
}
