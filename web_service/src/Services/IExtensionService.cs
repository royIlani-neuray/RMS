/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using WebService.Entites;

namespace WebService.Services;

public interface IExtensionService 
{
    public string ServiceId { get; }

    public List<DeviceEntity.DeviceTypes> SupportedDeviceTypes { get; }

    public ExtensionServiceSettings? Settings {get; set; }
    public IServiceContext CreateServiceContext(DeviceEntity device, Dictionary<string,string> serviceOptions);

    public void DisposeServiceContext(IServiceContext serviceContext);

    public void RunService(object dataObject, IServiceContext serviceContext);
    
}