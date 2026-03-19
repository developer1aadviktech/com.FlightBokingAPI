using Com.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.Services.ServiceInterface
{
    public interface IAdminService
    {
        Task<CommonResponse> GetAirportList(PaginationAndSortingDTO ListFilterDTO);
        Task<CommonResponse> AddUpdateAirport(AirportDTO request);
        Task<CommonResponse> DeleteAirportById(long Id);
        Task<CommonResponse> GetAirlineList(PaginationAndSortingDTO ListFilterDTO);
        Task<CommonResponse> AddUpdateAirline(AirlineDTO request);
        Task<CommonResponse> DeleteAirlineById(long Id);

        Task<CommonResponse> AddUser(AddUserDTO request);
        Task<CommonResponse> EditUser(AddUserDTO request);
        Task<CommonResponse> GetUserDetailById(int UserId);
        Task<CommonResponse> UserList(UserListFilterDTO ListFilterDTO);
    }
}
