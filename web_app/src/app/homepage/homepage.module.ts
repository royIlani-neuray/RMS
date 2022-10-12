import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomepageComponent } from './pages/homepage/homepage.component';
import { ErrorPageNotFoundComponent } from './pages/error-page-not-found/error-page-not-found.component';
import { ErrorPageNoServiceComponent } from './pages/error-page-no-service/error-page-no-service.component';



@NgModule({
  declarations: [
    HomepageComponent,
    ErrorPageNotFoundComponent,
    ErrorPageNoServiceComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    HomepageComponent,
    ErrorPageNotFoundComponent,
    ErrorPageNoServiceComponent
  ]
})
export class HomepageModule { }
