/***
** Copyright (C) 2020-2023 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, Inject, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import { MatInput } from '@angular/material/input';
import { MatRadioButton } from '@angular/material/radio';
import { DeviceMapping } from 'src/app/entities/radar-device';

export interface DialogData {
  targetDevice: DeviceMapping
}

@Component({
  selector: 'app-set-network-dialog',
  templateUrl: './set-network-dialog.component.html',
  styleUrls: ['./set-network-dialog.component.css']
})
export class SetNetworkDialogComponent implements OnInit, AfterViewInit {

  deviceMapping : DeviceMapping

  @ViewChildren(MatInput) inputComponents: QueryList<MatInput>;
  @ViewChildren(MatRadioButton) radioButtonComponents: QueryList<MatRadioButton>;

  ipValidationPattern = '(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)'

  ipAddressValidation = new FormControl('', [Validators.required, Validators.pattern(this.ipValidationPattern)])
  subnetMaskValidation = new FormControl('', [Validators.required, Validators.pattern(this.ipValidationPattern)])
  gatewayAddressValidation = new FormControl('', [Validators.required, Validators.pattern(this.ipValidationPattern)])

  constructor(public dialogRef: MatDialogRef<SetNetworkDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogData, private cd : ChangeDetectorRef) { }

  ngOnInit(): void 
  {
    this.deviceMapping = this.data.targetDevice    
  }

  private getDynamicRadioButton() {
    return this.radioButtonComponents.get(0)!
  }

  private getStaticRadioButton() {
    return this.radioButtonComponents.get(1)!
  }

  private getIpAddress() {
    return this.inputComponents.get(0)!
  }

  private getSubnetMask() {
    return this.inputComponents.get(1)!
  }

  private getGatewayAddress() {
    return this.inputComponents.get(2)!
  }

  ngAfterViewInit(): void {
    if (this.data.targetDevice.static_ip)
    {
      this.getDynamicRadioButton().checked = false
      this.getStaticRadioButton().checked = true

      this.getIpAddress().value=this.data.targetDevice.ip
      this.getSubnetMask().value=this.data.targetDevice.subnet
      this.getGatewayAddress().value=this.data.targetDevice.gateway

      this.ipAddressValidation.enable()
      this.subnetMaskValidation.enable()
      this.gatewayAddressValidation.enable()
    }
    else
    {
      this.getDynamicRadioButton().checked = true
      this.getStaticRadioButton().checked = false

      this.ipAddressValidation.disable()
      this.subnetMaskValidation.disable()
      this.gatewayAddressValidation.disable()
    }

    this.cd.detectChanges()
  }

  radioSelectionChanged(): void {
    if (this.getStaticRadioButton().checked)
    {
      this.ipAddressValidation.enable()
      this.subnetMaskValidation.enable()
      this.gatewayAddressValidation.enable()
    }
    else
    {
      this.ipAddressValidation.disable()
      this.subnetMaskValidation.disable()
      this.gatewayAddressValidation.disable()
    }
  }

  onCancelClicked(): void {
    this.dialogRef.close();
  }

  onSaveClicked(): void {

    if (this.getStaticRadioButton().checked)
    {
      if (!this.ipAddressValidation.valid || !this.subnetMaskValidation.valid || !this.gatewayAddressValidation.valid)
        return
    }

    this.deviceMapping.static_ip = this.getStaticRadioButton().checked
    this.deviceMapping.ip = this.getIpAddress().value
    this.deviceMapping.subnet = this.getSubnetMask().value
    this.deviceMapping.gateway = this.getGatewayAddress().value
    this.dialogRef.close(this.deviceMapping);
  }

}
