using webapi.Model.Constant;

namespace webapi.Model.DBCommunityModel.Request {
     public class RegisterStudentRequestModel {
          public string? UserName { get; set; }
          public string? Password { get; set; }
          public string? Email { get; set; }
          public string? FirstName { get; set; }
          public string? LastName { get; set; }
          public string? Phone { get; set; }
          public string? OTPKey { get; set; }
          public string? StudentCode { get; set; }
          public StudentState State {get; set; }
          public string? ActivationCode { get; set; }
     }
}