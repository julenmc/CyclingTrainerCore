using Microsoft.AspNetCore.Mvc;
using CyclingTrainer.TrainingDatabase.Core.Services;

namespace CyclingTrainer.TrainingDatabase.WebApi.Controllers
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