using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //we created a base class controller. Every controller MUST HAVE attributes - ApiController and Route and MUST BE DERIVED from Controller Base
    //to avoid repeating adding these attributes and class to every controller class we create intermediate class and derive from this class
    [ApiController]
    [Route("api/[controller]")] //we specify how to call the api on site
    public class BaseApiController : ControllerBase
    {
    }
}