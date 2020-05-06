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
            if (buildingId ==0)
                return null;

            if (startTime == null)
                return null;

            var query = _personRepository.Table;
            query = query.Where(x => x.BuildingId == buildingId);

            if (startTime != null)
                query = query.Where(x => x.TranDate >= startTime);

            if (endTime != null)
                query = query.Where(x => x.TranDate <= endTime);

            var ps = query.OrderBy(x => x.TranDate).ToList();
            
            if (ps.Count == 0)
                return null;

            var p1 = ps.FirstOrDefault();
            var p2 = ps.LastOrDefault();

            if (p1.TranDate == p2.TranDate)
                return p1;
            else if (p1.TranDate < p2.TranDate)
            {
                int fail = p2.NumberFail - p1.NumberFail + 1;
                int pass = p2.NumberPass - p1.NumberPass + 1;
                return new PersonAccess
                {
                    BuildingId = buildingId,
                    Id = p2.Id,
                    NumberFail = fail,
                    NumberPass = pass,
                    NumberTotal = pass + fail,
                    TranDate = DateTime.Now
                };
            }
            else
                return p2;

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
            return query.OrderBy(x => x.Name).ToList();
        }



        #endregion
    }
}