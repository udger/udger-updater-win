/*
  Udger-update - Data updater for udger local and cloud parser
 
  author     The Udger.com Team (info@udger.com)
  copyright  Copyright (c) Udger s.r.o.
  license    GNU Lesser General Public License
  link       https://udger.com/products

*/
using System;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.IO.Compression;

namespace udger_updater
{
    class Program
    {
        static void Main(string[] args)
        {            
            string accessKey;
            string global_destination_directory;
            string destination_directory;
            string localVersion;
            string remoteVersion;            
            FileStream fileStream;

            Console.WriteLine("udger-updater  version " + Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine("Copyright (c) udger.com");
            Console.WriteLine("");

            #region init, validate
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Error: missing parametr");
                Console.Error.WriteLine("");
                Console.Error.WriteLine("usage: udger-updater.exe <config_file>");
                Console.Error.WriteLine("  config_file   ex: c:\\udger\\my.config");
                Console.Error.WriteLine("");
                Environment.Exit(1);

            }
            if (!File.Exists(args[0]))
            {
                Console.Error.WriteLine("Error: config_file not found");
                Console.Error.WriteLine("");
                Environment.Exit(1);

            }
            else
            {
                setFromConfigFile(args);
            }

            accessKey = ConfigurationManager.AppSettings["access_key"];
            if (String.IsNullOrEmpty(accessKey))
            {
                Console.Error.WriteLine("Error: access_key not set");
                Console.Error.WriteLine("");
                Environment.Exit(1);
            }

            global_destination_directory = ConfigurationManager.AppSettings["global_destination_directory"];            
            if (String.IsNullOrEmpty(global_destination_directory))
            {
                Console.Error.WriteLine("Error: destination_directory not set");
                Console.Error.WriteLine("");
                Environment.Exit(5);
            }
            if (!Directory.Exists(global_destination_directory))
            {
                Console.Error.WriteLine("Error: global_destination_directory not found");
                Console.Error.WriteLine("");
                Environment.Exit(1);

            }

            // check local and remote version
            if (File.Exists(global_destination_directory + "\\version"))
            {
                fileStream = new FileStream(global_destination_directory + "\\version", FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream))
                {
                    localVersion = streamReader.ReadToEnd();
                }                
            }
            else
            {
                localVersion = "00000000-00";
            }            

            downloadFile(accessKey, "version", global_destination_directory);
            fileStream = new FileStream(global_destination_directory + "\\version", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream))
            {
                remoteVersion = streamReader.ReadToEnd();
            }
            Console.WriteLine("Local ver: " + localVersion + " - Remote ver: " + remoteVersion);
            if(localVersion == remoteVersion)
            {
                Console.WriteLine("Local files is up to date, exiting");
                Environment.Exit(0);
            }
            #endregion

