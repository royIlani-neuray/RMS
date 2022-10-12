import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HomepageComponent } from './pages/homepage/homepage.component';
import { ErrorPageNotFoundComponent } from './pages/error-page-not-found/error-page-not-found.component';



@NgModule({
  declarations: [
    HomepageComponent,
    ErrorPageNotFoundComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    HomepageComponent,
    ErrorPageNotFoundComponent
  ]
})
export class HomepageModule { }
