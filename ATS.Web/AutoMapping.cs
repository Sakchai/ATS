using AutoMapper;
using ATS.Model;
using ATS.Web.Models;

namespace ATS.Web
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<PersonAccess, PersonAccessViewModel>();
        }
    }
}
