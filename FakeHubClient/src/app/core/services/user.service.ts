import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable, tap } from "rxjs";
import { Path } from "../constant/path.enum";
import { ServiceResponse } from "../model/service-response";
import { RegistrationRequestDto } from "../model/user";
import { LoginRequestDto, LoginResponseDto } from "../model/login";
import { jwtDecode } from "jwt-decode";
import { StorageService } from "./local-storage.service";
import { UserRole } from "../model/user-role";

@Injectable({
  providedIn: "root",
})
export class UserService {
  private http: HttpClient = inject(HttpClient);

  private storageService: StorageService = inject(StorageService);

  register(user: RegistrationRequestDto): Observable<any | null> {
    if(this.getRole() === UserRole.SUPERADMIN) return this.http.post<ServiceResponse>(Path.RegisterAdmin, user);
    return this.http.post<ServiceResponse>(Path.Register, user);
  }

  public login(user: LoginRequestDto): Observable<LoginResponseDto | null> {
    return this.http.post<LoginResponseDto>(Path.Login, user).pipe(
      tap((result: LoginResponseDto) => {
        this.extractToken(result.token);
      })
    );
  }

  public extractToken(token: string): void {
    this.storageService.setItem("token", token);
    try {
      const decodedToken: any = jwtDecode(token);
      this.storageService.setItem("role", decodedToken.role);
      this.storageService.setItem("name", decodedToken.name);
    } catch (error) {
      console.error("Failed to decode JWT:", error);
    }
  }

  public getToken(): string | null {
    return this.storageService.getItem("token");
  }

  public getRole(): string | null {
    return this.storageService.getItem("role");
  }

  public getUserName(): string | null {
    return this.storageService.getItem("name");
  }

  public isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  public logout(): void {
    this.storageService.removeItem("token");
    this.storageService.removeItem("role");
  }
  
}