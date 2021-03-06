﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsFormCountries
{
    public partial class Form1 : Form
    {
        //lista zemalja, definirana u Country.cs, prazna lista
        public List<Country> lCountries;
        public Form1()
        {
            InitializeComponent();
            //DATA GRID
            lCountries = GetCountries();
            //izvor podataka za listu se nalazi u objektu dataGridViewCountries
            dataGridViewCountries.DataSource = lCountries;

            //COMBO BOX filtriranje
            //lista za regije, lista je string tako da sprema samo string vrijednosti, dohvaća vrijednosti Region
            List<String> lRegions = lCountries.Where(o => o.sRegion != "").Select(o => o.sRegion).Distinct().ToList();
            //Na index 0 u listi dodajemo odabir "Svi kontinenti
            lRegions.Insert(0, "Svi kontinenti");
            comboBoxRegion.DataSource = lRegions; //uspoređuje naziv člana padajućeg izbornika s nazivom regije iz liste lRegions
            comboBoxRegions.DataSource = lRegions;

            //COMBO BOX sortiranje
            List<String> lSortCriterias = new List<String>()
            {
                "‐",
                "Glavni grad",
                "Naziv",
                "Broj stanovnika",
                "Povrsina"
            };
            comboBoxSort.DataSource = lSortCriterias;
        }

        private void comboBoxRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            //čitanje odabrane vrijednosti
            string sRegion = (string)comboBoxRegion.SelectedItem; // odabrana vrijednost
            lCountries = GetCountries();
            if (sRegion != "Svi kontinenti")
            {
                lCountries = lCountries.Where(o => o.sRegion == sRegion).ToList();
                dataGridViewCountries.DataSource = lCountries;
            }
            else
            {
                dataGridViewCountries.DataSource = lCountries;
            }
        }

        private void comboBoxRegions_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sRegion = (string)comboBoxRegions.SelectedItem; // odabrana vrijednost
            lCountries = GetCountries();
            if (sRegion != "Svi kontinenti")
            {
                lCountries = lCountries.Where(o => o.sRegion == sRegion).ToList();
                dataGridViewCountries.DataSource = lCountries;
            }
            else
            {
                dataGridViewCountries.DataSource = lCountries;
            }
        }

        private void comboBoxSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            //sortiranje odabrane vrijednosti
            string sSortCriteria = (string)comboBoxSort.SelectedItem;//odabrana vrijednost
            switch (sSortCriteria)
            {
                case "-":
                    break;
                case "Glavni grad":
                    dataGridViewCountries.DataSource = lCountries.OrderBy(o => o.sCapital).ToList();
                    break;
                case "Naziv":
                    dataGridViewCountries.DataSource = lCountries.OrderBy(o => o.sName).ToList();
                    break;
                case "Broj stanovnika":
                    dataGridViewCountries.DataSource = lCountries.OrderByDescending(o => o.nPopulation).ToList();
                    break;
                case "Povrsina":
                    dataGridViewCountries.DataSource = lCountries.OrderByDescending(o => o.fArea).ToList();
                    break;
            }
        }

        public List<Country> GetCountries()
        {
            List<Country> lRESTCountries = new List<Country>();
            //Dodajemo System.Configuration i uključujemo ga u references
            //instalirati preko NuGet packages NewtonSoft.json
            string sUrl = System.Configuration.ConfigurationManager.AppSettings["RestApiUrl"];
            string sJson = CallRestMethod(sUrl);
            JArray json = JArray.Parse(sJson);
            foreach (JObject item in json)
            {
                //Čitanje vrijednosti iz JSON datoteke
                string code = (string)item.GetValue("alpha2Code");
                string name = (string)item.GetValue("name");
                string capital = (string)item.GetValue("capital");
                int population = (int)item.GetValue("population");
                float area = -1;
                //exception catcher, vrijednost područja ne može biti NULL
                if (item.GetValue("area").Type == JTokenType.Null)
                {
                    area = 0;
                }
                else
                {
                    area = (float)item.GetValue("area");
                }
                string region = (string)item.GetValue("region");
                //Dodavanje objekata u listu
                lRESTCountries.Add(new Country
                {
                    sCode = code,
                    sName = name,
                    sCapital = capital,
                    nPopulation = population,
                    sRegion = region,
                    fArea = area,
                });
            }
            return lRESTCountries;
        }
        //za ovu funkciju moramo uključiti System.net i System.IO
        public static string CallRestMethod(string url)
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.Method = "GET";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            //login mogućnost zakomentirana
            //webrequest.Headers.Add("Username", "xyz");
            //webrequest.Headers.Add("Password", "abc");
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(),
            enc);
            string result = string.Empty;
            result = responseStream.ReadToEnd();
            webresponse.Close();
            return result;
        }

        private void textBoxSearch_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSearch_click(object sender, EventArgs e)
        {
            string sPretrazi = textBoxSearch.Text;
            var vPretrazi = from c in lCountries where c.sName.Contains(sPretrazi) select c;
            List<Country> lPretrazeneDrzave = vPretrazi.ToList();
            dataGridViewCountries.DataSource = lPretrazeneDrzave.OrderBy(o => o.sName).ToList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string NewCode = textBox2.Text;
            string NewName = textBox3.Text;
            string NewCapital = textBox4.Text;
            int NewPopulation = Convert.ToInt32(textBox5.Text);
            float NewArea = Convert.ToSingle(textBox6.Text);
            string NewRegion = comboBoxRegions.Text;
            Country Countries = new Country()
            {
                sCode = NewCode,
                sName = NewName,
                sCapital = NewCapital,
                nPopulation = NewPopulation,
                fArea = NewArea,
                sRegion = NewRegion
            };
            lCountries.Add(Countries);
            dataGridViewCountries.DataSource = lCountries;
            MessageBox.Show("Zemlja uspješno dodana!");
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }
    }
}