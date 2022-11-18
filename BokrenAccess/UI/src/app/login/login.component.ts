import {Component, OnInit} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Router} from "@angular/router";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  constructor(private _httpClient: HttpClient,
              private _router:Router) {
  }

  private _username: string = '';
  private _password: string = '';
  invalidLogin: boolean = false;

  ngOnInit(): void {
  }


  login(event: Event) {
    event.stopPropagation();
    event.preventDefault();

    this._httpClient.post('http://localhost:5095/login', {
      username: this._username,
      password: this._password
    }).subscribe({
      next: res => {
        this.invalidLogin = false;
        localStorage.setItem("user",JSON.stringify(res));
        this._router.navigate(['/home']);
      },
      error: (err: Error) => {
        this.invalidLogin = true;
      }
    });
  }

  updateUsername(element: Event) {
    this._username = (element.target as HTMLInputElement).value;
  }

  updatePassword(element: Event) {
    this._password = (element.target as HTMLInputElement).value;
  }
}
