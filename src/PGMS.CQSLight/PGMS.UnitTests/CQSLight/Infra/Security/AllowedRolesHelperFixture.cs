using System.Linq;
using NUnit.Framework;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Security;

namespace PGMS.UnitTests.CQSLight.Infra.Security;

[TestFixture]
public class AllowedRolesHelperFixture
{
    [Test]
    public void WithAttribute()
    {
        var roles = AllowedRolesHelper.GetAllowedRoles<FakeCommand>();
        Assert.That(roles.Count, Is.EqualTo(1));
        Assert.That(roles.First(), Is.EqualTo("Prime"));
    }

    [Test]
    public void WithoutAttribute_expect_null()
    {
        var roles = AllowedRolesHelper.GetAllowedRoles<FakeCommandWithoutAttribute>();
        Assert.That(roles, Is.Null);
    }
}

[AllowedRoles(new string[]{"Prime"})]
public class FakeCommand : BaseCommand
{
}

public class FakeCommandWithoutAttribute : BaseCommand
{
}