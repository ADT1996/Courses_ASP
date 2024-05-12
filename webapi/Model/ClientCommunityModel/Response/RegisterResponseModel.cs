namespace webapi.Model.ClientCommunityModel.Response {
     public class RegisterResponseModel {
          public string? UserName { get; set; }
          public string? Email { get; set; }
          public string? FirstName { get; set; }
          public string? LastName { get; set; }
          public string? Phone { get; set; }
          public string? StudentCode { get; set; }

     }

     public enum RegisterReturnCode: int {
          SUCCESS = 0,
          USERNAME_IS_REQUREQUIED = 1,
          PASSWORD_IS_REQUIRED = 2,
          USERNAME_IS_TOO_LONG = 3,
          PASSWORD_IS_TOO_LONG = 4,
          USERNAME_IS_TOO_SHORT = 5,
          PASSWORD_IS_TOO_SHORT = 6,
          PHONE_INVALID = 7,
          PHONE_IS_TOO_LONG = 8,
          PHONE_IS_TOO_SHORT = 9,
          FIRSTNAME_IS_TOO_LONG = 10,
          LASTNAME_IS_TOO_LONG = 11,
          EMAIL_INVALID = 12,
          EMAIL_IS_TOO_LONG = 13,
          USERNAME_EXISTS = -1,
          EMAIL_EXISTS = -2,
          PHONE_EXISTS = -3
          
     }
}