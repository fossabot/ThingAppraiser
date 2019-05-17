﻿using System;
using ThingAppraiser.Data.Configuration;

namespace ThingAppraiser.ProcessingWebService.v1.Domain
{
    public class TargetServiceCreator : ITargetServiceCreator
    {
        public TargetServiceCreator()
        {
        }

        #region ITargetServiceCreator Implementation

        public IServiceRequestProcessor CreateRequestProcessor(ServiceType serviceType)
        {
            switch (serviceType)
            {
                case ServiceType.Sequential:
                    return new ServiceRequestProcessor();

                case ServiceType.TplDataflow:
                    return new ServiceAsyncRequestProcessor();

                case ServiceType.Rx:
                    return new ServiceRxRequestProcessor();

                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceType),
                                                          "Not known service type");
            }
        }

        #endregion
    }
}