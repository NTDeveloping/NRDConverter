using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NinjaTrader.Data;
using NinjaTrader.Cbi;
using System.Data.SqlServerCe;
using System.IO;

namespace NRDConverter
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
       
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("CONVERTED"))
                Directory.CreateDirectory("CONVERTED");

            string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal)+"\\NinjaTrader 8\\db\\NinjaTrader.sdf";
            
            SqlCeConnection scc = new SqlCeConnection(@"data source="+md);
            scc.Open();

            string query = "SELECT * FROM MasterInstruments ORDER BY Name";

            SqlCeCommand command = new SqlCeCommand(query, scc);

            SqlCeDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int c = 20 - reader[5].ToString().Length;
                string s = new string(' ', c>0 ? c : 20);
                MI tmp = new MI();
                tmp.Name = reader[5].ToString();
                tmp.TickSize = Convert.ToDouble(reader[8]);
                tmp.Type = (InstrumentType)reader[3];
                lstMaster.Add(tmp);
            }
            reader.Close();

            FillTreeView();
        }
        private void FillTreeView()
        {
            string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\NinjaTrader 8\\db\\replay";
            string[] dirs = Directory.GetDirectories(md, "*", SearchOption.AllDirectories);
            for (int i = 0; i < dirs.Length; i++)
            {
                string dirName = new DirectoryInfo(dirs[i]).Name;
                tv1.Nodes.Add(dirName);
                string[] files = Directory.GetFiles(dirs[i], "*.nrd", SearchOption.AllDirectories);
                for (int j = 0; j < files.Length; j++)
                    tv1.Nodes[i].Nodes.Add(Path.GetFileNameWithoutExtension(files[j]));
            }
        }
        List<MI> lstMaster = new List<MI>();
        private void button3_Click(object sender, EventArgs e)
        {
            if (tv1.SelectedNode == null)
                return;
            try
            {
                if (tv1.SelectedNode.Level == 1)
                {
                    string md = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\NinjaTrader 8\\db\\replay\\";
                    string fn = md + tv1.SelectedNode.Parent.Text + "\\" + tv1.SelectedNode.Text + ".nrd";
                    if (File.Exists(fn))
                    {
                        button3.Enabled = false;
                        string[] s = tv1.SelectedNode.Parent.Text.Split(' ');
                        string MIname = s[0];
                        MI tmp = lstMaster.First(i => i.Name == MIname);
                        MasterInstrument mi = new MasterInstrument();
                        mi.Name = tmp.Name;
                        mi.InstrumentType = tmp.Type;
                        mi.TickSize = tmp.TickSize;
                        DateTime exp = new DateTime(2099, 12, 12);
                        int month = 12;
                        int year = 0;
                        if (s.Length == 2)
                        {
                            month = Convert.ToInt16(s[1].Split('-')[0]);
                            year = 2000 + Convert.ToInt16(s[1].Split('-')[1]);
                            exp = new DateTime(year, month, 15);
                        }

                        Instrument instrument = new Instrument
                        {
                            Exchange = Exchange.Default,
                            Expiry = exp,
                            MasterInstrument = mi,
                        };
                        DateTime dt = new DateTime(Convert.ToInt16(tv1.SelectedNode.Text.Substring(0, 4)),
                            Convert.ToInt16(tv1.SelectedNode.Text.Substring(4, 2)),
                            Convert.ToInt16(tv1.SelectedNode.Text.Substring(6, 2)));
                        if (!Directory.Exists("CONVERTED\\" + tv1.SelectedNode.Parent.Text + "\\"))
                            Directory.CreateDirectory("CONVERTED\\" + tv1.SelectedNode.Parent.Text + "\\");
                        MarketReplay.DumpMarketDepth(instrument,
                            dt.AddDays(1), dt.AddDays(1),
                            "CONVERTED\\" + tv1.SelectedNode.Parent.Text + "\\" + tv1.SelectedNode.Text + ".txt");
                        button3.Enabled = true;
                        MessageBox.Show("Сonversion completed");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error"+Environment.NewLine+ex.Message);
                button3.Enabled = true;
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tv1.Nodes.Clear();
            FillTreeView();
        }
    }


    public class MI
    {
        public string Name { get; set; }
        public double TickSize { get; set; }
        public InstrumentType Type { get; set; }
    }
    
}
