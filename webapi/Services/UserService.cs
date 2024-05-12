using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using webapi.ClientCommunityModel;
using webapi.Dao;
using webapi.Dao.DBDao;
using webapi.Model.ClientCommunityModel;
using webapi.Model.ClientCommunityModel.Request;
using webapi.Model.ClientCommunityModel.Response;
using webapi.Model.Constant;
using webapi.Model.DBCommunityModel.Request;
using webapi.Model.DBCommunityModel.Response;
using webapi.Utils;

namespace webapi.Services
{
     public class UserService
     {
          private UserDao userDao;
          private IConfiguration configuration;
          private IHostEnvironment env;
          private MailDao mailDao;
          public UserService(UserDao userDao, IConfiguration configuration, IHostEnvironment env, MailDao mailDao)
          {
               this.userDao = userDao;
               this.configuration = configuration;
               this.env = env;
               this.mailDao = mailDao;
          }

          public ClientCommunityModel<LoginResponseModel> Login(string username, string password)
          {
               username = username.Trim().ToLower();
               var iss = configuration.GetValue<string>("Authentication:Issuer") ?? "";
               var aud = configuration.GetValue<string>("Authentication:Audience") ?? "";
               var secretKey = configuration.GetValue<string>("Authentication:SecretKey") ?? "";
               var _username = username.ToLower();
               var saltLoginPassword = configuration.GetValue<string>("SaltLoginPassword") ?? "";
               var passwordAfterMac = HMACSHA256.HashData(Encoding.UTF8.GetBytes(saltLoginPassword), Encoding.UTF8.GetBytes(password + "@~@" + _username + "@~@" + saltLoginPassword));
               var finalPassword = Convert.ToBase64String(passwordAfterMac);
               var loginResult = userDao.Login(username, finalPassword);
               var loginResponseModel = new ClientCommunityModel<LoginResponseModel>();

               loginResponseModel.Code = loginResult.Code;

               switch (loginResult.Code)
               {
                    case (int)ReturnCode.SUCCESS:
                         var loginResponseData = new LoginResponseModel();
                         loginResponseData.CreateAt = loginResult.Data!.CreateAt;
                         loginResponseData.UserName = loginResult.Data.UserName;
                         loginResponseData.LastName = loginResult.Data.LastName;
                         loginResponseData.Email = loginResult.Data.Email;
                         loginResponseData.FirstName = loginResult.Data.FirstName;
                         loginResponseData.StudentCode = loginResult.Data.StudentCode;
                         loginResponseData.Phone = loginResult.Data.Phone;
                         loginResponseData.LastModiafyAt = loginResult.Data.LastModified;
                         var loginAt = DateTime.Now.Subtract(new DateTime(1970, 1, 1));
                         var expire = DateTime.Now + TimeSpan.FromMinutes(240);
                         var joinedAt = loginResponseData.CreateAt?.Subtract(new DateTime(1970, 1, 1));
                         var claims = new List<Claim>() {
                              // new Claim(ClaimTypes.Name, username, ClaimValueTypes.String),
                              new Claim(JwtRegisteredClaimNames.Iat, ((long)loginAt.TotalSeconds).ToString(), ClaimValueTypes.Integer64),
                              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString(), ClaimValueTypes.String),
                              new Claim(JwtRegisteredClaimNames.Email, loginResponseData.Email ?? "", ClaimValueTypes.String),
                              new Claim(JwtRegisteredClaimNames.Sub, username, ClaimValueTypes.String),
                              new Claim(JwtRegisteredClaimNames.Name, loginResponseData.FirstName + "/" + loginResponseData.LastName, ClaimValueTypes.String),
                              new Claim("phone", loginResponseData.Phone ?? "", ClaimValueTypes.String),
                              new Claim("code", loginResponseData.StudentCode ?? "", ClaimValueTypes.String),
                              new Claim("joinat", ((long)(joinedAt?.TotalSeconds ?? 0)).ToString(), ClaimValueTypes.Integer64)
                         };

                         var auSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                         var token = new JwtSecurityToken(
                              iss ?? "",
                              aud ?? "",
                              claims,
                              notBefore: null,
                              expires: expire,
                              signingCredentials: new SigningCredentials(auSigningKey, SecurityAlgorithms.HmacSha256)
                         );
                         loginResponseData.Token = new JwtSecurityTokenHandler().WriteToken(token);
                         loginResponseModel.Data = loginResponseData;
                         break;
               }
               return loginResponseModel;
          }

