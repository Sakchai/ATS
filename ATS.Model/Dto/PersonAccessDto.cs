using System;

namespace ATS.Dto
{
    public class PersonAccessDto
    {
        public int BuildingId { get; set; }
        public int Total { get; set; }
        public int Failed { get; set; }
        public DateTime TranDate { get; set; }
    }
}
