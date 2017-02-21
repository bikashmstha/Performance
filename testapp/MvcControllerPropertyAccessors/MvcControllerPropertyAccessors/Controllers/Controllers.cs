using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MvcControllerPropertyAccessors.Controllers
{
    public class Controller1Controller : Controller
    {
        private readonly string _controllerName;

        public Controller1Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller2Controller : Controller
    {
        private readonly string _controllerName;

        public Controller2Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller3Controller : Controller
    {
        private readonly string _controllerName;

        public Controller3Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller4Controller : Controller
    {
        private readonly string _controllerName;

        public Controller4Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller5Controller : Controller
    {
        private readonly string _controllerName;

        public Controller5Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller6Controller : Controller
    {
        private readonly string _controllerName;

        public Controller6Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller7Controller : Controller
    {
        private readonly string _controllerName;

        public Controller7Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller8Controller : Controller
    {
        private readonly string _controllerName;

        public Controller8Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller9Controller : Controller
    {
        private readonly string _controllerName;

        public Controller9Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller10Controller : Controller
    {
        private readonly string _controllerName;

        public Controller10Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller11Controller : Controller
    {
        private readonly string _controllerName;

        public Controller11Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller12Controller : Controller
    {
        private readonly string _controllerName;

        public Controller12Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller13Controller : Controller
    {
        private readonly string _controllerName;

        public Controller13Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller14Controller : Controller
    {
        private readonly string _controllerName;

        public Controller14Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller15Controller : Controller
    {
        private readonly string _controllerName;

        public Controller15Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller16Controller : Controller
    {
        private readonly string _controllerName;

        public Controller16Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller17Controller : Controller
    {
        private readonly string _controllerName;

        public Controller17Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller18Controller : Controller
    {
        private readonly string _controllerName;

        public Controller18Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller19Controller : Controller
    {
        private readonly string _controllerName;

        public Controller19Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller20Controller : Controller
    {
        private readonly string _controllerName;

        public Controller20Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller21Controller : Controller
    {
        private readonly string _controllerName;

        public Controller21Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller22Controller : Controller
    {
        private readonly string _controllerName;

        public Controller22Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller23Controller : Controller
    {
        private readonly string _controllerName;

        public Controller23Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller24Controller : Controller
    {
        private readonly string _controllerName;

        public Controller24Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }

    public class Controller25Controller : Controller
    {
        private readonly string _controllerName;

        public Controller25Controller()
        {
            _controllerName = this.GetType().GetTypeInfo().Name;
        }

        [FromQuery]
        public string StringProperty { get; set; }

        [FromQuery]
        public int IntProperty { get; set; }

        public IActionResult Index()
        {
            return Content($"{_controllerName}.Index");
        }

        public IActionResult Contact()
        {
            return Content($"{_controllerName}.Contact");
        }
    }
}