            #region get data files (Local parser subscription)
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["udgerdb_v3.dat"]))
            {
                destination_directory = GetDestinationDirectory("udgerdb_v3.dat", global_destination_directory);
                procesFile(accessKey, "udgerdb_v3.dat", destination_directory, "udgerdb_v3_dat.md5");
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["udgerdb.dat"]))
            {
                destination_directory = GetDestinationDirectory("udgerdb.dat", global_destination_directory);
                procesFile(accessKey, "udgerdb.dat", destination_directory, "udgerdb_dat.md5");
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["udgerdata_old.ini"]))
            {
                destination_directory = GetDestinationDirectory("udgerdata_old.ini", global_destination_directory);
                procesFile(accessKey, "udgerdata_old.ini", destination_directory, "udgerdata_old_ini.md5");
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["udgerdata_old.xml"]))
            {
                destination_directory = GetDestinationDirectory("udgerdata_old.xml", global_destination_directory);
                procesFile(accessKey, "udgerdata_old.xml", destination_directory, "udgerdata_old_xml.md5");
            }
            #endregion

            #region get icons files  (Local and Cloud parser subscription)
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ico_ua.zip"]))
            {
                destination_directory = GetDestinationDirectory("ico_ua.zip", global_destination_directory);
                procesFile(accessKey, "ico_ua.zip", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ico_os.zip"]))
            {
                destination_directory = GetDestinationDirectory("ico_os.zip", global_destination_directory);
                procesFile(accessKey, "ico_os.zip", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["ico_device.zip"]))
            {
                destination_directory = GetDestinationDirectory("ico_device.zip", global_destination_directory);
                procesFile(accessKey, "ico_device.zip", destination_directory);
            }
            #endregion

            #region get CSV files (Local and Cloud parser subscription)
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["botIP.csv"]))
            {
                destination_directory = GetDestinationDirectory("botIP.csv", global_destination_directory);
                procesFile(accessKey, "botIP.csv", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["uas_example.csv"]))
            {
                destination_directory = GetDestinationDirectory("uas_example.csv", global_destination_directory);
                procesFile(accessKey, "uas_example.csv", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["uasOS_example.csv"]))
            {
                destination_directory = GetDestinationDirectory("uasOS_example.csv", global_destination_directory);
                procesFile(accessKey, "uasOS_example.csv", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["uaDEVICE_example.csv"]))
            {
                destination_directory = GetDestinationDirectory("uaDEVICE_example.csv", global_destination_directory);
                procesFile(accessKey, "uaDEVICE_example.csv", destination_directory);
            }
            #endregion

            #region get CSV files (Local parser subscription)
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["anonymizing_vpn_service.csv"]))
            {
                destination_directory = GetDestinationDirectory("anonymizing_vpn_service.csv", global_destination_directory);
                procesFile(accessKey, "anonymizing_vpn_service.csv", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["fake_crawler.csv"]))
            {
                destination_directory = GetDestinationDirectory("fake_crawler.csv", global_destination_directory);
                procesFile(accessKey, "fake_crawler.csv", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["known_attack_source.csv"]))
            {
                destination_directory = GetDestinationDirectory("known_attack_source.csv", global_destination_directory);
                procesFile(accessKey, "known_attack_source.csv", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["public_cgi_proxy.csv"]))
            {
                destination_directory = GetDestinationDirectory("public_cgi_proxy.csv", global_destination_directory);
                procesFile(accessKey, "public_cgi_proxy.csv", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["public_web_proxy.csv"]))
            {
                destination_directory = GetDestinationDirectory("public_web_proxy.csv", global_destination_directory);
                procesFile(accessKey, "public_web_proxy.csv", destination_directory);
            }

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["tor_exit_node.csv"]))
            {
                destination_directory = GetDestinationDirectory("tor_exit_node.csv", global_destination_directory);
                procesFile(accessKey, "tor_exit_node.csv", destination_directory);
            }
            #endregion
        }

        #region proces file
        static void procesFile(string access_key, string filename, string destination_dir, string md5filename="")
        {
            string tmpMD5;
            string tempPath = System.IO.Path.GetTempPath();
            Boolean md5 = true;

            downloadFile(access_key, filename, tempPath);

            if (!String.IsNullOrEmpty(md5filename))
            {
                downloadFile(access_key, md5filename, tempPath);

                FileStream fileStream = new FileStream(tempPath + md5filename, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(fileStream))
                {
                    tmpMD5 = streamReader.ReadToEnd();
                }
                File.Delete(tempPath + md5filename);

                if (tmpMD5 != GetMD5HashFromFile(tempPath + filename))
                {
                    md5 = false;

                    File.Delete(tempPath + filename);

                    Console.Error.WriteLine("Warning: md5 hash not ok, file not save to destination dir");
                    Console.Error.WriteLine("");
                }
            }

            if(md5)
            {
                if (File.Exists(destination_dir + "\\" + filename))
                {
                    File.Delete(destination_dir + "\\" + filename);
                }

                File.Move(tempPath + filename, destination_dir + "\\" + filename);
            }
        }
        #endregion

        #region download file
        static void downloadFile(string access_key, string filename, string dir)
        {
            string baseUrl = @"http://data.udger.com/";            

            if (File.Exists(dir + "\\" + filename))
            {
                File.Delete(dir + "\\" + filename);
            }

            Console.Write("Downloading file: {0} ...", filename);
            using (WebClient myWebClient = new WebClient())
            {
                try
                {
                    myWebClient.DownloadFile(new Uri(baseUrl + access_key + "/" + filename), dir + "\\" + filename);
                    myWebClient.Dispose();
                }
                catch (WebException e)
                {
                    Console.Error.WriteLine("\nError: " + e.Message);
                    Environment.Exit(1);
                }
            }
            Console.Write("... Done\n");

        }
        #endregion

        #region set Config file
        static void setFromConfigFile(string[] args)
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.File = args[0];
            config.Save(ConfigurationSaveMode.Modified);
        }
        #endregion

        #region MD5
        static string GetMD5HashFromFile(string ff)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.Open(ff, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
                }
            }
        }
        #endregion

        #region destination directory
        static string GetDestinationDirectory(string key, string global_dir)
        {
            string destination_directory = ConfigurationManager.AppSettings[key + "_destination_directory"];
            if (String.IsNullOrEmpty(destination_directory))
            {
                destination_directory = global_dir;
            }
            if (!Directory.Exists(destination_directory))
            {
                Console.Error.WriteLine("Error: " + key + "_destination_directory not found");
                Console.Error.WriteLine("");
                Environment.Exit(1);
            }
            return destination_directory;
        }
        #endregion
    }
}
