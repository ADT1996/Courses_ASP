namespace webapi.Model.DBCommunityModel.Response {
     public class DBResponse<T> where T : class{
          public int Code {get; set; }
          public T? Data { get; set; }
     }

}