using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Nipema.PictureReceiver
{
    public partial class PictureReceiverService : ServiceBase
    {
        private Thread _mainThread = null;
        private readonly PictureReceiverTask _receiver;

        public PictureReceiverService()
        {
            InitializeComponent();
            _receiver = new PictureReceiverTask();
        }

        protected override void OnStart(string[] args)
        {
            _mainThread = new Thread(_receiver.Run);
            _mainThread.Start();
        }

        protected override void OnStop()
        {
            _mainThread = null;
        }
    }
}
