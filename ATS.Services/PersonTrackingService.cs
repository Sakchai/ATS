using System;
using System.Collections.Generic;
using System.Linq;
using ATS.Model;

namespace ATS.Services
{
    /// <summary>
    /// PersonTracking service
    /// </summary>
    public partial class PersonTrackingService : IPersonTrackingService
    {
        #region Fields


        private readonly IRepository<PersonAccess> _personRepository;
        private readonly IRepository<Building> _buildingRepository;

        #endregion

        #region Ctor

        public PersonTrackingService(IRepository<PersonAccess> personRepository,
            IRepository<Building> buildingRepository)
        {
            _personRepository = personRepository;
            _buildingRepository = buildingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an person by person identifier
        /// </summary>
        /// <param name="personId">PersonTracking identifier</param>
        /// <returns>PersonTracking</returns>
        public virtual PersonAccess GetPersonTrackingByBuilding(int buildingId, DateTime startTime, DateTime endTime)
        {
            if (buildingId == 0)
                return null;

            if (startTime == null)
                return null;

            var query = _personRepository.Table;
            query = query.Where(x => x.BuildingId == buildingId);

            if (startTime != null)
                query = query.Where(x => x.TranDate >= startTime);

            if (endTime != null)
                query = query.Where(x => x.TranDate <= endTime);

            var ps = query.ToList();

            if (ps.Count == 0)
                return null;

            int pass = ps.Sum(x => x.RemainPass);
            int fail = ps.Sum(x => x.RemainFail);

            return new PersonAccess
            {
                BuildingId = buildingId,
                NumberFail = fail,
                NumberPass = pass,
                TranDate = DateTime.Now
            };

        }


        /// <summary>
        /// Marks person as deleted 
        /// </summary>
        /// <param name="person">PersonTracking</param>
        public virtual void DeletePersonTracking(PersonAccess person)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));
            _personRepository.Delete(person);
        }

        /// <summary>
        /// Inserts an person
        /// </summary>
        /// <param name="person">PersonTracking</param>
        public virtual void InsertPersonTracking(PersonAccess person)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));

            _personRepository.Insert(person);

        }

        /// <summary>
        /// Updates the person
        /// </summary>
        /// <param name="person">PersonTracking</param>
        public virtual void UpdatePersonTracking(PersonAccess person)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));

            _personRepository.Update(person);

        }

        public List<Building> GetBuildings()
        {
            var query = _buildingRepository.Table;
            query = query.Where(x => x.IsActive == true);
            return query.OrderBy(x => x.Name).ToList();
        }

        public PersonAccess GetPersonTrackingByTranDate(int buildingId, DateTime tranDate)
        {
            if (buildingId == 0)
                return null;

            if (tranDate == null)
                return null;

            var query = _personRepository.Table;
            query = query.Where(x => x.BuildingId == buildingId);

            return query.Where(x => x.TranDate == tranDate).FirstOrDefault();
        }

        public PersonAccess GetLastPersonTracking(int buildingId)
        {
            if (buildingId == 0)
                return null;

            var query = _personRepository.Table;
            query = query.Where(x => x.BuildingId == buildingId);

            return query.OrderByDescending(x => x.Id).FirstOrDefault();
        }



        #endregion
    }
}