/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface RecordingInfo {
  device_name: string
  device_id: string
  file_name: string
  file_size_bytes: number
  timestamp: string
}

@Injectable({
  providedIn: 'root'
})
export class DeviceEmulatorService {

  constructor(private http:HttpClient) { }

  public getRadarRecordings()
  {
    return this.http.get<RecordingInfo[]>("/device-emulator/emulator/recordings")
  }

  public setPlaybackSettings(recording : RecordingInfo)
  {
    return this.http.put("/device-emulator/emulator/playback", 
    {
      playback_file : recording.file_name,
      loop_forever: false
    })
  }

  public deleteRecording(recording : RecordingInfo)
  {
    return this.http.delete("/device-emulator/emulator/recordings/" + recording.file_name)
  }

  public uploadRecording(recordingFile : any)
  {
    return this.http.post("/device-emulator/emulator/recordings", recordingFile)
  }
}
