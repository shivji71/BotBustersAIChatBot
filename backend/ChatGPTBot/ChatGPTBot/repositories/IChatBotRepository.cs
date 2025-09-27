using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGPTBot.services
{
    public interface IChatBotRepository
    {
        Task<string> AskAgentAsync(string text);
    }
}
