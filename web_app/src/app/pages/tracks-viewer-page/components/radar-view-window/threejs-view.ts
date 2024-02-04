/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import * as THREE from "three";
import { FontLoader, Font } from 'three/examples/jsm/loaders/FontLoader.js';
import { TextGeometry } from 'three/examples/jsm/geometries/TextGeometry'
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls'
import { Material, MeshBasicMaterial, MeshStandardMaterial, PerspectiveCamera } from 'three';
import { ElementRef } from "@angular/core";
import { Radar } from "src/app/entities/radar";
import { PointData, TrackData } from "src/app/entities/frame-data";

export class ThreeJsView {

    private renderer!: THREE.WebGLRenderer
    private scene!: THREE.Scene
    private camera!: THREE.PerspectiveCamera;
    private controls!: OrbitControls
    private threeJsFont: Font

    private radar! : THREE.Mesh
    private floor! : THREE.GridHelper
    private boundingBox! : THREE.LineSegments
    private staticBoundingBox! : THREE.LineSegments
    private directionalLight : THREE.DirectionalLight
    private ambientLight : THREE.AmbientLight

    private tracksGroup : THREE.Group
    private pointsCloudGroup : THREE.Group

    public showBoundingBox : boolean = true;
    public showStaticBoundingBox : boolean = false;
    public showTracks : boolean = true;
    public showPointsCloud : boolean = false;

    constructor() {
    }

    public loadThreeJsFonts()
    {
      const loader = new FontLoader()
  
      loader.load('assets/threejs-fonts/roboto/normal-400.json', (font) => 
      {
        this.threeJsFont = font
      });
    }

    public dispose()
    {
      if (this.renderer != null)
      {
        this.disposeScene()
        this.renderer.dispose()
        this.renderer.forceContextLoss()
      }
    }

    public resizeView(height : number, width : number)
    {
      let aspectRatio = (width / height)
      this.camera.aspect = aspectRatio
      this.camera.updateProjectionMatrix();

      this.renderer.setSize(width, height)
    }

    public startRenderingLoop()
    {
      let component : ThreeJsView = this;
      (function render() 
      {
        requestAnimationFrame(render)
        component.controls.update()
        component.renderer.render(component.scene, component.camera)
      }())
    }

    public initializeThreeJs(canvasRef : ElementRef) 
    {
      this.scene = new THREE.Scene()
      //this.scene.background = new THREE.Color(0xFFFFFFFF)
  
      this.renderer = new THREE.WebGLRenderer()
      canvasRef.nativeElement.append(this.renderer.domElement);
      this.renderer.setPixelRatio(devicePixelRatio)
      this.renderer.setSize(canvasRef.nativeElement.offsetWidth, canvasRef.nativeElement.offsetHeight)
      
      let aspectRatio = (canvasRef.nativeElement.offsetWidth / canvasRef.nativeElement.offsetHeight)
  
      this.camera = new PerspectiveCamera(
        45,
        aspectRatio,
        0.1,
        350
      );
  
      this.camera.position.set(0,2,-17)
  
  
      this.controls = new OrbitControls(this.camera, this.renderer.domElement)
      this.controls.target.set(0,5,10) // change orbit center from [0,0,0] to plane center
      this.controls.listenToKeyEvents(window)
      this.controls.update()

      this.directionalLight = new THREE.DirectionalLight(0xFFFFFF, 0.8)
      this.ambientLight = new THREE.AmbientLight(0x333333)

      this.tracksGroup = new THREE.Group()
      this.pointsCloudGroup = new THREE.Group()
    }



