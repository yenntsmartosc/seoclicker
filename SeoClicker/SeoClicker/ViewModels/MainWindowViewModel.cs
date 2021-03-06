﻿using System.Windows;
using SeoClicker.Utils;
using SeoClicker.Models;
using SeoClicker.Helpers;
using System.Linq;
using System.Net;
using System.Threading;
using System;

namespace SeoClicker.ViewModels
{
    public class MainWindowViewModel
    {
        public RequestTaskRunner RequestWorker { get; set; }

        public MainWindowViewModel()
        {
            setupData();
            setupCommands();
            manageAppExit();
            initTaskScheduler();
        }
        public string SpinnerImagePath { get; set; }
        public DelegateCommand<string> DoStart { set; get; }
        public DelegateCommand<string> DoStop { set; get; }
        public DelegateCommand<string> DoClearLogs { get; set; }
        public DelegateCommand<string> DoSaveSettings { get; set; }
        public ProxySettings ProxySettings { get; set; }
        public DataServerSettings DataServerSettings { get; set; }
        public TaskSettings TaskSettings { get; set; }
        bool canDoStart(string data)
        {
            return false;
        }
        void currentExit(object sender, ExitEventArgs e)
        {
            exit();
        }

        void currentSessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            exit();
        }



        void doStart(string data)
        {
            if (Designer.IsInDesignModeStatic)
                return;
            var proxyRoute = ProxySettings.Route.FirstOrDefault(x => x.IsSelected)?.Value ?? "pass_dyn";
            var superProxy = ProxySettings.SuperProxy.FirstOrDefault(x => x.IsSelected)?.Value ?? "zproxy.lum-superproxy.io";
            var dnsResolution = ProxySettings.DNSResolution.FirstOrDefault(x => x.IsSelected)?.Value ?? "-session-";
            var dataItem = DataHelper.LoadData();

            RequestWorker.SpinnerVisibility = "Visible";
            RequestWorker.ResultMessage = "";
            RequestWorker.ThreadInfos.Clear();
            RequestWorker.Logs = "";
            RequestWorker.IsEnabled = false;

            var targetUri = dataItem.url;
            var geo = !string.IsNullOrWhiteSpace(dataItem.geo) ? dataItem.geo : "us";
            var clientSettings = new ClientSettings
            {
                Country = geo,
                DNSResolution = dnsResolution,
                Password = ProxySettings.Password,
                Port = ProxySettings.Port,
                Route = proxyRoute,
                SuperProxy = superProxy,
                TargetUrl = targetUri,
                Device = dataItem.device,
                UserName = ProxySettings.UserName,
                Zone = ProxySettings.ProxyZone,
                Timeout = Timeout.Infinite,
                NumberOfThread = TaskSettings.NumberOfThreads,
                RequestNumber = TaskSettings.TotalRequest,
                IpChangeRequestNumber = 1,
                ApiDataUri = DataServerSettings.GetDataApiLink,
                Take = DataServerSettings.UrlCount,
                LoadCount = TaskSettings.LoadCount != 0 ? TaskSettings.LoadCount : 10,
                LoadTime = TaskSettings.LoadTime != 0 ? TaskSettings.LoadTime : 5,
                ClearResult = TaskSettings.ClearResultFiles
            };
            try
            {
                RequestWorker.ClientSettings = clientSettings;
                RequestWorker.ConfigureTask();
                RequestWorker.Start();
            }
           catch(Exception ex)
            {
                
            }

        }
        void doStop(string data)
        {
            RequestWorker.Stop();

        }

        void doSaveSettings(string data)
        {
            var settings = new Settings
            {
                DataServerSettings = DataServerSettings,
                ProxySettings = ProxySettings,
                TaskSettings = TaskSettings
            };
            SettingsHelper.SaveSettings(settings);
        }

        void doClearLogs(string data)
        {
            RequestWorker.Logs = "";
        }

        private void initTaskScheduler()
        {

        }

        private void exit()
        {
            RequestWorker.Stop();

        }


        private void manageAppExit()
        {
            Application.Current.Exit += currentExit;
            Application.Current.SessionEnding += currentSessionEnding;
        }


        private void setupCommands()
        {
            DoStart = new DelegateCommand<string>(doStart, data => true);
            DoStop = new DelegateCommand<string>(doStop, data => true);
            DoSaveSettings = new DelegateCommand<string>(doSaveSettings, data => true);
            DoClearLogs = new DelegateCommand<string>(doClearLogs, data => true);
        }

        private void setupData()
        {
            var settings = SettingsHelper.LoadSettings();
            ProxySettings = settings.ProxySettings;
            DataServerSettings = settings.DataServerSettings;
            TaskSettings = settings.TaskSettings;

            RequestWorker = new RequestTaskRunner();
            SpinnerImagePath = DataHelper.GetSpinnerImagePath();

        }
    }
}