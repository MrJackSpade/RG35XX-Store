using AppStore.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppStore.Models
{
    public class AppMeta
    {
        public required GitCommitInfo CommitInfo { get; set; }
    }
}
