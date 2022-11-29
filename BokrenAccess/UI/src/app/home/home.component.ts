import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  titles: string[] = [];


  constructor(private _router: Router,
              private _httpClient: HttpClient,
              private _route: ActivatedRoute) {
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

    this._route.paramMap.subscribe(res => {
      let url = 'http://localhost:5095/employees';
      let title = res.get('title');
      if (title)
        url += `?role=${title}`;
      this._httpClient.get(url, {
        headers: {
          'user-role': this._user.role
        }
      })
        .subscribe(res => {
          this.employees = res as any[];
        });

      this._httpClient.get('http://localhost:5095/titles', {
        headers: {
          'user-role': this._user.role
        }
      })
        .subscribe(res => {
          this.titles = res as any[];
        });
    });
  }

  adminPage() {
    this._router.navigate(['/admin']);
  }

  logout() {
    localStorage.removeItem('user');
    this._router.navigate(['/login']);
  }

  typeChanged($event: Event) {
    let value = ($event.target as HTMLSelectElement).value;
    if (value)
      this._router.navigate(['/home/' + value]);
    else
      this._router.navigate(['/home/']);
  }
}
