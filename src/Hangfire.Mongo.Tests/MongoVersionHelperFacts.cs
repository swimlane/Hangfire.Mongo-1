﻿using System;
using System.Threading;
using Hangfire.Mongo.Tests.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Hangfire.Mongo.Tests
{
#pragma warning disable 1591
    [Collection("Database")]
    public class MongoVersionHelperFacts
    {
        private readonly MongoDbFixture _fixture;

        public MongoVersionHelperFacts(MongoDbFixture fixture) => _fixture = fixture;

        [Fact]
        public void GetVersion_HasAdditionalInfo_Success()
        {
            // ARRANGE
            var dbMock = new Mock<IMongoDatabase>(MockBehavior.Strict);
            var command = new JsonCommand<BsonDocument>("{'buildinfo': 1}");
            dbMock.Setup(m => m.RunCommand(It.Is<JsonCommand<BsonDocument>>(b => b.Json.Contains("buildinfo")), null, CancellationToken.None))
                .Returns(new BsonDocument
            {
                ["version"] = "3.6.4-1.2"
            });

            // ACT
            var version = MongoVersionHelper.GetVersion(dbMock.Object);

            // ASSERT
            Assert.Equal(version, new Version(3, 6, 4));
        }

        [Fact]
        public void GetVersion_FromDb_Success()
        {
            // ARRANGE
            var db = _fixture.CreateDbContext();

            // ACT
            var version = MongoVersionHelper.GetVersion(db.Database);

            // ASSERT
            // no exception
        }
    }
#pragma warning restore 1591
}