    public initScene(radar : Radar)
    {
      this.disposeScene()
      this.scene.add(this.directionalLight)
      this.scene.add(this.ambientLight)
      this.scene.add(this.tracksGroup)
      this.scene.add(this.pointsCloudGroup)
      
      if (radar.radar_settings.boundary_box != null)
      {
        let boundingBoxSizeX = Math.abs(radar.radar_settings.boundary_box.x_max - radar.radar_settings.boundary_box.x_min)
        let boundingBoxSizeY = Math.abs(radar.radar_settings.boundary_box.z_max - radar.radar_settings.boundary_box.z_min)
        let boundingBoxSizeZ = Math.abs(radar.radar_settings.boundary_box.y_max - radar.radar_settings.boundary_box.y_min)
        let boundingBoxXoffset = -radar.radar_settings.boundary_box.x_min
        let boundingBoxYoffset = radar.radar_settings.boundary_box.z_min
        let boundingBoxZoffset = radar.radar_settings.boundary_box.y_min

        // draw the floor (plane grid)
        let planeGridSize = Math.max(boundingBoxSizeZ, boundingBoxSizeX)
        let planeGridDivisions = planeGridSize
        this.floor = new THREE.GridHelper(planeGridSize,planeGridDivisions, 0x303030, 0x303030)
        this.floor.position.z += (planeGridSize / 2) + radar.radar_settings.boundary_box.y_min
          
        this.scene.add(this.floor)

        // this should be set only once 
        this.controls.target.set(0,5, this.floor.position.z) // change orbit center from [0,0,0] to plane center


        // draw the bounding box
        let boxGeometry = new THREE.BoxGeometry(boundingBoxSizeX,boundingBoxSizeY,boundingBoxSizeZ)
        let boxEdges = new THREE.EdgesGeometry(boxGeometry)
        this.boundingBox = new THREE.LineSegments(boxEdges, new THREE.LineBasicMaterial( { color: 0xff00ff } ) )
        this.boundingBox.position.set(boundingBoxXoffset - (boundingBoxSizeX/2), boundingBoxYoffset + (boundingBoxSizeY/2), boundingBoxZoffset + (boundingBoxSizeZ/2))
        
        if (!this.showBoundingBox)
        {
          this.boundingBox.visible = false
        }
        
        this.scene.add(this.boundingBox)   
      }

      if (radar.radar_settings.static_boundary_box != null)
      {
        let staticBoundingBoxSizeX = Math.abs(radar.radar_settings.static_boundary_box.x_max - radar.radar_settings.static_boundary_box.x_min)
        let staticBoundingBoxSizeY = Math.abs(radar.radar_settings.static_boundary_box.z_max - radar.radar_settings.static_boundary_box.z_min)
        let staticBoundingBoxSizeZ = Math.abs(radar.radar_settings.static_boundary_box.y_max - radar.radar_settings.static_boundary_box.y_min)
        let staticBoundingBoxXoffset = -radar.radar_settings.static_boundary_box.x_min
        let staticBoundingBoxYoffset = radar.radar_settings.static_boundary_box.z_min
        let staticBoundingBoxZoffset = radar.radar_settings.static_boundary_box.y_min

        // draw the static bounding box
        let staticBoxGeometry = new THREE.BoxGeometry(staticBoundingBoxSizeX,staticBoundingBoxSizeY,staticBoundingBoxSizeZ)
        let staticBoxEdges = new THREE.EdgesGeometry(staticBoxGeometry)
        this.staticBoundingBox = new THREE.LineSegments(staticBoxEdges, new THREE.LineBasicMaterial( { color: 0xffffff } ) )
        this.staticBoundingBox.position.set(staticBoundingBoxXoffset - (staticBoundingBoxSizeX/2), staticBoundingBoxYoffset + (staticBoundingBoxSizeY/2), staticBoundingBoxZoffset + (staticBoundingBoxSizeZ/2))

        if (!this.showStaticBoundingBox)
        {
          this.staticBoundingBox.visible = false
        }

        this.scene.add(this.staticBoundingBox)         
      }

      if (radar.radar_settings.sensor_position != null)
      {
        let radarHeight = radar.radar_settings.sensor_position.height
  
        // draw the radar
        let radarGeometery = new THREE.BoxGeometry(0.4,0.4,0.05)
        this.radar = new THREE.Mesh(radarGeometery, new THREE.MeshStandardMaterial({ color: 0xffffff, metalness:0.5, roughness: 0 }))
        this.radar.position.set(0,radarHeight,0)
        this.radar.rotateX(radar.radar_settings.sensor_position.elevation_tilt * (Math.PI / 180))
        this.radar.rotateY((-radar.radar_settings.sensor_position.azimuth_tilt) * (Math.PI / 180))
        this.scene.add(this.radar)  
      }

    }

