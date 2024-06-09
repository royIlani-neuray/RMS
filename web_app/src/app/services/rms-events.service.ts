/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RmsEventsService 
{
  private socket : WebSocket

  public radarAddedEvent: Subject<string> = new Subject<string>()
  public radarUpdatedEvent: Subject<string> = new Subject<string>()
  public radarDeletedEvent: Subject<string> = new Subject<string>()

  public deviceMappingUpdatedEvent: Subject<null> = new Subject()

  public templateAddedEvent: Subject<string> = new Subject<string>()
  public templateDeletedEvent: Subject<string> = new Subject<string>()

  public cameraAddedEvent: Subject<string> = new Subject<string>()
  public cameraUpdatedEvent: Subject<string> = new Subject<string>()
  public cameraDeletedEvent: Subject<string> = new Subject<string>()

  public recordingStartedEvent: Subject<string> = new Subject<string>()
  public recordingStoppedEvent: Subject<string> = new Subject<string>()

  constructor() 
  { 
    this.socket = new WebSocket("ws://" + window.location.host + "/websocket/ws/events");
    
    this.socket.onopen = function (event) {
      console.log('RMS Events Websockets connection state: [Open]')
    }

    this.socket.onerror = function (event) {
      console.log('RMS Events Websockets connection state: [Error]')
    }

    this.socket.onclose = function (event) {
      console.log('RMS Events Websockets connection state: [Closed]')
    };
    
    this.socket.onmessage = (event) => 
    {
      let message = JSON.parse(event.data)
      
      if (message['type'] == 'RADAR_DEVICE_UPDATED')
      {
        this.radarUpdatedEvent.next(message['data'])
      }
      else if (message['type'] == 'RADAR_DEVICE_ADDED')
      {
        this.radarAddedEvent.next(message['data'])
      }
      else if (message['type'] == 'RADAR_DEVICE_DELETED')
      {
        this.radarDeletedEvent.next(message['data'])
      }
      else if (message['type'] == 'DEVICE_MAPPING_UPDATED')
      {
        this.deviceMappingUpdatedEvent.next(null)
      }
      else if (message['type'] == 'TEMPLATE_ADDED')
      {
        this.templateAddedEvent.next(message['data'])
      }
      else if (message['type'] == 'TEMPLATE_DELETED')
      {
        this.templateDeletedEvent.next(message['data'])
      }
      else if (message['type'] == 'CAMERA_DEVICE_UPDATED')
      {
        this.cameraUpdatedEvent.next(message['data'])
      }
      else if (message['type'] == 'CAMERA_DEVICE_ADDED')
      {
        this.cameraAddedEvent.next(message['data'])
      }
      else if (message['type'] == 'CAMERA_DEVICE_DELETED')
      {
        this.cameraDeletedEvent.next(message['data'])
      }
      else if (message['type'] == 'RECORDING_STARTED')
      {
        this.recordingStartedEvent.next(message['data'])
      }
      else if (message['type'] == 'RECORDING_STOPPED')
      {
        this.recordingStoppedEvent.next(message['data'])
      }

    }
  }
}
