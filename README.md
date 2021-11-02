# Eventus

![Eventus](logo.png)

## Synopsis

Eventus is a framework that provides a common setup for working
with [Event Soured](https://martinfowler.com/eaaDev/EventSourcing.html) data. It provides an abstractions over different
storage providers. Some common storage providers like SQL, Azure CosmosDB and EventStore are implemented. This library
is inspired by [NEventStore](https://github.com/NEventStore/NEventStore) and other frameworks, they are all great but
they did not work exactly as I wanted them to.

## Install

The easiest way to install eventus is our [nuget](http://www.nuget.org) package. There are also storage provider
specific nuget [packages](https://www.nuget.org/packages?q=eventus).

**Core Eventus Library**

```
> dotnet add package Eventus
```

**Example Storage Provider**

```
> dotnet add package Eventus.EventStore
```

## Event Sourcing

Event sourcing is a large topic and an understanding of it is needed to effectively use Eventus.
This [video](https://www.youtube.com/watch?v=JHGkaShoyNs) by Greg young is a good starting place. Eventus provides a
persistence abstraction for storing events. It has a provider model that means you can plug and play different storage
database. Eventus comes with support for dotnet dependency injection and plug and play providers.

## Example Usage

Example using Eventstore storage provider.

```csharp
const string connectionString = "ConnectTo=tcp://admin:changeit@localhost:1113;UseSslConnection=false";
        
var services = new ServiceCollection();
services.AddEventus(options =>
{
    options.SnapshotFrequency = 3;
}).UseEventStore(connectionString);

var serviceProvider = services.BuildServiceProvider();
var repository = serviceProvider.GetService<IRepository>();

var bankAccount = new BankAccount("John Doe");
bankAccount.Deposit(100);
bankAccount.Withdrawal(50);
bankAccount.Deposit(5);
bankAccount.Withdrawal(15);

await repository.SaveAsync(bankAccount);

var savedAggregate = repository.GetByIdAsync(accountId);

```

The primary Eventus storage abstraction is the `Repository` it only has a limited number of methods you can get or
save `Aggregates`.

All aggregates must inherit from the `Aggregate` base class. Your aggregate classes should encapsulate their state and
only allow change through specific methods. In Eventus state change is then encapsulated into an `Event` object which is
applied to the aggregate.

```csharp
public class BankAccount : Aggregate
{
    //other code removed for brevity 
    public void Deposit(decimal amount)
    {
       var deposit = new FundsDepositedEvent(Id, CurrentVersion, correlationId, amount);
       ApplyEvent(deposit);
    }
}
```

Events must implement the `IEvent` interface. The state of the aggregate is change in apply event methods. By default in
Eventus these are private methods on the aggregate that accept the event as a single parameter. This behavior can be
changed by overriding the ApplyEvent method. These methods must exist on your aggregate and must be created by you. At
startup Eventus will validate that there is a method to handle each event type. NOTE if your Events have multiple public
constructors you will need to add the JsonConstructor attribute to the constructor you want to use during event
deserialization.

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

This allows events to be saved for storage in the provider being used and it also allows historic events to be loaded
from storage and applied to the aggregate.

## Snapshotting

In Eventus aggregates are stored as a series of events and then these events are replayed to construct the current state of the Aggregate.  To make loading of Aggregate state more efficient Eventus supports Snapshotting.  Snapshotting is when the current state of an Aggregate for a give version is saved to the storage provider.  The next time the aggregate is loaded if it is Snapshottable the latest snapshot will be loaded first and then any events that have happened since the snapshot will be played over the top. Snapshots are not saved transactionally with events this is due to the different way storage providers support ACID and units of work.  Its actually not the end of the world if a snapshot is not saved as long as the events are stored as they are the source of truth. 

### Notes on Snapshot Frequency

Snapshot frequency is how many events are written to storage before a snapshot is taken e.g. a snapshot frequency of 5 means on the 5th event a snapshot is taken.  Eventus currently will only store a maximum of a single snapshot any time a collection of events is stored. So if you save 10 events for a given Aggregate with a snapshot frequency of 5 only 1 snapshot will be taken which is the state of the most recent version of the Aggregate.  Also Eventus does not check what snapshot are currently stored when saving an aggregate so you can end up with multiple snapshots that dont reflect the snapshot frequency setting e.g. you save 5 events to an aggregate with a snapshot frequency of 3 end up with a snapshot at of version 5 of the aggregate, if you then save 1 more event to that aggregate you now have 6 events and with a frequency of 3 you are due one more so one more is saved. This practically does not happen in most use cases as events are stored one at a time in a lot of business cases. Eventus may implement different snapshot strategies in the future.   

## Settings

You can control Eventus setting when calling the `AddEventus`. The `EventusOptions` has settings to control things like Snapshot frequency, serialization settings and its allows you to control settings on a per aggregate basis via the `AggregateConfigs`.  Each Storage provider also has its own options object that control setting specific to that storage provider like the schema to use in SQL server for example. 

## Samples

There are a set of sample application located in the [samples](https://github.com/feanz/Eventus/tree/master/src/Samples)
folder. These samples provide examples for each storage provider.

## Project

You can view what issues are in progress on the [Eventus Project](https://github.com/feanz/Eventus/projects/1) and the
current release milestones [here](https://github.com/feanz/Eventus/milestones)

## Contribution Guidelines

Contributions guidelines can be found [here](/.github/contributing.md)



