/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Injectable } from '@angular/core';
import { Observable, Subscriber } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {

  private socket : WebSocket  

  constructor() 
  { 
    this.socket = new WebSocket("ws://" + window.location.host + "/websocket");
    this.socket.onopen = function (event) {
      console.log('Websockets connection state: [Open]')
    }

    this.socket.onerror = function (event) {
      console.log('Websockets connection state: [Error]')
    }

    this.socket.onclose = function (event) {
      console.log('Websockets connection state: [Closed]')
    };

  }

  public GetFrameData() 
  {
    return new Observable((subscriber) => 
    {
      this.socket.onmessage = function (event) {
        //console.log('Websockets Message -' + event.data)
        let frameData = JSON.parse(event.data)
        subscriber.next(frameData);
      }
    });
  }
}
