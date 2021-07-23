using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MoedaService
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        static void Main()
        {
#if DEBUG
            new Service1().Processar(); ;
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
        { 
            new YourWindowsService() 
        };
            ServiceBase.Run(ServicesToRun);
#endif
        
        /*ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);*/
        }
    }
}
