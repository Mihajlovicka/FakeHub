import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { BehaviorSubject, Observable, tap } from "rxjs";
import { getProfilePath, Path } from "../constant/path";
import { ServiceResponse } from "../model/service-response";
import { RegistrationRequestDto, UserProfileResponseDto } from "../model/user";
import { LoginRequestDto, LoginResponseDto } from "../model/login";
import { jwtDecode } from "jwt-decode";
import { StorageService } from "./local-storage.service";
import { UserRole } from "../model/user-role";
import { ChangePasswordRequest } from "../model/change-password-request";
import { ChangeEmailRequest } from "../model/change-email-request";
import { ChangeUserBadgeRequest } from "../model/change-badge-to-user-request";

@Injectable({
  providedIn: "root",
})
export class UserService {
  private http: HttpClient = inject(HttpClient);
  private storageService: StorageService = inject(StorageService);

  private _isAuthSubject = new BehaviorSubject(this.isLoggedIn());
  public isAuth$: Observable<boolean> = this._isAuthSubject.asObservable();

  private _searchQuerySubject = new BehaviorSubject<string>("");
  public searchQuery$ = this._searchQuerySubject.asObservable();

  public getUsers(query: string, fetchAdmins: boolean): Observable<UserProfileResponseDto[]> {
    return this.http.get<UserProfileResponseDto[]>(fetchAdmins ? Path.AdminUsers : Path.Users, {
      params: { query },
    });
  }

  public updateQuery(data: string): void {
    this._searchQuerySubject.next(data);
  }

  public register(user: RegistrationRequestDto): Observable<any | null> {
    if(this.getRole() === UserRole.SUPERADMIN) return this.http.post<ServiceResponse>(Path.RegisterAdmin, user);
    return this.http.post<ServiceResponse>(Path.Register, user);
  }

  public login(user: LoginRequestDto): Observable<LoginResponseDto | null> {
    return this.http.post<LoginResponseDto>(Path.Login, user).pipe(
      tap((result: LoginResponseDto) => {
        this.extractToken(result.token);
        this._isAuthSubject.next(true);
      })
    );
  }

  public getUserProfileByUsername(usernameParam: string): Observable<UserProfileResponseDto | null> {
    const profileUrl = getProfilePath(usernameParam);
    return this.http.get<UserProfileResponseDto>(profileUrl).pipe(
      tap((user: UserProfileResponseDto) => {
        return user;
      })
    );
  }

  public extractToken(token: string): void {
    localStorage.setItem("token", token);
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

  public isEnabled(): boolean {
    const token = this.getToken();
    if(token){
      const decodedToken: any = jwtDecode(token);
      return decodedToken.enabled === true.toString();
    }
    return false;
  }

  public getUserNameFromToken(): string | null {
    const token = this.storageService.getItem("token") ?? "";
    const decodedToken: any = jwtDecode(token);

    return decodedToken?.name ?? "";
  }

  public isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  public logout(): void {
    this.storageService.removeItem("token");
    this.storageService.removeItem("role");
    this._isAuthSubject.next(false);
  }

  public changePassword(data: ChangePasswordRequest): Observable<LoginResponseDto | null> {
    return this.http.post<LoginResponseDto>(Path.ChangePassword, data).pipe(
        tap((result: LoginResponseDto) => {
          this.extractToken(result.token);
        })
    );
  }

  public changeEmail(data: ChangeEmailRequest): Observable<LoginResponseDto | null> {
    return this.http.post<LoginResponseDto>(Path.ChangeEmail, data).pipe(
      tap((result: LoginResponseDto) => {
        this.extractToken(result.token);
      })
    )
  }

  public changeUserBadge(data: ChangeUserBadgeRequest): Observable<UserProfileResponseDto | null> {
    return this.http.post<UserProfileResponseDto>(Path.ChangeUserBadge, data).pipe(
      tap((user: UserProfileResponseDto) => {
        return user;
      })
    )
  }

  public isSuperAdminLoggedIn(): boolean {
    return this.getRole() === UserRole.SUPERADMIN;
  }

  public isUserLoggedIn(): boolean {
    return this.getRole() === UserRole.USER;
  }

  public isAdminLoggedIn(): boolean {
    return this.getRole() === UserRole.ADMIN;
  }
}
