using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WazeCredit.Service.LifeTimeExample;

namespace WazeCredit.Controllers
{
    public class LifeTimeController : Controller
    {
        private readonly TransientService _transientService;
        private readonly TransientService _transientService1;

        private readonly ScopedService _scopedService;
        private readonly SingletonService _singletonService;

        public LifeTimeController(TransientService transientService,
            ScopedService scopedService, SingletonService singletonService, TransientService transientService1)
        {
            _transientService = transientService;
            _scopedService = scopedService;
            _singletonService = singletonService;
            _transientService1 = transientService1;
        }

        public IActionResult Index()
        {
            var messages = new List<String>
            {
                HttpContext.Items["CustomMiddlewareTransient"].ToString(),
                $"Transient Controller - {_transientService.GetGuid()}",
                $"Transient1 Controller - {_transientService1.GetGuid()}",
                HttpContext.Items["CustomMiddlewareScoped"].ToString(),
                $"Scoped Controller - {_scopedService.GetGuid()}",
                HttpContext.Items["CustomMiddlewareSingleton"].ToString(),
                $"Singleton Controller - {_singletonService.GetGuid()}",
            };

            return View(messages);
        }
    }
}