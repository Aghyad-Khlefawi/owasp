import {Component, OnInit} from '@angular/core';
import {Router} from "@angular/router";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit {


  constructor(private _router: Router,
              private _httpClient: HttpClient) {
  }

  employees: any[] = [];
  private _user: any = null;
  isAdminUser: boolean = false;

  ngOnInit(): void {
    this._user = localStorage.getItem('user');
    if (!this._user) {
      this._router.navigate(['/login']);
      return;
    }
    this._user = JSON.parse(this._user);
    this.isAdminUser = this._user.role == 'admin';
    this._httpClient.get('http://localhost:5095/admin', {
      headers: {
        'user-role': this._user.role
      }
    })
      .subscribe(res => {
        this.employees = res as any[];
      });
  }

  homePage() {
    this._router.navigate(['/home']);
  }


  logout() {
    localStorage.removeItem('user');
    this._router.navigate(['/login']);
  }
}
