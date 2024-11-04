import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable, tap } from "rxjs";
import { Path } from "../constant/path.enum";
import { ServiceResponse } from "../model/service-response";
import { RegistrationRequestDto } from "../model/user";
import { LoginRequestDto, LoginResponseDto } from "../model/login";
import { jwtDecode } from "jwt-decode";

@Injectable({
  providedIn: "root",
})
export class UserService {
  private http: HttpClient = inject(HttpClient);

  register(user: RegistrationRequestDto): Observable<any | null> {
    return this.http.post<ServiceResponse>(Path.Register, user);
  }

  login(user: LoginRequestDto): Observable<LoginResponseDto | null> {
    return this.http.post<LoginResponseDto>(Path.Login, user).pipe(
      tap((result: LoginResponseDto) => {
        this.extractToken(result.token);
      })
    );
  }

  extractToken(token: string): void {
    localStorage.setItem("token", token);
    try {
      const decodedToken: any = jwtDecode(token);
      localStorage.setItem("role", decodedToken.role);
    } catch (error) {
      console.error("Failed to decode JWT:", error);
    }
  }

  getToken(): string | null {
    return localStorage.getItem("token");
  }

  getRole(): string | null {
    return localStorage.getItem("role");
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  logout(): void {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
  }
}