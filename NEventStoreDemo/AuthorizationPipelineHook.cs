using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NEventStore;
using NEventStore.Persistence;

namespace NEventStoreDemo
{


    public class AuthorizationPipelineHook : IPipelineHook
    {
        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //public Commit Select(Commit committed)
        //{
        //    // return null if the user isn't authorized to see this commit
        //    return committed;
        //}

        //public bool PreCommit(Commit attempt)
        //{
        //    // Can easily do logging or other such activities here
        //    return true; // true == allow commit to continue, false = stop.
        //}

        //public void PostCommit(Commit committed)
        //{
        //    // anything to do after the commit has been persisted.
        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    // no op
        //}
        public void Dispose()
        {
            // no op
        }

        public ICommit Select(ICommit committed)
        {
            return committed;
        }

        public bool PreCommit(CommitAttempt attempt)
        {
            return true;
        }

        public void PostCommit(ICommit committed)
        {
        }

        public void OnPurge(string bucketId)
        {
        }

        public void OnDeleteStream(string bucketId, string streamId)
        {
        }
    }
}
