import { AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { FrameData, PointData, TrackData } from 'src/app/entities/frame-data';
import { RadarDevice, RadarDeviceBrief } from 'src/app/entities/radar-device';
import * as THREE from "three";
import { MeshBasicMaterial, MeshStandardMaterial, PerspectiveCamera } from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'
import { DevicesService } from '../../services/devices.service';
import { WebsocketService } from '../../services/websocket.service';

@Component({
  selector: 'app-page-tracks-viewer',
  templateUrl: './tracks-viewer.component.html',
  styleUrls: ['./tracks-viewer.component.css']
})
export class TracksViewerComponent implements OnInit, AfterViewInit {

  showBoundryBoxFC = new FormControl(true)
  showStaticBoundryBoxFC = new FormControl(false)
  showPointCloudFC = new FormControl(false)

  selectedDeviceId : string = ''
  radarDevice : RadarDevice
  deviceList: RadarDeviceBrief[] = [];
  lastframeData: FrameData
  numberOfPoints: Number = 0

  dataSource = new MatTableDataSource<TrackData>()
  displayedColumns: string[] = ['track_id', 'range', 'position_x', 'position_y', 'position_z', 'velocity_x', 'velocity_y', 'velocity_z'];

  @ViewChild('canvas') private canvasRef : ElementRef;
  
  private get canvas() : HTMLCanvasElement {
    return this.canvasRef.nativeElement;
  }

  private renderer!: THREE.WebGLRenderer
  private scene!: THREE.Scene
  private camera!: THREE.PerspectiveCamera;
  private controls!: OrbitControls

  constructor(private devicesService : DevicesService,
              private websocketService : WebsocketService,
              private router : Router) { }

  ngOnInit(): void {
    this.getDeviceList()
  }

  public getDeviceList()
  {
    this.devicesService.getRadarDevices().subscribe({
      next : (response) => this.deviceList = response as RadarDeviceBrief[],
      error : (err) => this.router.navigate(['/no-service'])
    })
  }

  public getDevice()
  {
    // request the radar device info based on the given device id
    this.devicesService.getRadarDevice(this.selectedDeviceId).subscribe({
      next : (response) => {
        this.radarDevice = response as RadarDevice
        
        // we have the radar info, now subscribe for tracks streaming
        this.websocketService.GetFrameData().subscribe({
          next : (result) => {

            let frameData = result as FrameData

            if (frameData.device_id != this.selectedDeviceId)
              return

            this.lastframeData = frameData
            this.dataSource.data = this.lastframeData.tracks
            this.numberOfPoints = this.lastframeData.points.length
            // update the scene with the latest frame data
            this.updateScene()
          }
        })

      },
      error : (err) => err.status == 504 ? this.router.navigate(['/no-service']) : this.router.navigate(['/error-404'])
    })
  }

  public RadarDeviceSelected(deviceId : string)
  {
    this.selectedDeviceId = deviceId
    this.getDevice()
  }

  private initializeScene() 
  {
    this.scene = new THREE.Scene()
    //this.scene.background = new THREE.Color(0x000000)

    let aspectRatio = (this.canvas.clientWidth / this.canvas.clientHeight)

    this.camera = new PerspectiveCamera(
      45,
      aspectRatio,
      0.1,
      1000
    );

    this.camera.position.set(0,2,-17)

    this.renderer = new THREE.WebGLRenderer({ canvas: this.canvas })
    this.renderer.setPixelRatio(devicePixelRatio)
    this.renderer.setSize(this.canvas.clientWidth, this.canvas.clientHeight)

    this.controls = new OrbitControls(this.camera, this.renderer.domElement)
    this.controls.target.set(0,5,10) // change orbit center from [0,0,0] to plane center
    this.controls.listenToKeyEvents(window)
    this.controls.update()
  }

  private startRenderingLoop()
  {
    let component: TracksViewerComponent = this;
    (function render() 
    {
      requestAnimationFrame(render)
      component.controls.update()
      component.renderer.render(component.scene, component.camera)
    }())
  }

  ngAfterViewInit(): void {
    this.initializeScene()
    this.startRenderingLoop()
  }

  // Importent - Three.js rendering coordinates:
  // X: positive => canvas left side. negative => canvas right side
  // Y: positive => canvas top side. negative => canvas bottom
  // Z: positive => far side (canvas depth). negative => close side.
  //
  // For Radar: 
  //  X positive and negative are the opposite of Three.js X, 
  //  Radar Y is Three.js Z
  //  Radar Z is Three.js Y

  private updateScene()
  {
    if (this.radarDevice.radar_settings == null)
      return

    let scene = new THREE.Scene()

    let directionalLight = new THREE.DirectionalLight(0xFFFFFF, 0.8)
    scene.add(directionalLight)

    let ambientLight = new THREE.AmbientLight(0x333333)
    scene.add(ambientLight)

    if (this.radarDevice.radar_settings.boundary_box != null)
    {
        let boundingBoxSizeX = Math.abs(this.radarDevice.radar_settings.boundary_box.x_max - this.radarDevice.radar_settings.boundary_box.x_min)
        let boundingBoxSizeY = Math.abs(this.radarDevice.radar_settings.boundary_box.z_max - this.radarDevice.radar_settings.boundary_box.z_min)
        let boundingBoxSizeZ = Math.abs(this.radarDevice.radar_settings.boundary_box.y_max - this.radarDevice.radar_settings.boundary_box.y_min)
        let boundingBoxZoffset = this.radarDevice.radar_settings.boundary_box.y_min

        // draw the floor (plane grid)
        let planeGridSize = Math.max(this.radarDevice.radar_settings.boundary_box.y_max, boundingBoxSizeX)
        let planeGridDivisions = planeGridSize
        let planeGrid = new THREE.GridHelper(planeGridSize,planeGridDivisions, 0x303030, 0x303030)
        planeGrid.position.z += (planeGridSize / 2)
        scene.add(planeGrid)

        if (this.showBoundryBoxFC.value)
        {
          // draw the bounding box
          let boxGeometry = new THREE.BoxGeometry(boundingBoxSizeX,boundingBoxSizeY,boundingBoxSizeZ)
          let boxEdges = new THREE.EdgesGeometry(boxGeometry)
          let box = new THREE.LineSegments(boxEdges, new THREE.LineBasicMaterial( { color: 0xff00ff } ) )
          box.position.set(0,0,boundingBoxZoffset + (boundingBoxSizeZ/2))
          scene.add(box)   
        }
    }

    if ((this.radarDevice.radar_settings.static_boundary_box != null) && (this.showStaticBoundryBoxFC.value))
    {
      let staticBoundingBoxSizeX = Math.abs(this.radarDevice.radar_settings.static_boundary_box.x_max - this.radarDevice.radar_settings.static_boundary_box.x_min)
      let staticBoundingBoxSizeY = Math.abs(this.radarDevice.radar_settings.static_boundary_box.z_max - this.radarDevice.radar_settings.static_boundary_box.z_min)
      let staticBoundingBoxSizeZ = Math.abs(this.radarDevice.radar_settings.static_boundary_box.y_max - this.radarDevice.radar_settings.static_boundary_box.y_min)
      let staticBoundingBoxZoffset = this.radarDevice.radar_settings.static_boundary_box.y_min

      // draw the static bounding box
      let staticBoxGeometry = new THREE.BoxGeometry(staticBoundingBoxSizeX,staticBoundingBoxSizeY,staticBoundingBoxSizeZ)
      let staticBoxEdges = new THREE.EdgesGeometry(staticBoxGeometry)
      let staticBox = new THREE.LineSegments(staticBoxEdges, new THREE.LineBasicMaterial( { color: 0xffffff } ) )
      staticBox.position.set(0,0,staticBoundingBoxZoffset + (staticBoundingBoxSizeZ/2))
      scene.add(staticBox)         
    }

    if (this.radarDevice.radar_settings.sensor_position != null)
    {
      let radarHeight = this.radarDevice.radar_settings.sensor_position.height

      // draw the radar
      let radarGeometery = new THREE.BoxGeometry(0.4,0.4,0.05)
      let radar = new THREE.Mesh(radarGeometery, new THREE.MeshStandardMaterial({ color: 0xff00ff, metalness:0.5, roughness: 0 }))
      radar.position.set(0,radarHeight,0)
      scene.add(radar)  

      // draw tracks
      this.lastframeData.tracks.forEach(function (track) 
      {
        /*
        let boxGeometry = new THREE.BoxGeometry(1,2,1)
        let boxEdges = new THREE.EdgesGeometry(boxGeometry)
        let box = new THREE.LineSegments(boxEdges, new THREE.LineBasicMaterial( { color: 0xffffff } ) )
        box.position.set(-track.position_x, track.position_z, track.position_y)
        scene.add(box)
        */
        let trackGeometry = new THREE.SphereGeometry(0.25)
        let trackMesh = new THREE.Mesh(trackGeometry, new MeshStandardMaterial({color: 0xffea00, metalness:0.5, roughness: 0}))
        trackMesh.position.set(-track.position_x, track.position_z, track.position_y)
        scene.add(trackMesh)

      });

      // draw points 
      if (this.showPointCloudFC.value)
      {
        this.lastframeData.points.forEach((point) =>
        {
          //let azimuthDeg = point.azimuth * (180 / Math.PI)
          //let elevationDeg = point.elevation * (180 / Math.PI)
          //console.log(" Az:" + azimuthDeg + " El:" + elevationDeg)
          
          let pointGeometry = new THREE.SphereGeometry(0.02)
          let pointMesh = new THREE.Mesh(pointGeometry, new MeshBasicMaterial({color: 0xffffff}))
          pointMesh.position.set(-point.position_x, point.position_z, point.position_y)
          scene.add(pointMesh)
        })
      }
    }

    this.scene = scene
  }


  public getTrackRange(track : TrackData)
  {
    return Math.sqrt(Math.pow(track.position_x,2) + Math.pow(track.position_y,2) + Math.pow(track.position_z,2))
  }
}
