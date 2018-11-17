using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace LadderGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string settingsFileName = "Settings.ini";
        bool initialized = false;
        List<string> selectedMaps = new List<string>();
        List<string> selectedPlayer1 = new List<string>();
        List<string> selectedPlayer2 = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
                   
            LoadSettings();

            if (textBoxStarcraftExe.Text.Length == 0)
                SetStarcraftExe();

            if (textBoxLadderExe.Text.Length == 0)
                SetLadderManagerExe();

            Populate();
            initialized = true;
        }

        private void SaveSettings() {
            var iniFile = new IniFile(settingsFileName);
            iniFile.Write("StarcraftExe", textBoxStarcraftExe.Text);
            iniFile.Write("LadderManagerExe", textBoxLadderExe.Text);
            iniFile.Write("Iterations", textBoxIterations.Text);
            
            List<string> mapLines = new List<string>();
            foreach (var itemMap in listBoxMaps.SelectedItems) {
                var mapName = ((ListBoxItem) itemMap).Content.ToString();
                mapLines.Add(mapName);
            }
            iniFile.Write("Maps", String.Join("|", mapLines));
            
            List<string> botLines = new List<string>();
            foreach (var itemBot in listBoxPlayer1.SelectedItems) {
                var botName = ((ListBoxItem) itemBot).Content.ToString();
                botLines.Add(botName);
            }
            iniFile.Write("Player1", String.Join("|", botLines));

            botLines.Clear();
            foreach (var itemBot in listBoxPlayer2.SelectedItems) {
                var botName = ((ListBoxItem) itemBot).Content.ToString();
                botLines.Add(botName);
            }
            iniFile.Write("Player2", String.Join("|", botLines));
            
        }


        private void LoadSettings() {
            var iniFile = new IniFile(settingsFileName);
            
            textBoxStarcraftExe.Text = iniFile.Read("StarcraftExe");
            
            string mapsFolder = GetMapsFolder();
            if (mapsFolder == null) 
                textBoxStarcraftExe.Text = "";

            
            textBoxLadderExe.Text = iniFile.Read("LadderManagerExe");
            if (!File.Exists(textBoxLadderExe.Text))
                textBoxLadderExe.Text = "";
            
            int iterations = 0;
            Int32.TryParse(iniFile.Read("Iterations"), out iterations);
            if (iterations > 0)                         
                textBoxIterations.Text = String.Format("{0}", iterations);
            else
                textBoxIterations.Text = "1";
                        
            var maps = iniFile.Read("Maps");
            if (maps.Length > 0)
                selectedMaps = maps.Split('|').ToList();           
                        
            var player1 = iniFile.Read("Player1");
            if (player1.Length > 0)
                selectedPlayer1 = player1.Split('|').ToList();      
            
            var player2 = iniFile.Read("Player2");
            if (player2.Length > 0)
                selectedPlayer2 = player2.Split('|').ToList();            

        }
        
        private void SetStarcraftExe()
        {
            string myDocuments = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string executeInfo = System.IO.Path.Combine(myDocuments, "Starcraft II", "ExecuteInfo.txt");

            if (!File.Exists(executeInfo)) return;
            string starcraftExe = null;
            string[] lines = File.ReadAllLines(executeInfo);
            foreach (string line in lines)
            {
                string argument = line.Substring(line.IndexOf('=') + 1).Trim();
                if (line.Trim().StartsWith("executable"))
                {
                    starcraftExe = argument;
                }
            }
            
            textBoxStarcraftExe.Text = starcraftExe;
        }

        private void SetLadderManagerExe()
        {            
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*LadderServer.exe", SearchOption.AllDirectories);
            foreach (var file in files) {
                textBoxLadderExe.Text = file;
                return;
            }                        

            files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*LadderManager.exe", SearchOption.AllDirectories);
            foreach (var file in files) {
                textBoxLadderExe.Text = file;
                return;
            }                        
        }

        private string GetMapsFolder() {
            if (textBoxStarcraftExe.Text.Length == 0) return null;

            try {
                var starcraftDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(textBoxStarcraftExe.Text))), "Maps");
                if (System.IO.Directory.Exists(starcraftDir)) return starcraftDir;

                starcraftDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(textBoxStarcraftExe.Text)), "Maps");
                if (System.IO.Directory.Exists(starcraftDir)) return starcraftDir;
            
                starcraftDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(textBoxStarcraftExe.Text), "Maps");
                if (System.IO.Directory.Exists(starcraftDir)) return starcraftDir;
            }
            catch { }

            return null;
        }
        
        private string GetBotsFolder() {
            if (textBoxLadderExe.Text.Length == 0) return null;

            
            var botsDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(textBoxLadderExe.Text), "Bots");
            if (System.IO.Directory.Exists(botsDir)) return botsDir;

            botsDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(textBoxLadderExe.Text)), "Bots");
            if (System.IO.Directory.Exists(botsDir)) return botsDir;

            return null;
        }

        private string GetLadderManagerFolder() {
            if (textBoxLadderExe.Text.Length == 0) return null;
            var ladderDir = System.IO.Path.GetDirectoryName(textBoxLadderExe.Text);
            if (System.IO.Directory.Exists(ladderDir)) return ladderDir;

            return null;
        }

        private void Populate() {               
            listBoxMaps.Items.Clear();
            listBoxPlayer1.Items.Clear();
            listBoxPlayer2.Items.Clear();

            if (textBoxIterations.Text.Length == 0)
                textBoxIterations.Text = "1";

            string mapsFolder = GetMapsFolder();
            if (mapsFolder != null) {
                foreach (var fn in Directory.GetFiles(mapsFolder, "*.SC2Map").Select(System.IO.Path.GetFileName)) {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = fn;
                    if (selectedMaps.Contains(fn))
                        item.IsSelected = true;
                    listBoxMaps.Items.Add(item);
                }
            }
            
            string botsFolder = GetBotsFolder();
            if (botsFolder != null) {
                foreach (var fn in Directory.GetDirectories(botsFolder).Select(System.IO.Path.GetFileName)) {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = fn;
                    if (selectedPlayer1.Contains(fn))
                        item.IsSelected = true;
                    listBoxPlayer1.Items.Add(item);
                }

                foreach (var fn in Directory.GetDirectories(botsFolder).Select(System.IO.Path.GetFileName)) {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = fn;
                    if (selectedPlayer2.Contains(fn))
                        item.IsSelected = true;
                    listBoxPlayer2.Items.Add(item);
                }
            }

            UpdateInfo();
            SaveSettings();
        }



        private void UpdateInfo() {            
            if (textBlockInfo == null) return;

            int iterations = 0;
            bool result = Int32.TryParse(textBoxIterations.Text, out iterations);

            var matches = listBoxMaps.SelectedItems.Count * listBoxPlayer1.SelectedItems.Count * listBoxPlayer2.SelectedItems.Count * iterations;
            textBlockInfo.Text = String.Format("Keep in mind you can select multiple maps and bots by holding CTRL or SHIFT and clicking in the list boxes.\n\nYour current configuration will generate {0} matches.", matches);

            buttonGenerate.IsEnabled = (matches != 0);
        }

        private void GenerateAndRun() {
            List<string> mapLines = new List<string>();
            
            foreach (var itemMap in listBoxMaps.Items) {
                var mapName = ((ListBoxItem) itemMap).Content.ToString();
                mapLines.Add(mapName);
            }
            var mapListFile = System.IO.Path.Combine(GetLadderManagerFolder(), "MapListFile");
            System.IO.File.WriteAllLines(mapListFile, mapLines);


            Int32.TryParse(textBoxIterations.Text, out var iterations);

            List<string> matchupLines = new List<string>();

            for (var iteration=0; iteration <= iterations; iteration++) {
                foreach (var itemMap in listBoxMaps.SelectedItems) {
                    var mapName = ((ListBoxItem) itemMap).Content.ToString();
                    foreach (var itemPlayer1 in listBoxPlayer1.SelectedItems) {
                        var bot1 = ((ListBoxItem) itemPlayer1).Content.ToString();

                        foreach (var itemPlayer2 in listBoxPlayer2.SelectedItems) {
                            var bot2 = ((ListBoxItem) itemPlayer2).Content.ToString();

                            string line = String.Format("\"{0}\"vs\"{1}\" {2}", bot1, bot2, mapName);
                            matchupLines.Add(line);
                        }
                    }
                }
            }

            var matchupFileName = System.IO.Path.Combine(GetLadderManagerFolder(), "matchupList");
            System.IO.File.WriteAllLines(matchupFileName, matchupLines);

            try {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = textBoxLadderExe.Text;
                processInfo.ErrorDialog = true;
                processInfo.UseShellExecute = true;
                processInfo.RedirectStandardOutput = false;
                processInfo.RedirectStandardError = false;
                processInfo.WorkingDirectory = GetLadderManagerFolder();
                Process proc = Process.Start(processInfo);
            }
            catch {
                MessageBox.Show("Cannot launch Ladder Manager using: " + textBoxLadderExe.Text, "Unable to launch Ladder Manager");
            }
        }


        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {            
            if (!initialized) return;
            UpdateInfo();
            SaveSettings();
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!initialized) return;
            if (Equals(sender, buttonSelectLadderManager)) {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "SC2 Ladder Manager|*.exe";
                openFileDialog.FilterIndex = 0;
                openFileDialog.RestoreDirectory = true;

                var success = openFileDialog.ShowDialog() ;
                if (success == true) {
                    textBoxLadderExe.Text = openFileDialog.FileName;                
                    Populate(); 
                }
            }
            else if (Equals(sender, buttonSelectStarcraft)) {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "SC2 Executable|*.exe";
                openFileDialog.FilterIndex = 0;
                openFileDialog.RestoreDirectory = true;

                var success = openFileDialog.ShowDialog() ;
                if (success == true) {
                    textBoxStarcraftExe.Text = openFileDialog.FileName;
                    Populate();
                }
            }
            else if (Equals(sender, buttonGenerate)) {
                GenerateAndRun();
            }
        }

        private void textBoxIterations_TextChanged(object sender, TextChangedEventArgs e) {
            if (!initialized) return;
            UpdateInfo();
            SaveSettings();
        }
    }
}
