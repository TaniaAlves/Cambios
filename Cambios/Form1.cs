using Cambios.Modelos;
using Cambios.Serviços;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cambios
{
    public partial class Form1 : Form
    {
        #region Atributos
        private List<Rate> Rates;

        private NetworkService networkService;

        private ApiService apiService;

        private DialogService dialogService;

        private DataService dataService;
        #endregion
        //public List<Rate> Rates { get; set; } = new List<Rate>();
        public Form1()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();
            LoadRates();
        }

        private async void LoadRates() //async para ser uma tarefa assincrona
        {
            bool load;

            labelResultado.Text = " A atualizar taxas...";

            var connection = networkService.CheckConnection();

            if(!connection.IsSucess)
            {
                LoadLocalRates();
                load = false;
                
            }
            else
            {
                await LoadApiRates();
                load = true;
            }

            if(Rates.Count ==0) //esta a dar erro pq rates esta null
            {
                labelResultado.Text = "Não há ligação á Internet" + Environment.NewLine+ " e não foram " +
                    "préviamente carregasdas as taxas." + Environment.NewLine + " Tente mais tarde!";

                labelStatus.Text = "Primeira inicialização deverá ter ligação à internet";
                return;
            }
            
            comboBoxOrigem.DataSource = Rates;
            comboBoxOrigem.DisplayMember = "Name";

            //corrige bug da microsoft
            comboBoxDestino.BindingContext = new BindingContext();  //para ficarem com opções selecionadas diferentes

            comboBoxDestino.DataSource = Rates;
            comboBoxDestino.DisplayMember = "Name";


            buttonConverter.Enabled = true;
            buttonTroca.Enabled = true;

            labelResultado.Text = " Taxas atualizadas...";

            if (load)
            {
                labelStatus.Text = string.Format("Taxas carregadas da internet em {0:F}", DateTime.Now);
            }
            else
            {
                labelStatus.Text = string.Format("Taxas carregadas da base de dados");
            }

            progressBar1.Value = 100;
        }

        private void LoadLocalRates()
        {
            Rates = dataService.GetData();
        }

        private async Task LoadApiRates()
        {
            progressBar1.Value = 0;

            var response = await apiService.GetRates("http://cambios.somee.com", "/api/Rates");

            Rates = (List<Rate>) response.Result;

            dataService.DeleteData();
            //guardar nas base dados
            dataService.SaveData(Rates);
        }

        private void buttonConverter_Click(object sender, EventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if(string.IsNullOrEmpty(TextBoxValor.Text))
            {
                dialogService.ShowMessage("Erro", "Insira um valor a converter");
                return;
            }

            decimal valor;
            if(!decimal.TryParse(TextBoxValor.Text, out valor))
            {
                dialogService.ShowMessage("Erro de conversão", "Valor terá de ser numérico");
                return;
            }

            if(comboBoxOrigem.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem de escolher uma moeda a converter");
                return;
            }

            if (comboBoxDestino.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem de escolher uma moeda de destino para converter");
                return;
            }

            var taxaOrigem = (Rate)comboBoxOrigem.SelectedItem;
            var taxaDestino = (Rate)comboBoxDestino.SelectedItem;
            var valorConvertido = valor / (decimal)taxaOrigem.TaxRate * (decimal)taxaDestino.TaxRate;
            labelResultado.Text = string.Format("{0} {1:C2} = {2} {3:C2}", taxaOrigem.Code, valor, taxaDestino.Code, valorConvertido);
        }

        private void buttonTroca_Click(object sender, EventArgs e)
        {
            Troca();
        }

        private void Troca()
        {
            var aux = comboBoxOrigem.SelectedItem;
            comboBoxOrigem.SelectedItem = comboBoxDestino.SelectedItem;
            comboBoxDestino.SelectedItem = aux;
            Converter();
        }
    }
}
