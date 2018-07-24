using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net.Mail;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using System.Data.SQLite;
using Tiraggo.Interfaces;
using Watch_List.Classes;
using System.Net;

namespace Watch_List.ViewModels
{
    [POCOViewModel]
    public class MainViewModel
    {
        #region Properties
        protected IDialogService DialogService { get { return this.GetService<IDialogService>(); } }
        protected ISplashScreenService SplashScreenService { get { return this.GetService<ISplashScreenService>(); } }
        protected IMessageBoxService MessageBoxService {  get { return this.GetRequiredService<IMessageBoxService>(); } }
        public virtual TVShowsCollection TVShowsCollection { get; set; }
        public virtual ObservableCollection<TVShows> Shows { get; set; }
        public virtual ObservableCollection<TodayShow> TodayShows { get; set; }
        public virtual string TodayShowInfo { get; set; }
        #endregion

        public static MainViewModel Create()
        {
            return ViewModelSource.Create(() => new MainViewModel());
        }

        protected MainViewModel()
        {
            this.TVShowsCollection = new TVShowsCollection();
            this.Shows = new ObservableCollection<TVShows>();
            this.TodayShows = new ObservableCollection<TodayShow>();
        }

        public void OnLoaded()
        {
            #region Verify API is available
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://www.tvmaze.com");
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                this.MessageBoxService.ShowMessage("Unable to connect to www.tvmaze.com to retrieve information." + Environment.NewLine + 
                    "Please try again later on.", "Error");

                System.Windows.Application.Current.MainWindow.Close();
            }
            response.Close();
            #endregion

            #region Create Database - moved into App.xaml.cs
            //if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TV Shows\\database.db"))
            //{
            //    try
            //    {
            //        this.SplashScreenService.ShowSplashScreen();
            //        SQLiteConnection.CreateFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TV Shows\\database.db");

            //        //SQLiteConnection con = new SQLiteConnection("Data Source=database.db;Version=3;");
            //        string conString = "DataSource=" + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TV Shows\\database.db;Version=3;";
            //        SQLiteConnection con = new SQLiteConnection(conString);
            //        con.Open();

            //        string sql = "CREATE TABLE TVShows (Id integer primary key AUTOINCREMENT UNIQUE, MazeId varchar(25), Title  varchar(100), Synopsis varchar(255), Image varchar(255), NextAirDate varchar(100));";
            //        SQLiteCommand command = new SQLiteCommand(sql, con);
            //        command.ExecuteNonQuery();

            //        sql = "CREATE TABLE Recipients (Id integer primary key AUTOINCREMENT UNIQUE, EmailAddress varchar(255));";
            //        command = new SQLiteCommand(sql, con);
            //        command.ExecuteNonQuery();

            //        sql = "CREATE TABLE MailPreferences (Id integer primary key AUTOINCREMENT UNIQUE, EmailAddress varchar(255), Password varchar(255));";
            //        command = new SQLiteCommand(sql, con);
            //        command.ExecuteNonQuery();

            //        con.Close();
            //        this.SplashScreenService.HideSplashScreen();
            //    }
            //    catch (Exception ex)
            //    {
            //        this.SplashScreenService.HideSplashScreen();
            //        this.MessageBoxService.ShowMessage(ex.Message, "Error");
            //    }
            //}
            #endregion

            #region Populate Data
            try
            {
                //Load all of our shows
                this.TVShowsCollection.LoadAll();

                //Populate our observable collection
                foreach (TVShows show in TVShowsCollection)
                {
                    this.Shows.Add(show);
                }

                //Populate the next air date for each show in the background (async)
                NextAirDate();
            }
            catch (Exception ex)
            {
                this.MessageBoxService.ShowMessage(ex.Message, "Error");
            }
            #endregion
        }

        public void Exit()
        {
            System.Windows.Application.Current.MainWindow.Close();
        }

        public void Lookup()
        {
            try
            {
                var vm = LookupViewModel.Create();

                var result = this.DialogService.ShowDialog(
                    dialogCommands: vm.DialogCommands,
                    title: "TV Show Lookup",
                    documentType: "LookupView",
                    parameter: null,
                    parentViewModel: this,
                    viewModel: vm);

                if (result != null && result.IsDefault)
                {
                    this.SplashScreenService.ShowSplashScreen();

                    TVShows show = new TVShows()
                    {
                        Title = vm.ShowName,
                        MazeId = vm.ShowId,
                        Synopsis = vm.Synopsis,
                        Image = vm.ShowImage,
                        NextAirDate = vm.NextAirDate
                    };

                    show.Save();
                    this.Shows.Add(show);
                    this.SplashScreenService.HideSplashScreen();
                }
            }   
            catch (Exception ex)
            {
                this.SplashScreenService.HideSplashScreen();
                this.MessageBoxService.ShowMessage(ex.Message, "Error");
            }
        }

