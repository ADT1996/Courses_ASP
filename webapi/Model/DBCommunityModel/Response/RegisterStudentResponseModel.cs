using webapi.Model.Constant;

namespace webapi.Model.DBCommunityModel.Response {
     public class RegisterStudentResponseModel {
          public long Id { get; set; }
          public string? UserName { get; set; }
          public string? FirstName { get; set; }
          public string? LastName { get; set; }
          public string? Email { get; set; }
          public string? Phone { get; set; }
          public string? StudentCode { get; set; }
          public DateTime? CreateAt { get; set; }
          public StudentState State{ get; set; }
     }
}