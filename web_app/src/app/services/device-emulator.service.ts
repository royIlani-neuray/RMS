/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DeviceEmulatorService {

  constructor(private http:HttpClient) { }

  public setPlaybackSettings(recordingName : string, deviceId: string)
  {
    return this.http.put("/device-emulator/emulator/playback", 
    {
      recording_name : recordingName,
      device_id: deviceId,
      loop_forever: false
    })
  }

}
