using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
namespace HP.GFriend.Keywords
{
    internal class GFRestResult
    {
        public HttpStatusCode Status { get; set; }
        public string ResponseBody { get; set; }
        public bool IsSuccess { get; set; }

    }
}
