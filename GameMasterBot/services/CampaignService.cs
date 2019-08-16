using System;
using System.IO.Enumeration;
using System.Threading.Tasks;
using Common.Interfaces.DataAccess;
using Common.Interfaces.Entities.Core;
using Discord.Commands;
using Discord.WebSocket;
using Entities.Core;

namespace GameMasterBot.Services
{
    public class CampaignService
    {
        private readonly SocketCommandContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public CampaignService(SocketCommandContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        } 

        public Task<ICampaign> Create(string name, string system, string gameMaster, string url, string[] players)
        {
            // Build the campaign object from the params
            var campaign = new Campaign
            {
                Id = name.ToLower().Replace(' ', '-'),
                Name = name,
                System = system,
                GameMaster = gameMaster,
                Url = url,
                CreatedBy = _context.User.ToString()
            };
        }
    }
}
