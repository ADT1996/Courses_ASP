namespace webapi.Model.ClientCommunityModel.Request {
     public partial class RegisterRequestModel {
          public string? UserName { get; set; }
          public string? Password { get; set; }
          public string? Email { get; set; }
          public string? FirstName { get; set; }
          public string? LastName { get; set; }
          public DateTime? BirthDate { get; set; }
          public string? PhoneNumber { get; set;}
     }
}