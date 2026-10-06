using Microsoft.AspNetCore.SignalR;
using zCoach.Services.ThaiNH;
using System.Threading.Tasks;

namespace zCoach.MVCWebApp.ThaiNH.Hubs
{
    public class zCoachHub : Hub
    {
        private readonly ICoachThaiNhService _coachThaiNhService;
        
        public zCoachHub(ICoachThaiNhService coachThaiNhService) 
        {
            _coachThaiNhService = coachThaiNhService;
        }

        public async Task HubDelete_ICoachThaiNh(string id)
        {
            var result = await _coachThaiNhService.DeleteAsync(int.Parse(id));

            if (result)
            {
                await Clients.All.SendAsync("Receiver_DeleteICoachThaiNh", id);
            }
        }
    }
}
