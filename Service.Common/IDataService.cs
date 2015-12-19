﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service.Common
{
    [ServiceContract]
    public interface IDataService
    {
        [OperationContract]
        void SaveCustomers(IEnumerable<CustomerDto> customers);
    }
}
