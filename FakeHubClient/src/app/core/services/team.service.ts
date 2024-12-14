import { HttpClient } from "@angular/common/http";
import { inject, Injectable, signal, WritableSignal } from "@angular/core";
import { Observable } from "rxjs";
import { Path } from "../constant/path.enum";
import { ServiceResponse } from "../model/service-response";
import { Organization } from "../model/organization";
import { Team } from "../model/team";

@Injectable({
  providedIn: "root",
})
export class TeamService {
  private http: HttpClient = inject(HttpClient);

  public addTeam(team: Organization): Observable<any | null> {
    return this.http.post<ServiceResponse>(Path.Team, team);
  }

  public getTeam(organizationName: string, teamName: string): Observable<Team> {
    return this.http.get<Team>(`${Path.Team}/${organizationName}/${teamName}`);
  }
}
