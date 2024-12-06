import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Path } from "../constant/path.enum";
import { ServiceResponse } from "../model/service-response";
import { Organization } from "../model/organization";

@Injectable({
  providedIn: "root",
})
export class OrganizationService {
  private http: HttpClient = inject(HttpClient);

  public addOrganization(user: Organization): Observable<any | null> {
    return this.http.post<ServiceResponse>(Path.Organization, user);
  }

  public getOrganization(name: string): Observable<Organization> {
    return this.http.get<Organization>(`${Path.Organization}/${name}`);
  }

  public editOrganization(organization: Organization): Observable<any | null> {
    return this.http.put<ServiceResponse>(
      `${Path.Organization}/${organization.name}`,
      organization
    );
  }

  public getByUser(): Observable<Organization[]> {
    return this.http.get<Organization[]>(Path.OrganizationByUser);
  }
}
