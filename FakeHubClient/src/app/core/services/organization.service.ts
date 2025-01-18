import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";
import {
  addMemberToOrganizationPath,
  deactivateOrganization,
  deleteOrganizationMember,
  deleteTeamFromOrganization,
  Path,
} from "../constant/path";
import { ServiceResponse } from "../model/service-response";
import { Organization } from "../model/organization";
import { AddMembersRequest } from "../model/add-member-to-organization-request";
import { UserProfileResponseDto } from "../model/user";
import { IdNamePair } from "../model/id-name-pair";

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

  public addMember(
    organizationName: string,
    data: AddMembersRequest
  ): Observable<UserProfileResponseDto[] | null> {
    const addMemberUrl = addMemberToOrganizationPath(organizationName);
    return this.http.post<UserProfileResponseDto[]>(addMemberUrl, data);
  }

  public deleteMember(
    organizationName: string,
    username: string
  ): Observable<UserProfileResponseDto | null> {
    return this.http.delete<UserProfileResponseDto>(
      deleteOrganizationMember(organizationName, username)
    );
  }

  public deactivateOrganization(organizationName: string): Observable<boolean> {
    const deactivateOrganizationApi = deactivateOrganization(organizationName);
    return this.http.delete<boolean>(deactivateOrganizationApi);
  }

  public deleteTeam(
    organizationName: string,
    teamName: string
  ): Observable<boolean> {
    const deleteTeamFromOrganizationApi = deleteTeamFromOrganization(
      organizationName,
      teamName
    );
    return this.http.delete<boolean>(deleteTeamFromOrganizationApi);
  }

  public filterMembers(
    organizationName: string,
    query: string
  ): Observable<UserProfileResponseDto[]> {
    return this.http.get<UserProfileResponseDto[]>(
      `${Path.Organization}${organizationName}/users`,
      {
        params: { query },
      }
    );
  }

  public getByUserIdName(): Observable<IdNamePair[]> {
    return this.http.get<IdNamePair[]>(Path.OrganizationByUserIdName);
  }
}
