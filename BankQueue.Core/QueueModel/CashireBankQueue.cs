﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bank.Common;

namespace BankQueue.Core.QueueModel
{
    public sealed class CashireBankQueue : BankQueueCommon
    {
        public override QueueType Type { get {return QueueType.Cashire;} }
    }
}
