import { HttpClient } from "@angular/common/http";
import { inject, Injectable, signal, WritableSignal } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";
import { Path } from "../constant/path";
import { ServiceResponse } from "../model/service-response";
import { Organization } from "../model/organization";

@Injectable({
  providedIn: "root",
})
export class OrganizationService {
  private http: HttpClient = inject(HttpClient);

  private _searchQuerySubject = new BehaviorSubject<string>("");
  public searchQuery$ = this._searchQuerySubject.asObservable();

  public updateQuery(data: string): void {
    this._searchQuerySubject.next(data);
  }

  public getOrganizations(query: string): Observable<Organization[]> {
    return this.http.get<Organization[]>(Path.Organization, {
      params: { query },
    });
  }

  public addOrganization(user: Organization): Observable<any | null> {
    return this.http.post<ServiceResponse>(Path.Organization, user);
  }

  public getOrganization(name: string): Observable<Organization> {
    return this.http.get<Organization>(`${Path.Organization}${name}`);
  }

  public editOrganization(organization: Organization): Observable<any | null> {
    return this.http.put<ServiceResponse>(
      `${Path.Organization}${organization.name}`,
      organization
    );
  }

  public getByUser(): Observable<Organization[]> {
    return this.http.get<Organization[]>(Path.OrganizationByUser);
  }
}
