# Eventus

![Eventus](logo.png)

|Type |Badge |
|---|---|
| **Build** | [![Build status](https://ci.appveyor.com/api/projects/status/wi02wpdnqlyifcxg/branch/master?svg=true)](https://ci.appveyor.com/api/projects/status/wi02wpdnqlyifcxg/branch/master?svg=true) |
| **Chat** | [![Gitter](https://badges.gitter.im/JoinChat.svg)](https://gitter.im/github-Eventus/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
| **Quality** | [![Codacy Badge](https://api.codacy.com/project/badge/Grade/9a21e15a059f45eda0e0b8a81f32f983)](https://www.codacy.com/app/richard.a.forrest/Eventus?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=feanz/Eventus&amp;utm_campaign=Badge_Grade) |
| **Coverage** | [![Coverage Status](https://coveralls.io/repos/github/feanz/Eventus/badge.svg)](https://coveralls.io/github/feanz/Eventus) |

## Synopsis

Eventus is a framework that provides a common setup for working with [Event Soured](https://martinfowler.com/eaaDev/EventSourcing.html) data.  It provides an abstractions over different storage providers.  Some common storage providers like SQL, Azure DocumentDB and EventStore are implemented.  This libarary is inspired by [NEventStore](https://github.com/NEventStore/NEventStore) and other frameworks,  they are all great but they did not work exactly as I wanted them to.  

## Install

The easiest way to install eventus is our [nuget](http://www.nuget.org) package.  There are also storage provider specific nuget [packages](https://www.nuget.org/packages?q=eventus). 

**Core Eventus Library**

```
> Install-Package Eventus
```

**Example Storage Provider**

```
> Install-Package Eventus.EventStore
```

## Event Sourcing 

Event soucing is a large topic and an understanding of it is needed to effectively use Eventus.  This [video](https://www.youtube.com/watch?v=JHGkaShoyNs) by Greg young is a good starting place.

## Example Usage 

Example using Eventstore storage provider.  


```csharp
var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113);
var snapshotFrequency = 5;
var repository = new Repository(new EventstoreStorageProvider(connection),
                                new EventstoreSnapshotStorageProvider(connection, snapshotFrequency));

var accountId = Guid.NewGuid();
var bankAccount = new BankAccount(accountId, "John Doe");
bankAccount.Deposit(100);
bankAccount.Withdrawal(50);
bankAccount.Deposit(5);
bankAccount.Withdrawal(15);

await repository.SaveAsync(bankAccount);

var fromStore = repository.GetByIdAsync(accountId);

```

All aggregates must inherit from the `Aggregate` base class.  Your aggreagate classes should encapsulate their state and only allow change through specific methods.  In Eventus state change is then encapsulated into an `Event` object which is applied to the aggregate.

```csharp
public class BankAccount : Aggregate
{
     public void Deposit(decimal amount, Guid correlationId = new Guid())
     {
        var deposit = new FundsDepositedEvent(Id, CurrentVersion, correlationId, amount);
        ApplyEvent(deposit);
     }
}
```

Events must implement the `IEvent` interface.  The state of the aggregate is change in apply event methods.  By default in Eventus these are private methods on the aggregate that accept the event as a single parameter.  This behavior can be changed by overriding the ApplyEvent method.  

```csharp
public class BankAccount : Aggregate
{
    private void OnFundsDeposited(FundsDepositedEvent @event)
    {
        CurrentBalance = CurrentBalance + @event.Amount;
        var newTransaction = new Transaction(TransactionType.Deposit, @event.AggregateId, @event.Amount);
        Transactions.Add(newTransaction);
    }
}
```

This allows events to be saved for storage in the provider being used and it also allows historic events to be loaded from storage and applied to the aggregate. 

## Samples

There are a set of sample application located in the [samples](https://github.com/feanz/Eventus/tree/master/src/Samples) folder.  These samples provide a full (if simplistic) CQRS workflow.  [CQRS](https://martinfowler.com/bliki/CQRS.html) and [Event Sourcing](https://martinfowler.com/eaaDev/EventSourcing.html) work really well together but one does not require the other.  It should also be noted that it is not a top level architecture and should not be applied to all problems. 

## Project 

You can view what issues are in progress on the [Eventus Project](https://github.com/feanz/Eventus/projects/1) and the current release milestones [here](https://github.com/feanz/Eventus/milestones)

## Contribution Guidelines

Contributions guidelines can be found [here](/.github/contributing.md)



