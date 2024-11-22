import { HttpClient } from "@angular/common/http";
import { Inject, inject, Injectable, PLATFORM_ID } from "@angular/core";
import { Observable, tap } from "rxjs";
import { Path } from "../constant/path.enum";
import { ServiceResponse } from "../model/service-response";
import { RegistrationRequestDto } from "../model/user";
import { LoginRequestDto, LoginResponseDto } from "../model/login";
import { jwtDecode } from "jwt-decode";
import { isPlatformBrowser } from "@angular/common";

@Injectable({
  providedIn: "root",
})
export class UserService {
  private http: HttpClient = inject(HttpClient);
  private platformId: Object = inject(PLATFORM_ID);

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
      localStorage.setItem("name", decodedToken.name);
    } catch (error) {
      console.error("Failed to decode JWT:", error);
    }
  }

  getToken(): string | null {
    return this.getLocalStorageItem("token");
  }

  getRole(): string | null {
    return this.getLocalStorageItem("role");
  }

  getUserName(): string | null {
    return this.getLocalStorageItem('name');
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  logout(): void {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
  }
  
  private getLocalStorageItem(key: string): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(key);
    }
    return null; 
  }
}