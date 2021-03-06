﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bank.Common;
using Bank.Common.Interface;
using Bank.Common.Value;
using BankQueue.Core.Annotations;

namespace BankQueue.Core
{
    public sealed class WorkProcess : IWorkProcess
    {
        private readonly object _syncRoot = new object();
        private readonly Guid _id;
        
        private readonly Timer _timer;
        private readonly IOperationQueue _operationQueue;
        private readonly IStampProvider _stampProvider;

        public WorkProcess(IWorkPlace workplace, IOperationQueue operationQueue, [NotNull] IStampProvider stampProvider)
        {
            if (workplace == null) throw new ArgumentNullException("workplace");
            if (operationQueue == null) throw new ArgumentNullException("operationQueue");
            if (stampProvider == null) throw new ArgumentNullException("stampProvider");

            _id = Guid.NewGuid();
            Workplace = workplace;
            workplace.SetParrentWorkProcess(this);
            _operationQueue = operationQueue;
            _stampProvider = stampProvider;
            State = WorkState.Stoped;
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public event EventHandler<CustomerArgs> ProcessStarted = delegate { };
        public event EventHandler<CustomerArgs> ProcessCompleted = delegate { };
        public event EventHandler<WorkState> StateChanged = delegate {};

        public Guid ProcessId { get { return _id; } }
        public WorkState State { get; private set; }
        public IWorkPlace Workplace { get; private set; }

        public void Start()
        {
            State = WorkState.InWork;
            _timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
            StateChanged(this, State);
        }

        public void Stop()
        {
            State = WorkState.Stoped;
            StateChanged(this, State);
        }

        public void Pause()
        {
            State = WorkState.Paused;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            StateChanged(this, State);
        }

        private void TimerCallback(object state)
        {
            if (!Monitor.TryEnter(_syncRoot)) return;
            try
            {
                if (State != WorkState.InWork) return;
                WorkWithCustomer();
            }
            catch (Exception ex)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                throw new ApplicationException(string.Format("Oops! Critical WorkProcess error! ProcessId: {0}", _id), ex);
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }
        }

        private void WorkWithCustomer()
        {
            var customerArgs = _operationQueue.GetNextCustomer(Workplace.QueueType);
            if (customerArgs == null) return;

            ProcessStarted(this, customerArgs);

            var officer = Workplace.GetNextOfficer();
            Thread.Sleep(3000);

          //  var  stamp = _stampProvider.GetStamp(officer);
           /// Thread.Sleep(1000);
          //  _stampProvider.ReturnStamp(officer);

            ProcessCompleted(this, customerArgs);
            Thread.Sleep(500);
        }

    }
}
