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

@Injectable({
  providedIn: 'root'
})
export class RmsEventsService 
{
  private socket : WebSocket

  public deviceUpdatedEvent: Subject<string> = new Subject<string>()
  public deviceAddedEvent: Subject<string> = new Subject<string>()
  public deviceDeletedEvent: Subject<string> = new Subject<string>()
  public DeviceMappingUpdatedEvent: Subject<null> = new Subject()

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
        this.deviceUpdatedEvent.next(message['data'])
      }
      else if (message['type'] == 'RADAR_DEVICE_ADDED')
      {
        this.deviceAddedEvent.next(message['data'])
      }
      else if (message['type'] == 'RADAR_DEVICE_DELETED')
      {
        this.deviceDeletedEvent.next(message['data'])
      }
      else if (message['type'] == 'DEVICE_MAPPING_UPDATED')
      {
        this.DeviceMappingUpdatedEvent.next(null)
      }

    }
  }
}
