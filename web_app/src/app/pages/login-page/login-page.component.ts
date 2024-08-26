import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UsersService } from 'src/app/services/users.service';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.css']
})
export class LoginPageComponent implements OnInit {

  loginForm: FormGroup;
  loginFailed: boolean = false; 

  constructor(private fb: FormBuilder, private usersService : UsersService, private router : Router) 
  {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]],
    });
  }

  ngOnInit(): void {
  }

  onSubmit() {
    if (this.loginForm.valid) {
      // Handle login logic here
      var email = this.loginForm.value.email
      var password = this.loginForm.value.password

      this.usersService.authenticateUser(email, password).subscribe({
        next : (response) => 
        {
          this.loginFailed = false
          var jwtToken = response.token
          localStorage.setItem('auth', JSON.stringify(jwtToken));
          this.router.navigate([''])
        },
        error : (err) => this.loginFailed = true
      })
    }
  }

}
