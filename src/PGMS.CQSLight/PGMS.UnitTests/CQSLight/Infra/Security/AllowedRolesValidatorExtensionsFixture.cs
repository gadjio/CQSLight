using System.Collections.Generic;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;

namespace PGMS.UnitTests.CQSLight.Infra.Security;

[TestFixture]
public class AllowedRolesValidatorExtensionsFixture
{
    [Test]
    public void When_List_IsNull_expect_true()
    {
        var cmd = new FakeCommandWithoutAttribute();
        Assert.That(cmd.IsAllowed(new List<string>()), Is.True);
    }

    [Test]
    public void When_role_in_list_expect_true()
    {
        var cmd = new FakeCommand();
        Assert.That(cmd.IsAllowed(new List<string>{"Prime"}), Is.True);
    }

    [Test]
    public void When_role_in_list_expect_true_CI()
    {
        var cmd = new FakeCommand();
        Assert.That(cmd.IsAllowed(new List<string> { "PRIME" }), Is.True);
    }

    [Test]
    public void When_role_not_in_list_expect_false()
    {
        var cmd = new FakeCommand();
        Assert.That(cmd.IsAllowed(new List<string> { "Bee" }), Is.False);
    }
}