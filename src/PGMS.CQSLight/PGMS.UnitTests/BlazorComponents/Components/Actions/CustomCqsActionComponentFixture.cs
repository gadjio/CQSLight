using System.Threading.Tasks;
using NUnit.Framework;
using PGMS.BlazorComponents.Components.Actions;
using PGMS.CQSLight.Helpers;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.UnitTestUtilities.FakeImpl.Services;
using PGMS.UnitTests.CQSLight.Infra.Commands;

namespace PGMS.UnitTests.BlazorComponents.Components.Actions;

public class CustomCqsActionComponentFixture
{
	[Test]
	public void Test()
	{
		var fakeBus = new FakeBus();
		var sut = new MyCustomCqsActionComponent(fakeBus);

		var task = sut.ProcessAction();
		task.Wait();

		var contextInfo = (MyCustomContextInfo)fakeBus.PublishedCommandsMap[0].Value;
		Assert.That(contextInfo.UserFullName, Is.EqualTo("Optimus Prime"));
	}
}

public class MyCustomCqsActionComponent : BaseCqsActionComponent
{
	public MyCustomCqsActionComponent(IBus bus)
	{
		CommandHelper = new CommandHelper(bus);
	}

	public override Task InitCommand()
	{
		return Task.CompletedTask;
	}

	public override async Task<bool> ProcessAction()
	{
		return await ProcessCommand(new FakeCommand());
	}

	protected override async Task<SendCommandResult> SendCommand(ICommand command)
	{
		return await base.SendCommand(command);
		//await CommandHelper.Send(command, GetContextInfo());
		//return new SendCommandResult { IsSuccess = true };
	}

	protected override ContextInfo GetContextInfo()
	{
		return new MyCustomContextInfo {UserFullName = "Optimus Prime"};
	}
}