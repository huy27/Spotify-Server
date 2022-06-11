using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IBackupDataService
    {
        Task Backup(bool isSendMail);
    }
}
