using AutoMapper;
using EasyGroceries.Common.Dto;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.User.Mapping;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<AddressDto, Address>()
            .ForMember(dst => dst.Id, opt => opt.Ignore());

        CreateMap<Address, AddressDto>();
    }
}