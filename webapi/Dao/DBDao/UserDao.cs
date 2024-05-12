using System.Data;
using System.Data.SqlClient;
using webapi.Model.ClientCommunityModel;
using webapi.Model.DBCommunityModel.Request;
using webapi.Model.DBCommunityModel.Response;

namespace webapi.Dao.DBDao {
     public class UserDao {
          private DBContext.DBContext dBContext;
          public UserDao(DBContext.DBContext dBContext) {
               this.dBContext = dBContext;
          }

          public DBResponse<LoginStudentResponseModel> Login(string username, string password) {
               try {
                    LoginStudentResponseModel? model = null;
                    var command = dBContext.OpenConnection().CreateCommand();
                    command.CommandText = "LoginStudent";
                    var sqlReturnCode = new SqlParameter(parameterName: "@ReturnCode", dbType: SqlDbType.Int);
                    sqlReturnCode.Direction = ParameterDirection.ReturnValue;
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 32).Value = username;
                    command.Parameters.Add("@Password", SqlDbType.NVarChar, 50).Value = password;
                    command.Parameters.Add(sqlReturnCode);
                    command.CommandType = CommandType.StoredProcedure;
                    var dataReader = command.ExecuteReader(CommandBehavior.Default);
                    var result = (int)sqlReturnCode.Value;
                    if(result == 0) {
                         while(dataReader.NextResult()) {
                              long id = dataReader.GetInt64("Id");
                              string studentCode = dataReader.GetString("StudentCode");
                              string firstName = dataReader.GetString("FirstName");
                              string lastName = dataReader.GetString("LastName");
                              string email = dataReader.GetString("Email");
                              string phone = dataReader.GetString("Phone");
                              string otpKey = dataReader.GetString("OTPKey");
                              model = new LoginStudentResponseModel {
                                   Id = id,
                                   UserName = username,
                                   StudentCode = studentCode,
                                   FirstName = firstName,
                                   LastName = lastName,
                                   Email = email,
                                   Phone = phone,
                                   OTPKey = otpKey
                              };
                         }
                    }
                    return new DBResponse<LoginStudentResponseModel> {
                         Code = result,
                         Data = model
                    };
               } finally {
                    dBContext.CloseConnection();
               }
          }
          public DBResponse<RegisterStudentResponseModel> Register(RegisterStudentRequestModel registerStuentRequest) {
               try {
                    RegisterStudentResponseModel? response = null;
                    var command = dBContext.OpenConnection().CreateCommand();
                    
                    command.CommandText = "RegisterStudent";
                    command.CommandType = CommandType.StoredProcedure;
                    var returnParameter = command.Parameters.Add("@ReturnCode", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    var idParameter = command.Parameters.Add("@Id", SqlDbType.BigInt);
                    idParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add("@Username", SqlDbType.NVarChar, 32).Value = registerStuentRequest.UserName;
                    command.Parameters.Add("@Password", SqlDbType.VarChar, 50).Value = registerStuentRequest.Password;
                    command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 50).Value = registerStuentRequest.FirstName;
                    command.Parameters.Add("@LastName", SqlDbType.NVarChar, 50).Value = registerStuentRequest.LastName;
                    command.Parameters.Add("@Email", SqlDbType.VarChar, 255).Value = registerStuentRequest.Email;
                    command.Parameters.Add("@Phone", SqlDbType.VarChar, 32).Value = registerStuentRequest.Phone;
                    command.Parameters.Add("@Avatar", SqlDbType.Image).Value = null;
                    command.Parameters.Add("@StudentCode", SqlDbType.VarChar, 16).Value = registerStuentRequest.StudentCode;
                    command.Parameters.Add("@OTPKey", SqlDbType.NVarChar, 16).Value = registerStuentRequest.OTPKey;
                    command.Parameters.Add("@ActivationCode", SqlDbType.VarChar, 50).Value = registerStuentRequest.ActivationCode;

                    var executeReader = command.ExecuteReader();

                    var returnValue = (int)returnParameter.Value;

                    if(returnValue == 0) {
                         executeReader.NextResult();
                         response = new RegisterStudentResponseModel {
                              Id = executeReader.GetInt64("Id"),
                              UserName = executeReader.GetString("Username"),
                              FirstName = executeReader.GetString("FirstName"),
                              LastName = executeReader.GetString("LastName"),
                              Email = executeReader.GetString("Email"),
                              Phone = executeReader.GetString("Phone"),
                              StudentCode = executeReader.GetString("StudentCode"),
                              CreateAt = executeReader.GetDateTime("CreateAt"),
                         };
                    }

                    return new DBResponse<RegisterStudentResponseModel> {
                         Code = returnValue,
                         Data = response
                    };

               }
               catch {
                throw;
               }
               finally {
                    dBContext.CloseConnection();
               }
          }
     
          public DBResponse<ActivationStudentResponseModel> ActivationStudent(ActivationStudentRequestModel request) {
               try {
                    var command = dBContext.OpenConnection().CreateCommand();
                    command.CommandText = "ActiveStudent";
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter returnParam = new SqlParameter("@ReturnCode", SqlDbType.Int);
                    returnParam.Direction = ParameterDirection.ReturnValue;

                    command.Parameters.Add("@UserName", SqlDbType.NVarChar, 32).Value = request.UserName;
                    command.Parameters.Add("@ActivationCode", SqlDbType.NVarChar, 50).Value = request.ActivationCode;

                    command.Parameters.Add(returnParam);

                    command.ExecuteNonQuery();

                    switch((ActivationStudentReturnCode)returnParam.Value) {
                         case ActivationStudentReturnCode.SUCCESS:
                              return new DBResponse<ActivationStudentResponseModel>() {
                                   Code = (int)ActivationStudentReturnCode.SUCCESS,
                              };
                         case ActivationStudentReturnCode.USER_STATE_INVALID:
                              return new DBResponse<ActivationStudentResponseModel>() {
                                   Code = (int)ActivationStudentReturnCode.USER_STATE_INVALID,
                              };
                         case ActivationStudentReturnCode.USER_IS_NOT_EXIST:
                              return new DBResponse<ActivationStudentResponseModel>() {
                                   Code = (int)ActivationStudentReturnCode.USER_IS_NOT_EXIST,
                              };
                         case ActivationStudentReturnCode.ACTIVATION_CODE_NOT_EXIST:
                              return new DBResponse<ActivationStudentResponseModel>() {
                                   Code = (int)ActivationStudentReturnCode.ACTIVATION_CODE_NOT_EXIST,
                              };
                         default:
                         return new DBResponse<ActivationStudentResponseModel>() {
                                   Code = (int)ReturnCode.ERROR,
                              };
                    }
               } catch {
                    throw;
               } finally {
                    dBContext.CloseConnection();
               }
          }
     }
}