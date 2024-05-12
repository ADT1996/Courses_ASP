namespace webapi.Model.DBCommunityModel.Response {
     public class ActivationStudentResponseModel {

     }

     public enum ActivationStudentReturnCode {
          SUCCESS = 0,
          USER_IS_NOT_EXIST = -1,
          USER_STATE_INVALID = -2,
          ACTIVATION_CODE_NOT_EXIST = -3,
     }
}