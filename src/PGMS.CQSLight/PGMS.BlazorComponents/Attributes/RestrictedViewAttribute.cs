namespace PGMS.BlazorComponents.Attributes;

public class RestrictedViewAttribute : Attribute
{
    public string[] Roles { get; }
    public string SiteTextResource { get; set; }

    public RestrictedViewAttribute(string[] roles)
    {
        Roles = roles;
    }

    public RestrictedViewAttribute(string[] roles, string siteTextResource)
    {
        Roles = roles;
        SiteTextResource = siteTextResource;
    }

    public string GetRoles()
    {
        return string.Join(", ", Roles);
    }
}