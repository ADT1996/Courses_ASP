namespace webapi.Model.ClientCommunityModel.Response {
     public class LoginResponseModel {
          public string? Token { get; set; }
          public string? UserName { get; set; }
          public string? Email { get; set; }
          public string? FirstName { get; set; }
          public string? LastName { get; set; }
          public string? Phone { get; set; }
          public string? StudentCode { get; set; }
          public DateTime? CreateAt { get; set; }
          public DateTime? LastModiafyAt { get; set; }
     }

     public enum LoginReturnCode: long {
          USERNAME_IS_REQUREQUIED = 1,
          PASSWORD_IS_REQUIRED = 2,
          OTP_IS_REQUIRED = 3,
          USERNAME_IS_TOO_LONG = 4,
          PASSWORD_IS_TOO_LONG = 5,
          USERNAME_IS_TOO_SHORT = 6,
          PASSWORD_IS_TOO_SHORT = 7
     }
}