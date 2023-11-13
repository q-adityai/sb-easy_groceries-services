using AutoMapper;
using EasyGroceries.User.Dto;
using EasyGroceries.User.Model.Entities;

namespace EasyGroceries.User.Mapping;

public class UserAddressProfile : Profile
{
    public UserAddressProfile()
    {
        CreateMap<UserAddressDto, UserAddress>()
            .ForMember(dst => dst.CreatedAt, opt => opt.Ignore())
            .ForMember(dst => dst.LastModifiedAt, opt => opt.Ignore());

        CreateMap<UserAddress, UserAddressDto>();
    }
}