        public void Delete(TVShows show)
        {
            if (this.MessageBoxService.ShowMessage("Are you sure you wish to delete the selected show?", "Confirm", MessageButton.YesNo, MessageIcon.Question) == MessageResult.Yes)
            {
                try
                {
                    this.SplashScreenService.ShowSplashScreen();
                    using (tgTransactionScope scope = new tgTransactionScope())
                    {
                        show.MarkAsDeleted();
                        this.TVShowsCollection.Save();
                        this.Shows.Remove(show);
                        scope.Complete();
                    }
                    this.SplashScreenService.HideSplashScreen();
                }
                catch (Exception ex)
                {
                    this.SplashScreenService.HideSplashScreen();
                    this.MessageBoxService.ShowMessage(ex.Message, "Error");
                }
            }
        }

        public bool CanDelete(TVShows show)
        {
            return show != null;
        }

        public void Email()
        {
            SendEmails();
        }
        
        public bool CanEmail()
        {
            return true;
        }

        public async void SendEmails()
        {
            MailPreferencesCollection myPrefs = new MailPreferencesCollection();
            myPrefs.LoadAll();
            if (!myPrefs.HasData)
            {
                this.MessageBoxService.ShowMessage("You must setup your email first!", "Mail Not Setup", MessageButton.OK, MessageIcon.Stop);
                return;
            }

            this.SplashScreenService.ShowSplashScreen();
            this.SplashScreenService.SetSplashScreenState("Refreshing next showings...");
            string body = "";

            DataTable shows = new DataTable();
            shows.Columns.Add("Name", typeof(string));
            shows.Columns.Add("Time", typeof(DateTime));
            shows.Columns.Add("Summary", typeof(string));

            DataTable shows2 = new DataTable();
            shows2.Columns.Add("Name", typeof(string));
            shows2.Columns.Add("Time", typeof(DateTime));
            shows2.Columns.Add("Summary", typeof(string));

            foreach (TVShows show in TVShowsCollection)
            {
                var series = await TVMaze.TVMaze.GetSeries(Convert.ToUInt32(show.MazeId), true, false);
                this.SplashScreenService.SetSplashScreenState("Looking up show times for " + series.name + "...");

                foreach (var episode in series.Episodes)
                {
                    if (Convert.ToDateTime(episode.airdate).Date == DateTime.Today.Date)
                    {
                        var convertedDate = DateTime.SpecifyKind(
                            DateTime.Parse(episode.airstamp.ToString()),
                            DateTimeKind.Utc);

                        DataRow dr = shows.NewRow();
                        if (series.network != null && series.network.name != "")
                            dr[0] = series.name + "(" + series.network.name + ")";
                        else if (series.webChannel != null && series.webChannel.name != "")
                            dr[0] = series.name + "(" + series.webChannel.name + ")";
                        else
                            dr[0] = series.name;
                        dr[1] = convertedDate;
                        if (!string.IsNullOrEmpty(episode.summary))
                            dr[2] = episode.summary;
                        else dr[2] = episode.name;
                        shows.Rows.Add(dr);

                    }
                    else if (Convert.ToDateTime(episode.airdate).Date == DateTime.Today.Date.AddDays(1))
                    {
                        var convertedDate = DateTime.SpecifyKind(
                            DateTime.Parse(episode.airstamp.ToString()),
                            DateTimeKind.Utc);

                        DataRow dr = shows2.NewRow();
                        if (series.network != null && series.network.name != "")
                            dr[0] = series.name + "(" + series.network.name + ")";
                        else if (series.webChannel != null && series.webChannel.name != "")
                            dr[0] = series.name + "(" + series.webChannel.name + ")";
                        else
                            dr[0] = series.name;
                        dr[1] = convertedDate;
                        if (!string.IsNullOrEmpty(episode.summary))
                            dr[2] = episode.summary;
                        else dr[2] = episode.name;
                        shows2.Rows.Add(dr);
                    }
                }
            }

            if (shows.Rows.Count > 0)
            {
                var orderedRows = from row in shows.AsEnumerable()
                                  orderby row.Field<DateTime>("Time")
                                  select row;
                DataTable tblOrdered = orderedRows.CopyToDataTable();

                body = "The following shows are scheduled to air today: " + Environment.NewLine + Environment.NewLine;

                foreach (var row in tblOrdered.Rows)
                {
                    body += Convert.ToDateTime(((System.Data.DataRow)row).ItemArray[1]).ToShortTimeString() + ": " + ((System.Data.DataRow)row).ItemArray[0] + Environment.NewLine + ((System.Data.DataRow)row).ItemArray[2] + Environment.NewLine + Environment.NewLine;
                }
            }

            if (shows2.Rows.Count > 0)
            {
                var orderedRows2 = from row in shows2.AsEnumerable()
                                   orderby row.Field<DateTime>("Time")
                                   select row;
                DataTable tblOrdered2 = orderedRows2.CopyToDataTable();

                body += "The following shows are scheduled to air tomorrow: " + Environment.NewLine + Environment.NewLine;

                foreach (var row in tblOrdered2.Rows)
                {
                    body += Convert.ToDateTime(((System.Data.DataRow)row).ItemArray[1]).ToShortTimeString() + ": " + ((System.Data.DataRow)row).ItemArray[0] + Environment.NewLine + ((System.Data.DataRow)row).ItemArray[2] + Environment.NewLine + Environment.NewLine;
                }
            }

            if (shows.Rows.Count > 0 || shows2.Rows.Count > 0)
            {
                this.SplashScreenService.SetSplashScreenState("Creating and sending email...");

                //SmtpClient client = new SmtpClient();
                //client.Port = 587;
                //client.Host = "smtp.gmail.com";
                //client.EnableSsl = true;
                //client.Timeout = 10000;
                //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.UseDefaultCredentials = false;
                //client.Credentials = new System.Net.NetworkCredential(myPrefs[0].EmailAddress, myPrefs[0].Password);

                RecipientsCollection myCol = new RecipientsCollection();
                myCol.LoadAll();

                foreach (Recipients r in myCol)
                {
                    var client = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new System.Net.NetworkCredential(myPrefs[0].EmailAddress.ToString(), myPrefs[0].Password.ToString()),
                        EnableSsl = true
                    };
                    client.Send(myPrefs[0].EmailAddress.ToString(), r.EmailAddress.ToString(), "Today's TV Shows for " + DateTime.Today.ToShortDateString(), body);

                }
                
                this.SplashScreenService.HideSplashScreen();
            }
        }

        public async void NextAirDate()
        {
            this.TodayShowInfo = "Getting today's shows...";
            TodayShows = new ObservableCollection<TodayShow>();
            var tmpShows = new ObservableCollection<TVShows>();
            foreach (TVShows show in this.Shows)
            {
                tmpShows.Add(show);
            }

            foreach (TVShows show in tmpShows)
            {
                var series = await TVMaze.TVMaze.GetSeries(Convert.ToUInt32(show.MazeId), true, false);

                foreach (var episode in series.Episodes)
                {
                    if (episode.airdate >= DateTime.Today)
                    {
                        var convertedDate = DateTime.SpecifyKind(
                            DateTime.Parse(episode.airstamp.ToString()),
                            DateTimeKind.Utc);

                        show.NextAirDate = convertedDate.ToString("MM/dd/yy") + " " + convertedDate.ToShortTimeString();
                        show.Save();

                        if (convertedDate.Date == DateTime.Today)
                        {
                            TodayShow myShow = new TodayShow();
                            myShow.Name = show.Title;
                            myShow.Time = convertedDate;
                            TodayShows.Add(myShow);
                        }
                        break;
                    }
                }
            }
            this.TodayShowInfo = "Today's Shows";
        }

        public void Recipients()
        {
            try
            {
                var vm = RecipientsViewModel.Create();

                var result = this.DialogService.ShowDialog(
                    dialogCommands: vm.DialogCommands,
                    title: "Recipients",
                    documentType: "RecipientsView",
                    parameter: null,
                    parentViewModel: this,
                    viewModel: vm);

                if (result != null && result.IsDefault)
                {
                    
                }
            }
            catch (Exception ex)
            {
                this.SplashScreenService.HideSplashScreen();
                this.MessageBoxService.ShowMessage(ex.Message, "Error");
            }
        }

        public bool CanRecipients()
        {
            return true;
        }

        public void MailSetup()
        {
            try
            {
                var vm = PreferencesViewModel.Create();

                MailPreferencesCollection myPreferences = new MailPreferencesCollection();
                myPreferences.LoadAll();
                if (myPreferences.HasData)
                {
                    vm.EmailAddress = myPreferences[0].EmailAddress;
                    vm.Password = myPreferences[0].Password;
                }

                var result = this.DialogService.ShowDialog(
                    dialogCommands: vm.DialogCommands,
                    title: "Email Setup",
                    documentType: "PreferencesView",
                    parameter: null,
                    parentViewModel: this,
                    viewModel: vm);

                if (result != null && result.IsDefault)
                {
                    if (myPreferences.HasData)
                    {
                        myPreferences[0].EmailAddress = vm.EmailAddress;
                        myPreferences[0].Password = vm.Password;
                        myPreferences[0].Save();
                    }
                    else
                    {
                        MailPreferences pref = new MailPreferences();
                        pref.EmailAddress = vm.EmailAddress;
                        pref.Password = vm.Password;
                        pref.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                this.SplashScreenService.HideSplashScreen();
                this.MessageBoxService.ShowMessage(ex.Message, "Error");
            }
        }

        public bool CanMailSetup()
        {
            return true;
        }
    }
}   