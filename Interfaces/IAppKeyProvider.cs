using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Interfaces
{
    public interface IAppKeyProvider
    {
        Task<string> GetAppKey();
    }
}
