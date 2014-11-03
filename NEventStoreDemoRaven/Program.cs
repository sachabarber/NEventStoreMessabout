using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Transactions;
using NEventStore;
using NEventStore.Dispatcher;
using NEventStore.Persistence;
using NEventStore.Persistence.RavenDB;
using Raven.Imports.Newtonsoft.Json;


namespace NEventStoreDemo
{
    class Program
    {

        private static readonly Guid StreamId = Guid.NewGuid(); // aggregate identifier

        private static readonly byte[] EncryptionKey = new byte[]
            {
                0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf
            };

        private static IStoreEvents store;

        static void Main(string[] args)
        {
            using (var scope = new TransactionScope())
            using (store = WireupEventStore())
            {
                OpenOrCreateStream();
                AppendToStream();
                GetAll();
                //TakeSnapshot();
                //LoadFromSnapshotForwardAndAppend();
                scope.Complete();
            }

            Console.WriteLine("Press a key");
            Console.ReadKey();
        }

        private static IStoreEvents WireupEventStore()
        {

            //https://groups.google.com/forum/#!topic/neventstore/NH0SzLs3LpI

            return Wireup.Init()
                         .LogToOutputWindow()
                         //.UsingInMemoryPersistence()
                         
                         .UsingRavenPersistence("EventStore")
                         .EnlistInAmbientTransaction() // two-phase commit
                         .InitializeStorageEngine()
                         .TrackPerformanceInstance("example")
                         //.UsingJsonSerialization()
                         //.Compress()
                         //.EncryptWith(EncryptionKey)
                         .HookIntoPipelineUsing(new[] { new AuthorizationPipelineHook() })
                         .UsingSynchronousDispatchScheduler()
                         .DispatchTo(new DelegateMessageDispatcher(DispatchCommit))
                         .Build();
        }

        private static void DispatchCommit(ICommit commit)
        {
            // This is where we'd hook into our messaging infrastructure, such as NServiceBus,
            // MassTransit, WCF, or some other communications infrastructure.
            // This can be a class as well--just implement IDispatchCommits.
            try
            {
                foreach (EventMessage @event in commit.Events)
                    Console.WriteLine("MessagesDispatched" + ((SomeDomainEvent)@event.Body).Value);
            }
            catch (Exception)
            {
                Console.WriteLine("UnableToDispatch");
            }
        }

        private static void OpenOrCreateStream()
        {
            // we can call CreateStream(StreamId) if we know there isn't going to be any data.
            // or we can call OpenStream(StreamId, 0, int.MaxValue) to read all commits,
            // if no commits exist then it creates a new stream for us.
            using (IEventStream stream = store.OpenStream(StreamId, 0, int.MaxValue))
            {
                var @event = new SomeDomainEvent { Value = "Initial event." };


 
                stream.Add(new EventMessage { Body = @event });
                stream.CommitChanges(Guid.NewGuid());
            }
        }

        private static void AppendToStream()
        {
            using (IEventStream stream = store.OpenStream(StreamId, int.MinValue, int.MaxValue))
            {
                var @event = new SomeDomainEvent { Value = "Second event." };

                stream.Add(new EventMessage { Body = @event });
                stream.CommitChanges(Guid.NewGuid());
            }
        }


        private static void GetAll()
        {
            // we can call CreateStream(StreamId) if we know there isn't going to be any data.
            // or we can call OpenStream(StreamId, 0, int.MaxValue) to read all commits,
            // if no commits exist then it creates a new stream for us.
            foreach (var message in  store.Advanced.GetFrom(DateTime.Now.AddDays(-1)))
            {
                var temp = message;
            }
            
        }

        //private static void TakeSnapshot()
        //{
        //    var memento = new AggregateMemento { Value = "snapshot" };
        //    store.Advanced.AddSnapshot(new Snapshot(StreamId, 2, memento));
        //}

        //private static void LoadFromSnapshotForwardAndAppend()
        //{
        //    Snapshot latestSnapshot = store.Advanced.GetSnapshot(StreamId, int.MaxValue);

        //    using (IEventStream stream = store.OpenStream(latestSnapshot, int.MaxValue))
        //    {
        //        var @event = new SomeDomainEvent { Value = "Third event (first one after a snapshot)." };

        //        stream.Add(new EventMessage { Body = @event });
        //        stream.CommitChanges(Guid.NewGuid());
        //    }
        //}
    }
}
