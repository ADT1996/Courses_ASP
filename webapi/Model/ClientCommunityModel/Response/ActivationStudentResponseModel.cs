namespace webapi.Model.ClientCommunityModel.Response {
     public class ActiveStudentResponse {

     }

     public enum ActiveStudentResponseCode: int {
          SUCCESS = 0,
          ACTIVE_CODE_EMPTY = 1,
          STUDENT_STATE_INVALID = 2,
          STUDENT_NOT_EXIST = 3,
          ACTIVE_CODE_INVALID = 4,
          ACTIVE_NOT_EXIST = 5,
     }
}