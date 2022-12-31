/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { Observable, Subject, Subscriber } from 'rxjs';
import { FrameData } from '../entities/frame-data';

@Injectable()
export class DeviceWebsocketService {

  private socket : WebSocket  
  private frameDataSubject: Subject<FrameData>;
  
  constructor() 
  { 
    this.frameDataSubject = new Subject<FrameData>();
  }

  public Connect(deviceId : string)
  {
    console.log(`connecting to device: ${deviceId}`)
    this.socket = new WebSocket("ws://" + window.location.host + "/websocket/ws/devices/" + deviceId);
    
    this.socket.onopen = function (event) {
      console.log('Websockets connection state: [Open]')
    }

    this.socket.onerror = function (event) {
      console.log('Websockets connection state: [Error]')
    }

    this.socket.onclose = function (event) {
      console.log('Websockets connection state: [Closed]')
    };

    var frameDataSubject = this.frameDataSubject
    this.socket.onmessage = function (event) {
      //console.log('Websockets Message -' + event.data)
      let message = JSON.parse(event.data)
      
      if (message['type'] == 'FRAME_DATA')
      {
        frameDataSubject.next(message['data'])
      }
    }
  }

  public Disconnect()
  { 
    // Close the connection, if open.
    if (this.socket.readyState === WebSocket.OPEN) 
    {
      this.socket.close();
    }
  }

  public GetFrameData() 
  {
    return this.frameDataSubject;
  }
}
