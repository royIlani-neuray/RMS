/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface CameraFrameData {
  segment_type: string
  segment_data: string
}

@Injectable()
export class CameraWebsocketService {

  private connected : boolean
  private socket : WebSocket  
  private frameDataSubject: Subject<CameraFrameData>;
  
  constructor() 
  { 
    this.connected = false
  }

  public Connect(cameraId : string)
  {
    if (this.connected)
    {
      this.Disconnect()
    }
    
    console.log(`connecting to camera websocket: ${cameraId}`)
    this.socket = new WebSocket("ws://" + window.location.host + "/websocket/ws/cameras/" + cameraId);
    
    this.socket.onopen = function (event) {
      console.log('Camera Websocket connection state: [Open]')
    }

    this.socket.onerror = function (event) {
      console.log('Camera Websocket connection state: [Error]')
    }

    this.socket.onclose = function (event) {
      console.log('Camera Websocket connection state: [Closed]')
    };

    this.frameDataSubject = new Subject<CameraFrameData>();
    
    this.socket.onmessage = (event) => {
      //console.log('Websockets Message -' + event.data)
      let message = JSON.parse(event.data)
      
      if (message['type'] == 'FRAME_DATA')
      {
        this.frameDataSubject.next(message['data'])
      }
    }

    this.connected = true
  }

  public Disconnect()
  { 
    if (!this.connected)
      return

    // Close the connection, if open.
    if (this.socket.readyState === WebSocket.OPEN) 
    {
      this.socket.close();
    }

    this.connected = false
  }

  public GetFrameData() 
  {
    return this.frameDataSubject;
  }
  
}
