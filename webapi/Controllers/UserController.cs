using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi.ClientCommunityModel;
using webapi.Model.ClientCommunityModel;
using webapi.Model.ClientCommunityModel.Request;
using webapi.Model.ClientCommunityModel.Response;
using webapi.Services;

namespace webapi.Controllers
{

    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private UserService userService;
        private IConfiguration configuration;
        public UserController(UserService userService, IConfiguration configuration)
        {
            this.userService = userService;
            this.configuration = configuration;
        }

        [Route("login")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ClientCommunityModel<LoginResponseModel>> Login([FromBody] LoginRequestModel loginRequest)
        {
            int maxUserLength = configuration.GetValue<int>("StudentInfoConfig:MaxUserNameLength");
            int minUerLength = configuration.GetValue<int>("StudentInfoConfig:MinUserLength");
            int maxPasswordLength = configuration.GetValue<int>("StudentInfoConfig:MaxPasswordLength");
            int minPasswordLength = configuration.GetValue<int>("StudentInfoConfig:minPasswordLength");

            if (string.IsNullOrWhiteSpace(loginRequest.UserName))
            {
                return new ClientCommunityModel<LoginResponseModel>
                {
                    Code = (long)LoginReturnCode.USERNAME_IS_REQUREQUIED
                };
            }

            if (string.IsNullOrEmpty(loginRequest.Password))
            {
                return new ClientCommunityModel<LoginResponseModel>
                {
                    Code = (long)LoginReturnCode.PASSWORD_IS_REQUIRED
                };
            }

            if(string.IsNullOrWhiteSpace(loginRequest.OTP?.Trim())) {
                return new ClientCommunityModel<LoginResponseModel>
                {
                    Code = (long)LoginReturnCode.OTP_IS_REQUIRED
                };
            }

            if (loginRequest.UserName.Length < minUerLength)
            {
                return new ClientCommunityModel<LoginResponseModel>
                {
                    Code = (long)LoginReturnCode.USERNAME_IS_TOO_SHORT
                };
            }

            if (loginRequest.UserName.Length > maxUserLength)
            {
                return new ClientCommunityModel<LoginResponseModel>
                {
                    Code = (long)LoginReturnCode.USERNAME_IS_TOO_LONG
                };
            }

            if (loginRequest.Password.Length > maxPasswordLength)
            {
                return new ClientCommunityModel<LoginResponseModel>
                {
                    Code = (long)LoginReturnCode.PASSWORD_IS_TOO_LONG
                };
            }

            if (loginRequest.Password.Length < minPasswordLength)
            {
                return new ClientCommunityModel<LoginResponseModel>
                {
                    Code = (int)LoginReturnCode.PASSWORD_IS_TOO_SHORT
                };
            }

            var loginResult = userService.Login(loginRequest.UserName, loginRequest.Password);

            if (loginResult.Code == (int)ReturnCode.SUCCESS)
            {
                return loginResult;
            }
            else
            {
                return new ClientCommunityModel<LoginResponseModel>
                {
                    Code = loginResult.Code
                };
            }
        }

        [Route("register")]
        [HttpPut]
        public async Task<ClientCommunityModel<RegisterResponseModel>> Register([FromBody] RegisterRequestModel registerRequest)
        {
            int maxUserLength = configuration.GetValue<int>("StudentInfoConfig:MaxUserNameLength");
            int minUserLength = configuration.GetValue<int>("StudentInfoConfig:MinUserLength");
            int maxPasswordLength = configuration.GetValue<int>("StudentInfoConfig:MaxPasswordLength");
            int minPasswordLength = configuration.GetValue<int>("StudentInfoConfig:minPasswordLength");
            int maxEmailLength = configuration.GetValue<int>("StudentInfoConfig:MaxEmailLength");
            var emailValidString = configuration.GetValue<string>("StudentInfoConfig:RegexEmailValid");
            var regexEmailValid = new Regex(emailValidString ?? "");
            var maxFirstNameLength = configuration.GetValue<int>("StudentInfoConfig:MaxFirstNameLength");
            var maxLastNameLength = configuration.GetValue<int>("StudentInfoConfig:MaxLastNameLength");
            var maxPhoneNumberLength = configuration.GetValue<int>("StudentInfoConfig:MaxPhoneNumberLength");

            var isUserNAmeEmpty = string.IsNullOrWhiteSpace(registerRequest.UserName);
            var isPasswordEmpty = string.IsNullOrEmpty(registerRequest.Password);

            if (isUserNAmeEmpty)
            {
                return new ClientCommunityModel<RegisterResponseModel>()
                {
                    Code = (long)RegisterReturnCode.USERNAME_IS_REQUREQUIED
                };
            }

            if (isPasswordEmpty)
            {
                return new ClientCommunityModel<RegisterResponseModel>()
                {
                    Code = (long)RegisterReturnCode.PASSWORD_IS_REQUIRED
                };
            }

            if (!isUserNAmeEmpty)
            {
                if (registerRequest.UserName?.Length < minUserLength)
                {
                    return new ClientCommunityModel<RegisterResponseModel>()
                    {
                        Code = (long)RegisterReturnCode.PASSWORD_IS_REQUIRED
                    };
                }

                if (registerRequest.UserName?.Length > maxUserLength)
                {
                    return new ClientCommunityModel<RegisterResponseModel>()
                    {
                        Code = (long)RegisterReturnCode.USERNAME_IS_TOO_LONG
                    };
                }
            }

            if (registerRequest.Password?.Length < minPasswordLength)
            {
                return new ClientCommunityModel<RegisterResponseModel>()
                    {
                        Code = (long)RegisterReturnCode.PASSWORD_IS_TOO_SHORT
                    };
            }

            if (registerRequest.Password?.Length > maxPasswordLength)
            {
                return new ClientCommunityModel<RegisterResponseModel>()
                    {
                        Code = (long)RegisterReturnCode.PASSWORD_IS_TOO_LONG
                    };
            }

            if (!string.IsNullOrWhiteSpace(registerRequest.Email))
            {
                if (registerRequest.Email.Length > maxEmailLength)
                {
                    return new ClientCommunityModel<RegisterResponseModel>()
                    {
                        Code = (long)RegisterReturnCode.EMAIL_IS_TOO_LONG
                    };
                }

                if (!regexEmailValid.IsMatch(registerRequest.Email))
                {
                    return new ClientCommunityModel<RegisterResponseModel>()
                    {
                        Code = (long)RegisterReturnCode.EMAIL_INVALID
                    };
                }
            }

            if (!string.IsNullOrWhiteSpace(registerRequest.FirstName))
            {
                if (registerRequest.FirstName.Length > maxFirstNameLength)
                {
                    return new ClientCommunityModel<RegisterResponseModel>()
                    {
                        Code = (long)RegisterReturnCode.FIRSTNAME_IS_TOO_LONG
                    };
                }
                
            }

            if (!string.IsNullOrWhiteSpace(registerRequest.LastName))
            {
                if (registerRequest.LastName.Length > maxLastNameLength)
                {
                    return new ClientCommunityModel<RegisterResponseModel>()
                    {
                        Code = (long)RegisterReturnCode.LASTNAME_IS_TOO_LONG
                    };
                }
            }

            var response = await userService.Register(registerRequest, $"{this.Request.Scheme}://{this.Request.Host}");

            return response;
        }
    
        [Route("activebyactivationcode/{activationString}")]
        [HttpGet]
        public async Task<ClientCommunityModel<ActiveStudentResponse>> ActiveByActivationCode(string activationString) {
            if(string.IsNullOrWhiteSpace(activationString)) {
                return new ClientCommunityModel<ActiveStudentResponse> {
                    Code = (long)ActiveStudentResponseCode.ACTIVE_CODE_EMPTY
                };
            }

            return userService.ActiveByActivationCode(activationString);
        }
    }
}