﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;
using PGMS.DataProvider.EFCore.Contexts;
using PGMS.DataProvider.EFCore.Services;

namespace PGMS.IntegratedTests.DataProvider.EFCore.Services.UnitOfWorkFixtures
{
	[TestFixture]
	public class TransactionFixture
	{
		private IEntityRepository entityRepository;
		private string connectionString = "Server=localhost;Database=PGMSTestDb;Trusted_Connection=True;ConnectRetryCount=0;TrustServerCertificate=True";

		[SetUp]
		public void SetUp()
		{
			entityRepository = new BaseEntityRepository<TestContext>(new ConnectionStringProvider(connectionString), new IntegratedTestContextFactory());
		}

		[Test]
		public void CheckTransactionNewInsertedItem()
		{
			var id = DateTime.Now.ToEpoch();
			var idParametres = $"IntegratedTest-{id}";

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				using (var transaction = unitOfWork.GetTransaction())
				{
					entityRepository.InsertOperation(unitOfWork, new DbSequenceHiLo{id_parametres = idParametres, intval = 1});

					var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					Assert.That(value, Is.Not.Null);

					transaction.Rollback();
				}
			}

			var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.id_parametres == idParametres);

			Assert.That(result, Is.Null);
		}

		[Test]
		public void CheckTransactionNewInsertedItemPersisted()
		{
			var id = DateTime.Now.ToEpoch();
			var idParametres = $"IntegratedTest-Persisted-{id}";

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				using (var transaction = unitOfWork.GetTransaction())
				{
					entityRepository.InsertOperation(unitOfWork, new DbSequenceHiLo { id_parametres = idParametres, intval = 1 });

					var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					Assert.That(value, Is.Not.Null);

					transaction.Commit();
				}
			}

			var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.id_parametres == idParametres);

			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void CheckTransactionUpdatedItem()
		{
			var id = DateTime.Now.ToEpoch();
			var idParametres = $"IntegratedTest-update-{id}";

			entityRepository.Insert(new DbSequenceHiLo { id_parametres = idParametres, intval = 1 });

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				using (var transaction = unitOfWork.GetTransaction())
				{
					var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					value.intval = 100;
					entityRepository.UpdateOperation(unitOfWork, value);

					var updated = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					Assert.That(updated.intval, Is.EqualTo(100));

					transaction.Rollback();
				}
			}
			

			var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.id_parametres == idParametres);

			Assert.That(result.intval, Is.EqualTo(1));
		}

        [Test]
        public async Task CheckTransactionUpdatedItemAsync()
        {
            var id = DateTime.Now.ToEpoch();
            var idParametres = $"IntegratedTest-update-{id}";

            entityRepository.Insert(new DbSequenceHiLo { id_parametres = idParametres, intval = 1 });

            using (var unitOfWork = entityRepository.GetUnitOfWork())
            {
                using (var transaction = unitOfWork.GetTransaction())
                {
                    var value = await entityRepository.FindFirstOperationAsync<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
                    value.intval = 100;
                    await entityRepository.UpdateOperationAsync(unitOfWork, value);

                    var updated = await entityRepository.FindFirstOperationAsync<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
                    Assert.That(updated.intval, Is.EqualTo(100));

                    var findAll = await entityRepository.FindAllOperationAsync<DbSequenceHiLo>(unitOfWork);
                    var updatedFromFindAll = findAll.First(x => x.Id == updated.Id);
					Assert.That(updatedFromFindAll.intval, Is.EqualTo(100));

                    await transaction.RollbackAsync();
                }
            }


            var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.id_parametres == idParametres);

            Assert.That(result.intval, Is.EqualTo(1));
        }

        [Test]
		public void CheckTransactionUpdatedItemPersisted()
		{
			var id = DateTime.Now.ToEpoch();
			var idParametres = $"IntegratedTestPersisted-update-{id}";

			entityRepository.Insert(new DbSequenceHiLo { id_parametres = idParametres, intval = 1 });

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				using (var transaction = unitOfWork.GetTransaction())
				{
					var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					value.intval = 100;
					entityRepository.UpdateOperation(unitOfWork, value);

					var updated = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					Assert.That(updated.intval, Is.EqualTo(100));

					transaction.Commit();
				}
			}

			var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.id_parametres == idParametres);

			Assert.That(result.intval, Is.EqualTo(100));
		}

		[Test]
		public void CheckTransactionDeletedItem()
		{
			var id = DateTime.Now.ToEpoch();
			var idParametres = $"IntegratedTest-delete-{id}";

			entityRepository.Insert(new DbSequenceHiLo { id_parametres = idParametres, intval = 1 });

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				using (var transaction = unitOfWork.GetTransaction())
				{
					var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					value.intval = 100;
					entityRepository.DeleteOperation(unitOfWork, value);

					var updated = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					Assert.That(updated, Is.Null);

					transaction.Rollback();
				}
			}


			var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.id_parametres == idParametres);

			Assert.That(result.intval, Is.EqualTo(1));
		}


		[Test]
		public void CheckTransactionDeletedItemPersisted()
		{
			var id = DateTime.Now.ToEpoch();
			var idParametres = $"IntegratedTestPersisted-delete-{id}";

			entityRepository.Insert(new DbSequenceHiLo { id_parametres = idParametres, intval = 1 });

			using (var unitOfWork = entityRepository.GetUnitOfWork())
			{
				using (var transaction = unitOfWork.GetTransaction())
				{
					var value = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					value.intval = 100;
					entityRepository.DeleteOperation(unitOfWork, value);

					var updated = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
					Assert.That(updated, Is.Null);

					transaction.Commit();
				}
			}

			var result = entityRepository.FindFirst<DbSequenceHiLo>(x => x.id_parametres == idParametres);
			Assert.That(result, Is.Null);
		}

		[Test]
		public void ValidateValueInAndOutTransaction()
		{
			var id = DateTime.Now.ToEpoch();
			var idParametres = $"IntegratedTest-TransactionRead-{id}";

			entityRepository.Insert(new DbSequenceHiLo
			{
				id_parametres = idParametres,
				intval = 101
			});

			using var unitOfWork = entityRepository.GetUnitOfWork(true);
			using var transaction = unitOfWork.GetTransaction();

			var entity = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);
			entity.intval = 1001;
			entityRepository.UpdateOperation(unitOfWork, entity);

			//Act
			var readCommited = entityRepository.FindFirst<DbSequenceHiLo>(x => x.id_parametres == idParametres);
			var readUncommited = entityRepository.FindFirstOperation<DbSequenceHiLo>(unitOfWork, x => x.id_parametres == idParametres);

			//Assert
			Assert.That(readCommited.intval, Is.EqualTo(101));
			Assert.That(readUncommited.intval, Is.EqualTo(1001));

		}
	}

	
}