          public async Task<ClientCommunityModel<RegisterResponseModel>> Register(RegisterRequestModel registerRequest, string myUri)
          {
               long activationCodeIndex = 0;
               var studentCodeArray = File.ReadAllLines(Path.Combine(env.ContentRootPath, "studentActivationCode.txt"));
               var isExist = File.Exists(Path.Combine(env.ContentRootPath, "studentActivationIndex.txt"));

               var UserActivationJsonEncKey = configuration.GetValue<string>("StudentRegister:KeyEnc");
               var UserActivationJsonEncIV = configuration.GetValue<string>("StudentRegister:IV");

               if (isExist)
               {
                    var fileStream = File.OpenRead(Path.Combine(env.ContentRootPath, "studentActivationIndex.txt"));
                    using (var fileStreamReader = new StreamReader(fileStream))
                    {
                         var indexString = fileStreamReader.ReadLine();
                         if (!string.IsNullOrWhiteSpace(indexString))
                         {
                              activationCodeIndex = long.Parse(indexString);
                         }
                    }
                    fileStream.Close();
               }
               var studentActivationCode = studentCodeArray[activationCodeIndex];
               var _username = registerRequest.UserName?.Trim().ToLower() ?? "";
               var password = registerRequest.Password;
               var saltLoginPassword = configuration.GetValue<string>("SaltLoginPassword") ?? "";
               var passwordAfterMac = HMACSHA256.HashData(
                    Encoding.UTF8.GetBytes(saltLoginPassword),
                    Encoding.UTF8.GetBytes(password + "@~@" + _username + "@~@" + saltLoginPassword)
               );
               var finalPassword = Convert.ToBase64String(passwordAfterMac);

               var registerRequestModel = new RegisterStudentRequestModel
               {
                    UserName = _username,
                    Password = finalPassword,
                    Email = registerRequest.Email?.Trim().ToLower(),
                    FirstName = registerRequest.FirstName?.Trim(),
                    LastName = registerRequest.LastName?.Trim(),
                    Phone = registerRequest.PhoneNumber?.Trim(),
                    State = StudentState.UNACTIVE,
                    ActivationCode = studentActivationCode
               };

               var registerResponse = userDao.Register(registerRequestModel);

               switch (registerResponse.Code)
               {
                    case 0:
                         var response = new ClientCommunityModel<RegisterResponseModel>()
                         {
                              Code = (int)RegisterReturnCode.SUCCESS,
                              Data = new RegisterResponseModel
                              {
                                   FirstName = registerResponse.Data?.FirstName,
                                   Email = registerResponse.Data?.Email,
                                   LastName = registerResponse.Data?.LastName,
                                   Phone = registerResponse.Data?.Phone,
                                   StudentCode = registerResponse.Data?.StudentCode,
                                   UserName = registerResponse.Data?.UserName
                              }
                         };
                         activationCodeIndex++;
                         FileStream fileStream;
                         if (isExist)
                         {
                              fileStream = File.OpenWrite(Path.Combine(env.ContentRootPath, "studentActivationIndex.txt"));
                         }
                         else
                         {
                              fileStream = File.Create(Path.Combine(env.ContentRootPath, "studentActivationIndex.txt"));
                         }

                         using (var fileStreamWriter = new StreamWriter(fileStream))
                         {
                              fileStreamWriter.WriteLine(activationCodeIndex.ToString());
                         }
                         fileStream.Close();
                         
                         try
                         {
                              var dicUser = new Dictionary<string, string>();
                              dicUser.Add("UserName", response.Data.UserName!);
                              dicUser.Add("ActivationCode", studentActivationCode);
                              var UserActivationJson = JsonSerializer.Serialize(dicUser);

                              var aes = Utils.Utility.GetAESCipher(UserActivationJsonEncKey!, UserActivationJsonEncIV!);
                              var encrypted = Utils.Utility.Encrypt(aes, UserActivationJson, CipherFormat.BASE64_URL);

                              var mailTemplateFile = Path.Combine(env.ContentRootPath, "Template", "Mail", "RegisterStudentMailTemplate.html");
                              var fileReader = File.OpenText(mailTemplateFile);
                              var mailTemplate = fileReader.ReadToEnd();
                              fileReader.Close();
                              mailTemplate.Replace("{[StudentName]}", $"{response.Data.LastName} {response.Data.FirstName}");
                              mailTemplate.Replace("{[ProjectName]}", $"{env.ApplicationName}");
                              mailTemplate.Replace("{[ActivationCode]}", $"{studentActivationCode}");
                              mailTemplate.Replace("{[ActiviationLink]}", $"{myUri}/active/{encrypted}");
                              var sendMailResult = await mailDao.SendHTMLBodyMail(registerRequest.Email!, "[Courses ASP] - Đăng ký học viên", mailTemplate);
                         }
                         catch (Exception)
                         { }

                         return response;
                    case -1:
                         return new ClientCommunityModel<RegisterResponseModel>
                         {
                              Code = (long)RegisterReturnCode.USERNAME_EXISTS,
                              Data = null
                         };
                    case -2:
                         return new ClientCommunityModel<RegisterResponseModel>
                         {
                              Code = (long)RegisterReturnCode.EMAIL_EXISTS,
                              Data = null
                         };
                    case -3:
                         return new ClientCommunityModel<RegisterResponseModel>
                         {
                              Code = (long)RegisterReturnCode.PHONE_EXISTS,
                              Data = null
                         };
               }

               return new ClientCommunityModel<RegisterResponseModel>
               {
                    Code = registerResponse.Code,
                    Data = null
               };
          }

