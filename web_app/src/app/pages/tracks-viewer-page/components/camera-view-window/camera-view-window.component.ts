import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Camera } from 'src/app/entities/camera';
import { CameraWebsocketService } from 'src/app/services/camera-websocket.service';
import { CamerasService } from 'src/app/services/cameras.service';

@Component({
  selector: 'app-camera-view-window',
  templateUrl: './camera-view-window.component.html',
  styleUrls: ['./camera-view-window.component.css'],
  providers: [CameraWebsocketService]
})
export class CameraViewWindowComponent implements OnInit, AfterViewInit {

  constructor(private camerasService : CamerasService, 
              private cameraWebsocket : CameraWebsocketService,
              private router : Router) { }

  @ViewChild('video') private videoRef : ElementRef;

  camera : Camera | null
  frameDataSubscription! : any
  mediaSource : MediaSource = new MediaSource()
  sourceBuffer : any | null
  framesList = []

  ngOnInit(): void {
  }

  ngAfterViewInit(): void {
  }

  setCamera(cameraId : string)
  {
    if (this.frameDataSubscription != null)
    {
      this.frameDataSubscription.unsubscribe()
      this.frameDataSubscription = null
    }

    // request the camera device info based on the given camera id
    this.camerasService.getCamera(cameraId).subscribe({
      next : (camera) => {
        this.camera = camera
        
        this.cameraWebsocket.Connect(cameraId)
        
        this.initVideoPlayer()

        // we have the radar info, now subscribe for tracks streaming
        this.frameDataSubscription = this.cameraWebsocket.GetFrameData().subscribe({
          next : (frameData) => 
          {
            // TODO.....
          }
        })

      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })

  }

  initVideoPlayer()
  {
    console.log("Initializing video player...")
    var mimeCodec = 'video/mp4; codecs="avc1.4D0033, mp4a.40.2"';

    if (!MediaSource)
    {
      console.error("No Media Source API available");
      return;
    }

    if (!MediaSource.isTypeSupported(mimeCodec)) {
      console.error("Unsupported MIME type or codec: " + mimeCodec);
      return;
    }

    this.videoRef.nativeElement.src = window.URL.createObjectURL(this.mediaSource)
    
    this.mediaSource.addEventListener('sourceopen', () =>
    {
      console.log("Media source is open!");
      this.sourceBuffer = this.mediaSource.addSourceBuffer(mimeCodec);

      this.sourceBuffer.addEventListener("updateend",() => {
        console.log("on updateend...")
        if ((!this.sourceBuffer.updating) && (this.framesList.length >0))
        {
          let frameData = this.framesList.shift(); // pop from the begining
          this.sourceBuffer.appendBuffer(frameData)
        }

      });

      this.sourceBuffer.addEventListener("onerror", () => {
        console.log("Media source error");
      });
    });
    
    
  }

}
