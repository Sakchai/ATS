﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.EntityFrameworkCore;

namespace ATS.Model
{
    /// <summary>
    /// Represents the Entity repository
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public partial class EntityRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        #region Fields
        //protected DbContext _Context { get; private set; }
        //protected DbSet<TEntity> Set => _Context.Set<TEntity>();
        private readonly IATSDataProvider _dataProvider;
        private ITable<TEntity> _entities;

        #endregion

        #region Ctor
       // public IQueryable<TEntity> Query => Set;
        public EntityRepository(IATSDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

//        public EntityRepository(DbContext Context)
//        {
////            _dataProvider = dataProvider;
//            _Context = Context;
//        }
        #endregion

        #region Methods

        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        //public virtual TEntity GetById(object id)
        //{
        //    return Entities.FirstOrDefault(e => e.Id == id.ToString());
        //}

        //public virtual TEntity Find(params object[] keys)
        //{
        //    return Set.Find(keys);
        //}

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Insert(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dataProvider.InsertEntity(entity);
            //var entry = _Context.Entry(entity);
            //if (entry.State == EntityState.Detached)
            //    Set.Add(entity);
        }

        /// <summary>
        /// Insert entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Insert(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            using (var transaction = new TransactionScope())
            {
                _dataProvider.BulkInsertEntities(entities);
                transaction.Complete();
            }
        }

        /// <summary>
        /// Loads the original copy of the entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Copy of the passed entity</returns>
        //public virtual TEntity LoadOriginalCopy(TEntity entity)
        //{
        //    return _dataProvider.GetTable<TEntity>()
        //        .FirstOrDefault(e => e.Id == entity.Id);
        //}

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Update(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

             _dataProvider.UpdateEntity(entity);

            //var entry = _Context.Entry(entity);
            //if (entry.State == EntityState.Detached)
            //    Set.Attach(entity);
            //entry.State = EntityState.Modified;
        }

        /// <summary>
        /// Update entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Update(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Delete(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dataProvider.DeleteEntity(entity);
            //var entry = _Context.Entry(entity);
            //if (entry.State == EntityState.Detached)
            //    Set.Attach(entity);
            //Set.Remove(entity);
        }
        

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="entities">Entities</param>
        public virtual void Delete(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (var item in entities)
            {
                Delete(item);
            }
           // _dataProvider.BulkDeleteEntities(entities);
        }

        /// <summary>
        /// Delete entities
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        public virtual void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

           // _dataProvider.BulkDeleteEntities(predicate);
        }

        /// <summary>
        /// Executes command using System.Data.CommandType.StoredProcedure command type
        /// and returns results as collection of values of specified type
        /// </summary>
        /// <param name="storeProcedureName">Store procedure name</param>
        /// <param name="dataParameters">Command parameters</param>
        /// <returns>Collection of query result records</returns>
        public virtual IList<TEntity> EntityFromSql(string storeProcedureName, params DataParameter[] dataParameters)
        {
            return _dataProvider.QueryProc<TEntity>(storeProcedureName, dataParameters?.ToArray());
        }

        /// <summary>
        /// Truncates database table
        /// </summary>
        /// <param name="resetIdentity">Performs reset identity column</param>
        public virtual void Truncate(bool resetIdentity = false)
        {
            _dataProvider.GetTable<TEntity>().Truncate(resetIdentity);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IQueryable<TEntity> Table => Entities;

        /// <summary>
        /// Gets an entity set
        /// </summary>
        protected virtual ITable<TEntity> Entities => _entities ?? (_entities = _dataProvider.GetTable<TEntity>());
      //  protected virtual ITable<TEntity> Entities => _entities; 

        #endregion
    }
}