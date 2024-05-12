namespace webapi.Model.DBCommunityModel.Response {
     public class LoginStudentResponseModel {
          public long Id { get; set; }
          public string? UserName { get; set; }
          public string? StudentCode { get; set; }
          public string? FirstName { get; set; }
          public string? LastName { get; set; }
          public string? Email { get; set; }
          public string? Phone { get; set; }
          public DateTime? CreateAt { get; set; }
          public DateTime? LastModified { get; set; }
          public string? OTPKey { get; set; }
     }
}