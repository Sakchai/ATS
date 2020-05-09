using ATS.Model;
using System;
using System.Collections.Generic;

namespace ATS.Services
{
    /// <summary>
    /// PersonTracking service interface
    /// </summary>
    public partial interface IPersonTrackingService
    {
        /// <summary>
        /// Gets an person by person identifier
        /// </summary>
        /// <param name="email">PersonTracking identifier</param>
        /// <returns>PersonTracking</returns>
        PersonAccess GetPersonTrackingByBuilding(int buildingId, DateTime startTime, DateTime endTime);
        PersonAccess GetPersonTrackingByTranDate(int buildingId, DateTime tranDate);
        PersonAccess GetLastPersonTracking(int buildingId);

        List<Building> GetBuildings();
        /// <summary>
        /// Marks person as deleted 
        /// </summary>
        /// <param name="person">PersonTracking</param>
        void DeletePersonTracking(PersonAccess person);

        /// <summary>
        /// Inserts an person
        /// </summary>
        /// <param name="person">PersonTracking</param>
        void InsertPersonTracking(PersonAccess person);

        /// <summary>
        /// Updates the person
        /// </summary>
        /// <param name="person">PersonTracking</param>
        void UpdatePersonTracking(PersonAccess person);



    }
}