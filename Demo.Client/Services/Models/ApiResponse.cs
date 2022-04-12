namespace Demo.Client.Services.Models
{
    using System.Text.Json.Serialization;

    public class ApiResponse
    {
        [JsonPropertyName("results")]
        public List<User> Users { get; set; }

        public Info Info { get; set; }
    }
}