          public ClientCommunityModel<ActiveStudentResponse> ActiveByActivationCode(string code) {
               var UserActivationJsonEncKey = configuration.GetValue<string>("StudentRegister:KeyEnc");
               var UserActivationJsonEncIV = configuration.GetValue<string>("StudentRegister:IV");
               var aes = Utils.Utility.GetAESCipher(UserActivationJsonEncKey!, UserActivationJsonEncIV!);
               string userName;
               string activationCode;
               try {
                    var jsonString = Utils.Utility.Decrypt(aes, code, CipherFormat.BASE64_URL);

                    var dicUser = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                    userName = dicUser!["UserName"];
                    activationCode =  dicUser!["ActivationCode"];
                    

               } catch(Exception e) {
                    return new ClientCommunityModel<ActiveStudentResponse>() {
                         Code = (int)ActiveStudentResponseCode.ACTIVE_CODE_INVALID
                    };
               }

               if(string.IsNullOrWhiteSpace(userName)) {
                    return new ClientCommunityModel<ActiveStudentResponse>() {
                         Code = (int)ActiveStudentResponseCode.ACTIVE_CODE_INVALID
                    };
               }

               if(string.IsNullOrWhiteSpace(activationCode)) {
                    return new ClientCommunityModel<ActiveStudentResponse>() {
                         Code = (int)ActiveStudentResponseCode.ACTIVE_CODE_INVALID
                    };
               }

               var dbResponse = userDao.ActivationStudent(
                    new Model.DBCommunityModel.Request.ActivationStudentRequestModel {
                         UserName = userName,
                         ActivationCode = activationCode
                    }
               );
               
               switch(dbResponse.Code) {
                    case (int) ActivationStudentReturnCode.SUCCESS:
                         return new ClientCommunityModel<ActiveStudentResponse> {
                              Code = (int)ActiveStudentResponseCode.SUCCESS,
                              Data = new ActiveStudentResponse()
                         };
                    case (int) ActivationStudentReturnCode.ACTIVATION_CODE_NOT_EXIST:
                         return new ClientCommunityModel<ActiveStudentResponse> {
                              Code = (int)ActiveStudentResponseCode.ACTIVE_NOT_EXIST
                         };
                    case (int) ActivationStudentReturnCode.USER_STATE_INVALID:
                         return new ClientCommunityModel<ActiveStudentResponse> {
                              Code = (int)ActiveStudentResponseCode.STUDENT_STATE_INVALID
                         };
                    case (int) ActivationStudentReturnCode.USER_IS_NOT_EXIST:
                         return new ClientCommunityModel<ActiveStudentResponse> {
                              Code = (int)ActiveStudentResponseCode.STUDENT_NOT_EXIST
                         };
                    default:
                         return new ClientCommunityModel<ActiveStudentResponse> {
                              Code = (int)ReturnCode.ERROR
                         };
               }
          }
     }
}