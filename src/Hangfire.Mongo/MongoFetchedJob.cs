﻿using System;
using Hangfire.Mongo.Database;
using Hangfire.Storage;
using MongoDB.Bson;

namespace Hangfire.Mongo
{
    /// <summary>
    /// Hangfire fetched job for Mongo database
    /// </summary>
    public class MongoFetchedJob : IFetchedJob
    {
        private readonly HangfireDbContext _db;
        private readonly MongoStorageOptions _storageOptions;
        private readonly DateTime _fetchedAt;
        private readonly ObjectId _id;

        private bool _disposed;

        private bool _removedFromQueue;

        private bool _requeued;

        /// <summary>
        /// Constructs fetched job by database connection, identifier, job ID and queue
        /// </summary>
        /// <param name="db">Database connection</param>
        /// <param name="storageOptions">storage options</param>
        /// <param name="fetchedAt"></param>
        /// <param name="id">Identifier</param>
        /// <param name="jobId">Job ID</param>
        /// <param name="queue">Queue name</param>
        public MongoFetchedJob(
            HangfireDbContext db, 
            MongoStorageOptions storageOptions, 
            DateTime fetchedAt,
            ObjectId id, 
            ObjectId jobId, 
            string queue)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _storageOptions = storageOptions;
            _fetchedAt = fetchedAt;
            _id = id;
            JobId = jobId.ToString();
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        /// <summary>
        /// Job ID
        /// </summary>
        public string JobId { get; }

        /// <summary>
        /// Queue name
        /// </summary>
        public string Queue { get; }

        /// <summary>
        /// Removes fetched job from a queue
        /// </summary>
        public virtual void RemoveFromQueue()
        {
            using (var transaction = _storageOptions.Factory.CreateMongoWriteOnlyTransaction(_db, _storageOptions))
            {
                transaction.RemoveFromQueue(_id, _fetchedAt, Queue);
                transaction.Commit();
            }
            _removedFromQueue = true;
        }

        /// <summary>
        /// Puts fetched job into a queue
        /// </summary>
        public virtual void Requeue()
        {
            using (var transaction = _storageOptions.Factory.CreateMongoWriteOnlyTransaction(_db, _storageOptions))
            {
                transaction.Requeue(_id, Queue);
                transaction.Commit();
            }
            _requeued = true;
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public virtual void Dispose()
        {
            if (_disposed) return;
            if (!_removedFromQueue && !_requeued)
            {
                Requeue();
            }
            
            _disposed = true;
        }
    }
}