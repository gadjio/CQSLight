# CQSLight

[![Package Version](https://img.shields.io/nuget/v/PGMS.CQSLight.svg)](https://www.nuget.org/packages/PGMS.CQSLight)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PGMS.CQSLight.svg)](https://www.nuget.org/packages/PGMS.CQSLight)
[![License](https://img.shields.io/github/license/gadjio/CQSLight.svg)](https://github.com/gadjio/CQSLight/blob/master/LICENSE)


Version 3.X => DotNet 6.0

Version 2.X => DotNet 5.0


# Introduction 
CQS stand for Command Query Separation
This is a light framework that can help you to achieve it

PGMS.Data contains all abstractions over a specific Data Provider

PGMS.DataProvider is an implementation over EFCore

PGMS.CQSLight 

# Getting Started
Have a look at our sample app based on PGMS.CQSLight


# What's new
Version 3.1 -> async function for entityrepository, bus, EventHandlers, CommandHandlers and QueryHandlers
Version 3.1.0.5 -> EntityRepository - FindAllOperation (return all entries - not fetchsize / offset)

Version 3.2 -> Role Validation
				Breaking changes :
					- IBus.Send -> add ContextInfo
					- IHandleCommand<T>.Execute -> add ContextInfo
					- ICommand -> Remove ByUsername, ByUserId
					- BaseCommand -> Remove ByUsername, ByUserId