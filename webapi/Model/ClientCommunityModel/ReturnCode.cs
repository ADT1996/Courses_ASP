namespace webapi.Model.ClientCommunityModel {
     public enum ReturnCode: int {
          SUCCESS = 0,
          ERROR = 9999,
          TOKEN_EXPIRED = 9998,
          TOKEN_INVALID = 9997,
          UNAUTHORIZED = 9996
     }
}