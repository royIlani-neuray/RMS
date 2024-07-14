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

export interface RecordingInfo {
  name: string
  created_at: string
  upload_s3: boolean
  is_uploading: boolean
  last_uploaded: string
  entries: RecordingEntry[]
}

export interface RecordingEntry {
  device_name: string
  device_id: string
  device_type: string
  created_at: string
  entry_size_bytes: number
}

@Injectable({
  providedIn: 'root'
})
export class RecordingsService {

  constructor(private http:HttpClient) { }

  public getRadarRecording(recordingName : string)
  {
    return this.http.get<RecordingInfo>("/api/recordings/" + recordingName)
  }

  public getRadarRecordings()
  {
    return this.http.get<RecordingInfo[]>("/api/recordings")
  }

  public deleteRecording(recordingName : string)
  {
    return this.http.delete("/api/recordings/" + recordingName)
  }

  public uploadRecording(recordingFile : any)
  {
    return this.http.post("/api/recordings", recordingFile)
  }

  public uploadRecordingToCloud(recordingName : string)
  {
    return this.http.post("/api/recordings/" + recordingName + "/upload-cloud", {})
  }

  public renameRecording(recordingName : string, newRecordingName : string)
  {
    return this.http.post("/api/recordings/" + recordingName + "/rename", 
    {
      new_name : newRecordingName
    })
  }

  public startRecording(recordingName : string, radars : string[], cameras : string[], uploadS3 : boolean)
  {
    return this.http.post("/api/recordings/start-recording", 
    {
      recording_name : recordingName,
      radars : radars,
      cameras : cameras,
      upload_s3 : uploadS3
    })
  }

  public stopRecording(radars : string[], cameras : string[])
  {
    return this.http.post("/api/recordings/stop-recording", 
    {
      radars : radars,
      cameras : cameras
    })
  }

}
