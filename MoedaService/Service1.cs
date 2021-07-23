using MoedaService.Domain;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MoedaService
{
    public partial class Service1 : ServiceBase
    {
        private Timer _timer;
        private List<DadosMoeda> _dadosMoeda;
        private List<DadosCotacao> _dadosCotacao;
         private List<DePara> _dePara;
        private string diretorioRaiz = @"C:\dev\MoedaService\";
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _timer = new Timer(IniciarProcessamento, null, 0, 120000);         
        }
        private void IniciarProcessamento(object state)
        {
            Processar();
        }
        internal void Processar()
        {
            var moedas = GetMoedasFromApi();
            if (moedas!=null)
            {
                ExecutaProcessamento(moedas);
            }

        }

        private void ExecutaProcessamento(IEnumerable<Moeda> moedas)
        {
            _dadosMoeda = GetAllDadosMoeda().ToList();
            _dadosCotacao = GetAllDadosCotacao().ToList();
            _dePara = GetAllDadosDePara().ToList();
            var dadosMoedasList = GetTodasMoedasPeriodo(moedas).ToList();
            var valoresCotacoes = GetValoresCotacoes(dadosMoedasList);
            GerarArquivo(valoresCotacoes);
        }

        private IEnumerable<DadosMoedaCotacao> GetValoresCotacoes(IEnumerable<DadosMoeda> dadosMoeda)
        {
            var listValores =new List<DadosMoedaCotacao>();

            foreach (var item in dadosMoeda)
            {
                int codCotacao = _dePara.Where(x => x.ID_MOEDA == item.ID_MOEDA).Select(x => x.cod_cotacao).FirstOrDefault();
                var result = _dadosCotacao.Where(x => x.cod_cotacao == codCotacao).Select(x => x.vlr_cotacao).FirstOrDefault();

                var moedaCotacao = new DadosMoedaCotacao
                {
                    ID_MOEDA = item.ID_MOEDA,
                    DATA_REF = item.DATA_REF,
                    VL_COTACAO = result
                };

                listValores.Add(moedaCotacao);

            }
            return listValores;
        }

      
       
        protected override void OnStop()
        {
        }

       

        private void GerarArquivo(IEnumerable<DadosMoedaCotacao> listMoedaCotacoes)
        {
            var data = DateTime.Now.ToString("yyyyMMdd_hhmmss");
            string diretorio = $" {diretorioRaiz}Resultado {data}.csv";
            StreamWriter vWriter = new StreamWriter(diretorio, true);
            vWriter.WriteLine("ID_MOEDA;DATA_REF;VL_COTACAO");
            foreach (var item in listMoedaCotacoes)
            {
                vWriter.WriteLine($"{item.ID_MOEDA};{item.DATA_REF};{item.VL_COTACAO}");
            }          
           
            vWriter.Flush();
            vWriter.Close();
        }


        private IEnumerable<Moeda> GetMoedasFromApi()
        {
            var client = new RestClient("https://localhost:44313/");
            var request = new RestRequest("api/Fila/", Method.GET);
            var queryResult = client.Execute<List<Moeda>>(request).Data;

            return queryResult.ToList();
        }


            private IEnumerable<DadosMoeda> GetTodasMoedasPeriodo(IEnumerable<Moeda> moedasList)
            {
          
            var listMoedas = new List<DadosMoeda>();
            
            foreach (var moeda in moedasList)
            {
                var result = _dadosMoeda.Where(x =>  moeda.data_inicio>= x.DATA_REF && moeda.data_fim <= x.DATA_REF);
                listMoedas.AddRange(result.ToList());
            }
            return listMoedas;
        }

       

        private IEnumerable<DadosMoeda> GetAllDadosMoeda()
        {
            var reader = new StreamReader(File.OpenRead(@"C:\dev\MoedaService\DadosMoeda.csv"));
            var listMoedas = new List<DadosMoeda>();
           
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                var moeda = new DadosMoeda
                {
                    ID_MOEDA = values[0],
                    DATA_REF = DateTime.Parse(values[1])
                };

                listMoedas.Add(moeda);     
            }
            return listMoedas.ToList();
        }

        private IEnumerable<DadosCotacao> GetAllDadosCotacao()
        {
            var reader = new StreamReader(File.OpenRead(@"C:\dev\MoedaService\DadosMoeda.csv"));
            var list = new List<DadosCotacao>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                var contacao = new DadosCotacao
                {
                    vlr_cotacao = float.Parse( values[0]),
                    cod_cotacao = int.Parse(values[1]),
                    dat_cotacao = DateTime.Parse(values[2])
                };

                list.Add(contacao);
            }
            return list.ToList();
        }

        private IEnumerable<DePara> GetAllDadosDePara()
        {
            var reader = new StreamReader(File.OpenRead(@"C:\dev\MoedaService\DePara.csv"));
            var list = new List<DePara>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                var depara = new DePara
                {
                    ID_MOEDA = values[0],
                    cod_cotacao = int.Parse(values[1])
                   
                };

                list.Add(depara);
            }
            return list.ToList();
        }


    }
}
