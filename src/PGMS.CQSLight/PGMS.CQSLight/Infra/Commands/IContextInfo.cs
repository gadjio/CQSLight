using System.Collections.Generic;

namespace PGMS.CQSLight.Infra.Commands;

public interface IContextInfo
{
    string ByUserId { get; set; }
    string ByUsername { get; set; }
    List<string> UserRoles { get; set; }
    bool SkipRoleValidation { get; set; }
}

public class ContextInfo : IContextInfo
{
    public string ByUserId { get; set; }
    public string ByUsername { get; set; }

    public List<string> UserRoles { get; set; }
    public bool SkipRoleValidation { get; set; }
}