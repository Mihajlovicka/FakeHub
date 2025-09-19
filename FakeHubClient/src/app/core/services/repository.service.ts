import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";
import { Repository, EditRepositoryDto } from "../model/repository";
import { Path, getRepository } from "../constant/path";

@Injectable({
  providedIn: "root",
})
export class RepositoryService {
  private http: HttpClient = inject(HttpClient);

  private _searchQuerySubject = new BehaviorSubject<string>("");
  public searchQuery$ = this._searchQuerySubject.asObservable();

  public updateQuery(query: string): void {
    this._searchQuerySubject.next(query);
  }

  public save(data: Repository): Observable<any | null> {
    return this.http.post<Repository>(Path.Repositories, data);
  }

  public getRepositories(query: string = ''): Observable<Repository[]> {
    return this.http.get<Repository[]>(`${Path.Repositories}/search`, {
      params: { query }
    });
  }

  public getAllVisibleRepositoriesForUser(username: string): Observable<Repository[]> {
    return this.http.get<Repository[]>(`${Path.Repositories}/all/${username}`);
  }

  public getAllRepositoriesForOrganization(orgName: string): Observable<Repository[]> {
    return this.http.get<Repository[]>(`${Path.Repositories}/organization/${orgName}`);
  }

  public getRepository(id: number): Observable<Repository> {
    return this.http.get<Repository>(getRepository(id));
  }

  public delete(id: number): Observable<void> {
    return this.http.delete<void>(`${Path.Repositories}/${id}`);
  }

  public canEditRepository(id: number): Observable<boolean> {
    return this.http.get<boolean>(`${Path.Repositories}/canEdit/${id}`);
  }

  public editRepository(data: EditRepositoryDto): Observable<void> {
    return this.http.put<void>(`${Path.Repositories}`, data);
  }

  public getAllPublicRepositories(query: string = ''): Observable<any> {
    return this.http.get<any>(`${Path.Repositories}/public-repositories`, {
      params: { query }
    });
}
}