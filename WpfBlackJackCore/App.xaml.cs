﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfBlackJackCore
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public int numPlayers = 8;
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0 && int.TryParse(e.Args[0], out int players))
            {
                numPlayers = players;
            }
            base.OnStartup(e);
        }
    }
}
