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
  sourceBuffer : SourceBuffer | null = null
  framesList : Uint8Array[] = []
  streamingStarted = false

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
        
        //this.initVideoPlayer()

        // we have the radar info, now subscribe for tracks streaming
        this.frameDataSubscription = this.cameraWebsocket.GetFrameData().subscribe({
          next : (frameData) => 
          {
            
            this.videoRef.nativeElement.src = 'data:video/h264;base64,' + frameData
            /*
            //console.log(`Got camera frame data!!!!!! ${frameData}`)
            if (!this.streamingStarted)
            {
              if (this.sourceBuffer == null)
              {
                console.log("Media source not initialized yet!")
                return
              }

              this.streamingStarted = true;
              //console.log(`Got camera frame data!!!!!! ${frameData}`)
              //console.log(`source buffer = ${this.sourceBuffer}`)

              const typedArray1 = new Uint8Array(frameData);
              this.sourceBuffer!.appendBuffer(typedArray1)
              
              //this.sourceBuffer!.appendBuffer(frameData)
              return;
            }
            else
            {
              //console.log('writing frame to frameList.')
              this.framesList.push(new Uint8Array(frameData))
            }
            */
          }
        })

      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })

  }

  initVideoPlayer()
  {
    console.log("Initializing video player...")
    this.videoRef.nativeElement.loop = false

    this.videoRef.nativeElement.onerror = () => {
      console.log("Media element error");
    }

    this.videoRef.nativeElement.addEventListener('canplay', () => 
    {
      console.log('Video can start, but not sure it will play through.');
      this.videoRef.nativeElement.play();
    });

    /* NOTE: Chrome will not play the video if we define audio here
    * and the stream does not include audio */
    var mimeCodec = 'video/mp4; codecs="avc1.4D0033, mp4a.40.2"';
    //var mimeCodec = 'video/mp4; codecs="avc1.4D0033, mp4a.40.2"';
    //var mimeCodec = 'video/mp4; codecs=avc1.42E01E,mp4a.40.2'
    //var mimeCodec = 'video/mp4; codecs=avc1.4d002a,mp4a.40.2';
    //var mimeCodec = 'video/mp4; codecs="avc1.64001E, mp4a.40.2"'; high

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
        if (!this.sourceBuffer!.updating) 
        {
          if (this.framesList.length>0)
          {
            console.log("pushing next frame...")
            let frameData = this.framesList.shift(); // pop from the begining
            this.sourceBuffer!.appendBuffer(frameData!)
          }
          else
          {
            this.streamingStarted = false
          }

        }
        


      });

      this.sourceBuffer.addEventListener("onerror", () => {
        console.log("Media source error");
      });
    });

  }

}
