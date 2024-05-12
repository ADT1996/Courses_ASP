namespace webapi.ClientCommunityModel;

public class ClientCommunityModel<T>
{
     public long Code { get; set; }
     public T? Data { get; set; }
}
