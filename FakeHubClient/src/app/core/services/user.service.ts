import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Path } from '../constant/path.enum';
import { ServiceResponse } from '../model/service-response';
import { RegistrationRequestDto } from '../model/user';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  http: HttpClient = inject(HttpClient);

  register(user: RegistrationRequestDto): Observable<any | null> {
    return this.http.post<ServiceResponse>(Path.Register, user);
  }
}