    private clearGroup(group : THREE.Group)
    {
      group.traverse((child) => {
        if (child instanceof THREE.Mesh)
        {
          let mesh : THREE.Mesh = (child as THREE.Mesh);
          (mesh.material as THREE.Material).dispose()
          mesh.geometry.dispose()
        }
      });
      
      group.clear()
    }

    private disposeScene()
    {
      this.scene.clear() 

      this.clearGroup(this.tracksGroup)
      this.clearGroup(this.pointsCloudGroup)

      if (this.radar != null)
      {
        this.radar.geometry.dispose();
        (this.radar.material as Material).dispose();
      }

      if (this.boundingBox != null)
      {
        this.boundingBox.geometry.dispose();
        (this.boundingBox.material as Material).dispose();
      }

      if (this.staticBoundingBox != null)
      {
        this.staticBoundingBox.geometry.dispose();
        (this.staticBoundingBox.material as Material).dispose();
      }
    }

    public updateTracks(tracks : TrackData[])
    {
      this.clearGroup(this.tracksGroup)

      if (!this.showTracks)
        return

      tracks.forEach((track) => 
      {
        /*
        let boxGeometry = new THREE.BoxGeometry(1,2,1)
        let boxEdges = new THREE.EdgesGeometry(boxGeometry)
        let box = new THREE.LineSegments(boxEdges, new THREE.LineBasicMaterial( { color: 0xffffff } ) )
        box.position.set(-track.position_x, track.position_z, track.position_y)
        this.tracksGroup.add(box)
        */
        let trackGeometry = new THREE.SphereGeometry(0.25)
        let trackMesh = new THREE.Mesh(trackGeometry, new MeshStandardMaterial({color: 0xffea00, metalness:0.5, roughness: 0}))
        trackMesh.position.set(-track.position_x, track.position_z, track.position_y)
        this.tracksGroup.add(trackMesh)


        // Draw Track number text

        // Create a text geometry with the desired text and font
        const textGeometry = new TextGeometry(`Track-${track.track_id}`, {
          font: this.threeJsFont,
          size: 0.2,
          height: 0.02,
          curveSegments: 12
        });

        // Center the text geometry
        textGeometry.center();

        // Create a material for the text
        const textMaterial = new THREE.MeshPhongMaterial( { color: 0xffea00 } );

        // Create a mesh for the text using the geometry and material
        const textMesh = new THREE.Mesh(textGeometry, textMaterial);
        textMesh.rotateY(Math.PI);
        textMesh.position.set(-track.position_x, track.position_z + 0.5, track.position_y)
        this.tracksGroup.add(textMesh)

      });

    }

    public updatePointsCloud(points : PointData[])
    {
      this.clearGroup(this.pointsCloudGroup)

      if (!this.showPointsCloud)
        return

      points.forEach((point) =>
        {
          //let azimuthDeg = point.azimuth * (180 / Math.PI)
          //let elevationDeg = point.elevation * (180 / Math.PI)
          //console.log(" Az:" + azimuthDeg + " El:" + elevationDeg)
          
          let pointGeometry = new THREE.SphereGeometry(0.02)
          let pointMesh = new THREE.Mesh(pointGeometry, new MeshBasicMaterial({color: 0xffffff}))
          pointMesh.position.set(-point.position_x, point.position_z, point.position_y)
          this.pointsCloudGroup.add(pointMesh)
        })
    }

    public setShowBoundingBox(isVisible : boolean)
    {
      if (this.boundingBox != null)
      {
        this.boundingBox.visible = isVisible
      }
    }

    public setShowStaticBoundingBox(isVisible : boolean)
    {
      if (this.staticBoundingBox != null)
      {
        this.staticBoundingBox.visible = isVisible
      }
    }

}