//using System.Collections.Generic;

//namespace PGMS.CQSLight.Infra.Commands;

//public class BaseAuthTokenCommand : BaseCommand
//{
//    public string UserAuthToken { get; set; }

//    public BaseUserInfo? UserInfo { get; private set; }

//    public void ConvertAuthToken(ISecurityUserRepository securityUserRepository)
//    {
//        if (UserAuthToken != null)
//        {
//            UserInfo = securityUserRepository.GetUserInfo(UserAuthToken);
//        }
//    }
//}

//public class BaseUserInfo
//{
//    public string BearerToken { get; private set; }

//    public string UserFullName { get; set; }
//    public string Username { get; set; }
//    public string LanguageCode { get; set; }

//    public string UserId { get; set; }

//    public SupportedCulture Culture => SupportedCulture.GetFromName(LanguageCode);

//    public List<string> Roles { get; set; }
//}