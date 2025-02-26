using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PGMS.CQSLight.Infra.Commands;
using PGMS.CQSLight.Infra.Querying;
using PGMS.Data.Services;
using System.Collections;
using System.Collections.Concurrent;

namespace PGMS.ScenarioTesting.ScenarioTestHelpers;

public interface IScenarioTestHelper
{
    void SendCommand(ICommand command, List<string>? roles = null);
    T ProcessQuery<T>(IQuery<T> query);
    IEntityRepository GetEntityRepository();
}

//public class TypeToRegister
//{
//    public Type InterfaceType { get; set; }
//    public Type InstanceType { get; set; }
//    public object Instance { get; set; }
//}




