using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using DevExpress.Xpf.Core;
using Tiraggo.Core;
using Tiraggo.Interfaces;
using System.Data.SQLite;

namespace Watch_List
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnAppStartup_UpdateThemeName(object sender, StartupEventArgs e)
        {
            DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();

            this.SetConnections();
        }

        void SetConnections()
        {
            #region Database
            if (!System.IO.File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TV Shows\\database.db"))
            {
                if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TV Shows"))
                    System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TV Shows");

                SQLiteConnection.CreateFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TV Shows\\database.db");

                SQLiteConnection con = new SQLiteConnection("DataSource=" + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TV Shows\\database.db;Version=3;");
                con.Open();

                string sql = "CREATE TABLE TVShows (Id integer primary key AUTOINCREMENT UNIQUE, MazeId varchar(25), Title  varchar(100), Synopsis varchar(255), Image varchar(255), NextAirDate varchar(100));";
                SQLiteCommand command = new SQLiteCommand(sql, con);
                command.ExecuteNonQuery();

                sql = "CREATE TABLE Recipients (Id integer primary key AUTOINCREMENT UNIQUE, EmailAddress varchar(255));";
                command = new SQLiteCommand(sql, con);
                command.ExecuteNonQuery();

                sql = "CREATE TABLE MailPreferences (Id integer primary key AUTOINCREMENT UNIQUE, EmailAddress varchar(255), Password varchar(255));";
                command = new SQLiteCommand(sql, con);
                command.ExecuteNonQuery();

                con.Close();
            }
            #endregion

            #region Setup Tiraggo Defaults
            Tiraggo.Interfaces.tgProviderFactory.Factory = new Tiraggo.Loader.tgDataProviderFactory();
            tgEntity.ConvertEmptyStringToNull = true;

            tgConnectionElement conn = new tgConnectionElement();

            string conString = "DataSource=" + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\TV Shows\\database.db;Version=3;";
            conn.ConnectionString = conString;
            conn.Name = "SQLite";
            conn.Provider = "Tiraggo.SQLiteProvider";
            conn.ProviderClass = "DataProvider";
            conn.SqlAccessType = tgSqlAccessType.DynamicSQL;
            conn.ProviderMetadataKey = "tgDefault";

            tgConfigSettings.ConnectionInfo.Connections.Add(conn);
            tgConfigSettings.ConnectionInfo.Default = "SQLite";
            #endregion
        }
    }
}
