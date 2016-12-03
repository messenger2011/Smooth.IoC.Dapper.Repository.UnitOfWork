﻿using System.Data;
using System.IO;
using FakeItEasy;
using FakeItEasy.Core;
using NUnit.Framework;
using Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.TestHelpers.Migrations;
using Smooth.IoC.Dapper.Repository.UnitOfWork.Data;

namespace Smooth.IoC.Dapper.FastCRUD.Repository.UnitOfWork.Tests.TestHelpers
{
    public abstract class CommonTestDataSetup
    {
        private static IMyDatabaseSettings _settings;
        public static ITestSession Connection { get; private set; }
        public static IDbFactory Factory { get; private set; }

        [SetUp]
        public static void TestSetup()
        {
            if (Connection != null) return;
            Factory = A.Fake<IDbFactory>();
            _settings = A.Fake<IMyDatabaseSettings>();
            var path = $@"{TestContext.CurrentContext.TestDirectory}\RepoTests.db";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            A.CallTo(() => _settings.ConnectionString).Returns($@"Data Source={path};Version=3;New=True;BinaryGUID=False;");
            Connection = CreateSession(null);
            A.CallTo(() => Factory.Create<IUnitOfWork>(A<IDbFactory>._, A<ISession>._, IsolationLevel.Serializable))
                .ReturnsLazily(CreateUnitOrWork);
            A.CallTo(() => Factory.Create<ITestSession>()).ReturnsLazily(CreateSession);

            new MigrateDb(Connection);
        }

        private static ITestSession CreateSession(IFakeObjectCall arg)
        {
            return new TestSession(Factory, _settings);
        }

        private static IUnitOfWork CreateUnitOrWork(IFakeObjectCall arg)
        {
            return new Dapper.Repository.UnitOfWork.Data.UnitOfWork((IDbFactory)arg.FakedObject, Connection);
        }
    }
}
