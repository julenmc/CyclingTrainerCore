using Microsoft.AspNetCore.Mvc;
using TrainingDatabase.Core.Models;
using TrainingDatabase.Core.Services;

namespace TrainingDatabase.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrainingDatabaseController : ControllerBase
    {
        // GET all action
        //[HttpGet]
        //public ActionResult<List<Cyclist>> GetAll() =>
        //    DatabaseReaderService.GetAll();
    }
}