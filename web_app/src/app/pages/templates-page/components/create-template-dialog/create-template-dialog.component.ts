/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
import { Component, OnInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TemplatesService } from 'src/app/services/templates.service';

@Component({
  selector: 'app-create-template-dialog',
  templateUrl: './create-template-dialog.component.html',
  styleUrls: ['./create-template-dialog.component.css']
})
export class CreateTemplateDialogComponent implements OnInit {

  constructor(public dialogRef: MatDialogRef<CreateTemplateDialogComponent>,
              private templatesService : TemplatesService,
              private notification: MatSnackBar) { }

  nameFC = new FormControl('', [Validators.required])
  modelFC = new FormControl('', [Validators.required])
  applicationFC = new FormControl('', [Validators.required])
  configScriptFC = new FormControl('', [Validators.required])
  descriptionFC = new FormControl('', [])

  ngOnInit(): void {
  }

  public onSaveClicked()
  {
    let name = this.nameFC.value!
    let model = this.modelFC.value!
    let application = this.applicationFC.value!
    let configScript = this.configScriptFC.value!.split("\n")
    let description = this.descriptionFC.value!
    
    this.templatesService.addRadarTemplate(name, model, application, description, configScript).subscribe({
      next : (response) => this.dialogRef.close(true),
      error : (err) => this.showNotification("Error: add template failed.")
    })
    
  }

  private showNotification(message : string)
  {
    this.notification.open(message, "Close", { duration : 4000 })
  }

  public onCancelClicked()
  {
    this.dialogRef.close(false);
  }

}
